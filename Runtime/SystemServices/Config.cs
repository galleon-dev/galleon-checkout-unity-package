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
        
        public Step Initialize()
        => 
            new Step(name   : "initialize_config"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        var result = await CHECKOUT.Network.Post(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/config"
                                                                ,headers  : new ()
                                                                          {
                                                                              { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                          }
                                                                ,body     : new 
                                                                          {
                                                                              device_ip           = "192.168.1.1",
                                                                              device_platform     = "ios",
                                                                              os                  = "ios_25",
                                                                              app_version         = "1.0.0",
                                                                              galleon_sdk_version = "2.0.0",
                                                                              timezone            = "America/New_York"
                                                                          });
                        
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
                        
                        foreach (var pair in dictionary)
                            this.ConfigData.Add(pair.Key, pair.Value);
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow
        
        public string GetString(string key) => ConfigData.ContainsKey(key) ? ConfigData[key].ToString()              : string.Empty;
        public bool   GetBool  (string key) => ConfigData.ContainsKey(key) ? bool .Parse(ConfigData[key].ToString()) : false;
        public int    GetInt   (string key) => ConfigData.ContainsKey(key) ? int  .Parse(ConfigData[key].ToString()) : 0;
        public float  GetFloat (string key) => ConfigData.ContainsKey(key) ? float.Parse(ConfigData[key].ToString()) : 0;
    }
}
