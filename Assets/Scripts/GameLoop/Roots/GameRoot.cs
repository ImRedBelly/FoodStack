using Core;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Systems;
using Gameplay.Clients;
using Gameplay.Clients.Configs;
using Gameplay.Clients.Factory;
using Gameplay.Recipes.Configs;
using Gameplay.Recipes.Services;
using Gameplay.Tools;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Systems;
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
        [SerializeField] private ClientCard clientCardPrefab;
        [Space] [SerializeField] private RecipeConfig[] _recipeConfigs;
        [Space] [SerializeField] private Button _quitButton;

        [Space] [Header("Debug Data")] [SerializeField]
        private IngredientConfig[] _startCards;

        [SerializeField] private ToolConfig _panConfig;
        [SerializeField] private ClientConfig[] _clientConfigs;


        private RecipesStorage _recipesStorage;

        private CardDragSystem _cardDragSystem;
        private CardCollisionSystem _cardCollisionSystem;
        private CardStackSystem _cardStackSystem;
        private CardStackMoveSystem _cardStackMoveSystem;

        private CardFactory _cardFactory;
        private ToolFactory _toolFactory;
        private ClientFactory _clientFactory;

        protected override void OnInit()
        {
            base.OnInit();
            InitWindows();
            InitFactories();

            InitRecipesServices();
            InitCardsServices();
            InitToolsServices();

            CreateStartCards();
            CreateStartTools();
            CreateStartClients();
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

        private void InitFactories()
        {
            _cardFactory = new CardFactory(ingredientCardPrefab);
            _cardFactory
                .Init()
                .AddTo(Disposables);

            _toolFactory = new ToolFactory(toolCardPrefab);
            _toolFactory
                .Init()
                .AddTo(Disposables);

            _clientFactory = new ClientFactory(clientCardPrefab);
            _clientFactory
                .Init()
                .AddTo(Disposables);
        }


        private void InitRecipesServices()
        {
            _recipesStorage = new RecipesStorage(_recipeConfigs);
        }

        private void InitCardsServices()
        {
            _cardStackMoveSystem = new CardStackMoveSystem();
            _cardStackSystem = new CardStackSystem(_cardStackMoveSystem);

            _cardDragSystem = new CardDragSystem(_camera, _cardLayer, _cardStackSystem, _cardStackMoveSystem);
            _cardDragSystem
                .Init()
                .AddTo(Disposables);

            _cardCollisionSystem = new CardCollisionSystem(
                _cardFactory,
                _toolFactory,
                _cardDragSystem,
                _cardStackSystem);

            _cardCollisionSystem
                .Init()
                .AddTo(Disposables);

            new CardMergeSystem(_cardCollisionSystem, _cardDragSystem, _cardStackSystem)
                .Init()
                .AddTo(Disposables);

            new CardSortingOrderSystem(_cardFactory)
                .Init()
                .AddTo(Disposables);

            new CardEligibleFrameStateSystem(_cardFactory, _toolFactory, _cardDragSystem, _cardStackSystem)
                .Init()
                .AddTo(Disposables);

            CardPlacementSystem cardPlacementSystem = new CardPlacementSystem(
                _cardFactory,
                _cardCollisionSystem,
                _cardStackSystem,
                _cardStackMoveSystem);

            cardPlacementSystem
                .Init()
                .AddTo(Disposables);
        }


        private void InitToolsServices()
        {
            CreateDishService createDishService =
                new CreateDishService(_cardCollisionSystem, _cardStackSystem, _cardFactory);
            createDishService
                .Init()
                .AddTo(Disposables);

            SetupStackPositionInToolSystem setupStackPositionInToolSystem =
                new SetupStackPositionInToolSystem(_cardCollisionSystem, _cardStackSystem, _cardStackMoveSystem);
            setupStackPositionInToolSystem
                .Init()
                .AddTo(Disposables);
        }

        private void CreateStartCards()
        {
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

        private void CreateStartTools()
        {
            _toolFactory.CreateTool(_panConfig, Vector3.up * 2);
        }


        private void CreateStartClients()
        {
            for (int i = 0; i < _clientConfigs.Length; i++)
            {
                _clientFactory.CreateClient(_clientConfigs[i], new Vector3(i == 0 ? -1 : 1, 3.67f, 0f));
            }
        }
    }
}