using System.Collections.Generic;
using Core;
using Support;
using UniRx;

namespace Gameplay.CardsPack.Systems
{
    public class OpenCardPackSystem : DisposableClass
    {
        private readonly CardPackFactory _cardPackFactory;

        private Dictionary<CardPack, int> _cardPacks = new();

        public OpenCardPackSystem(CardPackFactory cardPackFactory)
        {
            _cardPackFactory = cardPackFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardPackFactory.OnCardPackCreated
                .SafeSubscribe(AddCardPack)
                .AddTo(Disposables);
        }

        private void AddCardPack(CardPack cardPack)
        {
            cardPack.OnClick
                .SafeSubscribe(OpenCardPack)
                .AddTo(Disposables);

            _cardPacks.Add(cardPack, 5);
            cardPack.UpdateCountText(_cardPacks[cardPack].ToString());
        }

        private void OpenCardPack(CardPack cardPack)
        {
            if (_cardPacks.TryGetValue(cardPack, out int count))
            {
                _cardPacks[cardPack]--;

                if (_cardPacks[cardPack] == 0)
                {
                    _cardPackFactory.RemoveCardPack(cardPack);
                    _cardPacks.Remove(cardPack);
                }
                else
                {
                    cardPack.UpdateCountText(_cardPacks[cardPack].ToString());
                }
            }
        }
    }
}