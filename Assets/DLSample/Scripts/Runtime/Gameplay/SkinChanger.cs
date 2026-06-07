using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;
using DLSample.Gameplay.Behaviours.Skin;
using DLSample.Framework;

namespace DLSample.Gameplay.Skin
{
    /// <summary>
    /// 管理每个SkinAdapter独立的SkinBehaviour实例，实现皮肤切换功能。
    /// 每个adapter拥有独立的皮肤实例，避免多Player间的皮肤状态竞争。
    /// </summary>
    public class SkinChanger : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_SKIN_CHANGER;

        private readonly List<SkinAdapter> _adapters = new();
        private readonly Dictionary<SkinAdapter, SkinBehaviourBase> _adapterSkins = new();

        private readonly SkinDataScriptable _skinData;
        private readonly Transform _skinContainer;

        /// <summary>
        /// 当前选中的皮肤ID，用于新adapter注册时应用正确的皮肤。
        /// </summary>
        private string _currentSkinId;

        public SkinChanger(SkinDataScriptable skinData, Transform skinContainer)
        {
            _skinData = skinData;
            _skinContainer = skinContainer;
        }

        /// <summary>
        /// 切换皮肤。销毁所有adapter的旧实例，为每个adapter创建独立的新实例。
        /// </summary>
        public bool ChangeSkin(string skinId)
        {
            SkinItem skin = _skinData.GetSkin(skinId);

            if (skin.IsValid)
            {
                _currentSkinId = skinId;

                foreach (var adapter in _adapters)
                {
                    DetachAndDestroySkin(adapter);
                    InstantiateAndApplySkin(adapter, skin.Prefab);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 添加SkinAdapter。如果已有选中皮肤，为新adapter实例化一份独立的皮肤。
        /// </summary>
        public void AddAdapter(SkinAdapter adapter)
        {
            if (_adapters.Contains(adapter)) return;

            _adapters.Add(adapter);

            if (!string.IsNullOrEmpty(_currentSkinId))
            {
                SkinItem skin = _skinData.GetSkin(_currentSkinId);
                if (skin.IsValid)
                    InstantiateAndApplySkin(adapter, skin.Prefab);
            }
        }

        /// <summary>
        /// 移除SkinAdapter，同时销毁其对应的皮肤实例。
        /// </summary>
        public void RemoveAdapter(SkinAdapter adapter)
        {
            DetachAndDestroySkin(adapter);
            _adapters.Remove(adapter);
        }

        /// <summary>
        /// 为指定adapter实例化皮肤prefab并应用。
        /// </summary>
        private void InstantiateAndApplySkin(SkinAdapter adapter, SkinBehaviourBase prefab)
        {
            var instance = GameObject.Instantiate(prefab, _skinContainer);
            instance.SetHeadContainer(adapter.HeadContainer);
            adapter.SetCurrentSkin(instance);
            instance.OnApply();
            _adapterSkins[adapter] = instance;
        }

        /// <summary>
        /// 销毁指定adapter的旧皮肤实例。
        /// </summary>
        private void DetachAndDestroySkin(SkinAdapter adapter)
        {
            if (_adapterSkins.TryGetValue(adapter, out var oldSkin))
            {
                oldSkin.OnDetach();
                GameObject.Destroy(oldSkin.gameObject);
                _adapterSkins.Remove(adapter);
            }
        }
    }
}
