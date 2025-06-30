using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Galleon.Checkout;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using StepAction=System.Func<Galleon.Checkout.Step,System.Threading.Tasks.Task>;

namespace Galleon.Checkout
{
    public class Step : Entity
    {
        ////////////////////////////////////////// Members
        
        public string           Name;
        public List<string>     Tags                        = new();
        public StepAction       Action;
        
        public List<Step>       ChildSteps                  = new();
        public List<Step>       PreSteps                    = new();
        public List<Step>       PostSteps                   = new();
        public Step             ParentStep { get; set; }    = null;

        public List<Breadcrumb> Breadcrumbs                 = new();
        
        public Stopwatch        ExecutionStopwatch;
        public DateTime         StartTime                   = DateTime.MaxValue;
        public DateTime         EndTime                     = DateTime.MaxValue;
        
        
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

        public Step(string                     name   = null
                    ,IEnumerable<string>       tags   = default
                    ,StepAction                action = null
                    ,[CallerMemberName] string CallerMemberName = ""
                    ,[CallerLineNumber] int    CallerLineNumber = 0
                    ,[CallerFilePath  ] string CallerFilePath   = "")
        {
            // Name
            this.Name = name;
            
            // Tags
            if (tags != default)
                this.Tags.AddRange(tags);
            
            // Action
            this.Action = action;
            
            // Breadcrumb
            this.Breadcrumbs.Add(new Breadcrumb(CallerMemberName, CallerLineNumber, CallerFilePath, displayName : "creation_breadcrumb"));
        }
    
        ////////////////////////////////////////// Main Methods

        public async Task Execute(Breadcrumb                sourceBreadcrumb   = default
                                 ,[CallerMemberName] string CallerMemberName   = ""
                                 ,[CallerLineNumber] int    CallerLineNumber   = 0
                                 ,[CallerFilePath  ] string CallerFilePath     = "")
        {
            try
            {   
                ////////////////////////////////////////////////
                
                // Log
                this.Log($"<color=white>[{this.Name}]</color>");
                
                ////////////////////////////////////////////////
                
                // Breadcrumbs and stats
                StartTime                = DateTime.Now;
                ExecutionStopwatch       = Stopwatch.StartNew();
                var execution_breadcrumb = sourceBreadcrumb ?? new Breadcrumb(CallerMemberName, CallerLineNumber, CallerFilePath, displayName: "execution_breadcrumb");
                this.Breadcrumbs.Add(execution_breadcrumb);

                ////////////////////////////////////////////////
                
                // Add to steps
                StepController.Steps.Add(this);
                
                // Add as linked child to step controller
                Root.Instance.Runtime.StepController.Node.AddLinkedChild(this);
                
                // set display name
                this.Node.DisplayName = $"Step {this.Name}";
                
                ////////////////////////////////////////////////
                
                // Execute
                if (Action != null)
                    await Action.Invoke(this);
                
                //////////////////////////////////////////////// Temp

                // Capture report
                if (this.Tags.Contains("report")
                ||  this.ParentStep != null && this.ParentStep.Tags.Contains("report"))
                {
                    await this.CaptureReport();
                }
                
                ////////////////////////////////////////////////
                
                // Execute Pre Steps
                for (int i = 0; i < PreSteps.Count; i++)
                {
                    var   preStep = PreSteps[i];
                    await preStep.Execute();
                }
                
                ////////////////////////////////////////////////
                
                // Execute Child Steps
                for (int i = 0; i < ChildSteps.Count; i++)
                {
                    var child = ChildSteps[i];
                    await child.Execute();
                }
                
                ////////////////////////////////////////////////
                
                // Execute Post Steps
                for (int i = 0; i < PostSteps.Count; i++)
                {
                    var   postStep = PostSteps[i];
                    await postStep.Execute();
                }
                
                ////////////////////////////////////////////////
                
                ExecutionStopwatch.Stop();
                EndTime = DateTime.Now;
                var execution_finished_breadcrumb = sourceBreadcrumb ?? new Breadcrumb(CallerMemberName, CallerLineNumber, CallerFilePath, displayName: "execution_finished_breadcrumb");
                this.Breadcrumbs.Add(execution_finished_breadcrumb);                
            }
            catch (Exception e)
            {
                Debug.Log($"<color=red>[HANDLED-ERROR]</color> {e.ToString()}");
            }
        }
        
        ////////////////////////////////////////// Child Steps
        
        public void AddChildStep(Step step)
        {
            this.ChildSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddChildStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            this.ChildSteps.Add(tempStep);
        }
        
        
        ////////////////////////////////////////// Pre Steps
        
        public void AddPreStep(Step step)
        {
            this.PreSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddPreStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            this.PreSteps.Add(tempStep);
        }
        
        
        ////////////////////////////////////////// Post Steps
        
        public void AddPostStep(Step step)
        {
            this.PostSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddPostStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            this.PostSteps.Add(tempStep);
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
                
                this.Add(new Label(target.Name));
                
                // seporator
                this.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(Color.gray), marginTop = 4, marginBottom = 4 } });
            }

            public override void OnExplorerItemAutoRefresh()
            {
                var childCoutText               = Target.ChildSteps.Count > 0 ? $"({Target.ChildSteps.Count})" : "";
                ExplorerItem.ButtonFoldout.Text = TitleLabel.text = $"Step {Target.Name + childCoutText}";
            }
        }
    }
}

