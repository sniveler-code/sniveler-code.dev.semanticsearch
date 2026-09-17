using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Search tab UI: query, sensitivity slider and result tabs.</summary>
    public sealed class SearchModuleView
    {
        public VisualElement Content { get; }

        /// <summary>Builds the search controls from the template.</summary>
        public SearchModuleView(SearchModel model)
        {
            var template = TemplateProvider.LoadByName("SemanticSearch_SearchModule");
            Content = template.Instantiate().Q<Tab>();

            var queryInput = Content.Q<TextField>("QueryInput");
            var queryButton = Content.Q<Button>("QueryButton");
            queryButton.clicked += () => model.Query(queryInput.value);
            queryButton.SetEnabled(model.Embedding);

            var sensitivitySlider = Content.Q<SliderInt>("SensitivitySlider");
            sensitivitySlider.value = model.Sensitivity;
            sensitivitySlider.RegisterValueChangedCallback(v => model.Sensitivity = v.newValue);

            var resultTabs = Content.Q<TabView>("ResultTabs");
            resultTabs.SetEnabled(false);

            model.ProcessButton = value => queryButton.SetEnabled(value);
            model.SetResults = items =>
            {
                resultTabs.SetEnabled(true);
                foreach (ISearchResult module in model.Modules)
                {
                    module.SetResults(items);
                }
            };

            foreach (ISearchResult module in model.Modules)
            {
                resultTabs.Add(module.Content);
            }
        }
    }
}
