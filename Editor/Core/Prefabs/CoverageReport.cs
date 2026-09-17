using System.Collections.Generic;
using System.IO;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Coverage of one baked category against the indexed corpus.</summary>
    public sealed class CategoryCoverageEntry
    {
        public string Id;
        public DatabaseType Database;
        public int Hits;
        public readonly List<string> SampleAssets = new();
    }

    /// <summary>
    /// Authoring aid: compares baked category keywords against the indexed asset texts
    /// to find dead categories (no asset matches) and uncovered assets (no category
    /// matches). Pure data + logic — no Unity or database dependencies — so it can be
    /// unit-tested deterministically.
    /// </summary>
    public sealed class CoverageReport
    {
        public int CategoryCount;
        public int MatchedCategoryCount;
        public int AssetCount;
        public int UncoveredAssetCount;

        public readonly List<CategoryCoverageEntry> Entries = new();
        public readonly List<string> UncoveredAssets = new();

        public int UnmatchedCategoryCount => CategoryCount - MatchedCategoryCount;

        /// <summary>Builds the report from baked categories and indexed assets.</summary>
        public static CoverageReport Build(MetadataTable[] categories, AssetsTable[] assets,
            int sampleLimit = 3, int uncoveredLimit = 50)
        {
            var report = new CoverageReport
            {
                CategoryCount = categories?.Length ?? 0,
                AssetCount = assets?.Length ?? 0
            };

            var assetTokens = new HashSet<string>[report.AssetCount];
            for (int i = 0; i < report.AssetCount; i++)
            {
                assetTokens[i] = Tokens(AssetText(assets[i]));
            }

            var matchedAssets = new bool[report.AssetCount];

            if (categories != null)
            {
                foreach (MetadataTable category in categories)
                {
                    var entry = new CategoryCoverageEntry
                    {
                        Id = category.Id,
                        Database = category.Category
                    };

                    HashSet<string> categoryTokens = Tokens(category.Keys);
                    for (int i = 0; i < report.AssetCount && categoryTokens.Count > 0; i++)
                    {
                        if (!Overlaps(categoryTokens, assetTokens[i])) continue;

                        entry.Hits++;
                        matchedAssets[i] = true;
                        if (entry.SampleAssets.Count < sampleLimit)
                        {
                            entry.SampleAssets.Add(Path.GetFileName(assets[i].Path));
                        }
                    }

                    if (entry.Hits > 0) report.MatchedCategoryCount++;
                    report.Entries.Add(entry);
                }
            }

            report.Entries.Sort((a, b) =>
            {
                int byHits = a.Hits.CompareTo(b.Hits);
                return byHits != 0 ? byHits : string.CompareOrdinal(a.Id, b.Id);
            });

            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (matchedAssets[i]) continue;

                    report.UncoveredAssetCount++;
                    if (report.UncoveredAssets.Count < uncoveredLimit)
                    {
                        report.UncoveredAssets.Add(assets[i].Path);
                    }
                }
            }

            return report;
        }

        /// <summary>Text searched for a category match.</summary>
        private static string AssetText(AssetsTable asset) =>
            $"{Path.GetFileNameWithoutExtension(asset.Path)} {asset.Metadata}";

        /// <summary>Normalised token set of a text.</summary>
        private static HashSet<string> Tokens(string text)
        {
            var set = new HashSet<string>();
            foreach (string word in (text ?? "").CleanForAI().SplitWords())
            {
                set.Add(word);
            }

            return set;
        }

        /// <summary>True when the two token sets intersect.</summary>
        private static bool Overlaps(HashSet<string> left, HashSet<string> right)
        {
            foreach (string word in left)
            {
                if (right.Contains(word)) return true;
            }

            return false;
        }
    }
}