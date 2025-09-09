using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class OperationController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public string OperationsStateFileRelativePath => $"TEMP/ongoing_operations.json";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
       
        public List<string> OngoingOperations = new();
        
        public List<Operation> AllRegisteredOperations;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task ResumeAllOngoingOperations()
        {
            await Load();
            
            List<Operation> operations = new();

            foreach (var operationID in OngoingOperations)
            {
                var op = Operation.Load(operationID);
                operations.Add(op);
            }

            for (int i = 0; i < operations.Count; i++)
            {
                var op = operations[i];
                Debug.Log($"Resuming : {op.ID}");
                await op.Resume();
            }
        }
        
        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(OngoingOperations);
            var path = Path.Combine(Application.dataPath, OperationsStateFileRelativePath);
            File.WriteAllText(path:path, contents:json);
        }
        
        public async Task Load()
        {
            try
            {
                var path = Path.Combine(Application.dataPath, OperationsStateFileRelativePath);

                if (!File.Exists(path))
                    throw new Exception($"Failed to load ongoing operations");
            
                var json = File.ReadAllText(path);
                OngoingOperations = JsonConvert.DeserializeObject<List<string>>(json);
                
            }
            catch (Exception e)
            {
                //Debug.LogError(e.Message);
                OngoingOperations = new List<string>();
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        public static async void InitializeOnLoad()
        {
            await Task.Yield();
            
            Root.Instance.Context.Operations.AllRegisteredOperations = Root.Instance.Node.Descendants().SelectMany(x => x.Node.Reflection.Operations()).ToList();
            
            await Root.Instance.Context.Operations.ResumeAllOngoingOperations();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Tests
        #region Tests
        
        // TEMP
        
        
        public Operation TestOperation => new Operation 
                                          (
                                              ID : "test_operation"
                                          )
                                          .AddStep(new Step(name   : "test_step_0_start_asset_editing"
                                                           ,action : async s =>
                                                                   {
                                                                       #if UNITY_EDITOR
                                                                       
                                                                       AssetDatabase.StartAssetEditing();
                                                                       
                                                                       #endif
                                                                   }))
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
                                                                       
                                                                       AssetDatabase.StopAssetEditing();
                                                                       AssetDatabase.Refresh();
                                                                       
                                                                       await Task.Delay(5000);
                                                                       
                                                                       #endif
                                                                   }))
                                          .AddStep(new Step(name   : "test_step_3_use_asset"
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
    
        
        #endregion // Tests
        
    }
}

