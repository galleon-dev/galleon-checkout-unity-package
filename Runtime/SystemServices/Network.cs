using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public enum RequestEncodingType
    {
        JSON,
        FormUrlEncoded,
    }
    
    public class Network : Entity
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle 
        
        public Step Initialize 
        => 
            new Step(name   : "initialize_network"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                    {
                        
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task<object> Get2(string                     url
                                      ,Dictionary<string, string> headers = null)
        {
            var webRequest = new NetworkRequest();
            
            return default;
        }
        
        public async Task<object> Get(string                     url
                                     ,Dictionary<string, string> headers = null)
        {
            // Create request
            var request = UnityWebRequest.Get(url);
            
            // Set headers
            if (headers != null)
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value.ToString());
            
            // Send request
            var op = request.SendWebRequest();
            Debug.Log($">>>".Color(Color.yellow)+$" ({request.method}){request.url}");

            
            // Await request and handle timeout
            DateTime starTime = DateTime.Now;
            while (!op.isDone)
            {
                if (DateTime.Now.Subtract(starTime).TotalMilliseconds >= 5000)
                    throw new TimeoutException();
                
                await Task.Yield();
            }
            
            // handle result
            string result = request.downloadHandler.text;
            Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {request.url} \n{result}");
            return result;
        }
        
        public async Task<object> Post(string                     url
                                      ,Dictionary<string, string> headers      = null
                                      ,string                     jsonBody     = ""
                                      ,object                     body         = default
                                      ,RequestEncodingType        encodingType = RequestEncodingType.JSON
                                      ,Dictionary<string, string> formFields   = null)
        {
            // Create Request
            UnityWebRequest request = default;
            
            if (encodingType == RequestEncodingType.JSON)
            {
                // Set Body
                if (body != default)
                    jsonBody = JsonConvert.SerializeObject(body);
                
                #if UNITY_6000_0_OR_NEWER
                request = UnityWebRequest.Post(uri        : url
                                             ,postData    : jsonBody
                                             ,contentType : "application/json");
                #else
                request = UnityWebRequest.Post(uri        : url
                                             ,postData    : jsonBody);
                #endif
            }
            else if (encodingType == RequestEncodingType.FormUrlEncoded)
            {
                WWWForm form = new WWWForm();
                foreach (var field in formFields)
                    form.AddField(field.Key, field.Value);
                
                request = UnityWebRequest.Post(url, form);                                      
            }
            
            
            // Set Headers
            if (headers != null)
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value.ToString());
            
            // Send Request
            //string formattedRequest = JToken.Parse(jsonBody).ToString(Formatting.Indented);
            //Debug.Log($">>> ({request.method}){request.url}\n{formattedRequest}");
            Debug.Log($">>>".Color(Color.yellow)+$" ({request.method}){request.url}");
            var op = request.SendWebRequest();
            
            // Wait for request and Handle timeout
            DateTime starTime = DateTime.Now;
            while (!op.isDone)
            {
                if (DateTime.Now.Subtract(starTime).TotalMilliseconds >= 5000)
                    throw new TimeoutException();
                
                await Task.Yield();
            }
            // Return result
            string result          = request.downloadHandler.text;
            //string formattedResult = JToken.Parse(result).ToString(Formatting.Indented);
            //Debug.Log($"<<< ({request.responseCode}) {request.url} \n{formattedResult}");
            Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {request.url} \n{result}");
            
            return result;
        }
    }
} 
