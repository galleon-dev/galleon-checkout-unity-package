using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public class PaypalController : Entity
    {
        public Step Initialize() 
        => 
            new Step(name   : "initialize_paypal_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
        
        // Replace with your real sandbox credentials
        public  string clientId = "ATTOFcXSgTh-2K_rrnQSWhPKAFpeLICk0kOJGqvjUCP-kAy2WSkXSC3hxIcQrQmKjwbt3zTAqw0Kdfhp"; // Sandbox Client ID
        public  string secret   = "ELPPeulUH7358n9NJqjSsF2iO9pANpbHHRQJ0bjkG4XOeS1z3bIPU_7bS3P7XWYi2TwbeEi3erheZQym"; // Sandbox secret
        public  string baseApi  = "https://api-m.sandbox.paypal.com";

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
        
        
    }
}

