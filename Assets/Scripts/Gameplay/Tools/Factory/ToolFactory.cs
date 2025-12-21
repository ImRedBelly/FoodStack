using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Factory
{
    public class ToolFactory : DisposableClass
    {
        public IObservable<ITool> OnCardCreated => _onToolCreated;
        private readonly Subject<ITool> _onToolCreated = new();

        private readonly Tool _toolPrefab;

        public ToolFactory(Tool toolPrefab)
        {
            _toolPrefab = toolPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onToolCreated.AddTo(Disposables);
        }

        public void CreateTool(ToolConfig config, Vector3 position)
        {
            var tool = UnityEngine.Object.Instantiate(_toolPrefab, position, Quaternion.identity);
            tool.name = config.Name;
            tool.Init(new Tool.Model(config));

            _onToolCreated?.OnNext(tool);
        }
    }
}