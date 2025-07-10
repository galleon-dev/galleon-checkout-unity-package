using System;
using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Galleon.Checkout
{
    public class PaymentMethodDefinition : Entity
    {
        //// Consts
        
        public const string PAYMENT_METHOD_TYPE_CREDIT_CARD = "credit_card";
        public const string PAYMENT_METHOD_TYPE_PAYPAL      = "paypal";
        public const string PAYMENT_METHOD_TYPE_GOOGLE_PAY  = "google_pay";
        
        //// Members
        
        public string                             Type;
        public Shared.PaymentMethodDefinitionData Data;
        
        //// Properties
        
        public string DisplayName => Data?.name ?? Type.ToString();
        
        //// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
       
    }
}