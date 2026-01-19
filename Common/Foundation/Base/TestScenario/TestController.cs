using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galleon.Checkout.Foundation.Tests
{
    public class TestController : Entity
    {
        public Collection<Scenario> Scenarios = new Collection<Scenario>();
        
        public static void RunTest()
        {
            
        }
    
        public Step Test() 
        =>
            new Step(name   : $"Test"
                    ,action : async (s) =>
                    {
                        Scenario scenario = new Scenario();
                        this.Scenarios.Add(scenario);
                        
                        string[] rules = new[]
                        {
                            // Test case - add frst card
                            @$"on 'sample_app_start'                         do 'test_purchase_product_1'",
                            @$"on 'on_panel_focus_PreselectionPanel'         do 'preselection_panel_test_checkout'",
                            @$"on 'on_panel_focus_CheckoutPanel '            do 'checkout_panel_test_select_other_pm'",
                            @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_new_card'",
                            @$"on 'on_panel_focus_CreditCardInfoPanel'       do 'credit_card_info_panel_fill_test_card' and 'credit_card_info_panel_confirm'",
                            @$"on 'on_panel_focus_SuccessPanel'              do 'fill_test_email' and 'test_click_send_receipt'", // and 'test_close_checkout_screen_clicked'",
                        
                            // Test case - pay again
                            @$"on 'on_back_to_store_screen'                  do 'test_purchase_product_2'",
                            @$"on 'on_panel_focus_CheckoutPanel '            do 'checkout_panel_test_confirm_purchase'",
                            @$"on 'on_panel_focus_SuccessPanel'              do 'success_panel_wait_and_do_nothing'",
                        
                            // // Test case - PayPal payment
                            // @$"on 'on_back_to_store_screen'                  do 'test_purchase_product_3'",
                            // @$"on 'on_panel_focus_CheckoutPanel'             do 'checkout_panel_test_select_other_pm'",
                            // @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_paypal'",
                            // @$"on 'on_paypal_redirect'                       do 'test_paypal_auto_complete'",
                            // @$"on 'on_panel_focus_SuccessPanel'              do 'success_panel_wait_and_do_nothing'",

                            // // Test case - Google Pay payment
                            // @$"on 'on_back_to_store_screen'                  do 'test_purchase_product_4'",
                            // @$"on 'on_panel_focus_CheckoutPanel'             do 'checkout_panel_test_select_other_pm'",
                            // @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_google_pay'",
                            // @$"on 'on_google_pay_sheet'                      do 'test_google_pay_auto_complete'",
                            // @$"on 'on_panel_focus_SuccessPanel'              do 'success_panel_wait_and_do_nothing'",

                            // // Test case - Failed card payment
                            // @$"on 'on_back_to_store_screen'                  do 'test_purchase_product_5'",
                            // @$"on 'on_panel_focus_PreselectionPanel'         do 'preselection_panel_test_checkout'",
                            // @$"on 'on_panel_focus_CheckoutPanel'             do 'checkout_panel_test_select_other_pm'",
                            // @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_new_card'",
                            // @$"on 'on_panel_focus_CreditCardInfoPanel'       do 'credit_card_info_panel_fill_declined_card' and 'credit_card_info_panel_confirm'",
                            // @$"on 'on_panel_focus_ErrorPanel'                do 'error_panel_test_close'",
                            
                        };

                        foreach (var rule in rules)
                        {
                            var r = Rule.Parse(rule);
                            scenario.Rules.Add(r);
                        }
                        
                        scenario.RegisterScenarioRules();
                        
                        /// On 'sample_app_start'      Do 'SampleApp.click_buy_product_1'
                        /// On 'Checkout_screen_focus' Do 'click_add_card'
                        /// On 'add_card_screen_focus' Do 'fill_card_details' > 'click_confim'
                        /// On 'success_page'          Do 'fill_email' > 'click_confirm' > 'click_X'
                        /// .
                        /// On 'home_screen'           Do 'SampleApp.click_buy_product_2'
                        /// On 'Checkout_screen_focus' Do 'click_select_other_payment_methods'
                        /// On 'select_screen_focus'   Do 'click_paypal'
                        /// - (await special test url with auto deeplink)
                        /// - ('success_page')
                        /// .
                        /// On 'home_screen'           Do 'SampleApp.click_buy_product_3'
                        /// On 'Checkout_screen_focus' Do 'click_select_other_payment_methods'
                        /// On 'select_screen_focus'   Do 'click_google_pay'
                        /// - (await special test url with auto deeplink)
                        /// - ('success_page')

                        foreach (var str in rules)
                        {
                            var rule = Rule.Parse(str);
                            scenario.Rules.Add(rule);
                            s.Log(rule.ToString());
                        }
                    });
        
        public Step PrintHello() 
        =>
            new Step(name   : $"print_hello"
                    ,action : async (s) =>
                    {
                        s.Log("Hello");
                    });
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class Scenario : Entity
    {
        public Collection<Rule> Rules             = new();
        public int              CurrentRuleIndex;
        public Rule             CurrentRule       => (Rules.Count > 0 && CurrentRuleIndex >= 0 && CurrentRuleIndex < Rules.Count) ? Rules[CurrentRuleIndex] : null;
        
        public void RegisterScenarioRules()
        {
            Step.OnPreStepExecute += async s =>
            {
                if (Rules.Count == 0 || CurrentRuleIndex < 0 || CurrentRuleIndex >= Rules.Count)
                    return;

                var    current            = CurrentRule;
                string rule_eventStepName = current.EventStepName;

                if (s.Name == rule_eventStepName)
                {
                  //Debug.Log($"#>#>#>#>#>#>#>#>#>#>#>#>#>#> TEST STEP: {s.Name}");
                    CurrentRuleIndex++;
                    
                    // Execute all actions in order
                    if (current.ActionStepPaths != null && current.ActionStepPaths.Count > 0)
                    {
                        foreach (var actionPath in current.ActionStepPaths)
                        {
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
        
        private Step FindStep(string stepName)
        {
            IEntity rootEntity = Root.Instance.Runtime;

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
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class Rule
    {
        public string EventStepName;
        public List<string> ActionStepPaths = new List<string>();
        
        public static Rule Parse(string str)
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

            return new Rule
            {
                EventStepName   = eventName,
                ActionStepPaths = actions
            };
        }
        
        public override string ToString()
        {
            var actions = ActionStepPaths != null && ActionStepPaths.Count > 0 ? string.Join(" and ", ActionStepPaths) : "";
            return $"Rule: On {EventStepName} Do {actions}";
        }
    }
}

