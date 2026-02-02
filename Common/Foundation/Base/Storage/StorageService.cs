using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout.Foundation
{
    public class StorageService : Entity
    {
        private const string ALL_KEYS_LIST = "storage_service_ALL_KEYS_LIST";

        public void Store(string key, object value)
        {
            try
            {
                PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));

                // Update global keys list
                var allKeys = GetAllStoredKeys();
                if (!allKeys.Contains(key))
                {
                    allKeys.Add(key);
                    PlayerPrefs.SetString(ALL_KEYS_LIST, JsonConvert.SerializeObject(allKeys));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public T Load<T>(string key)
        {
            return (T)Load(key);
        }

        public object Load(string key)
        {
            var json = PlayerPrefs.GetString(key, null);
            if (json == null) return null;
            return JsonConvert.DeserializeObject(json);
        }
                
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
                

        public List<string> GetStoredKeys(string keysListKey)
        {
            try
            {
                var json = PlayerPrefs.GetString(keysListKey, null);
                if (json == null) return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return new List<string>();
            }
        }

        public List<string> GetAllStoredKeys()
        {
            try
            {
                var json = PlayerPrefs.GetString(ALL_KEYS_LIST, null);
                if (json == null) return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return new List<string>();
            }
        }

        ///////////////////////////////
        
        public Step DumpStorage() 
        =>
            new Step(name   : $"dump_storage"
                    ,action : async (s) =>
                              {
                                  var allKeys = GetAllStoredKeys();
                                  
                                  foreach (var key in allKeys)
                                  {
                                      Debug.Log($"{key} =\n{Load(key)}\n\n");
                                  }
                              });
    }
}