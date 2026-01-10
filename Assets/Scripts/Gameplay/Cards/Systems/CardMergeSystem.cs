using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Core.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardMergeSystem : DisposableClass
    {
        private readonly CardCollisionSystem _cardCollisionSystem;

        private readonly CardDragSystem _dragSystem;
        private readonly CardStackSystem _cardStackSystem;

        private CardStack _dragOriginStack;

        public CardMergeSystem(
            CardCollisionSystem cardCollisionSystem,
            CardDragSystem dragSystem,
            CardStackSystem cardStackSystem)
        {
            _cardCollisionSystem = cardCollisionSystem;
            _dragSystem = dragSystem;
            _cardStackSystem = cardStackSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardCollisionSystem.OnCardCollisionWithCard
                .SafeSubscribe(EndDrag)
                .AddTo(Disposables);

            _dragSystem.OnStartDrag
                .SafeSubscribe(StartDrag)
                .AddTo(Disposables);
        }

        private void StartDrag(IDragObject dragObject)
        {
            if (dragObject is not ICard card) return;

            _dragOriginStack = _cardStackSystem.GetStack(card);
            _cardStackSystem.DetachSubStack(card);
        }

        private void EndDrag((ICard draggedCard, ICard targetCard) data)
        {
            bool merged = TryMerge(data.draggedCard, data.targetCard);

            if (!merged && DroppedOnOriginStack(data.draggedCard))
            {
                _cardStackSystem.RestoreDetachedStack(data.draggedCard);
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


        private bool TryMerge(ICard draggedCard, ICard targetCard)
        {
            if (targetCard == draggedCard) return false;

            var targetStack = _cardStackSystem.GetStack(targetCard);

            _cardStackSystem.MergeStacks(draggedCard, targetStack);
            return true;
        }

        private bool IsOverlapping(ICard draggedCard, ICard other)
        {
            var draggedStack = _cardStackSystem.GetStack(draggedCard);
            if (draggedStack != null && draggedStack == _cardStackSystem.GetStack(other))
                return false;

            return draggedCard.Collider.bounds.Intersects(other.Collider.bounds);
        }
    }
}