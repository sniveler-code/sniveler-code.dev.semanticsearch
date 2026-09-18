using System.Collections.Generic;
using System.Linq;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    public sealed partial class SqliteStorage : IMetadataStorage
    {
        /// <summary>Creates the metadata table and its indexes.</summary>
        private void InitMetadata()
        {
            _connection.CreateTable<MetadataTable>();
            const string tableName = nameof(MetadataTable);
            _connection.CreateIndex(
                tableName,
                new[]
                {
                    nameof(MetadataTable.Id),
                    nameof(MetadataTable.Category)
                },
                unique: true
            );
        }

        /// <summary>Deletes all rows of a category.</summary>
        public int MetadataClear(DatabaseType category)
        {
            int result = _connection.Table<MetadataTable>()
                .Delete(i => i.Category == category);
            _connection.Execute("VACUUM");
            return result;
        }

        /// <summary>Returns a category's rows keyed by id.</summary>
        public Dictionary<string, MetadataTable> MetadataCategory(DatabaseType category) =>
            _connection.Table<MetadataTable>()
                .Where(p => p.Category == category)
                .ToDictionary(a => a.Id, a => a);

        /// <summary>Inserts or replaces rows in a transaction.</summary>
        public void MetadataInsert(MetadataTable[] values)
        {
            _connection.RunInTransaction(() =>
            {
                foreach (MetadataTable value in values)
                {
                    _connection.InsertOrReplace(value);
                }
            });
        }

        /// <summary>Atomically clears and re-inserts a category.</summary>
        public void MetadataReplace(DatabaseType category, MetadataTable[] values)
        {

            _connection.RunInTransaction(() =>
            {
                _connection.Table<MetadataTable>()
                    .Delete(i => i.Category == category);

                foreach (MetadataTable value in values)
                {
                    _connection.InsertOrReplace(value);
                }
            });

            _connection.Execute("VACUUM");
        }
    }
}
