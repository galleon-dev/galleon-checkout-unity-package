using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout.UI;
using UnityEngine;

namespace Galleon.Checkout
{
    public class ExternalURLsController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API

        public Step OpenAndAwaitURL(string                     url, 
                                    string                     deepLinkPath           = null, 
                                    Dictionary<string, object> out_valuesFromDeepLink = null) 
        =>
            new Step(name   : $"OpenURL"
                    ,action : async (s) =>
                    {
                        bool isStillOpen = true;
                        
                        // Listen to focus change event
                        CheckoutScreenMobile.On_ApplicationFocus += onFocusChange;
                                                                    void onFocusChange(bool hasFocus)
                                                                    {
                                                                        Debug.Log($"OnApplicationFocus : {hasFocus}, date time : {DateTime.Now}");
                                                                     
                                                                        if (hasFocus)
                                                                        {
                                                                            CheckoutScreenMobile.On_ApplicationFocus -= onFocusChange;   
                                                                            isStillOpen = false;
                                                                        }
                                                                    }
                        
                        // Listen to deeplink if needed
                        if (deepLinkPath != null)
                        {
                            string deepLinkURL;
                            
                            // Depp link listener
                            Application.deepLinkActivated += ApplicationOndeepLinkActivated;
                                                             void ApplicationOndeepLinkActivated(string url)
                                                             {
                                                                 Debug.Log($"Got deeplink : {url}");
                                                                 Debug.Log($"listening to deeplink : {deepLinkPath}");
                                                                 
                                                                 if (!url.StartsWith(deepLinkPath)) return;
                                                                 Application.deepLinkActivated -= ApplicationOndeepLinkActivated;

                                                                 isStillOpen = false;
                                                                 deepLinkURL = url;
                                                                 
                                                                 // populate out_valuesFromDeepLink
                                                                 if (out_valuesFromDeepLink != null)
                                                                 {
                                                                     var uri        = new Uri(deepLinkURL);
                                                                     var linkValues = System.Web.HttpUtility.ParseQueryString(uri.Query);
                                                                     foreach (var key in linkValues.AllKeys)
                                                                     {
                                                                         out_valuesFromDeepLink[key] = linkValues[key];
                                                                     }
                                                                 }
                                                             }
                        }
                        
                        // Open the url                                            
                        OpenURL(url);

                        // Await browser to close
                        while (isStillOpen)
                        {
                            await Task.Yield();
                        }
                    });


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        private void OpenURL(string url)
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
                        Debug.Log($"Open link at {DateTime.Now}");
                        
                        // Launch the URL
                        customTabsIntent.Call("launchUrl", activity, uri);
                        
                        
                        Debug.Log($"return from link at {DateTime.Now}");
                        
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
    }
}



