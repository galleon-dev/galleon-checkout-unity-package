using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutClient : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Instance
        
        private static CheckoutClient instance;
        public  static CheckoutClient Instance => instance ?? (instance = new CheckoutClient());
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Children
        
        // Services
        [Header("Services")]
        public Logger                 Logger    = new();
        public Network                Network   = new();
        public Config                 Config    = new();
        public Analytics              Analytics = new();
        
        // Resources
        [Header("Resources")]
        public CheckoutResources      Resources;
        
        // Entities
        [Header("Entities")]
        public ProductsController     Products     = new();
        public CreditCardsController  CreditCards  = new();
        public TokensController       Tokens       = new();
        public UsersController        Users        = new();
        public TransactionsController Transactions = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Entry Point
        
        [RuntimeInitializeOnLoadMethod]
        public static async Task EntryPoint()
        {
            await Task.Yield();
            Debug.Log("CheckoutClient.EntryPoint()");
            
            Root.Instance.Runtime.Node.Children.Add(Instance);
            
            await Instance.Initialize.Execute();
            await Instance.TEST_FLOW.Execute();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize 
            => 
            new Step(name   : "initialize_checkout_client"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                              {
                                  // Services
                                  s.AddChildStep(Logger   .Initialize);
                                  s.AddChildStep(Network  .Initialize);
                                  s.AddChildStep(Config   .Initialize);
                                  s.AddChildStep(Analytics.Initialize);
                        
                                  // Resources
                                  this.Resources = CheckoutResources.Instance;
                                  s.AddChildStep(Resources.Initialize);
                        
                                  // Entities
                                  s.AddChildStep(Products    .Initialize);
                                  s.AddChildStep(CreditCards .Initialize);
                                  s.AddChildStep(Tokens      .Initialize);
                                  s.AddChildStep(Users       .Initialize);
                                  s.AddChildStep(Transactions.Initialize);
                              });
        
        
        public Step TEST_FLOW
            => 
            new Step(name   : "TEST_FLOW"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                              {
                                  // Test
                                  s.AddChildStep(this.SetupTest);
                                //s.AddChildStep(this.TestRealServer);
                                //s.AddChildStep(this.TestConfig);
                                //s.AddChildStep(this.TestToken);
                                //s.AddChildStep(this.TestTransaction);
                                //s.AddChildStep(this.TestUI);
                              });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow

        public Step SetupTest
            =>
            new Step(name   : $"setup_test"
                    ,action : async (s) =>
                              {
                                  User user = new User();
                                  this.Users.Users.Add(user);
                              });
        
        
        public Step TestConfig
            =>
            new Step(name   : $"test_config"
                    ,action : async (s) =>
                              {
                                  foreach (var pair in CHECKOUT.Config.ConfigData)
                                  {
                                      s.Log($"{pair.Key} : {pair.Value}");
                                  }
                              });


        public Step TestToken
            =>
            new Step(name   : $"test_token"
                    ,action : async (s) =>
                              {
                                  var token = await Tokens.CreateCreditCardToken();
                                  var user  = Users.Users.FirstOrDefault();
                                  user.Tokens.Add(token);
                              });

        public Step TestTransaction
            =>
            new Step(name   : $"test_transaction"
                    ,action : async (s) =>
                              {
                                  var transaction = await Transactions.CreateTestTransaction();
                                  s.AddChildStep(transaction.Purchase);
                              });

        public Step TestUI
            =>
            new Step(name   : $"test_ui"
                    ,action : async (s) =>
                              {
                                  GameObject.Instantiate(Resources.CheckoutPopupPrefab, position: new Vector3(0,0,999999), rotation: Quaternion.identity);
                        
                                  await Task.Delay(5000);
                              });
        
        public Step TestRealServer
            =>
            new Step(name   : $"test_real_server"
                    ,action : async (s) =>
                              {
                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Health Check");
                                  
                                  
                                  await CHECKOUT.Network.Get("https://localhost:4000/health");
                                  
                                  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                  
                                  s.Log("---------------------------------------Get Access Token");
                                  
                                  var accessToken = await CHECKOUT.Network.Post(url  : "https://localhost:4000/authenticate"
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
                                  
                                  var response  = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(value              : accessToken.ToString()
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
                                  
                                  var tokenizerResponse = await CHECKOUT.Network.Get(url     : "https://localhost:4000/tokenizer"
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
                                                                  
                                  var chargeResponse = await CHECKOUT.Network.Post(url      : $"https://localhost:4000/charge"
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
    }
    
    public static class CHECKOUT
    {
        public static Logger                 Logger      => CheckoutClient.Instance.Logger;
        public static Network                Network     => CheckoutClient.Instance.Network;
        public static Config                 Config      => CheckoutClient.Instance.Config;
        public static Analytics              Analytics   => CheckoutClient.Instance.Analytics;
        
        public static CheckoutResources      Resources   => CheckoutClient.Instance.Resources;
        
        public static ProductsController     Products    => CheckoutClient.Instance.Products;
        public static CreditCardsController  CreditCards => CheckoutClient.Instance.CreditCards;
        public static TokensController       Tokens      => CheckoutClient.Instance.Tokens;
        public static UsersController        Users       => CheckoutClient.Instance.Users;
        public static TransactionsController Transaction => CheckoutClient.Instance.Transactions;
    }   
}

