using Galleon.Checkout.Foundation;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Common.Galleon
{
    /// <summary>
    /// Galleon configuration settings accessible via Edit > Project Settings > Galleon Settings
    /// </summary>
    public class GalleonSettings : ScriptableObject
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string DeepLinkName = ""; // Deep link scheme name for the application
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        internal const string SettingsPath = "Assets/Resources/GalleonSettings.asset";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Instance
        
        
        private static GalleonSettings instance;
        public  static GalleonSettings Instance
        {
            get
            {
                // Try to load from cache first
                if (instance == null)
                {
                    // Attempt to load from Resources folder
                    instance = Resources.Load<GalleonSettings>(nameof(GalleonSettings));
                    
                    #if UNITY_EDITOR
                    
                    // Auto-create the settings asset if it doesn't exist
                    if (instance == null)
                    {
                        Debug.LogError($"GalleonSettings asset not found at {SettingsPath}. Please create it via Edit > Project Settings > Galleon Settings");
                    }
                    #endif
                    
                }
                return instance;
            }
        }
        
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Initialize on load
    #if UNITY_EDITOR
    
    /// <summary>
    /// Initializes settings on editor startup
    /// </summary>
    [InitializeOnLoad]
    static class GalleonSettingsInitializer
    {
        // Static constructor called when Unity loads
        static GalleonSettingsInitializer()
        {
            // Ensure the settings asset is created on editor startup
            EnsureSettingsAssetExists();
        }

        private static void EnsureSettingsAssetExists()
        {
            // Check if the asset already exists
            var settings = Resources.Load<GalleonSettings>(nameof(GalleonSettings));
            
            // Create the asset if it doesn't exist
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GalleonSettings>();
                Directory.CreateDirectory(Path.GetDirectoryName(GalleonSettings.SettingsPath));
                AssetDatabase.CreateAsset(settings, GalleonSettings.SettingsPath);
                AssetDatabase.SaveAssets();
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Project Settings
    
    /// <summary>
    /// Provides the Galleon Settings panel in Unity's Project Settings window
    /// </summary>
    static class GalleonSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            // Create a new settings provider for Project Settings
            var provider = new SettingsProvider("Project/Galleon Checkout", SettingsScope.Project)
                         {
                             label        = "Galleon Checkout",
                             guiHandler   = (searchContext) =>
                                          {
                                              var settings         = GalleonSettings.Instance;
                                              var serializedObject = new SerializedObject(settings);

                                              EditorGUILayout.Space();
                                              EditorGUILayout.LabelField("Galleon Checkout Configuration", EditorStyles.boldLabel);
                                              EditorGUILayout.Space();

                                              EditorGUI.BeginChangeCheck();
                                              var deepLinkProp     = serializedObject.FindProperty("DeepLinkName");
                                              EditorGUILayout.PropertyField(deepLinkProp);

                                              if (EditorGUI.EndChangeCheck())
                                              {
                                                  serializedObject.ApplyModifiedProperties();
                                                  EditorUtility.SetDirty(settings);
                                                  AssetDatabase.SaveAssets();
                                              }

                                              EditorGUILayout.Space();
                                              if (GUILayout.Button("Apply Deep Link to Android Manifest"))
                                              {
                                                  EditorApplication.ExecuteMenuItem("Tools/Galleon/Patch Android Manifest");
                                              }
                                          },
                             keywords     = new System.Collections.Generic.HashSet<string>(new[] { "Galleon", "Deep Link" }) // Keywords for search functionality in Project Settings
                         };

            return provider;
        }
    }
    #endif
}

