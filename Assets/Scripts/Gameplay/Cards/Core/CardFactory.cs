using System;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Interfaces;
using UniRx;
using Object = UnityEngine.Object;

namespace Gameplay.Cards.Core
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

        public void CreateIngredient(IngredientConfig config)
        {
            var card = Object.Instantiate(_cardPrefab);
            card.Init(new Card.Model(config));

            _onCardCreated?.OnNext(card);
        }
    }
}