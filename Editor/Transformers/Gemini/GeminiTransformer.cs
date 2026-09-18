using System;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding.Exceptions;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Gemini
{
    /// <summary>
    /// EXPERIMENTAL: reserved for PRO versions. This transformer is intentionally
    /// non-functional in the v1.0 build — selecting it in the Prefabs toolbar
    /// reports a clear error instead of crashing the indexing pipeline.
    /// </summary>
    public sealed class GeminiTransformer : Transformer<GameObject>
    {
        public override string ModuleName => "Gemini (PRO experimental)";
        protected override int chunkSize => 1;
        public override AssetStorageType Type => AssetStorageType.Prefabs;
        public override VisualElement Content => new();

        /// <summary>Keeps the injected dependencies (unused in this build).</summary>
        public GeminiTransformer(IMetadataStorage storage, IEmbeddingModule embedding)
        {
        }

        /// <summary>Always throws: Gemini is not functional in this build.</summary>
        protected override Task<string> CollectMetadata(GameObject asset)
        {
            throw new SemanticException(
                "Gemini Transformer is experimental and reserved for PRO versions. " +
                "Select 'Local Transformer' to index prefabs.");
        }
    }
}
