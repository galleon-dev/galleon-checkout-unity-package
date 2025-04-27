using System;
using System.Collections.Generic;
using System.Drawing;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Galleon.Checkout.UI
{
    public class View : MonoBehaviour, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// IEntity
        
        public EntityNode Node { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow
        
        public Step Flow;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public View()
        {
            this.Node  = new EntityNode(this);
            
            this.Flow = new Step(name: $"{this.GetType().Name}_Flow", tags: new []{"view_flow"});
        }
    }
}
