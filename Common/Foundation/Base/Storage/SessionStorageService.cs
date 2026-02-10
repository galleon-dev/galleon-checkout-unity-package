using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public class SessionStorageService : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts

        public  const string KEY_PREFIX     = "session_state_";
        private const string SAVED_KEYS_KEY = "session_state_saved_keys";

        private static Dictionary<string, string> _inMemoryStorage = new Dictionary<string, string>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Step Initialize()
        =>
            new Step(name   : "initialize_editor_session_state"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API

        public void Store(string key, object value)
        {
            #if UNITY_EDITOR

            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = JsonConvert.SerializeObject(value);
            UnityEditor.SessionState.SetString(fullKey, json);

            // Add key to saved keys list
            AddKeyToSavedList(fullKey);

            #else

            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = JsonConvert.SerializeObject(value);
            _inMemoryStorage[fullKey] = json;

            // Add key to saved keys list
            AddKeyToSavedList(fullKey);

            #endif

        }

        public T Load<T>(string key)
        {
            return (T)Load(key);
        }

        public object Load(string key)
        {
            #if UNITY_EDITOR

            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = UnityEditor.SessionState.GetString(fullKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject(json);
            }

            return null;

            #else

            string fullKey = $"{KEY_PREFIX}{key}";
            if (_inMemoryStorage.TryGetValue(fullKey, out string json) && !string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject(json);
            }

            return null;

            #endif

        }

        public List<string> GetStoredKeys(string keysListKey)
        {
            #if UNITY_EDITOR

            string json = UnityEditor.SessionState.GetString(keysListKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }

            return new List<string>();

            #else

            if (_inMemoryStorage.TryGetValue(keysListKey, out string json) && !string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }

            return new List<string>();

            #endif

        }

        public List<string> GetAllStoredKeys()
        {
            #if UNITY_EDITOR

            string json = UnityEditor.SessionState.GetString(SAVED_KEYS_KEY, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }

            return new List<string>();

            #else

            if (_inMemoryStorage.TryGetValue(SAVED_KEYS_KEY, out string json) && !string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }

            return new List<string>();

            #endif

        }

        public void ClearAll()
        {
            #if UNITY_EDITOR

            List<string> savedKeys = GetAllStoredKeys();

            // Remove all saved data
            foreach (string key in savedKeys)
            {
                UnityEditor.SessionState.EraseString(key);
            }

            // Clear the saved keys list
            UnityEditor.SessionState.EraseString(SAVED_KEYS_KEY);

            #else

            _inMemoryStorage.Clear();

            #endif

        }

        public bool HasKey(string key)
        {
            #if UNITY_EDITOR

            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = UnityEditor.SessionState.GetString(fullKey, string.Empty);
            return !string.IsNullOrEmpty(json);

            #else

            string fullKey = $"{KEY_PREFIX}{key}";
            return _inMemoryStorage.TryGetValue(fullKey, out string json) && !string.IsNullOrEmpty(json);

            #endif

        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// List API
        

        public List<T> LoadList<T>(string listKey)
        {
            try
            {
                #if UNITY_EDITOR

                string fullKey = $"{KEY_PREFIX}{listKey}";
                string json = UnityEditor.SessionState.GetString(fullKey, string.Empty);

                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                }

                return new List<T>();

                #else

                string fullKey = $"{KEY_PREFIX}{listKey}";
                if (_inMemoryStorage.TryGetValue(fullKey, out string json) && !string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                }

                return new List<T>();

                #endif
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                return new List<T>();
            }
        }

        public void AddToList<T>(string listKey, T item)
        {
            try
            {
                var list = LoadList<T>(listKey);
                list.Add(item);
                Store(listKey, list);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void RemoveFromList<T>(string listKey, T item)
        {
            try
            {
                var list = LoadList<T>(listKey);
                list.Remove(item);
                Store(listKey, list);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private Methods

        private void AddKeyToSavedList(string key)
        {
            #if UNITY_EDITOR

            List<string> savedKeys = GetAllStoredKeys();

            if (!savedKeys.Contains(key))
            {
                savedKeys.Add(key);
                string json = JsonConvert.SerializeObject(savedKeys);
                UnityEditor.SessionState.SetString(SAVED_KEYS_KEY, json);
            }

            #else

            List<string> savedKeys = GetAllStoredKeys();

            if (!savedKeys.Contains(key))
            {
                savedKeys.Add(key);
                string json = JsonConvert.SerializeObject(savedKeys);
                _inMemoryStorage[SAVED_KEYS_KEY] = json;
            }

            #endif

        }
    }
}
