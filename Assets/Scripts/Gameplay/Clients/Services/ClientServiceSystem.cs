using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Clients.Services
{
    public class ClientServiceSystem : DisposableClass
    {
        public IObservable<IClientCard> OnClientService => _onClientService;

        private readonly Subject<IClientCard> _onClientService = new();

        private readonly ClientFactory _clientFactory;
        private readonly CardFactory _cardFactory;
        private readonly CardCollisionSystem _cardCollisionSystem;

        private Dictionary<IClientCard, IngredientConfig> _clients = new();

        public ClientServiceSystem(
            ClientFactory clientFactory,
            CardFactory cardFactory,
            CardCollisionSystem cardCollisionSystem)
        {
            _clientFactory = clientFactory;
            _cardFactory = cardFactory;
            _cardCollisionSystem = cardCollisionSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onClientService.AddTo(Disposables);

            _clientFactory.OnClientCreated
                .SafeSubscribe(AddClient)
                .AddTo(Disposables);

            _clientFactory.OnClientRemoved
                .SafeSubscribe(RemoveClient)
                .AddTo(Disposables);

            _cardCollisionSystem.OnCardCollisionWithClient
                .SafeSubscribe(CollisionWithClient)
                .AddTo(Disposables);
        }

        private void AddClient((IClientCard client, IngredientConfig target) data)
        {
            _clients[data.client] = data.target;
        }

        private void RemoveClient(IClientCard clientCard)
        {
            _clients.Remove(clientCard);
        }


        private void CollisionWithClient((IClientCard clientCard, IIngredientCard ingredientCard) data)
        {
            if (_clients.TryGetValue(data.clientCard, out var target))
            {
                if (target == data.ingredientCard.IngredientConfig)
                {
                    _clientFactory.RemoveClient(data.clientCard);
                    _cardFactory.RemoveIngredient(data.ingredientCard);

                    _onClientService?.OnNext(data.clientCard);
                }
            }
        }
    }
}