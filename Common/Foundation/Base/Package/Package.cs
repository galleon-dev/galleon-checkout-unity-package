using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;
using Thing = Galleon.Checkout.ELEMENTS.Thing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Package : VirtualEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // Design Time
        public Elements Elements = new Elements();
        public Assets   Assets   = new Assets();
        
        // Runtime
        public Core Core = new Core();
        
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP
        
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
        
        
        public Step DoAI() 
        =>
            new Step(name   : $"do_ai"
                    ,action : async (s) =>
                    {
                        /// Entity
                        /// Live
                        /// Element
                        /// Thing
                        /// create code that does the things
                    });
        
        #region OLD
        // public Step Do_APF() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Assets.rootFolder.Node.Live.Plus_APF("> Folder f1");
        //             });
        // public Step Do_APFE1() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Assets.rootFolder.Node.Live.Plus_APFE1("> Folder f1");
        //             });
        // public Step Do_PPFE1M() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_PPFE1M("> Folder f1");
        //             });
        // public Step Do_PPFE1() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_PPFE1("> Folder f1");
        //             });
        // public Step Do_PpS1() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_PpS1("> Scene s1");
        //             });
        // public Step Do_SpGO1() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_SpGO1("> Gameobject go1");
        //             });
        // public Step Do_PpT1_M() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_SpGO1("> Thing t1");
        //             });
        // public Step Do_PpQuickSlices() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 await this.Node.Live.Plus_PpQuickSlices("Oh Boy !");
        //             });
        // public Step Do_P_Plus_T_Direct() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 ///         /////////////////////// T.A
        //                 ///   1      > P.A.F + T.A [v]
        //                 ///   2      > P.A   + T.A [v]
        //                 ///   3      > P     + T.A [v]
        //                 ///   4      > P.A.F + T   [ ]
        //                 ///   5      > P.A   + T   [ ]
        //                 ///   6      > P     + T   [ ]
        //                 ///         /////////////////////// F.A + T.A
        //                 ///   7      > P.A.F + F.A + T.A [v]
        //                 ///   8      > P.A   + F.A + T.A [ ]
        //                 ///   9      > P     + F.A + T.A [ ]
        //                 ///   10     > P.A.F + F   + T   [ ]
        //                 ///   11     > P.A   + F   + T   [ ]
        //                 ///   12     > P     + F   + T   [ ]
        //                 
        //                 #if UNITY_EDITOR
        //                 
        //                 this.Report().Execute();
        //                 
        //                 s.Log("// 1 - P.A.F + T.A");
        //                 // - Straign Forward add child + create
        //                 this.Assets.rootFolder.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_1_paf_p_qta" } );
        //                 
        //                 s.Log("// 2 - P.A + T.A");
        //                 // - parent = Get Default Entity
        //                 this.Assets.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_2_pa_p_qta" } );
        //                 
        //                 s.Log("// 3 - P + T.A");
        //                 // - mostly the same as 2
        //                 this.Node.Live.Plus(new Checkout.Assets.QuickThing() { thingName = "thing_3_p_p_qta" } );
        //                 
        //                 // theoretical : P.A.F + T.(A).A
        //                 //
        //                 
        //                 s.Log("// 4 - P.A.F + T");
        //                 // - zip namespaces - get only relevant tree using namespace
        //                 this.Assets.rootFolder.Node.Live.Plus(new Checkout.ELEMENTS.QuickThing("thing_4_paf_p_t"));
        //                 
        //                 s.Log("// 5 - P.A + T");
        //                 // - mostly same as 5
        //                 this.Assets.Node.Live.Plus(new Checkout.ELEMENTS.QuickThing("thing_5_pa_p_t"));
        //                 
        //                 s.Log("// 6 - P + T");
        //                 // - zip namespaces - add all namespaces 
        //                 this.Node.Live.Plus(new Checkout.ELEMENTS.QuickThing("thing_6_p_p_t"));
        //                 
        //                 
        //                 #endif
        //             });
        // 
        // public Step Do_P_Plus_T_Indirect() 
        // =>
        //     new Step(action : async (s) =>
        //             {
        //                 /// /////////////////////// T.A
        //                 ///         > P.A.F + T.A [v]
        //                 ///         > P.A   + T.A [ ]
        //                 ///         > P     + T.A [ ]
        //                 ///         > P.A.F + T   [ ]
        //                 ///         > P.A   + T   [ ]
        //                 ///         > P     + T   [ ]
        //                         
        //                 #if UNITY_EDITOR
        //                 
        //                 this.Report().Execute();
        //                 
        //                 s.Log("// 1 - P.A.F + QT.A");
        //                 this.Assets.rootFolder.Node.Live.Plus_Indirect("> Assets.QuickThing qta");
        //                 
        //                 #endif
        //             });
        
        //////////////////////////////////////////////////////////////////////////////////// Thing
        #endregion // OLD
        
        public Step PrepForThing() 
        =>
            new Step(name   : $"prep_for_thing"
                    ,action : async (s) =>
                    {
                        /// Thins.Slice1.Plus(thing t1)
                        
                        /// > Root                                        [V]  # hardcoded
                        ///     > Context                                 [V]  # hardcoded
                        ///         > Slices
                        ///             > Slice Slice1                    [ ]  # ve
                        ///         > Project
                        ///             > Package foundation              [ ]  # scan
                        ///             > Package package1                [ ]  # scan
                        ///                 > (Definitions)                 
                        ///                     > Slice Sliec1            [ ]  # text
                        ///                         > Thing t1            [ ]  # text
                        ///                         > Thing t2            [ ]  # text
                        ///                         > Thing t3            [ ]  # text
                        ///                 > (Elements)                    
                        ///                 > (Assets)                      
                        ///                 > (Hierarchy)
                        ///                 > (Slices)
                        ///                     > Slice slice1            [ ]  # ve, in memory
                        ///                         > thing t1            [ ]  # ve, in memory
                        ///                         > thing t2            [ ]  # ve, in memory
                        ///                         > thing t3            [ ]  # ve, in memory
                        /// 
                        ///     > Runtime                                 [V]  # hardcoded
                        ///         > App                                 [ ]  # hardcoded
                        ///             > Core                            [ ]  # class instance, hardcoded
                        ///                 > Slice Slice1                [ ]  # class instance, runtime
                        ///                     > Thing t1                [ ]  # class instance, runtime
                        ///                     > Thing t2                [ ]  # class instance, runtime
                        ///                     > Thing t3                [ ]  # class instance, runtime
                        
                        /// 1. Verify Package (IDs, entities, scene, core, etc)
                        /// 2. Verify Definitions (texts)
                        /// 3. Verify Slices (Ve)
                        /// 4. Print.
                        ///     > Elements (ve)
                        ///     > t1 t2 t3 (ve)
                        ///     > crud create
                        ///     > Elements (classes)
                        ///     > t1 t2 t3 (classes)
                        /// 5. available in runtime : t1 t2 t3
                    });
        
        
        public Step CreateThing1() 
        =>
            new Step(name   : $"create_thing_1"
                    ,action : async (s) =>
                    {   
                        /// ====================== Pre :
                        /// > Package Foundation
                        ///     > Element Thing
                        ///         > Assets    (Hardcoded) = Prefab, Script, Material.
                        ///         > Hierarchy (Hardcoded) = Prefab, Component, GO_"model", Cube, Rigindbody, Collider.
                        ///     > Assets
                        ///         > Folder Thing
                        ///             > Prefab Thing
                        ///             > Script Thing
                        ///             > Material Thing
                        ///     > Hierarchy
                        ///         > Scene S1
                        ///         > Prefab Thing
                        ///             > Component ting
                        ///             > GO model
                        ///                 > C Cube
                        ///                     > ref material
                        ///     > Core
                        ///         > class with fields and flow.
                        ///
                        /// ====================== Prep :
                        /// > Package1
                        ///     > Dependencies = Foundation.
                        /// ====================== Print text :
                        /// "
                        /// > package1
                        ///     > core
                        ///         > thing t1
                        ///         > thing t1 #tags
                        ///         > thing t3 #tags "prompt"
                        /// "
                        /// ====================== Result :
                        /// > Package package1
                        ///     > Assets
                        ///         > A.Scene s1
                        ///     > Hierarchy
                        ///         > H.Scene s1
                        ///             > prefab t1
                        ///             > prefab t2
                        ///             > prefab t3
                        ///     > Core
                        ///         > field Thing t1 
                        ///         > field Thing t2 
                        ///         > field Thing t3 

                        //////////////////////////////////////////////////////////////////
                        
                        /// Definitions
                        Core           core         = new Core();
                        ELEMENTS.Thing thingElement = Elements.ThingElement;
                        
                        //////////////////////////////////////////////////////////////////
                        
                        /// Create default Hardcoded thing
                        /// Thing t1 = new Thing(name : "t1");
                        VirtualEntity thing1 = await thingElement.Create(new Thing.ThingParams() { Name = "t1"});
                        
                        //////////////////////////////////////////////////////////////////
                        
                        /// Plus Direct
                        /// Package.Node.Plus(new Thing(name : "t3"));
                        
                        //////////////////////////////////////////////////////////////////
                        
                        /// Plus Op
                        /// Package.Node.Plus("> Thing t1");
                        
                        //////////////////////////////////////////////////////////////////
                    });
        
        
        public Step CreateThing2() 
        =>
            new Step(name   : $"create_thing_2"
                    ,action : async (s) =>
                    {   
                        /// Definitions
                        Core           core         = new Core();
                        ELEMENTS.Thing thingElement = Elements.ThingElement;
                        
                        //////////////////////////////////////////////////////////////////
                        
                        /// Create default Hardcoded thing
                        /// Thing t1 = new Thing(name : "t1");
                        VirtualEntity thing2 = await thingElement.Create(new Thing.ThingParams()
                        {
                                Name   = "t2",
                                Tags   = new[] { "#tag" },
                                Prompt = "",
                        });
                        
                    });
        
        public Step HardCopyThing() 
        =>
            new Step(name   : $"hard_copy_thing"
                    ,action : async (s) =>
                    {
                        
                    });
        
        public Step InheritThing() 
        =>
            new Step(name   : $"inherit_thing"
                    ,action : async (s) =>
                    {
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Package>
        {
            public Inspector(Package target) : base(target)
            {
                this.Add(new Button(() => target.Report       ().Execute()) { text = "report"          });
                this.Add(new Button(() => target.Do_Rescan    ().Execute()) { text = "rescan"          });
                this.Add(new Button(() => target.DoAI         ().Execute()) { text = "doai"            });
                
                this.Add(new Button(() => target.PrepForThing ().Execute()) { text = "prep for thing"  });
                this.Add(new Button(() => target.CreateThing1 ().Execute()) { text = "create thing 1"  });
                this.Add(new Button(() => target.CreateThing2 ().Execute()) { text = "create thing 2"  });
                this.Add(new Button(() => target.HardCopyThing().Execute()) { text = "hard copy thing" });
                this.Add(new Button(() => target.InheritThing ().Execute()) { text = "inherit thing"   });
                
                #region OLD
                
                // Button btn_APF                 = new Button(); this.Add(btn_APF);
                // btn_APF.clicked               += () => target.Do_APF().Execute(); 
                // btn_APF.text                   = "A+F";
                // 
                // Button btn_APFE1               = new Button(); this.Add(btn_APFE1);
                // btn_APFE1.clicked             += () => target.Do_APFE1().Execute(); 
                // btn_APFE1.text                 = "A+FE1";
                // 
                // Button btn_PPFE1M              = new Button(); this.Add(btn_PPFE1M);
                // btn_PPFE1M.clicked            += () => target.Do_PPFE1M().Execute(); 
                // btn_PPFE1M.text                = "PPF1_M";
                // 
                // Button btn_PPFE1               = new Button(); this.Add(btn_PPFE1);
                // btn_PPFE1.clicked             += () => target.Do_PPFE1().Execute(); 
                // btn_PPFE1.text                 = "P+F1";
                // 
                // Button btn_PpS1                = new Button(); this.Add(btn_PpS1);
                // btn_PpS1.clicked              += () => target.Do_PpS1().Execute(); 
                // btn_PpS1.text                  = "P+S1";
                // 
                // Button btn_SpGO1               = new Button(); this.Add(btn_SpGO1);
                // btn_SpGO1.clicked             += () => target.Do_SpGO1().Execute(); 
                // btn_SpGO1.text                 = "P+S1";
                // 
                // Button btn_PpT1_M              = new Button(); this.Add(btn_PpT1_M);
                // btn_PpT1_M.clicked            += () => target.Do_PpT1_M().Execute(); 
                // btn_PpT1_M.text                = "P+T1_M";
                // 
                // Button btn_PpQS              = new Button(); this.Add(btn_PpQS);
                // btn_PpQS.clicked            += () => target.Do_PpQuickSlices().Execute(); 
                // btn_PpQS.text                = "P + Quick-Slices";
                // 
                // this.Add(new Button(() => target.Do_P_Plus_T_Direct()  .Execute()) { text = "P + T direct",   style = { marginRight = 500 }});
                // this.Add(new Button(() => target.Do_P_Plus_T_Indirect().Execute()) { text = "P + T indirect", style = { marginRight = 500 }});

                #endregion // OLD
            }
        }   
    }
}
 