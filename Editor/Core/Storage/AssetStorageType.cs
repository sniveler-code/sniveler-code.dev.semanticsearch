namespace SnivelerCode.SemanticSearch.Editor.Core.Storage
{
    /// <summary>Kind of asset kept in the semantic index.</summary>
    public enum AssetStorageType
    {
        None = 0,
        Prefabs = 1
    }

    /// <summary>Indexing state of a stored asset.</summary>
    public enum AssetStorageStatus
    {
        Checked = 0,
        Indexed = 1
    }
}