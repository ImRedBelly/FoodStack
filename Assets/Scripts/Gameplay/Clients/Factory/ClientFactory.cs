using System;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Clients.Configs;
using Gameplay.Clients.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.Clients.Factory
{
    public class ClientFactory : DisposableClass
    {
        public IObservable<(IClientCard, CardConfig)> OnClientCreated => _onClientCreated;
        public IObservable<IClientCard> OnClientRemoved => _onClientRemoved;

        private readonly Subject<(IClientCard, CardConfig)> _onClientCreated = new();
        private readonly Subject<IClientCard> _onClientRemoved = new();

        private readonly ClientCard _clientCardPrefab;

        public ClientFactory(ClientCard clientCardPrefab)
        {
            _clientCardPrefab = clientCardPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onClientCreated.AddTo(Disposables);
            _onClientRemoved.AddTo(Disposables);
        }

        public IClientCard CreateClient(ClientConfig config, CardConfig cardConfig, Vector3 position, Transform parent)
        {
            var card = UnityEngine.Object.Instantiate(_clientCardPrefab, parent);
            card.Transform.localPosition = position;
            
            card.name = config.Name;
            card.Init(new ClientCard.Model(config));

            _onClientCreated?.OnNext((card, cardConfig));
            return card;
        }

        public void RemoveClient(IClientCard clientCard)
        {
            _onClientRemoved?.OnNext(clientCard);
            UnityEngine.Object.Destroy(clientCard.Transform.gameObject);
        }
    }
}