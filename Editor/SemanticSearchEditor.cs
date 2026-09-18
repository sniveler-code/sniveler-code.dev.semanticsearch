using UnityEditor;
using UnityEngine.UIElements;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding.MiniLM;
using SnivelerCode.SemanticSearch.Editor.Core.Prefabs;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Search;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite;
using SnivelerCode.SemanticSearch.Editor.Core.Tabs;
using SnivelerCode.SemanticSearch.Editor.Templates;
using SnivelerCode.SemanticSearch.Editor.Transformers.Gemini;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Processors;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor
{
    /// <summary>Main window hosting the search, embedding and prefab tabs.</summary>
    public sealed class SemanticSearchEditor : EditorWindow
    {
        private readonly MiniContainer _container = new();

        /// <summary>Opens the Semantic Search window.</summary>
        [MenuItem("Window/SnivelerCode/Semantic Search")]
        public static void ShowWindow() => GetWindow<SemanticSearchEditor>("Asset AI Search");

        /// <summary>Builds the window UI and wires the service container.</summary>
        public void CreateGUI()
        {
            try
            {
                RegisterContainer();
                ApplyWindowStyleSheets();

                var tabsModule = _container.Resolve<ITabsModule>();
                rootVisualElement.Add(tabsModule.Content);

                var status = _container.Resolve<IStatusView>();
                rootVisualElement.Add(status.Content);

                var embedding = _container.Resolve<IEmbeddingModule>();
                var prefabs = _container.Resolve<PrefabsModule>();

                var search = _container.Resolve<SearchModule>();
                tabsModule.Add(search.Content);

                tabsModule.Add(embedding.Content);
                tabsModule.Add(prefabs.Content);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                rootVisualElement.Add(new Label(
                    $"[SemanticSearch] Failed to open the window: {e.Message}") { style = { color = Color.red } });
            }
        }

        /// <summary>Applies the package style sheet and the optional Inspector override.</summary>
        private void ApplyWindowStyleSheets()
        {
            StyleSheet packageStyle = TemplateProvider.LoadStyleSheetByName("SemanticSearchEditor");
            if (packageStyle != null)
                rootVisualElement.styleSheets.Add(packageStyle);
        }

        /// <summary>Registers every editor service in the container.</summary>
        private void RegisterContainer()
        {
            _container.Bind<TabsModule>();
            _container.Bind<SqliteStorage>();
            _container.Bind<StatusView>();

            _container.Bind<EmbeddingModule>();
            _container.Bind<EmbeddingFacade>();
            _container.Bind<PropertyFactory>();

            _container.Bind<PrefabsModule>();
            _container.Bind<PrefabsFacade>();
            // REVIEW M13: the result view is registered here (not in PrefabsModule's
            // constructor), so every ISearchResult implementation exists before any module
            // is resolved — the SearchModule's ISearchResult[] no longer depends on the
            // resolve order in CreateGUI().
            _container.Bind<PrefabsResultView>();

            _container.Bind<SearchFacade>();
            _container.Bind<SearchModule>();

            _container.Bind<MiniProcessor>();
            _container.Bind<LocalMetadataProcessor>();
            _container.Bind<LocalTransformer>();
            _container.Bind<GeminiTransformer>();
            _container.Bind<ContextMatchConfig>();

            _container.Bind<MetadataFacade>();
            _container.Bind<LocalTransformerFacade>();
        }

        /// <summary>Releases the container when the window closes.</summary>
        private void OnDestroy() => _container.Dispose();
    }
}
