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
            GPay,
            PayPal,
        }
        
        //// Members
        
        public string Type; 
        public string DisplayName;
        public bool   IsSelected;
        
        //// Actions
        
        public void Select()
        {
            this.IsSelected = true;
        }
        
        public void Unselect()
        {
            this.IsSelected = false;
        }
    }
}
