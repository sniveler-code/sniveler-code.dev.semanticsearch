using System;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Provides UI rows for properties and persists their values.</summary>
    public interface IPropertyProvider
    {
        public event Action<BaseProperty> OnPropertyChange;
        /// <summary>Renders a property row under the given parent.</summary>
        public void Accept(BaseProperty property, VisualElement parent, bool isReset = false);
    }
}
