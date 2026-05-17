using DLSample.App;
using UnityEngine;

namespace DLSample.Facility.UI
{
    public class PanelOpener : MonoBehaviour
    {
        [SerializeField] string panelId = string.Empty;

        public async void OpenPanel()
        {
            await AppEntry.UIManager.OpenPanel(panelId);
        }
    }
}
