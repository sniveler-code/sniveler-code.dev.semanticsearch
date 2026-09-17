using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using Unity.InferenceEngine.Tokenization;
using UnityEngine;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding.Exceptions;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding.MiniLM
{
    /// <summary>MiniLM/BERT processor: tokenizes text and runs the Sentis worker.</summary>
    public sealed class MiniProcessor : IEmbeddingProcessor
    {
        private Tokenizer _tokenizer;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public bool IsCompleted => _tokenizer != null;
        /// <summary>True for models with input_ids and attention_mask inputs.</summary>
        public bool IsAssignableFrom(Model model)
        {
            bool hasInputIds = false;
            bool hasAttentionMask = false;

            foreach (var input in model.inputs)
            {
                if (input.name == "input_ids") hasInputIds = true;
                if (input.name == "attention_mask") hasAttentionMask = true;
            }

            return hasInputIds && hasAttentionMask;
        }

        /// <summary>Builds the tokenizer from a tokenizer.json asset, using the given max length.</summary>
        public void SetTokenizer(TextAsset asset, int maxLength)
        {
            if (asset == null)
            {
                _tokenizer = null;
                throw new TokenizerException("Tokenizer asset is null");
            }

            try
            {
                _tokenizer = MiniTokenizer.Get(asset.text, maxLength);
            }
            catch (Exception e)
            {
                throw new TokenizerException(e.Message);
            }
        }

        /// <summary>Encodes texts in a batch and returns sentence embeddings.</summary>
        public async Task<float[][]> GetVectorsAsync(Worker worker, string[] texts, int length, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                int batchSize = texts.Length;

                List<IEncoding> encodings = texts.Select(t => _tokenizer.Encode(t)).ToList();
                using var idsTensor = new Tensor<int>(new TensorShape(batchSize, length));
                using var maskTensor = new Tensor<int>(new TensorShape(batchSize, length));
                using var typeTensor = new Tensor<int>(new TensorShape(batchSize, length));

                for (int b = 0; b < batchSize; b++)
                {
                    FillTensorRow(idsTensor, encodings[b].GetIds(), b, length);
                    FillTensorRow(maskTensor, encodings[b].GetAttentionMask(), b, length);
                    FillTensorRow(typeTensor, encodings[b].GetTypeIds(), b, length);
                }

                if (ct.IsCancellationRequested) return null;
                // Worker.Schedule is fully synchronous in Sentis (executes every layer, then
                // flushes the GPU command buffer) — inference is complete when it returns
                // (REVIEW M6, verified against Sentis 2.6.1 source).
                worker.Schedule(idsTensor, maskTensor, typeTensor);
                // Yield back to the editor's main loop so the UI repaints between batches
                // during multi-batch indexing (intentional — do not remove).
                await Task.Yield();

                var gpuOutput = (Tensor<float>) worker.PeekOutput("sentence_embedding");
                float[][] results = new float[batchSize][];
                float[] rawData = gpuOutput.DownloadToArray();

                int vectorSize = rawData.Length / batchSize;
                for (int i = 0; i < batchSize; i++)
                {
                    float[] vector = new float[vectorSize];
                    Array.Copy(rawData, i * vectorSize, vector, 0, vectorSize);
                    // Model contract (REVIEW B3): score thresholds assume unit-length vectors.
                    // The bundled MiniLM already normalizes inside the graph (idempotent here);
                    // this keeps the invariant for any user-assigned model.
                    results[i] = vector.NormalizeInPlace();
                }

                return results;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>Copies token ids into a padded tensor row.</summary>
        private static void FillTensorRow(Tensor<int> tensor, IReadOnlyList<int> data, int row, int maxLength)
        {
            int startIdx = row * maxLength;
            for (int i = 0; i < maxLength; i++)
            {
                tensor[startIdx + i] = i < data.Count ? data[i] : 0;
            }
        }

        /// <summary>
        /// Runs one short probe inference and returns the L2 norm of the RAW model output
        /// (before defensive normalization). ≈1 means the model normalizes internally. Returns -1
        /// on failure (advisory check — callers must tolerate it).
        /// </summary>
        public async Task<float> ProbeOutputNormAsync(Worker worker, CancellationToken ct = default)
        {
            if (_tokenizer == null) return -1f;

            await _semaphore.WaitAsync(ct);
            try
            {
                const int probeLength = 16;
                var encoding = _tokenizer.Encode("probe");
                using var idsTensor = new Tensor<int>(new TensorShape(1, probeLength));
                using var maskTensor = new Tensor<int>(new TensorShape(1, probeLength));
                using var typeTensor = new Tensor<int>(new TensorShape(1, probeLength));

                FillTensorRow(idsTensor, encoding.GetIds(), 0, probeLength);
                FillTensorRow(maskTensor, encoding.GetAttentionMask(), 0, probeLength);
                FillTensorRow(typeTensor, encoding.GetTypeIds(), 0, probeLength);

                // Schedule is synchronous (REVIEW M6); the yield keeps the UI responsive.
                worker.Schedule(idsTensor, maskTensor, typeTensor);
                await Task.Yield();

                var output = (Tensor<float>) worker.PeekOutput("sentence_embedding");
                double sum = 0;
                foreach (float value in output.DownloadToArray())
                    sum += (double) value * value;

                return (float) Math.Sqrt(sum);
            }
            catch (Exception)
            {
                return -1f;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>Drops the tokenizer.</summary>
        public void Dispose()
        {
            _tokenizer = null;
        }
    }
}
