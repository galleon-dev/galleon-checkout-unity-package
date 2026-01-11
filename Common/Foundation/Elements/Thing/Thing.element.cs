using System;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.ELEMENTS
{
    [Element("Thing")]
    public class Thing : Element
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types
        
        public class ThingParams
        {
            public string   Name;
            public string[] Tags;
            public string   Prompt;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string FolderPath => Application.dataPath + "/" + "package1/";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Thing(string name) : base(name)
        {
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task<VirtualEntity> Create(ThingParams @thingParams)
        {
            await CreateOp.Execute();
            return default;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Operation

        public Operation CreateOp => new Operation(id: "thing_operation")
                                         .AddStep(CreateThingAsset())
                                         .AddStep(new Step(name : "wait", action: async s => { await Task.Delay(5000); } ))
                                         .AddStep(CreateThingHierarchy())
                                         .AddStep(CreateThingApp());
                                        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps
        
        public Step CreateThingAsset() 
        =>
            new Step(name   : $"create_thing_asset"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR
            
                        ThingParams @params = new ThingParams()
                                            {
                                                Name   = "t1",
                                                Prompt = "",
                                                Tags   = new [] { "" }
                                            };

                        var path = $"{FolderPath}{@params.Name}";
                        Debug.Log($"Creating QuickThing Asset at {path}");

                        // Ensure the folder exists
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // Create all components t
                        CreatePrefab  (thingName: @params.Name);
                        CreateScript  (thingName: @params.Name);
                        CreateMaterial(thingName: @params.Name);

                        #endif
                    });
        
        
        public Step CreateThingHierarchy() 
        =>
            new Step(name   : $"create_thing_hierarchy"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR

                        ThingParams @params = new ThingParams()
                        {
                            Name   = "t1",
                            Prompt = "",
                            Tags   = new [] { "" }
                        };

                        // Get paths
                        string prefabPath   = System.IO.Path.Combine("Assets/package1/Thing/Prefabs",   @params.Name + ".prefab");
                        string materialPath = System.IO.Path.Combine("Assets/package1/Thing/Materials", @params.Name + "_material.mat");

                        // Load and instantiate the prefab
                        GameObject prefabAsset    = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;

                        // Add the script component
                        string scriptName = @params.Name;
                        Type scriptType   = System.Type.GetType("TEST_THING." + scriptName + ", Assembly-CSharp");
                        
                        if (scriptType != null)
                            prefabInstance.AddComponent(scriptType);

                        // Create child cube
                        GameObject modelGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        modelGO.name = "model";

                        // Load and assign material
                        var material = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(materialPath);
                        if (material != null)
                        {
                            MeshRenderer renderer = modelGO.GetComponent<MeshRenderer>();
                            if (renderer != null)
                            {
                                renderer.sharedMaterial = material;
                            }
                        }

                        // Set parent
                        modelGO.transform.SetParent(prefabInstance.transform, false);

                        // Save changes
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);

                        // Cleanup
                        UnityEngine.Object.DestroyImmediate(prefabInstance);
                        UnityEngine.Object.DestroyImmediate(modelGO);

                        #endif
                    });
        
        
        public Step CreateThingApp() 
        =>
            new Step(name   : $"create_thing_app"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR
            
                        #endif
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper methods
        
        #region Helper Methods
        
        /// <summary>
        /// Creates a prefab with the given name
        /// </summary>
        /// <param name="thingName">Name of the prefab to create</param>
        private void CreatePrefab(string thingName)
        {
            #if UNITY_EDITOR

            var path = $"{FolderPath}{thingName}";
            
            // Create a simple GameObject
            GameObject gameObject = new GameObject(thingName);

            // Create prefab path
            string prefabFolder = System.IO.Path.Combine(path, "Prefabs");
            if (!Directory.Exists(prefabFolder))
                Directory.CreateDirectory(prefabFolder);

            // Convert to relative asset path
            string relativePath = "Assets" + prefabFolder.Substring(Application.dataPath.Length);
            string prefabPath   = System.IO.Path.Combine(relativePath, thingName + ".prefab");

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

            var path = $"{FolderPath}{thingName}";
            
            // Create script folder
            string scriptFolder = System.IO.Path.Combine(path, "Scripts");
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

            var path = $"{FolderPath}{thingName}";
            
            // Create material folder
            string materialFolder = System.IO.Path.Combine(path, "Materials");
            
            if (!Directory.Exists(materialFolder))
                Directory.CreateDirectory(materialFolder);

            // Create a new material
            UnityEngine.Material material = new UnityEngine.Material(Shader.Find("Standard"));
            material.color                = Color.white;

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
