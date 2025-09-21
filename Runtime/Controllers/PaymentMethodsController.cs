using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class PaymentMethodsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public const int MAX_LAST_USED_PAYMENT_METHODS = 3;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<PaymentMethod>           PaymentMethods               = new ();
        
        public Collection<PaymentMethodDefinition> PaymentMethodsDefinitions    = new ();
        public Collection<UserPaymentMethod>       UserPaymentMethods           = new ();
        
        public List<string>                        LastUsedUserPaymentMethodIDs = new ();
        
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
                        
                        // s.AddChildStep(TestPopulatePaymentMethodDefinitions());
                        // s.AddChildStep(TestPopulateUserPaymentMethods());

                        s.AddChildStep(InitializeDefinitions());
                        
                        s.AddChildStep(LoadLastUsedUserPaymentMethods());
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
        
        public Step TestPopulateUserPaymentMethods()
        =>
            new Step(name   : $"test_populate_user_payment_method"
                    ,action : async (s) =>
                    {
                        this.UserPaymentMethods.Add(new CreditCardUserUserPaymentMethod()
                                                    {
                                                        Type        = UserPaymentMethod.PaymentMethodType.MasterCard.ToString(),
                                                        DisplayName = "MasterCard - **** - 4587",
                                                        Data        = new()
                                                                    {
                                                                        type             = "credit_card",
                                                                        credit_card_type = "mastercard",
                                                                        display_name     = "MasterCard - **** - 4587",
                                                                        id               = "master_card",
                                                                    }
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
        
        public Step TestPopulateUnifiedPaymentMethods() 
        =>
            new Step(name   : $"test_populate_unified_payment_methods"
                    ,action : async (s) =>
                    {
                        this.PaymentMethods.Add(new PaymentMethod(new PaymentMethodData()
                                                    {
                                                        payment_method_type = "credit_card",
                                                        data_type           = "definition_data",
                                                        data                = new CreditCardPaymentMethodDefinitionData()
                                                                            {
                                                                                type                 = "credit_card",
                                                                                icon_url             = "123",
                                                                                logo_url             = "123",
                                                                                supported_card_types = new []{ "visa", "mastercard" },
                                                                            }
                                                    })
                                                );
                        this.PaymentMethods.Add(new PaymentMethod(new PaymentMethodData()
                                                    {
                                                        payment_method_type = "credit_card",
                                                        data_type           = "user_data",
                                                        data                = new UserPaymentMethodData()
                                                                            {
                                                                                type             = "credit_card",
                                                                                id               = "123",
                                                                                credit_card_type = "master_card",
                                                                                display_name     = "MasterCard - **** - 4587",
                                                                            }
                                                    })
                                                );
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public async Task Save()
        {
            var selectedPaymentMethodIDs = this.UserPaymentMethods.Take(3).Select(x => x.Data.id).ToList();
            
            CHECKOUT.Storage.Write(key   : "saved_payment_methods"
                                  ,value : selectedPaymentMethodIDs);
        }
        
        public async Task Load()
        {
           var savedPaymentMethods = CHECKOUT.Storage.Read<List<string>>(key : "saved_payment_methods");

           foreach (var upm in CHECKOUT.PaymentMethods.UserPaymentMethods)
           {
               if (upm.Data.id != null && savedPaymentMethods.Contains(upm.Data.id))
               {
                   // mark as featured 3
               }
           }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Last used
        
        public Step SaveUsedUserPaymentMethod() 
        =>
            new Step(name   : $"save_used_user_payment_method"
                    ,action : async (s) =>
                    {
                        var usedPaymentMethod = this.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);
                        
                        this.LastUsedUserPaymentMethodIDs.Add(usedPaymentMethod.Data.id);
            
                        if (this.LastUsedUserPaymentMethodIDs.Count > MAX_LAST_USED_PAYMENT_METHODS)
                            this.LastUsedUserPaymentMethodIDs.RemoveAt(0);
                        
                        bool forceGooglePay = true;
                        if (forceGooglePay)
                        {
                            var googlePayUpmID = this.UserPaymentMethods.FirstOrDefault(x => x.Type == "google_pay")?.Data.id;
                            if (googlePayUpmID == null)
                                return;
                            
                            if (!this.LastUsedUserPaymentMethodIDs.Contains(googlePayUpmID))
                            {
                                this.LastUsedUserPaymentMethodIDs.RemoveAt(0);
                                this.LastUsedUserPaymentMethodIDs.Add(googlePayUpmID);
                            }
                        }
                    });
        
        public Step LoadLastUsedUserPaymentMethods() 
        =>
            new Step(name   : $"load_last_used_user_payment_methods"
                    ,action : async (s) =>
                    {
                        this.Load();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Definitions
        
        public Step GetPaymentMethodDefinitions()
        =>
            new Step(name   : $"get_payment_method_definitions"
                    ,action : async (s) =>
                    {
                        var _result = await CHECKOUT.Network.Get<Shared.PaymentMethodDefinitionsResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions?currency=USD&country=US"
                                                                                                         ,headers  : new ()
                                                                                                                   {
                                                                                                                       { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                                   }
                                                                                                          );
                        
                        
                        var dataList = _result.definitions;

                        foreach (var data in dataList)
                        {
                            PaymentMethodDefinition pmd = new PaymentMethodDefinition();
                            pmd.Data                    = data;
                            
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
        
        public Step InitializeDefinitions() 
        =>
            new Step(name   : $"initialize_definitions"
                    ,action : async (s) =>
                    {
                        foreach (var definition in PaymentMethodsDefinitions)
                            s.AddChildStep(definition.Initialize());
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
                            pm.Data              = data;
                            pm.Data.type         = "credit_card";
                            
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

