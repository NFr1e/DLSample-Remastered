using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DLSample.Shared
{
    /// <summary>
    /// 关卡数据配置，保存关卡场景、名称、音轨信息和统计数据
    /// </summary>
    [CreateAssetMenu(
        menuName = DLSampleConsts.Editor.CREATE_MENU_LEVELDATA_MENU_NAME,
        fileName = DLSampleConsts.Editor.CREATE_MENU_LEVELDATA_FILE_NAME,
        order = DLSampleConsts.Editor.CREATE_MENU_LEVELDATA_ORDER)]
    public class LevelDataScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        public SceneAsset levelScene;
#endif
        public string sceneName;
        public string levelName = "LevelName";
        public string soundtrackInfo = "SoundtrackInfo";
        public int gemCount = 10;
        public float levelLength = 100;

        public string SceneName => sceneName;
        public string LevelName => levelName;
        public string SoundtrackInfo => soundtrackInfo;
        public int GemCount => gemCount;
        public float LevelLength => levelLength;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelScene != null)
            {
                sceneName = levelScene.name;
            }
        }
#endif
    }
}
