using System.Collections.Generic;
using System.Linq;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    public sealed partial class SqliteStorage : IPropertiesStorage
    {
        /// <summary>Creates the properties table and its unique index.</summary>
        private void InitProperties()
        {
            _connection.CreateTable<PropertiesTable>();
            _connection.CreateIndex(
                nameof(PropertiesTable),
                new[]
                {
                    nameof(PropertiesTable.Name),
                    nameof(PropertiesTable.Category)
                },
                unique: true
            );
        }

        /// <summary>Stores a property value.</summary>
        public void SetProperty(string category, string name, byte[] data)
        {
            _connection.InsertOrReplace(new PropertiesTable
            {
                Name = name,
                Category = category,
                Data = data
            });
        }

        /// <summary>Returns all values of a category.</summary>
        public Dictionary<string, byte[]> GetProperties(string category)
        {
            TableQuery<PropertiesTable> items = _connection.Table<PropertiesTable>()
                .Where(p => p.Category == category);
            return items.ToDictionary(p => p.Name, p => p.Data);
        }
    }
}
