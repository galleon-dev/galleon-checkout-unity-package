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
        
        // public Step CreateAndStoreThingOperation() 
        // =>
        //     new Step(name   : $"create_and_store_thing_operation"
        //             ,action : async (s) =>
        //                     {
        //                         this.Node.Storage.Store("myInt", 4);
        //                     });
        // 
        // public Step TestRead() 
        // =>
        //     new Step(name   : $"test_read"
        //             ,action : async (s) =>
        //                     {
        //                         s.Log($"storage id = {this.Node.ID.StorageID}");
        //                         s.Log($"keys : {this.Node.Storage.GetStoredKeys().ToMultilineString()}");
        //                         
        //                         var value = this.Node.Storage.Load("myInt");
        //                         s.Log(value);
        //                     });
        
        public Step Op(string OpString) 
        =>
            new Step(name   : $"Op"
                    ,action : async (s) =>
                    {   
                        this.Node.Storage.Store("OpString", OpString);
                        
                        TextNode parsed = TextNode.Parse(OpString);
        
                        ///////////
                        
                        foreach (var textNode in parsed.Node.Descendants().OfType<TextNode>())
                        {
                            if (textNode.RawText.IsNullOrEmpty()
                            ||  textNode.RawText.ToLower().StartsWith("> origin") )
                                continue;
                            
                            var ve = new VirtualEntity(){TextNode = textNode};
                            this.Node.AddChild(ve);
                        }
                        
                        ///////////
                        
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
                        if (!this.Node.Storage.HasKey("OpString"))
                            return;
                        
                        string   OpString = this.Node.Storage.Load<string>("OpString");
                        TextNode parsed   = TextNode.Parse(OpString);
                        
                        ///////////
                        
                        foreach (var textNode in parsed.Node.Descendants().OfType<TextNode>())
                        {
                            if (textNode.RawText.IsNullOrEmpty()
                            ||  textNode.RawText.ToLower().StartsWith("> origin") )
                                continue;
                            
                            var ve = new VirtualEntity(){TextNode = textNode};
                            this.Node.AddChild(ve);
                        }
                        
                        ///////////
                        
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
