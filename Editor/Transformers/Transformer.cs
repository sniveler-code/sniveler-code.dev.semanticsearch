using System;
using System.IO;
using UnityEditor;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers
{
    /// <summary>Base chunked transformer: collects metadata, then stores vectors.</summary>
    public abstract class Transformer<T> : ITransformer where T : UnityEngine.Object
    {
        public abstract string ModuleName { get; }
        public abstract AssetStorageType Type { get; }
        public abstract VisualElement Content { get; }

        protected abstract int chunkSize { get; }

        /// <summary>Runs the pipeline in chunks with progress reporting.</summary>
        public virtual async Task ProcessAsync(AssetsTable[] assets, Action<float, string> progress = null)
        {
            int total = assets.Length;
            ReadOnlyMemory<AssetsTable> memory = assets.AsMemory();
            for (int i = 0; i < memory.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, memory.Length - i);
                ReadOnlyMemory<AssetsTable> chunk = memory.Slice(i, length);
                for (int j = 0; j < chunk.Length; j++)
                {
                    var asset = chunk.Span[j];
                    var assetObject = AssetDatabase.LoadAssetAtPath<T>(asset.Path);
                    asset.Metadata = await CollectMetadata(assetObject);
                    progress?.Invoke(i * 100f / total, Path.GetFileName(asset.Path));
                }

                await ProcessAssets(chunk);
            }
        }

        /// <summary>Persists vectors for one processed chunk.</summary>
        protected virtual Task ProcessAssets(ReadOnlyMemory<AssetsTable> chunk) => Task.CompletedTask;
        /// <summary>Builds the semantic description of one asset.</summary>
        protected abstract Task<string> CollectMetadata(T asset);
    }
}
