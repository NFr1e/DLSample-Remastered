using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DLSample.Facility.UI;

namespace DLSample.Shared.UI
{
    /// <summary>
    /// UI面板数据配置，管理全部可用UI面板
    /// </summary>
    [CreateAssetMenu(
        menuName = DLSampleConsts.Editor.CREATE_MENU_PANELS_MENU_NAME,
        fileName = DLSampleConsts.Editor.CREATE_MENU_PANELS_FILE_NAME,
        order = DLSampleConsts.Editor.CREATE_MENU_PANELS_ORDER)]
    public class UIPanelsDataScriptable : ScriptableObject
    {
        [SerializeField] private List<UIElementData<Panel>> panelsData;

        /// <summary>
        /// 根据ID获取对应的面板数据
        /// </summary>
        /// <param name="id">面板标识符</param>
        /// <param name="item">输出的面板数据项</param>
        /// <returns>是否成功找到有效面板</returns>
        public bool GetPanel(string id, out UIElementData<Panel> item)
        {
            item = panelsData.FirstOrDefault(x => x.ItemId == id);

            if (item.Item == null || string.IsNullOrEmpty(item.ItemId))
                return false;

            return true;
        }
    }
}
