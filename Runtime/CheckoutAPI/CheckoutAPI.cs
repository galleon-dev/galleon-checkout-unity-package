using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
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
        
        public static async Task<PurchaseResult> Purchase(CheckoutProduct               product
                                                         ,CheckoutPurchaseConfiguration configuration)
        {
            // Safty
            if (configuration.Metadata  == null) configuration.Metadata  = new Dictionary<string, string>();
            if (configuration.BonusData == null) configuration.BonusData = new List<BonusItem>();
            
            // apply config
            if (configuration.Config != null)
            {
                foreach (var kvp in configuration.Config)
                    CHECKOUT.Config.SetValue(kvp.Key, kvp.Value);
            }
            
            // Create and setup session
            await CheckoutClient.Instance.CreateCheckoutSession(product).Execute();
            var sessionID = CHECKOUT.Session.SessionID;
            var session   = CheckoutClient.Instance.CurrentSession;
            
            session.PurchaseConfiguration = configuration;
            
            session.Metadata              = configuration.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            session.BonusData             = configuration.BonusData;
            
            // Run Session
            await  CheckoutClient.Instance.RunCheckoutSession(session).Execute();

            return session.PurchaseResult;
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
    
    public enum Environment
    {
        Test,
        Prod,
    }
    
    [Serializable]
    public class CheckoutConfiguration
    {
        public string                     JWT;
        public string                     Country;
        public string                     Currency;
        public string                     AppUserID;
        public string                     ApplicationDisplayName;
        public CheckoutOrientation        UIPanelOrientation        = CheckoutOrientation.Auto;
        public Dictionary<string, object> Config                    = new();
        public string                     DeepLinkName              = "checkout";
        public Environment                Environment               = Environment.Test;

        public override string ToString()
        {
            return $"CheckoutConfiguration: JWT={JWT}, Country={Country}, Currency={Currency}, AppUserID={AppUserID}, ApplicationDisplayName={ApplicationDisplayName}, UIPanelOrientation={UIPanelOrientation}, Config={Config}, DeepLinkName={DeepLinkName}";
        }
    }
    
    [Serializable]
    public class CheckoutPurchaseConfiguration
    {
        public Dictionary<string, string> Metadata                    = null;
        public List<BonusItem>            BonusData                   = null;
        public Dictionary<string, object> Config                      = null;
        public List<string>               AllowedPaymentMethodTypes   = null;
        public List<string>               ExcludedPaymentMethodTypes  = null;
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
        public string                     OrderID;
        public Dictionary<string, string> price_metadata;
        public bool                       IsSuccess;
        public bool                       IsCanceled;
        public bool                       IsError;
        public List<string>               Errors;
        public bool                       DidUserSelectNativeIAP;
        public string                     SelectedPaymentMethodType = "none";

        public override string ToString()
        {
            var priceMetadataString = price_metadata != null ? string.Join(", ", price_metadata.Select(kvp => $"{kvp.Key}={kvp.Value}")) : "null";
            return $"PurchaseResult: OrderID={OrderID}, price_metadata={{{priceMetadataString}}}, IsSuccess={IsSuccess}, IsCanceled={IsCanceled}, IsError={IsError}, DidUserSelectNativeIAP={DidUserSelectNativeIAP}, SelectedPaymentMethodType={SelectedPaymentMethodType}, Errors={string.Join("\n", Errors ?? new List<string>())},\n";
        }
    }
}
