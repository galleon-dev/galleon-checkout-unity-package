using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public string SERVER_BASE_URL = "https://localhost:4000";
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string GalleonUserAccessToken = "";
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle 
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_network"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {              
                        s.ChildSteps.Add(GetUserAccessToken());
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step GetUserAccessToken()
        =>
            new Step(name   : "get_user_access_token"
                    ,action : async s =>
                    {
                        string appID  = "test.app";
                        string id     = "app-1";
                        string device = "local_unity_test_client";
                        
                        s.Log($"Getting Galleon User Access Token.");
                        s.Log($"AppID = {appID}");
                        s.Log($"ID = {id}");
                        s.Log($"Device = {device}");
                        
                        var accessToken = await Post(url  : $"{SERVER_BASE_URL}/authenticate"
                                                    ,body : new
                                                          {
                                                              AppId  = appID,
                                                              Id     = id,
                                                              Device = device,
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
              //request = UnityWebRequest.Post(uri        : url
              //                              ,postData    : jsonBody);
                
                request                 = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                byte[] jsonToSend       = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler   = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

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
            string result = request.downloadHandler.text;
            //string formattedResult = JToken.Parse(result).ToString(Formatting.Indented);
            //Debug.Log($"<<< ({request.responseCode}) {request.url} \n{formattedResult}");
            Debug.Log($"<<<".Color(Color.yellow)+$" ({request.responseCode}) {request.url} \n{result}");
            
            if (!request.error.IsNullOrEmpty())
                Debug.LogError(request.error);
            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"ERROR FOR NETWORK REQUEST : {url}");
            
            return result;
        }
        
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class NetworkRequest : Entity
    {
        public string URL;
        public string Method;
        
        public Dictionary<string, string> Headers;
        
        public string Result;
        
        public string Error;       
        public float  Timeout = 5f;
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class Endpoint : Entity
    {
        public string       URL;
        public string       Method;
        public List<string> Headers;
        
        public List<string> InputsHistory;
        public List<string> OutputsHistory;
        
    }
} 

