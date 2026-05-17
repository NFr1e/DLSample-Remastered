using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.SceneManage
{
    public enum SceneStatus
    {
        Pending,
        Loading,
        Ready,
        Activating,
        Success,
        Canceled,
        Failed
    }
    public abstract class SceneRequest
    {
        public string SceneName { get; protected set; }

        public SceneStatus Status { get; internal set; } = SceneStatus.Pending;
        public float Progress { get; internal set; }

        protected readonly CancellationTokenSource _cts = new();
        protected readonly UniTaskCompletionSource _tcs = new();
        public CancellationToken CancellationToken => _cts.Token;
        public UniTask Task => _tcs.Task;

        public SceneRequest(string sceneName)
        {
            SceneName = sceneName;
        }

        public virtual void Cancel()
        {
            if (Status is SceneStatus.Pending or SceneStatus.Loading or SceneStatus.Ready)
            {
                Status = SceneStatus.Canceled;

                _cts.Cancel();
                _tcs.TrySetCanceled();
            }
        }

        internal abstract UniTask ExecuteAsync();

        internal void SetResult(bool success, string error = null)
        {
            if (success)
                _tcs.TrySetResult();
            else
                _tcs.TrySetException(new Exception(error ?? "Scene Operation Failed"));
        }
    }
}
