using System;
using UnityEditor;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Persists property values through IPropertiesStorage.</summary>
    public sealed class PropertySaveVisitor : IPropertyVisitor
    {
        private readonly IPropertiesStorage _storage;
        private readonly string _category;

        /// <summary>Binds the visitor to a category and storage.</summary>
        public PropertySaveVisitor(string category, IPropertiesStorage storage)
        {
            _storage = storage;
            _category = category;
        }

        /// <summary>Saves an integer property.</summary>
        public void Visit(IntProperty property) =>
            _storage.SetProperty(_category, property.Name, property.Value.Bytes());

        /// <summary>Saves a boolean property.</summary>
        public void Visit(BoolProperty property) =>
            _storage.SetProperty(_category, property.Name, property.Value.Bytes());

        /// <summary>Saves an enum property as its integer value.</summary>
        public void Visit(EnumProperty property)
        {
            int intValue = Convert.ToInt32(property.Value);
            _storage.SetProperty(_category, property.Name, intValue.Bytes());
        }

        /// <summary>Saves a text property.</summary>
        public void Visit(StringProperty property) =>
            _storage.SetProperty(_category, property.Name, property.Value.Bytes());

        /// <summary>Saves a floating-point property.</summary>
        public void Visit(FloatProperty property) =>
            _storage.SetProperty(_category, property.Name, property.Value.Bytes());

        /// <summary>Saves a dropdown selection.</summary>
        public void Visit(DropdownProperty property) =>
            _storage.SetProperty(_category, property.Name, property.Value.Bytes());

        /// <summary>Saves an asset reference as its GUID.</summary>
        public void Visit(AssetProperty property)
        {
            string assetPath = AssetDatabase.GetAssetPath(property.Value);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            _storage.SetProperty(_category, property.Name, assetGuid.Bytes());
        }

        /// <summary>Saves a database asset reference.</summary>
        public void Visit(DatabaseProperty property) => Visit((AssetProperty)property);
    }
}
