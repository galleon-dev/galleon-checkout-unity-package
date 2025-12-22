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
    [Element("Thing")]
    public class Thing : Element
    {
        public Thing(string name) : base(name)
        {
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.RT
{
    [Element("Thing")]
    public class Thing : Entity
    {
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Assets
{
    [Element("Thing")]
    public class Thing : Asset
    {
        public void CreateThingAsset() 
        {
            #if UNITY_EDITOR   
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
        }
        
        ////////////////////////////////////////////////////////////////////////
        
        public class LiveComponent : Foundation.LiveComponent<Thing> 
        {
            public override void Create()                        => Target.CreateThingAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Hierarchy
{
    [Element("Thing")]
    public class Thing : Entity
    {
        public void CreateThingAsset() { Debug.Log($"Created Thing Hierarchy {Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name}"); } 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<Thing> 
        {
            public override void Create()                        => Target.CreateThingAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}
