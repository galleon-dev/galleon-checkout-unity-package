#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

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

            proj.SetBuildProperty(mainTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            if (!string.IsNullOrEmpty(frameworkTargetGuid))
            {
                proj.SetBuildProperty(frameworkTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            }
 
            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
#endif
