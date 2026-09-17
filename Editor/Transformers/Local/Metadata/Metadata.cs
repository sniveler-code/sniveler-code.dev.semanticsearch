using System.Threading.Tasks;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Base class for semantic metadata extractors.</summary>
    public abstract class Metadata<T> : IMetadata
    {
        protected readonly IMetadataFacade _metadata;

        public virtual string Name => "Undefined";
        public virtual string Info => "Undefined";
        /// <summary>Extracts metadata from one asset.</summary>
        public abstract Task<IMetadataResult> ProcessAsync(T asset);
        /// <summary>Stores the metadata facade.</summary>
        protected Metadata(IMetadataFacade metadata) => _metadata = metadata;
    }
}
