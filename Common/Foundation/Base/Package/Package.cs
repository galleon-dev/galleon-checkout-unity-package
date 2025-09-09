using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Package : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Assets Assets = new Assets();
        public Core   Core   = new Core();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Package()
        {
            #if UNITY_EDITOR
          
            var rootFolderPath          = Application.dataPath + "/" + "package1/";
            this.Assets.RootFolderAsset = new FolderAsset() { Path = rootFolderPath };
            
            if (!this.Assets.RootFolderAsset.DoesFolderExist())
                this.Assets.RootFolderAsset.CreateFolder();
            
            #endif
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        public static void OnReload()
        {
            // this.Node.Scan.Scan();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
    }
}



