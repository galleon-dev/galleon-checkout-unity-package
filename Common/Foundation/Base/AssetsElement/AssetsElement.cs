using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Assets;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Assets : VirtualEntity
    {
        public Folder rootFolder;
        
        //////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Assets()
        {
            rootFolder            = new Folder();
            rootFolder.Path       = Folder.PACKAGE_ROOT_FOLDER_PATH;
            rootFolder.FolderPath = Folder.PACKAGE_ROOT_FOLDER_PATH;
            rootFolder.FolderName = "package1";
        }
        
        //////////////////////////////////////////////////////////////////////////////////// Scan
        
        public class AssetsScanHandler : ScanHandler<Assets>
        {
            public override void Scan()
            {
                base.Scan();
            }
        }
    }
}

