using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;
using UnityEngine;
using System.IO;
using System.Linq;

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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events

        public static Func<VirtualEntity, Task<string>> DoPrompt;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Thing(string name) : base(name)
        {
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        /// Supported Tags :
        /// Position    : 5,2       or  5,2,0 
        /// size        : 2x4       or  2,4,1
        /// color       : red       or  #113366FF
        /// collider    : collider  or  no-collider
        /// rigidboxy   : rb        or  no-rb
        
        public Vector3? GetPosition(string text)
        {
            // Return null if input text is empty or null
            if (string.IsNullOrEmpty(text))
                return null;

            // Remove parentheses and spaces for consistent formatting
            // e.g. "(1, 2)" or "1,2" both become "1,2"
            text = text.Trim().Trim('(', ')').Replace(" ", "");

            // Split text by comma to get individual coordinate values
            // e.g. "1,2" becomes ["1", "2"]
            string[] parts = text.Split(',');

            // Validate we have either 2D (x,y) or 3D (x,y,z) coordinates
            if (parts.Length != 2 && parts.Length != 3)
                return null;

            // Parse x,y coordinates, return null if either fails
            if (!float.TryParse(parts[0], out float x) || !float.TryParse(parts[1], out float y))
                return null;

            // Handle optional z coordinate, default to 0 if not provided
            float z = 0f;
            if (parts.Length == 3 && !float.TryParse(parts[2], out z))
                return null;

            return new Vector3(x, y, z);
        }

        public Vector3? GetPositionFromTags(string[] tags)
        {
            if (tags == null)
                return null;

            foreach (var tag in tags)
            {
                var position = GetPosition(tag);
                if (position.HasValue)
                    return position.Value;
            }

            return null;
        }

        public Vector3? GetSize(string text)
        {
            // Return null if input text is empty or null
            if (string.IsNullOrEmpty(text))
                return null;

            // Remove parentheses and spaces for consistent formatting
            text = text.Trim().Trim('(', ')').Replace(" ", "");

            // Split text by 'x' to get dimensions
            string[] parts = text.Split('x');

            // Validate we have either 2D (width x height) or 3D (width x height x depth) dimensions
            if (parts.Length != 2 && parts.Length != 3)
                return null;

            // Parse width and height, return null if either fails
            if (!float.TryParse(parts[0], out float width) || !float.TryParse(parts[1], out float height))
                return null;

            // Handle optional depth coordinate, default to 1 if not provided
            float depth = 1f;
            if (parts.Length == 3 && !float.TryParse(parts[2], out depth))
                return null;

            return new Vector3(width, height, depth);
        }

        public Vector3? GetSizeFromTags(string[] tags)
        {
            if (tags == null)
                return null;

            foreach (var tag in tags)
            {
                var size = GetSize(tag);
                if (size.HasValue)
                    return size.Value;
            }

            return null;
        }

        public Color? GetColor(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.ToLower().Trim();

            // Handle named colors
            switch (text)
            {
                case "red":            return     Color.red;
                case "dark_red":       return new Color(0.5f, 0f,   0f,   1f);
                case "light_red":      return new Color(1f,   0.4f, 0.4f, 1f);
                case "pastel_red":     return new Color(1f,   0.8f, 0.8f, 1f);
                case "blue":           return     Color.blue; 
                case "dark_blue":      return new Color(0f,   0f,   0.5f, 1f);
                case "light_blue":     return new Color(0.4f, 0.4f, 1f,   1f);
                case "pastel_blue":    return new Color(0.8f, 0.8f, 1f,   1f);
                case "green":          return     Color.green;
                case "dark_green":     return new Color(0f,   0.5f, 0f,   1f);
                case "light_green":    return new Color(0.4f, 1f,   0.4f, 1f);
                case "pastel_green":   return new Color(0.8f, 1f,   0.8f, 1f);
                case "yellow":         return     Color.yellow;
                case "dark_yellow":    return new Color(0.5f,  0.5f,  0f,    1f);
                case "light_yellow":   return new Color(1f,    1f,    0.4f,  1f);
                case "pastel_yellow":  return new Color(1f,    1f,    0.8f,  1f);
                case "purple":         return new Color(0.5f,  0f,    0.5f,  1f);
                case "dark_purple":    return new Color(0.25f, 0f,    0.25f, 1f);
                case "light_purple":   return new Color(0.8f,  0.4f,  0.8f,  1f);
                case "pastel_purple":  return new Color(0.85f, 0.7f,  0.85f, 1f);
                case "orange":         return new Color(1f,    0.5f,  0f,    1f);
                case "dark_orange":    return new Color(0.8f,  0.4f,  0f,    1f);
                case "light_orange":   return new Color(1f,    0.7f,  0.4f,  1f);
                case "pastel_orange":  return new Color(1f,    0.8f,  0.6f,  1f);
                case "pink":           return new Color(1f,    0.75f, 0.8f,  1f);
                case "dark_pink":      return new Color(0.8f,  0.4f,  0.6f,  1f);
                case "light_pink":     return new Color(1f,    0.8f,  0.9f,  1f);
                case "pastel_pink":    return new Color(1f,    0.9f,  0.95f, 1f);
                case "brown":          return new Color(0.6f,  0.4f,  0.2f,  1f);
                case "dark_brown":     return new Color(0.4f,  0.26f, 0.13f, 1f);
                case "light_brown":    return new Color(0.8f,  0.6f,  0.4f,  1f);
                case "pastel_brown":   return new Color(0.95f, 0.87f, 0.8f,  1f);
                case "cyan":           return     Color.cyan;
                case "dark_cyan":      return new Color(0f,   0.5f, 0.5f, 1f);
                case "light_cyan":     return new Color(0.4f, 1f,   1f,   1f);
                case "pastel_cyan":    return new Color(0.8f, 1f,   1f,   1f);
                case "magenta":        return     Color.magenta;
                case "dark_magenta":   return new Color(0.5f, 0f,   0.5f, 1f);
                case "light_magenta":  return new Color(1f,   0.4f, 1f,   1f);
                case "pastel_magenta": return new Color(1f,   0.8f, 1f,   1f);
                case "gray":           return     Color.gray;
                case "dark_gray":      return new Color(0.25f, 0.25f, 0.25f, 1f);
                case "light_gray":     return new Color(0.75f, 0.75f, 0.75f, 1f);
                case "pastel_gray":    return new Color(0.9f,  0.9f,  0.9f,  1f);
                case "grey":           return     Color.grey;
                case "black":          return     Color.black;
                case "white":          return     Color.white;
                case "clear":          return     Color.clear;
                
            }

            // Handle hex colors
            if (text.StartsWith("#"))
                text = text.Substring(1);

            try
            {
                if (text.Length == 6) // RGB
                {
                    float r = Convert.ToInt32(text.Substring(0, 2), 16) / 255f;
                    float g = Convert.ToInt32(text.Substring(2, 2), 16) / 255f;
                    float b = Convert.ToInt32(text.Substring(4, 2), 16) / 255f;
                    return new Color(r, g, b, 1);
                }
                else if (text.Length == 8) // RGBA
                {
                    float r = Convert.ToInt32(text.Substring(0, 2), 16) / 255f;
                    float g = Convert.ToInt32(text.Substring(2, 2), 16) / 255f;
                    float b = Convert.ToInt32(text.Substring(4, 2), 16) / 255f;
                    float a = Convert.ToInt32(text.Substring(6, 2), 16) / 255f;
                    return new Color(r, g, b, a);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public Color? GetColorFromTags(string[] tags)
        {
            if (tags == null)
                return null;

            foreach (var tag in tags)
            {
                var color = GetColor(tag);
                if (color.HasValue)
                    return color.Value;
            }

            return null;
        }

        public bool GetIsColliderFromTags(string[] tags)
        {
            return tags.Any(t => GetIsCollider(t));
        }

        public bool GetIsRigidBodyFromTags(string[] tags)
        {
            return tags.Any(t => GetIsRigidBody(t));
        }

        public bool GetIsCollider(string text)
        {
            return text.ToLower().Trim() == "collider";
        }
        
        public bool GetIsRigidBody(string text)
        {
            return text.ToLower().Trim() == "rb" || text.ToLower().Trim() == "rigidbody";
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps
        
        public Step CreateThingAsset(VirtualEntity ve) 
        =>
            new Step(name   : $"create_thing_asset"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR
                        
                        AssetDatabase.StartAssetEditing();

                        var path = $"{FolderPath}{ve.ThingName}";
                        Debug.Log($"Creating QuickThing Asset at {path}");

                        // Ensure the folder exists
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // Create all components t
                        await CreatePrefab  (ve: ve);
                        await CreateMaterial(ve: ve);
                        await CreateScript  (ve: ve);

                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh(options: ImportAssetOptions.ForceUpdate);
                        
                        #endif
                    });
        
        
        public Step CreateThingHierarchy(VirtualEntity ve) 
        =>
            new Step(name   : $"create_thing_hierarchy"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR

                        // Get paths
                        string prefabPath   = System.IO.Path.Combine($"Assets/package1/{ve.ThingName}/Prefabs",   ve.ThingName + ".prefab");
                        string materialPath = System.IO.Path.Combine($"Assets/package1/{ve.ThingName}/Materials", ve.ThingName + "_material.mat");

                        // Load and instantiate the prefab
                        GameObject prefabAsset    = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;

                        // Add the script component
                        string scriptName = ve.ThingName;
                        Type   scriptType = System.Type.GetType("TEST_THING." + scriptName + ", Assembly-CSharp");
                        
                        if (scriptType != null)
                            prefabInstance.AddComponent(scriptType);

                        // Create child cube
                        GameObject modelGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        modelGO.name = "model";

                        // Apply size from tags if available
                        Vector3? size = GetSizeFromTags(ve.Tags);
                        if (size.HasValue)
                            modelGO.transform.localScale = size.Value;

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

                        // Handle Collider
                        if (GetIsColliderFromTags(ve.Tags))
                        {
                            if (modelGO.GetComponent<Collider>() == null)
                                modelGO.AddComponent<BoxCollider>();
                        }
                        else
                        {
                            var collider = modelGO.GetComponent<Collider>();
                            if (collider != null)
                                UnityEngine.Object.DestroyImmediate(collider);
                        }

                        // Save changes
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);

                        // Cleanup
                        UnityEngine.Object.DestroyImmediate(prefabInstance);
                        UnityEngine.Object.DestroyImmediate(modelGO);

                        #endif
                    });
        
        
        public Step CreateThingApp(VirtualEntity ve) 
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
        /// <param name="ve">VirtualEntity containing the thing data</param>
        private async Task CreatePrefab(VirtualEntity ve)
        {
            #if UNITY_EDITOR

            string thingName = ve.ThingName;
            var path = $"{FolderPath}{thingName}";

            // Create a simple GameObject
            GameObject gameObject = new GameObject(thingName);

            // Apply position from tags if available
            Vector3? position = GetPositionFromTags(ve.Tags);
            if (position.HasValue)
                gameObject.transform.position = position.Value;

            // Handle Rigidbody
            if (GetIsRigidBodyFromTags(ve.Tags))
            {
                if (gameObject.GetComponent<Rigidbody>() == null)
                    gameObject.AddComponent<Rigidbody>();
            }

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
        private async Task CreateScript(VirtualEntity ve)
        {
            #if UNITY_EDITOR

            var path = $"{FolderPath}{ve.ThingName}";

            // Create script folder
            string scriptFolder = System.IO.Path.Combine(path, "Scripts");
            if (!Directory.Exists(scriptFolder))
            {
                Directory.CreateDirectory(scriptFolder);
            }

            // Script template
            string scriptTemplate =
            $@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEST_THING
{{
    public class CLASS_NAME : MonoBehaviour
    {{
        void Start()
        {{
            // prompt = '{ve.Prompt}'
            /// full = {ve.TextNode.Line}
        }}
    }}
}}
";
            
            if (!ve.Prompt.IsNullOrEmpty() 
            &&  DoPrompt != null)
            {
                scriptTemplate = await DoPrompt.Invoke(ve);
            }

            // Format the template with the thing name
            string scriptContent = scriptTemplate.Replace("CLASS_NAME", ve.ThingName);

            // Write the script file
            string scriptPath = System.IO.Path.Combine(scriptFolder, ve.ThingName + ".cs");
            File.WriteAllText(scriptPath, scriptContent);

            Debug.Log($"Created script: {scriptPath}");
            
            // await Task.Delay(2000);            

            #endif // UNITY_EDITOR
        }

        /// <summary>
        /// Creates a material with the given name
        /// </summary>
        /// <param name="ve">Virtual entity to create material for</param>
        private async Task CreateMaterial(VirtualEntity ve)
        {
            UnityEngine.Material material  = default;
            string               thingName = ve.ThingName;
            
            #if UNITY_EDITOR

            var path = $"{FolderPath}{thingName}";
            
            // Create material folder
            string materialFolder = System.IO.Path.Combine(path, "Materials");
            
            if (!Directory.Exists(materialFolder))
                Directory.CreateDirectory(materialFolder);

            // Create a new material
            material       = new UnityEngine.Material(Shader.Find("Standard"));
            material.color = GetColorFromTags(ve.Tags) ?? Color.white;

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
