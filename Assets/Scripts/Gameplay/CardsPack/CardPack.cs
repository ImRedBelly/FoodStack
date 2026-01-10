using System;
using Core;
using Gameplay.CardsPack.Handlers;
using Gameplay.Core.Interfaces;
using Support;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Gameplay.CardsPack
{
    public class CardPack : DisposableBehaviour<BuyCardsPackButton.Model>, IDragObject
    {
        public class Model
        {
        }

        public IObservable<Unit> OnClick => _onClick;
        private readonly Subject<Unit> _onClick = new();

        public Transform Transform => transform;

        [SerializeField] private ObservablePointerClickTrigger _clickTrigger;
        [SerializeField] private CardsPackViewHandler _cardsPackViewHandler;

        protected override void OnInit()
        {
            base.OnInit();

            _onClick.AddTo(Disposables);

            _clickTrigger
                .OnPointerClickAsObservable()
                .SafeSubscribe(_ => _onClick.OnNext(Unit.Default))
                .AddTo(Disposables);
        }

        public virtual void OnDragStart()
        {
            _cardsPackViewHandler.SetStateShadow(true);
        }

        public virtual void OnDragEnd()
        {
            _cardsPackViewHandler.SetStateShadow(false);
        }
    }
}