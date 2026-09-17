using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SnivelerCode.SemanticSearch.Editor.Core.Utils
{
    /// <summary>Vector helpers used by search and embedding code.</summary>
    public static class VectorMath
    {
        /// <summary>Normalises the vector to unit length in place.</summary>
        public static float[] NormalizeInPlace(this float[] vector)
        {
            int n = vector.Length;
            double sumOfSquares = 0;

            for (int i = 0; i < n; i++)
            {
                float val = vector[i];
                sumOfSquares += (double) val * val;
            }

            float magnitude = (float) Math.Sqrt(sumOfSquares);

            if (magnitude > 1e-10f)
            {
                float invMagnitude = 1.0f / magnitude;
                for (int i = 0; i < n; i++)
                {
                    vector[i] *= invMagnitude;
                }
            }
            else
            {
                Array.Clear(vector, 0, n);
            }

            return vector;
        }

        /// <summary>Dot product of two vectors, SIMD-accelerated when possible.</summary>
        public static float CosineSimilaritySimd(this float[] left, ReadOnlySpan<float> right)
        {
            if (left == null || left.Length != right.Length) return 0;

            int vSize = Vector<float>.Count;
            float dotProduct = 0;
            int i = 0;
            ref float leftRef = ref MemoryMarshal.GetReference(left.AsSpan());
            ref float rightRef = ref MemoryMarshal.GetReference(right);

            for (; i <= left.Length - vSize; i += vSize)
            {
                var va = Unsafe.ReadUnaligned<Vector<float>>(
                    ref Unsafe.As<float, byte>(ref Unsafe.Add(ref leftRef, i)));
                var vb = Unsafe.ReadUnaligned<Vector<float>>(
                    ref Unsafe.As<float, byte>(ref Unsafe.Add(ref rightRef, i)));

                dotProduct += Vector.Dot(va, vb);
            }

            for (; i < left.Length; i++)
            {
                dotProduct += left[i] * right[i];
            }

            return dotProduct;
        }

        /// <summary>Converts floats to raw bytes for storage.</summary>
        public static byte[] ToByteArray(this float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        /// <summary>Converts stored bytes back to floats.</summary>
        public static float[] ToFloatArray(this byte[] byteArray)
        {
            float[] floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }
    }
}
