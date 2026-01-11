using System;
using Core;
using Gameplay.CardsPack.Handlers;
using Gameplay.Core.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.CardsPack
{
    public class CardPack : DisposableBehaviour<BuyCardsPackButton.Model>, IDragObject
    {
        public class Model
        {
        }

        public IObservable<CardPack> OnClick => _onClick;
        private readonly Subject<CardPack> _onClick = new();

        public Transform Transform => transform;

        [SerializeField] private CardsPackViewHandler _cardsPackViewHandler;

        private float _deltaMove;

        protected override void OnInit()
        {
            base.OnInit();

            _onClick.AddTo(Disposables);

            _cardsPackViewHandler.Initialize();
        }

        public virtual void OnDragStart()
        {
            _deltaMove = 0;
            _cardsPackViewHandler.SetStateShadow(true);
        }

        public void OnDrag(float delta)
        {
            _deltaMove += delta;
        }

        public virtual void OnDragEnd()
        {
            if (_deltaMove <= 1.25f) _onClick.OnNext(this);
            _deltaMove = 0;

            _cardsPackViewHandler.SetStateShadow(false);
        }

        public void UpdateCountText(string countCards)
        {
            _cardsPackViewHandler.UpdateCountText(countCards);
        }
    }
}