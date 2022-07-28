using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Security;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class SecurityCacheEventConsumer :
        //AclRecord
        IConsumer<EntityInsertedEvent<AclRecord>>,
         IConsumer<EntityUpdatedEvent<AclRecord>>,
         IConsumer<EntityDeletedEvent<AclRecord>>,

        //Module
        IConsumer<EntityInsertedEvent<Module>>,
         IConsumer<EntityUpdatedEvent<Module>>,
         IConsumer<EntityDeletedEvent<Module>>,

        //ModuleRole
        IConsumer<EntityInsertedEvent<ModuleRole>>,
         IConsumer<EntityUpdatedEvent<ModuleRole>>,
         IConsumer<EntityDeletedEvent<ModuleRole>>,

        //PermissionRecord
        IConsumer<EntityInsertedEvent<PermissionRecord>>,
         IConsumer<EntityUpdatedEvent<PermissionRecord>>,
         IConsumer<EntityDeletedEvent<PermissionRecord>>,

        //PermissionRecordRoles
        IConsumer<EntityInsertedEvent<PermissionRecordRoles>>,
         IConsumer<EntityUpdatedEvent<PermissionRecordRoles>>,
         IConsumer<EntityDeletedEvent<PermissionRecordRoles>>,

         //DataChannelPermission
         IConsumer<EntityInsertedEvent<DataChannelPermission>>,
         IConsumer<EntityUpdatedEvent<DataChannelPermission>>,
         IConsumer<EntityDeletedEvent<DataChannelPermission>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public SecurityCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region AclRecord
        public void HandleEvent(EntityInsertedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Module
        public void HandleEvent(EntityInsertedEvent<Module> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MODULES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Module> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MODULES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Module> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MODULES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ModuleRole
        public void HandleEvent(EntityInsertedEvent<ModuleRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ModuleRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ModuleRole> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PermissionRecord
        public void HandleEvent(EntityInsertedEvent<PermissionRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PermissionRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PermissionRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PermissionRecordRoles 
        public void HandleEvent(EntityInsertedEvent<PermissionRecordRoles> eventMessage)
        {
            //_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PK, eventMessage.Entity.StoreId));
        }

        public void HandleEvent(EntityUpdatedEvent<PermissionRecordRoles> eventMessage)
        {
            //_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PermissionRecordRoles> eventMessage)
        {
            //_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region DataChannelPermission
        public void HandleEvent(EntityInsertedEvent<DataChannelPermission> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<DataChannelPermission> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<DataChannelPermission> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, eventMessage.Entity.StoreId));
        }
        #endregion


    }
}
