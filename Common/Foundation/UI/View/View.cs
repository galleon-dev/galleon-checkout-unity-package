using UnityEngine;
using UnityEngine.Serialization;

namespace Galleon.Checkout.UI
{
    public class View : MonoBehaviour, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// IEntity
        
        public EntityNode Node { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public GameObject      GameObject;
        
        public LayoutDirection LayoutDirection = LayoutDirection.LeftToRight;
        public string          Local           = "en-us";
        public Appearance      appearance      = Appearance.System;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public View()
        {
            this.Node = new EntityNode(this);
        }   
    }
    
    public enum LayoutDirection
    {
        LeftToRight,
        RightToLeft
    }
    
    public enum Appearance
    {
        System,
        Light,
        Dark,
    }
}
