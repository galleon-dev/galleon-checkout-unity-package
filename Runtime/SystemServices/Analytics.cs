using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Analytics : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize()
        =>
            new Step(name   : "initialize_analytics"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {  
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Common analytics parameters
        
    }

    public class CheckoutAnalyticsEvent
    {
        public string                     Name        { get; set; }
        public Dictionary<string, object> Data        { get; set; }

        public CheckoutAnalyticsEvent(string name, Dictionary<string, object> data)
        {
            this.Name        = name;
            this.Data        = new Dictionary<string, object>();

            // Copy common parameters first
            foreach (var kvp in CommonAnalyticsParameters)
                this.Data[kvp.Key] = kvp.Value;

            // Override with data from args (if provided)
            if (data != null)
                foreach (var kvp in data)
                    this.Data[kvp.Key] = kvp.Value;
        }
        
        public Dictionary<string, object> CommonAnalyticsParameters => new Dictionary<string, object>
        {
            { "date_time_utc", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") ?? "null" }, 
            { "session_id",    CHECKOUT.Session?.SessionID                     ?? "null" },
        };        
    }
}
