#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Galleon.Checkout
{
    public class ProjectExtentions : MonoBehaviour
    {
        #if UNITY_EDITOR
        [MenuItem("Assets/Create/Galleon/View")]
        #endif
        public static void CreateView()
        {
            string path = GetSelectedPath();
            if (path == "No folder selected")
            {
                EditorUtility.DisplayDialog("Error", "Please select a folder first", "OK");
                return;
            }

            string viewName = Path.GetFileName(path);
            CreateViewStructure(path, viewName);
        }

        private static string GetSelectedPath()
        {
            string[] selectedPaths = Selection.assetGUIDs;
            if (selectedPaths.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(selectedPaths[0]);
            }
            return "No folder selected";
        }

        private static void CreateViewStructure(string basePath, string viewName)
        {
            // Create View subfolder
            string viewFolderPath = Path.Combine(basePath, "View");
            if (!Directory.Exists(viewFolderPath))
            {
                Directory.CreateDirectory(viewFolderPath);
                AssetDatabase.Refresh();
            }

            // Create Element
            CreateElement(basePath, viewName);

            // Create View Script first so it's available for the prefab
            CreateViewScript(viewFolderPath, viewName);

            // Create Prefab
            CreatePrefab(viewFolderPath, viewName);

            AssetDatabase.Refresh();
        }

        private static void CreateElement(string path, string viewName)
        {
            // Create Element scriptable object
            var element = ScriptableObject.CreateInstance<Element>();
            element.Name = viewName;

            // Ensure the path exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Save the asset with proper path
            string elementPath = $"{path}/{viewName}.element.asset";
            AssetDatabase.CreateAsset(element, elementPath);
            EditorUtility.SetDirty(element);
            AssetDatabase.SaveAssets();
        }

        private static void CreatePrefab(string path, string viewName)
        {
            // Create a new GameObject
            var go = new GameObject(viewName);
            
            // Add the view component
            string scriptPath = Path.Combine(path, $"{viewName}View.cs");
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (monoScript != null)
            {
                var scriptType = monoScript.GetClass();
                if (scriptType != null)
                {
                    go.AddComponent(scriptType);
                }
            }
            
            // Save as prefab
            string prefabPath = Path.Combine(path, $"{viewName}.prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            
            // Clean up
            DestroyImmediate(go);
        }

        private static void CreateViewScript(string path, string viewName)
        {
            // Create the script file
            string scriptPath = Path.Combine(path, $"{viewName}View.cs");
            string scriptContent = $@"
using UnityEngine;

namespace Galleon.Checkout.UI
{{
    public class {viewName}View : View
    {{
        private void Start()
        {{
            // Initialize view
        }}
    }}
}}";

            File.WriteAllText(scriptPath, scriptContent);
            AssetDatabase.Refresh();
        }
    }
}

#endif