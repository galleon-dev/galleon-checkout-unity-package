using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public class NetworkRequest : Entity
    {
        //// Members
        
        public UnityWebRequest UnityWebRequest;
        
        int timeoutMilliseconds = 5000;
        
        //// Properties
        
        public string url    { get => UnityWebRequest.url;    set => UnityWebRequest.url    = value; } 
        public string method { get => UnityWebRequest.method; set => UnityWebRequest.method = value; }     
        
        //// Setters
        
        #region Setters
        
        public NetworkRequest SetHeader(string header, string value)
        {
            UnityWebRequest.SetRequestHeader(header, value);
            return this;
        }
        public NetworkRequest SetHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
                UnityWebRequest.SetRequestHeader(header.Key, header.Value);
                
            return this;
        }
        
        #endregion
    }
}