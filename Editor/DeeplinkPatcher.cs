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
        private const string DEEPLINK_SECTION_START = "Galleon Checkout Deeplink";
        private const string DEEPLINK_SECTION_END = "end of Galleon Checkout Deeplink";

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

            // Find and remove existing Galleon Checkout Deeplink section
            XmlNode startComment = null;
            XmlNode endComment = null;

            foreach (XmlNode child in activityNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment)
                {
                    if (child.Value == DEEPLINK_SECTION_START)
                    {
                        startComment = child;
                    }
                    else if (child.Value == DEEPLINK_SECTION_END)
                    {
                        endComment = child;
                        break;
                    }
                }
            }
            

            // Remove existing section if found
            if (startComment != null && endComment != null)
            {
                XmlNode current = startComment.NextSibling;
                while (current != null && current != endComment)
                {
                    XmlNode next = current.NextSibling;
                    activityNode.RemoveChild(current);
                    current = next;
                }
                activityNode.RemoveChild(startComment);
                activityNode.RemoveChild(endComment);
            }

            
            // Add XML comment before the intent-filter
            activityNode.AppendChild(doc.CreateComment(DEEPLINK_SECTION_START));
            activityNode.AppendChild(doc.CreateComment("(this was created automaticly from 'project settings > Galleon Checkout)"));

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

            activityNode.AppendChild(doc.CreateComment(DEEPLINK_SECTION_END));

            // Save the modified manifest
            doc.Save(manifestPath);
            return true;
        }
    }
}
#endif
