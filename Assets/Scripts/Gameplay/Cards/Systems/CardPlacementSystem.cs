using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Systems
{
    public class CardPlacementSystem : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardStackMoveSystem _cardStackMoveSystem;

        private readonly List<ICard> _cards = new();
        private readonly Rect _placementZone = new(-2.4f, -2.7f, 4.8f, 4.9f);

        public CardPlacementSystem(
            CardFactory cardFactory,
            CardCollisionSystem cardCollisionSystem,
            CardStackSystem cardStackSystem,
            CardStackMoveSystem cardStackMoveSystem)
        {
            _cardFactory = cardFactory;
            _cardCollisionSystem = cardCollisionSystem;
            _cardStackSystem = cardStackSystem;
            _cardStackMoveSystem = cardStackMoveSystem;
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

            _cardCollisionSystem.OnCardDropWithoutMerge
                .SafeSubscribe(CardDropWithoutMerge)
                .AddTo(Disposables);
        }

        private void AddCard(ICard newIngredientCard)
        {
            if (!_cards.Contains(newIngredientCard))
            {
                _cards.Add(newIngredientCard);
            }
        }

        private void RemoveCard(ICard newIngredientCard)
        {
            if (_cards.Contains(newIngredientCard))
            {
                _cards.Remove(newIngredientCard);
            }
        }

        private void CardDropWithoutMerge(ICard draggedIngredientCard)
        {
            var dragStack = _cardStackSystem.GetStack(draggedIngredientCard);
            var draggedBounds = draggedIngredientCard.Collider.bounds;
            if (!IsWithinZone(draggedBounds.center))
            {
                var clampedPosition = ClampToZone(draggedBounds.center, dragStack.Cards.Count);
                var draggedStack = _cardStackSystem.GetStack(draggedIngredientCard);
                _cardStackMoveSystem.UpdateWorldPositions(
                    draggedStack,
                    draggedIngredientCard,
                    clampedPosition,
                    Constants.MaxDragSpeed);
                return;
            }

            foreach (var otherCard in _cards)
            {
                if (otherCard == draggedIngredientCard) continue;
                if (dragStack.Cards.Contains(otherCard)) continue;

                if (IsIntersecting(draggedIngredientCard, otherCard))
                {
                    var otherCardStack = _cardStackSystem.GetStack(otherCard);

                    Vector3 targetPosition = FindFreePosition(draggedIngredientCard, otherCard);
                    targetPosition = ClampToZone(targetPosition, otherCardStack.Cards.Count);
                    
                    _cardStackMoveSystem.UpdateWorldPositions(otherCardStack, otherCard, targetPosition, Constants.MaxDragSpeed);
                }
            }
        }

        private Vector3 FindFreePosition(
            ICard draggedCard,
            ICard otherCard)
        {
            var origin = otherCard.Transform.position;
            var bounds = otherCard.Collider.bounds;

            float step = 0.1f;
            int maxSteps = 50;

            Vector3 bestPosition = origin;
            float bestDistance = float.MaxValue;

            for (int x = -maxSteps; x <= maxSteps; x++)
            {
                for (int y = -maxSteps; y <= maxSteps; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var offset = new Vector3(x * step, y * step, 0);
                    var candidate = origin + offset;

                    if (!IsIntersectingAtPosition(draggedCard, candidate, bounds))
                    {
                        float distance = offset.sqrMagnitude;
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestPosition = candidate;
                        }
                    }
                }
            }

            return bestPosition;
        }

        private bool IsIntersecting(ICard draggedIngredientCard, ICard otherCard)
        {
            return draggedIngredientCard.Collider.bounds.Intersects(otherCard.Collider.bounds);
        }

        private bool IsIntersectingAtPosition(
            ICard draggedCard,
            Vector3 position,
            Bounds bounds)
        {
            var size = bounds.size;
            var center = position;

            var hits = Physics2D.OverlapBoxAll(center, size, 0f);

            foreach (var hit in hits)
            {
                if (hit == null)
                    continue;

                if (draggedCard.Collider == hit)
                    return true;
            }

            return false;
        }

        private bool IsWithinZone(Vector3 position)
        {
            return _placementZone.Contains(new Vector2(position.x, position.y));
        }

        private Vector3 ClampToZone(Vector3 position, int countCardsInStack)
        {
            float offsetYMin = (countCardsInStack - 1) * 0.2f;

            float x = Mathf.Clamp(position.x, _placementZone.xMin, _placementZone.xMax);
            float y = Mathf.Clamp(position.y, _placementZone.yMin + offsetYMin, _placementZone.yMax);
            return new Vector3(x, y, 0);
        }
    }
}