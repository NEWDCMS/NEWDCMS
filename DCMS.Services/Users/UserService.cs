using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching;
using DCMS.Services.Common;
using DCMS.Services.Events;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Module = DCMS.Core.Domain.Security.Module;


namespace DCMS.Services.Users
{
	public class UserService : BaseService, IUserService
	{
		private readonly UserSettings _userSettings;

		private readonly IGenericAttributeService _genericAttributeService;

		public UserService(
			UserSettings userSettings,
			IServiceGetter getter,
			IStaticCacheManager cacheManager,

			IGenericAttributeService genericAttributeService,
			IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
		{
			_userSettings = userSettings;

			_genericAttributeService = genericAttributeService;
		}


		#region 用户

		/// <summary>
		/// 获取全部用户
		/// </summary>
		/// <param name="createdFromUtc"></param>
		/// <param name="createdToUtc"></param>
		/// <param name="userRoleIds"></param>
		/// <param name="email"></param>
		/// <param name="username"></param>
		/// <param name="realName"></param>
		/// <param name="store"></param>
		/// <param name="phone"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public virtual IPagedList<User> GetAllUsers(DateTime? createdFromUtc = null,
			DateTime? createdToUtc = null,
			int[] userRoleIds = null,
			string email = null,
			string username = null,
			string realName = null,
			int branchid = 0,
			int store = 0,
			string phone = null,
			int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;

			var query = UserRepository_RO.TableNoTracking;
			if (createdFromUtc.HasValue)
			{
				query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
			}

			if (createdToUtc.HasValue)
			{
				query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
			}

			query = query.Where(c => !c.Deleted);

			if (userRoleIds != null && userRoleIds.Length > 0)
			{
				//query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(userRoleIds).Any());
				//查询角色关联的用户
				//var query2 = UserUserRoleRepository.Table;

				var query2 = UserUserRoleRepository_RO.TableNoTracking;
				query2 = query2.Where(q => userRoleIds.Contains(q.UserRoleId));
				List<int> userIds = query2.Select(q => q.UserId).Distinct().ToList();
				//当前用户必须在角色关联的用户中
				query = query.Where(q => userIds.Contains(q.Id));
			}

			if (branchid != 0)
			{
				var queryBranchId = BranchRepository_RO.Table.Where(s => s.ParentId == branchid).Select(a => a.Id).ToList(); ;
				if (queryBranchId.Count > 0)
				{
					queryBranchId.Add(branchid);
					query = query.Where(c => queryBranchId.Contains(c.BranchId ?? 0));
				}
				else
				{
					query = query.Where(c => c.BranchId == branchid);
				}
			}

			if (store != 0)
			{
				query = query.Where(c => c.StoreId == store);
			}

			if (!string.IsNullOrWhiteSpace(email))
			{
				query = query.Where(c => c.Email.Contains(email));
			}

			if (!string.IsNullOrWhiteSpace(username))
			{
				query = query.Where(c => c.Username.Contains(username));
			}

			if (!string.IsNullOrWhiteSpace(realName))
			{
				query = query
					.Join(GaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
					.Where((z => z.Attribute.KeyGroup == "User" &&
						z.Attribute.Value.Contains(realName)))
					.Select(z => z.User);
			}

			//search by phone
			if (!string.IsNullOrWhiteSpace(phone))
			{
				query = query
					.Join(GaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
					.Where((z => z.Attribute.KeyGroup == "User" && z.Attribute.Value.Contains(phone)))
					.Select(z => z.User);
			}

			query = query.OrderByDescending(c => c.CreatedOnUtc);

			//var users = new PagedList<User>(query.ToList(), pageIndex, pageSize);
			//return users;

			//总页数
			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<User>(plists, pageIndex, pageSize, totalCount);
		}


		/// <summary>
		/// 根据密码格式获取用户
		/// </summary>
		/// <param name="passwordFormat"></param>
		/// <returns></returns>
		public virtual IList<User> GetAllUsersByPasswordFormat(PasswordFormat passwordFormat)
		{
			int passwordFormatId = (int)passwordFormat;
			var query = UserRepository.Table;
			query = query.Where(c => c.PasswordFormatId == passwordFormatId);
			query = query.OrderByDescending(c => c.CreatedOnUtc);
			var users = query.ToList();
			return users;
		}

		public virtual IList<User> GetAllUsers(int store = 0)
		{
			var query = UserRepository.Table;
			query = query.Where(c => c.StoreId == store && c.Deleted == false);
			var users = query.Select(s => s).ToList();
			return users;
		}


		public virtual IList<User> GetAllUsers(Expression<Func<User, User>> selector, int store = 0)
		{
			var query = UserRepository.Table;
			query = query.Where(c => c.StoreId == store && c.Deleted == false);
			var users = query.Select(selector).ToList();
			return users;
		}

		public virtual Dictionary<int, string> GetAllUsers(int? store, string systemName)
		{
			var dicts = new Dictionary<int, string>();
			if (store.HasValue && store > 0)
			{
				var sqlString = @" SELECT 
									u.Id as `Id` ,
									u.UserRealName as `Name` 
								FROM
									auth.Users AS u
										INNER JOIN
									auth.User_UserRole_Mapping AS urm ON u.id = urm.UserId
										INNER JOIN
									auth.UserRoles AS ur ON urm.UserRoleId = ur.id
										AND ur.SystemName = '{0}'
								WHERE
									u.StoreId = {1} group by Id;";

				dicts = UserRoleRepository_RO
					.QueryFromSql<DictType>(string.Format(sqlString, systemName, store ?? 0))
					.ToDictionary(k => k.Id, v => v.Name);
			}
			return dicts;
		}



		/// <summary>
		/// 获取在线用户
		/// </summary>
		/// <param name="lastActivityFromUtc"></param>
		/// <param name="userRoleIds"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public virtual IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc,
			int[] userRoleIds, int pageIndex, int pageSize)
		{
			if (pageSize >= 50)
				pageSize = 50;
			var query = UserRepository.Table;
			query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
			query = query.Where(c => !c.Deleted);
			if (userRoleIds != null && userRoleIds.Length > 0)
			{
				//query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(userRoleIds).Any());
				//查询角色关联的用户
				var query2 = UserUserRoleRepository.Table;
				query2 = query2.Where(q => userRoleIds.Contains(q.UserRoleId));
				List<int> userIds = query2.Select(q => q.UserId).Distinct().ToList();
				//当前用户必须在角色关联的用户中
				query = query.Where(q => userIds.Contains(q.Id));

			}

			query = query.OrderByDescending(c => c.LastActivityDateUtc);

			//var users = new PagedList<User>(query.ToList(), pageIndex, pageSize);
			//return users;

			//总页数
			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<User>(plists, pageIndex, pageSize, totalCount);
		}


		/// <summary>
		/// 删除用户
		/// </summary>
		/// <param name="user"></param>
		public virtual void DeleteUser(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			//系统用户不能被删除
			if (user.IsSystemAccount)
			{
				throw new DCMSException(string.Format("System user account ({0}) could not be deleted", user.SystemName));
			}

			user.Deleted = true;

			UpdateUser(user);
		}


		/// <summary>
		/// 根据ID获取用户
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public virtual User GetUserById(int? store, int userId, bool include = false)
		{
			if (userId == 0)
			{
				return null;
			}

			if (include)
			{
				return UserRepository.TableNoTracking.Where(u => u.StoreId == store)
					.FirstOrDefault(u => u.Id == userId);
			}
			else
			{
				return UserRepository.TableNoTracking.Where(u => u.StoreId == store)
					.FirstOrDefault(u => u.Id == userId);
			}
		}

		public virtual User GetUserById(int userId)
		{
			if (userId == 0)
			{
				return null;
			}

			return UserRepository.TableNoTracking.FirstOrDefault(u => u.Id == userId);
		}

		public virtual string GetUserName(int? store, int userId)
		{
			if (userId == 0)
			{
				return "";
			}

			return UserRepository.Table.Where(a => a.Id == userId && a.StoreId == store)
				  .Select(a => a.UserRealName)
				  .FirstOrDefault();
		}

		/// <summary>
		/// 用户最大欠款额度
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public virtual decimal GetUserMaxAmountOfArrears(int? store, int userId)
		{
			return UserRepository.TableNoTracking
				.Where(a => a.Id == userId && a.StoreId == store)
				.Select(a => a.MaxAmountOfArrears ?? 0).FirstOrDefault();
		}

		public virtual string GetUUID(int? store, int userId)
		{
			if (userId == 0)
			{
				return "";
			}

			var key = DCMSDefaults.USER_APPID_BY_ID_KEY.FillCacheKey(store ?? 0, userId);
			return _cacheManager.Get(key, () =>
		   {
			   return UserRepository.Table.Where(a => a.Id == userId && a.StoreId == store).Select(a => a.AppId).FirstOrDefault();
		   });
		}

		public virtual void UpdateUUID(int? store, int userId, string uuid)
		{
			var user = GetUserById(store, userId);
			if (user != null)
			{
				user.AppId = uuid;
				UpdateUser(user);
			}
		}

		/// <summary>
		/// 根据ID列表获取多个用户
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public virtual IList<User> GetUsersByIds(int? store, int[] userIds)
		{
			if (userIds == null || userIds.Length == 0)
			{
				return new List<User>();
			}

			var query = from c in UserRepository_RO.TableNoTracking
						where c.StoreId == store && userIds.Contains(c.Id)
						select c;
			var users = query.ToList();
			//sort by passed identifiers
			var sortedUsers = new List<User>();
			foreach (int id in userIds)
			{
				var user = users.Find(x => x.Id == id);
				if (user != null)
				{
					sortedUsers.Add(user);
				}
			}
			return sortedUsers;
		}


		/// <summary>
		/// 管理平台用
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public virtual IList<User> GetUsersByIds(int[] userIds)
		{
			if (userIds == null || userIds.Length == 0)
			{
				return new List<User>();
			}

			var query = from c in UserRepository_RO.TableNoTracking
						where userIds.Contains(c.Id)
						select c;
			var users = query.ToList();
			//sort by passed identifiers
			var sortedUsers = new List<User>();
			foreach (int id in userIds)
			{
				var user = users.Find(x => x.Id == id);
				if (user != null)
				{
					sortedUsers.Add(user);
				}
			}
			return sortedUsers;
		}

		public virtual Dictionary<int, string> GetUsersDictsByIds(int storeId, int[] userIds)
		{
			var categoryIds = new Dictionary<int, string>();
			if (userIds.Count() > 0)
			{
				categoryIds = UserRepository_RO.QueryFromSql<DictType>($"SELECT Id,UserRealName as Name FROM auth.Users where StoreId = " + storeId + " and id in(" + string.Join(",", userIds) + ");").ToDictionary(k => k.Id, v => v.Name);
			}
			return categoryIds;
		}


		public virtual IList<User> GetUsersIdsByUsersIds(int store, int[] userIds, bool platform = false)
		{
			if (userIds == null || userIds.Length == 0)
			{
				return new List<User>();
			}
			var key = DCMSDefaults.USER_GETUSER_BY_IDS_KEY.FillCacheKey(store, string.Join("_", userIds.OrderBy(a => a)));
			return _cacheManager.Get(key, () =>
			{
				var query = from c in UserRepository.Table
							where c.StoreId == store && userIds.Contains(c.Id)
							select new User { Id = c.Id, UserRealName = c.UserRealName };
				if (platform == true)
				{
					query = from c in UserRepository_RO.TableNoTracking
							where c.StoreId == store && userIds.Contains(c.Id)
							select new User { Id = c.Id, UserRealName = c.UserRealName };
				}
				var users = query.ToList();

				return users;
			});
		}



		/// <summary>
		/// 根据GUID 获取用户
		/// </summary>
		/// <param name="userGuid"></param>
		/// <returns></returns>
		public virtual User GetUserByGuid(int? store, Guid userGuid, bool noTracking = false)
		{
			return GetUserByGuidNoCache(store, userGuid, noTracking);
		}
		public virtual User GetUserByGuidNoCache(int? store, Guid userGuid, bool noTracking = false)
		{
			if (userGuid == Guid.Empty)
			{
				return null;
			}

			if (!noTracking)
			{
				var query = from c in UserRepository.Table
							where c.StoreId == store && c.UserGuid == userGuid.ToString()
							orderby c.Id
							select c;
				var user = query.Include(u => u.UserUserRoles).FirstOrDefault();
				return user;
			}
			else
			{
				User user = null;
				var query = from u in UserRepository.TableNoTracking
							where u.UserGuid == userGuid.ToString()
							orderby u.Id
							select u;

				user = query.FirstOrDefault();


				var query2 = from u in query
							 join uur in UserUserRoleRepository.TableNoTracking on u.Id equals uur.UserId
							 select uur;

				user.SetUserUserRole(query2?.ToList());


				var query3 = from u in query2
							 join ur in UserRoleRepository.TableNoTracking on u.UserRoleId equals ur.Id
							 select ur;

				user.SetUserRole(query3?.ToList());

				return user;
			}
		}


		/// <summary>
		/// 根据邮箱获取用户
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public virtual User GetUserByEmail(int? store, string email, bool noTracking = false,bool includeDelete = false)
		{
			return GetUserByEmailNoCache(store, email, noTracking, includeDelete);
		}
		public virtual User GetUserByEmailNoCache(int? store, string email, bool noTracking = false, bool includeDelete = false)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return null;
			}

