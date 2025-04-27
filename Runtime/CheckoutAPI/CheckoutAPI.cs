using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutAPI
    {
        public static async Task PurchaseGalleon()
        {
            CheckoutProduct product = new CheckoutProduct()
                                      { 
                                          DisplayName = "fake_product_1",
                                          PriceText   = "$24.99",
                                      };
            
            await PurchaseGalleon(product);
        }
        
        public static async Task PurchaseGalleon(CheckoutProduct product)
        {
            await CheckoutClient.Instance.RunCheckoutSession(product);
        }
    }
}
