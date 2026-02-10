using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
using Galleon.Checkout.Foundation;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Operation : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public OPERATION_STATE OperationState = new OPERATION_STATE();
        public Step            Flow;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string ID
        {
            get => OperationState.Op.ID;
            set => OperationState.Op.ID = value;
        }
        
        public Dictionary<string, object> Data
        {
            get => OperationState.Data;
            set => OperationState.Data = value;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public string OperationStateFileRelativePath => $"TEMP/operation_{ID}_state.json";
        
        [Serializable] 
        public class OPERATION_STATE
        {
            public OperationData              Op   = new OperationData();
            public Dictionary<string, object> Data = new();
        }
        [Serializable]
        public class OperationData
        {
            public string                     ID               { get; set; } = "";
            public List<string>               CompletedStepIDs { get; set; } = new();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Operation(string id)
        {
            this.ID                       = id;
            this.Flow                     = new Step(name: $"operation_{id}");
            this.Flow.PreChildStepAction  = (completedChildStep) =>
                                          {       
                                              this.OperationState.Op.CompletedStepIDs.Add(completedChildStep.Name);
                                              this.Save();
                                              Root.Instance.Context.SystemServices.Operations.Save();
                                          };
            this.Flow.PostStepAction      = (completedStep) =>
                                          {
                                              Root.Instance.Context.SystemServices.Operations.OngoingOperations.Remove(id);
                                              Root.Instance.Context.SystemServices.Operations.Save();
                                          };
        }
        
        public Operation AddStep(Step step)
        {
            this.Flow.AddChildStep(step);
            return this;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task Execute()
        {
            Root.Instance.Context.SystemServices.Operations.OngoingOperations.Add(ID);
            
            await Flow.Execute();
        }
        
        public async Task Resume()
        {
            await Flow.Execute();
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        public static void InitializeOnLoad()
        {
            // Debug.Log("OperationController.InitializeOnLoad()");
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this.OperationState);
            var path = Path.Combine(Application.dataPath, OperationStateFileRelativePath);
            File.WriteAllText(path, json);
        }
        
        public static Operation Load(string ID)
        {
            var operation = Root.Instance.Context.SystemServices.Operations.AllRegisteredOperations.Single(x => x.ID == ID);
            var path      = Path.Combine(Application.dataPath, operation.OperationStateFileRelativePath);

            if (!File.Exists(path))
                throw new Exception($"operation {ID} state file not found at path: " + path);

            var json                 = File.ReadAllText(path);
            operation.OperationState = JsonConvert.DeserializeObject<OPERATION_STATE>(json);

            foreach (var step in operation.Flow.ChildSteps)
            {
                if (operation.OperationState.Op.CompletedStepIDs.Contains(step.Name))
                    step.StepState = Step.STEP_STATE.PreviouslyCompleted;
            }
            
            return operation;
        }
        
        public async void Test()
        {
            /// a.crud_plus(f1)
            ///     add_child()
            ///     create()
            ///
            /// a.op_plus(f1)
            ///     new step("plus")
            ///         new step("add_child")
            ///         new step("create")
            ///
            
            await this.Node.Live2.CRUD_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.STEP_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.P_OP_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.OP_PLUS  (new Folder() {FolderName = "f1"});
            await this.Node.Live2.LIVE_PLUS(new Folder() {FolderName = "f1"});
        }
    }    
}