			if (!noTracking)
			{
				var query = from c in UserRepository.Table
							where c.StoreId == store && c.Email == email
							orderby c.Id
							select c;
				if (!includeDelete)
				{
					return query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else
				{
					return query.FirstOrDefault();
				}
			}
			else
			{
				User user = null;
				var query = from u in UserRepository.Table
							where u.Email == email
							orderby u.Id
							select u;

				if (!includeDelete)
				{
					user = query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else
				{
					user = query.FirstOrDefault();
				}


				var query2 = from u in query
							 join uur in UserUserRoleRepository.TableNoTracking on u.Id equals uur.UserId
							 select uur;

				user.SetUserUserRole(query2?.ToList());


				var query3 = from u in query2
							 join ur in UserRoleRepository.TableNoTracking on u.UserRoleId equals ur.Id
							 select ur;

				user.SetUserRole(query3?.ToList());

				return user;
			}
		}


		/// <summary>
		/// 根据系统名称获取用户
		/// </summary>
		/// <param name="systemName"></param>
		/// <returns></returns>
		public virtual User GetUserBySystemName(int? store, string systemName, bool noTracking = false)
		{
			return GetUserBySystemNameNoCache(store, systemName, noTracking);
		}
		public virtual User GetUserBySystemNameNoCache(int? store, string systemName, bool noTracking = false)
		{
			if (string.IsNullOrWhiteSpace(systemName))
			{
				return null;
			}
			if (!noTracking)
			{
				var query = from c in UserRepository.Table
							orderby c.Id
							where c.StoreId == store && c.SystemName == systemName
							select c;

				var user = query.FirstOrDefault();

				return user;
			}
			else
			{
				User user = null;
				var query = from u in UserRepository.TableNoTracking
							where u.SystemName == systemName
							orderby u.Id
							select u;

				user = query.FirstOrDefault();

				var query2 = from u in query
							 join uur in UserUserRoleRepository.TableNoTracking on u.Id equals uur.UserId
							 select uur;

				user.SetUserUserRole(query2?.ToList());


				var query3 = from u in query2
							 join ur in UserRoleRepository.TableNoTracking on u.UserRoleId equals ur.Id
							 select ur;

				user.SetUserRole(query3?.ToList());

				return user;
			}
		}



		/// <summary>
		/// 根据用户名获取用户
		/// </summary>
		/// <param name="username">Username</param>
		/// <returns>User</returns>
		public virtual User GetUserByUsername(int? store, string username, bool noTracking = false, bool includeDelete = false)
		{
			return GetUserByUsernameNoCache(store, username, noTracking, includeDelete);
		}
		public virtual User GetUserByUsernameNoCache(int? store, string username, bool noTracking = false, bool includeDelete = false)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				return null;
			}

