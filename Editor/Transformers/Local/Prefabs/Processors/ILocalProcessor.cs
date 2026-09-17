using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Processors
{
    /// <summary>Local metadata processor contract.</summary>
    public interface ILocalProcessor
    {
        public IMetadata[] Extractors { get; }
        /// <summary>Builds the semantic description of a prefab.</summary>
        public Task<string> Process(GameObject asset);
    }
}
