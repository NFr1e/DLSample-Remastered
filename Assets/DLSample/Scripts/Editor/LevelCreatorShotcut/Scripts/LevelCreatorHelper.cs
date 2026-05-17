#if UNITY_EDITOR
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using DLSample.Shared;
using DLSample.Editor.PathGrapher;

namespace DLSample.Editor.LevelCreator
{
    /// <summary>
    /// 关卡创建辅助类，负责在指定路径下创建关卡所需的场景、ScriptableObject 资源及文件夹结构。
    /// </summary>
    public static class LevelCreatorHelper
    {
        /// <summary>
        /// 在指定基路径下创建完整的关卡资源（场景、LevelData、BeatmapData、PathGrapherAsset）。
        /// </summary>
        /// <param name="basePath">关卡基路径（必须以 Assets 开头）</param>
        /// <param name="levelName">关卡名称</param>
        /// <param name="soundtrackInfo">音轨信息</param>
        /// <param name="gemCount">宝石数量</param>
        /// <param name="soundtrackClip">音轨 AudioClip</param>
        /// <returns>是否成功创建关卡</returns>
        public static bool CreateLevel(string basePath, string levelName, string soundtrackInfo, int gemCount, AudioClip soundtrackClip)
        {
            // 验证
            if (string.IsNullOrEmpty(basePath) || !basePath.StartsWith("Assets")) return false;

            if (string.IsNullOrEmpty(levelName)) return false;

            var levelFolderPath = Path.Combine(basePath, levelName).Replace('\\', '/');
            var fullFolderPath = Path.Combine(Application.dataPath, levelFolderPath["Assets/".Length..]).Replace('\\', '/');

            if (Directory.Exists(fullFolderPath))
            {
                EditorUtility.DisplayDialog("Error", $"已存在: {levelFolderPath}", "OK");
                return false;
            }

            try
            {
                // 文件夹
                Directory.CreateDirectory(fullFolderPath);
                AssetDatabase.Refresh();

                // Resource 文件夹
                var resourcesFolder = Path.Combine(fullFolderPath, "Resources");
                Directory.CreateDirectory(resourcesFolder);
                AssetDatabase.Refresh();

                // 场景
                var scenePath = CreateEmptyScene(levelFolderPath, levelName);
                if (string.IsNullOrEmpty(scenePath))
                {
                    throw new System.Exception("Failed to create scene.");
                }

                // LevelData
                var levelDataPath = Path.Combine(levelFolderPath, $"LevelData_{levelName}.asset").Replace('\\', '/');
                var levelData = ScriptableObject.CreateInstance<LevelDataScriptable>();

                levelData.levelScene = GetSceneAssetAtPath(scenePath);
                levelData.sceneName = levelName;
                levelData.levelName = levelName;
                levelData.soundtrackInfo = soundtrackInfo;
                levelData.gemCount = gemCount;
                levelData.levelLength = soundtrackClip != null ? soundtrackClip.length : 0f;

                AssetDatabase.CreateAsset(levelData, levelDataPath);

                // BeatmapData
                var beatmapPath = Path.Combine(levelFolderPath, $"BeatmapData_{levelName}.asset").Replace('\\', '/');
                var beatmapData = ScriptableObject.CreateInstance<BeatmapDataScriptable>();
                AssetDatabase.CreateAsset(beatmapData, beatmapPath);

                // PathGrapherAsset
                var pathGrapherPath = Path.Combine(levelFolderPath, $"PathGrapherAsset_{levelName}.asset").Replace('\\', '/');
                var pathGrapher = ScriptableObject.CreateInstance<PathGrapherAsset>();
                AssetDatabase.CreateAsset(pathGrapher, pathGrapherPath);

                var loadedBeatmap = AssetDatabase.LoadAssetAtPath<BeatmapDataScriptable>(beatmapPath);
                var loadedPathGrapher = AssetDatabase.LoadAssetAtPath<PathGrapherAsset>(pathGrapherPath);
                loadedPathGrapher.beatMapData = loadedBeatmap;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"关卡 [\"{levelName}\"] 创建成功，路径:\n {levelFolderPath}", "OK");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create level: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"创建失败: {ex.Message}", "OK");
                return false;
            }
        }

        private static string CreateEmptyScene(string folderRelativePath, string sceneName)
        {
            var originalSceneDirty = SceneManager.GetActiveScene().isDirty;

            if (originalSceneDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scenePath = Path.Combine(folderRelativePath, $"{sceneName}.unity").Replace('\\', '/');

            EditorSceneManager.SaveScene(newScene, scenePath);

            return scenePath;
        }

        private static SceneAsset GetSceneAssetAtPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }
    }
}
#endif