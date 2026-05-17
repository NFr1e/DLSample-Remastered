using DLSample.Facility.UI;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 关卡名称显示视图，从结果器中读取并显示关卡名称。
    /// </summary>
    public class LevelNameView : MonoBehaviour
    {
        [SerializeField] private LabelDisplayer levelNameLabel;

        private GameplayResulter _resulter;

        private void Start()
        {
            _resulter = GameplayEntry.Instance.ServiceLocator.Get<GameplayResulter>();

            if (_resulter is not null)
            {
                levelNameLabel.SetText(_resulter.LevelData.LevelName);
            }
        }
    }
}