			if (!noTracking)
			{
				var query = from c in UserRepository.Table
							where c.StoreId == store && c.Username == username
							orderby c.Id
							select c;

				if (!includeDelete)
				{
					return query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else 
				{
					return query.FirstOrDefault();
				}
			}
			else
			{
				User user = null;
				var query = from u in UserRepository.TableNoTracking
							where u.Username == username
							orderby u.Id
							select u;

				if (!includeDelete)
				{
					user = query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else
				{
					user = query.FirstOrDefault();
				}


				var query2 = from u in query
							 join uur in UserUserRoleRepository.TableNoTracking on u.Id equals uur.UserId
							 select uur;

				user.SetUserUserRole(query2?.ToList());


				var query3 = from u in query2
							 join ur in UserRoleRepository.TableNoTracking on u.UserRoleId equals ur.Id
							 select ur;

				user.SetUserRole(query3?.ToList());

				return user;

			}
		}

		/// <summary>
		/// 根据手机号获取用户
		/// </summary>
		/// <param name="username">Username</param>
		/// <returns>User</returns>
		public virtual User GetUserByMobileNamber(int? store, string mobileNamber, bool noTracking = false, bool includeDelete = false)
		{
			return GetUserByMobileNamberNoCache(store, mobileNamber, noTracking, includeDelete);
		}
		public virtual User GetUserByMobileNamberNoCache(int? store, string mobileNamber, bool noTracking = false, bool includeDelete = false)
		{
			if (string.IsNullOrWhiteSpace(mobileNamber))
			{
				return null;
			}

			if (!noTracking)
			{
				var query = from c in UserRepository.Table
							where c.StoreId == store && c.MobileNumber == mobileNamber
							orderby c.Id
							select c;

				if (!includeDelete)
				{
					return query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else
				{
					return query.FirstOrDefault();
				}
			}
			else
			{
				User user = null;
				var query = from u in UserRepository.TableNoTracking
							where u.MobileNumber == mobileNamber
							orderby u.Id
							select u;

				if (!includeDelete)
				{
					user = query.Where(u => u.Deleted == false).FirstOrDefault();
				}
				else
				{
					user = query.FirstOrDefault();
				}


				var query2 = from u in query
							 join uur in UserUserRoleRepository.TableNoTracking on u.Id equals uur.UserId
							 select uur;

				user.SetUserUserRole(query2?.ToList());


				var query3 = from u in query2
							 join ur in UserRoleRepository.TableNoTracking on u.UserRoleId equals ur.Id
							 select ur;

				user.SetUserRole(query3?.ToList());

				return user;
			}
		}


		/*
		/// <summary>
		/// 获取当前用户的直属下级用户ID（递归）
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public virtual List<int> GetSubordinate(int? store, int userId)
		{
			string sqlString = @"SELECT 
							   id as `value` 
							FROM
								(SELECT
									t1.id,
										t1.Dirtleader,
										IF(FIND_IN_SET(Dirtleader, @pids) > 0, @pids:= CONCAT(@pids, ',', id), 0) AS ischild
								FROM
									(SELECT
									id, Dirtleader
								FROM
									auth.Users t
								WHERE ";
			sqlString += " t.StoreId = '" + store + "' and t.Dirtleader is not null ";

			//sqlString += @" ORDER BY Dirtleader, id) t1, (SELECT @pids:= " + userId + ") t2) t3 WHERE ischild != 0; ";
			sqlString += @" ORDER BY Dirtleader, id) t1, (SELECT @pids:= " + userId + ") t2) t3 WHERE ischild != 0;  ";

			var userIds = UserRoleRepository_RO.QueryFromSql<IntQueryType>(sqlString)?.ToList();

			var users = userIds.Select(s => s.Value ?? 0).ToList();

			if (users == null) users = new List<int>();

			if (!users.Contains(userId))
			{
				users.Add(userId);
			}

			return users;
		}
		*/


		public List<int> GetSubordinate(int? store, int userId)
		{
			var ids = new List<int>();
			try
			{
				var query = UserRepository.Table;
				query = query.Where(c => c.Id == userId && c.StoreId == store.Value);

				var subs = query
						.Where(s=> !string.IsNullOrEmpty(s.Subordinates))
						.Select(s => s.Subordinates)
						.FirstOrDefault();

				if (!string.IsNullOrEmpty(subs))
				{
					ids = JsonConvert.DeserializeObject<List<int>>(subs);
				}

				if (ids != null && userId>0)
					ids.Add(userId);

				return ids;
			}
			catch (Exception )
			{
				if (ids != null)
					ids.Add(userId);

				return ids;
			}
		   
		}

		public List<int> GetUsersByParentId(int? store, int pid)
		{
			var query = UserRepository.Table;
			query = query.Where(c => c.Dirtleader == pid && c.StoreId == store.Value);
			return query.Select(s => s.Id).ToList();
		}

		public virtual List<int> GetSubordinate(int? store, int userId,string systemName)
		{
		   var sqlString = @" SELECT 
								u.Id as `value` 
							FROM
								auth.Users AS u
									INNER JOIN
								auth.User_UserRole_Mapping AS urm ON u.id = urm.UserId
									INNER JOIN
								auth.UserRoles AS ur ON urm.UserRoleId = ur.id
									AND ur.SystemName = '{0}'
							WHERE
								u.StoreId = {1};";

			var userIds = UserRoleRepository_RO.QueryFromSql<IntQueryType>(string.Format(sqlString, systemName, store ?? 0)).ToList();

			return userIds.Select(s => s.Value ?? 0).ToList();
		}

		/// <summary>
		/// 添加用户
		/// </summary>
		/// <param name="user"></param>
		public virtual void InsertUser(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			var uow = UserRepository.UnitOfWork;
			UserRepository.Insert(user);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityInserted(user);
		}

		/// <summary>
		/// 更新用户
		/// </summary>
		/// <param name="user"></param>

		public virtual void UpdateUser(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}

			var uow = UserRepository.UnitOfWork;
			UserRepository.Update(user);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityUpdated(user);

		}


		public virtual User GetUserByToken(string token)
		{
			var user = UserRepository.Table.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
			return user;
		}



		#endregion

		#region 用户角色


		/// <summary>
		/// 删除用户角色
		/// </summary>
		/// <param name="userRole"></param>
		public virtual void DeleteUserRole(UserRole userRole)
		{
			if (userRole == null)
			{
				throw new ArgumentNullException("userRole");
			}

			if (userRole.IsSystemRole)
			{
				throw new DCMSException("System role could not be deleted");
			}

			var uow = UserRoleRepository.UnitOfWork;
			UserRoleRepository.Delete(userRole);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityDeleted(userRole);
		}


		/// <summary>
		/// 根据角色ID获取角色
		/// </summary>
		/// <param name="userRoleId"></param>
		/// <returns></returns>
		public virtual UserRole GetUserRoleById(int userRoleId)
		{
			if (userRoleId == 0)
			{
				return null;
			}

			return UserRoleRepository.ToCachedGetById(userRoleId);
		}

		/// <summary>
		/// 根据角色ID获取用户
		/// </summary>
		/// <param name="userRoleId"></param>
		/// <returns></returns>
		public virtual List<int> GetUserByroleId(int RoleId)
		{
			if (RoleId == 0)
			{
				return null;
			}
			return UserUserRoleRepository_RO.Table.Where(s => s.UserRoleId == RoleId).Select(m => m.UserId).ToList();
		}

		/// <summary>
		/// 根据系统名称获取角色
		/// </summary>
		/// <param name="systemName"></param>
		/// <returns></returns>
		public virtual UserRole GetUserRoleBySystemName(int? store, string systemName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(systemName))
				{
					return null;
				}

				var key = DCMSDefaults.USERROLES_BY_SYSTEMNAME_KEY.FillCacheKey(store ?? 0, systemName);
				return _cacheManager.Get(key, () =>
				{
					var query = from cr in UserRoleRepository.Table
								orderby cr.Id
								where cr.SystemName == systemName && cr.StoreId == store
								select cr;
					var userRole = query.FirstOrDefault();
					return userRole;
				});
			}
			catch (Exception)
			{
				return null;
			}

		}


		/// <summary>
		/// 根据经销商、系统名称获取角色
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="systemName"></param>
		/// <returns></returns>
		public virtual UserRole GetUserRoleBySystemName(int storeId, string systemName)
		{
			if (string.IsNullOrWhiteSpace(systemName))
			{
				return null;
			}

			var key = DCMSDefaults.USERROLES_BY_SYSTEMNAME_KEY.FillCacheKey(storeId, systemName);
			return _cacheManager.Get(key, () =>
			 {
				 var query = from cr in UserRoleRepository.Table
							 orderby cr.Id
							 where cr.StoreId == storeId
							 && cr.SystemName == systemName
							 select cr;
				 var userRole = query.FirstOrDefault();
				 return userRole;
			 });
		}


		public virtual IList<UserRole> GetUserRolesByUser(int storeId, int userId)
		{
			var key = DCMSDefaults.USERROLES_BY_SYSTEMNAME_GETUSERROLESBYUSER_KEY.FillCacheKey(storeId, userId);
			return _cacheManager.Get(key, () =>
			 {
				 var userRoles = UserRoleRepository.EntityFromSql<UserRole>($"select ur.* from UserRoles as ur inner join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId inner join Users  as u on  u.Id = urm.UserId where u.StoreId = {storeId} and u.Id = {userId} group by ur.Id order by u.StoreId,u.Id;").ToList();
				 return userRoles.ToList();
			 });
		}



		public virtual Dictionary<int, string> GetAllUserRolesByUser(int storeId)
		{
			var key = DCMSDefaults.USERROLES_ALLBY_SYSTEMNAME_GETUSERROLESBYUSER_KEY.FillCacheKey(storeId);
			return _cacheManager.Get(key, () =>
			{
				var userRoles = UserRoleRepository.QueryFromSql<DictType>($"SELECT uur.UserId as Id, GROUP_CONCAT(ur.Name) AS Name FROM user_userrole_mapping AS uur INNER JOIN userroles AS ur ON uur.UserRoleId = ur.Id WHERE uur.StoreId = {storeId} AND ur.StoreId = {storeId} group by uur.UserId").ToDictionary(k => k.Id, v => v.Name);
				return userRoles;
			});
		}



		public virtual IPagedList<UserRole> GetAllUserRoles(int? store, bool showHidden = false, string name = "", int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;
			var lists = new List<UserRole>();
			var key = DCMSDefaults.USERROLES_ALL_PAGE_KEY.FillCacheKey(store, showHidden, pageIndex, pageSize);
			var plists = _cacheManager.Get(key, () =>
			{
				var query = UserRoleRepository_RO.TableNoTracking.Where(u => u.StoreId == store.Value);

				query = query.Where(u => u.Active == showHidden);

				if (!string.IsNullOrEmpty(name))
				{
					query = query.Where(u => u.Name.Contains(name));
				}

				query = query.OrderByDescending(u => u.IsSystemRole).OrderBy(s => s.Id);

				return query.ToList();
			});

			return new PagedList<UserRole>(plists, pageIndex, pageSize);

		}



