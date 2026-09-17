using SnivelerCode.SemanticSearch.Editor.Core.Search;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Prefab rows with drag-and-drop support.</summary>
    public sealed class PrefabsResultView : SearchResultView
    {
        /// <summary>Creates the prefab result tab.</summary>
        public PrefabsResultView() : base("Prefabs", "SemanticSearch_FilterPrefabsItem")
        {
        }

        /// <summary>Fills a row and registers drag handling.</summary>
        protected override void BindResultItem(FilterResult item, VisualElement element)
        {
            base.BindResultItem(item, element);

            element.UnregisterCallback<MouseDownEvent>(e => OnMouseDown(e, item));
            element.RegisterCallback<MouseDownEvent>(e => OnMouseDown(e, item));
        }

        /// <summary>Starts a drag for the selected prefab.</summary>
        private void OnMouseDown(MouseDownEvent e, FilterResult item)
        {
            if (e.button == 0 && e.modifiers == EventModifiers.None)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(item.Path);
                if (asset != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { asset };
                    DragAndDrop.paths = new[] { item.Path };
                    DragAndDrop.StartDrag(item.AssetName);
                }
            }
        }

        /// <summary>Selects a prefab row.</summary>
        protected override void SelectResultItem(FilterResult selected, VisualElement element)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(selected.Path);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }
    }
}
