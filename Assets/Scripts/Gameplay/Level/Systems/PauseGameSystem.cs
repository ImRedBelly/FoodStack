using System;
using Core;
using UniRx;

namespace Gameplay.Level.Systems
{
    public class PauseGameSystem : DisposableClass
    {
        public IObservable<bool> OnPauseGame => _onPauseGame;
        private readonly Subject<bool> _onPauseGame = new();
        
        protected override void OnInit()
        {
            base.OnInit();

            _onPauseGame.AddTo(Disposables);
        }

        public void SetStatePause(bool state)
        {
            _onPauseGame?.OnNext(state);
        }
    }
}