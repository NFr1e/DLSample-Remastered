using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;
using DLSample.Gameplay.Behaviours.Skin;
using DLSample.Framework;

namespace DLSample.Gameplay.Skin
{
    /// <summary>
    /// 通过实例化SkinBehaviour对象并传参给SkinAdapter实现皮肤切换功能
    /// </summary>
    public class SkinChanger : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_SKIN_CHANGER;

        private readonly List<SkinAdapter> _adapters = new();

        private readonly SkinDataScriptable _skinData;
        private readonly Transform _skinContainer;

        private SkinBehaviourBase _currentSkinBehaviour;

        public SkinChanger(SkinDataScriptable skinData, Transform skinContainer)
        {
            _skinData = skinData;
            _skinContainer = skinContainer;
        }

        public bool ChangeSkin(string skinId)
        {
            SkinItem skin = _skinData.GetSkin(skinId);

            if (skin.IsValid)
            {
                if (_currentSkinBehaviour != null)
                {
                    _currentSkinBehaviour.OnDetach();
                    GameObject.Destroy(_currentSkinBehaviour.gameObject);
                }

                SkinBehaviourBase behaviour = GameObject.Instantiate(skin.Prefab, _skinContainer);

                _currentSkinBehaviour = behaviour;
                RefreshAdapter(behaviour);

                _currentSkinBehaviour.OnApply();

                return true;
            }

            return false;
        }
        private void RefreshAdapter(SkinBehaviourBase behaviour)
        {
            foreach (var adapter in _adapters)
            {
                adapter.SetCurrentSkin(behaviour);
            }
        }

        /// <summary>
        /// 添加SkinAdapter，同时刷新状态
        /// </summary>
        /// <param name="adapter"></param>
        public void AddAdapter(SkinAdapter adapter)
        {
            if (_adapters.Contains(adapter)) return;

            _adapters.Add(adapter);
            RefreshAdapter(_currentSkinBehaviour);
        }

        /// <summary>
        /// 移除SkinAdapter，同时刷新状态
        /// </summary>
        /// <param name="adapter"></param>
        public void RemoveAdapter(SkinAdapter adapter)
        {
            _adapters.Remove(adapter);
            RefreshAdapter(_currentSkinBehaviour);
        }
    }
}