		/// <summary>
		/// 获取全部用户角色
		/// </summary>
		/// <param name="showHidden"></param>
		/// <returns></returns>
		public virtual IList<UserRole> GetAllUserRoles(int? store = 0, bool showHidden = false)
		{
			var query = from cr in UserRoleRepository_RO.Table
						where cr.StoreId == store && (showHidden || cr.Active)
						orderby cr.Name
						select cr;
			var userRoles = query.ToList();
			return userRoles;
		}

		///// <summary>
		///// 获取用户全部上级
		///// </summary>
		///// <param name="showHidden"></param>
		///// <returns></returns>
		//public virtual void GetAllLeaderByUserId(int userId,List<int> leaders, int store = 0)
		//{
		//    var query = from cr in UserRepository_RO.Table
		//                where cr.StoreId == store && cr.Id == userId
		//                select cr.Dirtleader;
		//    var leader = query.FirstOrDefault() ?? 0;
		//    leaders.Add(leader);
		//    if (leader != 0 && !leaders.Contains(leader))
		//    {
		//        GetAllLeaderByUserId(leader, leaders, store);
		//    }
		//}

		/// <summary>
		/// 获取用户全部下级
		/// </summary>
		/// <param name="showHidden"></param>
		/// <returns></returns>
		public virtual List<int> GetAllSubordinateByUserId(int userId, int store)
		{
			var query = from cr in UserRepository_RO.Table
						where cr.StoreId == store && cr.Dirtleader == userId
						select cr.Id;
			return query.ToList();
		}


		public virtual IList<UserRole> GetAllUserRolesByStore(bool showHidden = false, int storeId = 0, bool isPlatform = false)
		{
			var key = DCMSDefaults.USERROLES_ALL_BY_STORE_KEY.FillCacheKey(storeId, showHidden);
			_cacheManager.Remove(key);
			return _cacheManager.Get(key, () =>
			{
				if (isPlatform)
				{
					var query = from cr in UserRoleRepository.Table
								where (showHidden || cr.Active)
								orderby cr.Name
								select cr;
					var userRoles = query.ToList();
					return userRoles;
				}

				if (storeId != 0)
				{
					var query = from cr in UserRoleRepository.Table
								where (showHidden || cr.Active) && cr.StoreId == storeId
								orderby cr.Name
								select cr;
					var userRoles = query.ToList();
					return userRoles;
				}
				else
				{
					return GetAllUserRoles(storeId, showHidden);
				}

			});
		}


		/// <summary>
		/// 添加角色
		/// </summary>
		/// <param name="userRole"></param>
		public virtual void InsertUserRole(UserRole userRole)
		{
			if (userRole == null)
			{
				throw new ArgumentNullException("userRole");
			}

			var uow = UserRoleRepository.UnitOfWork;
			UserRoleRepository.Insert(userRole);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityInserted(userRole);
		}

		/// <summary>
		/// 更新角色
		/// </summary>
		/// <param name="userRole"></param>
		public virtual void UpdateUserRole(UserRole userRole)
		{
			if (userRole == null)
			{
				throw new ArgumentNullException("userRole");
			}

			var uow = UserRoleRepository.UnitOfWork;
			UserRoleRepository.Update(userRole);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityUpdated(userRole);
		}

		public void InsertUserRoleMapping(int userId, int roleId, int storeId)
		{
			if (userId <= 0 || roleId <= 0)
			{
				throw new ArgumentNullException("userUserRole");
			}

			var userUserRole = new UserUserRole
			{
				UserId = userId,
				UserRoleId = roleId,
				StoreId = storeId
			};

			var uow = UserUserRoleRepository.UnitOfWork;
			UserUserRoleRepository.Insert(userUserRole);
			uow.SaveChanges();
			//事件通知
			_eventPublisher.EntityInserted(userUserRole);
		}

		public void RemoveUserRoleMapping(int userId, int roleId, int storeId)
		{
			if (userId <= 0 || roleId <= 0)
			{
				throw new ArgumentNullException("userUserRole");
			}

			var userUserRole = UserUserRoleRepository.Table.FirstOrDefault(c => c.UserId == userId && c.UserRoleId == roleId && c.StoreId == storeId);

			if (userUserRole == null)
				return;

			var uow = UserUserRoleRepository.UnitOfWork;
			UserUserRoleRepository.Delete(userUserRole);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityDeleted(userUserRole);
		}
		#endregion

		#region 授权

		/// <summary>
		/// 构造ZTree树 授权数据
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public IList<ZTree> GetListZTreeVM(int roleId)
		{
			List<ZTree> result = new List<ZTree>();


			//获取当前用户角色权限记录
			var curRole = GetUserRoleById(roleId); //修改
			List<double> permissionIds = curRole.PermissionRecordRoles.Select(c => c.PermissionRecord_Id + 0.5).ToList();

			//获取当前角色所适用的模块树
			int storeId = 0;

			List<ZTree> mouduleNodes = ModuleRepository.Table.Select(c => c).Where(c => (c.Enabled == true) && c.StoreId == storeId)
				.ToList().OrderBy(c => c.ParentId)
				.Select(c =>
				{
					return new ZTree()
					{
						id = c.Id,
						pId = c.ParentId,
						name = c.Name,
						isParent = ModuleRepository.Table.Where(m => m.ParentId == c.Id).Count() > 0,
						open = !c.ParentId.HasValue
					};
				}).ToList();

			//获取当前角色所适用的权限记录树
			List<ZTree> permissionNodes = PermissionRecordRepository.Table
				.Select(c => c).Where(c => (c.Enabled == true) && c.StoreId == storeId)
				.ToList().Select(c =>
				{
					return new ZTree()
					{
						id = c.Id + 0.5,
						pId = c.ModuleId,
						name = c.Name
					};
				}).ToList();

			//是否拥有权限
			foreach (var node in permissionNodes)
			{
				if (permissionIds.Contains(node.id))
				{
					node.@checked = true;
				}
			}

			result.AddRange(mouduleNodes);
			result.AddRange(permissionNodes);

			return result;
		}

		/// <summary>
		/// 构造ZTree树 安装授权数据
		/// </summary>
		/// <returns></returns>
		public IList<ZTree> GetListZTreeSG()
		{
			List<ZTree> result = new List<ZTree>();

			//获取当前角色所适用的模块树
			List<ZTree> mouduleNodes = ModuleRepository.Table.Select(c => c).Where(c => (c.Enabled == true) && c.StoreId == 0)
				.ToList().OrderBy(c => c.ParentId)
				.Select(c =>
				{
					return new ZTree()
					{
						id = c.Id,
						pId = c.ParentId,
						name = c.Name,
						isParent = ModuleRepository.Table.Where(m => m.ParentId == c.Id).Count() > 0,
						open = !c.ParentId.HasValue
					};
				}).ToList();

			//获取当前角色所适用的权限记录树
			List<ZTree> permissionNodes = PermissionRecordRepository.Table
				.Select(c => c).Where(c => (c.Enabled == true) && c.StoreId == 0)
				.ToList().Select(c =>
				{
					return new ZTree()
					{
						id = c.Id + 0.5,
						pId = c.ModuleId,
						name = c.Name
					};
				}).ToList();

			result.AddRange(mouduleNodes);
			result.AddRange(permissionNodes);

			return result;
		}

		public IList<PermissionRecord> GetUserPermissionRecords(User user)
		{
			return GetUserPermissionRecordsAsync(user).Result;
		}

		/// <summary>
		/// 获取当前用户的所有角色PC端权限集
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task<IList<PermissionRecord>> GetUserPermissionRecordsAsync(User user)
		{
			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PATTERN_BYUSER_KEY.FillCacheKey(user.StoreId, user.Id, 0);
			key.CacheTime = 5;
			return await _cacheManager.GetAsync<List<PermissionRecord>>(key, () =>
			 {
				 int[] urids = new int[] { };
				 var permissionRecords = new List<PermissionRecord>();

				 if (user.UserRoles == null || user.UserRoles.Count == 0)
				 {
					 //var userRoles = UserRoleRepository.EntityFromSql<UserRole>($"select ur.* from UserRoles as ur inner join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId inner join Users  as u on  u.Id = urm.UserId where u.Id = {user.Id};").ToList();
					 //if (userRoles != null)
					 //{
						// urids = userRoles.Select(s => s.Id).ToArray();
					 //}
				 }
				 else
				 {
					 urids = user.UserRoles.Select(s => s.Id).ToArray();
				 }

				 if (urids.Length > 0)
				 {
					 string sqlString = $"select pr.* from  PermissionRecord as pr where pr.Id in (select prm.PermissionRecord_Id from PermissionRecord_Role_Mapping as prm  where prm.StoreId = {user.StoreId} and prm.UserRole_Id in({string.Join(",", urids)}) group by prm.PermissionRecord_Id) order by pr.Id;";
					 permissionRecords = PermissionRecordRepository.EntityFromSql<PermissionRecord>(sqlString).ToList();
				 }

				 return Task.FromResult(permissionRecords);
			 });
		}


