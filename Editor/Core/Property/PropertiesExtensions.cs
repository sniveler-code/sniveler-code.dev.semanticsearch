using System;
using UnityEngine;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Byte conversion helpers for property persistence.</summary>
    public static class PropertiesExtensions
    {
        /// <summary>Encodes an int as bytes.</summary>
        public static byte[] Bytes(this int value) => BitConverter.GetBytes(value);
        /// <summary>Encodes a float as bytes.</summary>
        public static byte[] Bytes(this float value) => BitConverter.GetBytes(value);
        /// <summary>Encodes a bool as bytes.</summary>
        public static byte[] Bytes(this bool value) => BitConverter.GetBytes(value);
        /// <summary>Encodes a UTF-8 string as bytes.</summary>
        public static byte[] Bytes(this string value) => System.Text.Encoding.UTF8.GetBytes(value);

        /// <summary>Decodes an int from bytes.</summary>
        public static int Int32(this byte[] value) => BitConverter.ToInt32(value, 0);
        /// <summary>Decodes a bool from bytes.</summary>
        public static bool Bool(this byte[] value) => BitConverter.ToBoolean(value, 0);
        /// <summary>Decodes a float from bytes.</summary>
        public static float Float(this byte[] value) => BitConverter.ToSingle(value, 0);
        /// <summary>Decodes a UTF-8 string from bytes.</summary>
        public static string String(this byte[] value) => System.Text.Encoding.UTF8.GetString(value);

        /// <summary>Loads a TextAsset from a stored asset path.</summary>
        public static TextAsset Asset(this string value)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(value);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }
    }
}
