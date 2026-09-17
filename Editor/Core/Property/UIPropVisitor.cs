using System;
using System.Collections.Generic;
using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Builds UI Toolkit fields for properties and reports edits.</summary>
    public sealed class UIPropVisitor : IPropertyVisitor
    {
        private readonly Action<BaseProperty> _onChanged;
        private readonly Dictionary<string, byte[]> _values;
        private readonly VisualTreeAsset _buttonTemplate;

        public VisualElement Element { get; private set; }

        /// <summary>Binds stored values and the change callback.</summary>
        public UIPropVisitor(Dictionary<string, byte[]> values, Action<BaseProperty> onChanged)
        {
            _onChanged = onChanged;
            _values = values;
            _buttonTemplate = TemplateProvider.LoadByName("SemanticSearch_ButtonReset");
        }

        /// <summary>Creates the field for an integer property.</summary>
        public void Visit(IntProperty property) =>
            CreateField<IntegerField, int>(property, property.DefaultValue);

        /// <summary>Creates the field for a boolean property.</summary>
        public void Visit(BoolProperty property) =>
            CreateField<Toggle, bool>(property, property.DefaultValue);

        /// <summary>Creates the field for an enum property.</summary>
        public void Visit(EnumProperty property)
        {
            var field = CreateField<EnumField, Enum>(property, property.DefaultValue);
            field.Init(property.Value);
        }

        /// <summary>Creates the field for a text property.</summary>
        public void Visit(StringProperty property) =>
            CreateField<TextField, string>(property, property.DefaultValue);

        /// <summary>Creates the field for a floating-point property.</summary>
        public void Visit(FloatProperty property) =>
            CreateField<FloatField, float>(property, property.DefaultValue);

        /// <summary>Creates the field for a dropdown property.</summary>
        public void Visit(DropdownProperty property) =>
            CreateField<DropdownField, string>(property, property.DefaultValue);

        /// <summary>Creates the field for an asset reference.</summary>
        public void Visit(AssetProperty property) =>
            CreateField<ObjectField, UnityEngine.Object>(property, property.DefaultValue);

        /// <summary>Creates the field for a database asset.</summary>
        public void Visit(DatabaseProperty property) =>
            CreateField<ObjectField, UnityEngine.Object>(property, property.DefaultValue);

        /// <summary>Creates a labelled field with a reset button and change handling.</summary>
        private TField CreateField<TField, TValue>(Property<TValue> property, TValue defaultValue)
            where TField : BaseField<TValue>, new()
        {
            var field = new TField
            {
                name = property.Name,
                label = property.Label,
                tooltip = property.Tooltip,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    height = 20,
                    unityTextAlign = UnityEngine.TextAnchor.MiddleRight
                }
            };

            property.Init(field);

            if (_values.TryGetValue(property.Name, out byte[] bytes))
            {
                var valueVisitor = new PropertyValueVisitor(bytes);
                property.Accept(valueVisitor);
                field.value = (TValue) valueVisitor.Result;
            }
            else
            {
                field.value = defaultValue;
            }

            field.RegisterCallback<ChangeEvent<TValue>>(evt =>
            {
                property.Value = property.Validate(evt.newValue);
                field.SetValueWithoutNotify(property.Value);
                _onChanged?.Invoke(property);
            });

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    alignItems = Align.Center,
                    flexGrow = 1
                }
            };

            var resetBtn = _buttonTemplate.Instantiate().Q<Button>();
            resetBtn.style.display = DisplayStyle.None;
            resetBtn.tooltip = $"Reset to: {defaultValue}";
            resetBtn.clicked += () => field.value = defaultValue;

            row.Add(field);
            row.Add(resetBtn);

            Element = row;

            return field;
        }
    }
}
