using UnityEngine;
using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardCollisionSystem : DisposableClass
    {
        public IObservable<(ICard, ICard)> OnCardCollisionWithCard => _onCardCollisionWithCard;
        public IObservable<(IClientCard, ICard)> OnCardCollisionWithClient => _onCardCollisionWithClient;
        public IObservable<ICard> OnCardDropWithoutMerge => _onCardDropWithoutMerge;

        private readonly Subject<(ICard, ICard)> _onCardCollisionWithCard = new();
        private readonly Subject<(IClientCard, ICard)> _onCardCollisionWithClient = new();
        private readonly Subject<ICard> _onCardDropWithoutMerge = new();

        private readonly CardFactory _cardFactory;
        private readonly ClientFactory _clientFactory;
        private readonly CardDragSystem _dragSystem;
        private readonly CardStackSystem _cardStackSystem;

        private readonly List<ICard> _cards = new();
        private readonly List<IClientCard> _clients = new();


        public CardCollisionSystem(
            CardFactory cardFactory,
            ClientFactory clientFactory,
            CardDragSystem dragSystem,
            CardStackSystem cardStackSystem)
        {
            _cardFactory = cardFactory;
            _clientFactory = clientFactory;
            _dragSystem = dragSystem;
            _cardStackSystem = cardStackSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardCollisionWithCard.AddTo(Disposables);
            _onCardCollisionWithClient.AddTo(Disposables);
            _onCardDropWithoutMerge.AddTo(Disposables);

            _cardFactory.OnCardCreated
                .SafeSubscribe(AddCard)
                .AddTo(Disposables);

            _cardFactory.OnCardRemoved
                .SafeSubscribe(RemoveCard)
                .AddTo(Disposables);

            _clientFactory.OnClientCreated
                .SafeSubscribe(AddClient)
                .AddTo(Disposables);

            _clientFactory.OnClientRemoved
                .SafeSubscribe(RemoveClient)
                .AddTo(Disposables);

            _dragSystem.OnEndDrag
                .SafeSubscribe(EndDrag)
                .AddTo(Disposables);
        }

        private void AddCard(ICard newCard)
        {
            if (!_cards.Contains(newCard))
            {
                _cards.Add(newCard);
            }
        }

        private void RemoveCard(ICard card)
        {
            if (_cards.Contains(card))
            {
                _cards.Remove(card);
            }
        }

        private void AddClient((IClientCard newClientCard, CardConfig ingredientConfig) data)
        {
            if (!_clients.Contains(data.newClientCard))
            {
                _clients.Add(data.newClientCard);
            }
        }

        private void RemoveClient(IClientCard clientCard)
        {
            if (_clients.Contains(clientCard))
            {
                _clients.Remove(clientCard);
            }
        }

        private void EndDrag(ICard draggedIngredientCard)
        {
            var dragStack = _cardStackSystem.GetStack(draggedIngredientCard);
            foreach (var card in _cards)
            {
                if (draggedIngredientCard == card) continue;
                if (dragStack.Cards.Contains(card)) continue;
                if (IsOverlapping(draggedIngredientCard, card))
                {
                    _onCardCollisionWithCard.OnNext((draggedIngredientCard, card));
                }
            }

            foreach (var client in _clients)
            {
                if (IsOverlapping(draggedIngredientCard, client))
                {
                    _onCardCollisionWithClient.OnNext((client, draggedIngredientCard));
                    break;
                }
            }

            _onCardDropWithoutMerge?.OnNext(draggedIngredientCard);
        }

        private bool IsOverlapping(ICard draggedIngredientCard, ICard otherCard)
        {
            var draggedBounds = draggedIngredientCard.Collider.bounds;
            var otherBounds = otherCard.Collider.bounds;

            if (!otherBounds.Intersects(draggedBounds)) return false;

            var distance = Vector3.Distance(draggedBounds.center, otherBounds.center);
            return distance < Constants.MinCollisionDistance;
        }

        private bool IsOverlapping(ICard draggedIngredientCard, IClientCard clientCard)
        {
            var draggedBounds = draggedIngredientCard.Collider.bounds;
            var toolBounds = clientCard.Collider.bounds;

            if (!toolBounds.Intersects(draggedBounds)) return false;

            var distance = Vector3.Distance(draggedBounds.center, toolBounds.center);
            return distance < Constants.MinCollisionDistance;
        }
    }
}