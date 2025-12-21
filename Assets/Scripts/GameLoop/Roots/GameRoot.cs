using Core;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Services;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Services;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Tool = Gameplay.Tools.Tool;

namespace GameLoop.Roots
{
    public class GameRoot : DisposableBehaviour<LobbyRoot.Model>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _cardLayer;
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private Tool _toolPrefab;

        [SerializeField] private IngredientConfig[] _startCards;
        [SerializeField] private ToolConfig _panConfig;

        [SerializeField] private Button _quitButton;

        private CardDragService _cardDragService;

        protected override void OnInit()
        {
            base.OnInit();

            InitWindows();
            InitCardsServices();
            InitToolsServices();
        }

        private void InitWindows()
        {
            var playConfirmWindow =
                ActiveModel.WindowResolver.GetPlayConfirmWindowModel(ActiveModel.OnGameAction, false);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.WindowsService.Open(playConfirmWindow, false))
                .AddTo(Disposables);
        }

        private void InitCardsServices()
        {
            CardStackMoveService cardStackMoveService = new CardStackMoveService();
            CardStackService cardStackService = new CardStackService(cardStackMoveService);

            _cardDragService = new CardDragService(_camera, _cardLayer, cardStackService, cardStackMoveService);
            _cardDragService
                .Init()
                .AddTo(Disposables);

            CardFactory cardFactory = new CardFactory(_cardPrefab);
            cardFactory
                .Init()
                .AddTo(Disposables);

            new CardMergeService(cardFactory, _cardDragService, cardStackService)
                .Init()
                .AddTo(Disposables);

            for (int i = 0; i < _startCards.Length; i++)
            {
                cardFactory.CreateIngredient(_startCards[i], new Vector3(i, 0, 0));
            }
        }

        private void InitToolsServices()
        {
            ToolFactory toolFactory = new ToolFactory(_toolPrefab);
            toolFactory
                .Init()
                .AddTo(Disposables);


            ToolCardsDetectService toolCardsDetectService = new ToolCardsDetectService(toolFactory, _cardDragService);
            toolCardsDetectService
                .Init()
                .AddTo(Disposables);
            
            toolFactory.CreateTool(_panConfig, Vector3.up * 2);
        }
    }
}