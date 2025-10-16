using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Galleon.Checkout
{
    public class CheckoutActionsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vaulting Steps
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        await CheckoutClient.Instance.TokenizerController.GetTokenizer().Execute();                        
                    });
        
        public Step TokenizeCreditCard()
        =>
            new Step(name   : $"tokenize_credit_card"
                    ,action : async (s) =>
                    {
                        var creditCard = CHECKOUT.User.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;
                      
                        var card = new 
                        {
                            Number = creditCard.CardNumber,
                            Month  = creditCard.CardMonth,
                            Year   = creditCard.CardYear,
                            Cvc    = creditCard.CardCCV,
                        };
                      //var card = new 
                      //           {
                      //               Number = "4242424242424242",
                      //               Month  = 12,
                      //               Year   = 2026,
                      //               Cvc    = "123"
                      //           };
                        
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

                        var    jsonObject  = JsonConvert.DeserializeObject<JObject>(cardTokenResponse.ToString());
                        string tokenID     = jsonObject["id"]?.ToString();
                        
                        creditCard.TokenID = tokenID;
                    });
        
        
        public Step AddCreditCardPaymentMethod()
        =>
            new Step(name   : $"add_credit_card_payment_method"
                    ,action : async (s) =>
                    {
                        var creditCard = CHECKOUT.User.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;
                        
                        var result = await CHECKOUT.Network.Post<AddPaymentMethodResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/add-payment-method" 
                                                                                          ,headers  : new ()
                                                                                                    {
                                                                                                        { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                    }
                                                                                          ,body     : new AddPaymentMethodRequest()
                                                                                                    {
                                                                                                        payment_method_definition_type = "credit_card",
                                                                                                        credit_card_token              = creditCard.TokenID,
                                                                                                    }
                                                                                            );

                        s.Log(result.created_payment_method.id);     
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Transaction Steps
        
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var selectedUserPaymentMethod = CHECKOUT.User.SelectedUserPaymentMethod;
                        
                        var response = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                  ,headers  : new ()
                                                                                            {
                                                                                                { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                            }
                                                                                  ,body     : new Shared.ChargeRequest()
                                                                                            {
                                                                                                session_id              = CHECKOUT.Session.SessionID,
                                                                                                is_new_payment_method   = selectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                                payment_method          = new PaymentMethodDetails()
                                                                                                                        {
                                                                                                                             id   = selectedUserPaymentMethod.Data.id,
                                                                                                                             data = new ()
                                                                                                                                  {
                                                                                                                             //          { "token", ((CreditCardUserUserPaymentMethod)selectedUserPaymentMethod).TokenID }
                                                                                                                                  }
                                                                                                                        },
                                                                                                save_payment_method     = selectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                            });
                        
                        CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                {
                                                                                    errors      = new [] { "error" },
                                                                                    is_canceled = false,
                                                                                    is_success  = true,
                                                                                    charge_id   = "12345",
                                                                                };
                        
                        if (response.next_actions != null)
                        {
                            var        flow        = s.ParentStep;
                            List<Step> nextActions = new();

                            foreach (var step in nextActions)
                            {
                                flow.AddChildStep(step);
                            }
                        }
                        else
                        {
                            // NO TRANSACTION RESULT AND NO NEXT ACTION . ERROR .
                        }
                    });      
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Email Steps
        
        public Step SetEmail() 
        =>
            new Step(name   : $"set_email"
                    ,action : async (s) =>
                    {
                        var email     = CHECKOUT.Session.User.Email;
                        var sessionID = CHECKOUT.Session.SessionID;
                        
                        var response  = await CHECKOUT.Network.Post<UpdateEmailResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/update-email"
                                                                                        ,headers  : new ()
                                                                                                  {
                                                                                                      { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                  }
                                                                                        ,body     : new Shared.UpdateEmailRequest()
                                                                                                  {
                                                                                                      email      = email,
                                                                                                      session_id = sessionID,
                                                                                                  });
                    });
    }
}

