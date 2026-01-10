using System;
using System.Collections.Generic;
using Core;
using Gameplay.Clients.Systems;
using Gameplay.Level.Systems;
using Gameplay.OrderButton.Interfaces;
using Gameplay.Recipes.Configs;
using Services.WindowService;
using Support;
using UniRx;

namespace Gameplay.OrderButton.Services
{
    public class OpenOrderInfoPopupSystem : DisposableClass
    {
        private readonly WindowsService _windowsService;
        private readonly WindowResolver _windowResolver;
        private readonly ClientOrderSystem _clientOrderSystem;
        private readonly OrderButtonClickSystem _orderButtonClickSystem;
        private readonly PauseGameSystem _pauseGameSystem;
        private readonly IReadOnlyCollection<RecipeConfig> _recipesCollection;

        private Dictionary<IOrderButton, RecipeConfig> _orderData = new();

        public OpenOrderInfoPopupSystem(WindowsService windowsService, WindowResolver windowResolver,
            ClientOrderSystem clientOrderSystem, OrderButtonClickSystem orderButtonClickSystem,
            PauseGameSystem pauseGameSystem, IReadOnlyCollection<RecipeConfig> recipesCollection)
        {
            _windowsService = windowsService;
            _windowResolver = windowResolver;
            _clientOrderSystem = clientOrderSystem;
            _orderButtonClickSystem = orderButtonClickSystem;
            _pauseGameSystem = pauseGameSystem;
            _recipesCollection = recipesCollection;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _clientOrderSystem.OnUpdateOrderButton
                .SafeSubscribe(UpdateOrderButton)
                .AddTo(Disposables);

            _orderButtonClickSystem.OnClickOrderButton
                .SafeSubscribe(ClickOrderButton)
                .AddTo(Disposables);
        }

        private void UpdateOrderButton((IOrderButton orderButton, RecipeConfig recipeConfig) data)
        {
            _orderData[data.orderButton] = data.recipeConfig;
        }

        private void ClickOrderButton(IOrderButton orderButton)
        {
            if (_orderData.TryGetValue(orderButton, out var value))
            {
                var pausePopupModel = _windowResolver.GeOrderInfoPopupModel(value, _recipesCollection,
                    () => { _pauseGameSystem.SetStatePause(false); });
                _windowsService.Open(pausePopupModel, false);
                _pauseGameSystem.SetStatePause(true);
            }
        }
    }
}