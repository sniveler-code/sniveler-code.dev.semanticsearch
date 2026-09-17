using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Result view contract shown in the search tab.</summary>
    public interface ISearchResult
    {
        public VisualElement Content { get; }
        /// <summary>Replaces the displayed results.</summary>
        public void SetResults(List<FilterResult> items);
    }
}
