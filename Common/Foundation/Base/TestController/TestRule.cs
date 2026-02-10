using System;
using System.Collections.Generic;

namespace Galleon.Checkout.Foundation.Tests
{
    public class TestRule
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public string       EventStepName;
        public List<string> ActionStepPaths = new List<string>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public static TestRule Parse(string str)
        {
            // Rule text formats:
            // - on 'event' do 'action'
            // - on 'event' do 'action1' and 'action2' and 'action3'

            var mainMatch = System.Text.RegularExpressions.Regex.Match(str, @"on\s*'([^']+)'\s*do\s*(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!mainMatch.Success)
                throw new FormatException("Input string is not in the correct format. Expected format: on 'EventStepName' do 'ActionStepName' [and 'ActionStepName2' ...]");

            var eventName     = mainMatch.Groups[1].Value;
            var actionsPart   = mainMatch.Groups[2].Value;
            var actionMatches = System.Text.RegularExpressions.Regex.Matches(actionsPart, @"'([^']+)'");
            var actions       = new List<string>();

            foreach (System.Text.RegularExpressions.Match m in actionMatches)
            {
                if (m.Success)
                    actions.Add(m.Groups[1].Value);
            }

            if (actions.Count == 0)
                throw new FormatException("No actions found. Expected at least one action in single quotes after do.");

            return new TestRule
                   {
                   EventStepName   = eventName,
                   ActionStepPaths = actions
                   };
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public override string ToString()
        {
            var actions = ActionStepPaths != null && ActionStepPaths.Count > 0 ? string.Join(" and ", ActionStepPaths) : "";
            return $"Rule: On {EventStepName} Do {actions}";
        }
    }
}