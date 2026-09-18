using SnivelerCode.SemanticSearch.Editor.Core.Property;
using Unity.InferenceEngine;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Persisted model settings: model, vocab, token limit and backend.</summary>
    public sealed class EmbeddingModel
    {
        public IPropertyProvider PropertyProvider { get; set; }

        public readonly AssetProperty Model = new()
        {
            Type = typeof(ModelAsset),
            Name = "model",
            Tooltip = "The ONNX embedding model. Recommended: all-MiniLM-L6-v2."
        };

        public readonly AssetProperty Vocab = new()
        {
            Type = typeof(TextAsset),
            Name = "vocab",
            Tooltip = "The vocabulary file used by the BERT tokenizer to encode text."
        };

        public readonly IntProperty MaxLength = new()
        {
            Name = "tokens",
            DefaultValue = 128,
            Value = 128,
            Range = (128, 512),
            Tooltip = "Maximum sequence length. 128 is usually enough for names. " +
                          "Increase to 256-512 if using long folder paths or descriptions."
        };

        public readonly EnumProperty Backend = new()
        {
            Name = "backend",
            DefaultValue = BackendType.GPUCompute,
            Value = BackendType.GPUCompute,
            Tooltip = "The Sentis compute backend used for inference. GPU is fastest when your " +
                      "hardware supports compute shaders; CPU is the portable fallback."
        };
    }
}
