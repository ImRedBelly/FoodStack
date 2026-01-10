using System;
using Configs;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Cards;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Systems;
using Gameplay.CardsPack;
using Gameplay.Clients;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Services;
using Gameplay.Level.Handlers;
using Gameplay.Level.Systems;
using Gameplay.OrderButton;
using Gameplay.OrderButton.Factory;
using Gameplay.OrderButton.Services;
using Gameplay.Recipes.Configs;
using Gameplay.Recipes.Services;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.Roots
{
    public class GameRoot : DisposableBehaviour<LobbyRoot.Model>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        [SerializeField] private LayerMask _cardLayer;
        [Space] 
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _pauseButton;

        [Space] [Header("UI")] 
        [SerializeField] private DaySliderHandler _daySliderHandler;
        [SerializeField] private BuyCardsPackButton[] _buyCardsPackButtons;

        [Space] [Header("Prefabs")] 
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private ClientCard _clientCardPrefab;
        [SerializeField] private OrderButton _orderButtonPrefab;
        [SerializeField] private CardPack _cardPackPrefab;

        [Space] [Header("Data")] 
        [SerializeField] private RecipeConfig[] _recipeConfigs;

        [Space] [Header("Debug Data")] 
        [SerializeField] private LevelsConfig _levelsConfig;

        private RecipesStorage _recipesStorage;

        private PauseGameSystem _pauseGameSystem;

        private CardDragSystem _cardDragSystem;
        private CardCollisionSystem _cardCollisionSystem;
        private CardStackSystem _cardStackSystem;
        private CardStackMoveSystem _cardStackMoveSystem;

        private CardFactory _cardFactory;
        private ClientFactory _clientFactory;
        private OrderButtonFactory _orderButtonFactory;
        private OrderButtonSelectSystem _orderButtonSelectSystem;
        private ClientOrderSystem _clientOrderSystem;

        protected override void OnInit()
        {
            base.OnInit();
            InitWindows();
            InitFactories();
            InitLevelSystems();


            InitOrderButtonsSystems();
            InitRecipesServices();

            CreateOrderButtons();

            InitCardsSystems();
            InitToolsSystems();
            InitClientsSystems();

            CreateStartCards();
            CreateStartTools();

            InitGame();
        }

        private void InitWindows()
        {
            var pausePopupModel = ActiveModel.WindowResolver.GetPausePopupModel(
                () => { _pauseGameSystem.SetStatePause(false); },
                ActiveModel.OnReload, () => { });

            _pauseButton
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    _pauseGameSystem.SetStatePause(true);
                    ActiveModel.WindowsService.Open(pausePopupModel, false);
                })
                .AddTo(Disposables);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.OnGameAction?.Invoke())
                .AddTo(Disposables);
        }

        private void InitFactories()
        {
            _cardFactory = new CardFactory(_cardPrefab);
            _cardFactory
                .Init()
                .AddTo(Disposables);

            _clientFactory = new ClientFactory(_clientCardPrefab);
            _clientFactory
                .Init()
                .AddTo(Disposables);

            _orderButtonFactory = new OrderButtonFactory(_orderButtonPrefab);
            _clientFactory
                .Init()
                .AddTo(Disposables);
        }

        private void InitOrderButtonsSystems()
        {
            _orderButtonSelectSystem = new OrderButtonSelectSystem(_orderButtonFactory);
            _orderButtonSelectSystem
                .Init()
                .AddTo(Disposables);
        }

        private void InitRecipesServices()
        {
            _recipesStorage = new RecipesStorage(_recipeConfigs);
        }


        private void InitCardsSystems()
        {
            _cardStackMoveSystem = new CardStackMoveSystem();
            _cardStackSystem = new CardStackSystem(_cardStackMoveSystem);

            _cardDragSystem = new CardDragSystem(_camera, _graphicRaycaster, _cardLayer,
                _cardFactory, _cardStackSystem, _cardStackMoveSystem);
            _cardDragSystem
                .Init()
                .AddTo(Disposables);

            _cardCollisionSystem = new CardCollisionSystem(
                _cardFactory,
                _clientFactory,
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

            new CardEligibleFrameStateSystem(_cardFactory, _cardDragSystem, _cardStackSystem)
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

        private void InitToolsSystems()
        {
            CreateDishService createDishService =
                new CreateDishService(_cardCollisionSystem, _cardStackSystem, _cardFactory, _recipesStorage,
                    _pauseGameSystem);
            createDishService
                .Init()
                .AddTo(Disposables);

            SetupStackPositionInToolSystem setupStackPositionInToolSystem =
                new SetupStackPositionInToolSystem(_cardCollisionSystem, _cardStackSystem, _cardStackMoveSystem);
            setupStackPositionInToolSystem
                .Init()
                .AddTo(Disposables);
        }

        private void InitClientsSystems()
        {
            ClientTriggerServiceSystem clientTriggerServiceSystem =
                new ClientTriggerServiceSystem(_clientFactory, _cardFactory, _cardCollisionSystem);
            clientTriggerServiceSystem
                .Init()
                .AddTo(Disposables);

            ClientServiceSystem clientServiceSystem =
                new ClientServiceSystem(clientTriggerServiceSystem, _clientFactory);
            clientServiceSystem
                .Init()
                .AddTo(Disposables);

            _clientOrderSystem = new ClientOrderSystem(_levelsConfig.GetLevelData(SaveUtility.Level), _clientFactory,
                clientTriggerServiceSystem, clientServiceSystem, _orderButtonSelectSystem);
            _clientOrderSystem
                .Init()
                .AddTo(Disposables);
        }


        private void InitLevelSystems()
        {
            _pauseGameSystem = new PauseGameSystem();

            _pauseGameSystem
                .Init()
                .AddTo(Disposables);

            LevelTimerSystem levelTimerSystem = new LevelTimerSystem(_daySliderHandler,
                _levelsConfig.GetLevelData(SaveUtility.Level).LevelTime, _pauseGameSystem);
            levelTimerSystem
                .Init()
                .AddTo(Disposables);
        }


        private void CreateOrderButtons()
        {
            for (int i = 0; i < 2; i++)
            {
                _orderButtonFactory.CreateOrderButton(i);
            }
        }

        private void CreateStartCards()
        {
            int columns = 4;
            float cellSize = 1.2f;

            var startCards = _levelsConfig.GetLevelData(SaveUtility.Level).IngredientCards;

            int rows = Mathf.CeilToInt((float)startCards.Length / columns);

            Vector3 offset = new Vector3(
                (columns - 1) * cellSize * 0.5f,
                (rows - 1) * cellSize * 0.5f,
                0f
            );

            for (int i = 0; i < startCards.Length; i++)
            {
                int x = i % columns;
                int y = i / columns;

                Vector3 position = new Vector3(
                    x * cellSize - offset.x,
                    offset.y - y * cellSize,
                    0f
                );

                _cardFactory.CreateCard(startCards[i], position);
            }
        }

        private void CreateStartTools()
        {
            var tools = _levelsConfig.GetLevelData(SaveUtility.Level).ToolCards;

            float spacing = 1f;
            float startY = 2f;

            int count = tools.Length;

            float offsetX = (count - 1) * spacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float xPos = i * spacing - offsetX;
                Vector3 position = new Vector3(xPos, startY, 0f);

                _cardFactory.CreateCard(tools[i], position);
            }

           // Instantiate(_cardPackPrefab, Vector2.zero, Quaternion.identity);
        }


        private async void InitGame()
        {
            await ShowGameDifficulty();
            InitClients();
        }

        private UniTask ShowGameDifficulty()
        {
            var difficultyType = _levelsConfig.GetLevelData(SaveUtility.Level).DifficultyType;
            //Debug.LogError("Difficulty: " + difficultyType);
            return UniTask.WaitForSeconds(2);
        }

        private void InitClients()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(Constants.TimeAnimationClient))
                .Take(2)
                .Subscribe(_ => { _clientOrderSystem.CreateClient(null); })
                .AddTo(Disposables);
        }
    }
}