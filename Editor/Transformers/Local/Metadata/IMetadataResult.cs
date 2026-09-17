namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Result of a metadata extractor.</summary>
    public interface IMetadataResult
    {

        /// <summary>Extractor produced no metadata.</summary>
        public class Empty : IMetadataResult
        {
        }

        /// <summary>Extractor produced a list of words.</summary>
        public sealed class Array : Empty
        {
            public string[] Value;
            /// <summary>Wraps the produced words.</summary>
            public Array(string[] value) => Value = value;
        }

        /// <summary>Creates an empty result.</summary>
        public static IMetadataResult FromEmpty() => new Empty();
        /// <summary>Creates a word-array result.</summary>
        public static IMetadataResult FromArray(string[] value) => new Array(value);
    }
}
