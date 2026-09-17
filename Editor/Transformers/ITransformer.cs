using System;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers
{
    /// <summary>Chunked indexing pipeline over one asset kind.</summary>
    public interface ITransformer
    {
        public string ModuleName { get; }
        public VisualElement Content { get; }
        /// <summary>Indexes the given assets, reporting progress.</summary>
        public Task ProcessAsync(AssetsTable[] assets,
            Action<float, string> progress = null);
    }
}