		public List<UserRole> GetCurrentUserRoles(int storeId, int userId)
		{
			var userRoles = UserRoleRepository.EntityFromSql<UserRole>($"select ur.* from UserRoles as ur inner join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId inner join Users  as u on  u.Id = urm.UserId where u.StoreId = {storeId} and u.Id = {userId}").ToList();
			return userRoles;
		}


		/// <summary>
		/// 获取当前角色的所有权限集
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="userRoleId"></param>
		/// <param name="platform">0:PC,1:APP</param>
		/// <returns></returns>
		public IList<PermissionRecord> GetUserRolePermissionRecords(int storeId, int userRoleId, int platform = 0)
		{
			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PATTERN_BYUSERROLE_KEY.FillCacheKey(storeId, userRoleId, platform);
			return _cacheManager.Get(key, () =>
			 {
				 var permissionRecords = new List<PermissionRecord>();
				 if (userRoleId > 0)
				 {
					 string sqlString = $"select pr.* from  PermissionRecord as pr where  pr.Id in (select prm.PermissionRecord_Id from PermissionRecord_Role_Mapping as prm  where  prm.StoreId = {storeId} and  prm.UserRole_Id = {userRoleId} and prm.Platform = {platform} group by prm.PermissionRecord_Id) order by pr.Id;";

					 permissionRecords = PermissionRecordRepository_RO.EntityFromSql<PermissionRecord>(sqlString).ToList();
				 }
				 return permissionRecords;
			 });
		}


		/// <summary>
		/// 获取当前用户的所有角色PC端权限集
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[Obsolete("获取当前用户的所有角色PC端权限集,次方法使用EF依赖加载特性，对于大量关联查询会导致性能损失,该方法已经弃用。", false)]
		public IList<PermissionRecord> GetUserPermissionRecordsByUser(User user)
		{
			var curRoles = user.UserRoles.ToList();
			var curPermissions = new List<PermissionRecord>();

#if DEBUG
			Stopwatch sw = new Stopwatch();
			sw.Start();
			System.Diagnostics.Debug.WriteLine($"Start GetUserPermissionRecords...");
#endif
			curRoles.ForEach(r =>
			{
				var permission = r.PermissionRecordRoles.Where(c => c.Platform == 0)?.Select(c => c.PermissionRecord).ToList();
				curPermissions.AddRange(permission);
			});
#if DEBUG
			sw.Stop();
			long times = sw.ElapsedMilliseconds;
			System.Diagnostics.Debug.WriteLine($"Stop:{times}");
			//Stop:6667
#endif


			var query = curPermissions.OrderBy(c => c.Id).ThenBy(c => c.Name);
			return query.ToList();
		}

		/// <summary>
		/// 获取当前用户的所有角色APP端权限集
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public IList<PermissionRecord> GetUserAPPPermissionRecords(User user)
		{
			int[] urids = new int[] { };
			var permissionRecords = new List<PermissionRecord>();

			if (user.UserRoles == null || user.UserRoles.Count == 0)
			{
				var userRoles = UserRoleRepository.QueryFromSql<IntQueryType>($"select ur.Id as Value from UserRoles as ur left join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId left join Users  as u on  u.Id = urm.UserId where (u.StoreId = {user.StoreId} and u.StoreId = {user.StoreId}) and u.Id = {user.Id};").ToList();
				if (userRoles != null)
				{
					//urids = userRoles.Select(s => s.Id).ToArray();
					urids = userRoles.Select(u => u.Value ?? 0).ToArray();
				}
			}
			else
			{
				urids = user.UserRoles.Select(s => s.Id).ToArray();
			}

			if (urids.Length > 0)
			{
				string sqlString = $"select pr.* from auth.PermissionRecord as pr  inner join auth.PermissionRecord_Role_Mapping as prm on pr.Id = prm.PermissionRecord_Id and prm.Platform  = 1 where prm.UserRole_Id in ({string.Join(",", urids)}) group by prm.PermissionRecord_Id order by pr.Id";
				//string sqlString = $"select pr.* from  PermissionRecord as pr where pr.Id in (select prm.PermissionRecord_Id from PermissionRecord_Role_Mapping as prm  where prm.Platform = 1 and prm.UserRole_Id in({string.Join(",", urids)}) group by prm.PermissionRecord_Id) order by pr.Id;";
				permissionRecords = PermissionRecordRepository.EntityFromSql<PermissionRecord>(sqlString).ToList();
			}
			return permissionRecords;
		}


		/// <summary>
		/// 获取当前用户的所有角色APP端权限集
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[Obsolete("获取当前用户的所有角色APP端权限集,次方法使用EF依赖加载特性，对于大量关联查询会导致性能损失,该方法已经弃用。", false)]
		public IList<PermissionRecord> GetUserAPPPermissionRecordsByUser(User user)
		{
			var curRoles = user.UserRoles.ToList();
			var curPermissions = new List<PermissionRecord>();
			curRoles.ForEach(r =>
			{
				var permission = r.PermissionRecordRoles.Where(c => c.Platform == 1)?.Select(c => c.PermissionRecord).ToList();
				curPermissions.AddRange(permission);
			});

			var query = curPermissions.OrderBy(c => c.Id).ThenBy(c => c.Name);

			return query.ToList();
		}

		/// <summary>
		/// 获取当前用户的所有角色模块访问集
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public IList<Module> GetUserModuleRecords(int? store, User user, bool showMobile = false)
		{
			var curModules = new List<Module>();

			int[] urids = new int[] { };


			var sql = $"select ur.Id from UserRoles as ur inner join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId inner join Users  as u on  u.Id = urm.UserId where ur.StoreId = {store} and u.StoreId = {store} and u.Id = {user.Id}";

			var userRoles = UserRoleRepository.EntityFromSql<UserRole>(sql).Select(b => new { b.Id }).ToList();

			if (userRoles != null)
			{
				urids = userRoles.Select(s => s.Id).ToArray();
				var modules = (from mr in ModuleRepository.Table
							   join mrr in ModuleRoleRepository.Table on mr.Id equals mrr.Module_Id
							   where urids.Contains(mrr.UserRole_Id)
							   select mr)
							  .OrderBy(s => s.Id);

				curModules = modules.ToList();
			}

			return curModules;
		}

		/// <summary>
		/// 获取当前角色拥有的模块
		/// </summary>
		/// <param name="userRoleId"></param>
		/// <param name="showMobile"></param>
		/// <returns></returns>
		public IList<Module> GetUserRoleModuleRecords(int? store, int userRoleId, bool showMobile = false)
		{
			var key = DCMSDefaults.GET_GETUSERAPPMODULERECORDS_BYUSERROLEID_KEY.FillCacheKey(store ?? 0, userRoleId, showMobile);
			try
			{
				var curModules = new List<Module>();
				if (userRoleId > 0)
				{
					var modules = from mr in ModuleRepository_RO.TableNoTracking
								  join mrr in ModuleRoleRepository_RO.TableNoTracking on mr.Id equals mrr.Module_Id
								  where userRoleId.Equals(mrr.UserRole_Id) //&& mr.ShowMobile == showMobile
								  select mr;

					modules = modules.Distinct()
					.OrderBy(c => c.Id)
					.ThenBy(c => c.Name);
					curModules = modules.ToList();
				}
				return curModules;
			}
			catch (Exception)
			{
				return new List<Module>();
			}

		}
		public IList<IntQueryType> GetUserModuleRecordsByUser(User user)
		{
			return GetUserModuleRecordsByUserAsync(user).Result;
		}
		public async Task<IList<IntQueryType>> GetUserModuleRecordsByUserAsync(User user)
		{
			int[] curRoles = user.UserRoles.Select(s => s.Id).ToArray();
			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PATTERN_CURROLES_KEY.FillCacheKey(user.StoreId, user, string.Join("_", curRoles));

			return await _cacheManager.GetAsync<List<IntQueryType>>(key, () =>
			{
				//if (curRoles.Count() == 0)
				//{
				//	//MySql.Data.MySqlClient.MySqlException:“Unknown column 'urm.User_Id' in 'on clause'”
				//	var userRoles = UserRoleRepository.EntityFromSql<UserRole>($"select ur.* from UserRoles as ur inner join User_UserRole_Mapping as urm on ur.id = urm.UserRoleId inner join Users  as u on  u.Id = urm.UserId where u.Id = {user.Id};").ToList();

				//	if (userRoles != null)
				//	{
				//		curRoles = userRoles.Select(s => s.Id).ToArray();
				//	}
				//}

				/*
				 A second operation started on this context before a previous operation completed. This is usually caused by different threads using the same instance of DbContext, however instance members are not guaranteed to be thread safe. This could also be caused by a nested query being evaluated on the client, if this is the case rewrite the query avoiding nested invocations.”
				 */
				var modules = UserRoleRepository.QueryFromSql<IntQueryType>($"select m.Id as `value` from UserRoles as ur inner  join Module_Role_Mapping as mrm on ur.Id = mrm.UserRole_Id inner join Module as m  on m.Id = mrm.Module_Id where ur.Id in ({string.Join(",", curRoles)}) group  by `value`;").ToList();


				return Task.FromResult(modules);
			});
		}


