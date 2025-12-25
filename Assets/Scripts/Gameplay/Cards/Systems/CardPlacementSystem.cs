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

        private readonly List<IIngredientCard> _cards = new();

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

        private void AddCard(IIngredientCard newIngredientCard)
        {
            if (!_cards.Contains(newIngredientCard))
            {
                _cards.Add(newIngredientCard);
            }
        }

        private void RemoveCard(IIngredientCard newIngredientCard)
        {
            if (_cards.Contains(newIngredientCard))
            {
                _cards.Remove(newIngredientCard);
            }
        }

        private void CardDropWithoutMerge(IIngredientCard draggedIngredientCard)
        {
            var dragStack = _cardStackSystem.GetStack(draggedIngredientCard);
            foreach (var otherCard in _cards)
            {
                if (otherCard == draggedIngredientCard) continue;
                if (dragStack.Cards.Contains(otherCard)) continue;

                if (IsIntersecting(draggedIngredientCard, otherCard))
                {
                    Vector3 targetPosition = FindFreePosition(draggedIngredientCard, otherCard);

                    var otherCardStack = _cardStackSystem.GetStack(otherCard);
                    _cardStackMoveSystem.UpdateWorldPositions(otherCardStack, otherCard, targetPosition, Constants.MaxDragSpeed);
                }
            }
        }

        private Vector3 FindFreePosition(
            IIngredientCard draggedCard,
            IIngredientCard otherCard)
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

            //TODO check min max values from camera aspect
            return bestPosition;
        }

        private bool IsIntersecting(IIngredientCard draggedIngredientCard, IIngredientCard otherCard)
        {
            return draggedIngredientCard.Collider.bounds.Intersects(otherCard.Collider.bounds);
        }

        private bool IsIntersectingAtPosition(
            IIngredientCard draggedCard,
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
    }
}