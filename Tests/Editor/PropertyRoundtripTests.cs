using System.IO;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>
    /// Save/load roundtrips of the property layer against a real (isolated) SQLite database.
    /// Uses the same guards as SqliteStorageTests: runs only when no user DB exists yet.
    /// </summary>
    public class PropertyRoundtripTests
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

        /// <summary>Saves a property and returns the stored bytes.</summary>
        private static byte[] SaveRoundtrip(SqliteStorage db, string category, BaseProperty property)
        {
            property.Accept(new PropertySaveVisitor(category, db));
            return db.GetProperties(category)[property.Name];
        }

        /// <summary>Boolean property values persist.</summary>
        [Test]
        public void BoolProperty_Persists()
        {
            using (var db = new SqliteStorage())
            {
                var prop = new BoolProperty {Name = "enabled", Value = true};

                byte[] raw = SaveRoundtrip(db, "roundtrip-test", prop);

                var fresh = new BoolProperty {Name = "enabled", Value = false};
                fresh.Accept(new PropertyValueVisitor(raw));
                Assert.AreEqual(true, fresh.Value);
            }
        }

        /// <summary>Integer property values persist.</summary>
        [Test]
        public void IntProperty_Persists()
        {
            using (var db = new SqliteStorage())
            {
                var prop = new IntProperty {Name = "max_length", Value = 128};
                byte[] raw = SaveRoundtrip(db, "roundtrip-test", prop);

                var fresh = new IntProperty {Name = "max_length", Value = 0};
                fresh.Accept(new PropertyValueVisitor(raw));
                Assert.AreEqual(128, fresh.Value);
            }
        }

        /// <summary>String property values persist.</summary>
        [Test]
        public void StringProperty_Persists()
        {
            using (var db = new SqliteStorage())
            {
                var prop = new StringProperty {Name = "title", Value = "hello semantic"};
                byte[] raw = SaveRoundtrip(db, "roundtrip-test", prop);

                var fresh = new StringProperty {Name = "title", Value = ""};
                fresh.Accept(new PropertyValueVisitor(raw));
                Assert.AreEqual("hello semantic", fresh.Value);
            }
        }
    }
}