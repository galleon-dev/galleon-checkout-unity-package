using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Galleon.Checkout
{
    public partial class SliceHub : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<Slice> Slices = new List<Slice>();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        // [RuntimeInitializeOnLoadMethod()]
        // public static async void InitializeOnRuntime()
        // {
        //     var sliceHub = Root.Instance.Node.Descendants().OfType<SliceHub>().FirstOrDefault();
        //     await sliceHub?.SystemInit().Execute();
        // }
        
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
            new Step(name   : $"report"
                    ,action : async (s) =>
                    {
                        s.Log("Hub folder = Assets/TEMP");

                        // List all types that inherit from Slice
                        var sliceTypes = GetAllSliceTypes();
                        s.Log($"Found {sliceTypes.Count} types that inherit from Slice:");
                        foreach (var type in sliceTypes.OrderBy(t => t.FullName))
                        {
                            s.Log($"  - {type.FullName} (Assembly: {type.Assembly.GetName().Name})");
                        }
                        
                        List<Type> GetAllSliceTypes()
                        {
                            var sliceTypes = new List<Type>();
                            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                            foreach (var assembly in assemblies)
                            {
                              //Debug.Log($"+assembly {assembly.FullName}");
                                try
                                {
                                    var types = assembly.GetTypes()
                                        .Where(t => t.IsClass &&
                                                   !t.IsAbstract &&
                                                   typeof(Slice).IsAssignableFrom(t) &&
                                                   t != typeof(Slice));

                                    sliceTypes.AddRange(types);
                                }
                                catch (ReflectionTypeLoadException)
                                {
                                    // Skip assemblies that can't be fully loaded
                                    continue;
                                }
                            }

                            return sliceTypes;
                        }
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
                        string OpString = "> Slice slice2"
                                 + "\n" + "";
                        await this.Node.Live.Operation(OpString).Execute(); 
                    });
        
    }
}
