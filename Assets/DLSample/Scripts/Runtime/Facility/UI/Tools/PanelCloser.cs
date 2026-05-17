using DLSample.App;
using UnityEngine;

namespace DLSample.Facility.UI
{
    public class PanelCloser : MonoBehaviour
    {
        public async void ClosePanel() => await AppEntry.UIManager.CloseCurrentFullScreenPanel();
    }
}
