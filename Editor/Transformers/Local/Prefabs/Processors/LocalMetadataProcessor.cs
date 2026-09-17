using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Processors
{
    /// <summary>Runs the metadata extractors and composes the description.</summary>
    public sealed class LocalMetadataProcessor : ILocalProcessor
    {
        private readonly Metadata<GameObject>[] _extractors;

        /// <summary>Creates the extractors through the container.</summary>
        public LocalMetadataProcessor(IMiniContainer container)
        {
            _extractors = new Metadata<GameObject>[]
            {
                container.Create<IdentityMetadata>(),
                container.Create<MaterialsMetadata>(),
                container.Create<ContextMetadata>(),
                container.Create<ComponentMetadata>(),
            };
        }

        public IMetadata[] Extractors =>
            _extractors.Select(a => (IMetadata) a)
                .ToArray();

        /// <summary>Composes the final description text.</summary>
        public async Task<string> Process(GameObject asset)
        {
            var tasks = _extractors
                .Select(a => a.ProcessAsync(asset));

            var tokens = new HashSet<string>();
            IMetadataResult[] results = await Task.WhenAll(tasks);

            string identity = Array(results[0]).JoinWords();
            string[] materials = Array(results[1]);
            var detailed = (ContextMetadata.Result) results[2];
            string[] components = Array(results[3]);

            StringBuilder finalDescription = new StringBuilder();
            string matPrefix = materials.Length > 0 ? FormatList(materials.Take(3).ToArray()) : "";
            string leadWord = !string.IsNullOrEmpty(matPrefix) ? matPrefix : identity;
            string article = GetArticle(leadWord);

            finalDescription.Append($"This asset is {article} {matPrefix} {identity}. ".Replace("  ", " "));

            if (detailed.Detailed.Count > 0)
            {
                finalDescription.Append("Specifically, it is ");

                List<string> specificClauses = new List<string>();
                foreach ((string rawId, string[] value) in detailed.Detailed)
                {
                    string categoryName = Distinct(rawId.Replace("_", " ").SplitWords()).JoinWords();
                    string catArticle = GetArticle(rawId);
                    string catArticlePart = string.IsNullOrEmpty(catArticle) ? "" : catArticle + " ";

                    string features = FormatList(Distinct(value).Take(4).ToArray());
                    specificClauses.Add($"{catArticlePart}{categoryName} for {features}");
                }

                finalDescription.Append(string.Join(" and ", specificClauses) + ". ");
            }

            if (detailed.General.Count > 0)
            {
                var totalGeneral = new List<string>();
                foreach (string general in detailed.General)
                {
                    var values = Distinct(general.SplitWords(",")).Take(3);
                    totalGeneral.AddRange(values);
                }

                string generalString = FormatList(totalGeneral.ToArray());
                finalDescription.Append($"Broadly, it relates to {generalString}. ");
            }

            if (components.Length > 0)
            {
                var total = new List<string>();
                foreach (string component in components)
                {
                    total.AddRange(Distinct(component.SplitWords(",")).Take(3));
                }

                string totalString = FormatList(total.ToArray());
                finalDescription.Append($"Functionally, it involves {totalString}. ");
            }

            if (!string.IsNullOrEmpty(detailed.CleanPath))
            {
                finalDescription.Append($"It is contextually associated with {detailed.CleanPath}.");
            }

            return DeduplicateByStem(finalDescription.ToString());

            /// <summary>Adds unseen word stems to the token set.</summary>
            IEnumerable<string> Distinct(IEnumerable<string> result) =>
                result.Where(a => tokens.Add(GetSimpleStem(a)));

            string[] Array(IMetadataResult result) => ((IMetadataResult.Array) result).Value
                .Where(a => tokens.Add(GetSimpleStem(a)))
                .ToArray();
        }

        /// <summary>Removes repeated word stems from the text.</summary>
        private static string DeduplicateByStem(string text)
        {
            string[] words = text.Split(" ");
            var seenStems = new HashSet<string>();
            var result = new List<string>();

            foreach (string word in words)
            {
                string clean = Regex.Replace(word.ToLower(), @"[^a-z]", "");
                string stem = GetSimpleStem(clean);
                if (clean.Length > 3 && seenStems.Contains(stem)) continue;
                if (clean.Length > 3) seenStems.Add(stem);
                result.Add(word);
            }

            return string.Join(" ", result).Replace("_", " ");
        }

        /// <summary>Reduces a word to a simple stem.</summary>
        private static string GetSimpleStem(string word)
        {
            if (word.Length <= 3) return word;
            if (word.EndsWith("ing")) return word[..^3];
            if (word.EndsWith("ed")) return word[..^2];
            if (word.EndsWith("s") && !word.EndsWith("ss")) return word[..^1];
            return word;
        }

        /// <summary>Returns a or an for the given word.</summary>
        private static string GetArticle(string word)
        {
            if (string.IsNullOrEmpty(word)) return "a";
            string trimmed = word.ToLower().Trim();
            if (trimmed.EndsWith("s")) return "";
            char first = trimmed[0];
            return "aeiou".Contains(first) ? "an" : "a";
        }

        /// <summary>Joins items as a readable list.</summary>
        private static string FormatList(string[] items)
        {
            if (items == null || items.Length == 0) return "";
            if (items.Length == 1) return items[0].Trim();

            return items.Take(items.Length - 1)
                .Select(i => i.Trim())
                .JoinWords(", ") + " and " + items.Last().Trim();
        }
    }
}
