using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Scoring helpers for category matching.</summary>
    public static class MetadataExtensions
    {
        /// <summary>Scores a category table against metadata words.</summary>
        public static ConcurrentDictionary<string, float[]> Similarity(this Dictionary<string, MetadataTable> table,
            MetadataWord[] words)
        {
            string[] ids = table.Keys.ToArray();

            var results = new ConcurrentDictionary<string, float[]>();
            Parallel.For(0, ids.Length, i =>
            {
                var item = table[ids[i]];

                float[] scoreResult = new float[words.Length];
                ReadOnlySpan<float> itemSpan = MemoryMarshal.Cast<byte, float>(item.Vector);
                for (int j = 0; j < words.Length; j++)
                {
                    var metaWord = words[j];
                    float score = metaWord.Vector.CosineSimilaritySimd(itemSpan);
                    scoreResult[j] = (score + item.BonusFrom(metaWord.Word)) * metaWord.Ratio;
                }

                results.TryAdd(ids[i], scoreResult);
            });

            return results;
        }

        /// <summary>Keyword bonus for a name, using word-boundary matching.</summary>
        private static float BonusFrom(this MetadataTable value, string name, in float bonus = 0.15f)
        {
            float result = 0f;
            string[] keysWords = value.Keys.SplitWords(",");
            string[] cleanNames = name.SplitWords();

            float deltaBonus = 0.5f / cleanNames.Length;
            float bonusFactor = 0.5f;

            foreach (string cleanName in cleanNames)
            {
                if (string.IsNullOrEmpty(cleanName)) continue;

                bonusFactor += deltaBonus;
                foreach (string rawKey in keysWords)
                {
                    string keyWord = rawKey.Trim();
                    if (string.IsNullOrEmpty(keyWord)) continue;

                    if (!keyWord.Contains(cleanName) && !cleanName.Contains(keyWord))
                    {
                        continue;
                    }

                    float lengthRatio =
                        (float) Math.Min(cleanName.Length, keyWord.Length) /
                        Math.Max(cleanName.Length, keyWord.Length);

                    if (HasWordBoundary(cleanName, keyWord) || HasWordBoundary(keyWord, cleanName))
                    {
                        result += bonus * lengthRatio * bonusFactor;
                    }
                    else if (lengthRatio >= 0.75f)
                    {
                        result += bonus * 0.5f * lengthRatio * bonusFactor;
                    }
                }
            }

            return result;
        }

        /// <summary>True when the substring sits on a word boundary.</summary>
        private static bool HasWordBoundary(string text, string substring)
        {
            int index = text.IndexOf(substring);
            while (index >= 0)
            {
                bool leftOk = index == 0 || !IsWordChar(text[index - 1]);
                int end = index + substring.Length;
                bool rightOk = end == text.Length || !IsWordChar(text[end]);
                if (leftOk && rightOk) return true;
                index = text.IndexOf(substring, index + 1);
            }

            return false;
        }

        /// <summary>True for letters, digits and underscores.</summary>
        private static bool IsWordChar(char value) =>
            (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z') ||
            (value >= '0' && value <= '9') || value == '_';

        /// <summary>Loose equality used when filtering name tokens.</summary>
        public static bool SimilarFor(this string value, string name, in float total = 0.8f)
        {
            if (value.Contains(name))
            {
                return name.Length * 1f / value.Length > total;
            }

            return false;
        }
    }
}
