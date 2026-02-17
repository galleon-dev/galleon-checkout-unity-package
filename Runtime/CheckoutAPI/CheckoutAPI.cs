using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutAPI
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main API
        
        public static async Task<InitializationResult> Initialize(CheckoutConfiguration configuration)
        {
            CheckoutClient.Instance.Network.GalleonUserAccessToken = configuration.JWT;
            CHECKOUT.User.AppUserID                                = configuration.AppUserID;
            CheckoutClient.Instance.ApplicationDisplayName         = configuration.ApplicationDisplayName;
            CHECKOUT.Globals.CheckoutInitConfiguration             = configuration;
            
            await CheckoutClient.Instance.SystemInitFlow().Execute();
            
            return new InitializationResult() { IsSuccess = true };
        }
        
        public static async Task<PurchaseResult> Purchase(CheckoutProduct            product
                                                         ,Dictionary<string, string> metadata      = null
                                                         ,List<BonusItem>            bonusData     = null
                                                         ,Dictionary<string, object> config        = null)
        {
            // Safty
            if (metadata  == null) metadata  = new Dictionary<string, string>();
            if (bonusData == null) bonusData = new List<BonusItem>();
            
            // apply config
            if (config != null)
            {
                foreach (var kvp in config)
                    CHECKOUT.Config.SetValue(kvp.Key, kvp.Value);
            }
            
            // Create and setup session
            await CheckoutClient.Instance.CreateCheckoutSession(product).Execute();
            
            CheckoutClient.Instance.CurrentSession.Metadata              = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            CheckoutClient.Instance.CurrentSession.BonusData             = bonusData;
            CheckoutClient.Instance.CurrentSession.PurchaseConfiguration = config;
            
            // Run Session
            await  CheckoutClient.Instance.RunCheckoutSession().Execute();
            
            // Return result
            return CheckoutClient.Instance.CurrentSession.PurchaseResult;
        }
    
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Analytics
        
        public static event Action<CheckoutAnalyticsEvent> OnCheckoutAnalyticsEvent;
        
        internal static void InvokeAnalyticsEvent(string name, Dictionary<string, object> data)
        {
            InvokeAnalyticsEvent(new CheckoutAnalyticsEvent(name, data));
        }
        internal static void InvokeAnalyticsEvent(CheckoutAnalyticsEvent @event)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"<color=orange>[Checkout-Analytics]</color>: {@event.Name}, \n{string.Join("\n", @event.Data?.Select(kvp => $"{kvp.Key} = {kvp.Value}") ?? Array.Empty<string>())}");
            }
    
            OnCheckoutAnalyticsEvent?.Invoke(@event);
            
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
    
    [Serializable]
    public class CheckoutConfiguration
    {
        public string                     JWT;
        public string                     Country;
        public string                     AppUserID;
        public string                     ApplicationDisplayName;
        public CheckoutOrientation        UIPanelOrientation        = CheckoutOrientation.Auto;
        public Dictionary<string, object> Config                    = new();
        public string                     DeepLinkName              = "checkout";
    }
    
    public enum CheckoutOrientation
    {
        Auto,
        ForcePortrait,
        ForceLandscape,
    }
    
    [Serializable]
    public class InitializationResult
    {
        public bool IsSuccess;
    }
    
    [Serializable]
    public class PurchaseResult
    {
        public string       OrderID;
        public bool         IsSuccess;
        public bool         IsCanceled;
        public bool         IsError;
        public List<string> Errors;
        public bool         DidUserSelectNativeIAP;
        public string       SelectedPaymentMethodType = "none";

        public override string ToString()
        {
            return $"PurchaseResult: OrderID={OrderID}, IsSuccess={IsSuccess}, IsCanceled={IsCanceled}, IsError={IsError}, DidUserSelectNativeIAP={DidUserSelectNativeIAP}, SelectedPaymentMethodType={SelectedPaymentMethodType}, Errors={string.Join(", ", Errors ?? new List<string>())}";
        }
    }
}
