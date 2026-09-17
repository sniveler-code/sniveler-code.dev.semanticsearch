using System.Collections.Generic;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage
{
    /// <summary>Baked category database storage: keyword rows and vectors.</summary>
    public interface IMetadataStorage
    {
        /// <summary>Deletes all rows of a category and returns the count.</summary>
        public int MetadataClear(DatabaseType category);
        /// <summary>Returns a category's rows keyed by id.</summary>
        public Dictionary<string, MetadataTable> MetadataCategory(DatabaseType category);
        /// <summary>Inserts or replaces the given rows.</summary>
        public void MetadataInsert(MetadataTable[] values);

        /// <summary>
        /// Replaces all rows of a category with the given values inside a single
        /// transaction — a crash mid-bake leaves the category unchanged, never empty.
        /// </summary>
        public void MetadataReplace(DatabaseType category, MetadataTable[] values);
    }
}
