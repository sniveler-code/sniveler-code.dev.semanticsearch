using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata
{
    /// <summary>Produces the identity words of a prefab.</summary>
    public sealed class IdentityMetadata : Metadata<GameObject>
    {
        public override string Name => "Identity";
        public override string Info => "Primary identity with semantic reinforcement.";

        /// <summary>Stores the metadata facade.</summary>
        public IdentityMetadata(IMetadataFacade facade) : base(facade)
        {
        }

        /// <summary>Returns the cleaned name parts.</summary>
        public override Task<IMetadataResult> ProcessAsync(GameObject go)
        {
            string cleanName = NamingUtility.Clean(go.name);
            return Task.FromResult(IMetadataResult.FromArray(cleanName.SplitWords()));
        }
    }
}
