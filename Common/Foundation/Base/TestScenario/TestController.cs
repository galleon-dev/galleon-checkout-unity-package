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
                            //
                            // Example:
                            // @$"on 'sample_app_start' do 'print_hello'",
                            //
                            // Test product 1
                            @$"on 'sample_app_start' do 'test_purchase_product_1'",
                            @$"on 'on_view_focus_CheckoutPanel ' do 'checkout_panel_test_confirm_purchase'",
                            @$"on 'on_view_focus_SuccessPanel' do 'test_close_checkout_screen_clicked'",
                            //
                            // Test Product 2
                            @$"on 'on_back_to_store_screen' do 'test_purchase_product_2'",
                            @$"on 'on_view_focus_CheckoutPanel ' do 'checkout_panel_test_confirm_purchase'",
                            @$"on 'on_view_focus_SuccessPanel' do 'test_close_checkout_screen_clicked'",
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
        public Rule             CurrentRule       => Rules.Count > 0 ? Rules[CurrentRuleIndex] : null;
        
        public void RegisterScenarioRules()
        {
            Step.OnPreStepExecute += async s =>
                                  {
                                      string rule_eventStepName  = CurrentRule.EventStepName;
                                      string rule_actionStepPath = CurrentRule.ActionStepPath;
                                      
                                      if (s.Name == CurrentRule.EventStepName)
                                      {
                                          // DynamicExpression.Evaluate<Step>(Root.Instance.Runtime, rule.ActionStepPath);
                                          var step = FindStep(CurrentRule.ActionStepPath);
                                          if (step != null)
                                          {
                                              await Task.Delay(1000);
                                              step.Execute();
                                              CurrentRuleIndex++;
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
                    int x = 4;
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
        public string ActionStepPath;
        
        public static Rule Parse(string str)
        {
            // on "bla" do "bla"
            
            var parts = str.Split(new[] { "on '", "' do '" }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length != 2 
            || !parts[1].EndsWith("'"))
                throw new FormatException("Input string is not in the correct format. Expected format: on 'EventStepName' do 'ActionStepName'");
            
            parts[1] = parts[1].TrimEnd('\'');
            
            return new Rule
            {
                EventStepName  = parts[0],
                ActionStepPath = parts[1]
            };
        }
        
        public override string ToString()
        {
            return $"Rule: On {EventStepName} Do {ActionStepPath}";
        }
    }
}



