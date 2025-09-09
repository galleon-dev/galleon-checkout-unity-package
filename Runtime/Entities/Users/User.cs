using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using UnityEngine;

namespace Galleon.Checkout
{
    public class User : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
       
        public string                Email = "";
 
        public List<CreditCardToken> Tokens                 = new();
        
        public List<Transaction>     Transactions           = new();
        
        public Transaction           CurrentTransaction;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CreditCardToken       MainToken    => Tokens.FirstOrDefault();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public User()
        {
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
        
        public async void RemovePaymentMethod(UserPaymentMethod userPaymentMethod)
        {
            CHECKOUT.PaymentMethods.UserPaymentMethods.Remove(userPaymentMethod);
            
            if (userPaymentMethod.Data.id != null)
            {
                var result = await CHECKOUT.Network.Post<RemovePaymentMethodResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/remove-payment-method" 
                                                                                     ,headers  : new ()
                                                                                               {
                                                                                                   { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                               }
                                                                                     ,body     : new RemovePaymentMethodRequest()
                                                                                               {
                                                                                                   payment_method_id = userPaymentMethod.Data.id,
                                                                                               }
                                                                                      );
                
            }
            
            foreach (var method in CHECKOUT.PaymentMethods.UserPaymentMethods)
                method.Unselect();
            
            CHECKOUT.PaymentMethods.UserPaymentMethods.First().Select();
        }
    }
}
