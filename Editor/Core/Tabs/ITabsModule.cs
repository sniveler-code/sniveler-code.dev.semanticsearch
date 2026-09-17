using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Tabs
{
    /// <summary>Tab container contract.</summary>
    public interface ITabsModule : IDisposableContent
    {
        /// <summary>Activates the tab containing the given element.</summary>
        public void Switch(VisualElement value);
        /// <summary>Appends a tab element.</summary>
        public void Add(VisualElement element);
    }
}
