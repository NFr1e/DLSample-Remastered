using System.IO;
using UnityEngine;
using UnityEditor;

namespace DLSample.Editor.PathBuilder
{
    /// <summary>
    /// PathBuilder 编辑器设置，跨重编译/重启保持预制体引用。
    /// 资产自动创建于脚本所在目录的 EditorResources 子目录下。
    /// </summary>
    public class PathBuilderSettings : ScriptableObject
    {
        public GameObject PathPrefab;
        public float PathWidth = 3f;
        public PathGenerateType GenerateType = PathGenerateType.Disconnected;
        public GameObject HintBoxPrefab;
        public GameObject HintSegmentPrefab;

        private const string AssetFileName = "PathBuilderSettings.asset";
        private const string AssetFolderName = "EditorResources";

        public static PathBuilderSettings LoadOrCreate()
        {
            var settings = FindExisting();
            if (settings != null) return settings;

            settings = CreateInstance<PathBuilderSettings>();
            var dir = GetDefaultAssetDirectory();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, AssetFileName);
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public void MarkDirty()
        {
            EditorUtility.SetDirty(this);
        }

        private static PathBuilderSettings FindExisting()
        {
            var guids = AssetDatabase.FindAssets("t:PathBuilderSettings");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<PathBuilderSettings>(path);
                if (asset != null) return asset;
            }
            return null;
        }

        private static string GetDefaultAssetDirectory()
        {
            var script = MonoScript.FromScriptableObject(CreateInstance<PathBuilderSettings>());
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var scriptDir = Path.GetDirectoryName(scriptPath);
            return Path.Combine(scriptDir, AssetFolderName);
        }
    }
}
