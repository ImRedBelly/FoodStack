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
        private readonly Subject<ICard> _onCardCreated = new();

        private readonly Card _cardPrefab;

        public CardFactory(Card cardPrefab)
        {
            _cardPrefab = cardPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardCreated.AddTo(Disposables);
        }

        public void CreateIngredient(IngredientConfig config, Vector3 position)
        {
            var card = Object.Instantiate(_cardPrefab,  position, Quaternion.identity);
            card.name = config.Name;
            card.Init(new Card.Model(config));

            _onCardCreated?.OnNext(card);
        }
    }
}