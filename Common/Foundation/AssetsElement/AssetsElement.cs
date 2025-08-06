using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout.Foundation
{
    public class Assets : Entity
    {
        public Folder RootFolder;
        
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Report() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        public Step Do_Assets_Plus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        this.RootFolder.Node.Live.Plus(new Folder() { Name = "f1" });
                    });
        public Step Do_Assets_Equals_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        var folder = this.RootFolder.Node.Descendants().OfType<Folder>().First(x => x.Name == "f1");
                        folder.Node.Live.Edit("Name", "f1_edited");
                    });
        public Step Do_Assets_Minus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        var folder = this.RootFolder.Node.Descendants().OfType<Folder>().First(x => x.Name == "f1");
                        folder.Node.Live.Minus();
                    });
        public Step Do_Assets_Print_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Assets>
        {
            public Inspector(Assets target) : base(target)
            {
                Button btn_Report   = new Button(); this.Add(btn_Report);
                btn_Report.clicked += () => target.Report().Execute(); 
                btn_Report.text     = "Report";
                
                Button btn_AssetsPlusFolder   = new Button(); this.Add(btn_AssetsPlusFolder);
                btn_AssetsPlusFolder.clicked += () => target.Do_Assets_Plus_Folder().Execute(); 
                btn_AssetsPlusFolder.text     = "Assets + Folder";
                
                Button btn_Equals   = new Button(); this.Add(btn_Equals);
                btn_Equals.clicked += () => target.Do_Assets_Equals_Folder().Execute(); 
                btn_Equals.text     = "Assets = Folder";
                
                Button btn_Minus   = new Button(); this.Add(btn_Minus);
                btn_Minus.clicked += () => target.Do_Assets_Minus_Folder().Execute(); 
                btn_Minus.text     = "Assets - Folder";
                
                Button btn_Print   = new Button(); this.Add(btn_Print);
                btn_Print.clicked += () => target.Do_Assets_Minus_Folder().Execute(); 
                btn_Print.text     = "Print Folder";
                
            }
        }
    }
}
