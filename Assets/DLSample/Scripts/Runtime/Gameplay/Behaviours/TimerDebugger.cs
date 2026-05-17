using DLSample.Gameplay;
using DLSample.Gameplay.Stream;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 计时器调试显示组件，实时更新计时器文本。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TimerDebugger : GameplayObject
    {
        public Text timeText;
        private GameplayTimer _timer;

        protected override void OnStart()
        {
            _timer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
        }

        private void Update()
        {
            if (_timer != null)
            {
                timeText.text = _timer.CurrentTime.ToString();
            }
        }
    }
}
