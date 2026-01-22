using System;
using Core;
using Gameplay.CardsPack.Configs;
using UniRx;
using Object = UnityEngine.Object;

namespace Gameplay.CardsPack.Systems
{
    public class CardPackFactory : DisposableClass
    {
        public IObservable<CardPack> OnCardPackCreated => _onCardPackCreated;

        private readonly Subject<CardPack> _onCardPackCreated = new();

        private readonly CardPack _cardPackPrefab;

        public CardPackFactory(CardPack cardPackPrefab)
        {
            _cardPackPrefab = cardPackPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onCardPackCreated.AddTo(Disposables);
        }

        public void CreateCardPack(CardPackConfig cardPackType)
        {
            var newCardPack = Object.Instantiate(_cardPackPrefab);
            newCardPack.Init(new CardPack.Model(cardPackType));

            _onCardPackCreated?.OnNext(newCardPack);
        }

        public void RemoveCardPack(CardPack cardPack)
        {
            Object.Destroy(cardPack.gameObject);
        }
    }
}