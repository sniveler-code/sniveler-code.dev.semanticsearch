using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Dependencies exposed to the search module.</summary>
    public interface ISearchFacade
    {
        public IEmbeddingModule Embedding { get; }
        public IStatusView Status { get; }
        /// <summary>Returns every indexed asset.</summary>
        public AssetsTable[] GetAssetsIndexes();
    }
}
