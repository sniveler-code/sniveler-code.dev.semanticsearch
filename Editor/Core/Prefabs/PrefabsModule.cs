using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Prefabs
{
    /// <summary>Prefabs tab: scan, index and database status.</summary>
    public sealed class PrefabsModule : IDisposable
    {
        private const string _errorKey = "e_database";
        private const string _defaultMessage = "Click check for scan project for new or modified assets";
        private const string _checkMessage = "Database is empty. Use 'Check' button to find all prefabs.";
        private const string _indexMessage = "Prefabs found but not indexed. Use 'Index' button to process them.";

        private readonly PrefabsView _view;
        private readonly PrefabsModel _model;
        private readonly IPrefabsFacade _facade;
        private readonly DbInfo _dbInfo = new();
        private bool _isBusy;

        private AssetStorageType moduleType => AssetStorageType.Prefabs;

        public VisualElement Content => _view.Content;

        /// <summary>Creates the view and registers the result view.</summary>
        public PrefabsModule(IMiniContainer container, IPrefabsFacade facade)
        {
            _facade = facade;
            _dbInfo.Check(_facade.Storage, moduleType);

            _model = new PrefabsModel
            {
                PropertyProvider = _facade.PropertyFactory.CreateProvider("PrefabsTransformerModule"),
                ButtonIndexEnabled = !_dbInfo.IsEqual,
                Transformers = _facade.Transformers,
                OnCheck = OnCheckHandle,
                OnIndex = OnIndexHandle,
                Property = new DropdownProperty
                {
                    Items = _facade.Transformers.Select(t => t.ModuleName).ToList(),
                    Name = "Transformer"
                }
            };

            _view = new PrefabsView(_model);
            container.Bind<PrefabsResultView>();
            _facade.Status.AddMessageListener(EmbeddingModule.ErrorKey, OnMessageProcessed);
            UpdateStatusState();
        }

        /// <summary>Refreshes counters and status messages.</summary>
        private void UpdateStatusState()
        {
            _facade.Status.UpdateDatabase(_dbInfo.Total, _dbInfo.Indexed);
            _view.ToggleMessage(_defaultMessage);
            if (_dbInfo.Total == 0)
            {
                _view.ToggleMessage(_checkMessage, true);
                _facade.Status.RegisterMessage(_errorKey, _checkMessage,
                    () => _facade.TabsModule.Switch(Content));
            }
            else if (_dbInfo.Indexed == 0)
            {
                _view.ToggleMessage(_indexMessage, true);
                _facade.Status.RegisterMessage(_errorKey, _indexMessage,
                    () => _facade.TabsModule.Switch(Content));
            }
            else _facade.Status.UnregisterMessage(_errorKey);
        }

        /// <summary>Toggles the index button from the embedding state.</summary>
        private void OnMessageProcessed(bool value) => _view.UpdateIndexButton(!value);

        /// <summary>Unsubscribes and releases the view.</summary>
        public void Dispose()
        {
            _facade.Status.UnregisterMessage(_errorKey);
            _facade.Status.RemoveMessageListener(EmbeddingModule.ErrorKey, OnMessageProcessed);
            _view.Dispose();
        }

        /// <summary>Indexes checked assets with the selected transformer.</summary>
        private async void OnIndexHandle()
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var assets = _facade.Storage.GetIndexes(moduleType, AssetStorageStatus.Checked);
                var transformer = _model.Transformers.FirstOrDefault(t =>
                    t.ModuleName == _model.Property.Value);

                await transformer?.ProcessAsync(assets, (progress, title)
                    => _facade.Status.Progress(progress, title))!;

                UpdateCompleteStatus($"Indexed: {assets.Length} assets");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _facade.Status.RegisterMessage(_errorKey, "Indexing failed. Check Console.");
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>Scans the project for new or changed prefabs.</summary>
        private async void OnCheckHandle()
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var result = await UpdateAssetsAsync((path, progress) =>
                    _facade.Status.Progress(progress, System.IO.Path.GetFileName(path)));
                UpdateCompleteStatus($"Checked: {result.outdated} assets");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _facade.Status.RegisterMessage(_errorKey, "Check failed. Check Console.");
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>Refreshes database info after a run.</summary>
        private void UpdateCompleteStatus(string title)
        {
            _dbInfo.Check(_facade.Storage, moduleType);
            _facade.Status.Progress(100, title);
            UpdateStatusState();
        }

        /// <summary>Removes stale rows and registers new assets.</summary>
        private async Task<(int removed, int outdated)> UpdateAssetsAsync(Action<string, float> callback = null)
        {
            Dictionary<string, AssetsTable> indexes = _facade.Storage.GetIndexes(moduleType);

            var removedEnumerator = from guid in indexes.Keys
                let path = AssetDatabase.GUIDToAssetPath(guid)
                where string.IsNullOrEmpty(path) || !System.IO.File.Exists(System.IO.Path.GetFullPath(path))
                select guid;
            int removedCount = _facade.Storage.ClearIndexes(new HashSet<string>(removedEnumerator));

            string[] filterGuids = AssetDatabase.FindAssets(moduleType.ToAssetFilter(),
                new[] {"Assets"});

            var added = new List<AssetsTable>();
            int total = filterGuids.Length;
            for (int i = 0; i < total; i++)
            {
                string guid = filterGuids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string currentHash = AssetDatabase.GetAssetDependencyHash(path).ToString();
                if (indexes.TryGetValue(guid, out AssetsTable asset))
                {
                    if (asset.Hash == currentHash)
                    {
                        continue;
                    }
                }

                added.Add(new AssetsTable
                {
                    Guid = guid,
                    Path = path,
                    Hash = currentHash,
                    StorageType = AssetStorageType.Prefabs,
                    Status = AssetStorageStatus.Checked
                });

                // REVIEW M8: total == 1 would divide by zero (NaN in the progress bar);
                // a single scanned asset is by definition the last one -> 100%.
                callback?.Invoke(path, total <= 1 ? 100f : i * 100f / (total - 1));

                if (i % 10 == 0)
                {
                    await Task.Yield();
                }
            }

            _facade.Storage.SetIndexes(added.ToArray());
            return (removedCount, added.Count);
        }

        /// <summary>Cached total and indexed counters.</summary>
        private sealed class DbInfo
        {
            public int Total;
            public int Indexed;
            public bool IsEqual => Total == Indexed;

            /// <summary>Reads the counters from storage.</summary>
            public void Check(IAssetsStorage storage, AssetStorageType type)
            {
                Total = storage.GetIndexesCount(type);
                Indexed = storage.GetIndexesCount(type, AssetStorageStatus.Indexed);
            }
        }
    }
}
