using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardEligibleFrameStateSystem : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly CardDragSystem _dragSystem;
        private readonly CardStackSystem _cardStackSystem;

        private readonly List<ICard> _cards = new();

        public CardEligibleFrameStateSystem(CardFactory cardFactory, CardDragSystem dragSystem,
            CardStackSystem cardStackSystem)
        {
            _cardFactory = cardFactory;
            _dragSystem = dragSystem;
            _cardStackSystem = cardStackSystem;
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

            _dragSystem.OnEndDrag
                .SafeSubscribe(EndDrag)
                .AddTo(Disposables);

            _dragSystem.OnStartDrag
                .SafeSubscribe(StartDrag)
                .AddTo(Disposables);
        }


        private void AddCard(ICard newIngredientCard)
        {
            if (!_cards.Contains(newIngredientCard))
            {
                _cards.Add(newIngredientCard);
            }
        }

        private void RemoveCard(ICard ingredientCard)
        {
            if (_cards.Contains(ingredientCard))
            {
                _cards.Remove(ingredientCard);
            }
        }

        private void StartDrag(ICard draggedIngredientCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(TryActivateEligibleFrame(draggedIngredientCard, card));
        }


        private void EndDrag(ICard draggedIngredientCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(false);
        }

        private bool TryActivateEligibleFrame(ICard draggedIngredientCard, ICard targetIngredientCard)
        {
            var draggedStack = _cardStackSystem.GetStack(draggedIngredientCard);
            var targetStack = _cardStackSystem.GetStack(targetIngredientCard);

            bool equalCards = draggedIngredientCard == targetIngredientCard;
            bool draggedStackContainsTarget = draggedStack.Cards.Contains(targetIngredientCard);
            bool isLastCardInStack = targetStack.Cards.Last() == targetIngredientCard;

            return !equalCards && !draggedStackContainsTarget && isLastCardInStack;
        }
    }
}