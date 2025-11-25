#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

namespace Protorius42.Editor
{
    public class PostBuildProcess
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) 
                return;
            
            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var mainTargetGuid = proj.GetUnityMainTargetGuid();
            var frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

           // === 1. Ensure GCC exceptions enabled (your existing code) ===
            proj.SetBuildProperty(mainTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            proj.SetBuildProperty(frameworkTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

            // === 2. Add StoreKit (from your previous issue) ===
            proj.AddFrameworkToProject(mainTargetGuid, "StoreKit.framework", false);
            proj.AddFrameworkToProject(frameworkTargetGuid, "StoreKit.framework", false);

            // === 3. Save project ===
            proj.WriteToFile(projPath);
        }
    }
}
#endif
