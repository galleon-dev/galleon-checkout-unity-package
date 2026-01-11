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
    [Element("Prefab")]
    public class Prefab : Element
    {
        public Prefab(string name) : base(name)
        {
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.RT
{
    [Element("Prefab")]
    public class Prefab : Entity
    {
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Assets
{
    [Element("Prefab")]
    public class Prefab : Asset
    {
        public void CreatePrefabAsset() 
        {
            #if UNITY_EDITOR   
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
        }
        
        ////////////////////////////////////////////////////////////////////////
        
        public class LiveComponent : Foundation.LiveComponent<Prefab> 
        {
            public override void Create()                        => Target.CreatePrefabAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Hierarchy
{
    [Element("Prefab")]
    public class Prefab : Entity
    {
        public void CreatePrefabAsset() { Debug.Log($"Created Prefab Hierarchy {Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name}"); } 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<Prefab> 
        {
            public override void Create()                        => Target.CreatePrefabAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}
