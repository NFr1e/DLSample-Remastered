using UnityEngine;
using UnityEditor;
using DLSample.Shared;

namespace DLSample.Editor.Tutorial
{
    /// <summary>
    /// 教程菜单项，通过编辑器菜单打开外部教程链接。
    /// </summary>
    public class TutorialMenuItem
    {
        [MenuItem(
            itemName: DLSampleConsts.Editor.MENU_ITEM_TUTORIAL,
            priority = DLSampleConsts.Editor.MENU_ITEM_TUTORIAL_PRIORITY)]
        private static void OpenUrl()
        {
            Application.OpenURL(DLSampleConsts.Others.URL_TUTORIAL);
        }
    }
}
