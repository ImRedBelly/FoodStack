using System;
using Services.WindowService;
using Support;
using Support.GraphicsGroup;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Windows
{
    public class PlayConfirmWindow : WindowBase<PlayConfirmWindow.Model>
    {
        public class Model
        {
            public readonly Action OnClick;
            public readonly WindowsService WindowsService;
            public readonly bool IsPlay;

            public Model(Action onClick, WindowsService windowsService, bool isPlay)
            {
                OnClick = onClick;
                WindowsService = windowsService;
                IsPlay = isPlay;
            }
        }

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        [SerializeField] private GraphicsSwitchGroup _playGroup;
        [SerializeField] private GraphicsSwitchGroup _quitGroup;

        protected override void OnOpen()
        {
            var switchedGroup = ActiveModel.IsPlay ? _playGroup : _quitGroup;
            switchedGroup.Switch();

            _playButton
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClick?.Invoke();
                    ActiveModel.WindowsService.Close();
                })
                .AddTo(Disposables);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.WindowsService.Close())
                .AddTo(Disposables);
        }
    }
}