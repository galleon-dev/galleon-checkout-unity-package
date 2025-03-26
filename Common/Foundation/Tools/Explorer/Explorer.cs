using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class Explorer : VisualElement
    {
        public object RootObject { get; private set; } = Root.Instance;
        
        public Explorer()
        {           
            EntityInspector rootObjectInspector = new EntityInspector(RootObject);
            this.Add(rootObjectInspector);
        }
    }
}
