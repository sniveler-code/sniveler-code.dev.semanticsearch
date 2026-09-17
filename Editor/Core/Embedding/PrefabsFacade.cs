using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Tabs;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Concrete embedding facade exposing services and processors.</summary>
    public class EmbeddingFacade : IEmbeddingFacade
    {
        public ITabsModule TabsModule { get; }
        public IStatusView Status { get; }
        public PropertyFactory PropertyFactory { get; }
        public IEmbeddingProcessor[] Processors { get; }

        /// <summary>Captures tabs, status, property factory and processors.</summary>
        public EmbeddingFacade(ITabsModule tabs, IStatusView status, PropertyFactory factory,
            IEmbeddingProcessor[] processor)
        {
            Status = status;
            TabsModule = tabs;
            PropertyFactory = factory;
            Processors = processor;
        }
    }
}
