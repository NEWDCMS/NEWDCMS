using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Security
{
    /// <summary>
    /// 访问控制服务
    /// </summary>
    public partial class AclService : BaseService, IAclService
    {

        private readonly IWorkContext _workContext;
        public AclService(IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _workContext = workContext;
        }

        #region Methods

        /// <summary>
        /// Deletes an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual void DeleteAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
            {
                throw new ArgumentNullException("aclRecord");
            }

            var uow = AclRecordRepository.UnitOfWork;
            AclRecordRepository.Delete(aclRecord);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(aclRecord);
        }

        /// <summary>
        /// Gets an ACL record
        /// </summary>
        /// <param name="aclRecordId">ACL record identifier</param>
        /// <returns>ACL record</returns>
        public virtual AclRecord GetAclRecordById(int aclRecordId)
        {
            if (aclRecordId == 0)
            {
                return null;
            }

            return AclRecordRepository.ToCachedGetById(aclRecordId);
        }

        /// <summary>
        /// Gets ACL records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>ACL records</returns>
        public virtual IList<AclRecord> GetAclRecords<T>(T entity) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from ur in AclRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName
                        select ur;
            var aclRecords = query.ToList();
            return aclRecords;
        }


        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual void InsertAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
            {
                throw new ArgumentNullException("aclRecord");
            }

            var uow = AclRecordRepository.UnitOfWork;
            AclRecordRepository.Insert(aclRecord);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(aclRecord);
        }

        /// <summary>
        /// Inserts an ACL record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="userRoleId">User role id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertAclRecord<T>(T entity, int userRoleId) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (userRoleId == 0)
            {
                throw new ArgumentOutOfRangeException("userRoleId");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var aclRecord = new AclRecord()
            {
                EntityId = entityId,
                EntityName = entityName,
                UserRoleId = userRoleId
            };

            InsertAclRecord(aclRecord);
        }

        /// <summary>
        /// Updates the ACL record
        /// </summary>
        /// <param name="aclRecord">ACL record</param>
        public virtual void UpdateAclRecord(AclRecord aclRecord)
        {
            if (aclRecord == null)
            {
                throw new ArgumentNullException("aclRecord");
            }

            var uow = AclRecordRepository.UnitOfWork;
            AclRecordRepository.Update(aclRecord);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(aclRecord);
        }

        /// <summary>
        /// Find user role identifiers with granted access
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>User role identifiers</returns>
        public virtual int[] GetUserRoleIdsWithAccess<T>(T entity) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var key = DCMSDefaults.ACLRECORD_BY_ENTITYID_NAME_KEY.FillCacheKey(entity.StoreId, entityId, entityName);
            return _cacheManager.Get(key, () =>
            {
                var query = from ur in AclRecordRepository.Table
                            where ur.EntityId == entityId &&
                            ur.EntityName == entityName
                            select ur.UserRoleId;
                var result = query.ToArray();
                //little hack here. nulls aren't cacheable so set it to ""
                if (result == null)
                {
                    result = new int[0];
                }

                return result;
            });
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported
        {
            return Authorize(entity, _workContext.CurrentUser);
        }

        /// <summary>
        /// Authorize ACL permission
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <param name="user">User</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, User user) where T : BaseEntity, IAclSupported
        {
            if (entity == null)
            {
                return false;
            }

            if (user == null)
            {
                return false;
            }

            if (!entity.SubjectToAcl)
            {
                return true;
            }

            foreach (var role1 in user.UserRoles.Where(cr => cr.Active))
            {
                foreach (var role2Id in GetUserRoleIdsWithAccess(entity))
                {
                    if (role1.Id == role2Id)
                    {
                        //yes, we have such permission
                        return true;
                    }
                }
            }

            //no permission found
            return false;
        }
        #endregion
    }
}