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
        private readonly List<IIngredientCard> _cards = new();

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

        private void AddCard(IIngredientCard newIngredientCard)
        {
            if (!_cards.Contains(newIngredientCard))
            {
                _cards.Add(newIngredientCard);
            }
        }

        private void RemoveCard(IIngredientCard ingredientCard)
        {
            if (_cards.Contains(ingredientCard))
            {
                _cards.Remove(ingredientCard);
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