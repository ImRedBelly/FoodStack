using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.CardsPack;
using Support;
using UniRx;

namespace Gameplay.Level.Systems
{
    public class CardSellSystem : DisposableClass
    {
        private readonly SellCardPanel _sellCardPanel;
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardFactory _cardFactory;
        private readonly CardStackSystem _cardStackSystem;

        public CardSellSystem(SellCardPanel sellCardPanel, 
            CardCollisionSystem cardCollisionSystem,
            CardFactory cardFactory,
            CardStackSystem cardStackSystem)
        {
            _sellCardPanel = sellCardPanel;
            _cardCollisionSystem = cardCollisionSystem;
            _cardFactory = cardFactory;
            _cardStackSystem = cardStackSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardCollisionSystem.OnCardDropWithoutMerge
                .SafeSubscribe(CardDropWithoutMerge)
                .AddTo(Disposables);
        }

        private void CardDropWithoutMerge(ICard card)
        {
            if (IsIntersecting(card))
            {
                _cardFactory.RemoveCard(card);
                _cardStackSystem.RemoveCardFromStack(card);
                SaveUtility.Money.Value += card.CardConfig.SellProfit;
            }
        }

        private bool IsIntersecting(ICard card)
        {
            return _sellCardPanel.Collider.bounds.Intersects(card.Collider.bounds);
        }
    }
}