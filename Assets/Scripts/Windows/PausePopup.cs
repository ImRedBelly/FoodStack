using System;
using Services.WindowService;
using Support;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Windows
{
    public class PausePopup : WindowBase<PausePopup.Model>
    {
        public class Model
        {
            public readonly Action OnClickReload;
            public readonly Action OnClickUpgrade;
            public readonly WindowsService WindowsService;


            public Model(
                Action onClickReload,
                Action onClickUpgrade,
                WindowsService windowsService)
            {
                OnClickReload = onClickReload;
                OnClickUpgrade = onClickUpgrade;
                WindowsService = windowsService;
            }
        }

        [SerializeField] private Button _buttonReload;
        [SerializeField] private Button _buttonResume;
        [SerializeField] private Button _buttonUpgrade;


        protected override void OnOpen()
        {
            _buttonReload
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClickReload?.Invoke();
                    ActiveModel.WindowsService.Close();
                })
                .AddTo(Disposables);

            _buttonResume
                .OnClickAsObservable()
                .SafeSubscribe(_ => { ActiveModel.WindowsService.Close(); })
                .AddTo(Disposables);

            _buttonUpgrade
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClickUpgrade?.Invoke();
                })
                .AddTo(Disposables);
        }
    }
}