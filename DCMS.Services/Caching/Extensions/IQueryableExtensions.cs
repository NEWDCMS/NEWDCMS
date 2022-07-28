using DCMS.Core.Caching;
using DCMS.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Caching.Extensions
{
    /// <summary>
    /// 用于缓存查询扩展
    /// </summary>
    public static class IQueryableExtensions
    {
        private static IStaticCacheManager CacheManager => EngineContext.Current.Resolve<IStaticCacheManager>();

        /// <summary>
        /// 获取缓存列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static IList<T> ToCachedList<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null ? query.ToList() : CacheManager.Get(cacheKey, query.ToList);
        }

        /// <summary>
        /// 获取缓存列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static T[] ToCachedArray<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null ? query.ToArray() : CacheManager.Get(cacheKey, query.ToArray);
        }

        /// <summary>
        /// 获取序列的第一个缓存元素或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static T ToCachedFirstOrDefault<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null
                ? query.FirstOrDefault()
                : CacheManager.Get(cacheKey, query.FirstOrDefault);
        }

        /// <summary>
        /// 仅获取序列的元素，如果序列中没有一个元素，则引发异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static T ToCachedSingle<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null
                ? query.Single()
                : CacheManager.Get(cacheKey, query.Single);
        }

        /// <summary>
        /// 获取一个缓存值，该值确定序列是否包含任何元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static bool ToCachedAny<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null
                ? query.Any()
                : CacheManager.Get(cacheKey, query.Any);
        }

        /// <summary>
        /// 获取序列中缓存的元素数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public static int ToCachedCount<T>(this IQueryable<T> query, CacheKey cacheKey)
        {
            return cacheKey == null
                ? query.Count()
                : CacheManager.Get(cacheKey, query.Count);
        }
    }
}
