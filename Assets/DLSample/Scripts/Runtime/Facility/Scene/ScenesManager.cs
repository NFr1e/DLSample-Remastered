using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.SceneManage
{
    public class ScenesManager
    {
        private readonly Queue<SceneRequest> _requestQueue = new();
        private bool _isProcessing = false;

        public LoadSceneRequest LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool autoActivate = true)
        {
            var request = new LoadSceneRequest(sceneName, mode, autoActivate);
            EnqueueRequest(request);
            return request;
        }

        public UnloadSceneRequest UnloadScene(string sceneName)
        {
            var request = new UnloadSceneRequest(sceneName);
            EnqueueRequest(request);
            return request;
        }

        private void EnqueueRequest(SceneRequest request)
        {
            _requestQueue.Enqueue(request);
            if (!_isProcessing) ProcessQueue().Forget();
        }

        private async UniTaskVoid ProcessQueue()
        {
            _isProcessing = true;
            while (_requestQueue.Count > 0)
            {
                var currentRequest = _requestQueue.Dequeue();
                if (currentRequest.Status == SceneStatus.Canceled) continue;

                try
                {
                    await currentRequest.ExecuteAsync();
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"Operation Cancelled: {currentRequest.SceneName}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Operation Error: {e.Message}");
                    currentRequest.SetResult(false, e.Message);
                }
            }
            _isProcessing = false;
        }
    }
}
