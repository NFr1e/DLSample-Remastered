using DLSample.Framework;
using DLSample.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Gameplay.Stream
{
    public class GameplayTimer : IModule, IStreamPlayer
    {
        int IModule.Priority => DLSampleConsts.Gameplay.PRIORITY_GAMEPLAY_TIMER;

        public bool IsPlaying { get; private set; } = false;
        public double CurrentTime { get; private set; } = 0;

        public void Tick(float deltaTime)
        {
            if (!IsPlaying) return;

            CurrentTime += deltaTime;
            ProcessTickEvents(CurrentTime);
        }

        #region Playback
        public void Play()
        {
            if (IsPlaying) return;
            IsPlaying = true;
        }

        public void Stop()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
        }

        public void Seek(double timeSecond)
        {
            CurrentTime = Math.Max(0.0, timeSecond);

            ResetCursor();
        }
        #endregion

        #region TickEventSystem
        public struct TickEvent
        {
            public double Time { get; set; }
            public Action Callback { get; set; }

            public TickEvent(double time, Action callback)
            {
                Time = time;
                Callback = callback;
            }
        }

        private readonly List<TickEvent> _tickEvents = new();
        private int _pendingEventIndex = 0;

        public TickEvent RegisterTickEvent(TickEvent tickEvent)
        {
            int index = _tickEvents.BinarySearch(tickEvent, Comparer<TickEvent>.Create((a, b) => a.Time.CompareTo(b.Time)));
            if (index < 0) index = ~index;

            _tickEvents.Insert(index, tickEvent);

            ResetCursor();

            return tickEvent;
        }

        public bool UnregisterTickEvent(TickEvent tickEvent)
        {
            bool removed = _tickEvents.Remove(tickEvent);
            if (removed)
            {
                ResetCursor();
            }
            return removed;
        }

        private void ProcessTickEvents(double currentTime)
        {
            while (_pendingEventIndex < _tickEvents.Count)
            {
                if (_tickEvents[_pendingEventIndex].Time > currentTime)
                    break;

                var evt = _tickEvents[_pendingEventIndex];

                _pendingEventIndex++;

                try
                {
                    evt.Callback?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void ResetCursor()
        {
            int left = 0;
            int right = _tickEvents.Count - 1;
            int resultIndex = _tickEvents.Count;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (_tickEvents[mid].Time >= CurrentTime)
                {
                    resultIndex = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            _pendingEventIndex = resultIndex;
        }
        #endregion
    }
}