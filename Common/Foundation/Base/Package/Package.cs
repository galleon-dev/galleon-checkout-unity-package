using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;
using UnityEngine.Assertions.Must;
using Thing = Galleon.Checkout.ELEMENTS.Thing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Package : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // Design Time
        public Elements Elements    = new Elements();
        public Assets   Assets      = new Assets();
        
        // Runtime
        public Core     Core        = new Core();
        
        // Slice
        public Slice    Slice       = new Slice();
        
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
        
        public Step CreateThings() 
        =>
            new Step(name   : $"create_things"
                    ,action : async (s) =>
                    {   
                        string OpString = "> Thing T1"
                                 + "\n" + "> Thing T2 #yellow #4x4 #10,10 #collider #rigidbody"
                                 + "\n" + "> Thing T3 #blue #rb prompt='a moving cube'"
                                 + "\n" + "";
                        
                        await Slice.Node.Live.Operation(OpString).Execute();
                    });
        
        
        public Step CreateSlice() 
        =>
            new Step(name   : $"create_slice"
                    ,action : async (s) =>
                    {   
                        /// > Slice slice1
                        ///     > Thing t1
                        ///     > Thing T2
                        var slice = new VirtualEntity() { TextNode = new TextNode("> Slice slice1") };
                        this.Node.AddChild(slice);                
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<Package>
        {
            public Inspector(Package target) : base(target)
            {
                this.Add(new Button(() => target.Report       ().Execute()) { text = "report"          });
                this.Add(new Button(() => target.Do_Rescan    ().Execute()) { text = "rescan"          });
                
                this.Add(new Button(() => target.CreateThings ().Execute()) { text = "create things"   });
                this.Add(new Button(() => target.CreateSlice  ().Execute()) { text = "create Slice"    });

            }
        }   
    }
}
