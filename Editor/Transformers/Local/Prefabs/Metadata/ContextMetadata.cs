using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEngine;
using UnityEditor;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata
{
    /// <summary>Derives category context from the prefab name and folders.</summary>
    public sealed class ContextMetadata : Metadata<GameObject>
    {
        public override string Name => "Hierarchy Context";
        public override string Info => "Extracts semantic meaning from folder structure.";

        private readonly ContextMatchConfig _config;

        /// <summary>Stores the metadata facade and thresholds.</summary>
        public ContextMetadata(IMetadataFacade facade, ContextMatchConfig config) : base(facade)
        {
            _config = config;
        }

        /// <summary>Scores categories and builds the context result.</summary>
        public override async Task<IMetadataResult> ProcessAsync(GameObject asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string[] clearedNames = GetClearName(asset.name);
            string[] cleanPath = path.CleanPath();

            if (cleanPath.Length == 0)
            {
                cleanPath = cleanPath
                    .Append(clearedNames[^1])
                    .ToArray();
            }

            var detailed = _metadata.MetadataCategory(DatabaseType.Detailed);
            var general = _metadata.MetadataCategory(DatabaseType.General);

            var result = new Result
            {
                Detailed = new Dictionary<string, string[]>(),
                General = new List<string>(),
                CleanPath = cleanPath.JoinWords()
            };

            MetadataWord[] words =
            {
                new(cleanPath.JoinWords(), 0.3f),
                new(clearedNames.JoinWords(), 0.7f),
            };

            await _metadata.GetVectorsAsync(words);

            var similarity = detailed.Similarity(words);

            var ranked = similarity
                .OrderByDescending(a => a.Value.Sum())
                .ToArray();

            if (ranked.Length > 0)
            {
                var first = ranked[0];
                float firstValue = first.Value.Sum();
                if (firstValue > _config.DetailedThreshold.Value)
                {
                    CombineResult(first.Key);
                    firstValue -= firstValue * _config.DetailedDecay.Value;
                    if (ranked.Length > 1 && ranked[1].Value.Sum() > firstValue)
                    {
                        CombineResult(ranked[1].Key);
                    }
                }
                else
                {
                    var generalRanked = general.Similarity(words)
                        .OrderByDescending(a => a.Value.Sum())
                        .ToArray();

                    if (generalRanked.Length > 0 && generalRanked[0].Value.Max() >= _config.GeneralThreshold.Value)
                    {
                        var generalItem = general[generalRanked[0].Key];
                        result.General.Add(generalItem.Values);
                    }
                }
            }

            return result;

            /// <summary>Adds a category's values to the result.</summary>
            void CombineResult(string key)
            {
                var detailedItem = detailed[key];
                result.Detailed.Add(detailedItem.Id, detailedItem.Values.SplitWords(","));
                if (general.TryGetValue(detailedItem.Parent, out MetadataTable generalItem))
                {
                    result.General.Add(generalItem.Values);
                }
            }
        }

        /// <summary>Removes material words from the prefab name.</summary>
        private string[] GetClearName(string prefabName)
        {
            string[] cleanNames = NamingUtility.Clean(prefabName).SplitWords();
            var finishedName = new List<string>();
            var materials = _metadata.MetadataCategory(DatabaseType.Materials);
            foreach (string cleanName in cleanNames)
            {
                bool nameIsMaterial = false;
                foreach (var material in materials.Values)
                {
                    string[] materialKeys = material.Keys.SplitWords(",");
                    if (materialKeys.Any(a => a.SimilarFor(cleanName)))
                    {
                        nameIsMaterial = true;
                        break;
                    }
                }

                if (!nameIsMaterial) finishedName.Add(cleanName);
            }

            return finishedName.ToArray();
        }

        /// <summary>Context result: detailed, general and clean path.</summary>
        public sealed class Result : IMetadataResult
        {
            public Dictionary<string, string[]> Detailed;
            public List<string> General;
            public string CleanPath;
        }
    }
}
