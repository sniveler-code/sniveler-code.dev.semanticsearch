using System;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Transformers;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Prefabs tab state: transformer choice, buttons and message.</summary>
    public sealed class PrefabsModel
    {
        public bool ButtonIndexEnabled { get; set; }
        public DropdownProperty Property { get; set; }
        public IPropertyProvider PropertyProvider { get; set; }
        public Action OnCheck { get; set; }
        public Action OnIndex { get; set; }
        public ITransformer[] Transformers { get; set; }
    }
}
