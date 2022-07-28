using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core.Caching
{
	public partial class CacheKey
	{
		protected string _keyFormat = "";
		public CacheKey(CacheKey cacheKey, Func<object, object> createCacheKeyParameters, params object[] keyObjects)
		{

			Init(cacheKey.Key, cacheKey.CacheTime, cacheKey.Prefixes.ToArray());

			if (!keyObjects.Any())
			{
				return;
			}

			var fats = "";
			for (var i = 1; i < keyObjects.Length; i++)
			{
				fats += "-{" + i + "}";
			}


			Key = string.Format(_keyFormat + fats, keyObjects.Select(createCacheKeyParameters).ToArray());
			for (var i = 0; i < Prefixes.Count; i++)
			{
				Prefixes[i] = string.Format(Prefixes[i], keyObjects.Select(createCacheKeyParameters).ToArray());
			}


			//Key = string.Format(_keyFormat, keyObjects.Select(createCacheKeyParameters).ToArray());
			//for (var i = 0; i < Prefixes.Count; i++)
			//	Prefixes[i] = string.Format(Prefixes[i], keyObjects.Select(createCacheKeyParameters).ToArray());
		}



		public CacheKey(string cacheKey, int? cacheTime = null, params string[] prefixes)
		{
			Init(cacheKey, cacheTime, prefixes);
		}
		public CacheKey(string cacheKey, params string[] prefixes)
		{
			Init(cacheKey, null, prefixes);
		}

		protected void Init(string cacheKey, int? cacheTime = null, params string[] prefixes)
		{
			//PERMISSIONS_PK.{0}
			//"USER_PK.438-7-5483&USER_PK.438-7"
			//USER_PK.{0}-7

			Key = cacheKey;
			_keyFormat = cacheKey;

			if (cacheTime.HasValue)
			{
				CacheTime = cacheTime.Value;
			}

			if (prefixes.Any())
			{
				Prefixes.AddRange(prefixes.Where(prefix => !string.IsNullOrEmpty(prefix)));
				Key = string.Join("-", Prefixes);
			}
			else
			{
				Prefixes.Add(cacheKey);
			}
		}

		public string Key { get; protected set; }

		/// <summary>
		/// 按前缀功能删除
		/// </summary>
		public List<string> Prefixes { get; protected set; } = new List<string>();

		/// <summary>
		/// 缓存时间（分钟）
		/// </summary>
		public int CacheTime { get; set; } = DCMSCachingDefaults.CacheTime;
	}
}
