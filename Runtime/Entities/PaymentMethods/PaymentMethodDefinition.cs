using System.Collections.Generic;

namespace Galleon.Checkout
{
    public class PaymentMethodDefinition
    {
        //     Visa,
        //     MasterCard,
        //     Amex,
        //     Diners,
        //     Discover,
        //     GPay,
        //     PayPal,
        
        //// Members
        
        public string Type; 
        
        //// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
    }
}