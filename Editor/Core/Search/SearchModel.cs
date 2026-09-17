using System;
using System.Collections.Generic;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Search tab state: query, sensitivity and result views.</summary>
    public sealed class SearchModel
    {
        public IReadOnlyList<ISearchResult> Modules;
        public Action<string> Query;
        public Action<bool> ProcessButton;
        public bool Embedding;
        public Action<List<FilterResult>> SetResults;
        public int Sensitivity = 25;
    }
}
