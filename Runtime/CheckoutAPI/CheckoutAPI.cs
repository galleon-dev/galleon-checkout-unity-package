using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutAPI
    {   
        public static async Task<InitializationResult> Initialize(string jwt)
        {
            await CheckoutClient.Instance.SystemInitFlow().Execute();
            return new InitializationResult() { IsSuccess = true };
        }
        
        public static async Task<PurchaseResult> Purchase(CheckoutProduct product, Dictionary<string, string> metadata = null)
        {
            if (metadata == null)
                metadata = new Dictionary<string, string>();
            
            await  CheckoutClient.Instance.CreateCheckoutSession(product).Execute();
            CheckoutClient.Instance.CurrentSession.Metadata = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            await  CheckoutClient.Instance.RunCheckoutSession().Execute();
            return CheckoutClient.Instance.CurrentSession.PurchaseResult;
        }
    }
    
    public class InitializationResult
    {
        public bool IsSuccess { get; set; }
    }
    
    public class PurchaseResult
    {
        public string       OrderID    { get; set; }
        public bool         IsSuccess  { get; set; }
        public bool         IsCanceled { get; set; }
        public bool         IsError    { get; set; }
        public List<string> Errors     { get; set; }

        public override string ToString()
        {
            return $"PurchaseResult: OrderID={OrderID} IsSuccess={IsSuccess}, IsCanceled={IsCanceled}, IsError={IsError}, Errors={string.Join(", ", Errors ?? new List<string>())}";
        }
    }
}
