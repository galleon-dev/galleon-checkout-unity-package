using System;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.ELEMENTS
{
    [Element("QuickThing")]
    public class QuickThing : Element
    {
        public QuickThing(string name) : base(name)
        {
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.RT
{
    [Element("QuickThing")]
    public class QuickThing : Entity
    {
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Assets
{
    [Element("QuickThing")]
    public class QuickThing : Asset, ICRUD
    {
        public void CreateQuickThingAsset() 
        {
            #if UNITY_EDITOR
            
            Debug.Log($"Creating QuickThing Asset at {FolderPath}");
            
            // Ensure the folder exists
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
             
             // Create all components t
             CreatePrefab  (thingName: "quick_thing");
             CreateScript  (thingName: "quick_thing");
             CreateMaterial(thingName: "quick_thing");
            
            #endif
        }
        
        public void OnAddedToParent (IEntity parent)
        {
            this.FolderPath = (this.Node.Parent as Asset)?.FolderPath + "Thing";   
        }
        
        //////////////////////////////////////////////////////////////////////// ICRUD
       
        public void Create() => CreateQuickThingAsset(); 
        
        //////////////////////////////////////////////////////////////////////// Helper Methods
        #region Helper Methods
         
        /// <summary>
        /// Creates a prefab with the given name
        /// </summary>
        /// <param name="thingName">Name of the prefab to create</param>
        private void CreatePrefab(string thingName)
        {
            #if UNITY_EDITOR
            
            // Create a simple GameObject
            GameObject gameObject = new GameObject(thingName);
            
            // Create prefab path
            string prefabFolder = System.IO.Path.Combine(FolderPath, "Prefabs");
            if (!Directory.Exists(prefabFolder))
                Directory.CreateDirectory(prefabFolder);
            
            // Convert to relative asset path
            string relativePath = "Assets" + prefabFolder.Substring(Application.dataPath.Length);
            string prefabPath = System.IO.Path.Combine(relativePath, thingName + ".prefab");
            
            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            
            // Destroy the temporary GameObject
            UnityEngine.Object.DestroyImmediate(gameObject);
            
            Debug.Log($"Created prefab: {prefabPath}");
            
            #endif // UNITY_EDITOR
        }

        /// <summary>
        /// Creates a MonoBehaviour script with the given name
        /// </summary>
        /// <param name="thingName">Name of the script to create</param>
        private void CreateScript(string thingName)
        {
            #if UNITY_EDITOR
            
            // Create script folder
            string scriptFolder = System.IO.Path.Combine(FolderPath, "Scripts");
            if (!Directory.Exists(scriptFolder))
            {
                Directory.CreateDirectory(scriptFolder);
            }
            
            // Script template
            string scriptTemplate = 
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEST_THING
{
    public class CLASS_NAME : MonoBehaviour
    {
        void Start()
        {
            
        }
    }
}
";

            // Format the template with the thing name
            string scriptContent = scriptTemplate.Replace("CLASS_NAME", thingName);
            
            // Write the script file
            string scriptPath = System.IO.Path.Combine(scriptFolder, thingName + ".cs");
            File.WriteAllText(scriptPath, scriptContent);
            
            // Refresh the asset database
            AssetDatabase.Refresh();
            
            Debug.Log($"Created script: {scriptPath}");
            
            #endif // UNITY_EDITOR
        }
        
        /// <summary>
        /// Creates a material with the given name
        /// </summary>
        /// <param name="thingName">Name of the thing to create material for</param>
        private void CreateMaterial(string thingName)
        {
            #if UNITY_EDITOR
            
            // Create material folder
            string materialFolder = System.IO.Path.Combine(FolderPath, "Materials");
            if (!Directory.Exists(materialFolder))
                Directory.CreateDirectory(materialFolder);
            
            // Create a new material
            UnityEngine.Material material = new UnityEngine.Material(Shader.Find("Standard"));
            material.color = Color.white;
            
            // Convert to relative asset path
            string relativePath = "Assets" + materialFolder.Substring(Application.dataPath.Length);
            string materialPath = System.IO.Path.Combine(relativePath, thingName + "_material.mat");
            
            // Save the material as an asset
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created material: {materialPath}");
            
            #endif // UNITY_EDITOR
        }
         
          
         #endregion // Helper Methods
        
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Galleon.Checkout.Hierarchy
{
    [Element("QuickThing")]
    public class QuickThing : Entity
    {
        public void CreateQuickThingAsset() {} 
        public void OnAddedToParent (IEntity Parent) {} 
        
        ////////////////////////////////////////////////////////////////////////
        public class LiveComponent : Foundation.LiveComponent<QuickThing> 
        {
            public override void Create()                        => Target.CreateQuickThingAsset();
            public override void OnAddedToParent(IEntity Parent) => Target.OnAddedToParent(Parent);
        }
    }
}

/// w1 - Direct / Indirect
/// w2 - Thing + Tags
/// w3 - Prompt
/// w4 - Elements