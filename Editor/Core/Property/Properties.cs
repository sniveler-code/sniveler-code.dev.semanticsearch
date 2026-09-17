using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SnivelerCode.SemanticSearch.Editor.Core.Property
{
    /// <summary>Visitor over the concrete property kinds.</summary>
    public interface IPropertyVisitor
    {
        /// <summary>Visits an integer property.</summary>
        public void Visit(IntProperty property);
        /// <summary>Visits a boolean property.</summary>
        public void Visit(BoolProperty property);
        /// <summary>Visits an enum property.</summary>
        public void Visit(EnumProperty property);
        /// <summary>Visits a text property.</summary>
        public void Visit(StringProperty property);
        /// <summary>Visits a floating-point property.</summary>
        public void Visit(FloatProperty property);
        /// <summary>Visits a dropdown property.</summary>
        public void Visit(DropdownProperty property);
        /// <summary>Visits an asset reference property.</summary>
        public void Visit(AssetProperty property);
        /// <summary>Visits a database asset property.</summary>
        public void Visit(DatabaseProperty property);
    }

    /// <summary>Base class for all editor properties.</summary>
    public abstract class BaseProperty
    {
        public string Name;
        public string Label = "";
        public string Tooltip;

        /// <summary>Dispatches to the matching visitor method.</summary>
        public abstract void Accept(IPropertyVisitor visitor);
    }

    /// <summary>Typed property with a current value and a default.</summary>
    public abstract class Property<T> : BaseProperty
    {
        public T Value;
        public T DefaultValue;
        /// <summary>Clamps or normalises a new value.</summary>
        public virtual T Validate(T newValue) => newValue;

        /// <summary>Applies property-specific field configuration.</summary>
        public virtual void Init(BaseField<T> field)
        {
        }
    }

    /// <summary>Integer property.</summary>
    public sealed class IntProperty : Property<int>
    {
        /// <summary>Optional inclusive bounds for the value.</summary>
        public (int? min, int? max) Range { get; set; }
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);

        /// <summary>Clamps the value into the configured range.</summary>
        public override int Validate(int newValue) =>
            Math.Clamp(newValue, Range.min ?? newValue, Range.max ?? newValue);
    }

    /// <summary>Boolean property.</summary>
    public sealed class BoolProperty : Property<bool>
    {
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>Enum property.</summary>
    public sealed class EnumProperty : Property<Enum>
    {
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>Text property rendered as a dropdown.</summary>
    public sealed class DropdownProperty : Property<string>
    {
        public List<string> Items { get; set; }
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);

        /// <summary>Populates the dropdown choices.</summary>
        public override void Init(BaseField<string> field)
        {
            var originalField = (DropdownField) field;
            originalField.choices = Items;
        }
    }

    /// <summary>Free-form text property.</summary>
    public sealed class StringProperty : Property<string>
    {
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>Floating-point property.</summary>
    public sealed class FloatProperty : Property<float>
    {
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>Unity object reference property.</summary>
    public class AssetProperty : Property<Object>
    {
        public Type Type;
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);

        /// <summary>Restricts the field to database TextAssets.</summary>
        /// <summary>Restricts the object field to the property type.</summary>
        public override void Init(BaseField<Object> field) =>
            ((ObjectField) field).objectType = Type;
    }

    /// <summary>Asset property that can be re-baked from the settings panel.</summary>
    public sealed class DatabaseProperty : AssetProperty
    {
        public DatabaseType Category;
        public Func<CancellationToken, Task> BakeClick;
        /// <summary>Accepts the matching visitor.</summary>
        public override void Accept(IPropertyVisitor visitor) => visitor.Visit(this);

        /// <summary>Restricts the field to database TextAssets.</summary>
        public override void Init(BaseField<Object> field)
        {
            Name = Category.ToString();
            field.name = Name;
            field.label = Name;
            base.Init(field);
        }
    }
}
