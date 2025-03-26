using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public static class InspectorUtilities 
    {
    }
    
    public class Divider : VisualElement
    {
        public Divider()
        {
            Add(new VisualElement { style = { height = 1, backgroundColor = new Color(0.1f, 0.1f, 0.1f), marginTop = 8, marginBottom = 8 } });
        }
    }
}
