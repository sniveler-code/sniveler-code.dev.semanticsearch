using System;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Prefabs tab UI: transformer dropdown, check and index buttons.</summary>
    public sealed class PrefabsView : IDisposable
    {
        private readonly VisualElement _contentContainer;
        private string _prevPropertyValue;
        private readonly Button _indexButton;
        private readonly PrefabsModel _model;
        private readonly Label _messageContainer;
        public VisualElement Content { get; }

        /// <summary>Builds the view and the transformer content panels.</summary>
        public PrefabsView(PrefabsModel model)
        {
            _model = model;
            _prevPropertyValue = string.Empty;

            var template = TemplateProvider.LoadByName("SemanticSearch_PrefabsTransformer");
            Content = template.Instantiate().Q<Tab>();
            Content.contentContainer.style.flexGrow = 1;

            var propertyContainer = Content.Q<VisualElement>("PropertyContainer");
            _contentContainer = Content.Q<VisualElement>("ContentContainer");

            _model.PropertyProvider.OnPropertyChange += OnPropertyChange;

            foreach (var transformer in _model.Transformers)
            {
                _model.Property.Items.Add(transformer.ModuleName);
                var visualContent = new VisualElement
                {
                    name = transformer.ModuleName,
                    style =
                    {
                        flexGrow = 1,
                        display = DisplayStyle.None
                    }
                };

                visualContent.Add(transformer.Content);
                _contentContainer.Add(visualContent);
            }

            if (_model.Property.Items.Count > 0)
            {
                _model.Property.DefaultValue = _model.Property.Items[0];
                _model.Property.Value = _model.Property.DefaultValue;
            }

            _model.PropertyProvider.Accept(_model.Property, propertyContainer);

            _messageContainer = Content.Q<Label>("Outdated");

            var checkButton = Content.Q<Button>("CheckButton");
            checkButton.clicked += () => _model.OnCheck?.Invoke();

            _indexButton = Content.Q<Button>("IndexButton");
            _indexButton.SetEnabled(_model.ButtonIndexEnabled);
            _indexButton.clicked += () => _model.OnIndex?.Invoke();
        }

        /// <summary>Switches the visible transformer panel.</summary>
        private void OnPropertyChange(BaseProperty obj)
        {
            if (!string.IsNullOrEmpty(_prevPropertyValue))
            {
                _contentContainer.Q<VisualElement>(_prevPropertyValue)
                    .style.display = DisplayStyle.None;
            }

            _prevPropertyValue = _model.Property.Value;
            _contentContainer.Q<VisualElement>(_prevPropertyValue)
                .style.display = DisplayStyle.Flex;
        }

        /// <summary>Enables or disables the index button.</summary>
        public void UpdateIndexButton(bool value)
        {
            _indexButton.SetEnabled(value);
        }

        /// <summary>Unsubscribes from property changes.</summary>
        public void Dispose()
        {
            _model.PropertyProvider.OnPropertyChange -= OnPropertyChange;
        }

        /// <summary>Shows a status message in the tab.</summary>
        public void ToggleMessage(string message, bool isError = false)
        {
            _messageContainer.text = message;
            _messageContainer.EnableInClassList("message-label--error", isError);
        }
    }
}
