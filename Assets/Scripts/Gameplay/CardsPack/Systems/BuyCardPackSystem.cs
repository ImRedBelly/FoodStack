using System.Collections.Generic;
using Core;
using Gameplay.CardsPack.Configs;
using Gameplay.CardsPack.Types;
using Support;
using UniRx;

namespace Gameplay.CardsPack.Systems
{
    public class BuyCardPackSystem : DisposableClass
    {
        private readonly CardPackFactory _cardPackFactory;
        private readonly BuyCardsPackButton[] _buyCardsPackButtons;
        private readonly IReadOnlyCollection<CardPackConfig> _cardPackConfigs;

        public BuyCardPackSystem(CardPackFactory cardPackFactory, BuyCardsPackButton[] buyCardsPackButtons,
            IReadOnlyCollection<CardPackConfig> cardPackConfigs)
        {
            _cardPackFactory = cardPackFactory;
            _buyCardsPackButtons = buyCardsPackButtons;
            _cardPackConfigs = cardPackConfigs;
        }

        protected override void OnInit()
        {
            base.OnInit();

            foreach (var buyCardPackButton in _buyCardsPackButtons)
            {
                buyCardPackButton.OnClick
                    .SafeSubscribe(AddCardPack)
                    .AddTo(Disposables);
            }
        }

        private void AddCardPack(CardPackType cardPackType)
        {
            foreach (var cardPackConfig in _cardPackConfigs)
            {
                if (cardPackConfig.CardPackType == cardPackType)
                {
                    _cardPackFactory.CreateCardPack(cardPackConfig);
                    break;
                }
            }
        }
    }
}