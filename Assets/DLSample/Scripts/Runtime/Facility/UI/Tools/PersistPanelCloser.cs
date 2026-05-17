using DLSample.App;
using UnityEngine;

namespace DLSample.Facility.UI
{
    public class PersistPanelCloser : MonoBehaviour
    {
        [SerializeField] string panelId = string.Empty;

        public async void ClosePersistPanel()
        {
            await AppEntry.UIManager.ClosePersistentPanel(panelId);
        }
    }
}
