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
                            CHECKOUT.Session.LogError("GetTokenizer", ex);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_network_tokenizer_message",     defaultValue: "Network Error"),
                                errorDescription: CHECKOUT.Config.GetString("error_network_tokenizer_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_network_tokenizer_button",      defaultValue: "Back To Payment Options"))
                                );
                        }
                    });
        
        public Step TokenizeCreditCard()
        =>
            new Step(name   : $"tokenize"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var creditCard          = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;

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
                            CHECKOUT.Session.LogError("TokenizeCreditCard", ex);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_card_validation_message",     defaultValue: "Card validation error"),
                                errorDescription: CHECKOUT.Config.GetString("error_card_validation_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_card_validation_button",      defaultValue: "Back To Payment Options"))
                                );
                        }
                    });
        
        
        public Step AddCreditCardPaymentMethod()
        =>
            new Step(name   : $"add_credit_card_payment_method"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var creditCard = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;

                            var result = await CHECKOUT.Network.Post<AddPaymentMethodResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/add-payment-method" 
                                                                                              ,headers  : new ()
                                                                                                        {
                                                                                                            { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                        }
                                                                                              ,body     : new AddPaymentMethodRequest()
                                                                                                        {
                                                                                                            payment_method_definition_type = "card",
                                                                                                            credit_card_token              = creditCard.TokenID,
                                                                                                        }
                                                                                                );

                            s.Log(result.created_payment_method.id);
                        }
                        catch (Exception ex)
                        {
                            CHECKOUT.Session.LogError("AddCreditCardPaymentMethod", ex);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_add_card_message",     defaultValue: "Card Can Not Be Added"),
                                errorDescription: CHECKOUT.Config.GetString("error_add_card_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_add_card_button",      defaultValue: "Back To Payment Options"))
                                );

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
                            var selectedUserPaymentMethod = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod;

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
                            
                            
                            if (response == null)
                                throw new Exception("/Charge unsuccessfull - (did you enter invalid/test card-info in prod ?)");
                                
                            CheckoutClient.Instance.CurrentSession.lastChargeResult = response.result;
                            
                            var result = response.result;

                            if (result == null)
                            {
                                SendErrorAnalytics();
                            }
                            if (response.next_actions == null && result == null)
                            {
                                // NO TRANSACTION RESULT AND NO NEXT ACTION . ERROR .
                                throw new Exception("No transaction result and no next action.");
                            }
                            else if (response.next_actions == null && result != null)
                            {
                                // Analytics: Payment Succeeded or Failed                                
                                if (result.is_success && !result.is_canceled)
                                    SendSuccessAnalytics();
                                else if (result.errors?.Length > 0 || !result.is_success)
                                    SendErrorAnalytics();
                            }
                            else
                            {
                                var        flow        = s; //.ParentStep;
                                List<Step> nextActions = new();

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
                            CHECKOUT.Session.LogError("Charge", ex.Message);
                            SendErrorAnalytics();
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_charge_message",     defaultValue: "Payment failed"),
                                errorDescription: CHECKOUT.Config.GetString("error_charge_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_charge_button",      defaultValue: "Back To Payment Options"))
                                );
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
                                  
                                  if (values.Count == 0)
                                      Debug.LogWarning($"Returned from URL - But not via DeepLink - User probably canceled");
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

                                      if (response?.errors != null && response?.errors?.Count() > 0)
                                      {
                                          CHECKOUT.Session.LogError("CheckStatus", $"Response contains errors: {string.Join(", ", response.errors)}");

                                          CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                                  {
                                                                                                      charge_id   = CheckoutClient.Instance.CurrentSession.lastChargeResult.charge_id,
                                                                                                      errors      = response.errors,
                                                                                                      is_canceled = false,
                                                                                                      is_success  = false,
                                                                                                  };

                                          SendErrorAnalytics();
                                      }
                                      else if (status != null && status == "completed")
                                      {
                                          // transaction over
                                          SendSuccessAnalytics();
                                          
                                          CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                                  {
                                                                                                      charge_id   = CheckoutClient.Instance.CurrentSession.lastChargeResult.charge_id,
                                                                                                      errors      = null,
                                                                                                      is_canceled = false,
                                                                                                      is_success  = true,
                                                                                                  };
                                      }
                                      else if (status != null && status == "canceled")
                                      {
                                          CHECKOUT.Session.LogError("cancelede", $"User Canceled");
                                          SendErrorAnalytics();
                                          
                                          CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                                  {
                                                                                                      charge_id   = CheckoutClient.Instance.CurrentSession.lastChargeResult.charge_id,
                                                                                                      errors      = null,
                                                                                                      is_canceled = true,
                                                                                                      is_success  = false,
                                                                                                  };
                                          
                                      }
                                      else if (attemptNumber < maxAttempts)
                                      {
                                          await Task.Delay(1000);
                                          s.ParentStep.AddChildStep(CheckStatus(attemptNumber + 1));
                                      }
                                      else
                                      {
                                          CHECKOUT.Session.LogError("CheckStatus", $"Max reattempts reached - transaction failed. Status was: {status}");
                                          
                                          Debug.Log("max reattempts reached - transaction failed.");
                                          SendErrorAnalytics();

                                          CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                                  {
                                                                                                      charge_id   = CheckoutClient.Instance.CurrentSession.lastChargeResult.charge_id,
                                                                                                      errors      = new []{ "timeout. max attempts reached." },
                                                                                                      is_canceled = true,
                                                                                                      is_success  = false,
                                                                                                  };

                                          s.RemoveStepsAfterThisInParentFlow();
                                          s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                                          s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                              errorMessage    : CHECKOUT.Config.GetString("error_payment_timeout_message",     defaultValue: "Payment failed"),
                                              errorDescription: CHECKOUT.Config.GetString("error_payment_timeout_description", defaultValue: "Try again or use another payment method."),
                                              buttonText      : CHECKOUT.Config.GetString("error_payment_timeout_button",      defaultValue: "Back To Payment Options"))
                                              );


                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      CHECKOUT.Session.LogError("CheckStatus", ex.Message);
                                      SendErrorAnalytics();
                                      s.RemoveStepsAfterThisInParentFlow();
                                      s.AddNextStepInParentFlow(CHECKOUT.Session.On_ChargeError());
                                      s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                          errorMessage    : CHECKOUT.Config.GetString("error_check_status_message",     defaultValue: "Payment Failed"),
                                          errorDescription: CHECKOUT.Config.GetString("error_check_status_description", defaultValue: "Try again or use another payment method."),
                                          buttonText      : CHECKOUT.Config.GetString("error_check_status_button",      defaultValue: "Back To Payment Options"))
                                          );
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
                                CHECKOUT.Session.LogError("AwaitSocket", ex.Message);
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
                            CHECKOUT.Session.LogError("GetUserInfo", ex.Message);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_network_user_info_message",     defaultValue: "Network Error"),
                                errorDescription: CHECKOUT.Config.GetString("error_network_user_info_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_network_user_info_button",      defaultValue: "Back To Payment Options"))
                                );
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
                            CHECKOUT.Session.LogError("SetUserInfo", ex.Message);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_network_set_user_info_message",     defaultValue: "Network Error"),
                                errorDescription: CHECKOUT.Config.GetString("error_network_set_user_info_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_network_set_user_info_button",      defaultValue: "Back To Payment Options"))
                                );
                        }
                    });
        
        public Step UpdateEmail() 
        =>
            new Step(name   : $"update_email"
                    ,action : async (s) =>
                    {
                        try
                        {
                            var response  = await CHECKOUT.Network.Post<UpdateEmailResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/update-email"
                                                                                            ,headers  : new ()
                                                                                                      {
                                                                                                          { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                      }
                                                                                            ,body     :  new Shared.UpdateEmailRequest()
                                                                                                      {
                                                                                                          email        = CHECKOUT.Session.User.Email,
                                                                                                          session_id   = CHECKOUT.Session.SessionID,
                                                                                                          delete_email = CHECKOUT.Session.User.Email.Trim() == "",
                                                                                                      });
                        }
                        catch (Exception ex)
                        {
                            CHECKOUT.Session.LogError("UpdateEmail", ex.Message);
                            s.RemoveStepsAfterThisInParentFlow();
                            s.AddNextStepInParentFlow(CheckoutClient.Instance.CheckoutScreenMobile.UI_ShowError(
                                errorMessage    : CHECKOUT.Config.GetString("error_network_update_email_message",     defaultValue: "Network Error"),
                                errorDescription: CHECKOUT.Config.GetString("error_network_update_email_description", defaultValue: "Try again or use another payment method."),
                                buttonText      : CHECKOUT.Config.GetString("error_network_update_email_button",      defaultValue: "Back To Payment Options"))
                                );
                        }
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods
        
        private void SendSuccessAnalytics()
        {
            try
            {
                var selectedPaymentMethod = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod;
                CheckoutAPI.InvokeAnalyticsEvent("payment_succeeded", new Dictionary<string, object>
                                                {
                                                    { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? ""     },
                                                    { "payment_method",      selectedPaymentMethod?.Type                 ?? "none" },
                                                    { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m     },
                                                    { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? ""     },
                                                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void SendErrorAnalytics()
        {
            try
            {
                var errors                = CHECKOUT.Session.sessionErrors.ToArray();
                var selectedPaymentMethod = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod;
                
                CheckoutAPI.InvokeAnalyticsEvent("payment_failed", new Dictionary<string, object>
                                                {
                                                    { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? ""              },
                                                    { "payment_method",      selectedPaymentMethod?.DisplayType          ?? "none"          },
                                                    { "fail_reason",         string.Join("\n", errors                    ?? new string[0])  },
                                                    { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m              },
                                                    { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? ""              }
                                                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

