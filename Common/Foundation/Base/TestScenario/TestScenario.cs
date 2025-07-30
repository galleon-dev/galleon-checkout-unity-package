using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Galleon.Checkout.Foundation
{
    public class TestScenario : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public IEntity          Target;
        public string           Name;
        public List<string>     Expressions = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public TestScenario()
        {
        }

        public TestScenario(string                    name       = null
                           ,[CallerMemberName] string callerName = ""
                           ,[CallerLineNumber] int    callerLine = 0
                           ,[CallerFilePath  ] string callerPath = ""
                           ,params string[]           expressions) : this()
        {
            this.Name = name ?? callerName; 
            this.Expressions.AddRange(expressions);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

        public Step RunScenario()
        =>
            new Step(name   : $"test_scenario_{Name}"
                    ,action : async (s) =>
                    {
                        foreach (var step in Steps())
                        {
                            s.AddChildStep(step);
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        private IEnumerable<Step> Steps()
        {
            foreach (var expression in Expressions)
            {
                var testStep = DynamicExpression.Evaluate<Step>(this.Target, expression);
                yield return testStep;
            }
        }
    }
}