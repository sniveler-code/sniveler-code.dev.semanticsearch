using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using SnivelerCode.SemanticSearch.Editor.Core.Storage;
using SnivelerCode.SemanticSearch.Editor.Core.Utils;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Metadata;
using UnityEngine.UIElements;
using SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs.Processors;
using UnityEditor;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local.Prefabs
{
    /// <summary>Indexes prefabs with the local metadata extractors.</summary>
    public sealed class LocalTransformer : Transformer<GameObject>
    {
        private readonly ILocalProcessor _processor;
        private readonly LocalTransformView _moduleView;
        private readonly ILocalTransformerFacade _localFacade;
        private readonly LocalTransformModel _model;
        private bool _unloadedThisRun;

        protected override int chunkSize => 10;

        public override string ModuleName => "Local Transformer";
        public override AssetStorageType Type => AssetStorageType.Prefabs;
        public override VisualElement Content => _moduleView.Content;

        /// <summary>Builds the settings model and view.</summary>
        public LocalTransformer(ILocalTransformerFacade localFacade,
            ILocalProcessor processor, PropertyFactory propertyFactory, ContextMatchConfig contextConfig)
        {
            _localFacade = localFacade;
            _processor = processor;

            _model = new LocalTransformModel
            {
                Properties = _processor.Extractors.Select(a => (new BoolProperty
                {
                    Name = "enable",
                    Label = a.Name,
                    DefaultValue = true,
                    Value = true,
                    Tooltip = a.Info
                }, propertyFactory.CreateProvider(a.Name))),
                DatabaseProvider = propertyFactory.CreateProvider(ModuleName),
                ContextProvider = propertyFactory.CreateProvider("ContextMatch"),
                ContextConfig = contextConfig,
                OnBakeClickHandle = OnBakeClickHandle
            };

            _moduleView = new LocalTransformView(_model);
        }

        /// <summary>Bakes stale databases, then runs the pipeline.</summary>
        public override async Task ProcessAsync(AssetsTable[] assets, Action<float, string> progress = null)
        {

            _unloadedThisRun = false;
            await AutoBakeDatabasesAsync(progress);
            await base.ProcessAsync(assets, progress);
        }

        /// <summary>Bakes databases whose content changed or are missing.</summary>
        private async Task AutoBakeDatabasesAsync(Action<float, string> progress)
        {
            using var cts = new CancellationTokenSource();
            foreach (var dbProperty in _model.Databases)
            {
                if (cts.IsCancellationRequested) return;

                var textAsset = (TextAsset) dbProperty.Value;
                if (textAsset == null) continue;

                string sourceHash = NamingUtility.HashFnv1a(textAsset.text);
                var existingData = _localFacade.GetCategoryKeywords(dbProperty.Category);
                string storedHash = _localFacade.GetDatabaseContentHash(dbProperty.Category);
                if (storedHash == sourceHash && existingData != null && existingData.Count != 0)
                {
                    continue;
                }

                progress?.Invoke(0, $"Auto-baking {dbProperty.Category} database...");
                await BakeDatabaseInternal(dbProperty, cts.Token, progress);
            }
        }

        /// <summary>Builds the description through the local processor.</summary>
        protected override Task<string> CollectMetadata(GameObject asset) => _processor.Process(asset);

        /// <summary>Embeds and stores one chunk of assets.</summary>
        protected override async Task ProcessAssets(ReadOnlyMemory<AssetsTable> chunk)
        {
            await Task.Yield();
            if (!_unloadedThisRun)
            {
                await Resources.UnloadUnusedAssets();
                _unloadedThisRun = true;
            }

            string[] batch = new string[chunk.Length];
            for (int i = 0; i < chunk.Length; i++)
            {
                batch[i] = chunk.Span[i].Metadata;
            }

            float[][] result = await _localFacade.GetVectorsAsync(batch);
            if (result != null)
            {
                for (int i = 0; i < chunk.Length; i++)
                {
                    chunk.Span[i].Vector = result[i].ToByteArray();
                    chunk.Span[i].Status = AssetStorageStatus.Indexed;
                }
            }

            _localFacade.SetIndexes(chunk.ToArray());
        }

        /// <summary>Runs a manual database bake.</summary>
        private Task OnBakeClickHandle(DatabaseProperty property, CancellationToken token)
        {
            return BakeDatabaseInternal(property, token, (p, title) =>
                _localFacade.Status.Progress(p, title));
        }

        /// <summary>Parses, embeds and stores a category database.</summary>
        private async Task BakeDatabaseInternal(DatabaseProperty property, CancellationToken token,
            Action<float, string> progress)
        {
            var textAsset = (TextAsset) property.Value;
            if (textAsset == null) return;

            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var library = JObject.Parse(textAsset.text);
            JArray categories = (JArray) library["categories"]!;

            var metadata = categories.Select(category => new MetadataTable
            {
                Id = (string) category["id"],
                Category = property.Category,
                Parent = category["parent"]?.ToString(),
                Keys = category["keys"]?.ToString(),
                Values = category["values"]?.ToString()
            }).ToArray();

            var examples = new List<string>();
            foreach (JToken category in categories)
            {
                examples.Add(ReadExamplesText(category));
            }

            string[] bakeTexts = new string[metadata.Length];
            for (int i = 0; i < metadata.Length; i++)
            {
                bakeTexts[i] = $"{metadata[i].Keys} {metadata[i].Values} {examples[i]}".CleanForAI();
            }

            ReadOnlyMemory<string> memory = bakeTexts.AsMemory();

            for (int i = 0; i < memory.Length; i += 10)
            {
                int length = Math.Min(10, memory.Length - i);
                ReadOnlyMemory<string> chunk = memory.Slice(i, length);

                float[][] result = await _localFacade.GetVectorsAsync(chunk.ToArray(), token);

                if (token.IsCancellationRequested) return;

                for (int j = 0; j < result.Length; j++)
                {
                    int index = i + j;
                    metadata[index].Vector = result[j].ToByteArray();

                    progress?.Invoke(index * 100f / memory.Length, $"Baking: {metadata[index].Keys}");
                }
            }

            _localFacade.DatabaseInsert(property.Category, metadata.ToArray());

            _localFacade.SetDatabaseContentHash(property.Category,
                NamingUtility.HashFnv1a(textAsset.text));
        }

        /// <summary>
        /// Reads the optional "examples" field of a category (array of asset names,
        /// or a single comma/space-joined string). Returns "" when absent.
        /// </summary>
        private static string ReadExamplesText(JToken category)
        {
            JToken examples = category["examples"];
            if (examples == null) return "";
            if (examples is JArray array)
            {
                return array.Select(a => a.ToString())
                    .Where(a => a.Length > 0)
                    .JoinWords(", ");
            }

            string single = examples.ToString();
            return single == null ? "" : single;
        }
    }
}
