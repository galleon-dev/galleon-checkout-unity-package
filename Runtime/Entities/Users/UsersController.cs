using System.Collections.Generic;
using Galleon.Checkout.Foundation;
using UnityEngine;

namespace Galleon.Checkout
{
    public class UsersController : Entity
    {
        public Collection<User> Users = new();
        
        public Step Initialize()
        => 
            new Step(name   : "initialize_users_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        CheckoutClient.Instance.CurrentUser = new User();
                        CheckoutClient.Instance.Users.Users.Add(CheckoutClient.Instance.CurrentUser);
                        
                        s.AddChildStep(CheckoutClient.Instance.CurrentUser.Initialize());
                    });
    }
}
