using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Core.Embedding.Exceptions;
using SnivelerCode.SemanticSearch.Editor.Core.Property;
using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Core.Embedding
{
    /// <summary>Owns the embedding model, tokenizer and Sentis worker.</summary>
    public sealed class EmbeddingModule : IEmbeddingModule
    {
        public const string ErrorKey = "e_embedding";
        private const string NormKey = "e_embedding_norm";
        private const float NormTolerance = 0.01f;

        private Worker _worker;
        private IEmbeddingProcessor _processor;
        private readonly EmbeddingModuleView _modelView;
        private readonly EmbeddingModel _model;
        private readonly IPropertyProvider _propertyProvider;
        private readonly IEmbeddingFacade _facade;

        public VisualElement Content => _modelView.Content;
        public event Action OnStatusChanged;
        public bool IsValid => _processor?.IsCompleted ?? false;

        /// <summary>Creates the model view and subscribes to property changes.</summary>
        public EmbeddingModule(IEmbeddingFacade facade)
        {
            _facade = facade;
            _propertyProvider = facade.PropertyFactory.CreateProvider(nameof(EmbeddingModule));
            _propertyProvider.OnPropertyChange += OnPropertyChange;

            _model = new EmbeddingModel {PropertyProvider = _propertyProvider};
            _modelView = new EmbeddingModuleView(_model);
            _modelView.ToggleEnable(_model.Vocab.Name, false);

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        /// <summary>Releases native resources before a script reload.</summary>
        private void OnBeforeAssemblyReload() => Dispose();

        /// <summary>Disposes the worker, processor and subscriptions.</summary>
        public void Dispose()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            _worker?.Dispose();
            _worker = null;

            _processor?.Dispose();
            _processor = null;

            _propertyProvider.OnPropertyChange -= OnPropertyChange;
        }

        /// <summary>Reacts to model or tokenizer changes.</summary>
        private async void OnPropertyChange(BaseProperty property)
        {
            try
            {
                await Task.Yield();

                _facade.Status.UnregisterMessage("e_general");
                if (property.Equals(_model.Model))
                {
                    await OnModelChanged((AssetProperty) property);
                }
                else if (property.Equals(_model.Vocab))
                {
                    if (_processor == null) return;

                    var asset = (AssetProperty) property;
                    _processor.SetTokenizer((TextAsset) asset.Value, _model.MaxLength.Value);
                    _facade.Status.UnregisterMessage("e_general");
                    _facade.Status.UnregisterMessage(ErrorKey);
                    _modelView.ToggleError(_model.Vocab.Name, false);
                }
                else if (property.Equals(_model.MaxLength))
                {
                    // REVIEW M5: truncation/padding follow the configured length — rebuild
                    // the tokenizer on change (cheap: tokenizer.json parse only).
                    if (_processor == null || _model.Vocab.Value == null) return;
                    _processor.SetTokenizer((TextAsset) _model.Vocab.Value, _model.MaxLength.Value);
                    _facade.Status.UnregisterMessage(ErrorKey);
                }
            }
            catch (TokenizerException e)
            {
                _facade.Status.RegisterMessage(ErrorKey, e.Message,
                    () => _facade.TabsModule.Switch(Content));
                _modelView.ToggleError(_model.Vocab.Name, true);
            }
            catch (SemanticException e)
            {
                _facade.Status.RegisterMessage(ErrorKey, e.Message,
                    () => _facade.TabsModule.Switch(Content));
                _modelView.ToggleError(_model.Model.Name, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _facade.Status.RegisterMessage(ErrorKey, e.Message);
            }
            finally
            {
                OnStatusChanged?.Invoke();
            }
        }

        /// <summary>Loads the ONNX model, creates the worker and probes the output norm.</summary>
        private async Task OnModelChanged(AssetProperty property)
        {
            _worker?.Dispose();
            _worker = null;

            _processor?.Dispose();
            _processor = null;

            if (property.Value == null)
            {
                _facade.Status.RegisterMessage(ErrorKey, "Semantic model is empty",
                    () => _facade.TabsModule.Switch(Content));
                _modelView.ToggleEnable(_model.Vocab.Name, false);
                _modelView.ToggleError(_model.Model.Name, true);
                return;
            }

            _modelView.ToggleError(_model.Model.Name, false);
            var backend = (BackendType) _model.Backend.Value;

            _facade.Status.Progress(0, "Loading model...");
            Model model = ModelLoader.Load((ModelAsset) property.Value);
            _facade.Status.Progress(100, "Model loaded");

            _processor = _facade.Processors.FirstOrDefault(p => p.IsAssignableFrom(model));

            if (_processor == null)
            {
                _worker?.Dispose();
                _worker = null;
                throw new SemanticException(
                    "Unsupported model. Expected BERT-like inputs (input_ids, attention_mask).");
            }

            _worker = new Worker(model, backend);

            _facade.Status.UnregisterMessage(ErrorKey);
            _modelView.ToggleEnable(_model.Vocab.Name, true);
            _processor.SetTokenizer((TextAsset) _model.Vocab.Value, _model.MaxLength.Value);

            // Advisory contract check (REVIEW B3): scoring thresholds assume unit-length
            // embeddings. The bundled MiniLM normalizes inside the graph; warn if a
            // user-assigned model does not (vectors are normalized automatically).
            _facade.Status.UnregisterMessage(NormKey);
            float norm = await _processor.ProbeOutputNormAsync(_worker);
            if (norm > 0f && Math.Abs(norm - 1f) > NormTolerance)
            {
                _facade.Status.RegisterMessage(NormKey,
                    $"Model outputs unnormalized embeddings (‖e‖ = {norm:0.##}, expected ≈ 1). " +
                    "Vectors are normalized automatically; Sensitivity thresholds are calibrated " +
                    "for the bundled MiniLM model and may need adjustment.",
                    () => _facade.TabsModule.Switch(Content));
            }
        }

        /// <summary>Computes embeddings for the given texts.</summary>
        public Task<float[][]> GetVectorsAsync(string[] texts, CancellationToken ct = default)
        {
            if (!IsValid) throw new DataException("Embedding not setup");

            if (texts == null || texts.Length == 0)
                return Task.FromResult(Array.Empty<float[]>());

            return _processor.GetVectorsAsync(_worker, texts, _model.MaxLength.Value, ct);
        }
    }
}
