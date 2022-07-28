using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCMS.Core.ComponentModel;
using DCMS.Core.Configuration;
using DCMS.Core.Redis;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;

namespace DCMS.Core.Caching
{
	/// <summary>
	/// 内存缓存管理器
	/// </summary>
	public partial class MemoryCacheManager : IRedLocker, IStaticCacheManager

	{
		private bool _disposed;


		private readonly IMemoryCache _memoryCache;

		private static readonly ConcurrentDictionary<string, CancellationTokenSource> _prefixes = new ConcurrentDictionary<string, CancellationTokenSource>();
		private static CancellationTokenSource _clearToken = new CancellationTokenSource();

		public MemoryCacheManager(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		private MemoryCacheEntryOptions PrepareEntryOptions(CacheKey key)
		{
			//set expiration time for the passed cache key
			var options = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
			};

			//add tokens to clear cache entries
			options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));
			foreach (var keyPrefix in key.Prefixes.ToList())
			{
				var tokenSource = _prefixes.GetOrAdd(keyPrefix, new CancellationTokenSource());
				options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
			}

			return options;
		}

		public T Get<T>(CacheKey key, Func<T> acquire, bool force = false)
		{
			if (key.CacheTime <= 0)
			{
				return acquire();
			}

			//:“Cache entry must specify a value for Size when SizeLimit is set.”
			var result = _memoryCache.GetOrCreate(key.Key, entry =>
			{
				var options = PrepareEntryOptions(key);
				entry.SetOptions(options);
				return acquire();
			});

			//do not cache null value
			if (result == null)
			{
				Remove(key);
			}

			return result;
		}

		public void Remove(CacheKey key)
		{
			_memoryCache.Remove(key.Key);
		}


		public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire, bool force = false)
		{
			if (key.CacheTime <= 0)
			{
				return await acquire();
			}

			var result = await _memoryCache.GetOrCreateAsync(key.Key, async entry =>
			{
				entry.SetOptions(PrepareEntryOptions(key));

				return await acquire();
			});

			//do not cache null value
			if (result == null)
			{
				Remove(key);
			}

			return result;
		}

		public void Set(CacheKey key, object data)
		{
			if (key.CacheTime <= 0 || data == null)
			{
				return;
			}

			_memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
		}

		public bool IsSet(CacheKey key)
		{
			return _memoryCache.TryGetValue(key.Key, out _);
		}

		public void Remove(string key)
		{
			_memoryCache.Remove(key);
		}

		public void RemoveByPrefix(string prefix)
		{
			var keys = _memoryCache.GetKeys<string>();
			var regex = new Regex(prefix,RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var matchesKeys = keys.Select(p => p.ToString()).Where(key => regex.IsMatch(key)).ToList();

			if (!matchesKeys.Any())
				return;

			foreach (var key in matchesKeys)
			{
				_memoryCache.Remove(key);
				//_prefixes.TryRemove(key, out var tokenSource);
				//tokenSource?.Cancel();
				//tokenSource?.Dispose();
			}

		}


		public void Clear()
		{
			_clearToken.Cancel();
			_clearToken.Dispose();
			_clearToken = new CancellationTokenSource();
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
				_memoryCache.Dispose();
			}

			_disposed = true;
		}

		public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
		{
			//ensure that lock is acquired
			if (IsSet(new CacheKey(key)))
			{
				return false;
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);

				//perform action
				action();

				return true;
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}

		public bool PerformActionWithLock(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Action action)
		{
			if (IsSet(new CacheKey(key)))
			{
				return false;
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);

				//perform action
				action();

				return true;
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}

		public Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, Func<bool> action)
		{
			if (IsSet(new CacheKey(key)))
			{
				return Task.FromResult(false);
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);

				//perform action
				action();

				return Task.FromResult(true);
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}

		/// <summary>
		/// 执行带锁操作
		/// </summary>
		/// <param name="key"></param>
		/// <param name="expirationTime">过期间隔</param>
		/// <param name="wait">等待间隔</param>
		/// <param name="retry">重试次数</param>
		/// <param name="action"></param>
		/// <returns></returns>
		public Task<bool> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
		{
			if (IsSet(new CacheKey(key)))
			{
				return Task.FromResult(false);
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);

				//perform action
				action();

				return Task.FromResult(true);
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}

		/// <summary>
		/// 执行带锁操作
		/// </summary>
		/// <param name="key"></param>
		/// <param name="expirationTime">过期间隔</param>
		/// <param name="wait">等待间隔</param>
		/// <param name="retry">重试次数</param>
		/// <param name="action"></param>
		/// <returns></returns>
		public Task<BaseResult> PerformActionWithLockAsync(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<BaseResult> action)
		{
			if (IsSet(new CacheKey(key)))
			{
				return Task.FromResult(new BaseResult());
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);
				return Task.FromResult(action?.Invoke());
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}

		/// <summary>
		/// 执行带锁操作
		/// </summary>
		/// <param name="key"></param>
		/// <param name="expirationTime">过期间隔</param>
		/// <param name="wait">等待间隔</param>
		/// <param name="retry">重试次数</param>
		/// <param name="action"></param>
		/// <returns></returns>
		public bool PerformActionWithLock(string key, TimeSpan expirationTime, TimeSpan wait, TimeSpan retry, Func<bool> action)
		{
			if (IsSet(new CacheKey(key)))
			{
				return false;
			}

			try
			{
				_memoryCache.Set(key, key, expirationTime);
				return action.Invoke();
			}
			finally
			{
				//release lock even if action fails
				Remove(key);
			}
		}
	}



	public static class MemoryCacheExtensions
	{
		private static readonly Func<MemoryCache, object> GetEntriesCollection = Delegate.CreateDelegate(
			typeof(Func<MemoryCache, object>),
			typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
			throwOnBindFailure: true) as Func<MemoryCache, object>;

		public static IEnumerable GetKeys(this IMemoryCache memoryCache) => ((IDictionary)GetEntriesCollection((MemoryCache)memoryCache)).Keys;

		public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) =>
			GetKeys(memoryCache).OfType<T>();

		public static void Clear(this IMemoryCache memoryCache) => ((IDictionary)GetEntriesCollection((MemoryCache)memoryCache)).Clear();
	}
}