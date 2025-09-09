using System;
using System.Linq;
using UnityEngine.UIElements;

namespace Galleon.Checkout.Foundation
{
    public class Core : Entity
    {
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Report() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        public Step Do_Reload() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        public Step Do_Rescan() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        public Step Do_Core_Plus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        public Step Do_Core_Equals_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                       
                    });
        public Step Do_Core_Minus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                       
                    });
        public Step Do_Core_Print_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                    });
        public Step Do_Export() 
        =>
            new Step(action : async (s) =>
                    {
                        
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
                btn_AssetsPlusFolder.text     = "Core + Folder";
                
                Button btn_Equals             = new Button(); this.Add(btn_Equals);
                btn_Equals.clicked           += () => target.Do_Assets_Equals_Folder().Execute(); 
                btn_Equals.text               = "Core = Folder";
                
                Button btn_Minus              = new Button(); this.Add(btn_Minus);
                btn_Minus.clicked            += () => target.Do_Assets_Minus_Folder().Execute(); 
                btn_Minus.text                = "Core - Folder";
                
                Button btn_Print              = new Button(); this.Add(btn_Print);
                btn_Print.clicked            += () => target.Do_Assets_Print_Folder().Execute(); 
                btn_Print.text                = "Core Folder";
                
                Button btn_Export             = new Button(); this.Add(btn_Export);
                btn_Export.clicked           += () => target.Do_Export().Execute(); 
                btn_Export.text               = "Export";
                
            }
        }   
    }
}

