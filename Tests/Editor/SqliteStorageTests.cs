using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>
    /// These tests create a real SQLite database at the product location
    /// (Library/SnivelerCode_SemanticIndex.db). To protect user data they run only
    /// when the database file did not exist before the test session — i.e. in a clean
    /// project / CI. In a working project with an existing index they are skipped.
    /// </summary>
    public class SqliteStorageTests
    {
        private const string DbFileName = "SnivelerCode_SemanticIndex.db";

        private string _dbPath;
        private bool _dbExistedBefore;

        /// <summary>Skips the test when a user database exists.</summary>
        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Library", DbFileName);
            _dbExistedBefore = File.Exists(_dbPath);
            Assume.That(!_dbExistedBefore,
                "Existing user database detected — skipped to protect user data. Run in a clean project or CI.");
        }

        /// <summary>Removes the database created by the test.</summary>
        [TearDown]
        public void TearDown()
        {
            if (!_dbExistedBefore && File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        /// <summary>Asset rows support insert, read, upsert and delete.</summary>
        [Test]
        public void Assets_InsertReadCountUpsertDelete()
        {
            using (var db = new SqliteStorage())
            {
                var row = new AssetsTable
                {
                    Guid = "abc123",
                    Path = "Assets/T.prefab",
                    Hash = "h1",
                    StorageType = AssetStorageType.Prefabs,
                    Status = AssetStorageStatus.Checked,
                    Vector = new byte[] {1, 2, 3, 4}
                };

                db.SetIndexes(new[] {row});
                Assert.AreEqual(1, db.GetIndexesCount(AssetStorageType.Prefabs));

                AssetsTable[] read = db.GetIndexes(AssetStorageType.Prefabs, AssetStorageStatus.Checked);
                Assert.AreEqual(1, read.Length);
                Assert.AreEqual("Assets/T.prefab", read[0].Path);
                CollectionAssert.AreEqual(row.Vector, read[0].Vector);

                row.Hash = "h2";
                db.SetIndexes(new[] {row});
                Assert.AreEqual(1, db.GetIndexesCount(AssetStorageType.Prefabs));
                Assert.AreEqual("h2",
                    db.GetIndexes(AssetStorageType.Prefabs).Values.Single().Hash);
            }
        }

        /// <summary>Checked and indexed counters are independent.</summary>
        [Test]
        public void Assets_StatusesAreIndependent()
        {
            using (var db = new SqliteStorage())
            {
                db.SetIndexes(new[]
                {
                    new AssetsTable {Guid = "g1", Path = "a", Hash = "h",
                        StorageType = AssetStorageType.Prefabs, Status = AssetStorageStatus.Checked},
                    new AssetsTable {Guid = "g2", Path = "b", Hash = "h",
                        StorageType = AssetStorageType.Prefabs, Status = AssetStorageStatus.Indexed}
                });

                Assert.AreEqual(1, db.GetIndexesCount(AssetStorageType.Prefabs, AssetStorageStatus.Checked));
                Assert.AreEqual(1, db.GetIndexesCount(AssetStorageType.Prefabs, AssetStorageStatus.Indexed));
            }
        }

        /// <summary>ClearIndexes removes only the given GUIDs.</summary>
        [Test]
        public void ClearIndexes_RemovesOnlyGivenGuids()
        {
            using (var db = new SqliteStorage())
            {
                db.SetIndexes(new[]
                {
                    new AssetsTable {Guid = "a", Path = "1", Hash = "h",
                        StorageType = AssetStorageType.Prefabs, Status = AssetStorageStatus.Checked},
                    new AssetsTable {Guid = "b", Path = "2", Hash = "h",
                        StorageType = AssetStorageType.Prefabs, Status = AssetStorageStatus.Checked}
                });

                int removed = db.ClearIndexes(new System.Collections.Generic.HashSet<string> {"a"});
                Assert.AreEqual(1, removed);
                Assert.AreEqual(1, db.GetIndexesCount(AssetStorageType.Prefabs));
            }
        }

        /// <summary>Property values round-trip through SQLite.</summary>
        [Test]
        public void Properties_RoundtripThroughSqlite()
        {
            using (var db = new SqliteStorage())
            {
                byte[] data = {(byte) 0xDE, (byte) 0xAD, (byte) 0xBE, (byte) 0xEF};
                db.SetProperty("category-a", "key1", data);

                var props = db.GetProperties("category-a");
                Assert.IsTrue(props.ContainsKey("key1"));
                CollectionAssert.AreEqual(data, props["key1"]);
            }
        }

        /// <summary>MetadataReplace swaps a category atomically.</summary>
        [Test]
        public void Metadata_Replace_SwapsCategoryAtomically()
        {
            using (var db = new SqliteStorage())
            {
                db.MetadataInsert(new[]
                {
                    new MetadataTable {Id = "old_1", Category = DatabaseType.General, Keys = "a", Values = "b"},
                    new MetadataTable {Id = "old_2", Category = DatabaseType.General, Keys = "c", Values = "d"}
                });
                Assert.AreEqual(2, db.MetadataCategory(DatabaseType.General).Count);

                db.MetadataReplace(DatabaseType.General, new[]
                {
                    new MetadataTable {Id = "new_1", Category = DatabaseType.General, Keys = "x", Values = "y"}
                });

                var after = db.MetadataCategory(DatabaseType.General);
                Assert.AreEqual(1, after.Count, "old rows must be gone after replace");
                Assert.IsTrue(after.ContainsKey("new_1"));
                Assert.AreEqual("x", after["new_1"].Keys);
            }
        }

        /// <summary>Metadata rows support insert, read and clear.</summary>
        [Test]
        public void Metadata_InsertReadClear()
        {
            using (var db = new SqliteStorage())
            {
                var row = new MetadataTable
                {
                    Id = "weapons_melee",
                    Category = DatabaseType.General,
                    Keys = "weapon, blade",
                    Values = "combat, attack",
                    Vector = new byte[4]
                };

                db.MetadataInsert(new[] {row});

                var category = db.MetadataCategory(DatabaseType.General);
                Assert.IsTrue(category.ContainsKey("weapons_melee"));
                Assert.AreEqual("weapon, blade", category["weapons_melee"].Keys);

                db.MetadataClear(DatabaseType.General);
                Assert.AreEqual(0, db.MetadataCategory(DatabaseType.General).Count);
            }
        }
    }
}