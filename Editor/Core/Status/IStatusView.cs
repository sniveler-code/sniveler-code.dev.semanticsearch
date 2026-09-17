using System;

namespace SnivelerCode.SemanticSearch.Editor.Core.Status
{
    /// <summary>Status bar contract: progress, database counters and messages.</summary>
    public interface IStatusView : IDisposableContent
    {
        /// <summary>Updates the progress bar and its title.</summary>
        public void Progress(float value, string title = "");
        /// <summary>Shows indexed and total database counters.</summary>
        public void UpdateDatabase(int total, int indexed);
        /// <summary>Subscribes to message visibility changes of a category.</summary>
        public void AddMessageListener(string category, Action<bool> callback);
        /// <summary>Unsubscribes a message listener.</summary>
        public void RemoveMessageListener(string category, Action<bool> callback);
        /// <summary>Shows a message for a category, optionally clickable.</summary>
        public void RegisterMessage(string category, string message, Action clickCallback = null);
        /// <summary>Removes the message of a category.</summary>
        public void UnregisterMessage(string category);
    }
}
