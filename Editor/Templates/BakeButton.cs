using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Templates
{
    /// <summary>Toolbar button that runs a cancellable bake operation.</summary>
    public sealed class BakeButton : Button
    {
        private const string _bakeTitle = "Bake";
        private const string _stopTitle = "Stop";

        private CancellationTokenSource _cts;
        private bool _isBaking;
        /// <summary>Creates the button and binds its click handler.</summary>
        public BakeButton(Func<CancellationToken, Task> bake)
        {
            text = _bakeTitle;
            clicked += async () =>
            {
                _cts?.Cancel();
                if (_isBaking)
                {
                    _isBaking = false;
                    text = _bakeTitle;
                    return;
                }

                _isBaking = true;
                text = _stopTitle;
                _cts = new CancellationTokenSource();
                await bake(_cts.Token);

                _isBaking = false;
                text = _bakeTitle;
            };
        }
    }
}
