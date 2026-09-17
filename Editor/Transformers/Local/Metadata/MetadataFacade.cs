using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Concrete metadata facade.</summary>
    public sealed class MetadataFacade : IMetadataFacade
    {
        private readonly IEmbeddingModule _embedding;
        private readonly IMetadataStorage _storage;

        /// <summary>Captures the embedding module and metadata storage.</summary>
        public MetadataFacade(IEmbeddingModule embedding, IMetadataStorage storage)
        {
            _embedding = embedding;
            _storage = storage;
        }

        /// <summary>Deletes all rows of a category.</summary>
        public void MetadataClear(DatabaseType type) => _storage.MetadataClear(type);
        /// <summary>Inserts or replaces category rows.</summary>
        public void MetadataInsert(MetadataTable[] values) => _storage.MetadataInsert(values);
        /// <summary>Returns a category's rows keyed by id.</summary>
        public Dictionary<string, MetadataTable> MetadataCategory(DatabaseType type) =>
            _storage.MetadataCategory(type);
        /// <summary>Atomically replaces a category.</summary>
        public void MetadataReplace(DatabaseType type, MetadataTable[] values) =>
            _storage.MetadataReplace(type, values);

        /// <summary>Embeds metadata words in place.</summary>
        public async Task GetVectorsAsync(MetadataWord[] words, CancellationToken ct)
        {
            float[][] result = await GetVectorsAsync(
                words.Select(a => a.Word).ToArray(), ct);

            for (int i = 0; i < words.Length; i++)
            {
                words[i].Vector = result[i];
            }
        }

        /// <summary>Embeds the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] words, CancellationToken ct) =>
            _embedding.GetVectorsAsync(words, ct);

    }
}
