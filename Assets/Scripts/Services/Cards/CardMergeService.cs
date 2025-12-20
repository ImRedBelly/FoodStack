using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;

namespace Services.Cards
{
    public class CardMergeService : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly CardDragService _dragService;
        private readonly CardStackService _cardStackService;

        private readonly List<ICard> _cards = new();
        
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

        private void UpdateSortingOrder()
        {
            foreach (var card in _cards)
            {
                card.UpdateSortingOrder();
            }
        }
        private void AddCard(ICard newCard)
        {
            if (!_cards.Contains(newCard))
            {
                _cards.Add(newCard);
            }
        }

        private void StartDrag(ICard draggedCard)
        {
            _dragOriginStack = _cardStackService.GetStack(draggedCard);

            _cardStackService.DetachSubStack(draggedCard);

            foreach (var card in _cards)
                if (card != draggedCard)
                    card.SetStateEligibleFrame(true);
        }


        private void EndDrag(ICard draggedCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(false);

            bool merged = TryMerge(draggedCard);

            if (!merged && DroppedOnOriginStack(draggedCard))
            {
                _cardStackService.RestoreDetachedStack(draggedCard);
            }

            _dragOriginStack = null;
        }
        private bool DroppedOnOriginStack(ICard draggedCard)
        {
            if (_dragOriginStack == null)
                return false;

            foreach (var card in _dragOriginStack.Cards)
            {
                if (IsOverlapping(draggedCard, card))
                    return true;
            }

            return false;
        }


        private bool TryMerge(ICard draggedCard)
        {
            foreach (var card in _cards)
            {
                if (card == draggedCard) continue;
                if (!IsOverlapping(draggedCard, card)) continue;

                var targetStack = _cardStackService.GetStack(card);

                _cardStackService.MergeStacks(draggedCard, targetStack);
                return true;
            }

            return false;
        }
        
        private bool IsOverlapping(ICard draggedCard, ICard other)
        {
            var draggedStack = _cardStackService.GetStack(draggedCard);
            if (draggedStack != null && draggedStack == _cardStackService.GetStack(other))
                return false;

            return draggedCard.Collider.bounds.Intersects(other.Collider.bounds);
        }
    }
}