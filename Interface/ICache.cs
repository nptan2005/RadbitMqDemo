using System;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Extension;

namespace RadbitMqDemo.Interface
{
    public interface ICache
    {
        Type GetCacheType();
        OrderedConcurrentDictionary<string, CacheData> Load();
        CacheData Reload(string key);
    }
}
