using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedInputFieldSamples;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class PaymentMethodsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public int                                  MAX_LAST_USED_PAYMENT_METHODS = 3;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<PaymentMethodDefinition>  PaymentMethodsDefinitions    = new ();
        public Collection<UserPaymentMethod>        UserPaymentMethods           = new ();
        
        public List<string>                         LastUsedUserPaymentMethodIDs = new ();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public  List<UserPaymentMethod>             SpecialUserPaymentMethods    => UserPaymentMethods.Where(x => x.Type == "native" || x.Type == "app").ToList();
        public  List<UserPaymentMethod>             LastUsedUserPaymentMethods   => GetLastUsedUserPaymentMethods();
        
        public  List<UserPaymentMethod>             UserPaymentMethodsToDisplay  => LastUsedUserPaymentMethods
                                                                                    .Concat(SpecialUserPaymentMethods)
                                                                                    .Distinct()
                                                                                    .OrderBy(x => x.SortOrder)
                                                                                    .Take(MAX_LAST_USED_PAYMENT_METHODS)
                                                                                    .Where(x => x?.Type != "app")
                                                                                    .ToList();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        PaymentMethodsDefinitions.Node.DisplayName = "Payment Method Definitions";
                        UserPaymentMethods       .Node.DisplayName = "User Payment Methods";
                        
                        s.AddChildStep(GetPaymentMethodDefinitions());
                        s.AddChildStep(GetUserPaymentMethods());
                        
                        // s.AddChildStep(TestPopulatePaymentMethodDefinitions());
                        // s.AddChildStep(TestPopulateUserPaymentMethods());

                        s.AddChildStep(InitializeDefinitions());
                        s.AddChildStep(LoadLastUsedUserPaymentMethods());
                    });

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public Step AddNewUserPaymentMethod(UserPaymentMethod upm) 
        =>
            new Step(name   : $"add_new_user_payment_method"
                    ,action : async (s) =>
                    {
                        s.AddPreStep(name : "setup_adding_user_payment_method"
                                     ,action: async step =>
                                              {
                                                  CheckoutClient.Instance.CurrentSession.User.AddPaymentMethod   (upm);
                                                  CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(upm);
                                              });
                        foreach (var vaultingStep in upm.GetVaultingSteps())
                            s.AddChildStep(vaultingStep);
                    });
        
        public void SelectPaymentMethodDefinition(PaymentMethodDefinition definition)
        {
            var upm = definition.CreateLocalUserPaymentMethod();
            UserPaymentMethods.Add(upm);
            upm.Node.Tags.Add("local");
            upm.Data.id = $"local_pm_id_{upm.Type}";
            
            upm.IsNewPaymentMethod      = true;
            upm.ShouldSavePaymentMethod = true;
            
            CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(upm);            
        }
        
        public void SelectUserPaymentMethod(UserPaymentMethod upm)
        {
            CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(upm);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public async Task Save()
        {
            CHECKOUT.Storage.Write(key   : "saved_payment_methods"
                                  ,value : LastUsedUserPaymentMethodIDs.Where(x => !x.StartsWith("local_pm_id")));
        }
        
        public async Task Load()
        {
            this.LastUsedUserPaymentMethodIDs.Clear();
            var saved = CHECKOUT.Storage.Read<List<string>>(key : "saved_payment_methods");
            this.LastUsedUserPaymentMethodIDs.AddRange(saved.Where(x => !x.StartsWith("local_pm_id")));
        }
        
        public async Task ClearSavedData()
        {
            CHECKOUT.Storage.ClearAll();
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
                        
                        Save();
                        
                    });
        
        public Step LoadLastUsedUserPaymentMethods() 
        =>
            new Step(name   : $"load_last_used_user_payment_methods"
                    ,action : async (s) =>
                    {
                        this.GetLastUsedUserPaymentMethods();
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
                                                               TransactionSteps    = {},
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
                        /////////////////////////////////// from server
                        
                        var _result = await CHECKOUT.Network.Get<Shared.UserPaymentMethodsResponse>(url     : $"{CHECKOUT.Network.SERVER_BASE_URL}/user-payment-methods"
                                                                                                   ,headers : new ()
                                                                                                   {
                                                                                                       { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                   });
                        
                        var dataList = _result.payment_methods;
        
                        if (dataList is not null
                        &&  dataList.Length > 0)
                        {
                            UserPaymentMethods.Clear();
                        }
                        
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
                        
                        /////////////////////////////////// Native
                        
                        string nativeDisplayName = "";
                        #if UNITY_ANDROID
                        nativeDisplayName = "Google Play";
                        #elif UNITY_IOS
                        nativeDisplayName = "Apple Pay";
                        #endif
                        this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                    {
                                                        Data               = new()
                                                                           {
                                                                              type = "native"
                                                                           },
                                                        DisplayName        = nativeDisplayName,
                                                        IsNewPaymentMethod = false,
                                                        IsSelected         = false,
                                                        Type               = "native"
                                                    });
                        
                        /////////////////////////////////// App
                        
                        this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                    {
                                                        Data               = new ()
                                                                           {
                                                                              type = "app"
                                                                           },
                                                        DisplayName        = CheckoutClient.Instance.ApplicationDisplayName ?? "Continue Checkout",
                                                        IsNewPaymentMethod = false,
                                                        IsSelected         = false,
                                                        Type               = "app"
                                                    });
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helpers
        
        public List<UserPaymentMethod> GetLastUsedUserPaymentMethods()
        {
            Load();
            
            if (this.LastUsedUserPaymentMethodIDs.Count == 0)
                return new List<UserPaymentMethod>();
            
            var lastUsedUpmID = this.LastUsedUserPaymentMethodIDs.Last();
            List<UserPaymentMethod> result = new List<UserPaymentMethod>();
            
            foreach (var id in  this.LastUsedUserPaymentMethodIDs)
            {
                var upm = this.UserPaymentMethods.Except(SpecialUserPaymentMethods).FirstOrDefault(x => x.Data.id == id);
                if (upm != null)
                {
                    result.Add(UserPaymentMethods.FirstOrDefault(upm => upm.ID == lastUsedUpmID));
                }
                else if (id.StartsWith("local_pm_id"))
                {
                    var definition = this.PaymentMethodsDefinitions.FirstOrDefault(x => x.LocalID == id);
                    if (definition is null) continue;
                    upm = definition.CreateLocalUserPaymentMethod();
                    
                    this.UserPaymentMethods.Add(upm);
                    if (upm.ID == lastUsedUpmID)
                        upm.Select();
                    
                    result.Add(upm);
                }
            }
            
            if (result.Count < MAX_LAST_USED_PAYMENT_METHODS)
            {
                int diff = MAX_LAST_USED_PAYMENT_METHODS - result.Count;
                result.AddRange(UserPaymentMethods.Except(result).Take(diff));
            }
            
            return result.Take(MAX_LAST_USED_PAYMENT_METHODS).ToList();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Mock for testing
        
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
                                                        Type        = "credit_card",
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
                                                        Type        = "paypal",
                                                        DisplayName = "PayPal - **** - 7348",
                                                    });
                        
                        this.UserPaymentMethods.Add(new PaypalUserUserPaymentMethod()
                                                    {
                                                        Type        = "google_pay",
                                                        DisplayName = "Google Pay - **** - 9101",
                                                    });
                    });
        
    }
}

