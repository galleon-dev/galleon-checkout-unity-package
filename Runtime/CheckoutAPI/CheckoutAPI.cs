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
            
            await CheckoutClient.Instance.SystemInitFlow().Execute();
            
            return new InitializationResult() { IsSuccess = true };
        }
        
        public static async Task<PurchaseResult> Purchase(CheckoutProduct product, Dictionary<string, string> metadata = null, List<BonusItem> bonusData = null)
        {
            // Safty
            if (metadata  == null) metadata  = new Dictionary<string, string>();
            if (bonusData == null) bonusData = new List<BonusItem>();
            
            // Create and setup session
            await CheckoutClient.Instance.CreateCheckoutSession(product).Execute();
            CheckoutClient.Instance.CurrentSession.Metadata  = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            CheckoutClient.Instance.CurrentSession.BonusData = bonusData;
                   
            // Run Session
            await  CheckoutClient.Instance.RunCheckoutSession().Execute();
            
            // Return result
            return CheckoutClient.Instance.CurrentSession.PurchaseResult;
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
    
    [Serializable]
    public class CheckoutConfiguration
    {
        public string JWT;
        public string Country;
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

        public override string ToString()
        {
            return $"PurchaseResult: IsSuccess={IsSuccess}, IsCanceled={IsCanceled}, IsError={IsError}, DidUserSelectNativeIAP={DidUserSelectNativeIAP}, Errors={string.Join(", ", Errors ?? new List<string>())}";
        }
    }
}
