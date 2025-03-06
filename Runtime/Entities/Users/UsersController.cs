using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class UsersController
    {
        public List<User> Users = new();
        
        public Step Initialize => new Step(name   : "initialize_users_controller"
                                          ,tags   : new[] { "init"}
                                          ,action : async s =>
                                          {
                                              
                                          });
    }
}
