using DLSample.App;
using DLSample.Shared;
using DLSample.Gameplay.Stream;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 节拍图谱创建器，在玩家转向时记录时间戳生成节拍数据。
    /// </summary>
    public class BeatMapCreator : MonoBehaviour
    {
        public GameplayPlayerMove playerMove;
        public BeatmapDataScriptable data;

        [SerializeField, ReadOnly]
        private List<Beat> _beats = new();

        private GameplayTimer _timer;

        private void OnEnable()
        {
            playerMove.OnTurn += Record;
        }

        private void OnDisable()
        {
            playerMove.OnTurn -= Record;
        }

        private void Start()
        {
            GameplayEntry.Instance.ServiceLocator.TryGet<GameplayTimer>(out _timer);
        }

        private void Record(PlayerMovingArgs _)
        {
            if (_timer is not null)
            {
                var beat = new Beat(_timer.CurrentTime);
                _beats.Add(beat);
            }
        }

        [Button("Save")]
        private void Save()
        {
            data.SetBeats(_beats);
        }
    }
}
