using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.SceneManage
{
    public class LoadSceneRequest : SceneRequest
    {
        public LoadSceneMode Mode { get; }
        public bool AutoActivate { get; set; }

        internal UniTaskCompletionSource ActivationSource { get; } = new UniTaskCompletionSource();

        public LoadSceneRequest(string sceneName, LoadSceneMode mode, bool autoActivate) : base(sceneName)
        {
            Mode = mode;
            AutoActivate = autoActivate;
        }

        internal override async UniTask ExecuteAsync()
        {
            Status = SceneStatus.Loading;
            Progress = 0f;

            AsyncOperation op = SceneManager.LoadSceneAsync(SceneName, Mode);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                }

                Progress = op.progress;

                await UniTask.Yield(CancellationToken);
            }

            Progress = 0.9f;
            Status = SceneStatus.Ready;

            if (!AutoActivate)
            {
                try
                {
                    await ActivationSource.Task.AttachExternalCancellation(CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }

            Status = SceneStatus.Activating;
            op.allowSceneActivation = true;

            await op.WithCancellation(CancellationToken);

            Progress = 1.0f;
            Status = SceneStatus.Success;
            SetResult(true);
        }

        public void ActivateScene()
        {
            if (Status == SceneStatus.Ready)
            {
                ActivationSource.TrySetResult();
            }
            else
            {
                Debug.LogWarning($"[SceneRequest] Cannot activate scene {SceneName} because it's in {Status} status.");
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            ActivationSource.TrySetCanceled();
        }
    }
}
