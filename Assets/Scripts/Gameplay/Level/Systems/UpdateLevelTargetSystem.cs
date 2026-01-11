using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Clients.Interfaces;
using Gameplay.Clients.Systems;
using Gameplay.Level.Handlers;
using Gameplay.Recipes.Configs;
using Support;
using UniRx;

namespace Gameplay.Level.Systems
{
    public class UpdateLevelTargetSystem : DisposableClass
    {
        public IObservable<int> OnUpdateLevelTarget => _onUpdateLevelTarget;
        private readonly Subject<int> _onUpdateLevelTarget = new();

        private readonly int _targetLevel;
        private readonly ClientServiceSystem _clientServiceSystem;
        private readonly LevelTargetHandler _levelTargetHandler;
        private readonly IReadOnlyCollection<RecipeConfig> _recipeConfigs;

        private int _money;

        public UpdateLevelTargetSystem(int targetLevel, ClientServiceSystem clientServiceSystem,
            LevelTargetHandler levelTargetHandler,
            IReadOnlyCollection<RecipeConfig> recipeConfigs)
        {
            _targetLevel = targetLevel;
            _clientServiceSystem = clientServiceSystem;
            _levelTargetHandler = levelTargetHandler;
            _recipeConfigs = recipeConfigs;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onUpdateLevelTarget.AddTo(Disposables);
            _clientServiceSystem.OnClientServiceFinish
                .SafeSubscribe(ClientServiceFinish)
                .AddTo(Disposables);

            UpdateTargetText();
        }


        private void ClientServiceFinish((IClientCard clientCard, ICard resultCard) data)
        {
            var money = 0;
            foreach (var recipeConfig in _recipeConfigs)
            {
                if (recipeConfig.Result.Name == data.resultCard.CardConfig.Name)
                {
                    money = recipeConfig.Profit;
                    break;
                }
            }

            _money += money;
            _onUpdateLevelTarget?.OnNext(money);

            UpdateTargetText();
        }

        private void UpdateTargetText()
        {
            _levelTargetHandler.SetLevelTarget($"{_money}/{_targetLevel}");
        }
    }
}