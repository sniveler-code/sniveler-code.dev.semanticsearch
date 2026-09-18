using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Templates
{
    /// <summary>
    /// Deterministic loader for the package UI assets (UXML templates and the window style sheet).
    /// Assets are resolved from inside the package (Packages/sniveler-code.dev.semanticsearch/...),
    /// so a user project cannot hijack templates by shadowing asset names (the old
    /// AssetDatabase.FindAssets-by-name behaviour). Results are cached per asset name.
    /// </summary>
    public static class TemplateProvider
    {
        internal const string PackageName = "sniveler-code.dev.semanticsearch";
        private const string UxmlDirectory = "Editor/Templates/Uxml";

        private static readonly Dictionary<string, VisualTreeAsset> TemplateCache =
            new();
        private static readonly Dictionary<string, StyleSheet> StyleSheetCache =
            new();

        /// <summary>
        /// Loads a UXML template shipped inside the package, e.g. "SemanticSearch_SearchModule".
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the template does not exist in the package. Callers should surface
        /// this as a status/error instead of letting a NullReferenceException propagate.
        /// </exception>
        public static VisualTreeAsset LoadByName(string uxmlName)
        {
            if (string.IsNullOrWhiteSpace(uxmlName))
                throw new ArgumentException("UXML name is empty.", nameof(uxmlName));

            if (TemplateCache.TryGetValue(uxmlName, out VisualTreeAsset cached))
                return cached;

            VisualTreeAsset asset = LoadFirstFromPackage($"{uxmlName}.uxml",
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>);

            if (asset == null)
                throw new InvalidOperationException(
                    $"[SemanticSearch] UXML template '{uxmlName}' not found inside the package " +
                    $"'{PackageName}' (expected at {PackageUxmlPath(uxmlName)}). " +
                    "Make sure the package folder is intact and has not been renamed.");

            TemplateCache[uxmlName] = asset;
            return asset;
        }

        /// <summary>
        /// Loads a USS style sheet shipped inside the package, e.g. "SemanticSearchEditor".
        /// Returns null instead of throwing, so styles are an optional decoration.
        /// </summary>
        public static StyleSheet LoadStyleSheetByName(string ussName)
        {
            if (string.IsNullOrWhiteSpace(ussName)) return null;

            if (StyleSheetCache.TryGetValue(ussName, out StyleSheet cached))
                return cached;

            StyleSheet sheet = LoadFirstFromPackage($"{ussName}.uss",
                AssetDatabase.LoadAssetAtPath<StyleSheet>);

            if (sheet != null)
                StyleSheetCache[ussName] = sheet;

            return sheet;
        }

        /// <summary>Loads an asset from the package path with a package-scoped fallback.</summary>
        private static T LoadFirstFromPackage<T>(
            string fileName, Func<string, T> loadByPath) where T : UnityEngine.Object
        {

            string packagePath = PackageUxmlPath(fileName);
            T direct = loadByPath(packagePath);
            if (direct != null) return direct;

            string[] guids = AssetDatabase.FindAssets(
                $"{fileName} t:{typeof(T).Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsInsidePackage(path)) continue;

                T found = loadByPath(path);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>Builds the UXML path inside the package.</summary>
        private static string PackageUxmlPath(string fileName) =>
            $"Packages/{PackageName}/{UxmlDirectory}/{fileName}";

        /// <summary>True when the asset path belongs to this package.</summary>
        private static bool IsInsidePackage(string assetPath) =>
            assetPath.StartsWith($"Packages/{PackageName}/", StringComparison.Ordinal);
    }
}
