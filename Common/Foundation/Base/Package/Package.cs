using log4net.Plugin;
using UnityEngine;

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
            var rootFolderPath          = Application.dataPath + "/" + "package1/";
            this.Assets.RootFolderAsset = new FolderAsset() { Path = rootFolderPath };
            
            if (!this.Assets.RootFolderAsset.DoesFolderExist())
                this.Assets.RootFolderAsset.CreateFolder();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        
        public Step Report()
        =>
            new Step(name   : $"report_package"
                    ,action : async (s) =>
                    {
                        s.Log("Package Report");
                        s.Log("Assets");
                        s.Log("Hierarchy");
                        s.Log("Core");
                    });
        
        public Step Scan()
        =>
            new Step(name   : $"scan_package"
                    ,action : async (s) =>
                    {
                        // Assets.rootFolder = "folder"
                        // Assets.Scan();
                    });
        
        public Step TestSingleElement()
        =>
            new Step(name   : $"test_single_element"
                    ,action : async (s) =>
                              {
                                  /// Element
                                  ///   - Definition
                                  ///   - Actions
                                  ///   - self-CRUD
                                  ///   - tree-(+/-/e)
                                  ///   .
                                  ///   - inner ingrediant nodes
                                  ///   - tree-nodes + core
                                  ///   .
                                  ///   - runtime classes
                                  ///   .
                                  ///   - settings
                                  ///   - tests
                                  ///   - extras
                                  ///   .
                                  ///   - duplicate()
                                  ///   - morph()
                                  ///   - tree_dependencies()
                              });
        
        /// Foundation Classes
        ///
        ///     Entity
        ///     Element
        ///     Asset
        ///
        ///     TextNode
        ///     PrintNode
        ///
        ///     Package
        ///     Asset
        ///         Folder
        ///         Script
        ///     Scene
        ///         GO
        ///         C
        ///     Core
        
        public Step DoPackageFlow()
        =>
            new Step(name   : $"do_package_flow"
                    ,action : async (s) =>
                    {
                        /// Assets
                        ///     Scan.       : recoursivly. using registered elements. 
                        ///     Assets
                        ///         +f      : Core.Node.Plus(new FolderAsset("f1"));
                        ///         ef      : FolderAsset.Edit(path : "Name", value : "F1e");
                        ///         -f      : FolderAsset.Minus(); -or- Core.Minus(FolderAsset);
                        ///     Core
                        ///         +f      : X // Only inside Assets of package or element.
                        ///         ef      : X 
                        ///         -f      : X 
                        ///     Assets
                        ///         +.cs    :
                        ///         e.cs    :
                        ///         -.cs    :
                        ///     Core
                        ///         +.cs    : X
                        ///         e.cs    : X
                        ///         -.cs    : X
                        /// Scene
                        ///     Scene
                        ///         +go     :
                        ///         ego     :
                        ///         -go     :
                        ///     Core
                        ///         +go     :
                        ///         ego     :
                        ///         -go     :
                        ///     Scene
                        ///         +c      :
                        ///         ec      :
                        ///         -c      :
                        ///     Core
                        ///         +c      :
                        ///         ec      :
                        ///         -c      :
                        /// Core
                        ///     Core
                        ///         +r      :
                        ///         er      :
                        ///         -r      :
                        ///     Core
                        ///         +t      :
                        ///         et      :
                        ///         -t      :
                        /// Print
                        ///     Print "Thing t1"
                        ///
                        ///
                        /// ///////////////////
                        /// 
                        /// easy : Assets + f
                        /// big  : Core   + go
                        /// 
                    });

        public Step Print_Package_Root_Folder() 
        =>
            new Step(name   : $"Print_Package_Root_Folder"
                    ,action : async (s) =>
                    {
                        s.Log(Assets.RootFolderAsset.Path);
                    });
        
        public Step Do_Assets_Plus_Folder() 
        =>
            new Step(name   : $"Do_Assets_Plus_Folder"
                    ,action : async (s) =>
                    {
                        this.Assets.RootFolderAsset.Node.Live.Plus(new FolderAsset() { FolderName = "f1" });
                    });
        
        public Step Do_Core_Plus_Folder() 
        =>
            new Step(name   : $"Do_Core_Plus_Folder"
                    ,action : async (s) =>
                    {
                        // Test :
                        Core.Plus(new FolderAsset() { FolderName = "f1" });
                    });
    }
}



