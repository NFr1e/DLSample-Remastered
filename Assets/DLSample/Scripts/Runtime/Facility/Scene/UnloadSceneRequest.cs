using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.SceneManage
{
    public class UnloadSceneRequest : SceneRequest
    {
        public UnloadSceneRequest(string sceneName) : base(sceneName) { }

        internal override async UniTask ExecuteAsync()
        {
            Status = SceneStatus.Loading;

            Scene toUnload = SceneManager.GetSceneByName(SceneName);

            if (!toUnload.isLoaded)
            {
                Debug.LogWarning($"Scene {SceneName} is not loaded");

                SetResult(true);
                return;
            }

            AsyncOperation op = SceneManager.UnloadSceneAsync(SceneName);

            while (!op.isDone)
            {
                Progress = op.progress;

                if (CancellationToken.IsCancellationRequested)
                {
                    throw new System.OperationCanceledException();
                }
                await UniTask.Yield();
            }

            Progress = 1.0f;
            Status = SceneStatus.Success;
            SetResult(true);
        }
    }
}
