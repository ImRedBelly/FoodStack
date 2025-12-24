using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Systems
{
    public class CardPlacementSystem : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly ToolFactory _toolFactory;
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardStackMoveSystem _cardStackMoveSystem;
        private readonly CardStackSystem _cardStackSystem;

        private readonly List<IIngredientCard> _cards = new();
        private readonly List<IToolCard> _tools = new();

        public CardPlacementSystem(CardFactory cardFactory, ToolFactory toolFactory, CardCollisionSystem cardCollisionSystem,
            CardStackMoveSystem cardStackMoveSystem, CardStackSystem cardStackSystem)
        {
            _cardFactory = cardFactory;
            _toolFactory = toolFactory;
            _cardCollisionSystem = cardCollisionSystem;
            _cardStackMoveSystem = cardStackMoveSystem;
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

            _toolFactory.OnCardCreated
                .SafeSubscribe(AddTool)
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

        private void AddTool(IToolCard newToolCard)
        {
            if (!_tools.Contains(newToolCard))
            {
                _tools.Add(newToolCard);
            }
        }

        private void CardDropWithoutMerge(IIngredientCard draggedIngredientCard)
        {
            var draggedCollider = draggedIngredientCard.Collider;
            if (draggedCollider == null)
                return;

            foreach (var otherCard in _cards)
            {
                if (otherCard == draggedIngredientCard)
                    continue;

                var otherCollider = otherCard.Collider;
                if (otherCollider == null)
                    continue;

                if (draggedCollider.bounds.Intersects(otherCollider.bounds))
                {
                    MoveToNearestFreePosition(draggedIngredientCard);
                    break;
                }
            }
        }

        private void MoveToNearestFreePosition(IIngredientCard card)
        {
            var originalPosition = card.Transform.position;
            var step = 0.5f;
            var maxRadius = 10;

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        var offset = new Vector3(x * step, y * step, 0);
                        var candidatePosition = originalPosition + offset;

                        card.Transform.position = candidatePosition;

                        if (!IsIntersecting(card))
                        {
                            return;
                        }
                    }
                }
            }

            
            _cardStackMoveSystem.UpdateWorldPositions(_cardStackSystem.GetStack(card), originalPosition, Constants.MaxDragSpeed);
            //card.Transform.position = originalPosition;
        }

        private bool IsIntersecting(IIngredientCard card)
        {
            var collider = card.Collider;
            if (collider == null)
                return false;

            foreach (var otherCard in _cards)
            {
                if (otherCard == card)
                    continue;

                var otherCollider = otherCard.Collider;
                if (otherCollider == null)
                    continue;

                if (collider.bounds.Intersects(otherCollider.bounds))
                {
                    return true;
                }
            }

            return false;
        }
    }
}