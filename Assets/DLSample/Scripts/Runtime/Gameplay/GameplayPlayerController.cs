using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Framework;
using DLSample.Shared;
using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Gameplay.Behaviours;
using Cysharp.Threading.Tasks;

namespace DLSample.Gameplay
{
    public class GameplayPlayerController : IModule, IBacktrackable
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_PLAYER_CONTROLLER;

        private readonly EventBus _evtBus;

        private readonly GameplayPlayerMove _mainPlayer;
        private readonly BacktrackablesHandler _backtrackablesHandler;
        private readonly CheckpointHandler _checkpointHandler;
        private readonly GameplayStateHandler _stateHandler;

        private List<GameplayPlayerMove> _playersList = new();

        private GameplayStateBase _currentState;

        private bool _synced = false;

        public GameplayPlayerMove MainPlayer => _mainPlayer;

        public GameplayPlayerController(EventBus eventBus, GameplayPlayerMove mainPlayer, GameplayStateHandler stateHandler, CheckpointHandler checkpointHandler, BacktrackablesHandler backtrackHandler)
        {
            _evtBus = eventBus;

            _mainPlayer = mainPlayer;

            _stateHandler = stateHandler;
            _checkpointHandler = checkpointHandler;
            _backtrackablesHandler = backtrackHandler;
        }

        public void AddPlayer(GameplayPlayerMove playerMove)
        {
            _playersList.Add(playerMove);

            HandleStates(_currentState);
        }
        public void RemovePlayer(GameplayPlayerMove playerMove)
        {
            _playersList.Remove(playerMove);
        }

        public void OnInit()
        {
            SubscribeEvents();
            _backtrackablesHandler?.Register(this);

            _mainPlayer.Ready();
        }
        public void OnShutdown()
        {
            UnsubscribeEvents();
            _playersList?.Clear();

            _backtrackablesHandler?.Unregister(this);
        }

        private void SubscribeEvents()
        {
            _evtBus?.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evtBus?.Subscribe<PlayerEventsParams.PlayerDieArg>(OnPlayerDie);

            _evtBus?.Subscribe<PlayerEventsParams.SpeedChangeRequest>(ChangePlayerSpeed);
            _evtBus?.Subscribe<PlayerEventsParams.GravityChangeRequest>(ChangePlayerGravity);
            _evtBus?.Subscribe<PlayerEventsParams.DirectionChangeRequest>(ChangePlayerDirections);
            _evtBus?.Subscribe<PlayerEventsParams.ForceTurnRequest>(ForceTurn);
            _evtBus?.Subscribe<PlayerEventsParams.TeleportRequest>(PlayerTeleport);
            _evtBus?.Subscribe<PlayerEventsParams.VelocityChangeRequest>(PlayerJump);
        }
        private void UnsubscribeEvents()
        {
            _evtBus?.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evtBus?.Unsubscribe<PlayerEventsParams.PlayerDieArg>(OnPlayerDie);

            _evtBus?.Unsubscribe<PlayerEventsParams.SpeedChangeRequest>(ChangePlayerSpeed);
            _evtBus?.Unsubscribe<PlayerEventsParams.GravityChangeRequest>(ChangePlayerGravity);
            _evtBus?.Unsubscribe<PlayerEventsParams.DirectionChangeRequest>(ChangePlayerDirections);
            _evtBus?.Unsubscribe<PlayerEventsParams.ForceTurnRequest>(ForceTurn);
            _evtBus?.Unsubscribe<PlayerEventsParams.TeleportRequest>(PlayerTeleport);
            _evtBus?.Unsubscribe<PlayerEventsParams.VelocityChangeRequest>(PlayerJump);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            HandleStates(ctx.CurrentState);
        }
        void HandleStates(GameplayStateBase state)
        {
            switch (state)
            {
                case GameplayStates.PreparingState:
                    if (!_stateHandler.IsGameStarted)
                    {
                        PlayerReady();
                    }
                    break;
                case GameplayStates.GamingState:
                    _ = StartPlayer();
                    break;
                case GameplayStates.PauseState:
                    StopPlayer();
                    break;
            }

            _currentState = state;
        }
        void OnPlayerDie(PlayerEventsParams.PlayerDieArg arg)
        {
            switch (arg.DieCause)
            {
                case PlayerDiecause.Obstacle:
                    StopPlayer();
                    break;
            }
        }

