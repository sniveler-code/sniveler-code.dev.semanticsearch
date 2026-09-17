using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Concrete search facade.</summary>
    public sealed class SearchFacade : ISearchFacade
    {
        private readonly IAssetsStorage _storage;

        public IStatusView Status { get; }
        public IEmbeddingModule Embedding { get; }

        /// <summary>Captures embeddings, storage and status.</summary>
        public SearchFacade(IEmbeddingModule embedding, IAssetsStorage storage, IStatusView status)
        {
            Embedding = embedding;
            _storage = storage;
            Status = status;
        }

        /// <summary>Returns every indexed asset.</summary>
        public AssetsTable[] GetAssetsIndexes() => _storage.GetIndexes();
    }
}
