using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Security
{
    public class UserGroupService : BaseService, IUserGroupService
    {

        public UserGroupService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }
        public virtual IPagedList<UserGroup> GetAllUserGroups(int? store,
            string userGroupname = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = UserGroupRepository.Table;

            if (!string.IsNullOrWhiteSpace(userGroupname))
            {
                query = query.Where(c => c.GroupName.Contains(userGroupname));
            }

            if (store.HasValue && store != 0)
            {
                query = query.Where(c => c.StoreId == store.Value);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            //var userGroups = new PagedList<UserGroup>(query.ToList(), pageIndex, pageSize);

            //return userGroups;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<UserGroup>(plists, pageIndex, pageSize, totalCount);

        }


        public List<UserGroup> GetUserGroupsByStore(int? store)
        {
            var query = from c in UserGroupRepository.Table where c.StoreId == store.Value select c;
            return query.ToList();
        }

        public virtual void DeleteUserGroup(UserGroup userGroup)
        {
            if (userGroup == null)
            {
                throw new ArgumentNullException("userGroup");
            }

            var uow = UserGroupRepository.UnitOfWork;
            UserGroupRepository.Delete(userGroup);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityDeleted(userGroup);
        }


        public virtual UserGroup GetUserGroupById(int userGroupId)
        {
            if (userGroupId == 0)
            {
                return null;
            }

            return UserGroupRepository.ToCachedGetById(userGroupId);
        }


        public virtual IList<UserGroup> GetUserGroupsByIds(int[] userGroupIds)
        {
            if (userGroupIds == null || userGroupIds.Length == 0)
            {
                return new List<UserGroup>();
            }

            var query = from c in UserGroupRepository.Table
                        where userGroupIds.Contains(c.Id)
                        select c;
            var userGroups = query.ToList();
            //sort by passed identifiers
            var sortedUserGroups = new List<UserGroup>();
            foreach (int id in userGroupIds)
            {
                var userGroup = userGroups.Find(x => x.Id == id);
                if (userGroup != null)
                {
                    sortedUserGroups.Add(userGroup);
                }
            }
            return sortedUserGroups;
        }


        public virtual UserGroup GetUserGroupByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var query = from c in UserGroupRepository.Table
                        orderby c.Id
                        where c.GroupName == name
                        select c;
            var userGroup = query.FirstOrDefault();
            return userGroup;
        }

        public virtual void InsertUserGroup(UserGroup userGroup)
        {
            if (userGroup == null)
            {
                throw new ArgumentNullException("userGroup");
            }

            var uow = UserGroupRepository.UnitOfWork;
            UserGroupRepository.Insert(userGroup);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityInserted(userGroup);
        }



        public virtual void UpdateUserGroup(UserGroup userGroup)
        {
            if (userGroup == null)
            {
                throw new ArgumentNullException("userGroup");
            }

            var uow = UserGroupRepository.UnitOfWork;
            UserGroupRepository.Update(userGroup);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(userGroup);
        }

        public IList<UserGroupUser> GetUserGroupUsersByUserId(int userId)
        {
            var query = from c in UserGroupUserRepository.Table where c.User_Id == userId select c;
            return query.ToList();
        }

        public IList<UserGroupUserRole> GetUserGroupUserRoleByGroupId(int groupId)
        {
            var query = from c in UserGroupUserRoleRepository.Table where c.UserGroup_Id == groupId select c;
            return query.ToList();
        }

        public virtual void InsertUserGroupUser(int groupId, int userId)
        {
            if (userId <= 0 || groupId <= 0)
            {
                throw new ArgumentNullException("userGroupUser");
            }

            var userGroupUser = new UserGroupUser
            {
                UserGroup_Id = groupId,
                User_Id = userId
            };

            var uow = UserGroupUserRepository.UnitOfWork;
            UserGroupUserRepository.Insert(userGroupUser);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(userGroupUser);
        }

        public virtual void RemoveUserGroupUser(int groupId, int userId)
        {
            if (userId <= 0 || groupId <= 0)
            {
                throw new ArgumentNullException("userGroupUser");
            }

            var userGroupUser = UserGroupUserRepository.Table.FirstOrDefault(c => c.UserGroup_Id == groupId && c.User_Id == userId);

            var uow = UserGroupUserRepository.UnitOfWork;
            UserGroupUserRepository.Delete(userGroupUser);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(userGroupUser);
        }

        public virtual void InsertUserGroupUserRole(int groupId, int userRoleId)
        {
            if (userRoleId <= 0 || groupId <= 0)
            {
                throw new ArgumentNullException("userGroupUserRole");
            }

            var userGroupUserRole = new UserGroupUserRole
            {
                UserGroup_Id = groupId,
                UserRole_Id = userRoleId
            };

            var uow = UserGroupUserRoleRepository.UnitOfWork;
            UserGroupUserRoleRepository.Insert(userGroupUserRole);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(userGroupUserRole);
        }

        public virtual void RemoveUserGroupUserRole(int groupId, int userRoleId)
        {
            if (userRoleId <= 0 || groupId <= 0)
            {
                throw new ArgumentNullException("userGroupUserRole");
            }

            var userGroupUserRole = UserGroupUserRoleRepository.Table.FirstOrDefault(c => c.UserGroup_Id == groupId && c.UserRole_Id == userRoleId);

            var uow = UserGroupUserRoleRepository.UnitOfWork;
            UserGroupUserRoleRepository.Delete(userGroupUserRole);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(userGroupUserRole);
        }

    }
}

