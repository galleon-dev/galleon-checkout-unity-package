using System;
using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Galleon.Checkout
{
    public class PaymentMethodDefinition : Entity
    {
        //// Members
        
        public string                             Type;
        public Shared.PaymentMethodDefinitionData Data;
        
        //// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
       
    }
}