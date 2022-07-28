using System;
using System.Threading.Tasks;

namespace DCMS.Core.Caching
{

    /// <summary>
    /// 表示用于在HTTP请求之间进行缓存的管理器（长期缓存） 3.0 添加 ICacheKeyService， 移除了 ICacheManager 和 PerRequestCacheManager
    /// </summary>
    public interface IStaticCacheManager : IDisposable
    {
        T Get<T>(CacheKey key, Func<T> acquire, bool force = false);
        Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire, bool force = false);
        void Remove(CacheKey key);
        void Set(CacheKey key, object data);
        bool IsSet(CacheKey key);
        void RemoveByPrefix(string prefix);
        void Clear();
    }
}