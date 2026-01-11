using System;
using Core;
using Gameplay.CardsPack.Handlers;
using Gameplay.CardsPack.Types;
using Support;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Gameplay.CardsPack
{
    public class BuyCardsPackButton : DisposableBehaviour<BuyCardsPackButton.Model>
    {
        public IObservable<CardPackType> OnClick => _onClick;
        private readonly Subject<CardPackType> _onClick = new();

        public class Model
        {
        }

        [SerializeField] private CardPackType _cardPackType;
        [SerializeField] private BuyCardsPackHandler _buyCardsPackHandler;
        [SerializeField] private ObservablePointerClickTrigger _clickTrigger;

        protected override void OnInit()
        {
            base.OnInit();

            _onClick.AddTo(Disposables);

            _clickTrigger
                .OnPointerClickAsObservable()
                .SafeSubscribe(_ => _onClick.OnNext(_cardPackType))
                .AddTo(Disposables);
        }

        public void SetActiveCardPanel(bool state)
        {
            _buyCardsPackHandler.SetActiveCardPanel(state);
        }
    }
}