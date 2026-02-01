#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Xml;
using UnityEngine;
using Common.Galleon;

namespace Galleon.Pipeline.Editor
{
    /// <summary>
    /// Editor-time patcher that updates the Android manifest with deeplink configuration
    /// </summary>
    [InitializeOnLoad]
    public class DeeplinkPatcher
    {
        static DeeplinkPatcher()
        {
            PatchManifestIfNeeded();
        }

        [MenuItem("Tools/Galleon/Patch Android Manifest")]
        public static void PatchManifestIfNeeded()
        {
            // Search for AndroidManifest.xml in the project
            string[] manifestGuids = AssetDatabase.FindAssets("AndroidManifest t:TextAsset");
            string manifestPath = null;

            foreach (string guid in manifestGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("AndroidManifest.xml"))
                {
                    manifestPath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(manifestPath))
            {
                Debug.LogWarning("Galleon Checkout Galleon Checkout DeeplinkPatcher: AndroidManifest.xml not found in project.");
                return;
            }

            
            // Get deeplink scheme from settings
            string deeplinkScheme = GalleonSettings.Instance?.DeepLinkName;

            if (string.IsNullOrEmpty(deeplinkScheme))
            {
                Debug.LogWarning("Galleon Checkout DeeplinkPatcher: DeepLinkName is not set in GalleonSettings. Skipping deeplink patching.");
                return;
            }

            try
            {
                if (PatchManifest(manifestPath, deeplinkScheme))
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"Galleon Checkout DeeplinkPatcher: Successfully patched {manifestPath} with deeplink scheme: {deeplinkScheme}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Galleon Checkout DeeplinkPatcher: Failed to patch AndroidManifest.xml - {e.Message}");
            }
        }

        private static bool PatchManifest(string manifestPath, string deeplinkScheme)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            // Find the main activity
            XmlNode activityNode = doc.SelectSingleNode("//activity[@android:name='com.unity3d.player.UnityPlayerActivity']", nsMgr);

            if (activityNode == null)
            {
                // Try GameActivity if UnityPlayerActivity not found
                activityNode = doc.SelectSingleNode("//activity[@android:name='com.unity3d.player.UnityPlayerGameActivity']", nsMgr);
            }

            if (activityNode == null)
            {
                Debug.LogError("Galleon Checkout DeeplinkPatcher: Could not find Unity activity node in AndroidManifest.xml");
                return false;
            }

            // Check if deeplink intent-filter already exists with the current scheme
            XmlNodeList intentFilters = activityNode.SelectNodes("intent-filter", nsMgr);
            bool deeplinkExists = false;

            foreach (XmlNode filter in intentFilters)
            {
                XmlNode dataNode = filter.SelectSingleNode("data[@android:scheme]", nsMgr);
                if (dataNode != null)
                {
                    string existingScheme = dataNode.Attributes["android:scheme"]?.Value;
                    if (existingScheme == deeplinkScheme)
                    {
                        deeplinkExists = true;
                        break;
                    }
                }
            }

            // No changes needed if deeplink already exists
            if (deeplinkExists)
                return false;

            // Add XML comment before the intent-filter
            XmlComment comment = doc.CreateComment(" Galleon Checkout Deeplink ");
            activityNode.AppendChild(comment);
            
            // Add deeplink intent-filter
            XmlElement intentFilter = doc.CreateElement("intent-filter");

            // Add action
            XmlElement action = doc.CreateElement("action");
            action.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.action.VIEW");
            intentFilter.AppendChild(action);

            // Add default category
            XmlElement categoryDefault = doc.CreateElement("category");
            categoryDefault.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.DEFAULT");
            intentFilter.AppendChild(categoryDefault);

            // Add browsable category
            XmlElement categoryBrowsable = doc.CreateElement("category");
            categoryBrowsable.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.BROWSABLE");
            intentFilter.AppendChild(categoryBrowsable);

            // Add data scheme
            XmlElement data = doc.CreateElement("data");
            data.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", deeplinkScheme);
            intentFilter.AppendChild(data);

            // Append to activity
            activityNode.AppendChild(intentFilter);

            // Save the modified manifest
            doc.Save(manifestPath);
            return true;
        }
    }
}
#endif
