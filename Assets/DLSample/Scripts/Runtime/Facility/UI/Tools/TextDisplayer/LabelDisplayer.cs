using System;

namespace DLSample.Facility.UI
{
    [Serializable]
    public class LabelDisplayer
    {
        public Label label;

        public void SetText(string content, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(content))
            {
                content = defaultValue;
            }

            label.SetText(content);
        }
    }
    public static class LableDisplayerExtensions
    {
        public static void DOText(this LabelDisplayer displayer, string content)
        {
            
        }
    }
}
