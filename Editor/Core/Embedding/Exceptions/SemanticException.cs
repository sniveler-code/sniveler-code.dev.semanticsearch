using System;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding.Exceptions
{
    /// <summary>Raised for unsupported or invalid semantic models.</summary>
    public sealed class SemanticException : Exception
    {
        /// <summary>Creates the exception with a message.</summary>
        public SemanticException(string message) : base(message)
        {
        }
    }
}
