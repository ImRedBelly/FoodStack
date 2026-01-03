using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardSortingOrderSystem : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly List<ICard> _cards = new();

        public CardSortingOrderSystem(CardFactory cardFactory)
        {
            _cardFactory = cardFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardFactory.OnCardCreated
                .SafeSubscribe(AddCard)
                .AddTo(Disposables);

            _cardFactory.OnCardRemoved
                .SafeSubscribe(RemoveCard)
                .AddTo(Disposables);

            Observable.EveryUpdate()
                .Subscribe(_ => UpdateSortingOrder())
                .AddTo(Disposables);
        }

        private void AddCard(ICard newCard)
        {
            if (!_cards.Contains(newCard))
            {
                _cards.Add(newCard);
            }
        }

        private void RemoveCard(ICard card)
        {
            if (_cards.Contains(card))
            {
                _cards.Remove(card);
            }
        }

        private void UpdateSortingOrder()
        {
            foreach (var card in _cards)
            {
                card.UpdateSortingOrder();
            }
        }
    }
}