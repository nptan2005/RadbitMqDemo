using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Extension;
using RadbitMqDemo.Interface;

namespace RadbitMqDemo.Global
{
    public static class CacheBase
    {
        private static readonly ConcurrentDictionary<string, OrderedConcurrentDictionary<string, CacheData>>
            CacheDictionary = new ConcurrentDictionary<string, OrderedConcurrentDictionary<string, CacheData>>();

        private static readonly Dictionary<string, ICache> ControllerDictionary = new Dictionary<string, ICache>();


        public static bool Inject(ICache controller)
        {
            try
            {
                string guid = GetClassGuid(controller.GetCacheType());
                if (ControllerDictionary.ContainsKey(guid)
                    || CacheDictionary.TryAdd(guid, controller.Load()) == false)
                {
                    return false;
                }

                ControllerDictionary.Add(guid, controller);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Reload<T>(string key = null) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            if (ControllerDictionary.ContainsKey(guid) == false)
            {
                return false;
            }

            OrderedConcurrentDictionary<string, CacheData> dictionary;
            ICache controller = ControllerDictionary[guid];
            if (string.IsNullOrWhiteSpace(key))
            {
                return CacheDictionary.TryRemove(guid, out dictionary)
                       && CacheDictionary.TryAdd(guid, controller.Load());
            }

            if (CacheDictionary.TryGetValue(guid, out dictionary) == false
                || dictionary.ContainsKey(key) && dictionary.TryRemove(key, out CacheData item) == false)
            {
                return false;
            }

            item = controller.Reload(key);
            return item != null && dictionary.TryAdd(key, item);
        }

        public static bool Insert<T>(string key, T value) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            return CacheDictionary.TryGetValue(guid, out dictionary) && dictionary.TryAdd(key, value);
        }

        public static bool Update<T>(string key, T value) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            if (CacheDictionary.TryGetValue(guid, out dictionary) == false)
            {
                return false;
            }

            CacheData item;
            if (dictionary.ContainsKey(key) && dictionary.TryRemove(key, out item) == false)
            {
                return false;
            }

            return dictionary.TryAdd(key, value);
        }

        public static bool Remove<T>(int index) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            if (CacheDictionary.TryGetValue(guid, out dictionary) == false)
            {
                return false;
            }

            if (index < 0 || index >= dictionary.SortKeys.Count)
            {
                return true;
            }

            CacheData item;
            return dictionary.TryRemove(dictionary.SortKeys[index], out item);
        }

        public static bool Remove<T>(string key) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            if (CacheDictionary.TryGetValue(guid, out dictionary) == false)
            {
                return false;
            }

            CacheData item;
            return dictionary.ContainsKey(key) == false || dictionary.TryRemove(key, out item);
        }

        public static T Receive<T>(string key) where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            if (CacheDictionary.TryGetValue(guid, out dictionary) == false)
            {
                return null;
            }

            return dictionary.TryGetValue(key, out CacheData item)
                ? item as T
                : null;
        }

        public static List<T> Receive<T>() where T : CacheData
        {
            string guid = GetClassGuid(typeof(T));
            List<T> listResult = new List<T>();
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            if (CacheDictionary.TryGetValue(guid, out dictionary))
            {
                listResult.AddRange(dictionary.SortKeys.Select(key => dictionary[key] as T));
            }
            return listResult;
        }

        public static T Find<T>(string fieldName, string fieldValue) where T : CacheData
        {
            Type type = typeof(T);
            string guid = GetClassGuid(type);
            OrderedConcurrentDictionary<string, CacheData> dictionary;
            return CacheDictionary.TryGetValue(guid, out dictionary)
                ? dictionary.Values.Cast<T>()
                    .FirstOrDefault(
                        item => string.Equals(type.GetField(fieldName)?.GetValue(item).ToString(), fieldValue)
                                || string.Equals(type.GetProperty(fieldName)?.GetValue(item).ToString(), fieldValue))
                : null;
        }

        private static string GetClassGuid(Type type)
        {
            return type.GUID.ToString("N");
        }
    }
}
