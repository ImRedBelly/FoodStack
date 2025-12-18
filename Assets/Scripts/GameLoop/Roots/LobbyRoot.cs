using System;
using Core;
using Services.WindowService;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.Roots
{
    public class LobbyRoot : DisposableBehaviour<LobbyRoot.Model>
    {
        public class Model
        {
            public readonly Action OnGameAction;
            public readonly WindowsService WindowsService;
            public readonly WindowResolver WindowResolver;

            public Model(Action onGameAction, WindowsService windowsService, WindowResolver windowResolver)
            {
                OnGameAction = onGameAction;
                WindowsService = windowsService;
                WindowResolver = windowResolver;
            }
        }

        [SerializeField] private Button _playButton;
      
        protected override void OnInit()
        {
            base.OnInit();

            _playButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.OnGameAction?.Invoke())
                .AddTo(Disposables);
            
        }
    }
}