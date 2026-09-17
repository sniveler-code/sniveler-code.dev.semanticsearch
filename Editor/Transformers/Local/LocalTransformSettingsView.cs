using System;
using System.Threading;
using System.Threading.Tasks;
using SnivelerCode.SemanticSearch.Editor.Templates;
using UnityEngine.UIElements;

namespace SnivelerCode.SemanticSearch.Editor.Transformers.Local
{
    /// <summary>Right panel: database bake buttons and context thresholds.</summary>
    public sealed class LocalTransformSettingsView
    {
        private CancellationTokenSource _cts;
        private readonly LocalTransformModel _model;
        public VisualElement Content { get; }

        /// <summary>Builds database rows and threshold fields.</summary>
        public LocalTransformSettingsView(LocalTransformModel model)
        {
            _model = model;
            var template = TemplateProvider.LoadByName("SemanticSearch_PrefabsLocalSettings");
            Content = template.Instantiate().Q<VisualElement>("Root");

            var metadataContainer = Content.Q<VisualElement>("MetadataContainer");
            foreach (var property in model.Databases)
            {
                model.DatabaseProvider.Accept(property, metadataContainer);
                var field = metadataContainer.Q(property.Name);
                field.parent.Add(new BakeButton(property.BakeClick) {style = {width = 80}});
            }

            var contextContainer = Content.Q<VisualElement>("ContextContainer");
            model.ContextProvider.Accept(model.ContextConfig.DetailedThreshold, contextContainer);
            model.ContextProvider.Accept(model.ContextConfig.DetailedDecay, contextContainer);
            model.ContextProvider.Accept(model.ContextConfig.GeneralThreshold, contextContainer);

        }

    }
}
