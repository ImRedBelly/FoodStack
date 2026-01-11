using System;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Interfaces;
using Gameplay.Level.Handlers;
using Support;
using UniRx;

namespace Gameplay.Clients.Systems
{
    public class ClientServiceSystem : DisposableClass
    {
        public IObservable<(IClientCard clientCard, ICard resultCard)> OnClientServiceFinish => _onClientServiceFinishFinish;
        public IObservable<(IClientCard clientCard, ICard resultCard)> OnClientServiceStart => _onClientServiceStart;

        private readonly Subject<(IClientCard clientCard, ICard resultCard)> _onClientServiceStart = new();
        private readonly Subject<(IClientCard clientCard, ICard resultCard)> _onClientServiceFinishFinish = new();

        private readonly ClientFactory _clientFactory;
        private readonly ServiceSuccessHandler _serviceSuccessHandler;
        private readonly ClientTriggerServiceSystem _clientTriggerServiceSystem;

        public ClientServiceSystem(ClientTriggerServiceSystem clientTriggerServiceSystem,
            ClientFactory clientFactory,
            ServiceSuccessHandler serviceSuccessHandler)
        {
            _clientTriggerServiceSystem = clientTriggerServiceSystem;
            _clientFactory = clientFactory;
            _serviceSuccessHandler = serviceSuccessHandler;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onClientServiceFinishFinish.AddTo(Disposables);
            _onClientServiceStart.AddTo(Disposables);

            _clientTriggerServiceSystem.OnClientTriggerService
                .SafeSubscribe(ClientTriggerService)
                .AddTo(Disposables);
        }


        private void ClientTriggerService((IClientCard clientCard, ICard resultCard) data)
        {
            _onClientServiceStart?.OnNext(data);
            
            Observable.Timer(TimeSpan.FromSeconds(Constants.TimeServeClient + Constants.TimeAnimationClient))
                .SafeSubscribe(_ =>
                {
                    _serviceSuccessHandler.ShowSuccess();
                    _clientFactory.RemoveClient(data.clientCard);
                    _onClientServiceFinishFinish?.OnNext(data);
                })
                .AddTo(Disposables);
        }
    }
}