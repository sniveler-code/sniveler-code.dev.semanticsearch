using System;
using System.IO;
using SnivelerCode.SemanticSearch.Editor.Core.Property;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    public sealed partial class SqliteStorage : IDisposable
    {
        private readonly SQLiteConnection _connection = new(dbPath);

        /// <summary>
        /// Bumped for future schema migrations. Persisted in the properties table
        /// (category "_schema") so tooling can detect index-format changes.
        /// </summary>
        public const int SchemaVersion = 1;

        private static string dbPath => Path.Combine(
            Directory.GetCurrentDirectory(), "Library", "SnivelerCode_SemanticIndex.db");

        /// <summary>Opens the project-local index database and ensures the schema.</summary>
        public SqliteStorage()
        {
            InitAssets();
            InitProperties();
            InitMetadata();
            SetProperty("_schema", "version", SchemaVersion.Bytes());
        }

        /// <summary>Closes the database connection.</summary>
        public void Dispose() => _connection.Close();
    }
}
