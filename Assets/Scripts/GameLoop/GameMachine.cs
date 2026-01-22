using System;
using System.Collections.Generic;
using Core;
using GameLoop.States;
using UniRx;

namespace GameLoop
{
    public class GameMachine : DisposableClass
    {
        private readonly Dictionary<Type, IGameState> _states = new();
        private IGameState _currentState;

        protected override void OnInit()
        {
            base.OnInit();

            Disposable
                .Create(OnDisposed)
                .AddTo(Disposables);
        }

        public void AddState(IGameState state)
        {
            _states.Add(state.GetType(), state);
        }

        public void ChangeState<T>() where T : IGameState
        {
            _currentState?.Deinit();
            _currentState = _states[typeof(T)];
            _currentState.Init();
        }

        public void ChangeState(Type stateType)
        {
            if (!typeof(IGameState).IsAssignableFrom(stateType))
                throw new ArgumentException($"Type {stateType} does not implement IGameState");

            if (!_states.TryGetValue(stateType, out var state))
                throw new InvalidOperationException($"State of type {stateType} is not registered");

            _currentState?.Deinit();
            _currentState = state;
            _currentState.Init();
        }

        public IGameState GetCurrentState()
        {
            return _currentState;
        }

        private void OnDisposed()
        {
            _currentState?.Deinit();
        }
    }
}