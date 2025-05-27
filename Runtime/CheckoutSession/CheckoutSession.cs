using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Galleon.Checkout.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Galleon.Checkout
{
    public class CheckoutSession : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // Purchase data
        public CheckoutProduct                    SelectedProduct;
        public PurchaseResult                     PurchaseResult        = new PurchaseResult();
        
        // Simple Dialog Panel data
        public string                             LastDialogRequest     = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult      = SimpleDialogPanelView.DialogResult.None;
        public PaymentMethod                      PaymentMethodToDelete = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient Client => CheckoutClient.Instance; // TODO : should be : Node.Ancestors().OfType<CheckoutClient>().First();
        public User           User   => Client.CurrentUser;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public static event Action Report;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step Flow()
        =>
            new Step(name   : $"checkout_session_flow"
                    ,action : async (s) =>
                    {
                        await CheckoutScreenMobile.OpenCheckoutScreenMobile(); //s.AddChildStep(Client.OpenCheckoutScreenMobile());
                        
                      //s.AddChildStep(Client.CheckoutScreenMobile.ViewSuccessPanel());
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                        
                        return;
                        
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewTestPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCheckoutPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCreditCardPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CurrentUser.RunTestTransaction());
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewSuccessPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(CheckoutScreenMobile.CloseCheckoutScreenMobile());
                        
                      //s.AddChildStep(new Step("Report", action: async (s) => { Report?.Invoke(); await Task.Delay(5000); }));
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        
        public class CheckoutInputData
        {
            public string AppID;
            public string ID;
            public String Device;
        }
        
        // Actual class in CheckoutAPI
        public class PurchaseResultData
        {
            public bool         IsSuccess  { get; set; }
            public bool         IsCanceled { get; set; }
            public bool         IsError    { get; set; }
            public List<string> Errors     { get; set; } 
        }
        
        ////////////////////
        
        public class PaymentMethodData
        {
            public string ID;
            public string Type;
            public string DisplayName;
            
            public string StartActionURL;
            public string WebURL;
            public string ResultURL;
        }
        
        ////////////////////
        
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 1 - Card
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 1 - Card
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 1 - Card
        
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
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 2 - PayPal
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 2 - PayPal
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 2 - PayPal
        
        // Replace with your real sandbox credentials
        public string clientId = "ATTOFcXSgTh-2K_rrnQSWhPKAFpeLICk0kOJGqvjUCP-kAy2WSkXSC3hxIcQrQmKjwbt3zTAqw0Kdfhp"; // Sandbox Client ID
        public string secret   = "ELPPeulUH7358n9NJqjSsF2iO9pANpbHHRQJ0bjkG4XOeS1z3bIPU_7bS3P7XWYi2TwbeEi3erheZQym"; // Sandbox secret
        public string baseApi  = "https://api-m.sandbox.paypal.com";

        private string returnUrl = "checkoutApp://paypal-success";
        private string cancelUrl = "checkoutApp://paypal-cancel";

        public Step GetPaypalAccessToken()
        =>
            new Step(name   : $"get_paypal_access_token"
                    ,action : async (s) =>
                    {
                        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
                        var    url         = "https://api-m.sandbox.paypal.com/v1/oauth2/token";

                        // Manually build a proper x-www-form-urlencoded request
                        UnityWebRequest request = new UnityWebRequest(url, "POST");
                        byte[]          bodyRaw = Encoding.UTF8.GetBytes("grant_type=client_credentials");

                        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.SetRequestHeader("Authorization", $"Basic {credentials}");
                        request.SetRequestHeader("Content-Type",  "application/x-www-form-urlencoded");

                        Debug.Log($">>> {request.url}");
                        var op = request.SendWebRequest();
                        while (!op.isDone) await Task.Yield();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError(request.downloadHandler.text);
                            throw new Exception($"Access token error: {request.error}");
                        }

                        Debug.Log($"<<< {request.downloadHandler.text}");
                        
                        var json = JsonUtility.FromJson<AccessTokenResponse>(request.downloadHandler.text);
                        this.PaypalAccessToken = json.access_token;
                    });
        
        ///////////////////////
        
        public string PaypalAccessToken;
        
        [Serializable]
        public class AccessTokenResponse
        {
            public string access_token;
        }
        
        ///////////////////////
        
        public Step CreatePaypalOrder()
        =>
            new Step(name   : $"CreatePaypalOrder"
                    ,action : async (s) =>
                    {
                        // Call Server to create paypal order
                        
                        
                        PaypalOrderID     = "";
                        PaypalApprovalURL = "";
                    });
        
        string PaypalOrderID;
        string PaypalApprovalURL;
        
        ///////////////////////
        
        public Step OpenAndAwaitPaypalURL()
        =>
            new Step(name   : $"open_and_await_paypal_url"
                    ,action : async (s) =>
                    {
                        Application.OpenURL(PaypalApprovalURL);
                        
                        //////////////////////////
                        
                        // await deep link return
                        
                        //////////////////////////
                        
                        // Extract result
                        
                        string redirectURL = "";
                        var    uri         = new Uri(redirectURL);
                        var    query       = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        
                        this.PaypalResultOrderToken = query.Get("token");
                    });
        
        string PaypalResultOrderToken;
        
        ///////////////////////
        
        public Step CapturePaypalPayment()
        =>
            new Step(name   : $"capture_paypal_payment"
                    ,action : async (s) =>
                    {
                        string orderId = PaypalResultOrderToken;
                        
                        UnityWebRequest request = new UnityWebRequest($"{baseApi}/v2/checkout/orders/{orderId}/capture", "POST");
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
                        request.SetRequestHeader("Authorization", $"Bearer {PaypalAccessToken}");
                        request.SetRequestHeader("Content-Type",  "application/json");

                        var op = request.SendWebRequest();

                        while (!op.isDone) 
                            await Task.Yield();

                        if (request.result != UnityWebRequest.Result.Success)
                            throw new Exception($"Capture error: {request.error}");

                        PaypalCaptureResult = request.downloadHandler.text;
                    });
        
        public string PaypalCaptureResult;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 3 - GPay
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 3 - GPay
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow 3 - GPay
        
        public bool IsGPayAvailable;
        
        public Step CheckIsGPayAvailable()
        =>
            new Step(name   : $"check_is_g_pay_available"
                    ,action : async (s) =>
                    {
                        #if UNITY_ANDROID 
        
                        using (AndroidJavaClass  unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
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
                        
                        ISGPayAvailable = false;
                        
                        #endif
                    });
        
        ///////////////////////
        
        string GPayURL;
        string GPayResult;
        
        public Step OpenAndAwaitGPayURL()
        =>
            new Step(name   : $"open_and_await_g_pay_url"
                    ,action : async (s) =>
                    {
                        Application.OpenURL(GPayURL);
                        
                        //////////////////////////
                        
                        // await deep link return
                        
                        //////////////////////////
                        
                        // Extract result
                        
                        string returnURL = "";
                        Uri    uri       = new Uri(returnURL);
                        string result    = HttpUtility.ParseQueryString(uri.Query).Get("result");
                        GPayResult       = result;
                    });
    }
}
