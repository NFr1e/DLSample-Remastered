using UnityEngine.SceneManagement;
using DLSample.App;
using DLSample.Shared;
using DLSample.Facility.Events;
using DLSample.Facility.SceneManage;

namespace DLSample.Gameplay.Behaviours
{
    public class LevelRestarter
    {
        private readonly string _sceneName;
        private readonly EventBus _eventBus;
        private readonly ScenesManager _sceneManager;
        private readonly GameplayEventParams.ExitGameRequest _exitRequest = new();

        public LevelRestarter(string levelScene)
        {
            _sceneName = levelScene;
            _sceneManager = AppEntry.SceneManager;
            _eventBus = GameplayEntry.Instance.EventBus;
        }

        public void RestartLevel()
        {
            _eventBus?.Invoke(this, _exitRequest);
            ScreenHelper.FullScreenMaskAction(Reload);
        }
        private async void Reload()
        {
            if (SceneManager.loadedSceneCount <= 1)
            {
                _sceneManager.LoadScene(_sceneName);
            }
            else
            {
                await _sceneManager.UnloadScene(_sceneName).Task;
                _sceneManager.LoadScene(_sceneName);
            }
        }
    }
}
