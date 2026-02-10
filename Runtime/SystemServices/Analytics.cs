using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Analytics : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public event Action<CheckoutAnalyticsEvent> OnCheckoutAnalyticsEvent;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize()
        =>
            new Step(name   : "initialize_analytics"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {  
                        OnCheckoutAnalyticsEvent?.Invoke(new CheckoutAnalyticsEvent
                        {
                            Name     = "Analytics Initialized",
                            Data     = CommonAnalyticsParameters,
                            DateTime = DateTime.UtcNow
                        });
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Common analytics parameters
        
        public Dictionary<string, object> CommonAnalyticsParameters => new Dictionary<string, object>
        {
            { "timestamp",  DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")   }, 
            { "session_id", CHECKOUT.Session?.SessionID ?? "null"             },
        };        
    }

    public class CheckoutAnalyticsEvent
    {
        public string                     Name     { get; set; }
        public Dictionary<string, object> Data     { get; set; }
        public DateTime                   DateTime { get; set; }
    }
}
