using System;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Interfaces;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Cards.Factory
{
    public class CardFactory : DisposableClass
    {
        public IObservable<ICard> OnCardCreated => _onCardCreated;
        public IObservable<ICard> OnCardRemoved => _onCardRemoved;
        public IObservable<ICard> OnToolCreated => _onToolCreated;

        private readonly Subject<ICard> _onCardCreated = new();
        private readonly Subject<ICard> _onCardRemoved = new();
        private readonly Subject<ICard> _onToolCreated = new();

        private readonly Card _cardPrefab;


        public CardFactory(Card cardPrefab)
        {
            _cardPrefab = cardPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardCreated.AddTo(Disposables);
            _onCardRemoved.AddTo(Disposables);
            _onToolCreated.AddTo(Disposables);
        }

        public void CreateIngredient(CardConfig config, Vector3 position)
        {
            var card = Object.Instantiate(_cardPrefab, position, Quaternion.identity);
            card.name = config.Name;
            card.Init(new Card.Model(config));

            _onCardCreated?.OnNext(card);
        }

        public void RemoveIngredient(ICard card)
        {
            _onCardRemoved?.OnNext(card);
            Object.Destroy(card.Transform.gameObject);
        }

        public void CreateTool(CardConfig config, Vector3 position)
        {
            var tool = Object.Instantiate(_cardPrefab, position, Quaternion.identity);
            tool.name = config.Name;
            tool.Init(new Card.Model(config));
            _onToolCreated?.OnNext(tool);
        }
    }
}