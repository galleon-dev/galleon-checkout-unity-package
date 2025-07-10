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
            
            CHECKOUT.PaymentMethods.UserPaymentMethods.Add(new CreditCardUserUserPaymentMethod() { Type = UserPaymentMethod.PaymentMethodType.MasterCard.ToString(), DisplayName = "MasterCard - **** - 4587" , IsSelected = false, });
            CHECKOUT.PaymentMethods.UserPaymentMethods.Add(new() { Type = UserPaymentMethod.PaymentMethodType.GPay  .ToString(), DisplayName = "Google Pay - **** - 7348" , IsSelected = false, });
          //CHECKOUT.PaymentMethods.UserPaymentMethods.Add(new() { Type = UserPaymentMethod.PaymentMethodType.Apple .ToString(), DisplayName = "Apple Pay - **** - 5231"  , IsSelected = false, });
            CHECKOUT.PaymentMethods.UserPaymentMethods.Add(new() { Type = UserPaymentMethod.PaymentMethodType.PayPal.ToString(), DisplayName = "Paypal - **** - 9101"     , IsSelected = false, });
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        public UserPaymentMethod SelectedUserPaymentMethod => CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);
        
        public void SelectPaymentMethod(UserPaymentMethod userPaymentMethod)
        {
            foreach (var method in CHECKOUT.PaymentMethods.UserPaymentMethods)
                method.Unselect();
            
            userPaymentMethod.Select();
        }
        
        public void AddPaymentMethod(UserPaymentMethod userPaymentMethod)
        {
            CHECKOUT.PaymentMethods.UserPaymentMethods.Add(userPaymentMethod);
        }
        
        public void RemovePaymentMethod(UserPaymentMethod userPaymentMethod)
        {
            CHECKOUT.PaymentMethods.UserPaymentMethods.Remove(userPaymentMethod);
            
            foreach (var method in CHECKOUT.PaymentMethods.UserPaymentMethods)
                method.Unselect();
        }
    }
}
