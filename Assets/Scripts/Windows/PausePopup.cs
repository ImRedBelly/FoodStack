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
            public readonly Action OnClick;
            public readonly WindowsService WindowsService;


            public Model(Action onClick, WindowsService windowsService)
            {
                OnClick = onClick;
                WindowsService = windowsService;
            }
        }

        [SerializeField] private Button _button;


        protected override void OnOpen()
        {
            _button
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClick?.Invoke();
                    ActiveModel.WindowsService.Close();
                })
                .AddTo(Disposables);
        }
    }
}