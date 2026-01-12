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

        private readonly Dictionary<Collider2D, ICard> _cardByCollider = new();
        private readonly Collider2D[] _overlapBuffer = new Collider2D[64];
        private readonly int _cardsLayerMask = ~0;


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
                _cards.Add(newIngredientCard);

            if (newIngredientCard?.Collider != null)
                _cardByCollider[newIngredientCard.Collider] = newIngredientCard;
        }

        private void RemoveCard(ICard newIngredientCard)
        {
            if (_cards.Contains(newIngredientCard))
                _cards.Remove(newIngredientCard);

            if (newIngredientCard?.Collider != null)
                _cardByCollider.Remove(newIngredientCard.Collider);
        }


        public void CardDropWithoutMerge(ICard sourceCard)
        {
            var queue = new Queue<CardStack>();
            var inQueue = new HashSet<CardStack>();

            void EnqueueStack(CardStack s)
            {
                if (s == null) return;
                if (inQueue.Add(s))
                    queue.Enqueue(s);
            }

            void EnqueueStackAndNeighbours(CardStack movedStack)
            {
                if (movedStack == null) return;

                EnqueueStack(movedStack);

                foreach (var neighbourStack in GetIntersectingStacks(movedStack))
                {
                    if (neighbourStack != null && neighbourStack != movedStack)
                        EnqueueStack(neighbourStack);
                }
            }

            EnqueueStack(_cardStackSystem.GetStack(sourceCard));

            const int maxIterations = 250;
            int iterations = 0;

            while (queue.Count > 0 && iterations++ < maxIterations)
            {
                var stack = queue.Dequeue();
                inQueue.Remove(stack);

                if (stack == null || stack.Cards == null || stack.Cards.Count == 0)
                    continue;

                var anchor = stack.Cards[^1];
                var anchorBounds = anchor.Collider.bounds;

                if (!IsWithinZone(anchorBounds.center))
                {
                    var clamped = ClampToZone(anchorBounds.center, stack.Cards.Count);

                    _cardStackMoveSystem.UpdateWorldPositions(
                        stack,
                        anchor,
                        clamped,
                        Constants.MaxDragSpeed);

                    Physics2D.SyncTransforms();

                    EnqueueStackAndNeighbours(stack);
                    continue;
                }

                bool movedSomeone = false;

                foreach (var cardInStack in stack.Cards)
                {
                    var b = cardInStack.Collider.bounds;
                    int hitCount = Physics2D.OverlapBoxNonAlloc(b.center, b.size, 0f, _overlapBuffer, _cardsLayerMask);

                    for (int i = 0; i < hitCount; i++)
                    {
                        var hit = _overlapBuffer[i];
                        if (hit == null) continue;

                        if (!_cardByCollider.TryGetValue(hit, out var otherCard) || otherCard == null)
                            continue;

                        if (stack.Cards.Contains(otherCard))
                            continue;

                        var otherStack = _cardStackSystem.GetStack(otherCard);
                        if (otherStack == null || otherStack.Cards.Count == 0)
                            continue;

                        var target = FindFreePositionForCard(otherCard, otherStack);
                        target = ClampToZone(target, otherStack.Cards.Count);

                        _cardStackMoveSystem.UpdateWorldPositions(
                            otherStack,
                            otherCard,
                            target,
                            Constants.MaxDragSpeed);

                        Physics2D.SyncTransforms();

                        movedSomeone = true;

                        EnqueueStackAndNeighbours(otherStack);
                    }
                }

                if (movedSomeone)
                    EnqueueStackAndNeighbours(stack);
            }
        }

        private Vector3 FindFreePositionForCard(ICard cardToMove, CardStack movingStack)
        {
            var origin = cardToMove.Transform.position;
            var size = cardToMove.Collider.bounds.size;

            float step = Random.Range(0.09f, 0.11f);
            int maxSteps = 50;

            Vector3 best = origin;
            float bestDist = float.MaxValue;

            var allowed = new HashSet<Collider2D>();
            foreach (var c in movingStack.Cards)
                allowed.Add(c.Collider);

            for (int x = -maxSteps; x <= maxSteps; x++)
            {
                for (int y = -maxSteps; y <= maxSteps; y++)
                {
                    if (x == 0 && y == 0) continue;

                    var candidate = origin + new Vector3(x * step, y * step, 0);

                    if (IsPositionFree(candidate, size, allowed))
                    {
                        float d = (x * x + y * y);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            best = candidate;
                        }
                    }
                }
            }

            return best;
        }

        private bool IsPositionFree(Vector3 center, Vector3 size, HashSet<Collider2D> allowed)
        {
            var hits = Physics2D.OverlapBoxAll(center, size, 0f);

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                if (!allowed.Contains(hit))
                    return false;
            }

            return true;
        }

        private IEnumerable<CardStack> GetIntersectingStacks(CardStack movedStack)
        {
            var result = new HashSet<CardStack>();

            foreach (var card in movedStack.Cards)
            {
                var b = card.Collider.bounds;
                int hitCount = Physics2D.OverlapBoxNonAlloc(b.center, b.size, 0f, _overlapBuffer, _cardsLayerMask);

                for (int i = 0; i < hitCount; i++)
                {
                    var hit = _overlapBuffer[i];
                    if (hit == null) continue;

                    if (!_cardByCollider.TryGetValue(hit, out var otherCard) || otherCard == null)
                        continue;

                    if (movedStack.Cards.Contains(otherCard))
                        continue;

                    var otherStack = _cardStackSystem.GetStack(otherCard);
                    if (otherStack != null && otherStack != movedStack)
                        result.Add(otherStack);
                }
            }

            return result;
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