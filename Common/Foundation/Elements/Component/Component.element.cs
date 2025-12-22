using System;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.ELEMENTS
{
    [Element("Component")]
    public class Component : Element
    {
        public Component(string name) : base(name)
        {
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.RT
{
    [Element("Component")]
    public class Component : Entity
    {
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Assets
{
    [Element("Component")]
    public class Component : Asset
    {
        public void CreateComponentAsset() 
        {
            #if UNITY_EDITOR   
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
        }
        
        ////////////////////////////////////////////////////////////////////////
        
        public class LiveComponent : Foundation.LiveComponent<Component> 
        {
            public override void Create()                        => Target.CreateComponentAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Hierarchy
{
    [Element("Component")]
    public class Component : Entity
    {
        public void CreateComponentAsset() {} 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<Component> 
        {
            public override void Create()                        => Target.CreateComponentAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}
