using System.Collections.Generic;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Validates the context-matching threshold configuration.</summary>
    public class ContextMatchConfigTests
    {
        /// <summary>Threshold defaults match the documented values.</summary>
        [Test]
        public void Defaults_MatchDocumentedValues()
        {
            var config = new ContextMatchConfig();

            Assert.AreEqual(0.3f, config.DetailedThreshold.Value);
            Assert.AreEqual(0.15f, config.DetailedDecay.Value);
            Assert.AreEqual(0.25f, config.GeneralThreshold.Value);

            Assert.AreEqual(config.DetailedThreshold.DefaultValue, config.DetailedThreshold.Value);
            Assert.AreEqual(config.DetailedDecay.DefaultValue, config.DetailedDecay.Value);
            Assert.AreEqual(config.GeneralThreshold.DefaultValue, config.GeneralThreshold.Value);
        }

        /// <summary>Threshold names are unique persistence keys.</summary>
        [Test]
        public void PropertyNames_AreUniquePersistedKeys()
        {
            var config = new ContextMatchConfig();

            var names = new HashSet<string>();
            names.Add(config.DetailedThreshold.Name);
            names.Add(config.DetailedDecay.Name);
            names.Add(config.GeneralThreshold.Name);
            Assert.AreEqual(3, names.Count);
            Assert.IsNotEmpty(config.DetailedThreshold.Tooltip);
        }
    }
}
