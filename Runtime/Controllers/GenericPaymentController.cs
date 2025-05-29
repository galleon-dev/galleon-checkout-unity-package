using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class GenericPaymentController : Entity
    {
        public Step Initialize() 
        => 
            new Step(name   : "initialize_generic_payment_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
    }
}
