using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Prefabs;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Storage.SQLite;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>
    /// Guard tests for review fixes that would otherwise regress silently:
    /// M8 progress NaN, M9 empty Components database, M14 database path anchoring.
    /// </summary>
    public class GuardTests
    {
        /// <summary>Zero or single-asset scans report 100%, not NaN (REVIEW M8).</summary>
        [TestCase(0, 0)]
        [TestCase(0, 1)]
        public void Progress_ZeroOrSingleTotal_IsHundred(int i, int total)
        {
            Assert.AreEqual(100f, PrefabsModule.ProgressPercent(i, total));
        }

        /// <summary>Multi-asset progress spans 0..100 across the run (REVIEW M8).</summary>
        [Test]
        public void Progress_MultiTotal_SpansZeroToHundred()
        {
            Assert.AreEqual(0f, PrefabsModule.ProgressPercent(0, 5));
            Assert.AreEqual(50f, PrefabsModule.ProgressPercent(2, 5));
            Assert.AreEqual(100f, PrefabsModule.ProgressPercent(4, 5));
        }

        /// <summary>Similarity on an empty category table yields no rows, not an exception (REVIEW M9).</summary>
        [Test]
        public void Similarity_EmptyTable_ReturnsNoRows()
        {
            var table = new Dictionary<string, MetadataTable>();
            var results = table.Similarity(new[]
            {
                new MetadataWord("HeavyAxe") {Vector = new float[4] {1f, 0f, 0f, 0f}}
            });
            Assert.AreEqual(0, results.Count);
        }

        /// <summary>The index database lives under the project root, not the working directory (REVIEW M14).</summary>
        [Test]
        public void DbPath_IsAnchoredToProjectRoot()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Assert.AreEqual(Path.Combine(projectRoot, "Library", "SnivelerCode_SemanticIndex.db"),
                SqliteStorage.DbPath);
        }
    }
}
