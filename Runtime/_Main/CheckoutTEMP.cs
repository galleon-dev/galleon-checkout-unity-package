using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using Galleon.Checkout.NETWORK;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
public static class LeakDetectionInitializer
{
    static LeakDetectionInitializer()
    {
        // Enable leak detection with full stack traces in editor
        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        Debug.Log("Native Leak Detection with Stack Traces Enabled");
    }
}
#endif

namespace Galleon.Checkout
{
    public class CheckoutTEMP : Entity
    {
        /// Init/Payment_Method_Definitions
        ///     Credit_Card
        ///     {
        ///         Supported Cards
        ///         Button Text
        ///         Image
        ///     }
        ///
        /// Init/Payment_Methods (Data)
        ///     Credit Card - Amex - **** - 1234
        ///     {
        ///         Data
        ///             Token
        ///         Actions
        ///             Charge
        ///             3DS
        ///     }
        ///
        /// Payment_Method_Entity
        /// =
        ///     Definition
        ///     Data
        ///     Actions
        ///
        ///         Session Start
        ///             Transaction Start
        ///                 payment_method_transaction_action
        ///                 payment_method_transaction_action
        ///                 payment_method_transaction_action
        ///                 final_action_updates_result_or_error
        ///             Transaction End
        ///         Session End
        ///
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp

        public static void OpenUrl(string url)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // Get current Android activity
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Create Uri.parse(url)
                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (var uri      = uriClass.CallStatic<AndroidJavaObject>("parse", url))

                    // Create CustomTabsIntent.Builder()
                    using (var builder          = new AndroidJavaObject("androidx.browser.customtabs.CustomTabsIntent$Builder"))
                    using (var customTabsIntent = builder.Call<AndroidJavaObject>("build"))
                    {
                        // Launch the URL
                        customTabsIntent.Call("launchUrl", activity, uri);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to open Chrome Custom Tab: " + e.Message);
                Application.OpenURL(url); // fallback
            }
            #else
            Application.OpenURL(url); // fallback in Editor or non-Android
            #endif
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        struct temp
        {
            public int number;
        }
        #if UNITY_EDITOR
        [MenuItem("Tools/Test Temporary Method")]
        private static void TestTemporaryMethod()
        {
            var list = Root.Instance.Node.Descendants().SelectMany(x => x.Node.Reflection.Steps(new temp()));
          //var list = Root.Instance.Node.Descendants().SelectMany(x => x.Node.Reflection.Steps());

            foreach (var step in list)
            {
                Debug.Log(step.Name);
            }        
        }
        #endif
        
        
        #region TEST STEPS
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Steps
        
        public Step TEST_FLOW()
        => 
            new Step(name   : "TEST_FLOW"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                              {
                                  await CheckoutClient.Instance.RunCheckoutSession(new CheckoutProduct() { DisplayName = "Test Product", PriceText = "$4.99"})
                                                               .Execute();
                                  
                                  // Test
                                  // s.AddChildStep(this.SetupTest());
                                  //s.AddChildStep(this.TestRealServer);
                                  //s.AddChildStep(this.TestConfig);
                                  //s.AddChildStep(this.TestToken);
                                  //s.AddChildStep(this.TestTransaction);
                                  //s.AddChildStep(this.TestUI);
                              });

        public Step SetupTest()
        =>
            new Step(name   : $"setup_test"
                    ,action : async (s) =>
                              {
                                  User user = new User();
                                  CheckoutClient.Instance.Users.Users.Add(user);
                              });
        
        
        public Step TestConfig()
        =>
            new Step(name   : $"test_config"
                    ,action : async (s) =>
                              {
                                  foreach (var pair in CHECKOUT.Config.ConfigData)
                                  {
                                      s.Log($"{pair.Key} : {pair.Value}");
                                  }
                              });


        public Step TestToken()
        =>
            new Step(name   : $"test_token"
                    ,action : async (s) =>
                              {
                                //var token = await CheckoutClient.Instance.Tokens.CreateCreditCardToken();
                                //var user  = CheckoutClient.Instance.Users.Users.FirstOrDefault();
                                //user.Tokens.Add(token);
                              });

