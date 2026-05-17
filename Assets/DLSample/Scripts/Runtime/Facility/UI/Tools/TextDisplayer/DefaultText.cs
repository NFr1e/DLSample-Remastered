using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Facility.UI
{
    public class DefaultText : Label
    {
        [SerializeField] Text text;

        public override void SetText(string content)
        {
            text.text = content;
        }
    }
}
