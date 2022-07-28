using DCMS.Core.ComponentModel;
using DCMS.Core.Configuration;
using DCMS.Core.Redis;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DCMS.Core.Caching
{
    /// <summary>
    /// 表示用于在redis存储中缓存的管理器（http://redis.io/）。
    /// 主要用于在web场合或azure中运行，也可以用于任何服务器环境
    /// </summary>
    public partial class RedisCacheManager : IRedLocker, IStaticCacheManager
    {

        private bool _disposed;
        private readonly IDatabase _db;
        private readonly IRedisConnectionWrapper _connectionWrapper;
        private readonly DCMSConfig _config;
        private readonly PerRequestCache _perRequestCache;

        public RedisCacheManager(IHttpContextAccessor httpContextAccessor,
            IRedisConnectionWrapper connectionWrapper,
            DCMSConfig config)
        {
            //连接为空时
            if (string.IsNullOrEmpty(config.RedisConnectionString))
                throw new Exception("Redis connection string is empty");

            _config = config;

            _connectionWrapper = connectionWrapper;

            _db = _connectionWrapper.GetDatabase(config.RedisDatabaseId ?? (int)RedisDatabaseNumber.Cache);

            _perRequestCache = new PerRequestCache(httpContextAccessor);
        }


        #region 公共

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected virtual IEnumerable<RedisKey> GetKeys(EndPoint endPoint, string prefix = null)
        {
            var server = _connectionWrapper.GetServer(endPoint);

            //we can use the code below (commented), but it requires administration permission - ",allowAdmin=true"
            //server.FlushDatabase();

            var keys = server.Keys(_db.Database, string.IsNullOrEmpty(prefix) ? null : $"{prefix}*");

            var ts = keys.ToList();

            //we should always persist the data protection key list
            keys = keys.Where(key => !key.ToString().Equals(DCMSCachingDefaults.RedisDataProtectionKey,
                StringComparison.OrdinalIgnoreCase));

            return keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual async Task<T> GetAsync<T>(CacheKey key)
        {
            //性能解决方案
            if (_perRequestCache.IsSet(key.Key))
                return _perRequestCache.Get(key.Key, () => default(T));

            //get serialized item from cache
            var serializedItem = await _db.StringGetAsync(key.Key);
            if (!serializedItem.HasValue)
                return default;

            //deserialize item
            var item = JsonConvert.DeserializeObject<T>(serializedItem);
            if (item == null)
                return default;

            //set item in the per-request cache
            _perRequestCache.Set(key.Key, item);

            return item;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        protected virtual async Task SetAsync(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            //set cache time
            var expiresIn = TimeSpan.FromMinutes(cacheTime);

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data);

            //and set it to cache
            await _db.StringSetAsync(key, serializedItem, expiresIn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual async Task<bool> IsSetAsync(CacheKey key)
        {
            //性能解决方案
            if (_perRequestCache.IsSet(key.Key))
                return true;

            return await _db.KeyExistsAsync(key.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        protected virtual (bool, T) TryPerformAction<T>(Func<T> action)
        {
            try
            {
                //attempts to execute the passed function
                var rez = action();

                return (true, rez);
            }
            catch (RedisTimeoutException)
            {
                //ignore the RedisTimeoutException if specified by settings
                if (_config.IgnoreRedisTimeoutException)
                    return (false, default);

                //or rethrow the exception
                throw;
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire, bool force = false)
        {

            if (await IsSetAsync(key) && !force)
                return await GetAsync<T>(key);

            if (acquire == null)
            {
                return default(T);
            }

            //or create it using passed function
            var result = await acquire();

            //and set in cache (if cache time is defined)
            if (key.CacheTime > 0)
                await SetAsync(key.Key, result, key.CacheTime);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private T Get<T>(CacheKey key)
        {
            //性能解决方案
            if (_perRequestCache.IsSet(key.Key))
                return _perRequestCache.Get(key.Key, () => default(T));

            var (_, rez) = TryPerformAction(() =>
            {
                //get serialized item from cache
                var serializedItem = _db.StringGet(key.Key);
                if (!serializedItem.HasValue)
                    return default;

                //deserialize item
                try
                {
                    var item = JsonConvert.DeserializeObject<T>(serializedItem);
                    if (item == null)
                        return default;

                    //set item in the per-request cache
                    _perRequestCache.Set(key.Key, item);

                    return item;
                }
                catch (Exception ex)
                {
                    _db.KeyDelete(key.Key);
                    return default;
                }
                
                
            });

            return rez;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public virtual T Get<T>(CacheKey key, Func<T> acquire, bool force = false)
        {
            if (IsSet(key) && !force)
            {
                var rez = Get<T>(key);
                if (rez != null && !rez.Equals(default(T)))
                    return rez;
            }

            if (acquire == null)
            {
                return default(T);
            }

            var result = acquire();

            //and set in cache (if cache time is defined)
            if (key.CacheTime > 0)
                Set(key, result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public virtual void Set(CacheKey key, object data)
        {
            if (data == null)
                return;

            //set cache time
            var expiresIn = TimeSpan.FromMinutes(key.CacheTime);

            //serialize item
            var serializedItem = JsonConvert.SerializeObject(data);

            //and set it to cache
            TryPerformAction(() => _db.StringSet(key.Key, serializedItem, expiresIn));
            _perRequestCache.Set(key.Key, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool IsSet(CacheKey key)
        {
            if (_perRequestCache.IsSet(key.Key))
                return true;

            var (flag, rez) = TryPerformAction(() => _db.KeyExists(key.Key));

            return flag && rez;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public virtual void Remove(CacheKey key)
        {
            //
            if (key.Key.Equals(DCMSCachingDefaults.RedisDataProtectionKey, StringComparison.OrdinalIgnoreCase))
                return;

            //remove item from caches
            TryPerformAction(() => _db.KeyDelete(key.Key));
            _perRequestCache.Remove(key.Key);
        }

        /// <summary>
        /// 根据前缀清除缓存
        /// </summary>
        /// <param name="prefix">String key prefix</param>
        public virtual void RemoveByPrefix(string prefix)
        {
            _perRequestCache.RemoveByPrefix(prefix);

            foreach (var endPoint in _connectionWrapper.GetEndPoints())
            {
                var keys = GetKeys(endPoint, prefix);
                TryPerformAction(() => _db.KeyDelete(keys.ToArray()));
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public virtual void Clear()
        {
            foreach (var endPoint in _connectionWrapper.GetEndPoints())
            {
                var keys = GetKeys(endPoint).ToArray();

                //we can't use _perRequestCache.Clear(),
                //because HttpContext stores some server data that we should not delete
                foreach (var redisKey in keys)
                    _perRequestCache.Remove(redisKey.ToString());

                TryPerformAction(() => _db.KeyDelete(keys.ToArray()));
            }
        }

        /// <summary>
        /// 释放缓存管理器
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 实现释放缓存
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        #endregion

        #region RedLock

        public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
        {
            return _connectionWrapper.PerformActionWithLock(key, expirationTime, action);
        }


        public async Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, Func<bool> action)
        {
            return await _connectionWrapper.PerformActionWithLockAsync(key, expirationTime, action);
        }


        public bool PerformActionWithLock(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
        {
            return _connectionWrapper.PerformActionWithLock(key, expirationTime, wait, retry, action);
        }


        public async Task<BaseResult> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<BaseResult> action)
        {
            return await _connectionWrapper.PerformActionWithLockAsync(key, expirationTime, wait, retry, action);
        }


        public async Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
        {
            return await _connectionWrapper.PerformActionWithLockAsync(key, expirationTime, wait, retry, action);
        }

        #endregion


        #region 嵌套类

        /// <summary>
        /// 表示在HTTP请求期间用于缓存的管理器（短期缓存）
        /// </summary>
        protected class PerRequestCache
        {

            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ReaderWriterLockSlim _locker;


            public PerRequestCache(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;

                _locker = new ReaderWriterLockSlim();
            }



            protected virtual IDictionary<object, object> GetItems()
            {
                return _httpContextAccessor.HttpContext?.Items;
            }

            public virtual T Get<T>(string key, Func<T> acquire)
            {
                IDictionary<object, object> items;

                using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.Read))
                {
                    items = GetItems();
                    if (items == null)
                        return acquire();

                    //item already is in cache, so return it
                    if (items[key] != null)
                        return (T)items[key];
                }

                //or create it using passed function
                var result = acquire();

                //and set in cache (if cache time is defined)
                using (new ReaderWriteLockDisposable(_locker))
                    items[key] = result;

                return result;
            }


            public virtual void Set(string key, object data)
            {
                if (data == null)
                    return;

                using (new ReaderWriteLockDisposable(_locker))
                {
                    var items = GetItems();
                    if (items == null)
                        return;

                    items[key] = data;
                }
            }


            public virtual bool IsSet(string key)
            {
                using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.Read))
                {
                    var items = GetItems();
                    return items?[key] != null;
                }
            }


            public virtual void Remove(string key)
            {
                using (new ReaderWriteLockDisposable(_locker))
                {
                    var items = GetItems();
                    items?.Remove(key);
                }
            }


            public virtual void RemoveByPrefix(string prefix)
            {
                using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.UpgradeableRead))
                {
                    var items = GetItems();
                    if (items == null)
                        return;

                    //get cache keys that matches pattern
                    var regex = new Regex(prefix,
                        RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matchesKeys = items.Keys.Select(p => p.ToString()).Where(key => regex.IsMatch(key)).ToList();

                    if (!matchesKeys.Any())
                        return;
                    //"PERMISSIONS_PK.438-5483-BusinessManagers-TruePERMISSIONS_PK.438-13PERMISSIONS_PK.438-13"
                    using (new ReaderWriteLockDisposable(_locker))
                    {
                        //remove matching values
                        foreach (var key in matchesKeys)
                        {
                            items.Remove(key);
                        }
                    }
                }
            }

            public virtual void Clear()
            {
                using (new ReaderWriteLockDisposable(_locker))
                {
                    var items = GetItems();
                    items?.Clear();
                }
            }

        }

        #endregion
    }

}

