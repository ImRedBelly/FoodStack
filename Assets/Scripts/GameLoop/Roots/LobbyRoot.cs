using System;
using Configs;
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
            public readonly Action OnReload;
            public readonly WindowsService WindowsService;
            public readonly WindowResolver WindowResolver;

            public Model(Action onGameAction, Action onReload, WindowsService windowsService,
                WindowResolver windowResolver)
            {
                OnGameAction = onGameAction;
                OnReload = onReload;
                WindowsService = windowsService;
                WindowResolver = windowResolver;
            }
        }

        [SerializeField] private Transform _windowsAnchor;
        [Space]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _recipesButton;
        
        [Space] [Header("Data")] 
        [SerializeField] private RecipesConfig _recipesConfig;


        protected override void OnInit()
        {
            base.OnInit();
            
            ActiveModel.WindowsService.SetupAnchor(_windowsAnchor);

            _playButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.OnGameAction?.Invoke())
                .AddTo(Disposables);

            var recipesPopupModel = ActiveModel.WindowResolver.GetRecipesPopupModel(_recipesConfig.RecipeConfigs, () => { });

            _recipesButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => { ActiveModel.WindowsService.Open(recipesPopupModel, false); })
                .AddTo(Disposables);
        }
    }
}