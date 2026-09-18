using System.Collections.Generic;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>
    /// Debug/authoring window: reports which baked categories are never matched by the
    /// indexed corpus (dead categories) and which assets match no category at all.
    /// Read-only — it never writes to the database.
    /// </summary>
    public sealed class CategoryCoverageWindow : EditorWindow
    {
        private static readonly DatabaseType[] AllDatabases =
        {
            DatabaseType.General, DatabaseType.Detailed,
            DatabaseType.Components, DatabaseType.Materials
        };

        private Label _summary;
        private ScrollView _categories;
        private Label _uncovered;
        private CoverageReport _report;

        /// <summary>Opens the coverage window.</summary>
        [MenuItem("Window/SnivelerCode/Category Coverage (Debug)")]
        public static void ShowWindow() =>
            GetWindow<CategoryCoverageWindow>("Category Coverage");

        /// <summary>Builds the toolbar, summary, list and uncovered section.</summary>
        public void CreateGUI()
        {
            var toolbar = new VisualElement
            {
                style = {flexDirection = FlexDirection.Row, marginBottom = 4, flexShrink = 0}
            };
            toolbar.Add(new Button(Build) {text = "Refresh"});
            toolbar.Add(new Button(LogReport) {text = "Log to Console"});
            rootVisualElement.Add(toolbar);

            _summary = new Label {style = {whiteSpace = WhiteSpace.Normal, marginBottom = 4}};
            rootVisualElement.Add(_summary);

            _categories = new ScrollView {style = {flexGrow = 1}};
            rootVisualElement.Add(_categories);

            _uncovered = new Label
            {
                style = {whiteSpace = WhiteSpace.Normal, marginTop = 4, maxHeight = 120}
            };
            rootVisualElement.Add(_uncovered);

            Build();
        }

        /// <summary>Loads categories and assets and renders the report.</summary>
        private void Build()
        {
            _categories.Clear();
            _uncovered.text = "";

            using var container = new MiniContainer();
            container.Bind<SqliteStorage>();

            AssetsTable[] assets = container.Resolve<IAssetsStorage>().GetIndexes();
            var metadata = container.Resolve<IMetadataStorage>();
            var categories = new List<MetadataTable>();
            foreach (DatabaseType type in AllDatabases)
            {
                foreach (KeyValuePair<string, MetadataTable> row in metadata.MetadataCategory(type))
                {
                    categories.Add(row.Value);
                }
            }

            _report = CoverageReport.Build(categories.ToArray(), assets);

            _summary.text =
                $"Indexed assets: {_report.AssetCount}   |   categories: {_report.CategoryCount} " +
                $"(matched {_report.MatchedCategoryCount}, unmatched {_report.UnmatchedCategoryCount})   |   " +
                $"assets with no category match: {_report.UncoveredAssetCount}";

            foreach (CategoryCoverageEntry entry in _report.Entries)
            {
                string line = $"[{entry.Database}] {entry.Id} — {entry.Hits} hits";
                if (entry.SampleAssets.Count > 0)
                {
                    line += $"  ({string.Join(", ", entry.SampleAssets)})";
                }

                _categories.Add(new Label(line)
                {
                    style = {color = entry.Hits == 0 ? new Color(1f, 0.45f, 0.45f) : Color.white}
                });
            }

            if (_report.UncoveredAssets.Count > 0)
            {
                _uncovered.text = "Assets with no category match:\n" +
                                  string.Join("\n", _report.UncoveredAssets);
            }
        }

        /// <summary>Logs unmatched categories to the console.</summary>
        private void LogReport()
        {
            if (_report == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[SemanticSearch] Category coverage report");
            sb.AppendLine($"  assets: {_report.AssetCount}, categories: {_report.CategoryCount}, " +
                          $"unmatched: {_report.UnmatchedCategoryCount}, " +
                          $"uncovered assets: {_report.UncoveredAssetCount}");

            foreach (CategoryCoverageEntry entry in _report.Entries)
            {
                if (entry.Hits == 0)
                {
                    sb.AppendLine($"  UNMATCHED [{entry.Database}] {entry.Id}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
