using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Tabs;
using SnivelerCode.SemanticSearch.Editor.Transformers;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Concrete prefabs facade.</summary>
    public sealed class PrefabsFacade : IPrefabsFacade
    {
        public IStatusView Status { get; }
        public IAssetsStorage Storage { get; }
        public ITransformer[] Transformers { get; }
        public ITabsModule TabsModule { get; set; }
        public PropertyFactory PropertyFactory { get; }

        /// <summary>Captures transformers, storage, status, tabs and properties.</summary>
        public PrefabsFacade(ITransformer[] transformer, IAssetsStorage storage, IStatusView status,
            ITabsModule tabsModule, PropertyFactory propertyFactory)
        {
            Status = status;
            Storage = storage;
            Transformers = transformer;
            TabsModule = tabsModule;
            PropertyFactory = propertyFactory;
        }
    }
}
