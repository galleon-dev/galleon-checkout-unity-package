using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

namespace Galleon.Checkout
{
    public class GooglePayController : Entity
    {
        public Step Initialize() 
        => 
            new Step(name   : "initialize_google_pay_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
        
        public bool IsGPayAvailable = false;
        
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
                        
                        // ISGPayAvailable = false;
                        
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
