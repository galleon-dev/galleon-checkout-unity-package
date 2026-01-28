#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Galleon.Checkout
{
    public class EditorSessionState : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts

        public  const string KEY_PREFIX     = "checkout_";
        private const string SAVED_KEYS_KEY = "checkout_saved_keys";

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Step Initialize()
        =>
            new Step(name   : "initialize_editor_session_state"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API

        public void Write<T>(string key, T value)
        {
            string fullKey = $"{KEY_PREFIX}{key}";
            string json    = JsonConvert.SerializeObject(value);
            SessionState.SetString(fullKey, json);

            // Add key to saved keys list
            AddKeyToSavedList(fullKey);
        }

        public T Read<T>(string key)
        {
            string fullKey = $"{KEY_PREFIX}{key}";
            string json = SessionState.GetString(fullKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
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
            string json = SessionState.GetString(SAVED_KEYS_KEY, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
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
                SessionState.EraseString(key);
            }

            // Clear the saved keys list
            SessionState.EraseString(SAVED_KEYS_KEY);
        }

        public bool HasKey<T>(string key)
        {
            string fullKey = $"{KEY_PREFIX}{key}";
            string json = SessionState.GetString(fullKey, string.Empty);
            return !string.IsNullOrEmpty(json);
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
                SessionState.SetString(SAVED_KEYS_KEY, json);
            }
        }
    }
}
#endif
