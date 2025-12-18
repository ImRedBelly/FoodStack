using System;
using GameLoop.Roots;
using Services.WindowService;
using Support;
using UniRx;

namespace GameLoop.States
{
    public class GameState : IGameState
    {
        private readonly GameMachine _gameMachine;
        private readonly WindowsService _windowsService;
        private readonly WindowResolver _windowResolver;

        private readonly CompositeDisposable _rootDisposable = new();

        private const string StateSceneName = "3.Game";

        public GameState(GameMachine gameMachine, WindowsService windowsService, WindowResolver windowResolver)
        {
            _gameMachine = gameMachine;
            _windowsService = windowsService;
            _windowResolver = windowResolver;
        }

        public void Init()
        {
            SceneExtensions.LoadScene(StateSceneName)
                .SafeSubscribe(_ => OnSceneLoaded())
                .AddTo(_rootDisposable);
        }

        public void Deinit()
        {
            _rootDisposable.Clear();
        }

        private void OnSceneLoaded()
        {
            InitControllers()
                .AddTo(_rootDisposable);
        }

        private IDisposable InitControllers()
        {
            var subscriptions = new CompositeDisposable();

            var gameRoot = SceneExtensions.LoadSceneRoot<GameRoot>();

            gameRoot
                .Init(new LobbyRoot.Model(OnExit, _windowsService, _windowResolver))
                .AddTo(subscriptions);

            return subscriptions;
        }

        private void OnExit()
        {
            _gameMachine.ChangeState<LobbyState>();
        }
    }
}