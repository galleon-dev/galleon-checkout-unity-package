using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using UnityEngine;

namespace Galleon.Checkout.ELEMENTS
{
    [Element("Scene")]
    public class Scene : Element
    {
        public Scene(string name) : base(name)
        {
        }
    }
}

namespace Galleon.Checkout.RT
{
    [Element("Scene")]
    public class Scene : Entity
    {
        
    }
}

namespace Galleon.Checkout.Assets
{
    [Element("Scene")]
    public class Scene : Asset
    {
        public void CreateSceneAsset() { Debug.Log($"Created Scene Asset {Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name}"); } 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveHandler : Foundation.LiveHandler<Scene> 
        {
            public override void Create()                        => Target.CreateSceneAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

namespace Galleon.Checkout.Hierarchy
{
    [Element("Scene")]
    public class Scene : Entity
    {
        public void CreateSceneAsset() { Debug.Log($"Created Scene Hierarchy {Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name}"); } 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveHandler : Foundation.LiveHandler<Scene> 
        {
            public override void Create()                        => Target.CreateSceneAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

/// [Pps1] :
///     > print scene Asset
///     > DONT print scene hierarchy
///     > Physycal state aplly refresh
///     > Entity
///             > PME
///             > CRUD
///                 > CRUD_Params
///             
