using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 皇冠视图，根据收集皇冠数量播放音效并显示皇冠图标。
    /// </summary>
    public class CrownView : MonoBehaviour
    {
        [SerializeField] private List<Image> _images = new();
        [SerializeField] private List<AudioClip> crownClips = new();

        private GameplayResulter _resulter;

        private void Start()
        {
            _resulter = GameplayEntry.Instance.ServiceLocator.Get<GameplayResulter>();
            Display();
        }

        private async void Display()
        {
            if (_resulter is null) return;

            var crownCount = _resulter.GetCrownsCount();

            if (crownCount > 0 && crownCount <= crownClips.Count)
            {
                AudioHelper.PlayAudioClip(crownClips[crownCount - 1]);
            }
            else if (crownCount > crownClips.Count)
            {
                AudioHelper.PlayAudioClip(crownClips[^1]);
            }

            for (int i = 0; i < crownCount; i++)
            {
                if (_images.Count > i)
                {
                    var icon = _images[i];

                    if (icon)
                    {
                        await icon.DOFade(1, 0.4f).From(0).AsyncWaitForCompletion();
                    }
                }
            }
        }
    }
}
