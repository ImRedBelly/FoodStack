using Configs;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Cards;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Systems;
using Gameplay.Clients;
using Gameplay.Clients.Factory;
using Gameplay.Clients.Services;
using Gameplay.OrderButton;
using Gameplay.OrderButton.Factory;
using Gameplay.OrderButton.Services;
using Gameplay.Tools;
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
        [Space] 
        [SerializeField] private Button _quitButton;
        [Space] 
        [Header("Prefabs")]
        [SerializeField] private IngredientCard ingredientCardPrefab;
        [SerializeField] private ToolCard toolCardPrefab;
        [SerializeField] private ClientCard clientCardPrefab;
        [SerializeField] private OrderButton orderButtonPrefab;
        [Space]
        [Header("Debug Data")] 
        [SerializeField] private LevelsConfig _levelsConfig;
        
        private CardDragSystem _cardDragSystem;
        private CardCollisionSystem _cardCollisionSystem;
        private CardStackSystem _cardStackSystem;
        private CardStackMoveSystem _cardStackMoveSystem;

        private CardFactory _cardFactory;
        private ToolFactory _toolFactory;
        private ClientFactory _clientFactory;
        private OrderButtonFactory _orderButtonFactory;
        private OrderButtonSelectSystem _orderButtonSelectSystem;
        private ClientOrderSystem _clientOrderSystem;

        protected override void OnInit()
        {
            base.OnInit();
            InitWindows();
            InitFactories();


            InitOrderButtonsSystems();


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

            _orderButtonFactory = new OrderButtonFactory(orderButtonPrefab);
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

        private void InitCardsSystems()
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

        private void InitToolsSystems()
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

        private void InitClientsSystems()
        {
            ClientTriggerServiceSystem clientTriggerServiceSystem = new ClientTriggerServiceSystem(_clientFactory, _cardFactory, _cardCollisionSystem);
            clientTriggerServiceSystem
                .Init()
                .AddTo(Disposables);
                
            ClientServiceSystem clientServiceSystem = new ClientServiceSystem(clientTriggerServiceSystem, _clientFactory);
            clientServiceSystem
                .Init()
                .AddTo(Disposables);

            _clientOrderSystem = new ClientOrderSystem(_levelsConfig.GetLevelData(SaveUtility.Level), _clientFactory, clientTriggerServiceSystem, clientServiceSystem, _orderButtonSelectSystem);
            _clientOrderSystem
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

                _cardFactory.CreateIngredient(startCards[i], position);
            }
        }

        private void CreateStartTools()
        {
            var tools = _levelsConfig.GetLevelData(SaveUtility.Level).ToolCards;
            foreach (var tool in tools)
            {
                _toolFactory.CreateTool(tool, Vector3.up * 2);
            }
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
        
        private async void InitClients()
        {
            for (int i = 0; i < 2; i++)
            {
                _clientOrderSystem.CreateClient(null);
                await UniTask.WaitForSeconds(Constants.TimeAnimationClient);
            }
        }
    }
}