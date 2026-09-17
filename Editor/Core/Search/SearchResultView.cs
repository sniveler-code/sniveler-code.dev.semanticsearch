using System.Collections.Generic;
using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Base list view for one kind of search result.</summary>
    public abstract class SearchResultView : ISearchResult
    {
        private readonly ListView _listView;
        public VisualElement Content { get; }

        /// <summary>Builds the list from the row template.</summary>
        protected SearchResultView(string title, string template)
        {
            VisualTreeAsset elementAsset = TemplateProvider.LoadByName(template);
            Content = new Tab(title)
            {
                style =
                {
                    flexGrow = 1,
                    marginBottom = 10
                }
            };
            _listView = new ListView
            {
                makeItem = () => elementAsset.Instantiate(),
                style =
                {
                    marginTop = 10,
                    flexGrow = 1
                }
            };
            _listView.bindItem = (element, i) =>
                BindResultItem((FilterResult) _listView.itemsSource[i], element);

            _listView.selectedIndicesChanged += _ =>
            {
                if (_listView.selectedIndex < 0)
                {
                    return;
                }

                VisualElement visualRow = _listView.GetRootElementForIndex(_listView.selectedIndex);
                SelectResultItem((FilterResult) _listView.selectedItem, visualRow);
            };

            Content.Add(_listView);
        }

        /// <summary>Handles selection of a result row.</summary>
        protected virtual void SelectResultItem(FilterResult item, VisualElement element)
        {
        }

        /// <summary>Fills a row with the result data.</summary>
        protected virtual void BindResultItem(FilterResult item, VisualElement element)
        {
            element.Q<Label>("Score").text = $"{item.Score * 100:F0}%";
            element.Q<Label>("Name").text = item.AssetName;
        }

        /// <summary>Replaces the list contents.</summary>
        public void SetResults(List<FilterResult> items)
        {
            _listView.itemsSource = items;
            _listView.Rebuild();
        }
    }
}
