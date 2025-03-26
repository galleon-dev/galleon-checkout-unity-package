
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public class TokensController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<CreditCardToken> tokens = new();
        
        public BasisTheoryAPI BasisTheory = new BasisTheoryAPI();
        
        //////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize 
        => 
            new Step(name   : "initialize_credit_card_tokens_controller"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                    {
                        s.AddChildStep(BasisTheory.Initialize);
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Main 
        
        public async Task<CreditCardToken> CreateCreditCardToken()
        {
            var token = await BasisTheoryAPI.CreateCreditCardToken();

            this.tokens.Add(token);
            return token;
        }
    }
}
