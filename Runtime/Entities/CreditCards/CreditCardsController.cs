using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CreditCardsController : Entity
    {
        public List<CreditCard> creditCards = new();
        
        public Step Initialize()
        => 
            new Step(name   : "initialize_credit_cards_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
    }
}
