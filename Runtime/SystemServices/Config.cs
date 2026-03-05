using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System.Net;
using Galleon.Checkout.Foundation;

namespace Galleon.Checkout
{
    public class Config : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public EntityDictionary<string, ConfigValue> ConfigData = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecyclew
        
        public Step Initialize()
        => 
            new Step(name   : "initialize_config"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        s.AddChildStep(CHECKOUT.Globals.PopulateGlobalConfigValues());
                        s.AddChildStep(InitializeConfigFromServer());
                        s.AddChildStep(InitializeConfigFromLocal());
                    });
        
        public Step InitializeConfigFromLocal() 
        =>
            new Step(name   : $"initialize_config_from_local"
                    ,action : async (s) =>
                    {
                        // Add all dynamic values
                        foreach (var pair in CHECKOUT.Globals.CheckoutInitConfiguration.Config)
                        {
                            this.SetValue(pair.Key, pair.Value);
                        }
                    });
        
        public Step InitializeConfigFromServer() 
        =>
            new Step(name   : $"initialize_config_from_server"
                    ,action : async (s) =>
                    {
                        var result = await CHECKOUT.Network.Post(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/config"
                                                                ,headers  : new ()
                                                                          {
                                                                              { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                          }
                                                                ,body     : new 
                                                                          {
                                                                              device_ip           = CHECKOUT.Network.deviceIP,
                                                                              device_platform     = Application.platform.ToString(),
                                                                              os                  = SystemInfo.operatingSystem,
                                                                              app_version         = Application.version,
                                                                              galleon_sdk_version = "1.0.0",
                                                                              timezone            = System.TimeZone.CurrentTimeZone.StandardName
                                                                          });
                        
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
                        
                        foreach (var pair in dictionary)
                        {
                            var configValue = new ConfigValue(key:pair.Key, value:pair.Value);
                            this.SetValue(configValue);
                        }
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public void SetValue(string key, object value)
        {
            var configValue = new ConfigValue(key, value);
            SetValue(configValue);
        }
        public void SetValue(ConfigValue configValue)
        {
            if (this.ConfigData.ContainsKey(configValue.Key))
                this.ConfigData[configValue.Key].Value = configValue.Value;
            else
                this.ConfigData[configValue.Key] = configValue;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Value Methods
        
        public string GetString(string key, string defaultValue = ""   ) => ConfigData.ContainsKey(key) ? ConfigData[key].Value.ToString()              : defaultValue;
        public bool   GetBool  (string key, bool   defaultValue = false) => ConfigData.ContainsKey(key) ? bool .Parse(ConfigData[key].Value.ToString()) : defaultValue;
        public int    GetInt   (string key, int    defaultValue = 0    ) => ConfigData.ContainsKey(key) ? int  .Parse(ConfigData[key].Value.ToString()) : defaultValue;
        public float  GetFloat (string key, float  defaultValue = 0    ) => ConfigData.ContainsKey(key) ? float.Parse(ConfigData[key].Value.ToString()) : defaultValue;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        public void SetOverrideValue(string key, object value)
        {
            var trimmedKey = key.Trim();
            if (!ConfigData.ContainsKey(trimmedKey))
                ConfigData.Add(trimmedKey, new ConfigValue(trimmedKey, value));
            else
                ConfigData[trimmedKey].OverrideValue(value);
        }
        public void ClearOverrideValue(string key)
        {
            var trimmedKey = key.Trim();
            if (ConfigData.ContainsKey(trimmedKey))
                ConfigData[trimmedKey].ClearOverrideValue();
        }
        
    }
    
    public class ConfigValue : Entity
    {
        //////////////////////////////////////////////////// Types
        
        public class PossibleValue { public string DisplayName; public  object Value; }
        
        //////////////////////////////////////////////////// Members
        
        public  string              Key;
        private object              _value; 
        public  object              valueOverride   = null;
        
        public string               displayName     = "";
        public List<PossibleValue>  possibleValues  = new();
        
        public string               tag             = "";
        
        //////////////////////////////////////////////////// Properties
        
        public object               Value
        {
            get => valueOverride != null ? valueOverride : _value;
            set => this._value = value;
        }

        //////////////////////////////////////////////////// Lifecycle

        public ConfigValue(string key, object value = null)
        {
            this.Key    = key;
            this._value = value;
        }
        
        //////////////////////////////////////////////////// ToString
        
        override public string ToString() => $"ConfigValue | {this.Key} = {this.Value}";
        
        //////////////////////////////////////////////////// Value Properties
        
        public bool   GetsBool                        => bool .Parse(Value.ToString());
        public int    GetsInt                         => int  .Parse(Value.ToString());
        public float  GetsFloat                       => float.Parse(Value.ToString());
        public string GetsString                      => Value.ToString();
        public T      Get<T>() where T : IConvertible => (T)Convert.ChangeType(Value, typeof(T));
        
        //////////////////////////////////////////////////// Methods
        
        public void OverrideValue(object value)
        {
            this.valueOverride = value;
        }

        public void ClearOverrideValue()
        {
            this.valueOverride = null;
        }
    }
}
