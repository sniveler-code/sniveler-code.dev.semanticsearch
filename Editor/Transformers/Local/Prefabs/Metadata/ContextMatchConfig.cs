using SnivelerCode.SemanticSearch.Editor.Core.Property;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata
{
    /// <summary>
    /// Live thresholds for context-category matching. The fields are regular
    /// properties persisted through the "ContextMatch" provider, so values are
    /// editable from the Prefabs settings panel and carry over between sessions.
    /// </summary>
    public sealed class ContextMatchConfig
    {
        /// <summary>Min similarity of the best detailed category before it is used.</summary>
        public readonly FloatProperty DetailedThreshold = new()
        {
            Name = "detailedThreshold",
            Label = "Detailed threshold",
            Tooltip = "Min similarity of the best detailed category before it is used.",
            DefaultValue = 0.3f,
            Value = 0.3f
        };

        /// <summary>Relative gap required to combine a second detailed category.</summary>
        public readonly FloatProperty DetailedDecay = new()
        {
            Name = "detailedDecay",
            Label = "Detailed decay",
            Tooltip = "Relative gap required to combine a second detailed category.",
            DefaultValue = 0.15f,
            Value = 0.15f
        };

        /// <summary>Min score the best general category needs to be used as fallback.</summary>
        public readonly FloatProperty GeneralThreshold = new()
        {
            Name = "generalThreshold",
            Label = "General threshold",
            Tooltip = "Min score the best general category needs to be used as fallback.",
            DefaultValue = 0.25f,
            Value = 0.25f
        };
    }
}
