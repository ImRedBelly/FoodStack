using Core;
using Gameplay.Level.Handlers;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Level.Systems
{
    public class LevelTimerSystem : DisposableClass
    {
        private readonly DaySliderHandler _daySliderHandler;
        private readonly PauseGameSystem _pauseGameSystem;
        private readonly float _levelTime;

        private bool _pauseState;
        private float _elapsedTime;

        public LevelTimerSystem(DaySliderHandler daySliderHandler, float levelTime, PauseGameSystem pauseGameSystem)
        {
            _daySliderHandler = daySliderHandler;
            _levelTime = levelTime;
            _pauseGameSystem = pauseGameSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _pauseGameSystem.OnPauseGame
                .SafeSubscribe(PauseGame)
                .AddTo(Disposables);

            Observable
                .EveryUpdate()
                .Where(_ => !_pauseState)
                .TakeWhile(_ => _elapsedTime < _levelTime)
                .Subscribe(_ =>
                    {
                        _elapsedTime += Time.deltaTime;

                        var progress = 1f - Mathf.Clamp01(_elapsedTime / _levelTime);

                        _daySliderHandler.SetProgress(progress);
                        _daySliderHandler.SetErrorStateView(progress <= 0.2f);
                    },
                    OnLevelTimeEnded)
                .AddTo(Disposables);
        }

        private void PauseGame(bool pauseState)
        {
            _pauseState = pauseState;
        }

        private void OnLevelTimeEnded()
        {
            Debug.Log("Level time ended");
        }
    }
}