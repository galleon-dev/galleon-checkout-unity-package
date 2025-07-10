using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class PaymentMethodsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<PaymentMethodDefinition> PaymentMethodsDefinitions = new ();
        public Collection<UserPaymentMethod>       UserPaymentMethods        = new ();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        PaymentMethodsDefinitions.Node.DisplayName = "Payment Method Definitions";
                        UserPaymentMethods.Node.DisplayName        = "User Payment Methods";
                        
                        s.AddChildStep(TestPopulatePaymentMethodDefinitions());
                        s.AddChildStep(TestPopulatePaymentMethods());
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public Step TestPopulatePaymentMethodDefinitions()
        =>
            new Step(name   : $"test_populate_payment_method_definitions"
                    ,action : async (s) =>
                    {
                        
                        this.PaymentMethodsDefinitions.Add(new CreditCardPaymentMethodDefinition()
                                                           {
                                                               Type             = "credit_card",
                                                               VaultingSteps    = { "get_tokenizer", "tokenize" },
                                                               TransactionSteps = { "charge" },
                                                           });
                        
                        this.PaymentMethodsDefinitions.Add(new GooglePayPaymentMethodDefinition()
                                                           {
                                                               Type                = "google_pay",
                                                               InitializationSteps = { "check_availability" },
                                                               TransactionSteps    =
                                                                                   {
                                                                                        "create_order",
                                                                                        "open_url",
                                                                                   },
                                                           });
                        
                        this.PaymentMethodsDefinitions.Add(new PayPalPaymentMethodDefinition()
                                                           {
                                                               Type                = "paypal",
                                                               InitializationSteps = { "check_availability" },
                                                               TransactionSteps    =
                                                                                   {
                                                                                        "create_order",
                                                                                        "open_url",
                                                                                   },
                                                           });
                        
                    });
        
        public Step TestPopulatePaymentMethods()
        =>
            new Step(name   : $"test_populate_user_payment_method"
                    ,action : async (s) =>
                    {
                        
                        this.UserPaymentMethods.Add(new CreditCardUserUserPaymentMethod()
                                                   {
                                                       Type = "credit_card",
                                                   });
                        
                        this.UserPaymentMethods.Add(new GooglePayUserPaymentMethod()
                                                   {
                                                       Type = "paypal",
                                                   });
                        
                        this.UserPaymentMethods.Add(new PaypalUserUserPaymentMethod()
                                                   {
                                                       Type = "google_pay",
                                                   });
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Definitions
 
        public Step GetPaymentMethodDefinitions()
        =>
            new Step(name   : $"get_payment_method_definitions"
                    ,action : async (s) =>
                    {
                      //var _result = await CHECKOUT.Network.Get<Shared.PaymentMethodDefinitionsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions-test");
                        
                        List<Shared.PaymentMethodDefinitionData> dataList = new();

                        foreach (var data in dataList)
                        {
                            PaymentMethodDefinition pmd = new PaymentMethodDefinition();
                            pmd.Data = data;
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Payment Methods
        
        public Step GetUserPaymentMethods()
        =>
            new Step(name   : $"get_user_payment_methods"
                    ,action : async (s) =>
                    {
                      //var _result = await CHECKOUT.Network.Get<Shared.UserPaymentMethodsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/user-payment-method-test");
                        
                        List<UserPaymentMethodData> dataList = new();

                        foreach (var data in dataList)
                        {
                            UserPaymentMethod pm = new UserPaymentMethod();
                            pm.Data          = data;
                        }
                    });
        
    }
}

