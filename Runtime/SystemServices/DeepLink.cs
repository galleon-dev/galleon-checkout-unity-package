using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class DeepLinkController : Entity
    {
        public DeepLinkController()
        {
             Application.deepLinkActivated += link => Debug.Log($"DEEP LINK RECEIVED : {link}");
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public Step WaitForDeepLinkWithPath(string path, Dictionary<string, object> out_valuesFromDeepLink) 
        =>
            new Step(name   : $"wait_for_deep_link_with_path_{path}"
                    ,action : async (s) =>
                    {
                        // Definitions
                        bool   didDeepLinkTrigger = false;
                        var    startTime          = DateTime.Now;
                        string deepLinkURL        = "null";

                        // Depp link listener
                        Application.deepLinkActivated += ApplicationOndeepLinkActivated;
                                                         void ApplicationOndeepLinkActivated(string url)
                                                         {
                                                             if (!url.StartsWith(path)) return;
                                                             Application.deepLinkActivated -= ApplicationOndeepLinkActivated;

                                                             didDeepLinkTrigger = true;
                                                             deepLinkURL        = url;
                                                         }

                        // Await deep link starting with path
                        while (!didDeepLinkTrigger)
                        {
                            await Task.Yield();
                        }
                        
                        // if we reached here - the deep link has been triggered, and the wait is over.
                        s.Log($"finished waiting for deep link. link is : {deepLinkURL}");
                        
                        if (deepLinkURL != null)
                        {
                            var uri        = new Uri(deepLinkURL);
                            var linkValues = System.Web.HttpUtility.ParseQueryString(uri.Query);
                            foreach (var key in linkValues.AllKeys)
                            {
                                out_valuesFromDeepLink[key] = linkValues[key];
                            }
                        }
                        
                    });
    }
}