using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Galleon.Checkout
{
    public class CreditCardPaymentMethod : PaymentMethod
    {
        //// Types
        
        public enum CREDIT_CARD_TYPE
        {
            MasterCard,
            Visa,
            Amex,
            Diners,
            Discover,
        }
        
        //// Members
        
        public string CreditCardType; 
        
        public string CardNumber;
        public string CardMonth;
        public string CardYear;
        public string CardCCV;
        public string CardHolderName;
        
        public string TokenID;
        
        //// Vaulting Steps
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        await CheckoutClient.Instance.TokenizerController.GetTokenizer();                        
                    });
        
        public Step Tokenize()
        =>
            new Step(name   : $"tokenize"
                    ,action : async (s) =>
                    {
                      //var card                = new 
                      //                          {
                      //                              Number = "4242424242424242",
                      //                              Month  = 12,
                      //                              Year   = 2026,
                      //                              Cvc    = "123"
                      //                          };
                        
                        var card                = new 
                                                  {
                                                      Number = CardNumber,
                                                      Month  = CardMonth,
                                                      Year   = CardYear,
                                                      Cvc    = CardCCV,
                                                  };
                        
                        
                        var Tokenizer           = CheckoutClient.Instance.TokenizerController.Tokenizer;

                        var cardTokenResponse   = await CHECKOUT.Network.Post(url      : Tokenizer.Payload.ServiceUrl
                                                                             ,headers  : Tokenizer.Payload.Headers
                                                                             ,jsonBody : Tokenizer.Payload.RequestFormat
                                                                                                          .Replace("<CC_NUMBER>", card.Number)
                                                                                                          .Replace("<CC_MONTH>",  card.Month.ToString())
                                                                                                          .Replace("<CC_YEAR>",   card.Year .ToString())
                                                                                                          .Replace("<CC_CVC>",    card.Cvc)
                                                                             );

                        s.Log(cardTokenResponse);

                        /// Reponse Example (basis theory) :
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
                    });

        
        
        //// Transaction Steps
        
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var chargeResponse = await CHECKOUT.Network.Post(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
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