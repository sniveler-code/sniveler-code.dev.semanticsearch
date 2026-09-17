using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local
{
    /// <summary>Left panel listing metadata extractor toggles.</summary>
    public sealed class LocalTransformPresetView
    {
        public VisualElement Content { get; }

        /// <summary>Adds one toggle per extractor.</summary>
        public LocalTransformPresetView(LocalTransformModel model)
        {
            var template = TemplateProvider.LoadByName("SemanticSearch_PrefabsLocalPreset");

            Content = template.Instantiate().Q<VisualElement>("Root");
            var presetContainer = Content.Q<VisualElement>("PresetContainer");

            foreach (var property in model.Properties)
            {
                property.provider.Accept(property.property, presetContainer);
            }

            var fields = presetContainer.Query<VisualElement>("enable");
            fields.ForEach(a => a.style.flexDirection = FlexDirection.RowReverse);
        }
    }
}
