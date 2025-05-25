using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class TransactionsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<Transaction> transactions;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize()
        => 
            new Step(name   : "initialize_transactions_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task<Transaction> CreateTestTransaction()
        {
            return new Transaction(user            : CHECKOUT.Users.Users.First()
                                  ,creditCardToken : CHECKOUT.Users.Users.First().Tokens.First());
        }
    }
}
