using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata
{
    /// <summary>Infers behaviour from attached components and scripts.</summary>
    public sealed class ComponentMetadata : Metadata<GameObject>
    {
        public override string Name => "Functional Roles";
        public override string Info => "Infers behavior and gameplay roles from attached scripts and components.";

        private readonly string[] _noisyWords =
        {
            "manager", "controller", "system", "handler", "logic", "processor", "engine",
            "component", "script", "behaviour", "behavior", "object", "entity", "mono", "instance",
            "effect", "trigger", "event", "listener", "provider", "emitter",
            "helper", "utility", "utils", "extension", "base", "core", "wrapper", "proxy",
            "data", "info", "settings", "config", "profile"
        };

        private readonly Dictionary<string, string> _standardComponent = new()
        {
            {"BoxCollider", "physical, solid, collision, obstacle"},
            {"MeshCollider", "physical, solid, collision, geometry"},
            {"CapsuleCollider", "physical, solid, collision, boundary"},
            {"SphereCollider", "physical, solid, collision, volume"},
            {"Rigidbody", "motion, mass, gravity, force, dynamic"},
            {"Light", "illumination, radiance, light_source, ambiance"},
            {"ParticleSystem", "visuals, emission, spectacle, atmosphere, vfx"},
            {"TrailRenderer", "motion, visual, path, emission"},
            {"AudioSource", "auditory, signal, volume, ambiance, sound"},
            {"AudioListener", "auditory, hearing, perspective, reception"},
            {"NavMeshAgent", "autonomy, navigation, logic, pathfinding, intelligence"},
            {"NavMeshObstacle", "obstacle, logic, navigation, boundary"},
            {"Camera", "perspective, vision, observation, viewport"},
            {"Animator", "performance, action, movement, fluidity, pose"},
            {"Animation", "performance, action, movement, sequence"},
            {"Canvas", "information, display, interface, overlay, gui"},
            {"RectTransform", "interface, display, layout, positioning"},
            {"LODGroup", "performance, efficiency, multiscale, optimization"},
            {"Terrain", "landscape, environment, natural, ground, massive"}
        };

        /// <summary>Stores the metadata facade.</summary>
        public ComponentMetadata(IMetadataFacade facade) : base(facade)
        {
        }

        /// <summary>
        /// Session cache: cleaned component name → embedding vector (REVIEW M10 — a script
        /// type is embedded once per session, not once per prefab).
        /// </summary>
        private readonly Dictionary<string, float[]> _nameVectorCache = new();

        /// <summary>Returns behavioural keywords for a prefab.</summary>
        public override async Task<IMetadataResult> ProcessAsync(GameObject go)
        {
            HashSet<string> tags = new HashSet<string>();
            var components = go.GetComponentsInChildren<Component>();
            var database = _metadata.MetadataCategory(DatabaseType.Components);

            // Pass 1 (REVIEW M10): standard tags are collected right away, custom component
            // names are collected so they can all be embedded in one batch call (previously:
            // one inference per component, per prefab, no caching).
            var customNames = new List<string>();
            foreach (var comp in components)
            {
                if (comp == null) continue;

                var componentType = comp.GetType();
                var componentTypeName = componentType.Name;
                if (_standardComponent.TryGetValue(componentTypeName, out string value))
                {
                    tags.Add(value);
                    continue;
                }

                if (componentType.Namespace != null && componentType.Namespace.StartsWith("UnityEngine"))
                {
                    continue;
                }

                customNames.Add(CleanNameSpecific(componentTypeName));
            }

            // Embed only the unique names not in the cache yet — in a single inference.
            var missing = customNames
                .Distinct()
                .Where(name => !_nameVectorCache.ContainsKey(name))
                .ToArray();
            if (missing.Length > 0)
            {
                var words = new MetadataWord[missing.Length];
                for (int i = 0; i < missing.Length; i++)
                    words[i] = new(missing[i]);

                await _metadata.GetVectorsAsync(words);
                for (int i = 0; i < words.Length; i++)
                    _nameVectorCache[missing[i]] = words[i].Vector;
            }

            // Pass 2: similarity of every custom component against the Components database.
            foreach (string name in customNames)
            {
                var results = database.Similarity(new[]
                {
                    new MetadataWord(name) {Vector = _nameVectorCache[name]}
                });

                var topTags = results
                    .OrderByDescending(a => a.Value[0])
                    .Take(2)
                    .ToArray();

                // REVIEW M9: an empty Components database yields no similarity rows — skip
                // this component instead of indexing into an empty array.
                if (topTags.Length == 0) continue;

                var first = topTags[0];
                if (first.Value[0] > 0.4f)
                {
                    tags.Add(database[first.Key].Values);
                }
            }

            return IMetadataResult.FromArray(tags.ToArray());
        }

        /// <summary>Strips generic component words from a name.</summary>
        private string CleanNameSpecific(string name)
        {
            string[] cleanWords = NamingUtility.Clean(name).SplitWords();
            return cleanWords.Where(a => !_noisyWords.Any(n => n.SimilarFor(a))).JoinWords();
        }
    }
}
