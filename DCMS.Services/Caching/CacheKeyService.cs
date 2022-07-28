using DCMS.Core;
using DCMS.Core.Caching;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace DCMS.Services.Caching
{
    public partial class CacheKeyService : ICacheKeyService
    {
        #region Fields

        private readonly CachingSettings _cachingSettings;
        private const string HASH_ALGORITHM = "SHA1";

        #endregion

        #region Ctor

        public CacheKeyService(CachingSettings cachingSettings)
        {
            _cachingSettings = cachingSettings;
        }

        #endregion

        #region 公共

        /// <summary>
        /// 创建标识符列表的哈希和
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        protected virtual string CreateIdsHash(IEnumerable<int> ids)
        {
            var identifiers = ids.ToList();

            if (!identifiers.Any())
                return string.Empty;

            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(string.Join(", ", identifiers.OrderBy(id => id))), HASH_ALGORITHM);
        }

        /// <summary>
        /// 将对象转换为缓存参数
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected virtual object CreateCacheKeyParameters(object parameter)
        {
            return parameter switch
            {
                null => "null",
                IEnumerable<int> ids => CreateIdsHash(ids),
                IEnumerable<BaseEntity> entities => CreateIdsHash(entities.Select(e => e.Id)),
                BaseEntity entity => entity.Id,
                decimal param => param.ToString(CultureInfo.InvariantCulture),
                _ => parameter
            };
        }

        /// <summary>
        /// 创建缓存键的副本并通过设置参数填充它
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        protected virtual CacheKey FillCacheKey(CacheKey cacheKey, params object[] keyObjects)
        {
            return new CacheKey(cacheKey, CreateCacheKeyParameters, keyObjects);
        }

        #endregion

        #region 方法

        /// <summary>
        /// 创建缓存键的副本并通过设置参数填充它
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        public virtual CacheKey PrepareKey(CacheKey cacheKey, params object[] keyObjects)
        {
            return FillCacheKey(cacheKey, keyObjects);
        }

        /// <summary>
        /// 使用默认缓存时间创建缓存键的副本，并使用设置的参数填充该副本
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        public virtual CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] keyObjects)
        {
            var key = FillCacheKey(cacheKey, keyObjects);

            key.CacheTime = _cachingSettings.DefaultCacheTime;

            return key;
        }

        /// <summary>
        /// 使用较短的缓存时间创建缓存键的副本，并使用设置的参数填充该副本
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        public virtual CacheKey PrepareKeyForShortTermCache(CacheKey cacheKey, params object[] keyObjects)
        {
            var key = FillCacheKey(cacheKey, keyObjects);

            key.CacheTime = _cachingSettings.ShortTermCacheTime;

            return key;
        }

        /// <summary>
        /// 创建缓存键前缀
        /// </summary>
        /// <param name="keyFormatter"></param>
        /// <param name="keyObjects"></param>
        /// <returns></returns>
        public virtual string PrepareKeyPrefix(string keyFormatter, params object[] keyObjects)
        {
            return keyObjects?.Any() ?? false
                ? string.Format(keyFormatter, keyObjects.Select(CreateCacheKeyParameters).ToArray())
                : keyFormatter;
        }

        #endregion
    }
}
