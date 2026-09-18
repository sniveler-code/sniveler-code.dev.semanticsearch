using System;
using System.IO;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite
{
    public sealed partial class SqliteStorage : IDisposable
    {
        private readonly SQLiteConnection _connection;

        /// <summary>
        /// Bumped for future schema migrations. Persisted in the properties table
        /// (category "_schema") so tooling can detect index-format changes.
        /// </summary>
        public const int SchemaVersion = 1;

        /// <summary>
        /// Path of the project-local index database, derived from the project location
        /// (REVIEW M14) instead of the current working directory, which is only an editor
        /// convention.
        /// </summary>
        private static string dbPath => Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "Library", "SnivelerCode_SemanticIndex.db"));

        /// <summary>Opens the project-local index database and ensures the schema.</summary>
        public SqliteStorage()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            _connection = new(dbPath);

            InitAssets();
            InitProperties();
            InitMetadata();
            SetProperty("_schema", "version", SchemaVersion.Bytes());
        }

        /// <summary>Closes the database connection.</summary>
        public void Dispose() => _connection.Close();
    }
}
