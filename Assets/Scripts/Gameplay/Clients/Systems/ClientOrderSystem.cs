using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Interfaces;
using Gameplay.OrderButton.Interfaces;
using Gameplay.Recipes.Configs;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Clients.Systems
{
    public class ClientOrderSystem : DisposableClass
    {
        public IObservable<(IOrderButton, RecipeConfig)> OnUpdateOrderButton => _onUpdateOrderButton;

        private readonly Subject<(IOrderButton, RecipeConfig)> _onUpdateOrderButton = new();

        private readonly ClientFactory _clientFactory;
        private readonly ClientTriggerServiceSystem _clientTriggerServiceSystem;
        private readonly ClientServiceSystem _clientServiceSystem;
        private readonly OrderButtonSelectSystem _orderButtonSelectSystem;
        private readonly ClientGenerateOrderSystem _clientGenerateOrderSystem;

        private readonly Dictionary<IClientCard, IOrderButton> _clientButtons = new();


        public ClientOrderSystem(
            ClientFactory clientFactory,
            ClientTriggerServiceSystem clientTriggerServiceSystem,
            ClientServiceSystem clientServiceSystem,
            OrderButtonSelectSystem orderButtonSelectSystem,
            ClientGenerateOrderSystem clientGenerateOrderSystem)
        {
            _clientFactory = clientFactory;
            _clientTriggerServiceSystem = clientTriggerServiceSystem;
            _clientServiceSystem = clientServiceSystem;
            _orderButtonSelectSystem = orderButtonSelectSystem;
            _clientGenerateOrderSystem = clientGenerateOrderSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onUpdateOrderButton.AddTo(Disposables);

            _clientTriggerServiceSystem.OnClientTriggerService
                .SafeSubscribe(ClientService)
                .AddTo(Disposables);

            _clientServiceSystem.OnClientServiceFinish
                .SafeSubscribe(CreateClient)
                .AddTo(Disposables);
        }

        private void ClientService((IClientCard clientCard, ICard resultCard) data)
        {
            _clientButtons[data.clientCard].UpdateOrderSprite(null);
            _clientButtons[data.clientCard].SetStateSlider(true);

            float duration = Constants.TimeServeClient;
            float elapsed = 0f;

            Observable.EveryUpdate()
                .TakeWhile(_ => elapsed < duration)
                .Do(_ =>
                {
                    elapsed += Time.deltaTime;
                    _clientButtons[data.clientCard].SetProgress(elapsed / duration);
                })
                .SafeSubscribe(
                    _ => { },
                    () =>
                    {
                        _clientButtons[data.clientCard].SetStateSlider(false);
                        _clientButtons[data.clientCard].HideClient(false);
                    })
                .AddTo(Disposables);
        }


        public void CreateClient((IClientCard clientServiced, ICard resultCard) data)
        {
            ResetOrderButton(data.clientServiced);

            var orderData = _clientGenerateOrderSystem.GenerateOrderData();

            var orderButton = _orderButtonSelectSystem.GetOrderButton();

            if (orderButton == null) return;
            orderButton.HideClient(true);

            var recipeConfig = orderData.recipeConfig;
            var clientCard = _clientFactory.CreateClient(orderData.clientConfig,
                recipeConfig.Result, Vector3.up * 0.5f,
                orderButton.ClientPoint);

            orderButton.UpdateOrderSprite(recipeConfig.InfoIcon);
            _clientButtons.TryAdd(clientCard, orderButton);
            _onUpdateOrderButton?.OnNext((orderButton, recipeConfig));

            orderButton.ShowClient(false);
        }

        private void ResetOrderButton(IClientCard clientServiced)
        {
            if (clientServiced != null && _clientButtons.TryGetValue(clientServiced, out var button))
            {
                _orderButtonSelectSystem.ReturnOrderButton(button);
            }
        }
    }
}