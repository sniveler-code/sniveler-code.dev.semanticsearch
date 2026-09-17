using System;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor
{
    /// <summary>UI module contract that owns disposable content.</summary>
    public interface IDisposableContent : IDisposable
    {
        public VisualElement Content { get; }

    }
}
