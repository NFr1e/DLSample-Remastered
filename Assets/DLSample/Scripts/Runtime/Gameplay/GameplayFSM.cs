using System;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Phase
{
    public class GameplayFSM
    {
        private readonly Dictionary<Type, GameplayStateBase> _statesCache = new();

        private GameplayStateBase _currentState;

        private readonly EventBus _evtBus;

        public GameplayStateBase CurrentState => _currentState;
        public EventBus EventBus => _evtBus;

        public GameplayFSM(EventBus evtBus)
        {
            _evtBus = evtBus;
        }

        public void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }

        public void Dispose()
        {
            _statesCache?.Clear();
        }

        #region Public API
        public void SetCurrentState<TState>() where TState : GameplayStateBase
        {
            if (_currentState != null && _currentState is TState) return;

            if (!TryGetState<TState>(out var state))
            {
                state = Activator.CreateInstance(typeof(TState), this) as TState;
                AddState<TState>(state);
            }

            SetCurrentState(state);
        }
        public void SetCurrentState(GameplayStateBase state)
        {
            if (_currentState != null && state == _currentState) return;

            var prevState = _currentState;

            state.Init();
            _currentState?.Exit();
            _currentState = state;
            _currentState?.Enter();

            Debug.Log($"[GameplayFSM] State changed from {prevState?.ToString() ?? "null"} to {_currentState?.ToString() ?? "null"}");

            var stateChangeCtx = new GameplayEventParams.GameplayStateChangeCtx(state, prevState);
            _evtBus.Invoke(this, stateChangeCtx);
        }
        #endregion

        #region HelperMethods
        private void AddState<TState>(GameplayStateBase instance)
        {
            if (!_statesCache.ContainsKey(typeof(TState)))
            {
                _statesCache.Add(typeof(TState), instance);
            }
        }
        private bool TryGetState<TState>(out TState state) where TState : GameplayStateBase
        {
            if (_statesCache.TryGetValue(typeof(TState), out var instance))
            {
                state = instance as TState;
                return true;
            }
            state = null;
            return false;
        }
        #endregion
    }
}
