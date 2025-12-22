using Core;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Services;
using Gameplay.Recipes.Configs;
using Gameplay.Recipes.Services;
using Gameplay.Tools;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Services;
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

        [Space] [Header("Prefabs")] [SerializeField]
        private IngredientCard ingredientCardPrefab;

        [SerializeField] private ToolCard toolCardPrefab;
        [Space] [SerializeField] private RecipeConfig[] _recipeConfigs;
        [Space] [SerializeField] private Button _quitButton;

        [Space] [Header("Debug Data")] [SerializeField]
        private IngredientConfig[] _startCards;

        [SerializeField] private ToolConfig _panConfig;


        private RecipesStorage _recipesStorage;
        private CardDragService _cardDragService;
        private CardStackService _cardStackService;
        private CardStackMoveService _cardStackMoveService;
        private CardFactory _cardFactory;

        protected override void OnInit()
        {
            base.OnInit();

            InitWindows();
            InitRecipesServices();
            InitCardsServices();
            InitToolsServices();
        }

        private void InitWindows()
        {
            var playConfirmWindow =
                ActiveModel.WindowResolver.GetPlayConfirmWindowModel(ActiveModel.OnGameAction);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.WindowsService.Open(playConfirmWindow, false))
                .AddTo(Disposables);
        }

        private void InitRecipesServices()
        {
            _recipesStorage = new RecipesStorage(_recipeConfigs);
        }

        private void InitCardsServices()
        {
            _cardStackMoveService = new CardStackMoveService();
            _cardStackService = new CardStackService(_cardStackMoveService);

            _cardDragService = new CardDragService(_camera, _cardLayer, _cardStackService, _cardStackMoveService);
            _cardDragService
                .Init()
                .AddTo(Disposables);

            _cardFactory = new CardFactory(ingredientCardPrefab);
            _cardFactory
                .Init()
                .AddTo(Disposables);

            new CardMergeService(_cardFactory, _cardDragService, _cardStackService)
                .Init()
                .AddTo(Disposables);
            
            int columns = 4;
            float cellSize = 1.2f;

            int rows = Mathf.CeilToInt((float)_startCards.Length / columns);

            Vector3 offset = new Vector3(
                (columns - 1) * cellSize * 0.5f,
                (rows - 1) * cellSize * 0.5f,
                0f
            );

            for (int i = 0; i < _startCards.Length; i++)
            {
                int x = i % columns;
                int y = i / columns;

                Vector3 position = new Vector3(
                    x * cellSize - offset.x,
                    offset.y - y * cellSize,
                    0f
                );

                _cardFactory.CreateIngredient(_startCards[i], position);
            }
        }

        private void InitToolsServices()
        {
            ToolFactory toolFactory = new ToolFactory(toolCardPrefab);
            toolFactory
                .Init()
                .AddTo(Disposables);


            ToolCardsDetectService toolCardsDetectService = new ToolCardsDetectService(toolFactory, _cardDragService);
            toolCardsDetectService
                .Init()
                .AddTo(Disposables);

            CreateDishService createDishService =
                new CreateDishService(toolCardsDetectService, _cardStackService, _cardFactory);
            createDishService
                .Init()
                .AddTo(Disposables);

            SetupStackPositionInToolService setupStackPositionInToolService =
                new SetupStackPositionInToolService(toolCardsDetectService, _cardStackService, _cardStackMoveService);
            setupStackPositionInToolService
                .Init()
                .AddTo(Disposables);

            toolFactory.CreateTool(_panConfig, Vector3.up * 2);
        }
    }
}