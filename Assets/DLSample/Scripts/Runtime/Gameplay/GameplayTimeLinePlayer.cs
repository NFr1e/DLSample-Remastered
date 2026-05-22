using UnityEngine;
using UnityEngine.Playables;

namespace DLSample.Gameplay.Stream
{
    public class GameplayTimeLinePlayer : IStreamPlayer
    {
        private readonly PlayableDirector _director;
        private bool _isStarted = false;

        public GameplayTimeLinePlayer(PlayableDirector playeble)
        {
            _director = playeble;
        }

        public bool IsPlaying { get; private set; }
        public double CurrentTime => _director.time;

        public void Play()
        {
            if (IsPlaying) return;

            if (!_isStarted)
            {
                _director.Play();
            }
            else
            {
                _director.Resume();
            }
            IsPlaying = true;
            _isStarted = true;
        }
        public void Stop()
        {
            if (!IsPlaying) return;

            _director.Pause();
            IsPlaying = false;
        }
        public void Seek(double timeSecond)
        {
            timeSecond = Mathf.Max(0f, (float)timeSecond);

            _director.time = timeSecond;
            _director.Evaluate();
        }
    }
}
