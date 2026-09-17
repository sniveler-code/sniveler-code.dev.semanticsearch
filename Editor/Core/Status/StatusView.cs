using System;
using System.Collections.Generic;
using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Status
{
    /// <summary>Status bar widget showing progress, counters and warnings.</summary>
    public sealed class StatusView : IStatusView
    {
        private readonly ProgressBar _progressBar;
        private readonly VisualElement _messageContainer;
        private readonly Dictionary<string, VisualElement> _messages = new();
        private readonly VisualTreeAsset _warningTemplate;
        private readonly Label _databaseLabel;

        private readonly Dictionary<string, Action<bool>> _messageListeners = new();

        public VisualElement Content { get; }

        /// <summary>Builds the status bar from the package templates.</summary>
        public StatusView()
        {
            VisualTreeAsset template = TemplateProvider.LoadByName("SemanticSearch_StatusModule");
            Content = template.Instantiate().Q<VisualElement>("Root");

            _progressBar = Content.Q<ProgressBar>("ProgressBar");
            _messageContainer = Content.Q<VisualElement>("Warnings");
            _databaseLabel = Content.Q<Label>("Database");
            _warningTemplate = TemplateProvider.LoadByName("SemanticSearch_StatusModuleWarning");
        }

        /// <summary>Updates the progress bar and its title.</summary>
        public void Progress(float value, string title = "")
        {
            _progressBar.value = value;
            if (!string.IsNullOrEmpty(title)) _progressBar.title = title;
        }

        /// <summary>Shows a clickable message for a category.</summary>
        public void RegisterMessage(string category, string message, Action clickCallback = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (_messages.TryGetValue(category, out var element))
            {
                element.Q<Label>().text = message;
                return;
            }

            var warningLabel = _warningTemplate.Instantiate();
            warningLabel.RegisterCallback<ClickEvent>(_ => clickCallback?.Invoke());

            warningLabel.UnregisterAllRemovableCallbacks();

            warningLabel.AddToClassList("warning-message");
            warningLabel.Q<Label>().text = message;

            _messages.Add(category, warningLabel);
            _messageContainer.Add(warningLabel);

            if (_messageListeners.TryGetValue(category, out Action<bool> newListener))
                newListener?.Invoke(true);
        }

        /// <summary>Removes a category message.</summary>
        public void UnregisterMessage(string category)
        {
            if (!_messages.TryGetValue(category, out var element)) return;

            element.UnregisterAllRemovableCallbacks();
            _messageContainer.Remove(element);
            _messages.Remove(category);

            if (_messageListeners.TryGetValue(category, out var removeListener))
                removeListener?.Invoke(false);
        }

        /// <summary>Updates the database counter label.</summary>
        public void UpdateDatabase(int total, int indexed) =>
            _databaseLabel.text = $"Database: {indexed} / {total}";

        /// <summary>Subscribes to a category's message state.</summary>
        public void AddMessageListener(string category, Action<bool> callback)
        {
            if (_messageListeners.TryGetValue(category, out var listener))
                _messageListeners[category] = listener + callback;
            else _messageListeners[category] = callback;

            if (_messages.ContainsKey(category)) callback?.Invoke(true);
        }

        /// <summary>Unsubscribes from a category's message state.</summary>
        public void RemoveMessageListener(string category, Action<bool> callback)
        {
            if (!_messageListeners.TryGetValue(category, out var listener)) return;
            listener -= callback;

            if (listener == null)
                _messageListeners.Remove(category);
            else
                _messageListeners[category] = listener;
        }

        /// <summary>Clears messages and listeners.</summary>
        public void Dispose()
        {
            _messages.Clear();
            _messageListeners.Clear();
        }
    }
}
