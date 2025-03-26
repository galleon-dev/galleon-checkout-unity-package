using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class BasisTheoryAPI 
    {
        ///////////////////////////////////////////////////////////////////////////////////// Members
        
        public static string BaseURL = "https://api.basistheory.com";
        public static string API_KEY = "key_test_us_pvt_XYYMD9r5tEGg2ntQx2g1LE.d0fa100949964f76210eba743ba441c1";
        
        ///////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize 
        => 
            new Step(name   : "initialize_basis_theory_api"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                    });
        
        ///////////////////////////////////////////////////////////////////////////////////// API
        
        public static async Task<CreditCardToken> CreateCreditCardToken()
        {
            try
            {
                var result = await CHECKOUT.Network.Post(url      : $"{BaseURL}/tokens"
                                                        ,headers  : new()
                                                                  {
                                                                      { "BT-API-KEY",   API_KEY            },
                                                                      { "Content-Type", "application/json" }
                                                                  }
                                                        ,body     : new
                                                                //{
                                                                //   Type     = "token",
                                                                //   Data     = "Sensitive Data",
                                                                //   Metadata = new
                                                                //            {
                                                                //                MyField = "My Value" 
                                                                //            }
                                                                //}
                                                                 {
                                                                     Type = "card",
                                                                     Data = new
                                                                     {
                                                                         number           = "4242424242424242",
                                                                         expiration_month = 12,
                                                                         expiration_year  = 2025,
                                                                         cvc              = "123"
                                                                     }
                                                                 }
                                                        );
                
                            
                // parse result
                var str = result.ToString(); 
                var id = (string)JObject.Parse(str)["id"];
                CreditCardToken creditCardToken = new CreditCardToken(id);
                
                return creditCardToken;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error Creting Token \n{e.Message}");
                return default;
            }
        }
    }
}
