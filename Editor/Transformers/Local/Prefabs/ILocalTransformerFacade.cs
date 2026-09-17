using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs
{
    /// <summary>Dependencies exposed to the local transformer.</summary>
    public interface ILocalTransformerFacade
    {
        public IStatusView Status { get; }
        /// <summary>Stores baked category rows.</summary>
        public void DatabaseInsert(DatabaseType category, MetadataTable[] toArray);
        /// <summary>Embeds the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] texts, CancellationToken ct = default);
        /// <summary>Returns baked category rows.</summary>
        public Dictionary<string, MetadataTable> GetCategoryKeywords(DatabaseType category);
        /// <summary>Stores asset rows.</summary>
        public void SetIndexes(AssetsTable[] toArray);

        /// <summary>Content hash of the baked source JSON (null when never baked).</summary>
        public string GetDatabaseContentHash(DatabaseType category);
        /// <summary>Stores the source hash of a database.</summary>
        public void SetDatabaseContentHash(DatabaseType category, string hash);
    }
}