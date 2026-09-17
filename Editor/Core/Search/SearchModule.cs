using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Search
{
    /// <summary>Embeds queries, scores the index and distributes results.</summary>
    public sealed class SearchModule : IDisposable
    {
        private readonly SearchModuleView _searchView;
        private readonly SearchModel _model;
        private readonly ISearchFacade _facade;

        private int _generation;

        public VisualElement Content => _searchView.Content;

        /// <summary>Creates the view and subscribes to embedding status.</summary>
        public SearchModule(ISearchFacade facade, ISearchResult[] modules)
        {
            _facade = facade;
            _model = new SearchModel
            {
                Modules = modules,
                Embedding = _facade.Embedding.IsValid,
                Query = OnSearchHandle
            };

            _facade.Embedding.OnStatusChanged += OnStatusChanged;
            _searchView = new SearchModuleView(_model);
        }

        /// <summary>Enables the search button when the model is ready.</summary>
        private void OnStatusChanged() =>
            _model.ProcessButton?.Invoke(_facade.Embedding.IsValid);

        /// <summary>Unsubscribes from embedding status.</summary>
        public void Dispose() =>
            _facade.Embedding.OnStatusChanged -= OnStatusChanged;

        /// <summary>Runs a query and publishes ranked results.</summary>
        private async void OnSearchHandle(string query)
        {
            int generation = ++_generation;
            try
            {
                if (string.IsNullOrWhiteSpace(query)) return;

                float sensitivity = _model.Sensitivity * 0.01f;
                _facade.Status.Progress(0, "Generating embedding...");
                float[][] queryVectors = await _facade.Embedding.GetVectorsAsync(new[] {query});
                if (queryVectors == null || queryVectors.Length == 0) return;

                float[] queryVector = queryVectors[0];
                AssetsTable[] storageIndexes = _facade.GetAssetsIndexes();
                _facade.Status.Progress(50, "Searching database...");

                List<FilterResult> sortedResults = await Task.Run(() =>
                {
                    var results = new List<FilterResult>(storageIndexes.Length / 10);
                    for (int i = 0; i < storageIndexes.Length; i++)
                    {
                        var item = storageIndexes[i];
                        ReadOnlySpan<float> itemSpan = MemoryMarshal.Cast<byte, float>(item.Vector);

                        float finalScore = queryVector.CosineSimilaritySimd(itemSpan);

                        if (item.Metadata.Contains(query, StringComparison.OrdinalIgnoreCase))
                            finalScore += 0.2f;

                        if (finalScore > sensitivity)
                        {
                            results.Add(new FilterResult
                            {
                                AssetName = Path.GetFileNameWithoutExtension(item.Path),
                                Path = item.Path,
                                Score = finalScore,
                                StorageType = item.StorageType
                            });
                        }
                    }

                    results.Sort((a, b) => b.Score.CompareTo(a.Score));
                    return results;
                });

                _facade.Status.Progress(100, $"Results found: {sortedResults.Count}");

                if (generation != _generation) return;
                _model.SetResults(sortedResults);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                _facade.Status.Progress(0, "Search failed.");
            }
        }
    }
}