		#endregion

		#region 用户片区

		public virtual void DeleteUserDistricts(UserDistricts userDistricts)
		{
			if (userDistricts == null)
			{
				throw new ArgumentNullException("userDistricts");
			}

			var uow = UserDistrictsRepository.UnitOfWork;
			UserDistrictsRepository.Delete(userDistricts);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityDeleted(userDistricts);
		}
		public virtual void DeleteUserDistricts(int userId, int districtId)
		{
			var query = from cr in UserDistrictsRepository.Table
						where cr.UserId == userId && cr.DistrictId == districtId
						orderby cr.Id
						select cr;
			var userDistricts = query.FirstOrDefault();
			if (userDistricts != null)
			{
				DeleteUserDistricts(userDistricts);
			}
		}
		public virtual UserDistricts GetUserDistrictsById(int userDistrictsId)
		{
			if (userDistrictsId == 0)
			{
				return null;
			}

			return UserDistrictsRepository.ToCachedGetById(userDistrictsId);
		}

		public virtual IList<UserDistricts> GetAllUserDistrictsByUserId(int? store, int userId)
		{
			var query = from cr in UserDistrictsRepository.Table
						where cr.UserId == userId && cr.StoreId == store
						orderby cr.Id
						select cr;

			var key = DCMSDefaults.USERS_USERDISTRICTSL_BY_ID_KEY.FillCacheKey(store ?? 0, userId);
			return _cacheManager.Get(key, () => query.ToList());
		}

		public virtual void InsertUserDistricts(UserDistricts userDistricts)
		{
			if (userDistricts == null)
			{
				throw new ArgumentNullException("userDistricts");
			}

			var uow = UserDistrictsRepository.UnitOfWork;
			UserDistrictsRepository.Insert(userDistricts);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityInserted(userDistricts);
		}
		public virtual void UpdateUserDistricts(UserDistricts userDistricts)
		{
			if (userDistricts == null)
			{
				throw new ArgumentNullException("userDistricts");
			}

			var uow = UserDistrictsRepository.UnitOfWork;
			UserDistrictsRepository.Update(userDistricts);
			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityUpdated(userDistricts);
		}


		#endregion

		#region 基础

		/// <summary>
		/// 获取经销商的具体角色用户
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="userRoleIds">多个角色以','分割,为空则查询当前经销商下所有角色</param>
		public List<User> GetUserByStoreIdUserRoleIds(int storeId, string userRoleIds)
		{
			if (storeId == 0)
			{
				return null;
			}

			if (string.IsNullOrEmpty(userRoleIds))
			{
				userRoleIds = ",";
			}

			var key = DCMSDefaults.USERS_ALL_BY_STORE_KEY.FillCacheKey(storeId, userRoleIds);
			return _cacheManager.Get(key, () =>
		   {
			   int[] _userRoleIds = userRoleIds
			   .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
			   .Select(r => int.Parse(r)).ToArray();

			   //经销商的所有用户
			   var query = UserRepository.Table;

			   query = query.Where(a => a.StoreId == storeId);

			   if (_userRoleIds != null && _userRoleIds.Length > 0)
			   {
				   //query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(_userRoleIds).Any());
				   //查询角色关联的用户
				   var query2 = UserUserRoleRepository.Table;
				   query2 = query2.Where(q => _userRoleIds.Contains(q.UserRoleId));
				   List<int> userIds = query2.Select(q => q.UserId).Distinct().ToList();
				   //当前用户必须在角色关联的用户中
				   query = query.Where(q => userIds.Contains(q.Id));
			   }

			   return query.ToList();
		   });
		}
		public List<User> GetUserBySystemRoleName(int storeId, string systemName)
		{
			if (storeId == 0)
			{
				return null;
			}
			systemName = CommonHelper.FilterSQLChar(systemName);
			var key = DCMSDefaults.USER_GETUSER_BY_SYSTEM_ROLE_NAME_KEY.FillCacheKey(storeId, systemName);
			return _cacheManager.Get(key, () =>
			 {
				 string sqlstring1 = @$"SELECT distinct b.Id,b.Username as Username,b.UserRealName FROM auth.User_UserRole_Mapping a left join auth.Users b on a.userid=b.id left join auth.UserRoles c on a.userroleid=c.id where b.storeid ='{storeId}' and b.deleted = 0";

				 if (!string.IsNullOrEmpty(systemName))
				 {
					 if (systemName.Equals("Employees")) systemName = "Salesmans";
					 sqlstring1 += $" and c.systemname = '{systemName}'";
				 }

				 var userQueryTypes = UserRepository_RO.QueryFromSql<UserQueryType>(sqlstring1).ToList();
				 List<int> userIds = new List<int>();
				 if (userQueryTypes != null && userQueryTypes.Count > 0)
				 {
					 userIds = userQueryTypes.Select(uq => uq.Id).ToList();
				 }
				 //var report = new List<User>();
				 var query = UserRepository_RO.TableNoTracking;
				 query = query.Where(a => a.StoreId == storeId);
				 query = query.Where(a => userIds.Contains(a.Id));
				 var report = query.ToList()
			  .Select(s => new User
			  {
				  Id = s.Id,
				  StoreId = s.StoreId,
				  UserGuid = s.UserGuid,
				  Username = s.Username,
				  UserRealName = s.UserRealName,
				  Email = s.Email,
				  MobileNumber = s.MobileNumber,
				  Password = s.Password,
				  PasswordFormatId = s.PasswordFormatId,
				  PasswordSalt = s.PasswordSalt,
				  AdminComment = s.AdminComment,
				  IsTaxExempt = s.IsTaxExempt,
				  Active = s.Active,
				  ActivationTime = s.ActivationTime,
				  Deleted = s.Deleted,
				  IsSystemAccount = s.IsSystemAccount,
				  SystemName = s.SystemName,
				  IsPlatformCreate = s.IsPlatformCreate,
				  LastIpAddress = s.LastIpAddress,
				  CreatedOnUtc = s.CreatedOnUtc,
				  LastLoginDateUtc = s.LastLoginDateUtc,
				  LastActivityDateUtc = s.LastActivityDateUtc,
				  EmailValidation = s.EmailValidation,
				  MobileValidation = s.MobileValidation,
				  AccountType = s.AccountType,
				  BranchCode = s.BranchCode,
				  BranchId = s.BranchId,
				  SalesmanExtractPlanId = s.SalesmanExtractPlanId,
				  DeliverExtractPlanId = s.DeliverExtractPlanId,
				  MaxAmountOfArrears = s.MaxAmountOfArrears,
				  FaceImage = s.FaceImage,
				  AppId = s.AppId,

			  });
				 return report.ToList();

			 });
		}

		/// <summary>
		/// 绑定用户信息
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="systemName"></param>
		/// <returns></returns>
		public List<User> BindUserList(int storeId, string systemName, int curUserId = 0, bool selectSubordinate = false, bool isadmin = false)
		{
			if (storeId == 0)
			{
				return new List<User>();
			}

			var subordinateIds = new List<int>();
			if (!isadmin)
			{
				subordinateIds.AddRange(GetSubordinate(storeId, curUserId));
			}
			if (subordinateIds?.Count == 0 && curUserId>0) 
			{
				subordinateIds.Add(curUserId);
			}
			string sqlString = $"select distinct u.Id,u.UserRealName,u.Username from Users u left join User_UserRole_Mapping uum on u.Id = uum.UserId and  uum.StoreId = '{storeId}' left join UserRoles ur on uum.UserRoleId = ur.Id and  ur.StoreId = '{storeId}' where u.StoreId = '{storeId}' and u.Deleted = '0' ";

			if (!isadmin && subordinateIds?.Count >0)
			{
				sqlString += $" and u.id in ({string.Join(",", subordinateIds)})  ";
			}

			if (!string.IsNullOrEmpty(systemName))
			{
				sqlString += $" and ur.SystemName = '{systemName}'";
			}

			var query = UserRoleRepository.QueryFromSql<UserQueryType>(sqlString).ToList()?
				.Select(u => new User { Id = u.Id, UserRealName = u.UserRealName, Username = u.Username })?
				.ToList() ?? new List<User>();

			return query.ToList();
		}

		/// <summary>
		/// 获取经销商主管人员
		/// </summary>
		/// <param name="storeId"></param>
		/// <returns></returns>
		public List<User> GetStoreManagers(int? storeId)
		{
			if (storeId == 0)
			{
				return new List<User>();
			}

			string sqlString = $"select distinct uum.UserId Id,'' Username,'' UserRealName from UserRoles ur left join User_UserRole_Mapping uum on ur.Id = uum.UserRoleId and uum.StoreId = '{storeId}' where ur.StoreId = '{storeId}' and ur.SystemName = '{DCMSDefaults.Administrators}' and ur.IsSystemRole='1' ";

			var userQueryTypes = UserRepository.QueryFromSql<UserQueryType>(sqlString).ToList();

			List<int> userIds = new List<int>();
			if (userQueryTypes != null && userQueryTypes.Count > 0)
			{
				userIds = userQueryTypes.Select(uq => uq.Id).ToList();
			}

			var query = UserRepository.TableNoTracking;
			query = query.Where(a => a.StoreId == storeId && userIds.Contains(a.Id));

			var key = DCMSDefaults.USER_GETSTOREMANAGERSBYSTOREID_KEY.FillCacheKey(storeId);
			return _cacheManager.Get(key, () => query.ToList());
		}


