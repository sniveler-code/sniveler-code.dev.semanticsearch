namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Metadata
{
    /// <summary>Weighted metadata word with its embedding.</summary>
    public sealed class MetadataWord
    {
        public string Word;
        public float Ratio;
        public float[] Vector;

        /// <summary>Creates the word with an optional weight.</summary>
        public MetadataWord(string word, float ratio = 1f)
        {
            Word = word;
            Ratio = ratio;
        }
    }
}
