using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.NETWORK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public class NetworkRequest : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        private UnityWebRequest             UnityWebRequest;
        
        public   int                        TimeoutMilliseconds = 5000;
        
        public  string                      URL                 = ""; 
        public  string                      Method              = "";
        public  Dictionary<string, string>  Headers             = new();
        public  EncodingType                EncodingType        = EncodingType.JSON;
        
        public  object                      Body                = default;
        public  object                      Response            = default;
        
        public  Type                        RequestBodyType     = default;
        public  Type                        ResponseBodyType    = default;
        
        private Dictionary<string, string> bodyForm             = new();
        private string                     bodyJson             = null;
        
        public  string                      ResponseText        = "";
        
        public  NetworkEndpoint             Endpoint            = default;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Setters
        
        private NetworkRequest SET                   (Action                             setter ) {  setter?.Invoke(); return this; }
        public  NetworkRequest setURL                (string                             url    ) => SET(() => this.URL       = url                  );
        public  NetworkRequest setMethod             (Http_Method                        method ) => SET(() => this.Method    = method.ToString()    );
        public  NetworkRequest setHeaders            (params (string, string )[]         headers) => SET(() => headers.ToList().ForEach(h => this.Headers.Add(h.Item1, h.Item2))); 
        public  NetworkRequest setHeaders            (params (string, Func<string> )[]   headers) => SET(() => headers.ToList().ForEach(h => this.Headers.Add(h.Item1, h.Item2?.Invoke()))); 
        public  NetworkRequest setHeaders            (Dictionary<string, string>         headers) => SET(() => headers.ToList().ForEach(h => this.Headers.Add(h.Key,   h.Value)));
        public  NetworkRequest setHeaders            (Dictionary<string, Func<string>>   headers) => SET(() => headers.ToList().ForEach(h => this.Headers.Add(h.Key,   h.Value?.Invoke())));
        public  NetworkRequest setBodyWWWForm        (Dictionary<string, string>         fields ) => SET(() => { fields .ToList().ForEach(x => this.bodyForm.Add(x.Key, x.Value)); this.EncodingType = EncodingType.FormUrlEncoded; });
        public  NetworkRequest setBodyJsonDirectly   (string                             json   ) => SET(() => this.bodyJson = json);
        public  NetworkRequest setBody               (object                             body   ) => SET(() => {this.Body = body; this.EncodingType = EncodingType.JSON; });
        public  NetworkRequest setRequestBodyType    (Type                               type   ) => SET(() => this.RequestBodyType  = type);
        public  NetworkRequest setRequestBodyType<T> (                                          ) => SET(() => this.RequestBodyType  = typeof(T));
        public  NetworkRequest setResponseBodyType   (Type                               type   ) => SET(() => this.ResponseBodyType = type);
        public  NetworkRequest setResponseBodyType<T>(                                          ) => SET(() => this.ResponseBodyType = typeof(T));
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task<object> SendWebRequest()
        {
            #if DEBUG
            if (Endpoint != null)
                Endpoint.AddToHistory(this);
            #endif
            
            try
            {
                ////////////////////////// Initialize Request
                
                // Create unity web request
                this.UnityWebRequest = new UnityWebRequest(url    : URL
                                                          ,method : Method);
                
                // Set headers
                foreach (var header in Headers)
                    this.UnityWebRequest.SetRequestHeader(header.Key, header.Value.ToString());
                
                // Setup
                UnityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                
                
                ////////////////////////// Set Request Body
                
                if (this.EncodingType == EncodingType.FormUrlEncoded)
                {
                    UnityWebRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                    
                    WWWForm form = new WWWForm();
                    foreach (var field in bodyForm)
                        form.AddField(field.Key, field.Value);
                
                    UnityWebRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
                    byte[] data = form.data;
                    if (data.Length == 0)
                      data = (byte[]) null;
                    if (data != null)
                      UnityWebRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(data);
                    foreach (KeyValuePair<string, string> header in form.headers)
                      UnityWebRequest.SetRequestHeader(header.Key, header.Value);
                    
                }
                else if (this.EncodingType == EncodingType.JSON)
                {
                    UnityWebRequest.SetRequestHeader("Content-Type", "application/json");
                    
                    string json = null;
                    if (this.bodyJson != null)
                        json = this.bodyJson;
                    else if (this.Body != null)
                        json = JsonConvert.SerializeObject(this.Body);
                    
                    if (json != null)
                    {
                        byte[] jsonToSend               = Encoding.UTF8.GetBytes(json);
                        UnityWebRequest.uploadHandler   = new UploadHandlerRaw(jsonToSend);
                    }
                }
                
                ////////////////////////// Log Outgoing
                
                try
                {
                    string requestBodyString = "";
                    if (EncodingType == EncodingType.FormUrlEncoded)
                    {
                        var formJson = new JObject();
                        foreach(var field in bodyForm)
                            formJson[field.Key] = field.Value;
                        requestBodyString = formJson.ToString(Formatting.Indented);
                    }
                    else if (EncodingType == EncodingType.JSON)
                    {
                        requestBodyString = bodyJson ?? (Body != null ? JsonConvert.SerializeObject(Body, Formatting.Indented) : "");
                    }

                    Debug.Log($">>>".Color(Color.yellow)+$" {Method} {URL} \n{requestBodyString.Color(Color.white)}");
                }
                catch (Exception e)
                {
                    Debug.Log($">>>".Color(Color.yellow)+$" {Method} {URL}");
                }
                
                
                ////////////////////////// Send Request
                
                // Send request
                var op = this.UnityWebRequest.SendWebRequest();
                
                // Await request and handle timeout
                DateTime starTime = DateTime.Now;
                while (!op.isDone)
                {
                    if (DateTime.Now.Subtract(starTime).TotalMilliseconds >= TimeoutMilliseconds)
                        throw new TimeoutException();
                    
                    await Task.Yield();
                }
                
                ////////////////////////// Handle errors
                
                if (this.UnityWebRequest.error != null)
                {
                    Debug.LogError(this.UnityWebRequest.error);
                    return default;
                }
                
                ////////////////////////// Handle Response
                
                // handle result
                string responseString = this.UnityWebRequest.downloadHandler.text;
                this.ResponseText     = responseString;
                
                ////////////////////////// Log incoming
                
                try
                {
                    string formattedResult = JToken.Parse(responseString).ToString(Formatting.Indented);
                    Debug.Log($"<<<".Color(Color.yellow)+$" ({UnityWebRequest.responseCode}) {URL} \n{formattedResult.Color(Color.white)}");

                }
                catch (Exception e)
                {
                    Debug.Log($"<<<".Color(Color.yellow)+$" ({UnityWebRequest.responseCode}) {URL} \n{responseString}");
                }
                
                ////////////////////////// Done
                
                // Return result
                var     resultObject  = JsonConvert.DeserializeObject<object>(responseString);   
                return  resultObject;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return default;
            }
            finally
            {
                this.UnityWebRequest.Dispose();
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
    }
}
