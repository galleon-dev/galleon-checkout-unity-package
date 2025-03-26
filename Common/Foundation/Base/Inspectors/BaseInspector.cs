using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class BaseInspector : VisualElement
    {
        public BaseInspector()
        {
            this.Add(new Button() { text = "Base Inspector" } );
        }
    }
}
