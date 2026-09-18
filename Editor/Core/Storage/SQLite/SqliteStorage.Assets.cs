using System.Collections.Generic;
using System.Linq;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    public sealed partial class SqliteStorage : IAssetsStorage
    {
        private AssetsTable[] _assetCache;

        /// <summary>Creates the assets table and its indexes.</summary>
        private void InitAssets()
        {
            connection.CreateTable<AssetsTable>();

            const string tableName = nameof(AssetsTable);
            connection.CreateIndex(tableName, nameof(AssetsTable.Guid), unique: true);
            connection.CreateIndex(tableName, nameof(AssetsTable.StorageType));
            connection.CreateIndex(tableName, nameof(AssetsTable.Status));

            connection.CreateIndex(
                tableName,
                new[]
                {
                    nameof(AssetsTable.Status),
                    nameof(AssetsTable.StorageType)
                },
                unique: false
            );
        }

        /// <summary>Inserts or replaces asset rows in a transaction.</summary>
        public void SetIndexes(AssetsTable[] indexes)
        {
            connection.RunInTransaction(() =>
            {
                foreach (AssetsTable index in indexes)
                {
                    connection.InsertOrReplace(index);
                }
            });

            _assetCache = null;
        }

        /// <summary>Returns assets of a type keyed by GUID.</summary>
        public Dictionary<string, AssetsTable> GetIndexes(AssetStorageType storageType) =>
            connection.Table<AssetsTable>()
                .Where(p => p.StorageType == storageType)
                .ToDictionary(k => k.Guid, v => v);

        /// <summary>Returns assets of a type and status.</summary>
        public AssetsTable[] GetIndexes(AssetStorageType type, AssetStorageStatus status) =>
            connection.Table<AssetsTable>()
                .Where(p => p.StorageType == type && p.Status == status)
                .ToArray();

        /// <summary>Deletes every asset of a type.</summary>
        public int ClearIndexes(AssetStorageType storageType)
        {
            int result = connection.Table<AssetsTable>()
                .Delete(i => i.StorageType == storageType);
            connection.Execute("VACUUM");
            _assetCache = null;
            return result;
        }

        /// <summary>Deletes the given GUIDs.</summary>
        public int ClearIndexes(HashSet<string> guids)
        {
            int result = connection.Table<AssetsTable>()
                .Delete(i => guids.Contains(i.Guid));
            connection.Execute("VACUUM");
            _assetCache = null;
            return result;
        }

        /// <summary>Returns all indexed assets, cached between writes.</summary>
        public AssetsTable[] GetIndexes()
        {
            _assetCache ??= connection.Table<AssetsTable>()
                .Where(a => a.Status == AssetStorageStatus.Indexed)
                .ToArray();

            return _assetCache;
        }

        /// <summary>Counts assets of a type.</summary>
        public int GetIndexesCount(AssetStorageType storageType) =>
            connection.Table<AssetsTable>().Count(a => a.StorageType == storageType);

        /// <summary>Counts assets of a type and status.</summary>
        public int GetIndexesCount(AssetStorageType storageType, AssetStorageStatus status) =>
            connection.Table<AssetsTable>().Count(a
                => a.Status == status && a.StorageType == storageType);
    }
}
