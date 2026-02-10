using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout.Foundation.Tests
{
    public class TestScenario : Entity
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public string               Name;
        public Collection<TestRule> Rules = new();
        public int                  CurrentRuleIndex;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public TestRule             CurrentTestRule => (Rules.Count > 0 && CurrentRuleIndex >= 0 && CurrentRuleIndex < Rules.Count) 
                                                     ?  Rules[CurrentRuleIndex] 
                                                     :  null;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public TestScenario(string name, string[] rules = null)
        {
            this.Name = name;
            
            if (rules != null)
                foreach (var ruleSTR in rules)
                    this.AddRule(ruleSTR);
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void AddRule(string   ruleStr) => Rules.Add(TestRule.Parse(ruleStr));
        public void AddRule(TestRule rule)
        {
            Rules.Add(rule);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
// Registers scenario rules to execute when matching steps are triggered
public void RegisterScenarioRules()
{
    Step.OnPreStepExecute += async s =>
                             {
                                 // Skip if no rules exist or index is out of bounds
                                 if (Rules.Count == 0 || CurrentRuleIndex < 0 || CurrentRuleIndex >= Rules.Count)
                                     return;

                                 var    current            = CurrentTestRule;
                                 string rule_eventStepName = current.EventStepName;

                                 // Check if current step matches the rule's event step
                                 if (s.Name == rule_eventStepName)
                                 {
                                     //Debug.Log($"#>#>#>#>#>#>#>#>#>#>#>#>#>#> TEST STEP: {s.Name}");
                                     
                                     // Move to next rule for subsequent step executions
                                     CurrentRuleIndex++;

                                     // Execute all actions in order
                                     if (current.ActionStepPaths != null && current.ActionStepPaths.Count > 0)
                                     {
                                         foreach (var actionPath in current.ActionStepPaths)
                                         {
                                            // Find the step by path and execute if found
                                             var step = FindStep(actionPath);
                                             if (step != null)
                                             {
                                                 await Task.Delay(1000);
                                                 step.Execute();
                                             }
                                         }
                                     }
                                 }
                             };
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step ActivateScenario() 
        =>
            new Step(name   : $"execute_test_scenario___{Name}"
                    ,action : async (s) =>
                    {
                        RegisterScenarioRules();
                    });
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Searches for a step by name in the entity hierarchy
        private Step FindStep(string stepName)
        {
            IEntity rootEntity = Root.Instance.Runtime;

            // Traverse all descendants to find the step
            foreach (var entity in rootEntity.Node.Descendants())
            {
                try
                {
                    var step = entity.Node.Reflection.Steps().FirstOrDefault(s => s.Name == stepName);
                    if (step != null)
                        return step;
                }
                catch (Exception e)
                {

                }
            }

            return default;
        }
    }
}