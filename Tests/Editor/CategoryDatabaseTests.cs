using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using Newtonsoft.Json.Linq;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Validates the shipped category keyword databases used by LocalTransformer.</summary>
    public class CategoryDatabaseTests
    {
        /// <summary>Folder containing the shipped databases.</summary>
        private static string DatabaseDir => Path.Combine(
            Path.GetFullPath("Packages/sniveler-code.dev.semanticsearch"),
            "Editor/Transformers/Local/Prefabs/Database");

        private static readonly string[] DatabaseFiles =
        {
            "GeneralDatabase.json",
            "DetailedDatabase.json",
            "ComponentsDatabase.json",
            "MaterialsDatabase.json"
        };

        /// <summary>Entries have unique ids and non-empty keys and values.</summary>
        [TestCaseSource(nameof(DatabaseFiles))]
        public void File_HasValidCategories_WithUniqueIds(string fileName)
        {
            string path = Path.Combine(DatabaseDir, fileName);
            Assume.That(File.Exists(path), $"database file missing: {fileName}");

            var library = JObject.Parse(File.ReadAllText(path));
            var categories = library["categories"] as JArray;

            Assert.IsNotNull(categories, $"{fileName}: missing 'categories' array");
            Assert.Greater(categories.Count, 0, $"{fileName}: empty categories");

            var ids = new HashSet<string>();
            foreach (JToken category in categories)
            {
                string id = (string) category["id"];
                Assert.IsNotEmpty(id, $"{fileName}: category without id");
                Assert.IsTrue(ids.Add(id), $"{fileName}: duplicate category id '{id}'");

                var keys = category["keys"] as JValue;
                var values = category["values"] as JValue;
                Assert.IsNotNull(keys, $"{fileName}: '{id}' missing 'keys'");
                Assert.IsNotNull(values, $"{fileName}: '{id}' missing 'values'");
                Assert.IsNotEmpty(keys.ToString(), $"{fileName}: '{id}' empty keys");
                Assert.IsNotEmpty(values.ToString(), $"{fileName}: '{id}' empty values");

                JToken examples = category["examples"];
                if (examples is JArray examplesArray)
                {
                    Assert.Greater(examplesArray.Count, 0,
                        $"{fileName}: '{id}' has an empty examples array");
                    foreach (JToken example in examplesArray)
                    {
                        Assert.IsNotEmpty(example.ToString(),
                            $"{fileName}: '{id}' has an empty example");
                    }
                }
            }
        }

        /// <summary>All database files exist and are valid JSON.</summary>
        [Test]
        public void AllDatabases_ExistAndParse()
        {
            foreach (string fileName in DatabaseFiles)
            {
                string path = Path.Combine(DatabaseDir, fileName);
                Assume.That(File.Exists(path), $"database file missing: {fileName}");
                Assert.DoesNotThrow(() => JObject.Parse(File.ReadAllText(path)), fileName);
            }
        }

        /// <summary>Category ids are unique across all four databases.</summary>
        [Test]
        public void Ids_AreUniqueAcrossAllDatabases()
        {
            var owners = new Dictionary<string, string>();
            foreach (string fileName in DatabaseFiles)
            {
                foreach (JToken category in LoadCategories(fileName))
                {
                    string id = (string) category["id"];
                    Assert.IsFalse(owners.ContainsKey(id),
                        $"duplicate category id '{id}' in {fileName} (already in {owners.GetValueOrDefault(id)})");
                    owners[id] = fileName;
                }
            }
        }

        /// <summary>Detailed parents point at existing General categories.</summary>
        [Test]
        public void DetailedParents_ResolveToGeneralCategories()
        {
            var generalIds = new HashSet<string>();
            foreach (JToken category in LoadCategories("GeneralDatabase.json"))
            {
                generalIds.Add((string) category["id"]);
            }

            foreach (JToken category in LoadCategories("DetailedDatabase.json"))
            {
                string id = (string) category["id"];
                string parent = (string) category["parent"];
                Assert.AreNotEqual(id, parent, $"'{id}' must not be its own parent");
                Assert.IsTrue(generalIds.Contains(parent),
                    $"'{id}' has unknown parent '{parent}'");
            }
        }

        /// <summary>No two categories share three or more keys.</summary>
        [Test]
        public void NoStrongKeyOverlap_WithinAnyDatabase()
        {
            foreach (string fileName in DatabaseFiles)
            {
                var categories = LoadCategories(fileName);
                var keys = new List<HashSet<string>>();
                var ids = new List<string>();

                foreach (JToken category in categories)
                {
                    ids.Add((string) category["id"]);
                    var set = new HashSet<string>();
                    string raw = (string) category["keys"] ?? "";
                    foreach (string word in raw.SplitWords(",")) set.Add(word);
                    keys.Add(set);
                }

                for (int i = 0; i < keys.Count; i++)
                {
                    for (int j = i + 1; j < keys.Count; j++)
                    {
                        int shared = 0;
                        foreach (string word in keys[i])
                        {
                            if (keys[j].Contains(word)) shared++;
                        }

                        Assert.Less(shared, 3,
                            $"{fileName}: '{ids[i]}' and '{ids[j]}' share {shared} keys — merge or narrow them");
                    }
                }
            }
        }

        /// <summary>Databases stay above their coverage floors.</summary>
        [Test]
        public void Databases_HaveBroadCoverage()
        {
            Assert.GreaterOrEqual(LoadCategories("GeneralDatabase.json").Count, 70,
                "General database lost coverage");
            Assert.GreaterOrEqual(LoadCategories("DetailedDatabase.json").Count, 120,
                "Detailed database lost coverage");
            Assert.GreaterOrEqual(LoadCategories("ComponentsDatabase.json").Count, 25,
                "Components database lost coverage");
            Assert.GreaterOrEqual(LoadCategories("MaterialsDatabase.json").Count, 10,
                "Materials database lost coverage");
        }

        /// <summary>Loads the categories array of a database file.</summary>
        private static JArray LoadCategories(string fileName)
        {
            string path = Path.Combine(DatabaseDir, fileName);
            Assume.That(File.Exists(path), $"database file missing: {fileName}");
            return JObject.Parse(File.ReadAllText(path))["categories"] as JArray;
        }
    }
}