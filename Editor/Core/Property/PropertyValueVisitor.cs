using System;
using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Restores property values from persisted bytes.</summary>
    public sealed class PropertyValueVisitor: IPropertyVisitor
    {
        private readonly byte[] _data;
        public object Result { get; private set; }

        /// <summary>Binds the visitor to the stored payload.</summary>
        public PropertyValueVisitor(byte[] data) => _data = data;

        /// <summary>Restores an integer property.</summary>
        public void Visit(IntProperty property)
        {
            property.Value = _data.Int32();
            Result = property.Value;
        }

        /// <summary>Restores a boolean property.</summary>
        public void Visit(BoolProperty property)
        {
            property.Value = _data.Bool();
            Result = property.Value;
        }

        /// <summary>Restores an enum property.</summary>
        public void Visit(EnumProperty property)
        {
            int value = _data.Int32();
            property.Value = (Enum)Enum.ToObject(property.Value.GetType(), value);
            Result = property.Value;
        }

        /// <summary>Restores a text property.</summary>
        public void Visit(StringProperty property)
        {
            property.Value = _data.String();
            Result = property.Value;
        }

        /// <summary>Restores a floating-point property.</summary>
        public void Visit(FloatProperty property)
        {
            property.Value = _data.Float();
            Result = property.Value;
        }

        /// <summary>Restores a dropdown property.</summary>
        public void Visit(DropdownProperty property)
        {
            property.Value = _data.String();
            Result = property.Value;
        }

        /// <summary>Restores an asset reference from its GUID.</summary>
        public void Visit(AssetProperty property)
        {
            string guid = _data.String();
            property.Value = AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guid), property.Type);
            Result = property.Value;
        }

        /// <summary>Restores a database asset reference.</summary>
        public void Visit(DatabaseProperty property) => Visit((AssetProperty)property);
    }
}
