using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout.Foundation
{
    public class DiskStorageService : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        private const string ALL_KEYS_LIST = "storage_service_ALL_KEYS_LIST";
        private const string FILE_NAME     = "disk_storage.json";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members 
        
        private Dictionary<string, string> _data = new Dictionary<string, string>();
        private string _filePath;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle 

        public DiskStorageService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
            LoadFromDisk();
        }
        
        public Step Initialize()
        =>
            new Step(name   : "initialize_disk_storage"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API 
        
        public void Store(string key, object value)
        {
            try
            {
                _data[key] = JsonConvert.SerializeObject(value);

                // Update global keys list
                var allKeys = GetAllStoredKeys();
                if (!allKeys.Contains(key))
                {
                    allKeys.Add(key);
                    _data[ALL_KEYS_LIST] = JsonConvert.SerializeObject(allKeys);
                }

                SaveToDisk();
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
            if (!_data.ContainsKey(key)) return null;
            
            var json = _data[key];
            if (json == null) return null;
            
            return JsonConvert.DeserializeObject(json);
        }
                
        public bool HasKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Remove(string key)
        {
            try
            {
                _data.Remove(key);

                // Remove key from saved keys list
                var allKeys = GetAllStoredKeys();
                if (allKeys.Remove(key))
                {
                    _data[ALL_KEYS_LIST] = JsonConvert.SerializeObject(allKeys);
                }

                SaveToDisk();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public List<string> GetStoredKeys(string keysListKey)
        {
            try
            {
                if (!_data.ContainsKey(keysListKey)) return new List<string>();
                
                var json = _data[keysListKey];
                if (json == null) return new List<string>();
                
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return new List<string>();
            }
        }

        public List<string> GetAllStoredKeys(string beginningWith = null)
        {
            try
            {
                if (!_data.ContainsKey(ALL_KEYS_LIST)) return new List<string>();

                var json = _data[ALL_KEYS_LIST];
                if (json == null) return new List<string>();

                var result = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

                if (beginningWith != null)
                    return result.FindAll(key => key.StartsWith(beginningWith));

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return new List<string>();
            }
        }

        public void ClearAll()
        {
            _data.Clear();
            SaveToDisk();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// List API

        public List<T> LoadList<T>(string listKey)
        {
            try
            {
                if (!_data.ContainsKey(listKey)) return new List<T>();

                var json = _data[listKey];
                if (json == null) return new List<T>();

                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Debug 

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage 

        
        private void LoadFromDisk()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load disk storage: {e}");
                _data = new Dictionary<string, string>();
            }
        }

        private void SaveToDisk()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save disk storage: {e}");
            }
        }
    }
}