        public Step TestTransaction()
        =>
            new Step(name   : $"test_transaction"
                    ,action : async (s) =>
                              {
                                //var transaction = await CheckoutClient.Instance.Transactions.CreateTestTransaction();
                                //s.AddChildStep(transaction.Purchase());
                              });

        public Step TestUI()
        =>
            new Step(name   : $"test_ui"
                    ,action : async (s) =>
                              {
                                  GameObject.Instantiate(CheckoutClient.Instance.Resources.CheckoutPopupPrefab, position: new Vector3(0,0,9999), rotation: Quaternion.identity);
                              });
        
        public Step TestRealServer()
        =>
            new Step(name   : $"test_real_server"
                    ,action : async (s) =>
                              {
                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Health Check");
                                  
                                  
                                  await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/health");
                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Get Access Token");
                                  
                                  var accessToken = await CHECKOUT.Network.Post(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/authenticate"
                                                                               ,body : new
                                                                                     {
                                                                                         AppId  = "test.app",
                                                                                         Id     = "app-1",
                                                                                         Device = "local_unity_test_client",
                                                                                     });
                                  
                                  /// Response Example :
                                  /// {
                                  ///     "accessToken" : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6InBheWVyIiwiaXNzIjoiR2FsbGVvbiIsImF1ZCI6InRlc3QuYXBwIn0.xdg3d7xMIVAhDm_9JpSl5MENnmWAaZDHRjApWUMj-t8",
                                  ///     "appId"       : "test.app",
                                  ///     "id"          : 1,
                                  ///     "externalId"  : "user id 1"
                                  /// }
                                  
                                  var response  = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(value               : accessToken.ToString()
                                                                                                      ,anonymousTypeObject : new
                                                                                                                           {
                                                                                                                               accessToken = "", 
                                                                                                                               appId       = "", 
                                                                                                                               id          = 0 , 
                                                                                                                               externalId  = "",
                                                                                                                           });
                                  var userAccessToken = response.accessToken;
                                  s.Log($"Retrieved access token: {userAccessToken}");

                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Get Tokenizer");
                                  
                                  var tokenizerResponse = await CHECKOUT.Network.Get(url     : $"{CHECKOUT.Network.SERVER_BASE_URL}/tokenizer"
                                                                                    ,headers : new()
                                                                                             {
                                                                                                { "Authorization", $"Bearer {userAccessToken}" },
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


                                  var tokenizer = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(value               : tokenizerResponse.ToString()
                                                                                                      ,anonymousTypeObject : new 
                                                                                                                           {
                                                                                                                               Timestamp = 0L,
                                                                                                                               Payload   = new
                                                                                                                               {
                                                                                                                                   ServiceUrl    = "",
                                                                                                                                   RequestFormat = "",
                                                                                                                                   Headers       = new Dictionary<string,string>()
                                                                                                                               }
                                                                                                                           });
                                  
                                  s.Log($"Retrieved tokenizer service URL: {tokenizer.Payload.ServiceUrl}");

                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Tokenize");
                                  
                                  var card = new 
                                           {
                                               Number = "4242424242424242",
                                               Month  = 12,
                                               Year   = 2026,
                                               Cvc    = "123"
                                           };
                                  
                                  var cardTokenResponse = await CHECKOUT.Network.Post(url      : tokenizer.Payload.ServiceUrl
                                                                                     ,headers  : tokenizer.Payload.Headers
                                                                                     ,jsonBody : tokenizer.Payload.RequestFormat
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

                                  var    jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(cardTokenResponse.ToString());
                                  string tokenID    = jsonObject["id"]?.ToString();
                                  s.Log($"Token ID: {tokenID}".Color(Color.green));
                                                                      
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                  
                                  var chargeResponse = await CHECKOUT.Network.Post(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                  ,headers  : new ()
                                                                                            {
                                                                                                { "Authorization", $"Bearer {userAccessToken}" }
                                                                                            }
                                                                                  ,body     : new
                                                                                            {
                                                                                                Sku      = "sku-1",
                                                                                                Quantity = 1,
                                                                                                Amount   = 100,
                                                                                                Currency = "USD",
                                                                                                Card     = new 
                                                                                                         {
                                                                                                             Number   = tokenID,
                                                                                                             ExpMonth = tokenID,
                                                                                                             ExpYear  = tokenID,
                                                                                                             Cvc      = tokenID,
                                                                                                         }
                                                                                            });

                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  //#if UNITY_EDITOR
                                  //UnityEditor.EditorApplication.isPlaying = false;
                                  //#endif
                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                              });
        
        #endregion // TEST STEPS
        
        #region OLD_CODE
        /// public async Task<CreditCardToken> CreateCreditCardToken()
        /// {
        ///     var token = await BasisTheoryAPI.CreateCreditCardToken();
        /// 
        ///     this.tokens.Add(token);
        ///     return token;
        /// }
        #endregion
        
        #region Socket Tests
        
        public Step Socket_Server()
        =>
            new Step(name   : $"Socket_Server"
                    ,action : async (s) =>
                    {   
                        // Create a generic server socket that listens on port 1234
                        using (var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            try
                            {
                                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 12345));
                                serverSocket.Listen(10); // Allow a queue of up to 10 pending connections
                                
                                s.Log("Server is listening on port 1234...");
                                
                                while (true) // Accept multiple connections
                                {
                                    // Accept a connection
                                    Socket clientSocket = await Task.Factory.FromAsync(serverSocket.BeginAccept, serverSocket.EndAccept, null);
                                    s.Log("Client connected.");
                                    
                                    // Read message
                                    var    buffer   = new byte[1024]; // Buffer for receiving messages
                                    int    received = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                                    string message  = Encoding.UTF8.GetString(buffer, 0, received);
                                    s.Log("Received message: " + message);
                                    
                                    // Close the client connection
                                    clientSocket.Close(); 
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Server socket error: " + ex.Message);
                            }
                            finally
                            {
                                serverSocket.Close();
                            }
                        }
                    });
        
        public Step Socket_Client()
        =>
            new Step(name   : $"Socket_Client"
                    ,action : async (s) =>
                    {                                        
                        // Create a generic socket connection to localhost:1234
                        using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            try
                            {
                                await Task.Factory.FromAsync(clientSocket.BeginConnect, clientSocket.EndConnect, "212.199.63.18", 12345, null);
                                
                                if (clientSocket.Connected)
                                {
                                    s.Log("Connected to the server.");

                                    // Send "hello world" to the server
                                    var message      = "hello world";
                                    var messageBytes = Encoding.UTF8.GetBytes(message);
                                    await clientSocket.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
                                    
                                    s.Log("Message sent: " + message);
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
        
        #endregion
        
        #region Updated_temp_flow
        /*
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        
        
        public Step Temp_Flow()
        =>
            new Step(name   : $"Temp_Flow"
                    ,action : async (flow) =>
                    {
                        PaymentMethodDefinitionsResponse PamentMethodDefinitions = default;
                        UserPaymentMethodsResponse       userPaymentMethods          = default;
                        
                        CreditCardUserUserPaymentMethod   firstCardUserUser;
                        
                        PaymentInitiationResponse paymentInitiation = default;
                        
                        
                        string sku       = "sku-1";
                        int    quantity  = 1;
                        int    amount    = 100;
                        string currency  = "USD";
                        
                        string cardToken = "asdf";
                         
                        
                        flow.AddChildStep("get_payment_method_definitions"
                                         ,async x =>
                                         {
                                             PamentMethodDefinitions = await CHECKOUT.Network.Get<PaymentMethodDefinitionsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/available_payment_methods");
     
                                             foreach (var definition in PamentMethodDefinitions.paymentMethods)
                                                 x.Log($"- {definition.name}, {definition.type}");
                                         });
                        
                        
                        
                        flow.AddChildStep("remove_card"
                                         ,async x =>
                                         {
                                             var response = await CHECKOUT.Network.Post<RemoveCardResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/remove_card"
                                                                                                           ,body : new RemoveCardRequest()
                                                                                                           {
                                                                                                              credit_card_token = cardToken,
                                                                                                           });

                                             x.Log($"- {response.ToString()}");
                                         });
                        
                        flow.AddChildStep("add_new_card"
                                         ,async x =>
                                         {
                                             var response = await CHECKOUT.Network.Post<AddCardResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/add_card"
                                                                                                        ,body : new AddCardRequest()
                                                                                                        {
                                                                                                           
                                                                                                        });

                                             x.Log($"- {response.ToString()}");
                                         });
                        
                        
                        flow.AddChildStep("get_user_payment_methods"
                                         ,async x =>
                                         {
                                             userPaymentMethods = await CHECKOUT.Network.Get<UserPaymentMethodsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/payment_methods");
   
                                             foreach (var paymentMethod in userPaymentMethods.payment_methods)
                                                 x.Log($"- {paymentMethod.type}");
                                         });
                        
                        
                        flow.AddChildStep("get_first_card"
                                         ,async x =>
                                         {
                                             var card = userPaymentMethods.payment_methods.FirstOrDefault(x => x.type == "card") as CreditCardUserPaymentMethodData;
                                             
                                             firstCardUserUser = new CreditCardUserUserPaymentMethod()
                                                         {
                                                           Type = card.type,
                                                         };
                                             
                                             x.Log($"first card : {firstCardUserUser.Type}");
                                         });
                                             
                        
                        flow.AddChildStep("transaction_1_credit_card"
                                         ,async transaction_1 =>
                                         {
                                             
                                             transaction_1.AddChildStep("start_transaction_1"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "stripe",
                                                                                                                                                                       data = new Dictionary<string, object>
                                                                                                                                                                            {
                                                                                                                                                                                { "Number",   cardToken },
                                                                                                                                                                                { "ExpMonth", cardToken },
                                                                                                                                                                                { "ExpYear",  cardToken },
                                                                                                                                                                                { "Cvc",      cardToken }
                                                                                                                                                                            }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                     { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                         //x.Log($"- is success     : {paymentInitiation.success}");
                                                                         //x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                             
                                             transaction_1.AddChildStep("charge"
                                                                       ,async x =>
                                                                       {   
                                                                           var chargeResponse = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                                                                           ,headers  : new ()
                                                                                                                                                     {
                                                                                                                                                         { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                                                                     }
                                                                                                                                           ,body     : new ChargeRequest()
                                                                                                                                                     {
                                                                                                                                                         sku      = "sku-1",
                                                                                                                                                         quantity = 1,
                                                                                                                                                         amount   = 100,
                                                                                                                                                         currency = "USD",
                                                                                                                                                         card     = new()
                                                                                                                                                                  {
                                                                                                                                                                      number   = cardToken,
                                                                                                                                                                      expMonth = cardToken,
                                                                                                                                                                      expYear  = cardToken,
                                                                                                                                                                      cvc      = cardToken,
                                                                                                                                                                  }
                                                                                                                                                     });
                                                                           
                                                                           x.Log($"- is success : {chargeResponse.transaction_result.isSuccess}");
                                                                           x.Log($"- payment ID : {chargeResponse.PaymentId}");
                                                                       });
                                             
                                             transaction_1.AddChildStep("validate"
                                                                       ,async x =>
                                                                       {
                                                                           var isValid = CHECKOUT.Network.Post<bool>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/validate"
                                                                                                                    ,body : new // Validate Request
                                                                                                                    {
                                                                                                                        PaymentId = 2.ToString(),
                                                                                                                        Sku       = sku,
                                                                                                                        Quantity  = quantity,
                                                                                                                        Amount    = amount,
                                                                                                                        Currency  = currency
                                                                                                                    });
                                                                           
                                                                           x.Log($"- is valid : {isValid}");
                                                                       });
                                         });
                        
                        
                        flow.AddChildStep("transaction_2_google_pay"
                                         ,async transaction_2 =>
                                         {
                                             transaction_2.AddChildStep("start_transaction_2"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "google_pay",
                                                                                                                                                                     //data = new Dictionary<string, object>
                                                                                                                                                                     //     {
                                                                                                                                                                     //         { "Number",   cardToken },
                                                                                                                                                                     //         { "ExpMonth", cardToken },
                                                                                                                                                                     //         { "ExpYear",  cardToken },
                                                                                                                                                                     //         { "Cvc",      cardToken }
                                                                                                                                                                     //     }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                       { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                         //x.Log($"- is success     : {paymentInitiation.success}");
                                                                         //x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                             
                                             transaction_2.AddChildStep("open_url_await_socket"
                                                                       ,async x =>
                                                                       {
                                                                           //var url = "http://www.google.com";
                                                                           //Application.OpenURL(url);
                                                                           
                                                                           NetworkSocket socket = new NetworkSocket();
                                                                           socket.socketIP      = "127.0.0.1";
                                                                           socket.socketPort    = 12345;
                                                                           socket.Timeout       = TimeSpan.FromSeconds(5);
                                                                           
                                                                           await socket.Listen().Execute();

                                                                           foreach (var message in socket.IncomingMessages)
                                                                               x.Log($"> {message}");
                                                                       });
                                         });
                        
                        
                        flow.AddChildStep("transaction_3_pay_pal"
                                         ,async transaction_2 =>
                                         {
                                             transaction_2.AddChildStep("start_transaction_3"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "paypal",
                                                                                                                                                                     //data = new Dictionary<string, object>
                                                                                                                                                                     //     {
                                                                                                                                                                     //         { "Number",   cardToken },
                                                                                                                                                                     //         { "ExpMonth", cardToken },
                                                                                                                                                                     //         { "ExpYear",  cardToken },
                                                                                                                                                                     //         { "Cvc",      cardToken }
                                                                                                                                                                     //     }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                       { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                         //x.Log($"- is success     : {paymentInitiation.success}");
                                                                         //x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                         });
                        
                        
                    });
        
        
        */
        #endregion
        
        #region Test Input Class
        
        public class TestInputEvent : Entity
        {
            /////////////////////////////////////////////////// Members
            
            public string       EventName;
            
            public List<string> ExpressionssToRun;
            public List<Step>   StepsToRun;
            
            /////////////////////////////////////////////////// Main API
            
            public Step Evaluate(string scenarioName)
            =>
                new Step(name   : $"evaluate_event_{EventName}"
                        ,action : async (s) =>
                        {
                            if (!scenarios.ContainsKey(scenarioName))
                                return;
                            
                            var scenarioExpressions = this.scenarios[scenarioName];
                            var steps               = scenarioExpressions.Select(x => new Step(name : x, action: async step=>{} ));

                            foreach (var step in steps)
                            {
                                await step.Execute();
                            }
                        });
            
            /////////////////////////////////////////////////// Methods
            
            Dictionary<string, List<string>> scenarios = new();
            
            public void RegisterAction(string eventName, string Action)
            {
                
            }
            
            //// Possible Steps
            ///  
            /// Finish Page with result.
            /// Update CurrentTest.
            /// DoAction (e.g. Fill all input fields) 
        }
        #endregion // Test input class
        
        #region Network
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Network
        
        public NetworkEndpoint TestEndpoint = new NetworkEndpoint()
                                              .setURL    (() => $"{CHECKOUT.Network.SERVER_BASE_URL}/authenticate")
                                              .setMethod (Http_Method.Post)
                                            //.setHeaders(("Authorization", () => "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44"))
                                              .setHeaders(("Authorization", () => $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}"))
                                              ;
        
        public NetworkEndpoint AuthEndpoint => Network.Endpoint("/authenticate")
                                              .setMethod          (Http_Method.Post)
                                              .setRequestBodyType <Shared.AuthenticateRequest> ()
                                              .setResponseBodyType<Shared.AuthenticateResponse>()
                                              ;

        public NetworkEndpoint PMD_Endpoint = new NetworkEndpoint()
                                              .setURL    (() => $"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions")
                                              .setMethod (Http_Method.Get)
                                              .setHeaders(("Authorization", () => $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}"))
                                              ;

        
        
        public Step TestEndpoints() 
        =>
            new Step(name   : $"test_endpoints"
                    ,action : async (s) =>
                    {
                        var request = TestEndpoint.Request()
                                      .setBody(new
                                               {
                                                   app_user_id = "test",
                                                   AppId       = "test.app-1",
                                                   Id          = "test.app-1",
                                                   Device      = "local_unity_test_client",
                                               })
                                      ;
                        
                        var result  = await request.SendWebRequest();
                        
                        s.Log(result);
                    });
        
        #endregion // Network
    }    
}

/// Standards
///  > example of a class :
///     "
///     public class MyExampleClass
///     {
///         ////////////////////////////////////////////////////////////////// Members
///         
///         public  int    myInt;
///         public  string myString;
///         
///         private float  myFloat;
///         
///         ////////////////////////////////////////////////////////////////// Lifecycle
///         
///         public MyExampleClass()
///         {
///         }
///         
///         ////////////////////////////////////////////////////////////////// API Steps
///         
///         public Step DoTheAPIThing()
///         =>
///             new Step(name   : "do_the_api_thing"
///                     ,action : async s =>
///                     {
///                         await DoTheThing();   
///                     });
///                     
///         ////////////////////////////////////////////////////////////////// helper Methods
///         
///         private async Task DoTheThing()
///         {
///             // Do the thing
///         }
///         
///         //////////////////////////////////////////////////////////////////
///     }
///     "
///     

/// Overview : 
///     - The app is designed like a tree.
///     - Every entity in the app is a node in the tree
///         - There is an EntityNode class,that links between parents and children
///         - There is an IEntity Infterface witch all entities implement
///             - It only has a Property of type "EntityNode" called "Node"
///             - and so all objects can access the tree by using object.Node.Parent / object.Node.Children
///             - Node also has "Decendents" and "Ancestors" for easy traversal in the tree
/// - Initialization :
///     - when we call myEntity.Node.Initialize : at the creation of an entity (EntityNode to be exact),
///       we automaticly recursivly go over all fields that are IEntities using reflection
///       and add them to the tree (each under its relevent parent).
///       this doesnt initialize any logic, just the logistics of the tntity tree and maybe other small stuff.
/// - Structure :
///     - There is a "Root" Class that is the root of the tree.
///         - it has a child entity "Project"
///             - many editor tools and or other entities that work in edit-time (not just run-time) are initialized and added as children of "Project"
///         - it has a child entity "Runtime"
///             - all runtime objects (or rather the root tuntime objects) are initialized added under "Runtime" entity at runtime start
/// - Explorer :
///     - There is an entity Explorer
///     - data is stored in entities, not temporary variables. that way its always available in the explorer
/// - Node Aspects :
///     - Log
///     - Breadcrumbs
///     - Testing
///     - Debug
///     - Storage
///     - ID (static, dynamic)
///     - Reflection
///     - Inspector
///     - Editor Extras
/// - Steps :
///     - we have a class called Step.
///     - we put all major code operations in steps
///     - example of a step :
///     "
///     public Step myStep()
///     =>
///         new Step(name   : "my_step"
///                 ,action : async (s) =>
///                 {
///                     // do something here
///                 } 
///     "
///     - a step has some basic properties like name and optional tags
///     - a step has an Execute() method that executes the step
///         - a step may have an action that is the first thing that runs when its executed
///         - a step may have child steps that run in sequence (after the action) when its executed
///         - a step may also have "PreSteps" and "PostSteps" that run (before and after) the child steps
///             - for example : a child step can dynamicly add more steps in runtime to the childSteps sequence, but the PreSteps and PostSteps remain as they are
///         - a step has some debug features, such as log, stopwatch, breadcumbs, etc..
///         - the action of a step receives a reference to the step itself, so that it can work with it and use its features
/// 

/// - There is an Interface called IEntity
/// - There is a class called Entity
/// - There is a class called EntityNode
///     + Aspects
///         + Tree     

/// - editor window called "Explorer"
/// - a visual element called "ExplorerItem"
///     - Row
///         + foldout button
///         + main button
///         + indicators
///     - Self Content
///         + Inspectors
///     - Children

        /// > Explorer Window
        ///     - Create an editor window called EditorExplorer
        ///    (- Create a runtime ui doc called RuntimeExplorer)
        /// > Explorer
        ///     - Create a VisualElement called Explorer
        ///       - the explorer will show the entity tree, and will contain the root visual element that will handle its children, etc.
        /// > Explorer Modes
        ///     - Tree
        ///     - Split Portrait
        ///     - Split Landscape
        /// > Widget
        ///     > Widget UI
        ///         - Create a VisualElement class called "Widget"
        ///         - it acts similar to a foldout, where it has a main item (button) and content that you can toggle, but more complicated then a foldout.
        ///         - the main item :
        ///             - Children Foldout Button
        ///             - Main button )opens content foldout)
        ///             - Extras
        ///                 - Indicators
        ///         - Children foldout
        ///         - Content foldout
        ///             - Inspectors
        ///             - Entity Inspector
        ///             - Raw Inspector
        ///     > Widget Code
        ///         - Data
        ///         - Refresh
        /// > Explorer Item
        ///     - Create a Class called ExplorerItem

/// > "Resource" (Element)
///     - the project is madeup of Assets.
///     - a logical related group of assets is an element.
///     - an entity (via its node) can get a reference to the relevent element that the entity belongs to.
///         - this information is defined in the element.
///         - for example. the "FolderElement" defines that it has a "FolderAsset" as its asset type. so when calling FolderAsset.Element we have to info needed to return FolderElement.  
/// > CRUD & Live Actions
///     - an element has CRUD and LIVE actions.
///         - Live = Plus, Minus, Equals
///         - CRUD = Create, Update, Delete
///         - Scan
/// > Trees
///     - there are different types or "layers" of logical trees in a project or a package
///     - for example
///         - the assets tree - the layer that is all the actual assets and files making up the project.
///         - Heirarchy tree - the layer that is all the objects in the heirarchy, such as Scenes Prefabs, GameObjects and components.
///         - Data tree - data is represented by its own tree.
///         - Runtime tree - the layer that is all the runtime entities in the app at runtime
///     - a tree is defined by its root. the root is just an entity. like all other entities. it functions as a root of a tree.
/// > CRUD & Live with trees
///     - element defines what trees ("layers") it relates to. and what elements are relevant for each of those trees.
///     - an element can define complex +/-/= logic between trees.
///         - for example :
///             - adding a "FolderAsset" to "Assets" tree is a direct simple action.
///             - but adding "FolderAsset" to "Core" is a complex action - the core element defines that assets that are added to it should be added under its assets element, and so the final result is that the folder asset is added to the assets tree under core.   
 
        /// > Elements
        ///     - what are elements ?
        ///         - for example : when we want to add a "Car" to our project, we dont say "add a prefab with a script and a mesh and a material of a car and put it in a folder called car, and initialize it in runtime, and manage all cards with a car-manager, etc.",
        ///                         we just say "add a car" and the rest is implied from our understanding of the structure and the standards of the project.
        ///         - a "car" in this example is an element.
        ///         - an Element is a class.
        ///         - an Element object is a group of related assets, resources, definitions, data, and whatever else is needed, that represents a specific entity/concept in a project.
        ///         - there are many "trees" that make up a project: the assets tree, the hierarch tree, data tree, runtime tree, etc.   
        ///         - an Element also defines what actions happen when you add/remove/modify it in a project,
        ///             for simple elements this maybe a simple straight-forward operation like adding a new folder under a parent folder in the assets tree,
        ///             and for a "car" for example, this will probably involve adding a prefab, a script, a mesh, a material, i.e. updating many trees in the project.
        /// 
        ///                


/// > Network
///     > Endpoint
///     > Request
///     > History
///     > Mock

