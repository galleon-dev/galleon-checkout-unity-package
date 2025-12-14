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
       
        public string                AppUserID;
        public Shared.UserInfo       UserInfo;
        public string Email => UserInfo.email;
 
        public List<Transaction>     Transactions           = new();
        public Transaction           CurrentTransaction;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public User()
        {
        }

        public Step Initialize()
        => 
            new Step(name   : "initialize_user"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        s.AddChildStep(CHECKOUT.Actions.GetUserInfo());
                    });
        
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
            await CHECKOUT.PaymentMethods.RemoveUserPaymentMethod(userPaymentMethod);
        }
    }
}
