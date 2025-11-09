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
    public class Assets : Entity
    {
        public Folder rootFolder;
        
        //////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Assets()
        {
            rootFolder            = new Folder();
            rootFolder.Path       = Folder.PACKAGE_ROOT_FOLDER_PATH;
            rootFolder.FolderName = "package1";
        }
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Report() 
        =>
            new Step(action : async (s) =>
                    {
                        var tree = rootFolder.Node.Descendants().OfType<Folder>().ToList();
                        foreach (var folder in tree)
                        {
                            s.Log(folder.Path);
                        }
                    });
        public Step Do_Rescan() 
        =>
            new Step(action : async (s) =>
                    {
                        this.rootFolder.Node.Scan.Register();
                        this.rootFolder.Node.Scan.ScanRecursive();
                    });
        public Step Do_Assets_Plus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.rootFolder.Node.Live.Plus("> Folder f1");
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Assets>
        {
            public Inspector(Assets target) : base(target)
            {
                Button btn_Report             = new Button(); this.Add(btn_Report);
                btn_Report.clicked           += () => target.Report().Execute(); 
                btn_Report.text               = "Report";
                
                Button btn_Rescan             = new Button(); this.Add(btn_Rescan);
                btn_Rescan.clicked           += () => target.Do_Rescan().Execute(); 
                btn_Rescan.text               = "Scan";
                
                Button btn_AssetsPlusFolder   = new Button(); this.Add(btn_AssetsPlusFolder);
                btn_AssetsPlusFolder.clicked += () => target.Do_Assets_Plus_Folder().Execute(); 
                btn_AssetsPlusFolder.text     = "Assets + Folder";
            }
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

