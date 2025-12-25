using UnityEngine;
using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardCollisionSystem : DisposableClass
    {
        public IObservable<(IIngredientCard, IIngredientCard)> OnCardCollisionWithCard => _onCardCollisionWithCard;
        public IObservable<(IToolCard, IIngredientCard)> OnCardCollisionWithTool => _onCardCollisionWithTool;
        public IObservable<IIngredientCard> OnCardDropWithoutMerge => _onCardDropWithoutMerge;

        private readonly Subject<(IIngredientCard, IIngredientCard)> _onCardCollisionWithCard = new();
        private readonly Subject<(IToolCard, IIngredientCard)> _onCardCollisionWithTool = new();
        private readonly Subject<IIngredientCard> _onCardDropWithoutMerge = new();

        private readonly CardFactory _cardFactory;
        private readonly ToolFactory _toolFactory;
        private readonly CardDragSystem _dragSystem;
        private readonly CardStackSystem _cardStackSystem;

        private readonly List<IIngredientCard> _cards = new();
        private readonly List<IToolCard> _tools = new();


        public CardCollisionSystem(
            CardFactory cardFactory,
            ToolFactory toolFactory,
            CardDragSystem dragSystem,
            CardStackSystem cardStackSystem)
        {
            _cardFactory = cardFactory;
            _toolFactory = toolFactory;
            _dragSystem = dragSystem;
            _cardStackSystem = cardStackSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardCollisionWithCard.AddTo(Disposables);
            _onCardCollisionWithTool.AddTo(Disposables);
            _onCardDropWithoutMerge.AddTo(Disposables);

            _cardFactory.OnCardCreated
                .SafeSubscribe(AddCard)
                .AddTo(Disposables);

            _cardFactory.OnCardRemoved
                .SafeSubscribe(RemoveCard)
                .AddTo(Disposables);

            _toolFactory.OnCardCreated
                .SafeSubscribe(AddTool)
                .AddTo(Disposables);

            _dragSystem.OnEndDrag
                .SafeSubscribe(EndDrag)
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

        private void EndDrag(IIngredientCard draggedIngredientCard)
        {
            foreach (var tool in _tools)
            {
                if (IsOverlapping(draggedIngredientCard, tool))
                {
                    _onCardCollisionWithTool.OnNext((tool, draggedIngredientCard));
                    return;
                }
            }

            var dragStack = _cardStackSystem.GetStack(draggedIngredientCard);
            foreach (var card in _cards)
            {
                if (draggedIngredientCard == card) continue;
                if (dragStack.Cards.Contains(card)) continue;
                if (IsOverlapping(draggedIngredientCard, card))
                {
                    _onCardCollisionWithCard.OnNext((draggedIngredientCard, card));
                    return;
                }
            }

            _onCardDropWithoutMerge?.OnNext(draggedIngredientCard);
        }

        private bool IsOverlapping(IIngredientCard draggedIngredientCard, IIngredientCard otherCard)
        {
            var draggedBounds = draggedIngredientCard.Collider.bounds;
            var otherBounds = otherCard.Collider.bounds;

            if (!otherBounds.Intersects(draggedBounds)) return false;

            var distance = Vector3.Distance(draggedBounds.center, otherBounds.center);
            return distance < Constants.MinCollisionDistance;
        }

        private bool IsOverlapping(IIngredientCard draggedIngredientCard, IToolCard toolCard)
        {
            var draggedBounds = draggedIngredientCard.Collider.bounds;
            var toolBounds = toolCard.Collider.bounds;

            if (!toolBounds.Intersects(draggedBounds)) return false;

            var distance = Vector3.Distance(draggedBounds.center, toolBounds.center);
            return distance < Constants.MinCollisionDistance;
        }
    }
}