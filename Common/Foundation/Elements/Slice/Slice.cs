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
        public VirtualEntity t2 =  new VirtualEntity() { TextNode = new TextNode("> Thing t2 #yellow #4x4 #10,10 #collider #rigidbody") };
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step Op() 
        =>
            new Step(name   : $"Op"
                    ,action : async (s) =>
                    {   
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
                    })
                    {
                        IsSilentLog = true,
                    };
        
        
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

/// > Feb27 App
///     > (Application)
///         > (Services)
///             > Log
///             > Storage
///             > Config
///             > Analytics
///             > Network
///             > Etc.
///         > (Core)
///             > Flow
///             > Segments
///     > (UI)
///         > Screen
///             > Panel
///                 > List
///                     > Item
///                         > Code
///                         > Text
///                         > Image
///                         > Button
///     > (Scene)
///         > Scene
///             > PSE
///                 > prefab
///                     > GO
///                         > Component
///                             > Field / ref
///                     > Model
///                         > Mesh
///                             > Material
///                                 > Sprite
///                                     > Texture
///                     > Physics
///                         > Rigidbody
///                         > Collider
///                     > Aniaiton
///                         > State?
///                         > Clip
///                             > Keyframe
///                                 > Value
///                 > Motor
///     > (Systm)
///         > Controller
///         > Rule
///             > Event
///             > Confition
///             > Action
///                 > Method
///                 > Step
///                 > Behaviour 
///     
