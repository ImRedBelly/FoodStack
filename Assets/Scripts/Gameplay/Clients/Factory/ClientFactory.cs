using System;
using Core;
using Gameplay.Clients.Configs;
using UniRx;
using UnityEngine;

namespace Gameplay.Clients.Factory
{
    public class ClientFactory : DisposableClass
    {
        public IObservable<ClientCard> OnCardCreated => _onClientCreated;
        public IObservable<ClientCard> OnCardRemoved => _onClientRemoved;

        private readonly Subject<ClientCard> _onClientCreated = new();
        private readonly Subject<ClientCard> _onClientRemoved = new();

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

        public void CreateClient(ClientConfig config, Vector3 position)
        {
            var card = UnityEngine.Object.Instantiate(_clientCardPrefab, position, Quaternion.identity);
            card.name = config.Name;
            card.Init(new ClientCard.Model(config));

            _onClientCreated?.OnNext(card);
        }

        public void RemoveClient(ClientCard clientCard)
        {
            _onClientRemoved?.OnNext(clientCard);
            UnityEngine.Object.Destroy(clientCard.gameObject);
        }
    }
}