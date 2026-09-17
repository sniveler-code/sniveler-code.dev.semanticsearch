using UnityEngine.UIElements;
using SnivelerCode.SemanticSearch.Editor.Templates;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Model settings panel built from the embedding template.</summary>
    public sealed class EmbeddingModuleView
    {
        private readonly VisualElement _assetContainer;
        public VisualElement Content { get; }

        /// <summary>Builds the settings view for the model.</summary>
        public EmbeddingModuleView(EmbeddingModel embedding)
        {
            VisualTreeAsset template = TemplateProvider.LoadByName("SemanticSearch_EmbeddingModule");
            Content = template.Instantiate().Q<Tab>();

            _assetContainer = Content.Q<VisualElement>("Assets");
            embedding.PropertyProvider.Accept(embedding.Model, _assetContainer);
            embedding.PropertyProvider.Accept(embedding.Vocab, _assetContainer);

            var settingsContainer = Content.Q<VisualElement>("Settings");
            embedding.PropertyProvider.Accept(embedding.MaxLength, settingsContainer, true);
            embedding.PropertyProvider.Accept(embedding.Backend, settingsContainer, true);
        }

        /// <summary>Enables or disables a settings field.</summary>
        public void ToggleEnable(string name, bool value) =>
            _assetContainer.Q<VisualElement>(name).SetEnabled(value);

        /// <summary>Marks a settings field as invalid.</summary>
        public void ToggleError(string name, bool value)
        {
            var element = _assetContainer.Q<VisualElement>(name);
            element.EnableInClassList("field-container--error", value);
        }
    }
}
