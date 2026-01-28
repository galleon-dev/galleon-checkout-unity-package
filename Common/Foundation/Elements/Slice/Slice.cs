using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Foundation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public class Slice : Entity
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        //public VirtualEntity t1 =  new VirtualEntity() { TextNode = new TextNode("> Thing t1")                                          };
        //public VirtualEntity t2 =  new VirtualEntity() { TextNode = new TextNode("> Thing t2 #yellow #4x4 #10,10 #collider #rigidbody") };
        //public VirtualEntity t3 =  new VirtualEntity() { TextNode = new TextNode("> Thing t3 #blue #rb prompt='a moving cube'")         };
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public static string OpString = "> Thing t1"
                               + "\n" + "> Thing t2 #yellow #4x4 #10,10 #collider #rigidbody"
                               + "\n" + "> Thing t3 #blue #rb prompt='a moving cube'"
                               + "\n" + "";
        
        //////////////////////////////
        
        public static TextNode parsed => TextNode.Parse(OpString);
        
        //////////////////////////////
        
        public List<VirtualEntity> LoadVirtualEntities()
        {
            List<VirtualEntity> result = new();
            
            foreach (var textNode in parsed.Node.Descendants().OfType<TextNode>())
            {
                if (textNode.RawText.IsNullOrEmpty()
                ||  textNode.RawText.ToLower().StartsWith("> origin") )
                    continue;
                
                var ve = new VirtualEntity(){TextNode = textNode};
                result.Add(ve);
            }
            
            return result;
        }
        
        //////////////////////////////
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Test Slice")]
        #endif
        public static void Test()
        {
            Debug.Log(parsed.ToTreeString());
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step Op() 
        =>
            new Step(name   : $"Op"
                    ,action : async (s) =>
                    {   
                        //var loadedVEs = LoadVirtualEntities();
                        //foreach (var ve in loadedVEs)
                        //    this.Node.AddChild(ve);
                        
                        var ves = this.Node.Descendants().OfType<VirtualEntity>();

                        foreach (var ve in ves)
                            Debug.Log(ve.TextNode?.RawText ?? "> NULL");
                        
                        foreach (var ve in ves)
                        {
                            ve.State = "assets";
                            await ve.SaveState();
                            await ve.DoNextLiveStep();
                        }

                    });
        
        public Step Resume() 
        =>
            new Step(name   : $"resume"
                    ,action : async (s) =>
                    {
                        
                        var loadedVEs = LoadVirtualEntities();
                        foreach (var ve in loadedVEs)
                            this.Node.AddChild(ve);
                        
                        var ves = this.Node.Descendants().OfType<VirtualEntity>();

                        foreach (var ve in ves)
                        {
                            await ve.DoNextLiveStep();
                        }
                    })
                    {
                        IsSilentLog = true,
                    };
        
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        public static async void InitializeOnLoad()
        {
            await Root.Instance.Context.Project.Package1.Slice.Resume().Execute();
        }
     
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step RuntimeStart() 
        =>
            new Step(name   : $"slice_runtime_start"
                    ,action : async (s) =>
                    {   
                    });
    }
}
