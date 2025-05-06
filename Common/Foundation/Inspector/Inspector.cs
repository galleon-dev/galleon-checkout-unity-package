using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class Inspector : VisualElement
    {
        //// Members
        
        public object Target;
        public bool   IsSelected = false;
        
        //// Properties
        
        public Explorer Explorer => Explorer.Instance;
        
        //// UI Members
        
        public Label TitleLabel;
        
        //// Lifecycle
        
        public Inspector(object target)
        {
            this.Target = target;
            
            TitleLabel = new Label(this.GetType().Name);
        }
    }
    
    public class Inspector<T> : Inspector
    {
        public new T Target => (T)base.Target;

        public Inspector(T target) : base(target)
        {
            TitleLabel.text = $"{typeof(T).Name} Inspector {this}";
        }
    }
}


