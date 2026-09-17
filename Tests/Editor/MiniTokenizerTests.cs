using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding.MiniLM;
using Unity.InferenceEngine.Tokenization;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Tests the BERT tokenizer built from the sample config.</summary>
    public class MiniTokenizerTests
    {

        /// <summary>Path of the sample tokenizer.json.</summary>
        private static string TokenizerJsonPath => Path.Combine(
            Path.GetFullPath("Packages/sniveler-code.dev.semanticsearch"),
            "Samples~/DemoContent/Medieval/Data", "tokenizer.json");

        /// <summary>Sample tokenizer configuration text.</summary>
        private static string Json => File.ReadAllText(TokenizerJsonPath);

        /// <summary>The tokenizer builds from the sample configuration.</summary>
        [Test]
        public void BuildsTokenizer_FromPackageJson()
        {
            Assume.That(File.Exists(TokenizerJsonPath), "tokenizer.json sample is missing");
            Tokenizer tokenizer = MiniTokenizer.Get(Json, 128);
            Assert.IsNotNull(tokenizer);
        }

        /// <summary>Encoding yields non-negative token ids.</summary>
        [Test]
        public void Encode_ProducesTokenIds()
        {
            Assume.That(File.Exists(TokenizerJsonPath), "tokenizer.json sample is missing");
            Tokenizer tokenizer = MiniTokenizer.Get(Json, 128);

            IEncoding encoding = tokenizer.Encode("heavy weapon");
            Assert.IsNotNull(encoding);

            var ids = new List<int>();
            int count = encoding.GetIds(ids);

            Assert.Greater(count, 0, "encoding must contain at least CLS/SEP");
            Assert.AreEqual(encoding.Length, count);
            Assert.AreEqual(count, ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                Assert.GreaterOrEqual(ids[i], 0, $"token id at {i} must be non-negative");
            }
        }

        /// <summary>Unknown words fall back to UNK tokens.</summary>
        [Test]
        public void Encode_UnknownWords_StillProducesTokens()
        {
            Assume.That(File.Exists(TokenizerJsonPath), "tokenizer.json sample is missing");
            Tokenizer tokenizer = MiniTokenizer.Get(Json, 128);

            var ids = new List<int>();
            int count = tokenizer.Encode("zzzqqq xyzzy").GetIds(ids);

            Assert.GreaterOrEqual(count, 2);
            foreach (int id in ids)
            {
                Assert.GreaterOrEqual(id, 0);
            }
        }

        /// <summary>Attention mask length matches the encoding.</summary>
        [Test]
        public void AttentionMask_MatchesLength()
        {
            Assume.That(File.Exists(TokenizerJsonPath), "tokenizer.json sample is missing");
            Tokenizer tokenizer = MiniTokenizer.Get(Json, 128);

            IEncoding encoding = tokenizer.Encode("a much longer query about medieval equipment");
            var mask = new List<int>();
            int maskCount = encoding.GetAttentionMask(mask);

            Assert.Greater(maskCount, 0);
            Assert.AreEqual(encoding.Length, maskCount);
        }

        /// <summary>maxLength controls truncation and padding (REVIEW M5).</summary>
        [Test]
        public void Encode_RespectsMaxLength()
        {
            Assume.That(File.Exists(TokenizerJsonPath), "tokenizer.json sample is missing");

            // 300 OOV words -> far more tokens than either limit.
            string longText = string.Join(" ", Enumerable.Range(0, 300).Select(i => "word" + i));

            int smallCount = MiniTokenizer.Get(Json, 32).Encode(longText).GetIds(new List<int>());
            int largeCount = MiniTokenizer.Get(Json, 256).Encode(longText).GetIds(new List<int>());

            Assert.AreEqual(32, smallCount, "truncation must cap the sequence at maxLength");
            Assert.AreEqual(256, largeCount, "padding must extend the sequence to maxLength");
            Assert.Less(smallCount, largeCount, "longer maxLength must keep more tokens");
        }
    }
}