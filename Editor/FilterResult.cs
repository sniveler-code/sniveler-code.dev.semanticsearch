using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor
{
    /// <summary>One semantic search hit: name, path, score and storage type.</summary>
    public struct FilterResult
    {
        public string AssetName;
        public string Path;
        public float Score;
        public AssetStorageType StorageType;
    }
}
