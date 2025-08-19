using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Codice.Client.BaseCommands;
using UnityEngine;
using Galleon.Checkout;
using Galleon.Checkout.Foundation;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using StepAction=System.Func<Galleon.Checkout.Step,System.Threading.Tasks.Task>;

namespace Galleon.Checkout
{
    public class Step : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string             Name;
        public List<string>       Tags                        = new();
        public StepAction         Action;
        
        public List<Step>         ChildSteps                  = new();
        public List<Step>         PreSteps                    = new();
        public List<Step>         PostSteps                   = new();
        public Step               ParentStep { get; set; }    = null;

        public List<Breadcrumb>   Breadcrumbs                 = new();
        
        public Stopwatch          ExecutionStopwatch;
        public DateTime           StartTime                   = DateTime.MaxValue;
        public DateTime           EndTime                     = DateTime.MaxValue;
        
        public List<string>       StepLog                     = new();
        
        public STEP_STATE         StepState                   = STEP_STATE  .None;
        public ACTION_STATE       ActionState                 = ACTION_STATE.None;
        
        public Action<Step>       OnPostChildStep             = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types
        
        public enum STEP_STATE
        {
            None,
            Running,
            Completed,
            PreviouslyCompleted,
        }
        
        public enum ACTION_STATE
        {
            None,
            Success,
            Error,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        public static event Func<Step, Task> OnStepExecuted;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Link
        
        public StepController StepController => Root.Instance.Context.StepController;
        
        //////// TEMP
        public static event Action<Step> ReportStep;
        public async Task CaptureReport()
        {
            ReportStep?.Invoke(this);
            await Task.Yield();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Step(string                     name       = null
                   ,IEnumerable<string>       tags       = default
                   ,StepAction                action     = null
                   ,[CallerMemberName] string callerName = ""
                   ,[CallerLineNumber] int    callerLine = 0
                   ,[CallerFilePath  ] string callerPath = "")
        {
            // Name
            this.Name = name ?? callerName;
            
            // Tags
            if (tags != default)
                this.Tags.AddRange(tags);
            
            // Action
            this.Action = action;
            
            // Breadcrumb
            this.Breadcrumbs.Add(new Breadcrumb(callerName, callerLine, callerPath, displayName : "creation_breadcrumb"));
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Setters
        
        private Step SET         (Action            setter   ) {  setter?.Invoke(); return this; }
        public  Step setName     (string            name     ) => SET(() => this.Name      = name);
        public  Step setTags     (params string[]   tags     ) => SET(() => this.Tags      .AddRange(tags)); 
        public  Step setAction   (StepAction        action   ) => SET(() => this.Action    = action);
        
        public Step Temp()
        =>
            new Step()
           .setName  ("name")
           .setTags  ("bla", "bli")
           .setAction(async s =>
                      {
                          s.Log("step action");
                      });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Methods
        
        
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
                if (this.ParentStep == null)
                    Root.Instance.Context.StepController.Node.AddLinkedChild(this);
                //else
                //    this.ParentStep.Node.AddLinkedChild(this);
                
                // set display name
                this.Node.DisplayName = $"Step {this.Name}";
                
                ////////////////////////////////////////////////
                
                if (StepState == STEP_STATE.PreviouslyCompleted)
                    goto skip_to_end;
                
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
                
                skip_to_end:
                
                ////////////////////////////////////////////////
                
                ExecutionStopwatch.Stop();
                EndTime = DateTime.Now;
                var execution_finished_breadcrumb = sourceBreadcrumb ?? new Breadcrumb(CallerMemberName, CallerLineNumber, CallerFilePath, displayName: "execution_finished_breadcrumb");
                this.Breadcrumbs.Add(execution_finished_breadcrumb);                
                
                ////////////////////////////////////////////////
                
                // Fire Event
                if ( OnStepExecuted?.Invoke(this) is Task t )
                    await t;
                
            }
            catch (Exception e)
            {
                Debug.Log($"<color=red>[CAUGHT]</color> {e.ToString()}");
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Child Steps
        
        public void AddChildStep(Step step)
        {
            this.ChildSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddChildStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            AddChildStep(tempStep);
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Pre Steps
        
        public void AddPreStep(Step step)
        {
            this.PreSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddPreStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            AddChildStep(tempStep);
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Post Steps
        
        public void AddPostStep(Step step)
        {
            this.PostSteps.Add(step);
            step.ParentStep = this;
            this.Node.AddLinkedChild(step);
        }
        public void AddPostStep(string name = "temp", StepAction action = default)
        {
            Step tempStep = new Step(name, tags: new []{"temp"}, action: action);
            AddChildStep(tempStep);
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Log Methods
        
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
            this.StepLog.Add($"{prefix}{message}");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inspector
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public class StepInspector : Inspector<Step>
        {
            public Foldout breadcrumbsFolderout;
            
            public StepInspector(Step target) : base(target)
            {
                TitleLabel.text = $"Step {target.Name}";
                
                // NameLabel
                this.Add(new Label(target.Name));
                
                // Seperator
                this.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(Color.gray), marginTop = 4, marginBottom = 4 } });
                
                // Breadcrumbs
                breadcrumbsFolderout = new Foldout() { text = "Breadcrumbs", value = true }; this.Add(breadcrumbsFolderout);
                breadcrumbsFolderout?.Clear();
                foreach (var breadcrumb in Target.Breadcrumbs)
                {
                    var breadcrumbInspector = new Breadcrumb.Inspector(breadcrumb, breadcrumb.DisplayName);
                    breadcrumbsFolderout.Add(breadcrumbInspector);
                }
            }

            public override void OnExplorerItemAutoRefresh()
            {
                var childCoutText               = Target.ChildSteps.Count > 0 ? $"({Target.ChildSteps.Count})" : "";
                ExplorerItem.ButtonFoldout.Text = TitleLabel.text = $"Step {Target.Name + childCoutText}";
                
            }
        }
    }
}

