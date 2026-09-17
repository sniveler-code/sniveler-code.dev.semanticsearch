using System.Collections.Generic;

namespace SnivelerCode.SemanticSearch.Editor.Core.Storage
{
    /// <summary>Persistent key/value storage for editor settings.</summary>
    public interface IPropertiesStorage
    {
        /// <summary>Stores a value under a category and name.</summary>
        public void SetProperty(string category, string name, byte[] data);
        /// <summary>Returns all values of a category.</summary>
        public Dictionary<string, byte[]> GetProperties(string category);
    }
}
