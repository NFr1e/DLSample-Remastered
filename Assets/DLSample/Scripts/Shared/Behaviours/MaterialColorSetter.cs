using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 材质颜色设置器，在启用时批量设置材质颜色
    /// </summary>
    [ExecuteAlways]
    public class MaterialColorSetter : MonoBehaviour
    {
        [Serializable]
        public class MaterialData
        {
            public Material material;
            public Color color = Color.white;

            [Button("GetColor", ButtonHeight = 30)]
            private void GetColor()
            {
                if (material)
                    color = material.color;
            }

            public void SetColor()
            {
                if (material)
                {
                    material.color = color;
                }
            }
        }

        [SerializeField]
        private List<MaterialData> _colors = new();

        private void OnEnable()
        {
            SetColor();
        }

        private void SetColor()
        {
            foreach (var item in _colors)
            {
                item.SetColor();
            }
        }
    }
}
