using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Model-specific tokenizer and inference processor.</summary>
    public interface IEmbeddingProcessor: IDisposable
    {
        /// <summary>True when the processor supports the model.</summary>
        public bool IsAssignableFrom(Model model);
        public bool IsCompleted { get; }
        /// <summary>Assigns the tokenizer asset and the max sequence length (truncation + padding).</summary>
        public void SetTokenizer(TextAsset asset, int maxLength);
        /// <summary>Runs inference and returns embeddings.</summary>
        public Task<float[][]> GetVectorsAsync(Worker worker, string[] texts, int length, CancellationToken ct);
        /// <summary>
        /// Runs one short probe inference and returns the L2 norm of the RAW model output
        /// (before defensive normalization). ≈1 means the model normalizes internally, as the
        /// bundled MiniLM does. Returns -1 on failure (advisory check, callers must tolerate it).
        /// </summary>
        public Task<float> ProbeOutputNormAsync(Worker worker, CancellationToken ct = default);
    }
}
