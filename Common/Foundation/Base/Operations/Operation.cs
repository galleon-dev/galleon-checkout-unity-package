using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Operation : Entity
    {
        //////////////////////////////////////////////////////////////////////////// Members
        
        public STATE State = new STATE();
        
        public Step  Flow;
        
        //////////////////////////////////////////////////////////////////////////// State
        
        public string StateFileRelativePath => $"/TEMP/operation_{State.ID}_state.json";
        
        [Serializable] 
        public class STATE
        {
            public string       ID               { get; set; } = "";
            public List<string> CompletedStepIDs { get; set; } = new();
        }
        
        //////////////////////////////////////////////////////////////////////////// Lifecycle

        public Operation(string ID)
        {
            this.State.ID             = ID;
            this.Flow                 = new Step(name: $"operation_{ID}");
            this.Flow.OnPostChildStep = (completedStep) =>
                                        {       
                                            this.State.CompletedStepIDs.Add(completedStep.Name);
                                        };
        }
        
        public Operation AddStep(Step step)
        {
            this.Flow.AddChildStep(step);
            return this;
        }
        
        //////////////////////////////////////////////////////////////////////////// API
        
        public async Task Execute()
        {
            await Flow.Execute();
        }
        
        public async Task Resume()
        {
            
        }
        
        
        //////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        public static void InitializeOnLoad()
        {
            Debug.Log("OperationController.InitializeOnLoad()");
            
            
        }
        
        //////////////////////////////////////////////////////////////////////////// Storage
        
        public void Save()
        {
            
        }
        
        public static Operation Load()
        {
            return default;
        }
    }
}

