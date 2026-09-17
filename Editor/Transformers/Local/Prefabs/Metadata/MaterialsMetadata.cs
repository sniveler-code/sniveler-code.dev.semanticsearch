using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEditor;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata
{
    /// <summary>Detects materials used by a prefab.</summary>
    public sealed class MaterialsMetadata : Metadata<GameObject>
    {
        public override string Name => "Surface & Visual Detail";
        public override string Info => "Clean material keyword extraction.";

        /// <summary>Stores the metadata facade.</summary>
        public MaterialsMetadata(IMetadataFacade facade) : base(facade)
        {
        }

        /// <summary>Returns material keywords.</summary>
        public override Task<IMetadataResult> ProcessAsync(GameObject asset)
        {
            List<string> rendererKeywords = GetRendererKeywords(asset);
            string[] cleanNames = NamingUtility.Clean(asset.name).SplitWords();
            string[] cleanPath = AssetDatabase.GetAssetPath(asset).CleanPath();

            string[] allKeywords = rendererKeywords
                .Concat(cleanNames)
                .Concat(cleanPath)
                .ToArray();

            var materials = _metadata.MetadataCategory(DatabaseType.Materials);
            var tokens = new List<string>();
            foreach (var material in materials)
            {
                string[] keys = material.Value.Keys.SplitWords(",");
                if (allKeywords.Any(k => keys.Contains(k)))
                {
                    var values = material.Value.Values
                        .SplitWords(",")
                        .Take(3);

                    tokens.AddRange(values);
                }
            }

            var result = IMetadataResult.FromArray(tokens.Distinct().ToArray());
            return Task.FromResult(result);
        }

        /// <summary>Collects material names from the prefab renderers.</summary>
        private static List<string> GetRendererKeywords(GameObject prefab)
        {
            var keywords = new List<string>();
            foreach (var r in prefab.GetComponentsInChildren<Renderer>())
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    keywords.Add(m.name.ToLower());
                    keywords.Add(m.shader.name.Split('/').Last().ToLower());
                }
            }

            return keywords
                .Select(NamingUtility.Clean)
                .Where(a => a.Length > 2)
                .Distinct()
                .ToList();
        }
    }
}
