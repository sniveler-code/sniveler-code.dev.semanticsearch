using System;
using System.Linq;
using NUnit.Framework;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;

namespace SnivelerCode.SemanticSearch.Tests.Editor
{
    /// <summary>Tests the vector math used by search and embedding.</summary>
    public class VectorMathTests
    {
        /// <summary>Reference dot product for parity checks.</summary>
        private static float NaiveDot(float[] a, float[] b)
        {
            float sum = 0;
            for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
            return sum;
        }

        /// <summary>Creates a random vector for parity checks.</summary>
        private static float[] RandomVector(Random rnd, int length)
        {
            var v = new float[length];
            for (int i = 0; i < length; i++) v[i] = (float) (rnd.NextDouble() * 2 - 1);
            return v;
        }

        /// <summary>A vector dotted with itself equals one.</summary>
        [Test]
        public void SameVector_DotProductIsOne()
        {
            var v = new float[384];
            v[0] = 1f;
            Assert.AreEqual(1f, v.CosineSimilaritySimd(v), 1e-4f);
        }

        /// <summary>Orthogonal vectors score zero.</summary>
        [Test]
        public void OrthogonalVectors_DotProductIsZero()
        {
            var a = new float[384];
            var b = new float[384];
            a[0] = 1f;
            b[1] = 1f;
            Assert.AreEqual(0f, a.CosineSimilaritySimd(b), 1e-6f);
        }

        /// <summary>Opposite vectors score minus one.</summary>
        [Test]
        public void OppositeVector_DotProductIsMinusOne()
        {
            var a = new float[384];
            var b = new float[384];
            a[0] = 1f;
            b[0] = -1f;
            Assert.AreEqual(-1f, a.CosineSimilaritySimd(b), 1e-4f);
        }

        /// <summary>Length mismatch returns zero.</summary>
        [Test]
        public void MismatchedLengths_ReturnsZero()
        {
            var a = Enumerable.Repeat(1f, 16).ToArray();
            var b = Enumerable.Repeat(1f, 32).ToArray();
            Assert.AreEqual(0f, a.CosineSimilaritySimd(b));
        }

        /// <summary>Empty vectors do not throw.</summary>
        [Test]
        public void EmptyArrays_DoNotThrow()
        {
            var empty = Array.Empty<float>();
            Assert.DoesNotThrow(() => empty.CosineSimilaritySimd(empty));
        }

        /// <summary>The SIMD result matches the naive dot product.</summary>
        [TestCase(7, 384)]
        [TestCase(17, 384)]
        [TestCase(64, 384)]
        [TestCase(128, 1000)]
        public void Simd_MatchesNaiveDot(int seed, int length)
        {
            var rnd = new Random(seed);
            float[] a = RandomVector(rnd, length);
            float[] b = RandomVector(rnd, length);

            Assert.AreEqual(NaiveDot(a, b), a.CosineSimilaritySimd(b), 1e-3f);
        }

        /// <summary>Float byte round-trip preserves values.</summary>
        [Test]
        public void ByteRoundTrip_PreservesValues()
        {
            var v = Enumerable.Range(0, 100).Select(i => i * 0.5f).ToArray();
            CollectionAssert.AreEqual(v, v.ToByteArray().ToFloatArray());
        }

        /// <summary>Normalisation produces a unit vector.</summary>
        [Test]
        public void NormalizeInPlace_ProducesUnitLength()
        {
            var v = new[] {3f, 4f};
            v.NormalizeInPlace();

            double length = Math.Sqrt((double) v[0] * v[0] + v[1] * v[1]);
            Assert.AreEqual(1.0, length, 1e-5);
            Assert.AreEqual(0.6f, v[0], 1e-4f);
            Assert.AreEqual(0.8f, v[1], 1e-4f);
        }

        /// <summary>Zero vectors normalise safely.</summary>
        [Test]
        public void NormalizeInPlace_ZeroVector_DoesNotCrash()
        {
            var v = new float[8];
            Assert.DoesNotThrow(() => v.NormalizeInPlace());
        }

        /// <summary>
        /// Normalization is idempotent for unit vectors — the defensive normalization in
        /// MiniProcessor.GetVectorsAsync must leave bundled-model (already normalized) output
        /// unchanged (REVIEW B3).
        /// </summary>
        [Test]
        public void NormalizeInPlace_IdempotentForUnitVectors()
        {
            var rnd = new Random(42);
            var v = RandomVector(rnd, 384).NormalizeInPlace();
            var copy = (float[]) v.Clone();

            v.NormalizeInPlace();

            CollectionAssert.AreEqual(copy, v);
        }

        /// <summary>
        /// Dot product of normalized vectors equals the true cosine similarity — the scoring
        /// contract behind the Sensitivity thresholds and the "%" display (REVIEW B3).
        /// </summary>
        [Test]
        public void NormalizeThenDot_EqualsCosineSimilarity()
        {
            var rnd = new Random(7);
            var a = RandomVector(rnd, 384);
            var b = RandomVector(rnd, 384);

            float cosine = NaiveDot(a, b) / (Norm(a) * Norm(b));
            float dot = a.NormalizeInPlace().CosineSimilaritySimd(b.NormalizeInPlace());

            Assert.AreEqual(cosine, dot, 1e-3f);
        }

        /// <summary>Euclidean norm helper.</summary>
        private static float Norm(float[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++) sum += (double) v[i] * v[i];
            return (float) Math.Sqrt(sum);
        }
    }
}