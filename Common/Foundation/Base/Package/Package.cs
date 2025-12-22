using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;

#if UNITY_EDITOR
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
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Report() 
        =>
            new Step(action : async (s) =>
                    {
                        var tree = Assets.rootFolder.Node.Descendants().OfType<Folder>().ToList();
                        foreach (var folder in tree)
                        {
                            s.Log(folder.Path);
                        }
                    });
        public Step Do_Rescan() 
        =>
            new Step(action : async (s) =>
                    {
                        this.Assets.rootFolder.Node.Scan.Register();
                        this.Assets.rootFolder.Node.Scan.ScanRecursive();
                    });
        public Step Do_APF() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Assets.rootFolder.Node.Live.Plus_APF("> Folder f1");
                    });
        public Step Do_APFE1() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Assets.rootFolder.Node.Live.Plus_APFE1("> Folder f1");
                    });
        public Step Do_PPFE1M() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_PPFE1M("> Folder f1");
                    });
        public Step Do_PPFE1() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_PPFE1("> Folder f1");
                    });
        public Step Do_PpS1() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_PpS1("> Scene s1");
                    });
        public Step Do_SpGO1() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_SpGO1("> Gameobject go1");
                    });
        public Step Do_PpT1_M() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_SpGO1("> Gameobject go1");
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Package>
        {
            public Inspector(Package target) : base(target)
            {
                Button btn_Report             = new Button(); this.Add(btn_Report);
                btn_Report.clicked           += () => target.Report().Execute(); 
                btn_Report.text               = "Report";
                
                Button btn_Rescan             = new Button(); this.Add(btn_Rescan);
                btn_Rescan.clicked           += () => target.Do_Rescan().Execute(); 
                btn_Rescan.text               = "Scan";
                
                ///
                
                Button btn_APF                 = new Button(); this.Add(btn_APF);
                btn_APF.clicked               += () => target.Do_APF().Execute(); 
                btn_APF.text                   = "A+F";
                
                Button btn_APFE1               = new Button(); this.Add(btn_APFE1);
                btn_APFE1.clicked             += () => target.Do_APFE1().Execute(); 
                btn_APFE1.text                 = "A+FE1";
                
                Button btn_PPFE1M              = new Button(); this.Add(btn_PPFE1M);
                btn_PPFE1M.clicked            += () => target.Do_PPFE1M().Execute(); 
                btn_PPFE1M.text                = "PPF1_M";
                
                Button btn_PPFE1               = new Button(); this.Add(btn_PPFE1);
                btn_PPFE1.clicked             += () => target.Do_PPFE1().Execute(); 
                btn_PPFE1.text                 = "P+F1";
                
                Button btn_PpS1                = new Button(); this.Add(btn_PpS1);
                btn_PpS1.clicked              += () => target.Do_PpS1().Execute(); 
                btn_PpS1.text                  = "P+S1";
                
                Button btn_SpGO1               = new Button(); this.Add(btn_SpGO1);
                btn_SpGO1.clicked             += () => target.Do_SpGO1().Execute(); 
                btn_SpGO1.text                 = "P+S1";
                
                Button btn_PpT1_M              = new Button(); this.Add(btn_PpT1_M);
                btn_PpT1_M.clicked            += () => target.Do_SpGO1().Execute(); 
                btn_PpT1_M.text                = "P+S1_M";
                
            }
        }   
    }
}
