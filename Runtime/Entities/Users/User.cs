using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class User : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
       
        public string                ID;
        public string                Name;
        
        public List<PaymentMethod>   PaymentMethods         = new();
        
        public List<CreditCardToken> Tokens                 = new();
        
        public List<Transaction>     Transactions           = new();
        
        public Transaction           CurrentTransaction;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CreditCardToken       MainToken    => Tokens.FirstOrDefault();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public User()
        {
            this.ID   = new Guid().ToString();
            this.Name = "Fake User";
            
            this.PaymentMethods.Add(new() { Type = PaymentMethod.PaymentMethodType.MasterCard.ToString(), DisplayName = "MasterCard - **** - 4587" , IsSelected = false, });
            this.PaymentMethods.Add(new() { Type = PaymentMethod.PaymentMethodType.GPay      .ToString(), DisplayName = "Google Pay - **** - 7348" , IsSelected = false, });
            this.PaymentMethods.Add(new() { Type = PaymentMethod.PaymentMethodType.PayPal    .ToString(), DisplayName = "Paypal - **** - 9101" ,     IsSelected = false, });
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        public PaymentMethod SelectedPaymentMethod => PaymentMethods.FirstOrDefault(x => x.IsSelected);
        
        public void SelectPaymentMethod(PaymentMethod paymentMethod)
        {
            foreach (var method in this.PaymentMethods)
                method.Unselect();
            
            paymentMethod.Select();
        }
        
        public void AddPaymentMethod(PaymentMethod paymentMethod)
        {
            this.PaymentMethods.Add(paymentMethod);
        }
        
        public void RemovePaymentMethod(PaymentMethod paymentMethod)
        {
            this.PaymentMethods.Remove(paymentMethod);
            
            foreach (var method in this.PaymentMethods)
                method.Unselect();
        }
    }
}
