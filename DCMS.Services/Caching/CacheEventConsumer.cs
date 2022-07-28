using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Events;
using DCMS.Core.Infrastructure;
using DCMS.Services.Events;

namespace DCMS.Services.Caching
{
    /// <summary>
    /// 用于缓存事件消费
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract partial class CacheEventConsumer<TEntity> : IConsumer<EntityInsertedEvent<TEntity>>,
        IConsumer<EntityUpdatedEvent<TEntity>>,
        IConsumer<EntityDeletedEvent<TEntity>> where TEntity : BaseEntity
    {
        protected readonly ICacheKeyService _cacheKeyService;
        protected readonly IStaticCacheManager _cacheManager;

        protected CacheEventConsumer()
        {
            _cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
            _cacheKeyService = EngineContext.Current.Resolve<ICacheKeyService>();
        }

        /// <summary>
        /// entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        protected virtual void ClearCache(TEntity entity, EntityEventType entityEventType)
        {
            ClearCache(entity);
        }

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        protected virtual void ClearCache(TEntity entity)
        {
        }

        /// <summary>
        /// Removes items by key prefix
        /// </summary>
        /// <param name="prefixCacheKey">String key prefix</param>
        /// <param name="useStaticCache">Indicates whether to use the statistical cache</param>
        protected virtual void RemoveByPrefix(string prefixCacheKey, bool useStaticCache = true)
        {
            _cacheManager.RemoveByPrefix(prefixCacheKey);
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="cacheKey">Key of cached item</param>
        /// <param name="useStaticCache">Indicates whether to use the statistical cache</param>
        protected virtual void Remove(CacheKey cacheKey, bool useStaticCache = true)
        {
            _cacheManager.Remove(cacheKey);
        }

        /// <summary>
        /// Handle entity inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public virtual void HandleEvent(EntityInsertedEvent<TEntity> eventMessage)
        {
            var entity = eventMessage.Entity;
            ClearCache(entity, EntityEventType.Insert);
        }

        /// <summary>
        /// Handle entity updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public virtual void HandleEvent(EntityUpdatedEvent<TEntity> eventMessage)
        {
            var entity = eventMessage.Entity;

            _cacheManager.Remove(new CacheKey(entity.EntityCacheKey));
            ClearCache(eventMessage.Entity, EntityEventType.Update);
        }

        /// <summary>
        /// Handle entity deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public virtual void HandleEvent(EntityDeletedEvent<TEntity> eventMessage)
        {
            var entity = eventMessage.Entity;

            _cacheManager.Remove(new CacheKey(entity.EntityCacheKey));
            ClearCache(eventMessage.Entity, EntityEventType.Delete);
        }

        protected enum EntityEventType
        {
            Insert,
            Update,
            Delete
        }
    }
}