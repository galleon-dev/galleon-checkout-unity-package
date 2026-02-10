using System;
using UnityEngine;

namespace Galleon.Checkout.Foundation.Tests
{
    public class TestController : Entity
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Collection<TestScenario> Scenarios = new Collection<TestScenario>();
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public TestScenario SinglePurchaseHappyFlow 
        = 
            new TestScenario(name  : "single_purchase_happy_flow"
                            ,rules : new[]
                            {
                                 // Test case - add first card
                                 @$"on 'sample_app_start'                         do 'test_purchase_product_1'",
                                 @$"on 'on_panel_focus_PreselectionPanel'         do 'preselection_panel_test_checkout'",
                                 @$"on 'on_panel_focus_CheckoutPanel '            do 'checkout_panel_test_select_other_pm'",
                                 @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_new_card'",
                                 @$"on 'on_panel_focus_CreditCardInfoPanel'       do 'credit_card_info_panel_fill_test_card' and 'credit_card_info_panel_confirm'",
                                 @$"on 'on_panel_focus_SuccessPanel'              do 'fill_test_email' and 'test_click_send_receipt'",
                            });
        
        public TestScenario DoublePurchaseHappyFlow 
        = 
            new TestScenario(name  : "duble_purchase_happy_flow"
                            ,rules : new[]
                            {
                                 // Test case - add first card
                                 @$"on 'sample_app_start'                         do 'test_purchase_product_1'",
                                 @$"on 'on_panel_focus_PreselectionPanel'         do 'preselection_panel_test_checkout'",
                                 @$"on 'on_panel_focus_CheckoutPanel '            do 'checkout_panel_test_select_other_pm'",
                                 @$"on 'on_panel_focus_SelectPaymentMethodPanel'  do 'selection_panel_test_new_card'",
                                 @$"on 'on_panel_focus_CreditCardInfoPanel'       do 'credit_card_info_panel_fill_test_card' and 'credit_card_info_panel_confirm'",
                                 @$"on 'on_panel_focus_SuccessPanel'              do 'fill_test_email' and 'test_click_send_receipt'",
                                 
                                  // Test case - pay again
                                 @$"on 'on_back_to_store_screen'                  do 'test_purchase_product_2'",
                                 @$"on 'on_panel_focus_CheckoutPanel '            do 'checkout_panel_test_confirm_purchase'",
                                 @$"on 'on_panel_focus_SuccessPanel'              do 'success_panel_wait_and_do_nothing'",
                             
                            });

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step Test()
        =>
            new Step(name: $"Test", action: async (s) =>
            {
                this.Scenarios.Add(SinglePurchaseHappyFlow);
                
                foreach (var scenario in Scenarios)
                {
                    await scenario.ActivateScenario().Execute();
                }
            });

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step PrintHello()
        =>
            new Step(name: $"print_hello", action: async (s) =>
            {
                s.Log("Hello");
            });
    }
}

