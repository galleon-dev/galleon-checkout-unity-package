using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using UnityEngine;
using UnityEngine.Assertions.Must;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
#endif

namespace Galleon.Checkout
{
    public class Slice : EntityBehaviour
    {

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Step RuntimeStart() 
            =>
            new Step(name   : $"slice_runtime_start"
                    ,action : async (s) =>
                    {   
                    });
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Step PrintMe() 
        =>
            new Step(name   : $"print_slice"
                    ,action : async (s) =>
                    {
                        //this.Node.SessionStorage.Remove("SliceOperationState");
                        //return;
                        //////////////////////////////////////////////////////////////////////////
                        if (!this.Node.SessionStorage.HasKey("SliceOperationState"))
                            this.Node.SessionStorage.Store("SliceOperationState", "start");
                        string operationState    = this.Node.SessionStorage.Load<string>("SliceOperationState");
                        string SliceName         = "Slice_1";
                        string folderName        = $"package_{SliceName}";
                        string folderPath        = Path.Combine(Application.dataPath, "TEMP", folderName);
                        string sceneName         = $"{SliceName}.unity";
                        string scenePath         = Path.Combine(folderPath, sceneName);
                        string relativeScenePath = "Assets" + scenePath.Substring(Application.dataPath.Length);
                        //////////////////////////////////////////////////////////////////////////
                        if (operationState == "start")
                            goto start;
                        if (operationState == "after_domain_reload")
                            goto after_domain_reload;
                        //////////////////////////////////////////////////////////////////////////
                        start:
                        Debug.Log("start");
                        //////////////////////////////////////////////////////////////////////////
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);
                        //////////////////////////////////////////////////////////////////////////
                        #if UNITY_EDITOR
                        var    newScene          = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                        EditorSceneManager.SaveScene (newScene, relativeScenePath);
                        EditorSceneManager.CloseScene(newScene, true);
                        #endif
                        //////////////////////////////////////////////////////////////////////////
                        string scriptName    = $"{SliceName}.cs";
                        string scriptPath    = Path.Combine(folderPath, scriptName);
                        string scriptContent = $@"using UnityEngine;

namespace Galleon.Checkout
{{
    public class {SliceName} : Slice
    {{
    }}
}}
";
                        operationState = "after_domain_reload";
                        this.Node.SessionStorage.Store("SliceOperationState", operationState);
                        File.WriteAllText(scriptPath, scriptContent);
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                        await Task.Delay(5000);
                        //////////////////////////////////////////////////////////////////////////
                        after_domain_reload:
                        Debug.Log("after_domain_reload");
                        //////////////////////////////////////////////////////////////////////////
                        #if UNITY_EDITOR
                        var scene = EditorSceneManager.OpenScene(relativeScenePath, OpenSceneMode.Additive);
                        GameObject sliceObject = new GameObject(SliceName);
                        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(sliceObject, scene);
                        var componentType = System.Type.GetType($"Galleon.Checkout.{SliceName}, Assembly-CSharp");
                        if (componentType != null)
                        {
                            sliceObject.AddComponent(componentType);
                        }
                        EditorSceneManager.SaveScene(scene);
                        EditorSceneManager.CloseScene(scene, true);
                        #endif
                        //////////////////////////////////////////////////////////////////////////
                        this.Node.SessionStorage.Remove("SliceOperationState");
                        Debug.Log("done");
                        //////////////////////////////////////////////////////////////////////////
                         
                    });
        
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
    }
}

