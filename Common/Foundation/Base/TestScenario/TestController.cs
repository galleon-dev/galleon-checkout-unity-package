using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Galleon.Checkout.Foundation.Tests
{
    public class TestController : Entity
    {
        public Collection<Scenario> Scenarios = new Collection<Scenario>();
        
        public Step Test() 
        =>
            new Step(name   : $"Test"
                    ,action : async (s) =>
                    {
                        Scenario scenario = new Scenario();
                        this.Scenarios.Add(scenario);
                        
                        string[] rules = new[]
                                       {
                                          @$"on ""some_step_1"" do ""some_other_step_1""",
                                          @$"on ""some_step_2"" do ""some_other_step_2""",
                                          @$"on ""some_step_3"" do ""some_other_step_3""",
                                       };

                        foreach (var str in rules)
                        {
                            var rule = Rule.Parse(str);
                            scenario.Rules.Add(rule);
                            s.Log(rule.ToString());
                        }
                    });
    }
    
    public class Scenario : Entity
    {
        public Collection<Rule> Rules = new();
        public int              CurrentRuleIndex;
        public Rule             CurrentRule => Rules.Count > 0 ? Rules[CurrentRuleIndex] : null;
    }
    
    public class Rule
    {
        public string EventStepName;
        public string ActionStepName;
        
        public static Rule Parse(string str)
        {
            // on "bla" do "bla"
            
            var parts = str.Split(new[] { "on \"", "\" do \"" }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length != 2 
            || !parts[1].EndsWith("\""))
                throw new FormatException("Input string is not in the correct format. Expected format: on \"EventStepName\" do \"ActionStepName\"");
            
            parts[1] = parts[1].TrimEnd('"');
            
            return new Rule
            {
                EventStepName  = parts[0],
                ActionStepName = parts[1]
            };
        }
        
        
        public override string ToString()
        {
            return $"Rule: On {EventStepName} Do {ActionStepName}";
        }
    }
}


