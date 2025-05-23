using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Galleon.Checkout;
using UnityEngine.UIElements;
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
        
        ////////////////////////////////////////// Link
        
        public StepController StepController => Root.Instance.Runtime.StepController;
        
        //////// TEMP
        public static event Action<Step> ReportStep;
        public async Task CaptureReport()
        {
            ReportStep?.Invoke(this);
            await Task.Yield();
        }
        
        
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
                
                StepController.Steps.Add(this);
                
                // Execute
                if (Action != null)
                    await Action.Invoke(this);
                
                // Execute Child Steps
                for (int i = 0; i < ChildSteps.Count; i++)
                {
                    var child = ChildSteps[i];
                    await child.Execute();
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
        public void AddChildStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            this.ChildSteps.Add(tempStep);
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

        ////////////////////////////////////////// Awaitable Implementation
        
        public TaskAwaiter GetAwaiter()
        {
            return Execute().GetAwaiter();
        }
        
        ////////////////////////////////////////// Inspector
        
        public class StepInspector : Inspector<Step>
        {
            public StepInspector(Step target) : base(target)
            {
                TitleLabel.text = $"Step {target.Name}";
            }
        }
    }
}