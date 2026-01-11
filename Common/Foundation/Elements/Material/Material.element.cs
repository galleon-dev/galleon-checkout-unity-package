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
    [Element("Material")]
    public class Material : Element
    {
        public Material(string name) : base(name)
        {
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.RT
{
    [Element("Material")]
    public class Material : Entity
    {
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Assets
{
    [Element("Material")]
    public class Material : Asset
    {
        public void CreateMaterialAsset() 
        {
            #if UNITY_EDITOR   
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
        }
        
        ////////////////////////////////////////////////////////////////////////
        
        public class LiveComponent : Foundation.LiveComponent<Material> 
        {
            public override void Create()                        => Target.CreateMaterialAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Hierarchy
{
    [Element("Material")]
    public class Material : Entity
    {
        public void CreateMaterialAsset() {} 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<Material> 
        {
            public override void Create()                        => Target.CreateMaterialAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}
