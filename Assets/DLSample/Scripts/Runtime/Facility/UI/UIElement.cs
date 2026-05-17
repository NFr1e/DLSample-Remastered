using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DLSample.Facility.UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [Serializable]
        public class UIElementCallbacks
        {
            public UnityEvent onLoad;
            public UnityEvent onLoaded;
            public UnityEvent onUnload;
            public UnityEvent onUnloaded;
            public UnityEvent onPause;
            public UnityEvent onPaused;
            public UnityEvent onResume;
            public UnityEvent onResumed;
        }

        public UIElementCallbacks Callbacks = new();

        public bool IsActive => _isActive;

        bool _isActive = false;
        protected bool _isDestroyed = false;

        CancellationTokenSource _loadCts = new();
        CancellationTokenSource _unloadCts = new();
        CancellationTokenSource _pauseCts = new();
        CancellationTokenSource _resumeCts = new();

        public async void Load()
        {
            _isActive = true;

            _loadCts?.Cancel();
            _loadCts = new();

            Callbacks.onLoad?.Invoke();

            await OnLoadAsync(_loadCts.Token);
            OnLoaded();
        }
        public async void Unload()
        {
            _isActive = false;

            _unloadCts?.Cancel();
            _unloadCts = new();

            Callbacks.onUnload?.Invoke();

            await OnUnloadAsync(_unloadCts.Token);
            OnUnloaded();
        }
        public async void Pause()
        {
            if (!_isActive)
                return;

            _isActive = false;

            _pauseCts?.Cancel();
            _pauseCts = new();

            Callbacks.onPause?.Invoke();

            await OnPauseAsync(_pauseCts.Token);
            OnPaused();
        }
        public async void Resume()
        {
            if (_isActive)
                return;

            _isActive = true;

            _resumeCts?.Cancel();
            _resumeCts = new();

            Callbacks.onResume?.Invoke();

            await OnResumeAsync(_resumeCts.Token);
            OnResumed();
        }

        public void Update()
        {
            if (_isActive)
            {
                OnUpdate();
            }
        }

        protected virtual void OnLoaded()
        {
            Callbacks.onLoaded?.Invoke();
        }
        protected virtual void OnUnloaded()
        {
            Callbacks.onUnloaded?.Invoke();

            if (!_isDestroyed)
            {
                _isDestroyed = true;

                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            }
        }
        protected virtual void OnPaused()
        {
            Callbacks.onPaused?.Invoke();
        }
        protected virtual void OnResumed()
        {
            Callbacks.onResumed?.Invoke();
        }
        protected virtual void OnUpdate()
        {

        }

        #region Async Methods
        public virtual UniTask OnLoadAsync(CancellationToken token = default)
        {
            return UniTask.CompletedTask;
        }
        public virtual UniTask OnUnloadAsync(CancellationToken token = default)
        {
            return UniTask.CompletedTask;
        }
        public virtual UniTask OnPauseAsync(CancellationToken token = default)
        {
            return UniTask.CompletedTask;
        }
        public virtual UniTask OnResumeAsync(CancellationToken token = default)
        {
            return UniTask.CompletedTask;
        }
        #endregion
    }
}
