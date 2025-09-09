using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Assets : Entity
    {
        public FolderAsset RootFolderAsset;
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Report() 
        =>
            new Step(action : async (s) =>
                    {
                        var tree = RootFolderAsset.Node.Descendants().OfType<FolderAsset>().ToList();
                        foreach (var folder in tree)
                        {
                            s.Log(folder.Path);
                        }
                    });
        public Step Do_Reload() 
        =>
            new Step(action : async (s) =>
                    {
                        #if UNITY_EDITOR
                        UnityEditor.AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                        #endif
                    });
        public Step Do_Rescan() 
        =>
            new Step(action : async (s) =>
                    {
                        this.RootFolderAsset.Node.Scan.Register();
                        this.RootFolderAsset.Node.Scan.ScanRecursive();
                    });
        public Step Do_Assets_Plus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        this.RootFolderAsset.Node.Live.Plus(new FolderAsset() { FolderName = "f1" });
                    });
        public Step Do_Assets_Equals_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        var folder = this.RootFolderAsset.Node.Descendants().OfType<FolderAsset>().First(x => x.FolderName == "f1");
                        folder.Node.Live.Edit("Name", "f1_edited");
                    });
        public Step Do_Assets_Minus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        var folder = this.RootFolderAsset.Node.Descendants().OfType<FolderAsset>().First(x => x.Path.EndsWith("f1"));
                        folder.Node.Live.Minus();
                        
                        // var folder = this.RootFolderAsset.Node.Descendants().OfType<FolderAsset>().First(x => x.FolderName == "f1");
                        // folder.Node.Live.Minus();
                    });
        public Step Do_Assets_Print_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        this.RootFolderAsset.Node.Printing.Print(id: "apf", text: "> Folder f1");
                    });
        public Step Do_Export() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Report().Execute();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Assets>
        {
            public Inspector(Assets target) : base(target)
            {
                Button btn_Report             = new Button(); this.Add(btn_Report);
                btn_Report.clicked           += () => target.Report().Execute(); 
                btn_Report.text               = "Report";
                
                Button ReloadRescan           = new Button(); this.Add(ReloadRescan);
                ReloadRescan.clicked         += () => target.Do_Reload().Execute(); 
                ReloadRescan.text             = "Reload";
                
                Button btn_Rescan             = new Button(); this.Add(btn_Rescan);
                btn_Rescan.clicked           += () => target.Do_Rescan().Execute(); 
                btn_Rescan.text               = "Scan";
                
                Button btn_AssetsPlusFolder   = new Button(); this.Add(btn_AssetsPlusFolder);
                btn_AssetsPlusFolder.clicked += () => target.Do_Assets_Plus_Folder().Execute(); 
                btn_AssetsPlusFolder.text     = "Assets + Folder";
                
                Button btn_Equals             = new Button(); this.Add(btn_Equals);
                btn_Equals.clicked           += () => target.Do_Assets_Equals_Folder().Execute(); 
                btn_Equals.text               = "Assets = Folder";
                
                Button btn_Minus              = new Button(); this.Add(btn_Minus);
                btn_Minus.clicked            += () => target.Do_Assets_Minus_Folder().Execute(); 
                btn_Minus.text                = "Assets - Folder";
                
                Button btn_Print              = new Button(); this.Add(btn_Print);
                btn_Print.clicked            += () => target.Do_Assets_Print_Folder().Execute(); 
                btn_Print.text                = "Print Folder";
                
                Button btn_Export             = new Button(); this.Add(btn_Export);
                btn_Export.clicked           += () => target.Do_Export().Execute(); 
                btn_Export.text               = "Export";
                
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

