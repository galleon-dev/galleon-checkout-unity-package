using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Galleon.Checkout.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutActionsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization Steps
        
        public bool IsGPayAvailable = false;
        public Step CheckIsGPayAvailable()
        =>
            new Step(name   : $"check_is_google_pay_available"
                    ,action : async (s) =>
                    {
                        #if UNITY_ANDROID 
        
                        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                                using (AndroidJavaClass plugin = new AndroidJavaClass("com.example.checkoutgpaybridge.GooglePayBridge"))
                                {                            
                                    plugin.CallStatic("CheckGPayAvailable", activity);
                                    bool result     = plugin.GetStatic<bool>("IsGPayAvailable"); 
                                    IsGPayAvailable = result;
                                    
                                    var log = plugin.GetStatic<string>("Log");
                                    s.Log(log);
                                }
                        
                        #else
                        
                        // ISGPayAvailable = false;
                        
                        #endif
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vaulting Steps
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        try
                        {
                            await CheckoutClient.Instance.TokenizerController.GetTokenizer().Execute();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in GetTokenizer: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                        }
                    });
        
        public Step TokenizeCreditCard()
        =>
            new Step(name   : $"tokenize"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var creditCard          = CHECKOUT.User.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;

                            var Tokenizer           = CheckoutClient.Instance.TokenizerController.Tokenizer;

                            var cardTokenResponse   = await CHECKOUT.Network.Post(url      : Tokenizer.Payload.ServiceUrl
                                                                                 ,headers  : Tokenizer.Payload.Headers
                                                                                 ,jsonBody : Tokenizer.Payload.RequestFormat
                                                                                                              .Replace("<CC_NUMBER>", creditCard.CardNumber)
                                                                                                              .Replace("<CC_MONTH>",  $@"""{creditCard.CardMonth}""")
                                                                                                              .Replace("<CC_YEAR>",   $@"""20{creditCard.CardYear}""")
                                                                                                              .Replace("<CC_CVC>",    creditCard.CardCCV)
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
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in TokenizeCreditCard: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                        }
                    });
        
        
        public Step AddCreditCardPaymentMethod()
        =>
            new Step(name   : $"add_credit_card_payment_method"
                    ,action : async (s) =>
                    {
                        try
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
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in AddCreditCardPaymentMethod: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                            
                        }
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Transaction Steps
        
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var selectedUserPaymentMethod = CHECKOUT.User.SelectedUserPaymentMethod;

                            // ReSharper disable once SimplifyConditionalTernaryExpression
                            bool isNewPaymentMethod = (selectedUserPaymentMethod is CreditCardUserUserPaymentMethod)
                                                      ? selectedUserPaymentMethod.ShouldSavePaymentMethod
                                                      : false;

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
                                                                                                                                 data = selectedUserPaymentMethod.GetDataForCharge(),
                                                                                                                            },
                                                                                                    save_payment_method     = isNewPaymentMethod,
                                                                                                    return_url              = CheckoutClient.Instance.URLs.AppDeepLinkReturnURL
                                                                                                });
                            
                            CheckoutClient.Instance.CurrentSession.lastChargeResult = response.result;
                            
                            // CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                            //                                                         {
                            //                                                             errors      = new [] { "error" },
                            //                                                             is_canceled = false,
                            //                                                             is_success  = true,
                            //                                                             charge_id   = "12345",
                            //                                                         };

                            //////////////////////////////////////////////////////
                            // bool hasErrors = false;
                            // if (hasErrors)
                            // {
                            //     s.RemoveStepsAfterThisInParentFlow();
                            // 
                            //     s.AddNextStepsInParentFlow(new Step(name : "set_error", action: async x => { CheckoutClient.Instance.CheckoutScreenMobile.NavigationNext = "Error"; })
                            //                               ,CheckoutClient.Instance.CheckoutScreenMobile.Navigate()
                            //                               );
                            // 
                            //     return;
                            // }
                            //////////////////////////////////////////////////////

                            if (response.next_actions == null)
                            {
                                // NO TRANSACTION RESULT AND NO NEXT ACTION . ERROR .
                                throw new Exception("No transaction result and no next action.");
                            }
                            else
                            {
                                var        flow        = s; //.ParentStep;
                                List<Step> nextActions = new();

                                // foreach (var step in nextActions)
                                // {
                                //     flow.AddChildStep(step);
                                // }

                                foreach (var paymentAction in response.next_actions)
                                {
                                    if (paymentAction.action == "open_url")
                                    {
                                        var    url          = paymentAction.parameters["url"].ToString();
                                        string deepLinkPath = CheckoutClient.Instance.URLs.AppDeepLinkReturnURL;

                                        // url = "https://levan-galleon.github.io/galleon_web_demo/";

                                        flow.AddChildStep(OpenURL(url, deepLinkPath));
                                        flow.AddChildStep(CheckStatus());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in Charge: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
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
                                  UnityEditor.EditorApplication.isPaused = true;
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
                                  try
                                  {
                                      int maxAttempts = 3;

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
                                          s.RemoveStepsAfterThisInParentFlow();
                                          s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                                          s.AddNextStepsInParentFlow(new Step(name : "set_error", action: async x => { CheckoutClient.Instance.CheckoutScreenMobile.NavigationNext = "Error"; })
                                                                    ,CheckoutClient.Instance.CheckoutScreenMobile.Navigate()
                                                                    );

                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      Debug.LogError($"Error in CheckStatus: {ex.Message}");
                                      s.RemoveStepsAfterThisInParentFlow();
                                      s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                                      s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Email Steps
        
        public Step GetUserInfo() 
        =>
            new Step(name   : $"get_user_data"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var response = await CHECKOUT.Network.Get<UserInfo>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/user-info"
                                                                               ,headers  : new ()
                                                                                         {
                                                                                             { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                         });
                            CHECKOUT.User.UserInfo = response;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in GetUserInfo: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                        }
                    });
        
        public Step SetUserInfo() 
        =>
            new Step(name   : $"set_user_data"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var response = await CHECKOUT.Network.Post<UserInfo>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/user-info"
                                                                                ,headers  : new ()
                                                                                          {
                                                                                              { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                          }
                                                                                ,body     : CHECKOUT.User.UserInfo);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in SetUserInfo: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                        }
                    });
        
        public Step SetEmail() 
        =>
            new Step(name   : $"set_email"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var email     = CHECKOUT.Session.User.Email;
                            var sessionID = CHECKOUT.Session.SessionID;

                            var body      = new Shared.UpdateEmailRequest()
                                          {
                                              email      = email,
                                              session_id = sessionID,
                                          };

                            var response  = await CHECKOUT.Network.Post<UpdateEmailResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/update-email"
                                                                                            ,headers  : new ()
                                                                                                      {
                                                                                                          { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                      }
                                                                                            ,body     : body);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in SetEmail: {ex.Message}");
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.ErrorPage));
                        }
                    });
    }
}

