using System;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Clients.Interfaces;
using Gameplay.Clients.Systems;
using Services.WindowService;
using Support;
using UniRx;

namespace Gameplay.Level.Systems
{
    public class LevelFinishSystem : DisposableClass
    {
        private readonly WindowsService _windowsService;
        private readonly WindowResolver _windowResolver;
        private readonly UpdateLevelTargetSystem _updateLevelTargetSystem;
        private readonly LevelTimerSystem _levelTimerSystem;
        private readonly ClientServiceSystem _clientServiceSystem;
        private readonly Action _onClickResume;

        private readonly int _levelTarget;
        private int _currentLevelTarget;
        private bool _waitClientService;
        private int _servedClients;

        public LevelFinishSystem(int levelTarget, UpdateLevelTargetSystem updateLevelTargetSystem,
            LevelTimerSystem levelTimerSystem, ClientServiceSystem clientServiceSystem, WindowsService windowsService,
            WindowResolver windowResolver,
            Action onClickResume)
        {
            _levelTarget = levelTarget;
            _updateLevelTargetSystem = updateLevelTargetSystem;
            _levelTimerSystem = levelTimerSystem;
            _clientServiceSystem = clientServiceSystem;
            _windowsService = windowsService;
            _windowResolver = windowResolver;
            _onClickResume = onClickResume;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _updateLevelTargetSystem.OnUpdateLevelTarget
                .SafeSubscribe(UpdateLevelTarget)
                .AddTo(Disposables);

            _clientServiceSystem.OnClientServiceStart
                .SafeSubscribe(ClientServiceStart)
                .AddTo(Disposables);

            _clientServiceSystem.OnClientServiceFinish
                .SafeSubscribe(ClientServiceFinish)
                .AddTo(Disposables);

            _levelTimerSystem.OnFinishTimer
                .SafeSubscribe(FinishTimer)
                .AddTo(Disposables);
        }

        private void UpdateLevelTarget(int value)
        {
            _servedClients++;
            _currentLevelTarget = value;

            if (_currentLevelTarget >= _levelTarget)
            {
                ShowWinPopup();
            }
        }

        private void ClientServiceStart((IClientCard clientCard, ICard resultCard) data)
        {
            _waitClientService = true;
        }

        private void ClientServiceFinish((IClientCard clientCard, ICard resultCard) data)
        {
            _waitClientService = false;
        }

        private void FinishTimer(Unit _)
        {
            if (_waitClientService) return;
            
            if (_currentLevelTarget < _levelTarget)
            {
                ShowLosePopup();
            }
            else
            {
                ShowWinPopup();
            }
        }


        private void ShowWinPopup()
        {
            Remove();
            SaveUtility.AppendStars(_levelTarget);
            var winPopupModel = _windowResolver.GeWinPopupModel(_onClickResume, _levelTarget, _servedClients, _servedClients);
            _windowsService.Open(winPopupModel, false);
        }

        private void ShowLosePopup()
        {
            Remove();
            var losePopupModel = _windowResolver.GeLosePopupModel(_onClickResume, _currentLevelTarget, _levelTarget);
            _windowsService.Open(losePopupModel, false);
        }

        private void Remove()
        {
            Disposables?.Dispose();
        }
    }
}