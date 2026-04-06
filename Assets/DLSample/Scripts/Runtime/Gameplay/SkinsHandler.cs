using DLSample.App;
using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;
using UnityEngine;

namespace DLSample.Gameplay.Skin
{
    public struct ChangeSkinRequest : IEventArg 
    { 
        public string SkinId { get; set; }
    }

    /// <summary>
    /// 通过全局事件系统（切换时）和持久化系统持有当前皮肤状态信息，根据皮肤状态信息通过SkinChanger实例实现实时切换皮肤
    /// </summary>
    public class SkinsHandler : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_SKIN_HANDLER;

        public string CurrentSkinId { get; private set; }

        private readonly SkinChanger _skinChanger;
        
        private EventBus _globalEvtBus;

        public SkinsHandler(SkinChanger skinChanger)
        {
            _skinChanger = skinChanger;
        }

        public void OnInit()
        {
            _globalEvtBus = AppEntry.EventBus;
            _globalEvtBus.Subscribe<ChangeSkinRequest>(OnSkinChangeRequested);

            var savedSkin = PlayerPrefs.GetString(DLSampleConsts.SaveAndLoad.ID_SKIN);
            _skinChanger.ChangeSkin(savedSkin);
            CurrentSkinId = savedSkin;
        }
        public void OnShutdown()
        {
            _globalEvtBus.Unsubscribe<ChangeSkinRequest>(OnSkinChangeRequested);
        }

        private void OnSkinChangeRequested(ChangeSkinRequest request)
        {
            CurrentSkinId = request.SkinId;

            _skinChanger.ChangeSkin(request.SkinId);

            PlayerPrefs.SetString(DLSampleConsts.SaveAndLoad.ID_SKIN, request.SkinId);
        }
    }
}
