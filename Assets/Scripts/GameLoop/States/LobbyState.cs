using System;
using GameLoop.Roots;
using Services.WindowService;
using Support;
using UniRx;

namespace GameLoop.States
{
    public class LobbyState : IGameState
    {
        private readonly GameMachine _gameMachine;
        private readonly WindowsService _windowsService;
        private readonly WindowResolver _windowResolver;

        private readonly CompositeDisposable _rootDisposable = new();

        private const string StateSceneName = "2.Lobby";

        public LobbyState(GameMachine gameMachine, WindowsService windowsService, WindowResolver windowResolver)
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

            var lobbyRoot = SceneExtensions.LoadSceneRoot<LobbyRoot>();

            lobbyRoot
                .Init(new LobbyRoot.Model(OnGameStartRequested, null, _windowsService, _windowResolver))
                .AddTo(subscriptions);

            return subscriptions;
        }

        private void OnGameStartRequested()
        {
            _gameMachine.ChangeState<GameState>();
        }
    }
}