using DCMS.Core.Caching;

namespace DCMS.Services.Caching
{

    /// <summary>
    /// 用于缓存键提供服务
    /// </summary>
    public partial interface ICacheKeyService
    {
        /// <summary>
        /// 创建缓存键的副本并通过设置参数填充它
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        CacheKey PrepareKey(CacheKey cacheKey, params object[] keyObjects);

        /// <summary>
        /// 使用默认缓存时间创建缓存键的副本，并使用设置的参数填充该副本
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] keyObjects);

        /// <summary>
        /// 使用较短的缓存时间创建缓存键的副本，并使用设置的参数填充该副本
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        CacheKey PrepareKeyForShortTermCache(CacheKey cacheKey, params object[] keyObjects);

        /// <summary>
        /// 创建缓存键前缀
        /// </summary>
        /// <param name="keyFormatter"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        string PrepareKeyPrefix(string keyFormatter, params object[] keyObjects);
    }
}
