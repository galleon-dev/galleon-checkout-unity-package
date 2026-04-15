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
        public string                Email => UserInfo.email;
 
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
        
    }
}
