using System;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding.Exceptions
{
    /// <summary>Raised when the tokenizer asset cannot be used.</summary>
    public sealed class TokenizerException : Exception
    {
        /// <summary>Creates the exception with a message.</summary>
        public TokenizerException(string message) : base(message)
        {
        }
    }
}
