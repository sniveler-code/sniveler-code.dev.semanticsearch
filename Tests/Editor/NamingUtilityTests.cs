using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Tests the naming and hashing helpers.</summary>
    public class NamingUtilityTests
    {
        /// <summary>The FNV-1a hash is deterministic.</summary>
        [Test]
        public void HashFnv1a_IsDeterministic()
        {
            string input = "weapons_melee, claymore, heavy_ordnance";
            Assert.AreEqual(NamingUtility.HashFnv1a(input), NamingUtility.HashFnv1a(input));
            Assert.IsNotEmpty(NamingUtility.HashFnv1a(input));
        }

        /// <summary>The hash changes when the text changes.</summary>
        [Test]
        public void HashFnv1a_ChangesOnEdit()
        {
            Assert.AreNotEqual(
                NamingUtility.HashFnv1a("combat, damage, attack"),
                NamingUtility.HashFnv1a("combat, damage, attack, extra"));
        }

        /// <summary>Different inputs produce different hashes.</summary>
        [Test]
        public void HashFnv1a_DiffersAcrossDatabases()
        {
            Assert.AreNotEqual(
                NamingUtility.HashFnv1a("weapon, melee, blade"),
                NamingUtility.HashFnv1a("wood, oak, plank"));
        }

        /// <summary>Underscores are split before embedding.</summary>
        [Test]
        public void CleanForAI_SplitsUnderscoredKeywords()
        {
            string cleaned = "heavy_ordnance, life_points".CleanForAI();
            Assert.AreEqual("heavy ordnance life points", cleaned);
        }

        /// <summary>Hyphens are split before embedding.</summary>
        [Test]
        public void CleanForAI_SplitsHyphenatedKeywords()
        {
            string cleaned = "close-quarters, four-poster, hi-tech".CleanForAI();
            Assert.AreEqual("close quarters four poster hi tech", cleaned);
        }

        /// <summary>Ampersands and commas are removed.</summary>
        [Test]
        public void CleanForAI_RemovesAmpersandAndCommas()
        {
            Assert.AreEqual("dark & moody, loud".CleanForAI(), "dark moody loud");
        }
    }
}
