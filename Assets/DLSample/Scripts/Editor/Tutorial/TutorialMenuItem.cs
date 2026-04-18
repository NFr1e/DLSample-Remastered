using UnityEngine;
using UnityEditor;
using DLSample.Shared;

namespace DLSample.Editor.Tutorial
{
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
