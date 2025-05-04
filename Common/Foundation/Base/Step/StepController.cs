using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class StepController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<Step> Steps = new List<Step>();
        
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
                
                stepsContainer = new VisualElement();
                Add(stepsContainer);
                
                Refresh();
            }
            
            //// Refresh
            
            private void Refresh()
            {
                stepsContainer.Clear();
                
                foreach (var step in Target.Steps)
                {
                    var stepInspector = new EntityInspector(step);
                    stepsContainer.Add(stepInspector);
                }
            }
        }
        
        #endregion // INSPECTOR
    }
}
