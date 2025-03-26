using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutAPI
    {
        public static async Task PurchaseGooglePay()
        {
            
        }
        
        public static async Task PurchaseGalleon()
        {
            await CheckoutClient.Instance.TestUI.Execute();
        }
    }
}

