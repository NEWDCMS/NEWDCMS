using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class UsersCacheEventConsumer :
        //Branch
        IConsumer<EntityInsertedEvent<Branch>>,
         IConsumer<EntityUpdatedEvent<Branch>>,
         IConsumer<EntityDeletedEvent<Branch>>,

        //ExternalAuthenticationRecord
        IConsumer<EntityInsertedEvent<ExternalAuthenticationRecord>>,
         IConsumer<EntityUpdatedEvent<ExternalAuthenticationRecord>>,
         IConsumer<EntityDeletedEvent<ExternalAuthenticationRecord>>,

        //User
        IConsumer<EntityInsertedEvent<User>>,
         IConsumer<EntityUpdatedEvent<User>>,
         IConsumer<EntityDeletedEvent<User>>,

        //UserUserRole
        IConsumer<EntityInsertedEvent<UserUserRole>>,
         IConsumer<EntityUpdatedEvent<UserUserRole>>,
         IConsumer<EntityDeletedEvent<UserUserRole>>,

        //UserPassword
        IConsumer<EntityInsertedEvent<UserPassword>>,
         IConsumer<EntityUpdatedEvent<UserPassword>>,
         IConsumer<EntityDeletedEvent<UserPassword>>,

        //UserGroupUser
        IConsumer<EntityInsertedEvent<UserGroupUser>>,
         IConsumer<EntityUpdatedEvent<UserGroupUser>>,
         IConsumer<EntityDeletedEvent<UserGroupUser>>,

        //UserAttribute
        IConsumer<EntityInsertedEvent<UserAttribute>>,
         IConsumer<EntityUpdatedEvent<UserAttribute>>,
         IConsumer<EntityDeletedEvent<UserAttribute>>,

        //UserAttributeValue
        IConsumer<EntityInsertedEvent<UserAttributeValue>>,
         IConsumer<EntityUpdatedEvent<UserAttributeValue>>,
         IConsumer<EntityDeletedEvent<UserAttributeValue>>,

        //UserDistricts
        IConsumer<EntityInsertedEvent<UserDistricts>>,
         IConsumer<EntityUpdatedEvent<UserDistricts>>,
         IConsumer<EntityDeletedEvent<UserDistricts>>,

        //UserGroup
        IConsumer<EntityInsertedEvent<UserGroup>>,
         IConsumer<EntityUpdatedEvent<UserGroup>>,
         IConsumer<EntityDeletedEvent<UserGroup>>,

        //UserGroupUserRole
        IConsumer<EntityInsertedEvent<UserGroupUserRole>>,
         IConsumer<EntityUpdatedEvent<UserGroupUserRole>>,
         IConsumer<EntityDeletedEvent<UserGroupUserRole>>,

        //UserRole
        IConsumer<EntityInsertedEvent<UserRole>>,
         IConsumer<EntityUpdatedEvent<UserRole>>,
         IConsumer<EntityDeletedEvent<UserRole>>
    {
        protected readonly IStaticCacheManager _cacheManager;

        public UsersCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Branch
        public void HandleEvent(EntityInsertedEvent<Branch> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BINDBRANCH_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Branch> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BINDBRANCH_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Branch> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BINDBRANCH_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ExternalAuthenticationRecord
        public void HandleEvent(EntityInsertedEvent<ExternalAuthenticationRecord> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ExternalAuthenticationRecord> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ExternalAuthenticationRecord> eventMessage)
        {
        }
        #endregion

        #region User
        public void HandleEvent(EntityInsertedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region UserUserRole
        public void HandleEvent(EntityInsertedEvent<UserUserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<UserUserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<UserUserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region UserPassword
        public void HandleEvent(EntityInsertedEvent<UserPassword> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserPassword> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserPassword> eventMessage)
        {
        }
        #endregion

        #region UserGroupUser
        public void HandleEvent(EntityInsertedEvent<UserGroupUser> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserGroupUser> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserGroupUser> eventMessage)
        {
        }
        #endregion

        #region UserAttribute
        public void HandleEvent(EntityInsertedEvent<UserAttribute> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserAttribute> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserAttribute> eventMessage)
        {
        }
        #endregion

        #region UserAttributeValue
        public void HandleEvent(EntityInsertedEvent<UserAttributeValue> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserAttributeValue> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserAttributeValue> eventMessage)
        {
        }
        #endregion

        #region UserDistricts
        public void HandleEvent(EntityInsertedEvent<UserDistricts> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<UserDistricts> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<UserDistricts> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region UserGroup
        public void HandleEvent(EntityInsertedEvent<UserGroup> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserGroup> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserGroup> eventMessage)
        {
        }
        #endregion

        #region UserGroupUserRole
        public void HandleEvent(EntityInsertedEvent<UserGroupUserRole> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UserGroupUserRole> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UserGroupUserRole> eventMessage)
        {
        }
        #endregion

        #region UserRole
        public void HandleEvent(EntityInsertedEvent<UserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<UserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<UserRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, eventMessage.Entity.StoreId));
        }
        #endregion
    }
}
