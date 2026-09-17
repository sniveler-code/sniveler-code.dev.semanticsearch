using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Facade over the embedding module and metadata storage.</summary>
    public interface IMetadataFacade
    {
        /// <summary>Deletes all rows of a category.</summary>
        public void MetadataClear(DatabaseType name);
        /// <summary>Inserts or replaces category rows.</summary>
        public void MetadataInsert(MetadataTable[] values);
        /// <summary>Returns a category's rows keyed by id.</summary>
        public Dictionary<string, MetadataTable> MetadataCategory(DatabaseType name);

        /// <summary>Atomically replaces all rows of a category (single transaction).</summary>
        public void MetadataReplace(DatabaseType name, MetadataTable[] values);

        /// <summary>Embeds the given metadata words in place.</summary>
        public Task GetVectorsAsync(MetadataWord[] words, CancellationToken ct = default);
        /// <summary>Embeds the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] texts, CancellationToken ct = default);
    }
}
