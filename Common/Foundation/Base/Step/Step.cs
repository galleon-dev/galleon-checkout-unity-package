using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Galleon.Checkout;
using StepAction=System.Func<Galleon.Checkout.Step,System.Threading.Tasks.Task>;

namespace Galleon.Checkout
{

    public class Step
    {
        ////////////////////////////////////////// Members
        
        public string       Name;
        public List<string> Tags = new();
        public StepAction   Action;
        
        public List<Step>   ChildSteps = new();
        public Step         ParentStep = null;
        
        ////////////////////////////////////////// Lifecycle

        public Step(string name = null, IEnumerable<string> tags = default, StepAction action = null)
        {
            this.Name = name;
            
            if (tags != default)
                this.Tags.AddRange(tags);
            
            this.Action = action;
        }
    
        ////////////////////////////////////////// Main Methods

        public async Task Execute()
        {
            try
            {
                // Log
                this.Log($"<color=white>[{this.Name}]</color>");
                
                // Execute
                await Action(this);
                
                // Execute Child Steps
                foreach (var childStep in ChildSteps)
                {
                    await childStep.Execute();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        
        ////////////////////////////////////////// Child Steps
        
        public void AddChildStep(Step step)
        {
            this.ChildSteps.Add(step);
            step.ParentStep = this;
        }
        
        ////////////////////////////////////////// Methods
        
        public void Log(object message)
        {
            #region PREFIX
            int  parents = 0;
            Step current = this;
            
            while (current.ParentStep != null)
            {
                parents++;
                current = current.ParentStep;
            }
            
            string prefix = new string(' ', parents * 4);
            #endregion // PREFIX
            
            Debug.Log($"{prefix}{message}");
        }
    }
}