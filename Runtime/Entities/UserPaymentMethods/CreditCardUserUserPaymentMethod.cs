using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CreditCardUserUserPaymentMethod : UserPaymentMethod
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public string CreditCardType => Data?.credit_card_type ?? ""; 
        
        public string CardNumber;
        public string CardMonth;
        public string CardYear;
        public string CardCCV;
        public string CardHolderName;
        
        public string TokenID;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vaulting Steps
        
        public override Step RunVaultingSteps() 
        =>
            new Step(name   : $"run_vaulting_steps"
                    ,action : async (s) =>
                    {
                        s.AddChildStep(GetTokenizer());
                        s.AddChildStep(Tokenize());
                        s.AddChildStep(AddPaymentMethod());
                        
                    });
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        await CheckoutClient.Instance.TokenizerController.GetTokenizer().Execute();                        
                    });
        
        public Step Tokenize()
        =>
            new Step(name   : $"tokenize"
                    ,action : async (s) =>
                    {
                      //var card = new 
                      //           {
                      //               Number = "4242424242424242",
                      //               Month  = 12,
                      //               Year   = 2026,
                      //               Cvc    = "123"
                      //           };
                      
                        var card = new 
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
                                                                                                          .Replace("<CC_MONTH>",  $@"""{card.Month}""")
                                                                                                          .Replace("<CC_YEAR>",   $@"""20{card.Year}""")
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

        
        public Step AddPaymentMethod()
        =>
            new Step(name   : $"add_payment_method"
                    ,action : async (s) =>
                    {
                        var result = await CHECKOUT.Network.Post<AddPaymentMethodResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/add-payment-method" 
                                                                                          ,headers  : new ()
                                                                                                    {
                                                                                                        { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                    }
                                                                                          ,body     : new AddPaymentMethodRequest()
                                                                                                    {
                                                                                                        payment_method_definition_type = "card",
                                                                                                        credit_card_token              = this.TokenID,
                                                                                                    }
                                                                                            );

                        s.Log(result.created_payment_method.id);     
                    });
        
    }
}
