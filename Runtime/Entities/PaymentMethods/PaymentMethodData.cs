using System;

namespace Galleon.Checkout
{
    [Serializable]
    public class PaymentMethodData
    {
        public string payment_method_type { get; set; } // "credit_card" , "paypal" , "google_pay"
        public string data_type           { get; set; } // "definition_data", "user_data"
        public object data                { get; set; } 
    }
}