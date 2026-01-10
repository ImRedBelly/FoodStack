using System;
using Core;
using Gameplay.CardsPack.Handlers;
using Support;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Gameplay.CardsPack
{
    public class BuyCardsPackButton : DisposableBehaviour<BuyCardsPackButton.Model>
    {
        public IObservable<Unit> OnClick => _onClick;
        private readonly Subject<Unit> _onClick = new();

        public class Model
        {
        }

        [SerializeField] private BuyCardsPackHandler _buyCardsPackHandler;
        [SerializeField] private ObservablePointerClickTrigger _clickTrigger;

        protected override void OnInit()
        {
            base.OnInit();

            _onClick.AddTo(Disposables);

            _clickTrigger
                .OnPointerClickAsObservable()
                .SafeSubscribe(_ => _onClick.OnNext(Unit.Default))
                .AddTo(Disposables);
        }

        public void SetActiveCardPanel(bool state)
        {
            _buyCardsPackHandler.SetActiveCardPanel(state);
        }
    }
}