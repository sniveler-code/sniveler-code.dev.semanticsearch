using System;

namespace SnivelerCode.SemanticSearch.Editor.Core.Metadata
{
    /// <summary>Marks a metadata extractor with a display name and order.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MetadataAttribute : Attribute
    {
        public string CategoryName { get; }
        public int Order { get; }

        /// <summary>Creates the attribute.</summary>
        public MetadataAttribute(string name, int order)
        {
            CategoryName = name;
            Order = order;
        }
    }
}
