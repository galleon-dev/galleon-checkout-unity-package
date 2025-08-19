using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class OperationController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        public static void InitializeOnLoad()
        {
            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test
        
        public Operation TestOperation => new Operation (ID : "test_operation")
                                          .AddStep(new Step(name   : "test_step_1_creat_asset"
                                                           ,action : async s =>
                                                                   {
                                                                       #if UNITY_EDITOR

                                                                       // create prefab
                                                                       var go = new GameObject("Test");
                                                                       var prefabPath = "Assets/TEMP/Test.prefab";
                                                                       PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                                                                       GameObject.DestroyImmediate(go);

                                                                       // create a new default script at path
                                                                       string scriptPath = "Assets/TEMP/Test.cs";
                                                                       string scriptContent = 
                                                                       @"using UnityEngine;

                                                                       public class Test : MonoBehaviour 
                                                                       {
                                                                           void Start()
                                                                           {
                                                                           }

                                                                           void Update()
                                                                           {
                                                                           }
                                                                       }";
                                                                       System.IO.File.WriteAllText(scriptPath, scriptContent);
                                                                                                                                               
                                                                       
                                                                       #endif
                                                                       
                                                                   }))
                                          .AddStep(new Step(name   : "test_step_2_reload"
                                                           ,action : async s => 
                                                                   {
                                                                       #if UNITY_EDITOR
                                                                       
                                                                       AssetDatabase.Refresh();
                                                                       
                                                                       #endif
                                                                   }))
                                          .AddStep(new Step(name   : "test_step_1_use_asset"
                                                           ,action : async s =>
                                                                   {
                                                                       #if UNITY_EDITOR

                                                                       // Load the prefab asset
                                                                       string prefabPath = "Assets/TEMP/Test.prefab";
                                                                       GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                                                                       // Load the script
                                                                       string scriptPath = "Assets/TEMP/Test.cs";
                                                                       MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

                                                                       // Add component
                                                                       GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                                                                       prefabInstance.AddComponent(script.GetClass());

                                                                       // Save changes back to prefab
                                                                       PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                                                                       GameObject.DestroyImmediate(prefabInstance);

                                                                       #endif
                                                                   })
                                          );
        
        public Step ExecuteTestOperation() 
        =>
            new Step(name   : $"TestOperation"
                    ,action : async (s) =>
                    {
                        await TestOperation.Execute();
                    });
        }
}