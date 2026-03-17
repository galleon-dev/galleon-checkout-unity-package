#if UNITY_EDITOR

using System;
using System.IO;
using Galleon.Checkout;
using Galleon.Checkout.Symbols;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Symbols
{
    [Element("Component")]
    public class Component : Symbol
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
        
        ////////////////////////////////////////////////////////////////
        
        public void Do_H()
        {
            var PrefabPath   = "";
            var ScriptPath   = "";
            var MaterialPath = "";
           
            //////////////////////////////////////////// Open prefab for edit
            
            string     prefabAssetPath = PrefabPath;
            string     relativePath    = "Assets" + PrefabPath.Substring(Application.dataPath.Length);
            GameObject prefabRoot      = PrefabUtility.LoadPrefabContents(relativePath);
            
            if (prefabRoot == null)
            {
                Debug.LogError($"Failed to load prefab at path: {relativePath}");
                return;
            }
            
            //////////////////////////////////////////// Add component from script
            
            string      scriptName = Path.GetFileNameWithoutExtension(ScriptPath);
            System.Type scriptType = System.Type.GetType($"TEST_THING.{scriptName}" + ", Assembly-CSharp");
            
            if (scriptType != null)
            {
                prefabRoot.AddComponent(scriptType);
                Debug.Log($"Added component {scriptName} to prefab");
            }
            else
            {
                Debug.LogError($"Could not find script type {scriptName}. Make sure the script has been compiled.");
            }
            
            //////////////////////////////////////////// Add cube named "model"
            
            GameObject cubeObj              = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeObj.name                    = "model";
            cubeObj.transform               .SetParent(prefabRoot.transform);
            cubeObj.transform.localPosition = Vector3.zero;
            cubeObj.transform.localScale    = Vector3.one;
            
            //////////////////////////////////////////// Add material to cube
            
            string   materialAssetPath = "Assets" + MaterialPath.Substring(Application.dataPath.Length);
            var      material          = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(materialAssetPath);
            
            if (material != null)
            {
                MeshRenderer renderer = cubeObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                    Debug.Log($"Applied material to cube model");
                }
            }
            else
            {
                Debug.LogError($"Could not find material at path: {materialAssetPath}");
            }
            
            //////////////////////////////////////////// Save the prefab
            
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            
            ////////////////////////////////////////////
            
            Debug.Log($"Finished updating prefab at {prefabAssetPath}");
        }
    }    
}


#endif