using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace DLSample.Shared
{
    /// <summary>
    /// 音频播放辅助工具类
    /// </summary>
    public static class AudioHelper
    {
        /// <summary>
        /// 异步播放一段AudioClip
        /// </summary>
        /// <param name="clip">要播放的音频片段</param>
        /// <param name="volume">播放音量</param>
        /// <param name="onComplete">播放完成后的回调</param>
        public static async void PlayAudioClip(AudioClip clip, float volume = 1f, Action onComplete = default)
        {
            AudioSource source = new GameObject{
                hideFlags = HideFlags.HideAndDontSave,
            }.AddComponent<AudioSource>();

            source.clip = clip;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = volume;

            source.Play();

            await UniTask.Delay((int)(1000 * clip.length));

            onComplete?.Invoke();
            GameObject.Destroy(source.gameObject);
        }
    }
}
