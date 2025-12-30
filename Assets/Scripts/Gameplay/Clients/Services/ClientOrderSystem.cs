using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Interfaces;
using Gameplay.Level.Configs;
using Gameplay.OrderButton.Interfaces;
using Gameplay.OrderButton.Services;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Clients.Services
{
    public class ClientOrderSystem : DisposableClass
    {
        private readonly LevelData _levelData;
        private readonly ClientFactory _clientFactory;
        private readonly ClientTriggerServiceSystem _clientTriggerServiceSystem;
        private readonly ClientServiceSystem _clientServiceSystem;
        private readonly OrderButtonSelectSystem _orderButtonSelectSystem;

        private readonly Dictionary<IClientCard, IOrderButton> _clientButtons = new();

        private int _currentClientIndex = 0;

        public ClientOrderSystem(LevelData levelData,
            ClientFactory clientFactory,
            ClientTriggerServiceSystem clientTriggerServiceSystem,
            ClientServiceSystem clientServiceSystem,
            OrderButtonSelectSystem orderButtonSelectSystem)
        {
            _levelData = levelData;
            _clientFactory = clientFactory;
            _clientTriggerServiceSystem = clientTriggerServiceSystem;
            _clientServiceSystem = clientServiceSystem;
            _orderButtonSelectSystem = orderButtonSelectSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _clientTriggerServiceSystem.OnClientTriggerService
                .SafeSubscribe(ClientService)
                .AddTo(Disposables);

            _clientServiceSystem.OnClientService
                .SafeSubscribe(CreateClient)
                .AddTo(Disposables);
        }

        private void ClientService(IClientCard clientCard)
        {
            _clientButtons[clientCard].UpdateOrderSprite(null);
            _clientButtons[clientCard].SetStateSlider(true);

            float duration = Constants.TimeServeClient;
            float elapsed = 0f;

            Observable.EveryUpdate()
                .TakeWhile(_ => elapsed < duration)
                .Do(_ =>
                {
                    elapsed += Time.deltaTime;
                    _clientButtons[clientCard].SetProgress(elapsed / duration);
                })
                .SafeSubscribe(
                    _ => { },
                    () =>
                    {
                        _clientButtons[clientCard].SetStateSlider(false);
                        _clientButtons[clientCard].HideClient(false);
                    })
                .AddTo(Disposables);
        }


        public void CreateClient(IClientCard clientServiced)
        {
            ResetOrderButton(clientServiced);
            if (_levelData.OrderQueue.Length <= _currentClientIndex) return;

            var orderButton = _orderButtonSelectSystem.GetOrderButton();

            if (orderButton == null) return;
            orderButton.HideClient(true);

            var clientCard = _clientFactory.CreateClient(_levelData.OrderQueue[_currentClientIndex].ClientConfig,
                _levelData.OrderQueue[_currentClientIndex].RecipeConfig.Result, Vector3.up * 0.5f,
                orderButton.ClientPoint);

            orderButton.UpdateOrderSprite(_levelData.OrderQueue[_currentClientIndex].RecipeConfig.InfoIcon);
            _clientButtons.TryAdd(clientCard, orderButton);
            orderButton.ShowClient(false);
            _currentClientIndex++;
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