using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutProduct
    {
        public string DisplayName = "my product";
        public string PriceText   = "$24.90";
        
        public string  Sku;
        public decimal Amount;
        public string  Currency;
    }
}