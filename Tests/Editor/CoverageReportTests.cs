using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Prefabs;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Tests the category coverage report logic.</summary>
    public class CoverageReportTests
    {
        /// <summary>Creates a category row for the report.</summary>
        private static MetadataTable Category(string id, string keys, DatabaseType db)
        {
            return new MetadataTable {Id = id, Category = db, Keys = keys, Values = "x, y"};
        }

        /// <summary>Creates an indexed asset row.</summary>
        private static AssetsTable Asset(string path, string metadata)
        {
            return new AssetsTable
            {
                Path = path, Metadata = metadata, StorageType = AssetStorageType.Prefabs,
                Status = AssetStorageStatus.Indexed
            };
        }

        /// <summary>Counts hits and reports unmatched categories.</summary>
        [Test]
        public void Build_CountsHitsAndUnmatchedCategories()
        {
            var categories = new[]
            {
                Category("weapons_melee", "weapon, melee, blade", DatabaseType.General),
                Category("space_futuristic", "laser, plasma, spaceship", DatabaseType.General)
            };
            var assets = new[]
            {
                Asset("Assets/Axe_Bronze.prefab", "this asset is a bronze axe blade for combat"),
                Asset("Assets/Wooden_Door.prefab", "this asset is a wooden door for a house")
            };

            CoverageReport report = CoverageReport.Build(categories, assets);

            Assert.AreEqual(2, report.CategoryCount);
            Assert.AreEqual(1, report.MatchedCategoryCount);
            Assert.AreEqual(1, report.UnmatchedCategoryCount);
            Assert.AreEqual(2, report.AssetCount);
            Assert.AreEqual(1, report.UncoveredAssetCount, "the door matches no category");

            Assert.AreEqual("space_futuristic", report.Entries[0].Id);
            Assert.AreEqual(0, report.Entries[0].Hits);
            Assert.AreEqual("weapons_melee", report.Entries[1].Id);
            Assert.AreEqual(1, report.Entries[1].Hits);
            Assert.AreEqual(1, report.Entries[1].SampleAssets.Count);
        }

        /// <summary>Underscored and hyphenated keywords still match.</summary>
        [Test]
        public void Build_NormalizesUnderscoresAndHyphens()
        {
            var categories = new[]
            {
                Category("combat_utility", "close-quarters, heavy_ordnance", DatabaseType.General)
            };
            var assets = new[]
            {
                Asset("Assets/Crate.prefab", "a heavy ordnance crate kept at close quarters")
            };

            CoverageReport report = CoverageReport.Build(categories, assets);

            Assert.AreEqual(1, report.MatchedCategoryCount);
            Assert.AreEqual(1, report.Entries[0].Hits);
            Assert.AreEqual(0, report.UncoveredAssetCount);
        }

        /// <summary>Empty inputs do not throw.</summary>
        [Test]
        public void Build_EmptyInputs_AreSafe()
        {
            CoverageReport empty = CoverageReport.Build(new MetadataTable[0], new AssetsTable[0]);
            Assert.AreEqual(0, empty.CategoryCount);
            Assert.AreEqual(0, empty.AssetCount);
            Assert.AreEqual(0, empty.UnmatchedCategoryCount);
            Assert.AreEqual(0, empty.UncoveredAssetCount);

            Assert.DoesNotThrow(() => CoverageReport.Build(null, null));
        }

        /// <summary>Unmatched categories sort before matched ones.</summary>
        [Test]
        public void Build_UnmatchedCategories_AreListedFirst()
        {
            var categories = new[]
            {
                Category("a_used", "used", DatabaseType.Detailed),
                Category("z_dead", "unusedkeyword", DatabaseType.Detailed)
            };
            var assets = new[] {Asset("Assets/Used_Item.prefab", "an item that is used")};

            CoverageReport report = CoverageReport.Build(categories, assets);

            Assert.AreEqual(1, report.UnmatchedCategoryCount);
            Assert.AreEqual("z_dead", report.Entries[0].Id);
            Assert.AreEqual(0, report.Entries[0].Hits);
        }

        /// <summary>Sample asset lists honour the configured limit.</summary>
        [Test]
        public void Build_SampleAssets_AreLimited()
        {
            var categories = new[] {Category("containers", "crate, barrel", DatabaseType.General)};
            var assets = new[]
            {
                Asset("Assets/Crate_A.prefab", "a crate"),
                Asset("Assets/Crate_B.prefab", "a crate"),
                Asset("Assets/Crate_C.prefab", "a crate"),
                Asset("Assets/Crate_D.prefab", "a crate")
            };

            CoverageReport report = CoverageReport.Build(categories, assets, sampleLimit: 2);

            Assert.AreEqual(4, report.Entries[0].Hits);
            Assert.AreEqual(2, report.Entries[0].SampleAssets.Count);
            Assert.AreEqual(0, report.UncoveredAssetCount);
        }
    }
}