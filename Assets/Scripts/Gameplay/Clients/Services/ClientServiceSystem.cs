using System;
using Core;
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
        private readonly ClientTriggerServiceSystem _clientTriggerServiceSystem;

        public ClientServiceSystem(ClientTriggerServiceSystem clientTriggerServiceSystem, ClientFactory clientFactory)
        {
            _clientTriggerServiceSystem = clientTriggerServiceSystem;
            _clientFactory = clientFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onClientService.AddTo(Disposables);

            _clientTriggerServiceSystem.OnClientTriggerService
                .SafeSubscribe(ClientTriggerService)
                .AddTo(Disposables);
        }


        private void ClientTriggerService(IClientCard clientCard)
        {
            Observable.Timer(TimeSpan.FromSeconds(Constants.TimeServeClient + Constants.TimeAnimationClient))
                .SafeSubscribe(_ =>
                {
                    _clientFactory.RemoveClient(clientCard);
                    _onClientService?.OnNext(clientCard);
                })
                .AddTo(Disposables);
        }
    }
}