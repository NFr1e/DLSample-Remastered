using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DLSample.Gameplay.Stream
{
    public class GameplaySoundtrackPlayer : IStreamPlayer
    {
        private readonly AudioClip _audioClip;
        private readonly AudioSource _audioSource;

        private Tweener _fadeoutTween;
        private UniTaskCompletionSource _prepareCompletion;

        public GameplaySoundtrackPlayer(AudioClip clip, AudioSource source)
        {
            _audioClip = clip;
            _audioSource = source;

            _audioSource.clip = _audioClip;
        }
        public void Init() { }
        public void Dispose() { }

        public UniTask PrepareAsync()
        {
            if (_audioClip == null)
            {
                return UniTask.CompletedTask;
            }

            if (_audioClip.loadState == AudioDataLoadState.Loaded)
            {
                return UniTask.CompletedTask;
            }

            if (_prepareCompletion != null)
            {
                return _prepareCompletion.Task;
            }

            _prepareCompletion = new UniTaskCompletionSource();
            PrepareInternalAsync().Forget();
            return _prepareCompletion.Task;
        }

        public bool IsPlaying { get; private set; } = false;
        public double CurrentTime
        {
            get
            {
                return _audioSource.time;
            }
        }

        public void Play()
        {
            if (IsPlaying) return;

            RestoreVolume();

            _audioSource.Play();
            IsPlaying = true;
        }
        public void Stop()
        {
            _audioSource.Pause();
            IsPlaying = false;
        }
        public void Seek(double timeSecond)
        {
            Stop();

            timeSecond = Mathf.Max(0f, (float)timeSecond);
            _audioSource.time = (float)timeSecond;
        }
        public void Fadeout()
        {
            IsPlaying = false;
            
            _fadeoutTween?.Kill();
            _fadeoutTween = _audioSource.DOFade(0, 3f).SetLink(_audioSource.gameObject);
        }
        private void RestoreVolume()
        {
            _fadeoutTween?.Kill();
            _fadeoutTween = _audioSource.DOFade(1, 0.3f);
        }

        private async UniTask PrepareInternalAsync()
        {
            if (_audioClip.loadState == AudioDataLoadState.Unloaded)
            {
                _audioClip.LoadAudioData();
            }

            await UniTask.WaitUntil(() => _audioClip.loadState != AudioDataLoadState.Loading);
            _prepareCompletion?.TrySetResult();
            _prepareCompletion = null;
        }
    }
}
