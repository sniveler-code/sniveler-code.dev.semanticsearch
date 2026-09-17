using System.Collections.Generic;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Tests content-hash persistence of the local transformer facade.</summary>
    public class LocalTransformerFacadeTests
    {
        /// <summary>In-memory properties storage for tests.</summary>
        private sealed class FakeProperties : IPropertiesStorage
        {
            private readonly Dictionary<string, Dictionary<string, byte[]>> _data = new();

            /// <summary>Stores a value in memory.</summary>
            public void SetProperty(string category, string name, byte[] data)
            {
                if (!_data.TryGetValue(category, out Dictionary<string, byte[]> bucket))
                {
                    bucket = new Dictionary<string, byte[]>();
                    _data[category] = bucket;
                }

                bucket[name] = data;
            }

            /// <summary>Returns all values of a category.</summary>
            public Dictionary<string, byte[]> GetProperties(string category) =>
                _data.TryGetValue(category, out Dictionary<string, byte[]> bucket)
                    ? bucket
                    : new Dictionary<string, byte[]>();
        }

        /// <summary>Creates a facade with only the properties storage wired.</summary>
        private static LocalTransformerFacade Facade(IPropertiesStorage properties) =>
            new(null, null, null, properties);

        /// <summary>An absent hash returns null instead of throwing.</summary>
        [Test]
        public void GetDatabaseContentHash_ReturnsNull_WhenNeverBaked()
        {
            var facade = Facade(new FakeProperties());

            Assert.DoesNotThrow(() => facade.GetDatabaseContentHash(DatabaseType.General));
            Assert.IsNull(facade.GetDatabaseContentHash(DatabaseType.General));
            Assert.IsNull(facade.GetDatabaseContentHash(DatabaseType.Detailed));
        }

        /// <summary>Stored hashes are returned per database type.</summary>
        [Test]
        public void SetThenGetContentHash_Roundtrips()
        {
            var facade = Facade(new FakeProperties());

            facade.SetDatabaseContentHash(DatabaseType.General, "ABC123");

            Assert.AreEqual("ABC123", facade.GetDatabaseContentHash(DatabaseType.General));
            Assert.IsNull(facade.GetDatabaseContentHash(DatabaseType.Detailed),
                "hashes are per database type");
        }

        /// <summary>Storing a hash again overwrites the previous value.</summary>
        [Test]
        public void SetContentHash_OverwritesPreviousValue()
        {
            var facade = Facade(new FakeProperties());

            facade.SetDatabaseContentHash(DatabaseType.Materials, "v1");
            facade.SetDatabaseContentHash(DatabaseType.Materials, "v2");

            Assert.AreEqual("v2", facade.GetDatabaseContentHash(DatabaseType.Materials));
        }

        /// <summary>Hashes are persisted as UTF-8 bytes.</summary>
        [Test]
        public void StoredHash_SurvivesByteRoundtrip()
        {
            var properties = new FakeProperties();
            var facade = Facade(properties);

            facade.SetDatabaseContentHash(DatabaseType.Components, "FF00AA");

            byte[] raw = properties.GetProperties("_bake")["Components"];
            Assert.AreEqual("FF00AA", raw.String());
            CollectionAssert.AreEqual("FF00AA".Bytes(), raw);
        }
    }
}