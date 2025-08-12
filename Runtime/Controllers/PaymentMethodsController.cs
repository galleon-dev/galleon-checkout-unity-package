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
                        
                        s.AddChildStep(GetPaymentMethodDefinitions());
                        s.AddChildStep(GetUserPaymentMethods());
                        
                        //s.AddChildStep(TestPopulatePaymentMethodDefinitions());
                        //s.AddChildStep(TestPopulatePaymentMethods());
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public Step TestPopulatePaymentMethodDefinitions()
        =>
            new Step(name   : $"test_populate_payment_method_definitions"
                    ,action : async (s) =>
                    {
                        
                      //this.PaymentMethodsDefinitions.Add(new CreditCardPaymentMethodDefinition()
                      //                                   {
                      //                                       Type             = "credit_card",
                      //                                       VaultingSteps    = { "get_tokenizer", "tokenize" },
                      //                                       TransactionSteps = { "charge" },
                      //                                   });
                        
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
                                                        Type        = UserPaymentMethod.PaymentMethodType.MasterCard.ToString(),
                                                        DisplayName = "MasterCard - **** - 4587",
                                                    });
                        
                        this.UserPaymentMethods.Add(new GooglePayUserPaymentMethod()
                                                    {
                                                        Type        = UserPaymentMethod.PaymentMethodType.PayPal.ToString(),
                                                        DisplayName = "PayPal - **** - 7348",
                                                    });
                        
                        this.UserPaymentMethods.Add(new PaypalUserUserPaymentMethod()
                                                    {
                                                        Type        = UserPaymentMethod.PaymentMethodType.GPay.ToString(),
                                                        DisplayName = "Google Pay - **** - 9101",
                                                    });
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Definitions
 
        public Step GetPaymentMethodDefinitions()
        =>
            new Step(name   : $"get_payment_method_definitions"
                    ,action : async (s) =>
                    {
                        var _result = await CHECKOUT.Network.Get<Shared.PaymentMethodDefinitionsResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions"
                                                                                                         ,headers  : new ()
                                                                                                         {
                                                                                                             { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                         });
                        
                        var dataList = _result.definitions;

                        foreach (var data in dataList)
                        {
                            PaymentMethodDefinition pmd = new PaymentMethodDefinition();
                            pmd.Data = data;
                            
                            this.PaymentMethodsDefinitions.Add(new PayPalPaymentMethodDefinition()
                                                           {
                                                               Type                = pmd.Type,
                                                               InitializationSteps = {},
                                                               TransactionSteps    =
                                                                                   {
                                                                                   },
                                                               Data                = data,
                                                           });
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Payment Methods
        
        public Step GetUserPaymentMethods()
        =>
            new Step(name   : $"get_user_payment_methods"
                    ,action : async (s) =>
                    {
                        var _result = await CHECKOUT.Network.Get<Shared.UserPaymentMethodsResponse>(url     : $"{CHECKOUT.Network.SERVER_BASE_URL}/user-payment-methods"
                                                                                                   ,headers : new ()
                                                                                                   {
                                                                                                       { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                   });
                        
                        var dataList = _result.payment_methods;

                        foreach (var data in dataList)
                        {
                            UserPaymentMethod pm = new UserPaymentMethod();
                            pm.Data          = data;
                            
                            this.UserPaymentMethods.Add(new CreditCardUserUserPaymentMethod()
                                                        {
                                                            Type        = pm.Data.credit_card_type ?? "credit card",
                                                            DisplayName = pm.Data.display_name,
                                                            Data        = data,
                                                        });
                        }
                        
                    });
        
    }
}

