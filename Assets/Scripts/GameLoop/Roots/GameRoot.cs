using Core;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Services.Cards;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.Roots
{
    public class GameRoot : DisposableBehaviour<LobbyRoot.Model>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _cardLayer;
        [SerializeField] private Card _cardPrefab;

        [SerializeField] private IngredientConfig[] _startCards;

        [SerializeField] private Button _quitButton;

        protected override void OnInit()
        {
            base.OnInit();

            var playConfirmWindow =
                ActiveModel.WindowResolver.GetPlayConfirmWindowModel(ActiveModel.OnGameAction, false);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.WindowsService.Open(playConfirmWindow, false))
                .AddTo(Disposables);

            CardStackMoveService cardStackMoveService = new CardStackMoveService();
            CardStackService cardStackService = new CardStackService(cardStackMoveService);

            CardDragService cardDragService = new CardDragService(_camera, _cardLayer, cardStackService, cardStackMoveService);
            cardDragService
                .Init()
                .AddTo(Disposables);

            CardFactory cardFactory = new CardFactory(_cardPrefab);
            cardFactory
                .Init()
                .AddTo(Disposables);

            new CardMergeService(cardFactory, cardDragService, cardStackService)
                .Init()
                .AddTo(Disposables);

            for (int i = 0; i < _startCards.Length; i++)
            {
                cardFactory.CreateIngredient(_startCards[i], new Vector3(i, 0, 0));
            }
        }
    }
}