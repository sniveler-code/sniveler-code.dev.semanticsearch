using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local
{
    /// <summary>Local transformer panel combining presets and settings.</summary>
    public sealed class LocalTransformView
    {
        private readonly VisualElement _presetContainer;
        private readonly TextField _propertyField;
        private readonly VisualTreeAsset _presetTemplate;

        public VisualElement Content { get; }

        /// <summary>Adds the preset and settings panels.</summary>
        public LocalTransformView(LocalTransformModel model)
        {
            Content = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };

            var presetView = new LocalTransformPresetView(model);
            var settingsView = new LocalTransformSettingsView(model);

            Content.Add(presetView.Content);
            Content.Add(settingsView.Content);
        }
    }
}
