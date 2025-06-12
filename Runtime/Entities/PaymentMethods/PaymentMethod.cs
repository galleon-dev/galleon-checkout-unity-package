using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class PaymentMethod : Entity
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
        
        public List<Step> VaultingSteps    = new List<Step>();
        public List<Step> TransactionSteps = new List<Step>();
    }
}
