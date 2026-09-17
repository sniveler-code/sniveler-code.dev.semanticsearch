using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Tabs;
using SnivelerCode.SemanticSearch.Editor.Transformers;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Dependencies exposed to the prefabs module.</summary>
    public interface IPrefabsFacade
    {
        public IStatusView Status { get; }
        public IAssetsStorage Storage { get; }
        public ITransformer[] Transformers { get; }
        public ITabsModule TabsModule { get; }
        public PropertyFactory PropertyFactory { get; }
    }
}
