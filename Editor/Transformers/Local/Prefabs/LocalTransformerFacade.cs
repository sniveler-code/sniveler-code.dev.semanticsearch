using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Status;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs
{
    /// <summary>Concrete facade for the local transformer.</summary>
    public sealed class LocalTransformerFacade : ILocalTransformerFacade
    {
        private const string HashCategory = "_bake";

        private readonly IAssetsStorage _storage;
        private readonly IMetadataFacade _metadataFacade;
        private readonly IPropertiesStorage _properties;

        public IStatusView Status { get; }

        /// <summary>Captures metadata, storage, status and properties.</summary>
        public LocalTransformerFacade(IMetadataFacade metadataFacade,
            IAssetsStorage storage, IStatusView status, IPropertiesStorage properties)
        {
            _storage = storage;
            _metadataFacade = metadataFacade;
            Status = status;
            _properties = properties;
        }

        /// <summary>Atomically replaces baked category rows.</summary>
        public void DatabaseInsert(DatabaseType type, MetadataTable[] values)
        {
            Status.Progress(100, $"Database: {type} saved");
            _metadataFacade.MetadataReplace(type, values);
        }

        /// <summary>Embeds the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] texts, CancellationToken ct = default) =>
            _metadataFacade.GetVectorsAsync(texts, ct);

        /// <summary>Returns baked category rows.</summary>
        public Dictionary<string, MetadataTable> GetCategoryKeywords(DatabaseType type) =>
            _metadataFacade.MetadataCategory(type);
        /// <summary>Stores asset rows.</summary>
        public void SetIndexes(AssetsTable[] assets) => _storage.SetIndexes(assets);

        /// <summary>Returns the stored source hash, or null.</summary>
        public string GetDatabaseContentHash(DatabaseType category)
        {
            var properties = _properties.GetProperties(HashCategory);
            return properties.TryGetValue(category.ToString(), out byte[] stored) ? stored?.String() : null;
        }

        /// <summary>Stores the source hash.</summary>
        public void SetDatabaseContentHash(DatabaseType category, string hash) =>
            _properties.SetProperty(HashCategory, category.ToString(), hash.Bytes());
    }
}
