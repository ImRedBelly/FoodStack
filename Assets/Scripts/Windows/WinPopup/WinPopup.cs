using System;
using Services.WindowService;
using Support;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.WinPopup
{
    public class WinPopup : WindowBase<WinPopup.Model>
    {
        public class Model
        {
            public readonly Action OnClickResume;
            public readonly WindowsService WindowsService;

            public readonly int DayProfit;
            public readonly int ServedClients;
            public readonly int CookedFood;

            public Model(WindowsService windowsService, Action onClickResume, 
                int dayProfit, 
                int servedClients,
                int cookedFood)
            {
                WindowsService = windowsService;
                OnClickResume = onClickResume;
                DayProfit = dayProfit;
                ServedClients = servedClients;
                CookedFood = cookedFood;
            }
        }

        [SerializeField] private Button _buttonResume;
        [Space]
        [SerializeField] private TMP_Text _textDayProfit;
        [SerializeField] private TMP_Text _textServedClients;
        [SerializeField] private TMP_Text _textCookedFood;

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
            
            _textDayProfit.SetText(ActiveModel.DayProfit.ToString());
            _textServedClients.SetText(ActiveModel.ServedClients.ToString());
            _textCookedFood.SetText(ActiveModel.CookedFood.ToString());
        }
    }
}