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
                        Debug.Log($"Package.Asset.Elements = {string.Join(", ", this.Elements.Collection.Select(x => x.Name))}");
                        Debug.Log($"Package.Asset.Folder   = {this.Assets.rootFolder.FolderPath}");
                        Debug.Log($"Package.Asset.Scene    = {"TBD"}");
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
                        await this.Node.Live.Plus_SpGO1("> Thing t1");
                    });
        public Step Do_PpQuickSlices() 
        =>
            new Step(action : async (s) =>
                    {
                        await this.Node.Live.Plus_PpQuickSlices("Oh Boy !");
                    });
        public Step Do_P_Plus_T_Direct() 
        =>
            new Step(action : async (s) =>
                    {
                        ///         /////////////////////// T.A
                        ///         > P.A.F + T.A [v]
                        ///         > P.A   + T.A [ ]
                        ///         > P     + T.A [ ]
                        ///         > P.A.F + T   [ ]
                        ///         > P.A   + T   [ ]
                        ///         > P     + T   [ ]
                        ///         /////////////////////// F.A + T.A
                        ///         > P.A.F + F.A + T.A [v]
                        ///         > P.A   + F.A + T.A [ ]
                        ///         > P     + F.A + T.A [ ]
                        ///         > P.A.F + F   + T   [ ]
                        ///         > P.A   + F   + T   [ ]
                        ///         > P     + F   + T   [ ]
                        
                        #if UNITY_EDITOR
                        
                        this.Report().Execute();
                        
                        s.Log("// 1 - P.A.F + T.A");
                        this.Assets.rootFolder.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_paf_p_qta" } );
                        
                        s.Log("// 2 - P.A + T.A");
                        this.Assets.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_pa_p_qta" } );
                        
                        s.Log("// 2 - P + T.A");
                        this.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_p_p_qta" } );
                        
                        
                        #endif
                    });
        
        public Step Do_P_Plus_T_Indirect() 
        =>
            new Step(action : async (s) =>
                    {
                        /// /////////////////////// T.A
                        ///         > P.A.F + T.A [v]
                        ///         > P.A   + T.A [ ]
                        ///         > P     + T.A [ ]
                        ///         > P.A.F + T   [ ]
                        ///         > P.A   + T   [ ]
                        ///         > P     + T   [ ]
                                
                        #if UNITY_EDITOR
                        
                        this.Report().Execute();
                        
                        
                        s.Log("// 1 - P.A.F + T.A");
                        this.Assets.rootFolder.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_paf_p_qta" } );
                        
                        s.Log("// 2 - P.A + T.A");
                        this.Assets.rootFolder.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_pa_p_qta" } );
                        
                        #endif
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
                btn_PpT1_M.clicked            += () => target.Do_PpT1_M().Execute(); 
                btn_PpT1_M.text                = "P+T1_M";
                
                Button btn_PpQS              = new Button(); this.Add(btn_PpQS);
                btn_PpQS.clicked            += () => target.Do_PpQuickSlices().Execute(); 
                btn_PpQS.text                = "P + Quick-Slices";
                
                this.Add(new Button(() => target.Do_P_Plus_T_Direct()  .Execute()) { text = "P + T direct",   style = { marginRight = 500 }});
                this.Add(new Button(() => target.Do_P_Plus_T_Indirect().Execute()) { text = "P + T indirect", style = { marginRight = 500 }});
                
                /// w1 - Direct / Indirect
                ///     > Direct
                ///     > Indirect
                /// w2 - Thing + Tags
                /// w3 - Prompt
                /// w4 - Elements

            }
        }   
    }
}
