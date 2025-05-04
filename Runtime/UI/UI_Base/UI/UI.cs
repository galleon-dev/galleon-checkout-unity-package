using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class UI : Entity
    {
        //// Members
        
        GameObject         UIRoot;
        
        //// Properties
        
        public IEnumerable<UI_Element> UI_Elements => UIRoot.GetComponentsInChildren<UI_Element>();

        //// Lifecycle
        
        public UI(GameObject uiRoot)
        {
            this.UIRoot = uiRoot;
        }
        
        //// API
       
        public IEnumerable<T> Q<T>() where T : Component
        {
            return UIRoot.GetComponentsInChildren<T>();
        }
    }

}