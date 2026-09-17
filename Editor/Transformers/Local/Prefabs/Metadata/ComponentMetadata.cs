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

        /// <summary>Returns behavioural keywords for a prefab.</summary>
        public override async Task<IMetadataResult> ProcessAsync(GameObject go)
        {
            HashSet<string> tags = new HashSet<string>();
            var components = go.GetComponentsInChildren<Component>();
            var database = _metadata.MetadataCategory(DatabaseType.Components);
            foreach (var comp in components)
            {
                if (comp == null) continue;

                var componentType = comp.GetType();
                if (_standardComponent.TryGetValue(componentType.Name, out string value))
                {
                    tags.Add(value);
                    continue;
                }

                if (componentType.Namespace != null && componentType.Namespace.StartsWith("UnityEngine"))
                {
                    continue;
                }

                var words = new MetadataWord[]
                {
                    new(CleanNameSpecific(componentType.Name))
                };

                await _metadata.GetVectorsAsync(words);
                var results = database.Similarity(words);

                var topTags = results
                    .OrderByDescending(a => a.Value[0])
                    .Take(2)
                    .ToArray();

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