        private void PlayerReady()
        {
            _mainPlayer.Ready();
        }

        private async UniTaskVoid StartPlayer()
        {
            if (!_synced)
                await SyncDelay();

            foreach (var player in _playersList)
            {
                player.StartMove();
            }
        }
        public async UniTask SyncDelay()
        {
            _synced = true;

            var delay = PlayerPrefs.GetFloat(DLSampleConsts.SaveAndLoad.ID_SYNC_DELAY, 0f);
            if (delay < 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(-delay));
            }
        }

        private void StopPlayer()
        {
            foreach (var player in _playersList)
            {
                player.StopMove();
            }
        }

        public void PlayerInput()
        {
            foreach (var player in _playersList)
            {
                player.Inputed();
            }
        }

        #region ApplyEvents
        private void ChangePlayerSpeed(PlayerEventsParams.SpeedChangeRequest request)
        {
            foreach (var player in _playersList)
            {
                player.PlayerParams.SetSpeed(request.Speed);
            }
        }
        private void ChangePlayerGravity(PlayerEventsParams.GravityChangeRequest request)
        {
            foreach (var player in _playersList)
            {
                player.PlayerParams.SetLocalGravity(request.Gravity);
            }
        }
        private void ChangePlayerDirections(PlayerEventsParams.DirectionChangeRequest request)
        {
            foreach (var player in _playersList)
            {
                player.PlayerParams.SetDirection(request.Directions);
            }
        }

        private void ForceTurn(PlayerEventsParams.ForceTurnRequest request)
        {
            _mainPlayer.Turn();
        }
        private void PlayerTeleport(PlayerEventsParams.TeleportRequest request)
        {
            foreach (var player in _playersList)
            {
                player.SetGrounded(false);
                player.transform.position = request.Position;
            }
        }
        private void PlayerJump(PlayerEventsParams.VelocityChangeRequest request)
        {
            foreach (var player in _playersList)
            {
                player.SetVelocity(request.Velocity);
            }
        }
        #endregion

        #region Backtrack
        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_PLAYER_CONTROLLER;

        private struct BacktrackStates
        {
            public GameplayPlayerMove player;
            public bool isMainPlayer;

            public Vector3 position;
            public Quaternion rotation;

            public float speed;
            public Vector3 gravity;
            public Vector3 velocity;
            public int directionIndex;
            public PlayerDirections directions;

            public readonly void Backtrack()
            {
                player.StopMove();
                player.SetVelocity(velocity);

                player.PlayerParams.SetSpeed(speed);
                player.PlayerParams.SetLocalGravity(gravity);
                player.PlayerParams.SetDirection(directions);
                directions.SetCurrentIndex(directionIndex);

                if (isMainPlayer) return;
                player.transform.SetPositionAndRotation(position, rotation);
            }
        }

        private List<GameplayPlayerMove> backtrackPlayers = new();
        private readonly List<BacktrackStates> backtrackStates = new();

        public void GetBacktrackState()
        {
            backtrackPlayers = _playersList.ToList();

            backtrackStates.Clear();

            foreach (var p in backtrackPlayers)
            {
                var state = new BacktrackStates
                {
                    player = p,
                    isMainPlayer = p == _mainPlayer,

                    position = p.transform.position,
                    rotation = p.transform.rotation,

                    speed = p.PlayerParams.MoveSpeed,
                    gravity = p.PlayerParams.LocalGravity,
                    velocity = p.MovingArgs.Velocity,
                    directionIndex = p.PlayerParams.Directions.CurrentIndex,
                    directions = p.PlayerParams.Directions
                };

                backtrackStates.Add(state);
            }
        }
        public void Backtrack()
        {
            _synced = false;

            foreach (var state in backtrackStates)
            {
                state.Backtrack();
            }

            _playersList = backtrackPlayers.ToList();

            _mainPlayer.transform.SetPositionAndRotation(_checkpointHandler.CurrentCheckpoint.MainPlayerTransform.position, _checkpointHandler.CurrentCheckpoint.MainPlayerTransform.rotation);
        }
        #endregion
    }
}
