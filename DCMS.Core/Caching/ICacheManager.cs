using System;

namespace DCMS.Core.Caching
{
    /// <summary>
    /// 用于缓存管理接口
    /// </summary>
    public interface ICacheManager : IDisposable
    {
        T Get<T>(CacheKey key, Func<T> acquire);
        void Remove(CacheKey key);
        void Set(CacheKey key, object data);
        bool IsSet(CacheKey key);
        void RemoveByPrefix(string prefix);
        void Clear();
    }

}
