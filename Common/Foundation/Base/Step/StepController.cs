using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class StepController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<Step> Steps = new List<Step>();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Debug
        
        public string DumpSteps()
        {
            StringBuilder str = new StringBuilder();
            
            void DumpStep(Step step, StringBuilder str, int depth = 0)
            {
                str.AppendLine($"{new string(' ', depth * 2)}> [{step.Name}]");

                foreach (var preStep in step.PreSteps)
                {
                    DumpStep(preStep, str, depth + 1);
                }
                foreach (var childStep in step.ChildSteps)
                {
                    DumpStep(childStep, str, depth + 1);
                }
                foreach (var postStep in step.PostSteps)
                {
                    DumpStep(postStep, str, depth + 1);
                }
            }

            foreach (var step in Steps)
            {
                DumpStep(step, str);
            }
            
            return str.ToString();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inspector
        
        #region INSPECTOR
        
        public class Inspector : Inspector<StepController>
        {
            //// Members
            
            private VisualElement stepsContainer;
            
            //// Lifecycle
            
            public Inspector(StepController target) : base(target)
            {   
                var refreshButton = new Button(Refresh) { text = "Refresh" };
                Add(refreshButton);
                
                // stepsContainer = new VisualElement();
                // target.Node.ExplorerItem.ChildrenHolder.Add(stepsContainer);
                
                // Refresh();
            }
            
            //// Refresh
            
            private void Refresh()
            {
                stepsContainer.Clear();
                
                foreach (var step in Target.Steps)
                {
                    var stepInspector = new ExplorerItem(step);
                    stepsContainer.Add(stepInspector);
                }
                
                RefreshChildren();
            }
        }
        
        #endregion // INSPECTOR
    }
}
