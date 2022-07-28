using DCMS.Core.ComponentModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace DCMS.Core.Caching
{
    /// <summary>
    /// 表示在HTTP请求期间用于缓存的管理器（短期缓存）
    /// </summary>
    public partial class PerRequestCacheManager : ICacheManager
    {
        #region Fields

        private bool _disposed;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReaderWriterLockSlim _locker;


        #endregion

        public PerRequestCacheManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            _locker = new ReaderWriterLockSlim();
        }

        protected virtual IDictionary<object, object> GetItems()
        {
            return _httpContextAccessor.HttpContext?.Items;
        }




        public virtual T Get<T>(CacheKey key, Func<T> acquire)
        {
            IDictionary<object, object> items;

            if (_disposed)
            {
                return acquire();
            }

            using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.Read))
            {
                items = GetItems();
                if (items == null)
                {
                    return acquire();
                }

                //item already is in cache, so return it
                if (items[key.Key] != null)
                {
                    try
                    {
                        return (T)items[key.Key];
                    }
                    catch (Exception)
                    {
                        return acquire();
                    }
                }
            }

            //or create it using passed function
            var result = acquire();

            if (result == null || key.CacheTime <= 0)
            {
                return result;
            }

            //and set in cache (if cache time is defined)
            using (new ReaderWriteLockDisposable(_locker))
            {
                items[key.Key] = result;
            }

            return result;
        }


        public virtual void Set(CacheKey key, object data)
        {
            if (data == null)
                return;

            using (new ReaderWriteLockDisposable(_locker))
            {
                var items = GetItems();
                if (items == null)
                    return;

                items[key.Key] = data;
            }
        }


        public virtual bool IsSet(CacheKey key)
        {
            using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.Read))
            {
                var items = GetItems();
                return items?[key.Key] != null;
            }
        }

        public virtual void Remove(CacheKey key)
        {
            using (new ReaderWriteLockDisposable(_locker))
            {
                var items = GetItems();
                items?.Remove(key.Key);
            }
        }

        public virtual void RemoveByPrefix(string prefix)
        {
            using (new ReaderWriteLockDisposable(_locker, ReaderWriteLockType.UpgradeableRead))
            {
                var items = GetItems();
                if (items == null)
                {
                    return;
                }

                //get cache keys that matches pattern
                var regex = new Regex(prefix,
                    RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matchesKeys = items.Keys.Select(p => p.ToString()).Where(key => regex.IsMatch(key)).ToList();

                if (!matchesKeys.Any())
                {
                    return;
                }

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


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _locker?.Dispose();
            }

            _disposed = true;
        }
    }
}