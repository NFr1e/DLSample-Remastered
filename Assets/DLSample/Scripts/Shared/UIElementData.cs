using DLSample.Facility.UI;
using System;
using UnityEngine;

namespace DLSample.Shared.UI
{
    /// <summary>
    /// UI元素数据泛型结构，包含元素ID和元素引用
    /// </summary>
    /// <typeparam name="T">UI元素类型</typeparam>
    [Serializable]
    public struct UIElementData<T> where T : UIElement
    {
        public readonly T Item
        {
            get
            {
                return _item;
            }
        }

        public readonly string ItemId
        {
            get
            {
                return _itemId;
            }
        }

        [SerializeField] string _itemId;
        [SerializeField] T _item;

        public readonly override bool Equals(object obj)
        {
            if (obj is UIElementData<T> element)
            {
                return element.ItemId == ItemId;
            }

            return false;
        }
        public readonly override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
