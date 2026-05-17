using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DLSample.Shared.UI;

namespace DLSample.Facility.UI
{
    public class UIElementManager
    {
        readonly Stack<UIElementData<Panel>> _fullscreenPanelsStack = new();
        readonly Dictionary<UIElementData<Panel>, Panel> _fullscreenPanelsCache = new();
        UIElementData<Panel> _currentFullscreenPanelData = new();

        readonly Dictionary<string, Panel> _persistentPanelCache = new();

        UIPanelsDataScriptable _panelsConfig;

        Camera _uiCamera;

        Transform _root;
        Transform _fullscreenPanelsContainer;
        Transform _persistPanelsContainer;

        public void SetupConfigs(UIPanelsDataScriptable panelsConfig)
        {
            _panelsConfig = panelsConfig;
        }

        public void SetupCamera(Camera camera)
        {
            _uiCamera = camera;
        }

        void SetupContainers()
        {
            if (!_root)
            {
                _root = new GameObject("UILayer").transform;
            }

            if (!_fullscreenPanelsContainer)
            {
                _fullscreenPanelsContainer = new GameObject("FullscreenPanels").transform;
            }

            if (!_persistPanelsContainer)
            {
                _persistPanelsContainer = new GameObject("PersistPanels").transform;
            }

            _fullscreenPanelsContainer.SetParent(_root);
            _persistPanelsContainer.SetParent(_root);

            GameObject.DontDestroyOnLoad(_root);
        }

        public void Init()
        {
            SetupContainers();
        }

        public void Dispose()
        {
            _uiCamera = null;
            _panelsConfig = null;

            _fullscreenPanelsStack?.Clear();
            _fullscreenPanelsCache?.Clear();
            _persistentPanelCache?.Clear();

            if (_root)
            {
                GameObject.Destroy(_root.gameObject);
            }
        }

        #region Panel

        #region OpenPanel
        public async UniTask<Panel> OpenPanel(string id)
        {
            if (_panelsConfig.GetPanel(id, out var panelItem))
            {
                var panelComp = panelItem.Item;

                return panelComp.Type switch
                {
                    PanelType.Persistent => await OpenPersistPanel(panelItem),
                    PanelType.FullScreen => await OpenFullScreenPanel(panelItem),
                    _ => null
                };
            }

            return null;
        }

        async UniTask<Panel> OpenFullScreenPanel(UIElementData<Panel> panelData)
        {
            if (_fullscreenPanelsCache.TryGetValue(_currentFullscreenPanelData, out var currentPanel))
            {
                if (!_currentFullscreenPanelData.Equals(panelData))
                {
                    await UniTask.SwitchToMainThread();
                    currentPanel.Pause();
                }
            }

            if (_fullscreenPanelsCache.TryGetValue(panelData, out var cachedPanel))
            {
                await UniTask.SwitchToMainThread();

                cachedPanel.Resume();
                _currentFullscreenPanelData = panelData;
                _fullscreenPanelsStack.Push(panelData);

                cachedPanel.SetCamera(_uiCamera);

                return cachedPanel;
            }

            await UniTask.SwitchToMainThread();
            var go = GameObject.Instantiate(panelData.Item, _fullscreenPanelsContainer);
            var panel = go.GetComponent<Panel>();
            _fullscreenPanelsCache[panelData] = panel;
            panel.Load();

            _currentFullscreenPanelData = panelData;
            _fullscreenPanelsStack.Push(panelData);
            panel.SetCamera(_uiCamera);

            return panel;
        }

        async UniTask<Panel> OpenPersistPanel(UIElementData<Panel> panelData)
        {
            if (_persistentPanelCache.TryGetValue(panelData.ItemId, out var existing))
            {
                return existing;
            }

            await UniTask.SwitchToMainThread();
            var panel = GameObject.Instantiate(panelData.Item, _persistPanelsContainer).GetComponent<Panel>();

            _persistentPanelCache[panelData.ItemId] = panel;
            panel.Load();

            panel.SetCamera(_uiCamera);

            return panel;
        }
        #endregion

        #region ClosePanel
        public async UniTask CloseCurrentFullScreenPanel()
        {
            if (_fullscreenPanelsStack.Count == 0)
                return;

            var closingPanelData = _fullscreenPanelsStack.Pop();
            _currentFullscreenPanelData = default;

            if (_fullscreenPanelsCache.TryGetValue(closingPanelData, out var panel))
            {
                await UniTask.SwitchToMainThread();

                panel.Unload();
                _fullscreenPanelsCache.Remove(closingPanelData);
            }

            if (_fullscreenPanelsStack.Count > 0)
            {
                var prevPanelData = _fullscreenPanelsStack.Peek();

                if (_fullscreenPanelsCache.TryGetValue(prevPanelData, out var prevPanel))
                {
                    await UniTask.SwitchToMainThread();
                    prevPanel.Resume();
                }
                _currentFullscreenPanelData = prevPanelData;
            }
        }

        public async UniTask CloseAllFullscreenPanel()
        {
            while (_fullscreenPanelsStack.Count > 0)
            {
                await CloseCurrentFullScreenPanel();
            }
        }

        public async UniTask ClosePersistentPanel(string panelId)
        {
            if (!_persistentPanelCache.TryGetValue(panelId, out var page))
                return;

            await UniTask.SwitchToMainThread();
            page.Unload();
            _persistentPanelCache.Remove(panelId);
        }
        #endregion

        #endregion
    }
}
