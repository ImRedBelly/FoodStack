using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Services
{
    public class CardMergeService : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly CardDragService _dragService;
        private readonly CardStackService _cardStackService;

        private readonly List<IIngredientCard> _cards = new();

        private CardStack _dragOriginStack;

        public CardMergeService(CardFactory cardFactory, CardDragService dragService, CardStackService cardStackService)
        {
            _cardFactory = cardFactory;
            _dragService = dragService;
            _cardStackService = cardStackService;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _cardFactory.OnCardCreated
                .SafeSubscribe(AddCard)
                .AddTo(Disposables);

            _dragService.OnStartDrag
                .SafeSubscribe(StartDrag)
                .AddTo(Disposables);

            _dragService.OnEndDrag
                .SafeSubscribe(EndDrag)
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

        private void StartDrag(IIngredientCard draggedIngredientCard)
        {
            _dragOriginStack = _cardStackService.GetStack(draggedIngredientCard);

            _cardStackService.DetachSubStack(draggedIngredientCard);

            foreach (var card in _cards)
                card.SetStateEligibleFrame(TryActivateEligibleFrame(draggedIngredientCard, card));
        }


        private void EndDrag(IIngredientCard draggedIngredientCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(false);

            bool merged = TryMerge(draggedIngredientCard);

            if (!merged && DroppedOnOriginStack(draggedIngredientCard))
            {
                _cardStackService.RestoreDetachedStack(draggedIngredientCard);
            }

            _dragOriginStack = null;
        }

        private void UpdateSortingOrder()
        {
            foreach (var card in _cards)
            {
                card.UpdateSortingOrder();
            }
        }

        private bool DroppedOnOriginStack(IIngredientCard draggedIngredientCard)
        {
            if (_dragOriginStack == null)
                return false;

            foreach (var card in _dragOriginStack.Cards)
            {
                if (IsOverlapping(draggedIngredientCard, card))
                    return true;
            }

            return false;
        }


        private bool TryMerge(IIngredientCard draggedIngredientCard)
        {
            foreach (var card in _cards)
            {
                if (card == draggedIngredientCard) continue;
                if (!IsOverlapping(draggedIngredientCard, card)) continue;

                var targetStack = _cardStackService.GetStack(card);

                _cardStackService.MergeStacks(draggedIngredientCard, targetStack);
                return true;
            }

            return false;
        }

        private bool IsOverlapping(IIngredientCard draggedIngredientCard, IIngredientCard other)
        {
            var draggedStack = _cardStackService.GetStack(draggedIngredientCard);
            if (draggedStack != null && draggedStack == _cardStackService.GetStack(other))
                return false;

            return draggedIngredientCard.Collider.bounds.Intersects(other.Collider.bounds);
        }

        private bool TryActivateEligibleFrame(IIngredientCard draggedIngredientCard, IIngredientCard targetIngredientCard)
        {
            var draggedStack = _cardStackService.GetStack(draggedIngredientCard);
            var targetStack = _cardStackService.GetStack(targetIngredientCard);

            bool equalCards = draggedIngredientCard == targetIngredientCard;
            bool draggedStackContainsTarget = draggedStack.Cards.Contains(targetIngredientCard);
            bool isLastCardInStack = targetStack.Cards.Last() == targetIngredientCard;

            return !equalCards && !draggedStackContainsTarget && isLastCardInStack;
        }
    }
}