using System;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.UI
{
    [Serializable]
    public class UIElementAnimator
    {
        public bool enabled = true;
        public BrunoMikoski.AnimationSequencer.AnimationSequencer animationSequencer;

        public async UniTask WaitForCompletion()
        {
            if (!enabled || animationSequencer == null)
            {
                await UniTask.CompletedTask;
                return;
            }

            await animationSequencer.WaitForSequenceAsync();
        }
    }
}
