using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using Galleon.Checkout.Assets;
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Package : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Elements Elements = new Elements();
        public Assets   Assets   = new Assets();
        public Core     Core     = new Core();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Package()
        {
            #if UNITY_EDITOR
          
            var rootFolderPath     = Application.dataPath + "/" + "package1/";
            this.Assets.rootFolder = new Folder() { Path = rootFolderPath };
            
            if (!this.Assets.rootFolder.DoesFolderExist())
                this.Assets.rootFolder.CreateFolder();
            
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



