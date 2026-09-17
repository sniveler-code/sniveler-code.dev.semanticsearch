using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Embedding service contract.</summary>
    public interface IEmbeddingModule : IDisposableContent
    {
        /// <summary>Computes embeddings for the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] texts, CancellationToken ct = default);
        public bool IsValid { get; }
        public event Action OnStatusChanged;
    }
}
