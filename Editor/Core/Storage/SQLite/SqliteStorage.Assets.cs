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
            _connection.CreateTable<AssetsTable>();

            const string tableName = nameof(AssetsTable);
            _connection.CreateIndex(tableName, nameof(AssetsTable.Guid), unique: true);
            _connection.CreateIndex(tableName, nameof(AssetsTable.StorageType));
            _connection.CreateIndex(tableName, nameof(AssetsTable.Status));

            _connection.CreateIndex(
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
            _connection.RunInTransaction(() =>
            {
                foreach (AssetsTable index in indexes)
                {
                    _connection.InsertOrReplace(index);
                }
            });

            _assetCache = null;
        }

        /// <summary>Returns assets of a type keyed by GUID.</summary>
        public Dictionary<string, AssetsTable> GetIndexes(AssetStorageType storageType) =>
            _connection.Table<AssetsTable>()
                .Where(p => p.StorageType == storageType)
                .ToDictionary(k => k.Guid, v => v);

        /// <summary>Returns assets of a type and status.</summary>
        public AssetsTable[] GetIndexes(AssetStorageType type, AssetStorageStatus status) =>
            _connection.Table<AssetsTable>()
                .Where(p => p.StorageType == type && p.Status == status)
                .ToArray();

        /// <summary>Deletes every asset of a type.</summary>
        public int ClearIndexes(AssetStorageType storageType)
        {
            int result = _connection.Table<AssetsTable>()
                .Delete(i => i.StorageType == storageType);
            _connection.Execute("VACUUM");
            return result;
        }

        /// <summary>Deletes the given GUIDs.</summary>
        public int ClearIndexes(HashSet<string> guids)
        {
            int result = _connection.Table<AssetsTable>()
                .Delete(i => guids.Contains(i.Guid));
            _connection.Execute("VACUUM");
            _assetCache = null;
            return result;
        }

        /// <summary>Returns all indexed assets, cached between writes.</summary>
        public AssetsTable[] GetIndexes()
        {
            _assetCache ??= _connection.Table<AssetsTable>()
                .Where(a => a.Status == AssetStorageStatus.Indexed)
                .ToArray();

            return _assetCache;
        }

        /// <summary>Counts assets of a type.</summary>
        public int GetIndexesCount(AssetStorageType storageType) =>
            _connection.Table<AssetsTable>().Count(a => a.StorageType == storageType);

        /// <summary>Counts assets of a type and status.</summary>
        public int GetIndexesCount(AssetStorageType storageType, AssetStorageStatus status) =>
            _connection.Table<AssetsTable>().Count(a
                => a.Status == status && a.StorageType == storageType);
    }
}
