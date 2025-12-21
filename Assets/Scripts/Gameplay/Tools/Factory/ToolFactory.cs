using System;
using Core;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Factory
{
    public class ToolFactory : DisposableClass
    {
        public IObservable<IToolCard> OnCardCreated => _onToolCreated;
        private readonly Subject<IToolCard> _onToolCreated = new();

        private readonly ToolCard _toolCardPrefab;

        public ToolFactory(ToolCard toolCardPrefab)
        {
            _toolCardPrefab = toolCardPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onToolCreated.AddTo(Disposables);
        }

        public void CreateTool(ToolConfig config, Vector3 position)
        {
            var tool = UnityEngine.Object.Instantiate(_toolCardPrefab, position, Quaternion.identity);
            tool.name = config.Name;
            tool.Init(new ToolCard.Model(config));

            _onToolCreated?.OnNext(tool);
        }
    }
}