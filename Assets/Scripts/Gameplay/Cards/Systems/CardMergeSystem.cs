using Core;
using Gameplay.Cards.Interfaces;
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

        private void StartDrag(IIngredientCard draggedIngredientCard)
        {
            _dragOriginStack = _cardStackSystem.GetStack(draggedIngredientCard);
            _cardStackSystem.DetachSubStack(draggedIngredientCard);
        }


        private void EndDrag((IIngredientCard draggedIngredientCard, IIngredientCard targetIngredientCard) data)
        {
            bool merged = TryMerge(data.draggedIngredientCard, data.targetIngredientCard);

            if (!merged && DroppedOnOriginStack(data.draggedIngredientCard))
            {
                _cardStackSystem.RestoreDetachedStack(data.draggedIngredientCard);
            }

            _dragOriginStack = null;
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


        private bool TryMerge(IIngredientCard draggedIngredientCard, IIngredientCard targetIngredientCard)
        {
            if (targetIngredientCard == draggedIngredientCard) return false;

            var targetStack = _cardStackSystem.GetStack(targetIngredientCard);

            _cardStackSystem.MergeStacks(draggedIngredientCard, targetStack);
            return true;
        }

        private bool IsOverlapping(IIngredientCard draggedIngredientCard, IIngredientCard other)
        {
            var draggedStack = _cardStackSystem.GetStack(draggedIngredientCard);
            if (draggedStack != null && draggedStack == _cardStackSystem.GetStack(other))
                return false;

            return draggedIngredientCard.Collider.bounds.Intersects(other.Collider.bounds);
        }
    }
}