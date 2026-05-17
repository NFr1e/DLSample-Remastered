using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DLSample.Facility.UI
{
    public enum PanelType
    {
        FullScreen,
        Persistent
    }
    public class Panel : UIElement
    {
        [Serializable]
        public class Animations
        {
            public UIElementAnimator loadAnimation;
            public UIElementAnimator unloadAnimation;
            public UIElementAnimator pauseAnimation;
            public UIElementAnimator resumeAnimation;
        }

        [SerializeField]
        Animations animations = new();

        [SerializeField]
        PanelType panelType = PanelType.FullScreen;

        [SerializeField]
        List<Canvas> cameraSpaceCanvas;

        public PanelType Type
        {
            get => panelType;
        }

        public void SetCamera(Camera camera)
        {
            foreach (Canvas c in cameraSpaceCanvas)
            {
                c.worldCamera = camera;
            }
        }

        #region Async Methods
        public override async UniTask OnLoadAsync(CancellationToken token)
        {
            await animations.loadAnimation.WaitForCompletion();
        }
        public override async UniTask OnUnloadAsync(CancellationToken token)
        {
            await animations.unloadAnimation.WaitForCompletion();
        }
        public override async UniTask OnPauseAsync(CancellationToken token)
        {
            await animations.pauseAnimation.WaitForCompletion();
        }
        public override async UniTask OnResumeAsync(CancellationToken token)
        {
            await animations.resumeAnimation.WaitForCompletion();
        }
        #endregion
    }
}
