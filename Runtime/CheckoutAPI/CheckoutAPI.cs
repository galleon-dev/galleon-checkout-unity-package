using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutAPI
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main API
        
        public static async Task<InitializationResult> Initialize(CheckoutConfiguration configuration)
        {
            await CheckoutClient.Instance.SystemInitFlow().Execute();
            return new InitializationResult() { IsSuccess = true };
        }
        
        public static async Task<PurchaseResult> Purchase(CheckoutProduct product)
        {
            await  CheckoutClient.Instance.RunCheckoutSession(product).Execute();
            return CheckoutClient.Instance.CurrentSession.PurchaseResult;
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
    
    public class CheckoutConfiguration
    {
        public string JWT;
        public string Country;
    }
    
    public class InitializationResult
    {
        public bool IsSuccess { get; set; }
    }
    
    public class PurchaseResult
    {
        public bool         IsSuccess              { get; set; }
        public bool         IsCanceled             { get; set; }
        public bool         IsError                { get; set; }
        public List<string> Errors                 { get; set; }
        public bool         DidUserSelectNativeIAP { get; set; }

        public override string ToString()
        {
            return $"PurchaseResult: IsSuccess={IsSuccess}, IsCanceled={IsCanceled}, IsError={IsError}, DidUserSelectNativeIAP={DidUserSelectNativeIAP}, Errors={string.Join(", ", Errors ?? new List<string>())}";
        }
    }
}
