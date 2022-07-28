using DCMS.Core.Caching;
using DCMS.Core.Domain.Chat;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace DCMS.Services.Chat
{
    public class ChatUserService : BaseService, IChatUserService
    {
        private readonly IConnectionService _connectionService;

        public ChatUserService(IServiceGetter serviceGetter,
         IStaticCacheManager cacheManager,
         IConnectionService connectionService,
         IEventPublisher eventPublisher) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _connectionService = connectionService;
        }


        public User RegisterUser(User user, string connectionId)
        {
            _connectionService.AddUserToConnectedUsers(user.UserId.ToString(), connectionId);
            InsertUser(user);
            return user;
        }


        public void InsertUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var uow = ChatUserRepository.UnitOfWork;
            ChatUserRepository.Insert(user);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(user);
        }
        public void UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var uow = ChatUserRepository.UnitOfWork;
            ChatUserRepository.Update(user);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(user);
        }

        public User GetUserById(int storeId, int userId)
        {
            var user = ChatUserRepository.Table.Where(s =>s.StoreId== storeId && s.UserId == userId).FirstOrDefault();
            if (user == null)
                return null;

            return user;
        }


        public User GetUserById(int userId)
        {
            var user = ChatUserRepository.Table.Where(s => s.UserId == userId).FirstOrDefault();
            if (user == null)
                return null;

            return user;
        }

        public User LoginUser(int storeId, int userId, string connectionId, string name, string avatar, string mobileNumber, string openId)
        {

            var user = GetUserById(storeId,userId);
            if (user != null)
            {
                UpdateUser(user);
                _connectionService.AddUserToConnectedUsers(userId.ToString(), connectionId);
                return user;
            }
            else
            {
                user = new User
                {
                    Name = name,
                    Avatar = avatar,
                    MobileNumber = mobileNumber,
                    StoreId = storeId,
                    UserId = userId,
                    OpenId = openId,
                };

                _connectionService.AddUserToConnectedUsers(userId.ToString(), connectionId);

                InsertUser(user);

                return user;
            }

        }


        public bool NameValidation(string name)
        {
            User user = ChatUserRepository.Table.Where(s => s.Name == name).FirstOrDefault();
            if (user == null)
                return true;

            return false;
        }


        public void LogOut(User user)
        {
            _connectionService.RemoveUserFromConnectedUsersById(user.UserId);
        }


        public IEnumerable<User> GetConnectedUsersExceptMe(User user)
        {
            List<string> ids = _connectionService.GetConnectedUsers();

            List<User> users = new List<User>();
            foreach (string id in ids)
            {
                int.TryParse(id, out int userId);

                if (userId == user.UserId)
                    continue;

                users.Add(GetUserById(user.StoreId, userId));
            }

            return users;
        }


        public ConcurrentDictionary<string, string> CTUS ()
        {
            try
            {
                var ids = _connectionService.ConnectionIdToUser;
                return ids;
            }
            catch (Exception)
            {
                return new ConcurrentDictionary<string, string>();
            }
        }

        public ConcurrentDictionary<string, string> UTCS()
        {
            try
            {
                var ids = _connectionService.UserToConnectionId;
                return ids;
            }
            catch (Exception)
            {
                return new ConcurrentDictionary<string, string>();
            }
        }



        public IEnumerable<User> GetUsers(int? storeId)
        {
            try
            {
                var ids = _connectionService.GetConnectedUsers();
                var query = ChatUserRepository.Table;

                if (storeId.HasValue && storeId > 0)
                {
                    if (ids != null && ids.Any())
                        query = query.Where(s => s.StoreId == storeId && ids.Contains(s.UserId.ToString()));
                    else
                        query = query.Where(s => s.StoreId == storeId);

                }
                else
                {
                    query = query.Where(s => ids.Contains(s.UserId.ToString()));
                }

                return query.ToList();
            }
            catch (Exception)
            {
                return new List<User>();
            }
        }

    }
}
