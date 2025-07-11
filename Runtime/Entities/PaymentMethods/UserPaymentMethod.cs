using System;
using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class UserPaymentMethod : Entity
    {
        //// Types
        
        public enum PaymentMethodType
        {
            Visa,
            MasterCard,
            Amex,
            Diners,
            Discover,
            GPay,
            PayPal,
        }
        
        //// Members
        
        public string Type; 
        public string DisplayName;
        public bool   IsSelected;
        
        public UserPaymentMethodData Data;
        
        //// UI Actions
        
        public void Select()
        {
            this.IsSelected = true;
        }
        
        public void Unselect()
        {
            this.IsSelected = false;
        }
        
        //// Transaction Steps
        
        public List<Func<Step>> InitializationSteps  = new ();
        public List<Func<Step>> VaultingSteps        = new ();
        public List<Func<Step>> TransactionSteps     = new ();
        public List<Func<Step>> PostTransactionSteps = new ();
        
    }
}
