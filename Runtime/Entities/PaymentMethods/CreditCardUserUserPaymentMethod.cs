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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types
        
        public enum CREDIT_CARD_TYPE
        {
            MasterCard,
            Visa,
            Amex,
            Diners,
            Discover,
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public string CreditCardType; 
        
        public string CardNumber;
        public string CardMonth;
        public string CardYear;
        public string CardCCV;
        public string CardHolderName;
        
        public string TokenID;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization

        public CreditCardUserUserPaymentMethod()
        {
            this.TransactionSteps = new()
                                    {
                                        Charge,
                                      //AwaitSocket
                                    };
        }
        
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
                                                                                                        payment_method_definition_type = "credit_card",
                                                                                                        credit_card_token              = this.TokenID,
                                                                                                    }
                                                                                            );

                        var pendingPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.Data.id == "pending");
                        pendingPaymentMethod.Data.id = result.created_payment_method.id;
                        
                        s.Log(result.created_payment_method.id);     
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Transaction Steps
        
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var upm = CHECKOUT.PaymentMethods.UserPaymentMethods.First();
                        
                        var response = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                  ,headers  : new ()
                                                                                            {
                                                                                                { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                            }
                                                                                  ,body     : new Shared.ChargeRequest()
                                                                                            {
                                                                                                session_id            = CHECKOUT.Session.SessionID,
                                                                                                is_new_payment_method = false,
                                                                                                payment_method        = new PaymentMethodDetails()
                                                                                                                      {
                                                                                                                           id   = upm.Data.id,
                                                                                                                           data = new ()
                                                                                                                                {
                                                                                                                                     { "token",  this.TokenID },                                                                                                                                   
                                                                                                                                }
                                                                                                                      },
                                                                                                save_payment_method   = false,
                                                                                                //metadata              = CHECKOUT.Session.Metadata,
                                                                                            });
                        
                        
                        //////////////////////////////////////////////////////////////////////
                        ///
                        // response = new ChargeResponse()
                        // {
                        //     result       = null,
                        //     next_actions = new PaymentAction[]
                        //                  {
                        //                      new PaymentAction()
                        //                      {
                        //                          action     = "open_url",
                        //                          parameters = new Dictionary<string, object>()
                        //                                     {
                        //                                         { "url",            "https://levan-galleon.github.io/galleon_web_demo?title=Superplay%20Product&price=$4.99" },
                        //                                         { "deep_link_path", "checkoutapp"                                                                            }
                        //                                     } 
                        //                      }
                        //                  }
                        // };
                        ///
                        //////////////////////////////////////////////////////////////////////
                        
                        if (response.next_actions != null)
                        {
                            var flow = s.ParentStep;
                            
                            foreach (var paymentAction in response.next_actions)
                            {
                                if (paymentAction.action == "open_url")
                                {
                                    var url          = paymentAction.parameters["url"           ].ToString();
                                  //var deepLinkPath = paymentAction.parameters["deep_link_path"].ToString();
                                    string deepLinkPath = "test.app";
                                    flow.AddChildStep(OpenURL(url,deepLinkPath));
                                    flow.AddChildStep(CheckStatus());
                                }
                            }
                        }
                        else if (response.result != null)
                        {    
                            CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                          {
                                                                                              errors      = null,
                                                                                              is_canceled = false,
                                                                                              is_success  = true,
                                                                                              charge_id   = response.result.charge_id,
                                                                                          };
                        } 
                        else
                        {
                            // NO TRANSACTION RESULT AND NO NEXT ACTION. ERROR.
                            throw new Exception("Charge: No result or next action");
                        }
                    });   
        
        public Step OpenURL(string url, string deepLinkPath = null) 
        =>
            new Step(name   : $"open_url"
                    ,action : async (s) =>
                              {
                                  #if UNITY_EDITOR
                                  Application.OpenURL(url);
                                  await Task.Delay(2000);
                                  return;
                                  #endif
                                  
                                  Dictionary<string, object> values = new();
                                  
                                  await CheckoutClient.Instance.URLs.OpenAndAwaitURL(url,deepLinkPath, values).Execute();
                                  
                                  foreach (var kvp in values)
                                      Debug.Log($"+ value : {kvp}");
                              });
        
        public Step CheckStatus(int attemptNumber = 1) 
        =>
            new Step(name   : $"check_status_attempt_{attemptNumber}"
                    ,action : async (s) =>
                              {
                                  int maxAttempts = 5;
                                  
                                  var response = await CHECKOUT.Network.Get<CheckoutSessionResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/checkout-session/{CHECKOUT.Session.SessionID}"
                                                                                                    ,headers  : new ()
                                                                                                              {
                                                                                                                  { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                              });
                                  
                                  var status = response?.status ?? "NULL";
                                  s.Log(status);
                        
                                  if (status != null && status == "completed")
                                  {
                                      // transaction over
                                  }
                                  else if (attemptNumber < maxAttempts)
                                  {
                                      await Task.Delay(1000);
                                      s.ParentStep.AddChildStep(CheckStatus(attemptNumber + 1));
                                  }
                                  else
                                  {
                                      Debug.Log("max reattempts reached - transaction failed.");
                                  }
                              });
        
        public Step AwaitSocket()
        =>
            new Step(name   : $"await_socket"
                    ,action : async (s) =>
                    {
                        //// Definitions
                        
                        string action     = "next_action";
                        string socketIP   = "127.0.0.1";
                        int    socketPort = 12345;
                        
                        //// Socket
                        
                        using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            try
                            {
                                await Task.Factory.FromAsync(clientSocket.BeginConnect, clientSocket.EndConnect, socketIP, socketPort, null);
                                
                                if (clientSocket.Connected)
                                {
                                    s.Log("Connected to the server.");

                                    /////// Test
                                    
                                    // Send action name to the server
                                    var message      = action;
                                    var messageBytes = Encoding.UTF8.GetBytes(message);
                                    await clientSocket.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
                                    
                                    s.Log("Message sent: " + message);
                                    
                                    /////// Read incoming
                                    
                                    // Buffer for incoming data.
                                    var buffer          = new byte[1024];
                                    var receivedMessage = new List<string>();

                                    // Receive data from the server in a loop.
                                    while (clientSocket.Connected)
                                    {
                                        try
                                        {
                                            int receivedBytes = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                                            if (receivedBytes > 0)
                                            {
                                                var incomingMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                                                receivedMessage.Add(incomingMessage);
                                                s.Log("Message received: " + incomingMessage);
                                            }
                                            else
                                            {
                                                break; // Connection closed by the server.
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            s.Log("Error receiving message: " + ex.Message);
                                            break;
                                        }
                                    }
                                    
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Socket connection failed: " + ex.Message);
                            }
                            finally
                            {
                                clientSocket.Close();
                            }
                        }
                        
                    });
    }
}

