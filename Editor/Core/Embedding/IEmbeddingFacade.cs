using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Tabs;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Dependencies exposed to the embedding module.</summary>
    public interface IEmbeddingFacade
    {
        public ITabsModule TabsModule { get; }
        public IStatusView Status { get; }
        public PropertyFactory PropertyFactory { get; }
        public IEmbeddingProcessor[] Processors { get; }
    }
}
