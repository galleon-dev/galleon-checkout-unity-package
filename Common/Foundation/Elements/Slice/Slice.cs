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
        
        public VirtualEntity t1 =  new VirtualEntity() { TextNode = new TextNode("> Thing t1") };
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step Op() 
        =>
            new Step(name   : $"Op"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR
                        EditorPrefs.SetString("session_key", "bla");
                        #endif
                        
                        var ves = this.Node.Descendants().OfType<VirtualEntity>();

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
                        var ves = this.Node.Descendants().OfType<VirtualEntity>();

                        foreach (var ve in ves)
                        {
                            await ve.DoNextLiveStep();
                        }
                    });
        
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
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

