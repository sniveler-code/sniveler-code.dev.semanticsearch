using System.Collections.Generic;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage
{
    /// <summary>Asset index storage: vectors, paths and indexing status.</summary>
    public interface IAssetsStorage
    {
        /// <summary>Inserts or updates asset rows.</summary>
        public void SetIndexes(AssetsTable[] indexes);
        /// <summary>Returns stored assets of a type keyed by GUID.</summary>
        public Dictionary<string, AssetsTable> GetIndexes(AssetStorageType storageType);
        /// <summary>Returns assets of a type with the given status.</summary>
        public AssetsTable[] GetIndexes(AssetStorageType storageType, AssetStorageStatus status);
        /// <summary>Returns every indexed asset.</summary>
        public AssetsTable[] GetIndexes();
        /// <summary>Counts assets of a type.</summary>
        public int GetIndexesCount(AssetStorageType storageType);
        /// <summary>Counts assets of a type and status.</summary>
        public int GetIndexesCount(AssetStorageType storageType, AssetStorageStatus status);
        /// <summary>Removes the given GUIDs and returns how many were deleted.</summary>
        public int ClearIndexes(HashSet<string> removed);
    }
}
