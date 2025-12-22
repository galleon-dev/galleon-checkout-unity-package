using System;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        public void CreateSceneAsset() 
        {
            #if UNITY_EDITOR
            
            var name       = Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name;                                                         Debug.Log($"name        : {name}");
            var scene      = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);     Debug.Log($"scene       : {scene.name} - {scene.IsValid()} - {scene.path}");
          //var parentPath = this.Path;                                                                                                         Debug.Log($"parent path : {parentPath}");
          //var path       = $"{parentPath}/{name}.unity";                                                                                      Debug.Log($"path        : {path}");
            var path       = this.Path;                                                                                                         Debug.Log($"path        : {path}");
            
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"Created Scene Asset {path}");
            
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
            if (parent is not Asset parentAsset)
                throw new Exception("Assets.Scene.OnAddedToParent(): Parent is not Asset");

            var name       = Node.GetData<EntityNode.CRUD_Params>("CRUD_params")?.Name;
            this.Path = System.IO.Path.Combine(parentAsset.Path, name + ".unity");
        }
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<Scene> 
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
        public class LiveComponent : Foundation.LiveComponent<Scene> 
        {
            public override void Create()                        => Target.CreateSceneAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

/// [Pps1] :
///     > print scene Asset             [V]
///     > DONT print scene hierarchy    [ ]
///     > Physycal state aplly refresh  [ ]
///     > Entity                        [ ]
///             > PME                   [ ] 
///             > CRUD                  [ ]
///                 > CRUD_Params       [ ]
///             
