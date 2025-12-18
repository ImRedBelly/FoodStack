using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Core
{
    public class CardMergeService : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly CardDragService _dragService;
        private readonly List<ICard> _cards = new();

        public CardMergeService(CardFactory cardFactory, CardDragService dragService)
        {
            _cardFactory = cardFactory;
            _dragService = dragService;
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
            foreach (var card in _cards)
            {
                if (card != draggedCard)
                {
                    card.SetStateEligibleFrame(true);
                }
            }
        }

        private void EndDrag(ICard draggedCard)
        {
            foreach (var card in _cards)
            {
                card.SetStateEligibleFrame(false);
            }

            TryMerge(draggedCard);
        }

        private void TryMerge(ICard draggedCard)
        {
            foreach (var card in _cards)
            {
                if (card == draggedCard) continue;

                if (IsOverlapping(draggedCard, card))
                {
                    draggedCard.Transform.SetParent(card.Container);
                    draggedCard.Transform.localPosition = Vector3.zero;
                }
                else
                {
                    draggedCard.Transform.SetParent(null);
                }
            }
        }


        private bool IsOverlapping(ICard draggedCard, ICard other)
        {
            return draggedCard.Collider.bounds.Intersects(other.Collider.bounds);
        }
    }
}