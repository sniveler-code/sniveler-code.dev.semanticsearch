namespace SnivelerCode.SemanticSearch.Editor.Core.Storage
{
    /// <summary>Row of the properties table.</summary>
    public sealed class PropertiesTable
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>Row of the indexed assets table.</summary>
    public sealed class AssetsTable
    {
        public string Guid { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public string Metadata { get; set; }
        public byte[] Vector { get; set; }
        public AssetStorageType StorageType { get; set; }
        public AssetStorageStatus Status { get; set; }
    }

    /// <summary>Row of a baked category database.</summary>
    public sealed class MetadataTable
    {
        public string Id { get; set; }
        public DatabaseType Category { get; set; }
        public string Parent { get; set; }
        public string Keys { get; set; }
        public string Values { get; set; }
        public byte[] Vector { get; set; }
    }
}
