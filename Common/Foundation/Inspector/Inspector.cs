using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class Inspector : VisualElement
    {
        public object Target;

        public Inspector(object target)
        {
            this.Target = target;
        }
    }
    
    public class Inspector<T> : Inspector
    {
        public new T Target => (T)base.Target;

        public Inspector(T target) : base(target)
        {
            
        }
    }
}


