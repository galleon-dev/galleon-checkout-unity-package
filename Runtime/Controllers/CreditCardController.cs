using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CreditCardController : Entity
    {
        public Step Initialize() 
        => 
            new Step(name   : "initialize_credit_cards_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        var tokenizerResponse = await CHECKOUT.Network.Get(url     : "https://localhost:4000/tokenizer"
                                                                          ,headers : new()
                                                                                   {
                                                                                      { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" },
                                                                                   }
                                                                           );
                        
                        /// Response Example :
                        /// {
                        ///   "Timestamp" : 1742385314,
                        ///   "Payload"   :
                        ///               {
                        ///                   "ServiceUrl"    : "https://api.basistheory.com/tokens",
                        ///                   "RequestFormat" : "{ type: "card", data: { "number": "<CC_NUMBER>", "expiration_month": <CC_MONTH>, "expiration_year": <CC_YEAR>, "cvc": "<CC_CVC>"  } }",
                        ///                   "Headers"       :
                        ///                                   {
                        ///                                       "Content-Type" : "application/json",
                        ///                                       "BT-API-KEY"   : "key_test_us_pvt_Vii1FVRcm9BVtiUjZQBsX.2cb2d1e874906289f09adb4640bdf3d9"
                        ///                                   }
                        ///               }
                        /// }


                        // var tokenizer = JsonConvert.DeserializeAnonymousType(value               : tokenizerResponse.ToString()
                        //                                                     ,anonymousTypeObject : new 
                        //                                                                          {
                        //                                                                              Timestamp = 0L,
                        //                                                                              Payload   = new
                        //                                                                              {
                        //                                                                                  ServiceUrl    = "",
                        //                                                                                  RequestFormat = "",
                        //                                                                                  Headers       = new Dictionary<string,string>()
                        //                                                                              }
                        //                                                                          });
                        
                        TokenizerData tokenizer = JsonConvert.DeserializeObject<TokenizerData>(tokenizerResponse.ToString());
                        this.Tokenizer = tokenizer;
                        
                        s.Log($"Retrieved tokenizer service URL: {tokenizer.Payload.ServiceUrl}");
                    });
        
        ///////////////////////
        
        public TokenizerData Tokenizer;
        
        public class TokenizerData
        {
            public long             Timestamp = 0L;
            public TokenizerPayload Payload   = new();
            
            public class TokenizerPayload
            {
                public string                     ServiceUrl      = "";
                public string                     RequestFormat   = "";
                public Dictionary<string, string> Headers         = new();
            }
        }
        
        ///////////////////////

        public Step Tokenize()
        =>
            new Step(name   : $"tokenize"
                    ,action : async (s) =>
                    {
                        var card = new 
                                   {
                                       Number = "4242424242424242",
                                       Month  = 12,
                                       Year   = 2026,
                                       Cvc    = "123"
                                   };

                        var cardTokenResponse = await CHECKOUT.Network.Post(url      : Tokenizer.Payload.ServiceUrl
                                                                           ,headers  : Tokenizer.Payload.Headers
                                                                           ,jsonBody : Tokenizer.Payload.RequestFormat
                                                                                                        .Replace("<CC_NUMBER>", card.Number)
                                                                                                        .Replace("<CC_MONTH>",  card.Month.ToString())
                                                                                                        .Replace("<CC_YEAR>",   card.Year.ToString())
                                                                                                        .Replace("<CC_CVC>",    card.Cvc)
                                                                           );

                        s.Log(cardTokenResponse);

                        /// Reponse Example :
                        /// {
                        ///     "id"             : "a7b05b2b-1f5f-48cb-a2ca-9a8ac69c0beb",
                        ///     "type"           : "card",
                        ///     "tenant_id"      : "f182f521-4bae-49df-b2eb-e812b8bc9931",
                        ///     "data"           : 
                        ///                      {
                        ///                          "number"           : "4242424242424242",
                        ///                          "expiration_month" : 12,
                        ///                          "expiration_year"  : 2026,
                        ///                          "cvc"              : "123"
                        ///                      },
                        ///     "created_by"     : "50109434-3085-40d7-9951-259a4bcbf86d",
                        ///     "created_at"     : "2025-03-19T12:41:36.0209919+00:00",
                        ///     "card"           : 
                        ///                      {
                        ///                          "bin"              : "42424242",
                        ///                          "last4"            : "4242",
                        ///                          "expiration_month" : 12,
                        ///                          "expiration_year"  : 2026,
                        ///                          "brand"            : "visa",
                        ///                          "funding"          : "credit",
                        ///                          "issuer_country"   : 
                        ///                                             {
                        ///                                                 "alpha2"  : "PL",
                        ///                                                 "name"    : "Bermuda",
                        ///                                                 "numeric" : "369"
                        ///                                             }
                        ///                      },
                        ///     "mask"           : 
                        ///                      {
                        ///                          "number"           : "{{ data.number | reveal_last: 4 }}",
                        ///                          "expiration_month" : "{{ data.expiration_month }}",
                        ///                          "expiration_year"  : "{{ data.expiration_year }}"
                        ///                      },
                        ///     "privacy"        : 
                        ///                      {
                        ///                          "classification"     : "pci",
                        ///                          "impact_level"       : "high",
                        ///                          "restriction_policy" : "mask"
                        ///                      },
                        ///     "search_indexes" : [],
                        ///     "containers"     : 
                        ///                      [
                        ///                          "/pci/high/"
                        ///                      ],
                        ///     "aliases"        : 
                        ///                      [
                        ///                          "a7b05b2b-1f5f-48cb-a2ca-9a8ac69c0beb"
                        ///                      ],
                        ///     "_extras"        : 
                        ///                      {
                        ///                          "deduplicated" : false
                        ///                      }
                        /// }

                        var    jsonObject = JsonConvert.DeserializeObject<JObject>(cardTokenResponse.ToString());
                        string tokenID    = jsonObject["id"]?.ToString();
                        this.TokenID      = tokenID;
                        s.Log($"Token ID: {tokenID}".Color(Color.green));
                    });

        ///////////////////////
        
        public string TokenID;
        
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var chargeResponse = await CHECKOUT.Network.Post(url    : $"https://localhost:4000/charge"
                                                                      ,headers  : new ()
                                                                                {
                                                                                    { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                }
                                                                      ,body     : new
                                                                                {
                                                                                    Sku      = "sku-1",
                                                                                    Quantity = 1,
                                                                                    Amount   = 100,
                                                                                    Currency = "USD",
                                                                                    Card     = new 
                                                                                             {
                                                                                                 Number   = TokenID,
                                                                                                 ExpMonth = TokenID,
                                                                                                 ExpYear  = TokenID,
                                                                                                 Cvc      = TokenID,
                                                                                             }
                                                                                });
                    });   
        
        
    }
}
