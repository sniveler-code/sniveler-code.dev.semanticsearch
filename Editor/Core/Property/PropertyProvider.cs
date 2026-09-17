using System;
using UnityEngine.UIElements;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Creates property providers bound to a settings category.</summary>
    public sealed class PropertyFactory
    {
        private readonly IPropertiesStorage _storage;

        /// <summary>Stores the persistence backend.</summary>
        public PropertyFactory(IPropertiesStorage storage) => _storage = storage;

        /// <summary>Creates a provider for a category.</summary>
        public IPropertyProvider CreateProvider(string category) =>
            new PropertyProvider(category, _storage);
    }

    /// <summary>Builds property rows and persists their values for one category.</summary>
    public sealed class PropertyProvider : IPropertyProvider
    {
        private readonly UIPropVisitor _uiVisitor;
        public event Action<BaseProperty> OnPropertyChange;

        /// <summary>Loads stored values and prepares the UI visitor.</summary>
        public PropertyProvider(string category, IPropertiesStorage storage)
        {
            var categoryValues = storage.GetProperties(category);
            var saveVisitor = new PropertySaveVisitor(category, storage);

            _uiVisitor = new UIPropVisitor(categoryValues, prop =>
            {
                prop.Accept(saveVisitor);
                OnPropertyChange?.Invoke(prop);
            });
        }

        /// <summary>Adds a property row and raises change notifications.</summary>
        public void Accept(BaseProperty prop, VisualElement element, bool isReset = false)
        {
            prop.Accept(_uiVisitor);
            _uiVisitor.Element.Q<Button>().style.display =
                isReset ? DisplayStyle.Flex : DisplayStyle.None;
            element.Add(_uiVisitor.Element);
            OnPropertyChange?.Invoke(prop);
        }
    }
}
