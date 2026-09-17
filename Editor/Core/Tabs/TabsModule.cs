using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Tabs
{
    /// <summary>TabView wrapper implementing the tab container.</summary>
    public sealed class TabsModule : ITabsModule
    {
        private readonly TabView _tabsView = new()
        {
            style =
            {
                marginLeft = 5,
                marginRight = 5,
                marginTop = 5,
                flexGrow = 1,
                flexShrink = 0
            }
        };
        public VisualElement Content => _tabsView;
        /// <summary>Activates the tab that owns the element.</summary>
        public void Switch(VisualElement value)
        {
            _tabsView.activeTab = (Tab)value;
        }

        /// <summary>No-op; the tab view is owned by the window.</summary>
        public void Dispose()
        {
        }

        /// <summary>Adds a tab element to the view.</summary>
        public void Add(VisualElement element)
        {
            _tabsView.Add(element);
        }
    }
}
