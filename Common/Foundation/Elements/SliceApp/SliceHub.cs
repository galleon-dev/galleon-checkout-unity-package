using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class SliceHub : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<Slice> Slices = new List<Slice>();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        [RuntimeInitializeOnLoadMethod()]
        public static async void InitializeOnRuntime()
        {
            var sliceHub = Root.Instance.Node.Descendants().OfType<SliceHub>().FirstOrDefault();
            await sliceHub?.SystemInit().Execute();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flows
        
        public Step SystemInit() 
        =>
            new Step(name   : $"slice_hub_system_init"
                    ,action : async (s) =>
                    {
                        var slices = Root.Instance.Node.Descendants().OfType<Slice>();
                        this.Slices.AddRange(slices);
                    });
        
        public Step MainFlow() 
        =>
            new Step(name   : $"slice_hub_main_flow"
                    ,action : async (s) =>
                    {    
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Debug
        
        public Step PrepAnReport() 
        =>
            new Step(name   : $"prep_and_report"
                    ,action : async (s) =>
                    {
                        
                    });
        
        public Step PlusSlice1() 
        =>
            new Step(name   : $"plus_slice_1"
                    ,action : async (s) =>
                    {
                        string OpString = "> Slice slice1"
                                 + "\n" + "";
                        
                        await this.Node.Live.Operation(OpString).Execute(); 
                    });
        
        public Step PlusSlice2() 
        =>
            new Step(name   : $"plus_slice_2"
                    ,action : async (s) =>
                    {
                        
                    });
    }
}

/// Hub
/// Slice
/// ...
/// Hub + Slice1 + Slice2
/// folder slice1
///     slice1.cs
///         class Slice1 : MonoBehaviour, IEntity
///     slice1.txt
/// folder slice2
///     slice2.cs
///         class Slice2 : MonoBehaviour, IEntity
///     slice2.txt
