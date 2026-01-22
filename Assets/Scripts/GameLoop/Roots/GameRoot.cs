using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Cards;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Systems;
using Gameplay.Cards.Types;
using Gameplay.CardsPack;
using Gameplay.CardsPack.Systems;
using Gameplay.Clients;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Systems;
using Gameplay.GameCamera.Systems;
using Gameplay.Level.Handlers;
using Gameplay.Level.Systems;
using Gameplay.OrderButton;
using Gameplay.OrderButton.Factory;
using Gameplay.OrderButton.Services;
using Gameplay.Recipes.Configs;
using Gameplay.Recipes.Systems;
using Support;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.Roots
{
    public class GameRoot : DisposableBehaviour<LobbyRoot.Model>
    {  
        [SerializeField] private Transform _windowsAnchor;
        [Space]
        [SerializeField] private Camera _camera;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        [SerializeField] private LayerMask _cardLayer;
        [SerializeField] private LayerMask _orderButtonLayer;
        [Space] 
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _pauseButton;

        [Space] [Header("UI")] 
        [SerializeField] private DaySliderHandler _daySliderHandler;
        [SerializeField] private LevelTargetHandler _levelTargetHandler;
        [SerializeField] private ServiceSuccessHandler _serviceSuccessHandler;
        [SerializeField] private BuyCardsPackButton[] _buyCardsPackButtons;
        [SerializeField] private SellCardPanel _sellCardPanel;

        [Space] [Header("Prefabs")] 
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private ClientCard _clientCardPrefab;
        [SerializeField] private OrderButton _orderButtonPrefab;
        [SerializeField] private CardPack _cardPackPrefab;

        [Space] [Header("Data")] 
        [SerializeField] private RecipesConfig _recipesConfig;
        [SerializeField] private ClientsConfig _clientsConfig;
        [SerializeField] private LevelsConfig _levelsConfig;
        [SerializeField] private CardPacksConfig _cardPacksConfig;

        private RecipesStorage _recipesStorage;
        private PauseGameSystem _pauseGameSystem;
        private LevelTimerSystem _levelTimerSystem;

        private CardDragSystem _cardDragSystem;
        private CardCollisionSystem _cardCollisionSystem;
        private CardStackSystem _cardStackSystem;
        private CardStackMoveSystem _cardStackMoveSystem;

        private CardFactory _cardFactory;
        private ClientFactory _clientFactory;
        private CardPlacementSystem _cardPlacementSystem;
        private OrderButtonFactory _orderButtonFactory;
        private CardPackFactory _cardPackFactory;
        
        private OrderButtonSelectSystem _orderButtonSelectSystem;
        private ClientServiceSystem _clientServiceSystem;
        private ClientOrderSystem _clientOrderSystem;

        protected override void OnInit()
        {
            base.OnInit();
            
            ActiveModel.WindowsService.SetupAnchor(_windowsAnchor);
            
            InitWindows();
            InitFactories();
            InitLevelSystems();
            
            InitRecipesServices();

            InitCardsSystems();
            InitToolsSystems();
            InitClientsSystems();
            InitOrderButtonsSystems();
            InitCardPackSystems();
            InitLevelTargetSystems();

            CreateStartCards();
            CreateStartTools();
            CreateOrderButtons();
            
            InitGameCamera();
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

            _cardPackFactory = new CardPackFactory(_cardPackPrefab);
            _clientFactory
                .Init()
                .AddTo(Disposables);
        }

        private void InitOrderButtonsSystems()
        {
           var orderButtonClickSystem = new OrderButtonClickSystem(_camera, _orderButtonLayer, _graphicRaycaster);
           orderButtonClickSystem
                .Init()
                .AddTo(Disposables);

           var openOrderInfoPopupSystem = new OpenOrderInfoPopupSystem(
               ActiveModel.WindowsService,
               ActiveModel.WindowResolver,
               _clientOrderSystem, 
               orderButtonClickSystem,
               _pauseGameSystem,
               _recipesConfig.RecipeConfigs);
           
           openOrderInfoPopupSystem
               .Init()
               .AddTo(Disposables);
        }

        private void InitRecipesServices()
        {
            _recipesStorage = new RecipesStorage(_recipesConfig.RecipeConfigs);
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

            _cardPlacementSystem = new CardPlacementSystem(
                _cardFactory,
                _cardCollisionSystem,
                _cardStackSystem,
                _cardStackMoveSystem);

            _cardPlacementSystem
                .Init()
                .AddTo(Disposables);
        }

        private void InitToolsSystems()
        {
            CreateDishService createDishService =
                new CreateDishService(_cardPlacementSystem, _cardStackSystem, _cardFactory, _recipesStorage,
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
            ClientGenerateOrderSystem clientGenerateOrderSystem = new ClientGenerateOrderSystem( _clientsConfig.ClientsConfigs, _recipesConfig.RecipeConfigs);
            clientGenerateOrderSystem
                .Init()
                .AddTo(Disposables);
            
            _orderButtonSelectSystem = new OrderButtonSelectSystem(_orderButtonFactory);
            _orderButtonSelectSystem
                .Init()
                .AddTo(Disposables);
            
            ClientTriggerServiceSystem clientTriggerServiceSystem =
                new ClientTriggerServiceSystem(_clientFactory, _cardFactory, _cardCollisionSystem, _cardStackSystem);
            clientTriggerServiceSystem
                .Init()
                .AddTo(Disposables);

            _clientServiceSystem =
                new ClientServiceSystem(clientTriggerServiceSystem, _clientFactory, _serviceSuccessHandler);
            _clientServiceSystem
                .Init()
                .AddTo(Disposables);

            _clientOrderSystem = new ClientOrderSystem(_clientFactory,
                clientTriggerServiceSystem, _clientServiceSystem, _orderButtonSelectSystem, clientGenerateOrderSystem);
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

            _levelTimerSystem = new LevelTimerSystem(_daySliderHandler,
                _levelsConfig.GetLevelData(SaveUtility.Level).LevelTime, _pauseGameSystem);
            _levelTimerSystem
                .Init()
                .AddTo(Disposables);
        }
        
        private void InitCardPackSystems()
        {
            foreach (var cardsPackButton in _buyCardsPackButtons)
            {
                cardsPackButton
                    .Init(new BuyCardsPackButton.Model(_cardPacksConfig.CardPackConfigs))
                    .AddTo(Disposables);
            }
            
            BuyCardPackSystem buyCardPackSystem = new BuyCardPackSystem(_cardPackFactory, _buyCardsPackButtons, _cardPacksConfig.CardPackConfigs);
            buyCardPackSystem
                .Init()
                .AddTo(Disposables);
            
            OpenCardPackSystem openCardPackSystem = new OpenCardPackSystem(_cardPackFactory, _cardFactory, _cardPlacementSystem, _cardStackSystem);
            openCardPackSystem
                .Init()
                .AddTo(Disposables);
        }


        private void InitLevelTargetSystems()
        {
            UpdateLevelTargetSystem updateLevelTargetSystem = new UpdateLevelTargetSystem(
                _levelsConfig.GetLevelData(SaveUtility.Level).LevelTarget,_levelsConfig.GetLevelData(SaveUtility.Level).StartMoney,
                _clientServiceSystem, _levelTargetHandler, _recipesConfig.RecipeConfigs);
            updateLevelTargetSystem
                .Init()
                .AddTo(Disposables);

            LevelFinishSystem levelFinishSystem = new LevelFinishSystem(
                _levelsConfig.GetLevelData(SaveUtility.Level).LevelTarget,
                updateLevelTargetSystem, _levelTimerSystem, _clientServiceSystem,
                ActiveModel.WindowsService, ActiveModel.WindowResolver, ActiveModel.OnGameAction);
            levelFinishSystem
                .Init()
                .AddTo(Disposables);
            
            CardSellSystem cardSellSystem = new CardSellSystem(_sellCardPanel, _cardCollisionSystem, 
                _cardFactory, _cardStackSystem);
            cardSellSystem
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
            List<CardConfig> tools = new List<CardConfig>();
            foreach (var recipeConfig in _recipesStorage.GetOpenRecipes())
            {
                foreach (var cardConfig in recipeConfig.Ingredients)
                {
                    if (cardConfig.CardType == CardType.Tool && !tools.Contains(cardConfig))
                    {
                        tools.Add(cardConfig);
                    }
                }
            }

            float spacing = 1f;
            float startY = 2f;

            int count = tools.Count;

            float offsetX = (count - 1) * spacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float xPos = i * spacing - offsetX;
                Vector3 position = new Vector3(xPos, startY, 0f);

                _cardFactory.CreateCard(tools[i], position);
            }
        }

        private void InitGameCamera()
        {
            OrthoCameraScaleSystem orthoCameraScaleSystem = new OrthoCameraScaleSystem(_camera);
            orthoCameraScaleSystem
                .Init()
                .AddTo(Disposables);
        }

        private async void InitGame()
        {
            _serviceSuccessHandler.Init();
            await ShowGameDifficulty();
            InitClients();
        }

        private UniTask ShowGameDifficulty()
        {
            var difficultyType = _levelsConfig.GetLevelData(SaveUtility.Level).DifficultyType;
            return UniTask.CompletedTask;
        }

        private void InitClients()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(Constants.TimeAnimationClient))
                .Take(2)
                .Subscribe(_ => { _clientOrderSystem.CreateClient((null, null)); })
                .AddTo(Disposables);
        }
    }
}