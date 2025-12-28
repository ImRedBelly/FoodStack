using System.Collections.Generic;
using Core;
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
        private readonly ClientServiceSystem _clientServiceSystem;
        private readonly OrderButtonSelectSystem _orderButtonSelectSystem;

        private readonly Dictionary<IClientCard, IOrderButton> _clientButtons = new();

        private int _currentClientIndex = 0;

        public ClientOrderSystem(
            LevelData levelData,
            ClientFactory clientFactory,
            ClientServiceSystem clientServiceSystem,
            OrderButtonSelectSystem orderButtonSelectSystem)
        {
            _levelData = levelData;
            _clientFactory = clientFactory;
            _clientServiceSystem = clientServiceSystem;
            _orderButtonSelectSystem = orderButtonSelectSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _clientServiceSystem.OnClientService
                .SafeSubscribe(ClientService)
                .AddTo(Disposables);

            InitClients();
        }

        private void ClientService(IClientCard clientCard)
        {
            _clientFactory.RemoveClient(clientCard);
            _orderButtonSelectSystem.ReturnOrderButton(_clientButtons[clientCard]);
            CreateClient();
        }

        private void InitClients()
        {
            for (int i = 0; i < 2; i++)
            {
                CreateClient();
            }
        }

        private void CreateClient()
        {
            if (_levelData.OrderQueue.Length <= _currentClientIndex) return;
            
            var orderButton = _orderButtonSelectSystem.GetOrderButton();
            var clientCard = _clientFactory.CreateClient(_levelData.OrderQueue[_currentClientIndex].ClientConfig,
                _levelData.OrderQueue[_currentClientIndex].IngredientConfig, Vector3.up * 0.5f,
                orderButton.ClientPoint);

            _clientButtons.TryAdd(clientCard, orderButton);
            _currentClientIndex++;
        }
    }
}