using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Galleon.Checkout.NETWORK;
using Galleon.Checkout.Shared;
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
        /////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        //public string SERVER_BASE_URL = "https://localhost:4000/v1";
        //public string SERVER_BASE_URL = "https://galleon-bridge-server-production.up.railway.app";
        //public string SERVER_BASE_URL = "http://localhost:3000";
        public string SERVER_BASE_URL = "https://bridge-staging-api.galleon.so";
        
        public const int TIMEOUT_MILLISECONDS = 10000;
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string GalleonUserAccessToken = "";
      //public string GalleonUserAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdHJpcGVDdXN0b21lcklkIjoiY3VzX1NxQm9ocHZ0M2IzZ0JWIiwiYXBwSWQiOiJ0ZXN0LmFwcCIsImlhdCI6MTc1NjEyOTQzNSwiZXhwIjoxNzU2MTMzMDM1fQ.VHXbOh8fetbUHumIunpWzB-pMQBmEzLsrXrde2r06oY";
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle 

        public Step Initialize() 
        => 
            new Step(name   : "initialize_network"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        s.AddChildStep(GetUserAccessToken());
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Factory
        
        public static void SetupEndpoint(NetworkEndpoint ep, string endpointPath)
        {   
        }
        
        public static NetworkEndpoint Endpoint(string endpointPath)
        {
            return new NetworkEndpoint().setURL    (() => $"{CHECKOUT.Network?.SERVER_BASE_URL ?? "NULL"}/{endpointPath}")
                                        .setHeaders(("Authorization", () => $"Bearer {CHECKOUT.Network?.GalleonUserAccessToken ?? "NULL"}"));
                    
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step GetUserAccessToken()
        =>
            new Step(name   : "get_user_access_token"
                    ,action : async s =>
                    {   
                        return;
                        
                        var accessToken = await Post(url     : $"{SERVER_BASE_URL}/authenticate"
                                                    ,headers : new()
                                                             {
                                                               //{ "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44" }
                                                                 { "Authorization", $"Bearer {GalleonUserAccessToken}" }
                                                             }
                                                    ,body    : new
                                                             {
                                                                 app_user_id = "test_vadimski"
                                                             });
                        
                        /// Response Example :
                        /// {
                        ///     "accessToken" : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6InBheWVyIiwiaXNzIjoiR2FsbGVvbiIsImF1ZCI6InRlc3QuYXBwIn0.xdg3d7xMIVAhDm_9JpSl5MENnmWAaZDHRjApWUMj-t8",
                        ///     "appId"       : "test.app",
                        ///     "id"          : 1,
                        ///     "externalId"  : "user id 1"
                        /// }
                        
                        var response  = JsonConvert.DeserializeAnonymousType(value               : accessToken.ToString()
                                                                            ,anonymousTypeObject : new
                                                                                                 {
                                                                                                     accessToken = "", 
                                                                                                     appId       = "", 
                                                                                                     id          = 0 , 
                                                                                                     externalId  = "",
                                                                                                 });
                        var userAccessToken = response.accessToken;
                        s.Log($"Retrieved access token: {userAccessToken}");
                        
                        this.GalleonUserAccessToken = userAccessToken;
                        

                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task<T> Get<T>(string                     url
                                   ,Dictionary<string, string> headers = null)
        {
            try
            {
                var responseJson = await Get(url, headers);
                var result       = JsonConvert.DeserializeObject<T>(responseJson.ToString());
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] : {e.ToString()}");
                return default;
            }
        }
        
        public async Task<object> Get(string                     url
                                     ,Dictionary<string, string> headers = null)
        {
            // Create request
            using var request = UnityWebRequest.Get(url);
            
            // Set headers
            if (headers != null)
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value.ToString());
            
            // Send request
            var op = request.SendWebRequest();
            var endpointName = request.url.Replace(SERVER_BASE_URL, "");
            Debug.Log($">>>".Color(Color.yellow)+$" ({request.method}) {endpointName}");

            
            // Await request and handle timeout
            DateTime starTime = DateTime.Now;
            while (!op.isDone)
            {
                if (DateTime.Now.Subtract(starTime).TotalMilliseconds >= TIMEOUT_MILLISECONDS)
                    throw new TimeoutException();
                
                await Task.Yield();
            }
            
            // handle result
            string result = request.downloadHandler.text;
            try
            {
                string formattedResult = JToken.Parse(result).ToString(Formatting.Indented);
                Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {endpointName} \n{formattedResult.Color(Color.white)}");

            }
            catch (Exception e)
            {
                Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {endpointName} \n{result}");
            }
            return result;
        }
        
        public async Task<T> Post<T>(string                     url
                                    ,Dictionary<string, string> headers      = null
                                    ,string                     jsonBody     = ""
                                    ,object                     body         = default
                                    ,RequestEncodingType        encodingType = RequestEncodingType.JSON
                                    ,Dictionary<string, string> formFields   = null)
        {
            try
            {
                var responseJson = await Post(url, headers, jsonBody, body, encodingType, formFields);
                var result       = JsonConvert.DeserializeObject<T>(responseJson.ToString());
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] : {e.ToString()}");
                return default;
            }
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
                Debug.Log($"- Post request with JSON body");
                
                // Set Body
                if (body != default)
                {
                    {
                        var type = body.GetType();
                        Debug.Log($"body.type = {type.Name}");
                        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                            try { Debug.Log($"- body.{field.Name} = {field.GetValue(body)}"); } catch (Exception e) { Debug.Log($"- error serializing {field.Name}"); }
                        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                            try { Debug.Log($"- body.{prop.Name} = {prop.GetValue(body)}"); } catch (Exception e) { Debug.Log($"- error serializing {prop.Name}"); }
                            
                    }
                    
                    jsonBody = JsonConvert.SerializeObject(body);
                    Debug.Log($"- jsonBody : {jsonBody}");
                }
                else
                {
                    Debug.Log($"body is NULL");
                }
                
                // #if UNITY_6000_0_OR_NEWER
                // Debug.Log($"- creating post request UNITY 6");
                // 
                // request = UnityWebRequest.Post(uri         : url
                //                               ,postData    : jsonBody
                //                               ,contentType : "application/json");
                // #else
                Debug.Log($"- creating post request");
                
                request                 = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                byte[] jsonToSend       = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler   = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // #endif
            }
            else if (encodingType == RequestEncodingType.FormUrlEncoded)
            {
                Debug.Log($"- Post request URL encoded");
                
                WWWForm form = new WWWForm();
                foreach (var field in formFields)
                    form.AddField(field.Key, field.Value);
                
                request = UnityWebRequest.Post(url, form);                                      
            }
            
            
            // Set Headers
            if (headers != null)
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value.ToString());
                        
            // Log outgoing
            var endpointName = request.url.Replace(SERVER_BASE_URL, "");
            if (jsonBody != default)
            {
                try
                {
                    string formattedBody = JToken.Parse(jsonBody).ToString(Formatting.Indented);
                    Debug.Log($">>>".Color(Color.yellow)+$" ({request.method}) {endpointName} \n{formattedBody.Color(Color.white)}");
                }
                catch (Exception e)
                {
                    Debug.Log($">>>".Color(Color.yellow)+$" ({request.method}) {endpointName}");
                }
                
            }
            
            // Send Request
            var op = request.SendWebRequest();
            
            // Wait for request and Handle timeout
            DateTime starTime = DateTime.Now;
            while (!op.isDone)
            {
                if (DateTime.Now.Subtract(starTime).TotalMilliseconds >= TIMEOUT_MILLISECONDS)
                    throw new TimeoutException();
                
                await Task.Yield();
            }
            // Return result
            string result = request.downloadHandler.text;
            try
            {
                string formattedResult = JToken.Parse(result).ToString(Formatting.Indented);
                Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {endpointName} \n{formattedResult.Color(Color.white)}");

            }
            catch (Exception e)
            {
                Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {endpointName} \n{result}");
            }
            
            if (!request.error.IsNullOrEmpty())
                Debug.LogError(request.error);
            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"ERROR FOR NETWORK REQUEST : {url}\n{request.error}");
            
            request.Dispose();
            
            return result;
        }
    }
} 

