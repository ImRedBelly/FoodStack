using System;
using Services.WindowService;
using Support;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.LosePopup
{
    public class LosePopup : WindowBase<LosePopup.Model>
    {
        public class Model
        {
            public readonly Action OnClickResume;
            public readonly WindowsService WindowsService;
            public readonly int LevelTarget;
            public readonly int CurrentLevelTarget;

            public Model(WindowsService windowsService, Action onClickResume, int currentLevelTarget, int levelTarget)
            {
                WindowsService = windowsService;
                OnClickResume = onClickResume;
                LevelTarget = levelTarget;
                CurrentLevelTarget = currentLevelTarget;
            }
        }

        [SerializeField] private Button _buttonResume;
        [Space] [SerializeField] private TMP_Text _textEarnedMoney;

        protected override void OnOpen()
        {
            _buttonResume
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClickResume?.Invoke();
                    ActiveModel.WindowsService.Close();
                })
                .AddTo(Disposables);
            _textEarnedMoney.SetText($"{ActiveModel.CurrentLevelTarget}/{ActiveModel.LevelTarget}");
        }
    }
}