		public List<User> GetAllAdminUsersByStoreIds(int[] storeIds)
		{
			if (storeIds == null || storeIds.Length == 0)
			{
				return new List<User>();
			}

			var key = DCMSDefaults.USERS_ALL_ADMIN_BY_STOREIDS_KEY.FillCacheKey(storeIds);
			return _cacheManager.Get(key, () =>
			{
				//1.获取管理员角色Id
				var query = UserRoleRepository.Table;
				query = query.Where(q => q.SystemName == DCMSDefaults.Administrators && q.IsSystemRole == true);
				List<int> userRoleIds = query.Select(q => q.Id).ToList();
				if (userRoleIds == null && userRoleIds.Count == 0)
				{
					return new List<User>();
				}
				//获取有管理员角色的用户Id
				var query2 = UserUserRoleRepository.Table;
				query2 = query2.Where(q => userRoleIds.Contains(q.UserRoleId));
				List<int> userIds = query2.Select(q => q.UserId).Distinct().ToList();
				if (userIds == null && userIds.Count == 0)
				{
					return new List<User>();
				}
				//获取用户
				var query3 = UserRepository.Table;
				return query3.Where(q => userIds.Contains(q.Id) && storeIds.Contains(q.StoreId)).ToList();
			});

		}

		/// <summary>
		/// 获取经销商所有人员
		/// </summary>
		/// <param name="storeId"></param>
		/// <returns></returns>
		public List<User> GetAllUsers(int? storeId)
		{
			if (storeId.HasValue)
			{
				var query = UserRepository.Table;
				query = query.Where(u => u.StoreId == storeId);
				return query.ToList();
			}
			else
			{
				return null;
			}
		}

		public List<UserPercentages> GetAllUserPercentages(int? storeId)
		{
			if (storeId.HasValue)
			{
				var upsers = new List<UserPercentages>();
				var query = UserRepository.Table;
				query = query.Where(u => u.StoreId == storeId && u.Deleted == false);
				var users = query.ToList();
				users.ForEach(u =>
				{
					var spt = from pt in PercentageRepository.Table where pt.StoreId == storeId && pt.PercentagePlanId == u.SalesmanExtractPlanId orderby pt.StoreId, pt.PercentagePlanId select pt;
					var dpt = from pt in PercentageRepository.Table where pt.StoreId == storeId && pt.PercentagePlanId == u.DeliverExtractPlanId orderby pt.StoreId, pt.PercentagePlanId select pt;

					upsers.Add(new UserPercentages
					{
						User = u,
						SPercentages = spt.Include(s => s.PercentageRangeOptions).ToList(),
						DPercentages = dpt.Include(s => s.PercentageRangeOptions).ToList()
					});
				});
				return upsers.ToList();
			}
			else
			{
				return null;
			}
		}

		#region 获取人员电话号码

		/// <summary>
		/// 根据用户Id获取用户手机号
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public virtual string GetMobileNumberByUserId(int userId)
		{
			if (userId == 0)
			{
				return "";
			}
			var mobileNumber = UserRepository.Table.Where(u => u.Id == userId).Select(u => u.MobileNumber).FirstOrDefault();
			return mobileNumber ?? "";
		}
		/// <summary>
		/// 获取当前经销商 管理员用户 电话号码 
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public IList<string> GetAllAdminUserMobileNumbersByStore(int storeId)
		{
			List<string> mobileList = new List<string>();
			try
			{
				if (storeId > 0)
				{
					var query = from a in UserRepository.Table
								join b in UserUserRoleRepository.Table on a.Id equals b.UserId
								join c in UserRoleRepository.Table on b.UserRoleId equals c.Id
								where a.StoreId == storeId
								&& c.SystemName == DCMSDefaults.Administrators
								&& c.IsSystemRole == true
								select a.MobileNumber;
					mobileList = query.Distinct().ToList();
				}
				return mobileList;
			}
			catch (Exception)
			{
				return mobileList;
			}
		}
		public IList<string> GetAllUserMobileNumbersByUserIds(List<int> userIds)
		{
			if (userIds == null || userIds.Count == 0)
			{
				return new List<string>();
			}
			else
			{
				var query = UserRepository.Table;
				var list = query.Where(q => userIds.Contains(q.Id)).Select(q => q.MobileNumber).ToList();
				if (list == null || list.Count == 0)
				{
					list = new List<string>();
				}
				return list;
			}
		}


		#endregion


		#endregion


		#region 用户密码


		public virtual IList<UserPassword> GetUserPasswords(int? userId = null, PasswordFormat? passwordFormat = null, int? passwordsToReturn = null)
		{
			var query = UserPasswordRepository.Table;

			//filter by user
			if (userId.HasValue)
			{
				query = query.Where(password => password.UserId == userId.Value);
			}

			//filter by password format
			if (passwordFormat.HasValue)
			{
				query = query.Where(password => password.PasswordFormatId == (int)passwordFormat.Value);
			}

			//get the latest passwords
			if (passwordsToReturn.HasValue)
			{
				query = query.OrderByDescending(password => password.CreatedOnUtc).Take(passwordsToReturn.Value);
			}

			return query.ToList();
		}


		public virtual UserPassword GetCurrentPassword(int userId)
		{
			if (userId == 0)
			{
				return null;
			}

			//return the latest password
			return GetUserPasswords(userId, passwordsToReturn: 1).FirstOrDefault();
		}

		public virtual void InsertUserPassword(UserPassword userPassword)
		{
			if (userPassword == null)
			{
				throw new ArgumentNullException(nameof(userPassword));
			}

			var uow = UserPasswordRepository.UnitOfWork;
			UserPasswordRepository.Insert(userPassword);
			uow.SaveChanges();

			//event notification
			_eventPublisher.EntityInserted(userPassword);
		}


		public virtual void UpdateUserPassword(UserPassword userPassword)
		{
			if (userPassword == null)
			{
				throw new ArgumentNullException(nameof(userPassword));
			}

			var uow = UserPasswordRepository.UnitOfWork;
			UserPasswordRepository.Update(userPassword);
			uow.SaveChanges();


			//event notification
			_eventPublisher.EntityUpdated(userPassword);
		}


		public virtual bool IsPasswordRecoveryTokenValid(User user, string token)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var cPrt = _genericAttributeService.GetAttribute<string>(user, DCMSDefaults.PasswordRecoveryTokenAttribute);
			if (string.IsNullOrEmpty(cPrt))
			{
				return false;
			}

