using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Config : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Dictionary<string, object> ConfigData = new Dictionary<string, object>();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecyclew
        
        public Step Initialize => new Step(name   : "initialize_config"
                                          ,tags   : new[] { "init"}
                                          ,action : async s =>
                                          {
                                              //var result = await CHECKOUT.Network.Get("http://localhost:5007/config");
                                              //
                                              //var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
                                              //
                                              //foreach (var pair in dictionary)
                                              //    this.ConfigData.Add(pair.Key, pair.Value);
                                          });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow
        
        public string GetString(string key) => ConfigData.ContainsKey(key) ? ConfigData[key].ToString()              : string.Empty;
        public bool   GetBool  (string key) => ConfigData.ContainsKey(key) ? bool .Parse(ConfigData[key].ToString()) : false;
        public int    GetInt   (string key) => ConfigData.ContainsKey(key) ? int  .Parse(ConfigData[key].ToString()) : 0;
        public float  GetFloat (string key) => ConfigData.ContainsKey(key) ? float.Parse(ConfigData[key].ToString()) : 0;
    }
}

