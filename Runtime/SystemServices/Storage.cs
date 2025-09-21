using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Galleon.Checkout
{
    public class Storage : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public  const string KEY_PREFIX     = "checkout_";
        private const string SAVED_KEYS_KEY = "checkout_saved_keys";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize()
        =>
            new Step(name   : "initialize_storage"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {    
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public void Write<T>(string key, T value)
        {
            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString(fullKey, json);
            
            // Add key to saved keys list
            AddKeyToSavedList(fullKey);
            
            PlayerPrefs.Save();
        }
        
        public T Read<T>(string key)
        {   
            string fullKey = $"{KEY_PREFIX}{key}";
            
            if (PlayerPrefs.HasKey(fullKey))
            {
                string json = PlayerPrefs.GetString(fullKey);
                return JsonConvert.DeserializeObject<T>(json);
            }
            
            return default;
        }
        
        public T Read<T>()
        {   
            return Read<T>(typeof(T).FullName);
        }
        
        public List<string> GetSavedKeys()
        {
            if (PlayerPrefs.HasKey(SAVED_KEYS_KEY))
            {
                string json = PlayerPrefs.GetString(SAVED_KEYS_KEY);
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            return new List<string>();
        }
        
        public void ClearAll()
        {
            List<string> savedKeys = GetSavedKeys();
            
            // Remove all saved data
            foreach (string key in savedKeys)
            {
                PlayerPrefs.DeleteKey(key);
            }
            
            // Clear the saved keys list
            PlayerPrefs.DeleteKey(SAVED_KEYS_KEY);
            PlayerPrefs.Save();
        }
        
        public bool HasKey<T>(string key)
        {
            string fullKey = $"{KEY_PREFIX}{key}";
            return PlayerPrefs.HasKey(fullKey);
        }
        
        public bool HasKey<T>()
        {
            return HasKey<T>(typeof(T).FullName);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private Methods
        
        private void AddKeyToSavedList(string key)
        {
            List<string> savedKeys = GetSavedKeys();
            
            if (!savedKeys.Contains(key))
            {
                savedKeys.Add(key);
                string json = JsonConvert.SerializeObject(savedKeys);
                PlayerPrefs.SetString(SAVED_KEYS_KEY, json);
            }
        }
    }
}