			if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			return true;
		}


		public virtual bool IsPasswordRecoveryLinkExpired(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (_userSettings.PasswordRecoveryLinkDaysValid == 0)
			{
				return false;
			}

			var geneatedDate = _genericAttributeService.GetAttribute<DateTime?>(user, DCMSDefaults.PasswordRecoveryTokenDateGeneratedAttribute);
			if (!geneatedDate.HasValue)
			{
				return false;
			}

			var daysPassed = (DateTime.UtcNow - geneatedDate.Value).TotalDays;
			if (daysPassed > _userSettings.PasswordRecoveryLinkDaysValid)
			{
				return true;
			}

			return false;
		}


		public virtual bool PasswordIsExpired(User user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			//the guests don't have a password
			if (IsGuest(user))
			{
				return false;
			}

			//password lifetime is disabled for user
			if (!user.UserRoles.Any(role => (role?.Active ?? false) && (role?.EnablePasswordLifetime??false)))
			{
				return false;
			}

			//setting disabled for all
			if (_userSettings.PasswordLifetime == 0)
			{
				return false;
			}

			//cache result between HTTP requests
			var cacheKey = DCMSUserServiceDefaults.UserPasswordLifetimeCacheKey.FillCacheKey(user.Id);

			var currentLifetime = _cacheManager.Get(cacheKey, () =>
			{
				var userPassword = GetCurrentPassword(user.Id);
				//password is not found, so return max value to force user to change password
				if (userPassword == null)
				{
					return int.MaxValue;
				}

				return (DateTime.UtcNow - userPassword.CreatedOnUtc).Days;
			});

			return currentLifetime >= _userSettings.PasswordLifetime;
		}

		#endregion


		/// <summary>
		/// 清理权限缓存
		/// </summary>
		/// <param name="store"></param>
		public void ClearUserCache(int? store)
		{
			//PERMISSIONS_PK MODULES_PK
			_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MODULES_PK, store));
			_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, store));
			_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, store));
		}

		/// <summary>
		/// 清理系统缓存
		/// </summary>
		/// <param name="store"></param>
		public void ClearSystemCache(int? store)
		{
			_cacheManager.RemoveByPrefix(store.ToString());
			////1、经销商
			//_cacheManager.RemoveByPattern(DCMSDefaults.STORES_PK.FillCacheKey( store ?? 0));
			////2、用户角色
			//_cacheManager.RemoveByPattern(DCMSDefaults.USERROLES_PK.FillCacheKey( store ?? 0));
			////3、用户
			//_cacheManager.RemoveByPattern(DCMSDefaults.USER_PK.FillCacheKey( store ?? 0));
			////4、权限
			//_cacheManager.RemoveByPattern(DCMSDefaults.PERMISSIONS_PK.FillCacheKey( store ?? 0));
			//_cacheManager.RemoveByPattern(DCMSDefaults.DADAPERMISSIONS_PK.FillCacheKey( store ?? 0));
			////5、角色权限映射
			//_cacheManager.RemoveByPattern(DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PK.FillCacheKey( store ?? 0));
			////6、配置
			//_cacheManager.RemoveByPattern(DCMSDefaults.SETTINGS_PK.FillCacheKey( store ?? 0));
			////7、商品类别
			//_cacheManager.RemoveByPattern(DCMSDefaults.CATEGORIES_PK.FillCacheKey( store ?? 0));
			////8、供应商
			//_cacheManager.RemoveByPattern(DCMSDefaults.MANUFACTURER_PK.FillCacheKey( store ?? 0));
			////9、品牌
			//_cacheManager.RemoveByPattern(DCMSDefaults.BRAND_PK.FillCacheKey( store ?? 0));
			////10、仓库、
			//_cacheManager.RemoveByPattern(DCMSDefaults.WAREHOUSE_PK.FillCacheKey( store ?? 0));
			////11、商品规格属性
			//_cacheManager.RemoveByPattern(DCMSDefaults.PRODUCTATTRIBUTES_PK.FillCacheKey( store ?? 0));
			//_cacheManager.RemoveByPattern(DCMSDefaults.PRODUCTSPECIFICATIONATTRIBUTE_PK.FillCacheKey( store ?? 0));
			////12、会计科目
			//_cacheManager.RemoveByPattern(DCMSDefaults.ACCOUNTTYPES_PK.FillCacheKey( store ?? 0));
			//_cacheManager.RemoveByPattern(DCMSDefaults.ACCOUNTINGOPTION_PK.FillCacheKey( store ?? 0));
			////13、打印模板
			//_cacheManager.RemoveByPattern(DCMSDefaults.PRINTTEMPLATE_PK.FillCacheKey( store ?? 0));
			////14、商品价格
			//_cacheManager.RemoveByPattern(DCMSDefaults.PRICEPLAN_PK.FillCacheKey( store ?? 0));
			////15、商品
			//_cacheManager.RemoveByPattern(DCMSDefaults.PRODUCTS_PK.FillCacheKey( store ?? 0));
		}

		#region 判断当前用户是否拥有角色（代替：UserExtentions 中的方法）

		public bool IsInUserRole(int? store, int userId, string userRoleSystemName, bool onlyActiveUserRoles = true)
		{
			var key = DCMSDefaults.GET_GETUSERAPPMODULERECORDS_ISINUSERROLE_KEY.FillCacheKey(store, userId, userRoleSystemName, onlyActiveUserRoles);
			return _cacheManager.Get(key, () =>
			{
				var query = from a in UserRepository.Table
							join b in UserUserRoleRepository.Table on a.Id equals b.UserId
							join c in UserRoleRepository.Table on b.UserRoleId equals c.Id
							where a.StoreId == store && a.Id == userId
							&& c.SystemName == userRoleSystemName
							&& (c.Active == true || !onlyActiveUserRoles)
							select c.Id;

				return query.FirstOrDefault() > 0;
			});
		}
		public bool IsAdmin(int? store, int userId, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(store, userId, DCMSDefaults.Administrators, onlyActiveUserRoles);
		}

		/// <summary>
		/// 获取用户的角色
		/// </summary>
		/// <param name="user"></param>
		/// <param name="showHidden"></param>
		/// <returns></returns>
		public int[] GetUserRoleIds(User user, bool showHidden = false)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var customerRolesIds = (from a in UserRepository.Table
									join b in UserUserRoleRepository.Table on a.Id equals b.UserId
									join c in UserRoleRepository.Table on b.UserRoleId equals c.Id
									where a.StoreId == user.StoreId && a.Id == user.Id
									&& (c.Active == true || showHidden)
									select c.Id).ToArray();
			return customerRolesIds;
		}

		/// <summary>
		/// 是否营销总经理
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsMarketManager(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId,user.Id, DCMSDefaults.MarketManagers, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否大区总经理
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsRegionManager(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.RegionManagers, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否财务经理
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsFinancialManager(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.FinancialManagers, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否业务部经理
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsGuest(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.BusinessManagers, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否业务员
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsSalesman(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.Salesmans, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否配送员
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsDelivers(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.Delivers, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否员工
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsEmployee(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.Employees, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否经销商
		/// </summary>
		/// <param name="user">User</param>
		/// <param name="onlyActiveUserRoles"></param>
		/// <returns>Result</returns>
		public bool IsDistributor(User user, bool onlyActiveUserRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.Distributors, onlyActiveUserRoles);
		}

		/// <summary>
		/// 是否注册用户
		/// </summary>
		/// <param name="user"></param>
		/// <param name="onlyActiveCustomerRoles"></param>
		/// <returns></returns>
		public bool IsRegistered(User user, bool onlyActiveCustomerRoles = true)
		{
			return IsInUserRole(user.StoreId, user.Id, DCMSDefaults.RegisteredRoleName, onlyActiveCustomerRoles);
		}

		#endregion



		public virtual List<User> GetSubordinateUsers(int? store, int userId)
		{
			try
			{
				string sqlString = @"SELECT 
							   id as `value` 
							FROM
								(SELECT
									t1.id,
										t1.Dirtleader,
										IF(FIND_IN_SET(Dirtleader, @pids) > 0, @pids:= CONCAT(@pids, ',', id), 0) AS ischild
								FROM
									(SELECT
									id, Dirtleader
								FROM
									auth.Users t
								WHERE ";
				sqlString += " t.StoreId = '" + store + "' and t.Dirtleader is not null";

				sqlString += @" ORDER BY Dirtleader, id) t1, (SELECT @pids:= " + userId + ") t2) t3 ";

				var query1 = UserRoleRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();

				var ids = query1.Select(s => s.Value ?? 0).ToList();
				if (ids == null || !ids.Any())
				{
					ids.Add(userId);
				}

				var query = UserRepository_RO.TableNoTracking;
				query = query.Where(a => a.StoreId == store && a.Deleted == false);
				query = query.Where(a => ids.Contains(a.Id));
				var users = query.ToList().Select(s => new User
				{
					Id = s.Id,
					StoreId = s.StoreId,
					UserGuid = s.UserGuid,
					Username = s.Username,
					UserRealName = s.UserRealName,
					Email = s.Email,
					MobileNumber = s.MobileNumber,
					Password = s.Password,
					PasswordFormatId = s.PasswordFormatId,
					PasswordSalt = s.PasswordSalt,
					AdminComment = s.AdminComment,
					IsTaxExempt = s.IsTaxExempt,
					Active = s.Active,
					ActivationTime = s.ActivationTime,
					Deleted = s.Deleted,
					IsSystemAccount = s.IsSystemAccount,
					SystemName = s.SystemName,
					IsPlatformCreate = s.IsPlatformCreate,
					LastIpAddress = s.LastIpAddress,
					CreatedOnUtc = s.CreatedOnUtc,
					LastLoginDateUtc = s.LastLoginDateUtc,
					LastActivityDateUtc = s.LastActivityDateUtc,
					EmailValidation = s.EmailValidation,
					MobileValidation = s.MobileValidation,
					AccountType = s.AccountType,
					BranchCode = s.BranchCode,
					BranchId = s.BranchId,
					SalesmanExtractPlanId = s.SalesmanExtractPlanId,
					DeliverExtractPlanId = s.DeliverExtractPlanId,
					MaxAmountOfArrears = s.MaxAmountOfArrears,
					FaceImage = s.FaceImage,
					AppId = s.AppId,
				});

				return users.ToList();
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public virtual List<User> GetAllSubordinateUser(int? storeId, string subordinates, bool isAdmin = false)
		{
			if (isAdmin)
			{
				return UserRepository_RO.TableNoTracking.Where(w=>w.StoreId == storeId && w.Deleted == false).ToList();
			}
			else 
			{
				if (string.IsNullOrEmpty(subordinates)) subordinates = string.Empty;
				var str_arr = subordinates.Trim(new char[] { '[', ']' }).Split(",");
				var query = UserRepository_RO.TableNoTracking.Where(x => x.StoreId == storeId && str_arr.Contains(x.Id.ToString()));
				return query.Distinct().ToList().Concat(query.Distinct().ToList().SelectMany(t => GetAllSubordinateUser(storeId, t.Subordinates))).ToList();
			}
		}



		public void AddToken(RefreshToken refreshToken)
		{

			if (refreshToken == null)
			{
				throw new ArgumentNullException("refreshToken");
			}

			var uow = RefreshTokenRepository.UnitOfWork;

			RefreshTokenRepository.Insert(refreshToken);

			uow.SaveChanges();

			//事件通知
			_eventPublisher.EntityInserted(refreshToken);
		}
	}
}

