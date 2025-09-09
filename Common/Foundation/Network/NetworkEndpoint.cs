using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.NETWORK;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class NetworkEndpoint : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Func<String>                     URL              = () => "";
        public Http_Method                      Method           = new();
        public Dictionary<string, Func<string>> Headers          = new();
        
        public Type                             RequestBodyType  = default;
        public Type                             ResponseBodyType = default;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Dev
        
        /// History
        List<NetworkRequest> History = new();
        
        #if DEBUG
        /// Mocks
        Dictionary<string, NetworkRequest> Mocks = new();
        #endif

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Setters
        
        private NetworkEndpoint SET                     (Action                          setter ) {  setter?.Invoke(); return this;   } 
        public  NetworkEndpoint setURL                  (Func<string>                    url    ) => SET(() => this.URL       = url   );
        public  NetworkEndpoint setMethod               (Http_Method                     method ) => SET(() => this.Method    = method);
        public  NetworkEndpoint setHeaders              (params (string, Func<string>)[] headers) => SET(() => headers.ToList().ForEach(h => this.Headers.Add(h.Item1, h.Item2))); 
        public  NetworkEndpoint setRequestBodyType      (Type                            type   ) => SET(() => this.RequestBodyType  = type);
        public  NetworkEndpoint setRequestBodyType<T>   (                                       ) => SET(() => this.RequestBodyType  = typeof(T));
        public  NetworkEndpoint setResponseBodyType     (Type                            type   ) => SET(() => this.ResponseBodyType = type);
        public  NetworkEndpoint setResponseBodyType<T>  (                                       ) => SET(() => this.ResponseBodyType = typeof(T));
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public NetworkRequest Request()
        {
            var request = new NetworkRequest()
                         .setURL             (URL?.Invoke())
                         .setMethod          (Method)
                         .setHeaders         (Headers)
                         .setRequestBodyType (RequestBodyType)
                         .setResponseBodyType(ResponseBodyType)
                         ;
            
            return request;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// INSPECTOR
        
        public class Inspector : Inspector<NetworkEndpoint>
        {
            public Inspector(NetworkEndpoint target) : base(target)
            {
                this.Add(new TextField(label:"URL"    ) { value = Target.URL?.Invoke() });
                this.Add(new TextField(label:"Method" ) { value = Target.Method.ToString() });
                this.Add(new TextField(label:"Headers") { value = string.Join("\n", Target.Headers.Select(h => $"'{h.Key}'='{h.Value?.Invoke()}")) });
                this.Add(new Label($""));
            }
        }
    }
}
