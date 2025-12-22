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
        public IObservable<IIngredientCard> OnCardCreated => _onCardCreated;
        public IObservable<IIngredientCard> OnCardRemoved => _onCardRemoved;

        private readonly Subject<IIngredientCard> _onCardCreated = new();
        private readonly Subject<IIngredientCard> _onCardRemoved = new();

        private readonly IngredientCard _ingredientCardPrefab;

        public CardFactory(IngredientCard ingredientCardPrefab)
        {
            _ingredientCardPrefab = ingredientCardPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardCreated.AddTo(Disposables);
            _onCardRemoved.AddTo(Disposables);
        }

        public void CreateIngredient(IngredientConfig config, Vector3 position)
        {
            var card = Object.Instantiate(_ingredientCardPrefab, position, Quaternion.identity);
            card.name = config.Name;
            card.Init(new IngredientCard.Model(config));

            _onCardCreated?.OnNext(card);
        }

        public void RemoveIngredient(IIngredientCard ingredientCard)
        {
            _onCardRemoved?.OnNext(ingredientCard);
            Object.Destroy(ingredientCard.Transform.gameObject);
        }
    }
}