using System;
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
        
        public bool                                 IsGooglePayAvailableOnDevice = false;
                        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public UserPaymentMethod                    NativeStoreUserPaymentMethod      => UserPaymentMethods     .FirstOrDefault(x => x.Type == "native");
        public UserPaymentMethod                    AppUserPaymentMethod              => UserPaymentMethods     .FirstOrDefault(x => x.Type == "app");
        public UserPaymentMethod                    EmptyCreditCardPaymentMethod      => EmptyUserPaymentMethods.FirstOrDefault(x => x.Type == "card");
        
        
        
        public  List<UserPaymentMethod>             PreselectionUserPaymentMethods    => UserPaymentMethods.Where(x => x.Type == "native" || x.Type == "app").ToList();
        public  List<UserPaymentMethod>             SpecialUserPaymentMethods         => UserPaymentMethods.Where(x => x.Type == "native" || x.Type == "app").ToList();
        public  List<UserPaymentMethod>             EmptyUserPaymentMethods           => UserPaymentMethods.Where(x => x.Type != null && x.Type.Contains("empty")).ToList();
        public  List<UserPaymentMethod>             LastUsedUserPaymentMethods        => GetLastUsedUserPaymentMethods();
        
        public  List<UserPaymentMethod>             UserPaymentMethodsToDisplay       => GetUserPaymentMethodsToDisplay();
        
        public  List<UserPaymentMethod>             UserPaymentMethodsToSelect        =>  GetUserPaymentMethodToSelect();
        
        public  List<UserPaymentMethod>             UserPaymentMethodsToRemove        =>  UserPaymentMethods
                                                                                          .Distinct()
                                                                                          .Except(SpecialUserPaymentMethods)
                                                                                          .Except(EmptyUserPaymentMethods)
                                                                                          .OrderBy(x => x.SortOrder)
                                                                                          .ToList();
        
        public List<UserPaymentMethod>              RealPaymentMethods                 => UserPaymentMethods.Except(EmptyUserPaymentMethods).Except(SpecialUserPaymentMethods).ToList();
        public UserPaymentMethod                    LastUsedUserPaymentMethod          => RealPaymentMethods.FirstOrDefault();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        s.AddChildStep(CheckIfGooglePayIsAvailableOnDevice());
                        s.AddChildStep(RefreshPaymentMethods());                // Get from server    
                        s.AddChildStep(InitializeDefinitions());                // setup (e.g. download images)
                        s.AddChildStep(LoadLastUsedUserPaymentMethods());       // Load from storage
                    });
        
        public Step RefreshPaymentMethods()
        =>
            new Step(name   : "refresh_payment_methods"
                    ,action : async s =>
                    {
                        Load();
                        s.AddChildStep(GetPaymentMethodDefinitions());
                        s.AddChildStep(GetUserPaymentMethods());
                        // s.AddChildStep(TestPopulatePaymentMethodDefinitions());
                        // s.AddChildStep(TestPopulateUserPaymentMethods());
                    });

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public Step AddNewUserPaymentMethod(UserPaymentMethod upm) 
        =>
            new Step(name   : $"add_new_user_payment_method"
                    ,action : async (s) =>
                    {
                        s.AddPreStep(name     : "setup_adding_user_payment_method"
                                     ,action  : async step =>
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
        
        public async Task RemoveUserPaymentMethod(UserPaymentMethod userPaymentMethod)
        {
            UserPaymentMethods.Remove(userPaymentMethod);
            
            if (userPaymentMethod.Data.id != null)
            {
                var result = await CHECKOUT.Network.Post<RemovePaymentMethodResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/remove-payment-method" 
                                                                                     ,headers  : new ()
                                                                                               {
                                                                                                   { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                               }
                                                                                     ,body     : new RemovePaymentMethodRequest()
                                                                                               {
                                                                                                   payment_method_id = userPaymentMethod.Data.id,
                                                                                               });
                
            }
            
            foreach (var method in CHECKOUT.PaymentMethods.UserPaymentMethods)
                method.Unselect();

            if (CHECKOUT.PaymentMethods.UserPaymentMethods.Count != 0)
                CHECKOUT.PaymentMethods.UserPaymentMethods.First().Select();
            
            // if (UserPaymentMethodsToDisplay.Count < MAX_LAST_USED_PAYMENT_METHODS
            // &&  UserPaymentMethods.All(x => x.Type != "empty_card"))
            // {
            //     this.UserPaymentMethods.Add(new UserPaymentMethod()
            //                                 {
            //                                     Data               = new ()
            //                                                        {
            //                                                           type = "empty_card"
            //                                                        },
            //                                     DisplayName        = "Add Credit Card",
            //                                     IsNewPaymentMethod = false,
            //                                     IsSelected         = false,
            //                                     SortOrder          = float.PositiveInfinity, 
            //                                     Type               = "empty_card",
            //                                     ButtonText         = "Add Card"
            //                                 });
            // }
            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public async Task Save()
        {
            CHECKOUT.Storage.Write(key   : $"saved_payment_methods_{CHECKOUT.User.AppUserID}"
                                  ,value : LastUsedUserPaymentMethodIDs.Where(x => !x.StartsWith("local_pm_id")));

            foreach (var upm in UserPaymentMethods.Where(x => !x.ID.StartsWith("local_pm_id")))
            {
                upm.SaveData();
            }
            
        }
        
        public async Task Load()
        {
            this.LastUsedUserPaymentMethodIDs.Clear();

            List<string> saved = CHECKOUT.Storage.Read<List<string>>(key : $"saved_payment_methods_{CHECKOUT.User.AppUserID}");
            
            if (saved == null || saved.Count == 0)
                return;
            
            this.LastUsedUserPaymentMethodIDs.AddRange(saved.Where(x => !x.StartsWith("local_pm_id")));
            
            foreach (var upm in UserPaymentMethods.Where(x => !x.ID.StartsWith("local_pm_id")))
            {
                upm.LoadData();
            }
            
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
                        
                        usedPaymentMethod.LastSuccessfulUseTime = DateTime.Now;
                        
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
                        PaymentMethodsDefinitions.Clear();
                        
                        var country  = CHECKOUT.Globals.CheckoutInitConfiguration.Country;
                        var currency = CHECKOUT.Globals.CheckoutInitConfiguration.Currency;
                        
                        var _result = await CHECKOUT.Network.Get<Shared.PaymentMethodDefinitionsResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions?currency={currency}&country={country}&platform=unity"
                                                                                                         ,headers  : new ()
                                                                                                                   {
                                                                                                                       { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                                   }
                                                                                                          );
                        
                        
                        var dataList = _result.definitions;

                        foreach (var data in dataList)
                        {
                            if (data.type.ToLower().Contains("google_pay") && !IsGooglePayAvailableOnDevice)
                                continue;
                            
                            var definition = new PayPalPaymentMethodDefinition()
                                           {
                                               InitializationSteps = {},
                                               TransactionSteps    = {},
                                               Data                = data,
                                           };

                            this.PaymentMethodsDefinitions.Add(definition);
                            
                            if (definition.Type == "card")
                            {
                                definition.Data.display_empty_upm_in_checkout_page = false;
                            }
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
                        
                        UserPaymentMethods.Clear();
                        
                        var _result = await CHECKOUT.Network.Get<Shared.UserPaymentMethodsResponse>(url     : $"{CHECKOUT.Network.SERVER_BASE_URL}/user-payment-methods"
                                                                                                   ,headers : new ()
                                                                                                   {
                                                                                                       { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                   });
                        
                        var dataList = _result.payment_methods;
        
                        // if (dataList is not null
                        // &&  dataList.Length > 0)
                        // {
                        //     UserPaymentMethods.Clear();
                        // }
                        
                        foreach (var data in dataList)
                        {
                            // Apply whitelist/blacklist filtering
                            if (!ShouldIncludePaymentMethodType(data.type))
                                continue;

                            if (data.type == "credit_card")
                            {
                                Debug.Log($"got UMP type 'credit_card'. should be 'card'. this is server error. fixing locally.".Color(Color.red));
                                data.type = "card";
                            }
                            if (data.type == "card")
                            {
                                this.UserPaymentMethods.Add(new CreditCardUserUserPaymentMethod()
                                {
                                    DisplayName = data.display_name,
                                    Data        = data,
                                });
                            }
                            else
                            {
                                this.UserPaymentMethods.Add(new UserPaymentMethod()
                                {
                                    DisplayName = data.display_name,
                                    Data        = data,
                                });
                            }
                        }
                        
                        /////////////////////////////////// Empty

                        foreach (var definition in this.PaymentMethodsDefinitions)
                        {
                            if (definition.ShouldDisplayemptyUPMIncheckout)
                            {
                                var emptyUPM = definition.CreateEmptyPaymentMethod();
                                UserPaymentMethods.Add(emptyUPM);
                            }
                        }
                        
                        /////////////////////////////////// Empty Card
                        
                        if (UserPaymentMethods       .All(pm => pm.Data.type != "card")
                        &&  PaymentMethodsDefinitions.Any(pm => pm.Data.type == "card")
                        &&  UserPaymentMethods       .All(pm => pm.Type      != "empty_card"))
                        {
                            this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                        {
                                                            Data               = new ()
                                                                               {
                                                                                  type = "empty_card"
                                                                               },
                                                            DisplayName        = "Add Credit Card",
                                                            IsNewPaymentMethod = false,
                                                            IsSelected         = false,
                                                            SortOrder          = float.PositiveInfinity, 
                                                            Type               = "empty_card",
                                                            ButtonText         = "Add Card"
                                                        });
                        }
                        
                        if (UserPaymentMethods       .All(pm => pm.Type      != "paypal")
                        &&  PaymentMethodsDefinitions.Any(pm => pm.Data.type == "paypal")
                        &&  UserPaymentMethods       .All(pm => pm.Type      != "empty_paypal"))
                        {
                            this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                        {
                                                            Data               = new ()
                                                                               {
                                                                                  type = "empty_paypal"
                                                                               },
                                                            DisplayName        = "Add Paypal Account",
                                                            IsNewPaymentMethod = false,
                                                            IsSelected         = false,
                                                            SortOrder          = float.PositiveInfinity, 
                                                            Type               = "empty_paypal",
                                                            ButtonText         = "Add Paypal Account"
                                                        });
                        }
                        
                        
                        
                        /////////////////////////////////// Native
                        
                        if (CHECKOUT.Globals.IsNativeStoreEnabled
                        && !CHECKOUT.Globals.IsPreselectionEnabled)
                        {
                            string nativeDisplayName = "";
                            #if UNITY_ANDROID
                            nativeDisplayName = "Google Play";
                            #elif UNITY_IOS
                            nativeDisplayName = "Apple Pay";
                            #endif
                            this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                        {
                                                            Data                  = new()
                                                                                  {
                                                                                      type = "native"
                                                                                  },
                                                            DisplayName           = nativeDisplayName,
                                                            IsNewPaymentMethod    = false,
                                                            IsSelected            = false,
                                                            SortOrder             = float.PositiveInfinity,
                                                            Type                  = "native",
                                                            ButtonText            = "Continue with google play",
                                                            LastSuccessfulUseTime = DateTime.MinValue
                                                        });
                        }
                        
                        /////////////////////////////////// App
                        
                        if (CHECKOUT.Globals.IsNativeStoreEnabled)
                        {
                            this.UserPaymentMethods.Add(new UserPaymentMethod()
                                                        {
                                                            Data               = new ()
                                                                               {
                                                                                  type = "app"
                                                                               },
                                                            DisplayName        = CheckoutClient.Instance.ApplicationDisplayName ?? "Continue Checkout",
                                                            IsNewPaymentMethod = false,
                                                            IsSelected         = false,
                                                            SortOrder          = float.PositiveInfinity, 
                                                            Type               = "app"
                                                        });    
                        }
                        
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Collections
        
        public List<UserPaymentMethod> GetUserPaymentMethodsToDisplay()
        {
            Debug.Log($"GetUserPaymentMethodsToDisplay - 1 ({UserPaymentMethods.Count}) : \n{string.Join("\n", UserPaymentMethods.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            // Add empty
            if (UserPaymentMethods.All(pm => pm.Type != "card")
            &&  UserPaymentMethods.All(pm => pm.Type != "empty_card"))
            {
                var emptyCard = CreateEmptyCreditCardUserPaymentMethod();
                UserPaymentMethods.Add(emptyCard);
            }
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - 2 ({UserPaymentMethods.Count}) : \n{string.Join("\n", UserPaymentMethods.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            // Remove pending cards added temporeraly
            ClearPendingUserPaymentMethods();
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - 3 ({UserPaymentMethods.Count}) : \n{string.Join("\n", UserPaymentMethods.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            List<UserPaymentMethod> result;
            
            // Group
            result = UserPaymentMethods.OrderByDescending(x => x.LastSuccessfulUseTime)
                                       .GroupBy(x => x.Type).Select(x => x.First()).ToList();
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - g ({UserPaymentMethods.Count}) : \n{string.Join("\n", UserPaymentMethods.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            // filter
                                                                                        Debug.Log($"GetUserPaymentMethodsToDisplay - EmptyPaymentMethods ({EmptyUserPaymentMethods.Count}) : \n{string.Join("\n", EmptyUserPaymentMethods)}");
                                                                                        Debug.Log($"GetUserPaymentMethodsToDisplay - AppUserPaymentMethod : \n{AppUserPaymentMethod.Type} - {AppUserPaymentMethod.ID} - {AppUserPaymentMethod.DisplayName}");
            result = result.Concat(EmptyUserPaymentMethods).ToList();                   Debug.Log($"GetUserPaymentMethodsToDisplay - after Concat ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            result = result.Distinct().ToList();                                        Debug.Log($"GetUserPaymentMethodsToDisplay - after Distinct ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            result = result.Except(new[]{AppUserPaymentMethod}).ToList();               Debug.Log($"GetUserPaymentMethodsToDisplay - after Except ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            result = result.Take(MAX_LAST_USED_PAYMENT_METHODS -1).ToList();            Debug.Log($"GetUserPaymentMethodsToDisplay - after Take ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            result = result.OrderByDescending(x => x.LastSuccessfulUseTime).ToList();   Debug.Log($"GetUserPaymentMethodsToDisplay - 4 ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            // Remove Native ?
            if (result.Count >= MAX_LAST_USED_PAYMENT_METHODS
            || !CHECKOUT.Globals.IsNativeStoreEnabled
            || !CHECKOUT.Globals.IsNativeStoreEnabledInCheckoutPage
            ||  CHECKOUT.Globals.IsPreselectionEnabled)
            {
                result.Remove(NativeStoreUserPaymentMethod);
            }
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - 5 ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");

            // set last used time
            foreach (var userPaymentMethod in result)
            {
                if (userPaymentMethod.Type == "native")
                    userPaymentMethod.LastSuccessfulUseTime = DateTime.MinValue;
                
                if (userPaymentMethod.Type == "empty_card")
                    userPaymentMethod.LastSuccessfulUseTime = DateTime.MaxValue;
            }
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - 6 ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            // Apply filter
            for (int i = result.Count - 1; i >= 0; i--)
            {
                var pm = result.ElementAt(i);
                
                if (!ShouldIncludePaymentMethodType(pm.Type))
                {
                    Debug.Log($"removed upm {pm.DisplayName} = {pm.Type}");
                    result.Remove(pm);
                }
                else
                {
                    Debug.Log($"Allowed upm {pm.DisplayName} = {pm.Type} ");
                }
            }
            
            Debug.Log($"GetUserPaymentMethodsToDisplay - final ({result.Count}) : \n{string.Join("\n", result.Select(x => $"{x.Type}-({x.DisplayName})-{x.ID}"))}\n");
            
            return result;
        }
        
        public List<UserPaymentMethod> GetUserPaymentMethodToSelect()
        {
            var result = UserPaymentMethods
                .Distinct()
                .Except(new[]{AppUserPaymentMethod})
                .Except(EmptyUserPaymentMethods)
                .OrderBy(x => x.SortOrder)
                .ToList();

            if (result.Count >= MAX_LAST_USED_PAYMENT_METHODS
            || !CHECKOUT.Globals.IsNativeStoreEnabled
            || !CHECKOUT.Globals.IsNativeStoreEnabledInSelectionPage
            ||  CHECKOUT.Globals.IsPreselectionEnabled)
            {
                result.Remove(NativeStoreUserPaymentMethod);
            }
         
            
            // Apply filter
            result = result.Where(x => ShouldIncludePaymentMethodType(x.Type)).ToList();
                            
            
            return result;
        }
        
        public List<PaymentMethodDefinition> GetPaymentMethodDefinitionsToSelect()
        {
            return PaymentMethodsDefinitions.Where(x => ShouldIncludePaymentMethodType(x.Data.type)).ToList();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helpers

        private void ClearPendingUserPaymentMethods()
        {
            UserPaymentMethods.RemoveAll(x=>x.Node.Tags.Contains("pending"));
            
        }
        
        private bool ShouldIncludePaymentMethodType(string type)
        {   
            if (CHECKOUT.Session == null  || CHECKOUT.Session.PurchaseConfiguration == null)
                return true;
            
            var config = CHECKOUT.Session.PurchaseConfiguration;

            if (config == null || string.IsNullOrEmpty(type))
                return true;

            type = type.Replace("empty_", "");
            
            // If AllowedPaymentMethodTypes is set, only include types in the whitelist
            if (config.AllowedPaymentMethodTypes != null && config.AllowedPaymentMethodTypes.Count > 0)
            {
                return config.AllowedPaymentMethodTypes.Contains(type);
            }

            // If ExcludedPaymentMethodTypes is set, exclude types in the blacklist
            if (config.ExcludedPaymentMethodTypes != null && config.ExcludedPaymentMethodTypes.Count > 0)
            {
                return !config.ExcludedPaymentMethodTypes.Contains(type);
            }

            // If neither is set, include all types
            return true;
        }

        public UserPaymentMethod CreateEmptyCreditCardUserPaymentMethod()
        {
            var result = new UserPaymentMethod()
            {
                Data                  = new ()
                                      {
                                         type = "empty_card"
                                      },
                DisplayName           = "Add Credit Card",
                IsNewPaymentMethod    = false,
                IsSelected            = false,
                SortOrder             = float.PositiveInfinity, 
                Type                  = "empty_card",
                ButtonText            = "Add Card",
                LastSuccessfulUseTime = DateTime.MaxValue,
            };
            
            return result;
        }
        
        public List<UserPaymentMethod> GetLastUsedUserPaymentMethods()
        {
            // Return empty list if no payment methods were used
            if (this.LastUsedUserPaymentMethodIDs.Count == 0)
                return new List<UserPaymentMethod>();
            
            var                     lastUsedUpmID = this.LastUsedUserPaymentMethodIDs.Last();
            List<UserPaymentMethod> result        = new List<UserPaymentMethod>();

            // Process each saved payment method ID
            foreach (var id in  this.LastUsedUserPaymentMethodIDs)
            {
                // Try to find existing payment method
                var upm = this.UserPaymentMethods.Except(SpecialUserPaymentMethods).Except(EmptyUserPaymentMethods).FirstOrDefault(x => x.Data.id == id);
                if (upm != null)
                {
                    result.Add(UserPaymentMethods.FirstOrDefault(upm => upm.ID == lastUsedUpmID));
                }
                // Handle locally stored payment methods 
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

            // Fill remaining slots if we have fewer than maximum allowed
            if (result.Count < MAX_LAST_USED_PAYMENT_METHODS)
            {
                int diff = MAX_LAST_USED_PAYMENT_METHODS - result.Count;
                result.AddRange(UserPaymentMethods.Except(result).Take(diff));
            }
            
            return result.Take(MAX_LAST_USED_PAYMENT_METHODS).ToList();
        }
        
        public Step CheckIfGooglePayIsAvailableOnDevice() 
        =>
            new Step(name   : $"check_if_google_pay_is_available_on_device"
                    ,action : async (s) =>
                    {
                        ////////////////////////////////////////////////////////////
                        
                        #if UNITY_EDITOR
                        // Mock For Editor
                        IsGooglePayAvailableOnDevice = true;
                        s.Log($"IsGooglePayAvailableOnDevice: {IsGooglePayAvailableOnDevice}");
                        return;
                        #endif
                        
                        ////////////////////////////////////////////////////////////
                        
                        #if UNITY_ANDROID 
                      
                        using (AndroidJavaClass  unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        {
                            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                            {
                                using (AndroidJavaClass plugin = new AndroidJavaClass("com.example.checkoutgpaybridge.GooglePayBridge"))
                                {
                                    plugin.CallStatic("CheckGPayAvailable", activity);
                                    await Task.Delay(1000);
                                    IsGooglePayAvailableOnDevice = plugin.GetStatic<bool>("IsGPayAvailable");
                                }
                            }
                        }
                        
                        #endif
                        
                        s.Log($"IsGooglePayAvailableOnDevice: {IsGooglePayAvailableOnDevice}");
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Mock for testing
        
        public Step TestPopulatePaymentMethodDefinitions()
        =>
            new Step(name   : $"test_populate_payment_method_definitions"
                    ,action : async (s) =>
                    {   
                       this.PaymentMethodsDefinitions.Add(new CreditCardPaymentMethodDefinition()
                                                          {
                                                              Type             = "card",
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
                                                        Type        = "card",
                                                        DisplayName = "MasterCard - **** - 4587",
                                                        Data        = new()
                                                                    {
                                                                        type             = "card",
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

