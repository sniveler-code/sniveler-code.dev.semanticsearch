using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Core.Utils
{
    /// <summary>Text helpers for cleaning asset names and building tokens.</summary>
    public static class NamingUtility
    {
        private static readonly Regex _prefixRegex = new("^([A-Z]{1,4}_)+", RegexOptions.Compiled);
        private static readonly Regex _camelCaseRegex = new("([a-z])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex _whitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        private static readonly Regex _suffixRegex = new(@"(_v\d+|_LOD\d+|_01|_02|(?<=\d)$)+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _technicalNoiseRegex =
            new Regex(@"\b(Groupium|Simplified|Static|Internal|Vertex)\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] _ignoreFolders =
        {
            "assets", "resources", "fbx", "models", "prefabs", "meshes",
            "textures", "lods", "materials", "v1", "final", "exports"
        };

        /// <summary>Normalises an asset name: prefixes, camelCase, suffixes and noise.</summary>
        public static string Clean(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string result = _prefixRegex.Replace(input, " ");
            result = _camelCaseRegex.Replace(result, "$1 $2");
            result = result.Replace('_', ' ').Replace('-', ' ');
            result = _technicalNoiseRegex.Replace(result, "");
            result = _suffixRegex.Replace(result, "");

            return _whitespaceRegex.Replace(result, " ").Trim().ToLower();
        }

        /// <summary>Returns the AssetDatabase filter for a storage type.</summary>
        public static string ToAssetFilter(this AssetStorageType value)
        {
            return value switch
            {
                AssetStorageType.Prefabs => "t:Prefab",
                _ => "t:None"
            };
        }

        /// <summary>Extracts up to count meaningful folders from an asset path.</summary>
        public static string[] CleanPath(this string value, int count = 2) => value.Split('/')
            .Reverse()
            .Skip(1)
            .SelectMany(x => x.Split(" "))
            .Select(Clean)
            .Where(x => !_ignoreFolders.Contains(x.ToLower()))
            .Where(x => x.Length > 2)
            .Distinct()
            .Take(count)
            .ToArray();

        /// <summary>Splits text into lowercase words longer than two characters.</summary>
        public static string[] SplitWords(this string value, string separator = " ") =>
            value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.ToLower().Trim())
                .Where(a => a.Length > 2)
                .ToArray();

        /// <summary>Joins words with the given separator.</summary>
        public static string JoinWords(this string[] value, string separator = " ") =>
            string.Join(separator, value);

        /// <summary>Joins words with the given separator.</summary>
        public static string JoinWords(this List<string> value, string separator = " ") =>
            string.Join(separator, value);

        /// <summary>Joins words with the given separator.</summary>
        public static string JoinWords(this IEnumerable<string> value, string separator = " ") =>
            string.Join(separator, value);

        /// <summary>Normalises text for embedding: punctuation, underscores and hyphens.</summary>
        public static string CleanForAI(this string value) =>
            value.Replace("&", "")
                .Replace(",", "")
                .Replace("_", " ")
                .Replace("-", " ")
                .Replace("  ", " ")
                .ToLower()
                .Trim();

        /// <summary>
        /// FNV-1a 64-bit digest of a string, used to detect content changes of
        /// baked category databases (stable, dependency-free, collision-safe
        /// enough for staleness detection).
        /// </summary>
        public static string HashFnv1a(string value)
        {
            long hash = -3750763034362895579;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 1099511628211;
            }

            return $"{hash:X}";
        }
    }
}
