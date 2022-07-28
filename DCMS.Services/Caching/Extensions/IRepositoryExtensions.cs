using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Infrastructure;
using DCMS.Core.Data;

namespace DCMS.Services.Caching
{
    public static class IRepositoryExtensions
    {
        /// <summary>
        /// 获取实体缓存
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="repository"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TEntity ToCachedGetById<TEntity>(this IRepository<TEntity> repository, object id) where TEntity : BaseEntity
        {
            var cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
            return cacheManager.Get(new CacheKey(BaseEntity.GetEntityCacheKey(typeof(TEntity), id)), () => repository.GetById(id));
        }
    }
}
