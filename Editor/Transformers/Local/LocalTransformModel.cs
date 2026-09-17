using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local
{
    /// <summary>Local transformer settings: extractor toggles, databases and thresholds.</summary>
    public sealed class LocalTransformModel
    {
        /// <summary>Extractor toggle properties.</summary>
        public IEnumerable<(BoolProperty property, IPropertyProvider provider)> Properties;
        public IPropertyProvider DatabaseProvider;
        public IPropertyProvider ContextProvider;
        public ContextMatchConfig ContextConfig;
        public Func<DatabaseProperty, CancellationToken, Task> OnBakeClickHandle;

        public List<DatabaseProperty> Databases => new()
        {
            BuildDatabase<TextAsset>(DatabaseType.General, "f7311d0eebe4e094b9d29e8b728a7f51"),
            BuildDatabase<TextAsset>(DatabaseType.Detailed, "85299634aa4eb1b4da9bbb74539441bb"),
            BuildDatabase<TextAsset>(DatabaseType.Components, "0901e36658e457542b7099dd553f51e0"),
            BuildDatabase<TextAsset>(DatabaseType.Materials, "0f06679fa0bb63040a874d8762ce8c5d")
        };

        /// <summary>Builds a database property from a fixed asset GUID.</summary>
        private DatabaseProperty BuildDatabase<TObject>(DatabaseType type, string value)
            where TObject : UnityEngine.Object
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(value);
            var assetValue = AssetDatabase.LoadAssetAtPath<TObject>(assetPath);
            return new DatabaseProperty
            {
                Category = type,
                Type = typeof(TObject),
                DefaultValue = assetValue,
                Value = assetValue,
                BakeClick = token => OnBakeClick(token, type)
            };
        }

        /// <summary>Invokes the bake handler for a database type.</summary>
        private async Task OnBakeClick(CancellationToken token, DatabaseType type)
        {
            var databaseProperty = Databases.FirstOrDefault(a => a.Category == type);
            await OnBakeClickHandle.Invoke(databaseProperty, token);
        }
    }
}
