using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Configuration;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Security
{
	/// <summary>
	///  表示权限服务
	/// </summary>
	public partial class PermissionService : BaseService, IPermissionService
	{
		private readonly IWorkContext _workContext;
		private readonly IUserService _userService;
		
		private readonly DCMSConfig _config;

		public PermissionService(
			IServiceGetter getter,
			IWorkContext workContext,
			IStaticCacheManager cacheManager,
			IUserService userService,
			DCMSConfig config,
		   
			IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
		{
			_workContext = workContext;
			_userService = userService;
			
			_config = config;
		}

		#region Utilities


		/// <summary>
		/// 根据用户角色获取用户权限记录
		/// </summary>
		/// <param name="userRoleId"></param>
		/// <returns></returns>
		protected virtual IList<PermissionRecord> GetPermissionRecordsByUserRoleId(int userRoleId)
		{
			var key = DCMSSecurityDefaults.PermissionsAllByUserRoleIdCacheKey.FillCacheKey(userRoleId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pr in PermissionRecordRepository.Table
							join prcrm in PermissionRecordRolesMappingRepository.Table on pr.Id equals prcrm.PermissionRecord_Id
							where prcrm.UserRole_Id == userRoleId
							orderby pr.Id
							select pr;

				return query.ToList();
			});
		}


		protected virtual bool Authorize(string permissionRecordSystemName, int userRoleId)
		{
			if (string.IsNullOrEmpty(permissionRecordSystemName))
			{
				return false;
			}

			var key = DCMSSecurityDefaults.PermissionsAllowedCacheKey.FillCacheKey(userRoleId, permissionRecordSystemName);
			return _cacheManager.Get(key, () =>
			{
				var permissions = GetPermissionRecordsByUserRoleId(userRoleId);
				foreach (var permission1 in permissions)
				{
					if (permission1.SystemName.Equals(permissionRecordSystemName, StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}

				return false;
			});
		}


		#endregion

		#region Methods


		public virtual void DeletePermissionRecord(PermissionRecord permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException(nameof(permission));
			}

			var uow = PermissionRecordRepository.UnitOfWork;
			PermissionRecordRepository.Delete(permission);
			uow.SaveChanges();

			_eventPublisher.EntityDeleted(permission);
		}

		public virtual PermissionRecord GetPermissionRecordById(int permissionId)
		{
			if (permissionId == 0)
			{
				return null;
			}

			return PermissionRecordRepository.ToCachedGetById(permissionId);
		}

		/// <summary>
		/// 验证当前权限码是否存在
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public bool CheckPermissionRecordCode(int code, int permissionId = 0)
		{
			bool fg = false;
			var query = PermissionRecordRepository.Table;
			query = query.Where(a => a.Code == code);
			//编辑验证
			if (permissionId > 0)
			{
				query = query.Where(a => a.Id != permissionId);
			}

			fg = query.ToList().Count > 0;
			return fg;
		}

		public virtual IList<PermissionRecord> GetPermissionRecordByIds(int[] sIds)
		{
			if (sIds == null || sIds.Length == 0)
			{
				return new List<PermissionRecord>();
			}

			var query = from c in PermissionRecordRepository.Table
						where sIds.Contains(c.Id)
						select c;
			var permissions = query.ToList();

			var sortedPermission = new List<PermissionRecord>();
			foreach (int id in sIds)
			{
				var permission = permissions.Find(x => x.Id == id);
				if (permission != null)
				{
					sortedPermission.Add(permission);
				}
			}
			return sortedPermission;
		}


		public virtual PermissionRecord GetPermissionRecordBySystemName(string systemName)
		{
			if (string.IsNullOrWhiteSpace(systemName))
			{
				return null;
			}

			var query = from pr in PermissionRecordRepository.Table
						where pr.SystemName == systemName
						orderby pr.Id
						select pr;

			var permissionRecord = query.FirstOrDefault();
			return permissionRecord;
		}



		public virtual IPagedList<PermissionRecord> GetAllPermissionRecords(string name, string systemName, int? store, int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;
			var query = PermissionRecordRepository.Table;
			if (store.HasValue && store.Value != 0)
			{
				query = query.Where(u => u.StoreId == store.Value);
			}

			if (!string.IsNullOrEmpty(name))
			{
				query = query.Where(u => u.Name.Contains(name) || u.Module.Name.Contains(name));
			}

			if (!string.IsNullOrEmpty(systemName))
			{
				query = query.Where(u => u.SystemName.Contains(systemName));
			}

			query = query.OrderByDescending(u => u.Id);

			//var permissions = new PagedList<PermissionRecord>(query.ToList(), pageIndex, pageSize);
			//return permissions;

			//总页数
			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<PermissionRecord>(plists, pageIndex, pageSize, totalCount);

		}

		public virtual IList<PermissionRecord> GetAllPermissionRecords()
		{
			var query = from pr in PermissionRecordRepository.Table
						orderby pr.Name
						select pr;

			var query2 = query.Include(u => u.Module);

			var permissions = query2.ToList();
			return permissions;
		}

		/// <summary>
		/// 一定注意：由系统创建的PermissionRecord，必须从 store = 0  取值，不能更改
		/// </summary>
		/// <param name="store"></param>
		/// <returns></returns>
		public virtual IList<PermissionRecord> GetAllPermissionRecordsByStore(int? store)
		{
			var key = DCMSDefaults.PERMISSIONS_BY_ALL_MODULE_KEY.FillCacheKey(0);
			return _cacheManager.Get(key, () =>
			  {
				  try
				  {
					  var query = from pr in PermissionRecordRepository_RO.TableNoTracking
								  where pr.StoreId == 0
								  orderby pr.Id
								  select pr;
					  return query.ToList();
				  }
				  catch (Exception)
				  {
					  return new List<PermissionRecord>();
				  }
			  });
		}


		public virtual IList<PermissionRecord> GetAllPermissionRecordsByModuleId(int? store, int? moduleId, bool showMobile)
		{
			var key = DCMSDefaults.PERMISSIONS_BY_MODULE_KEY.FillCacheKey(store, moduleId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pr in PermissionRecordRepository.TableNoTracking
							where pr.StoreId == (store ?? 0) && pr.ModuleId == moduleId.Value && pr.ShowMobile == showMobile
							orderby pr.Name
							select pr;
				var permissions = query.ToList();
				return permissions;
			});
		}

		public virtual void InsertPermissionRecord(PermissionRecord permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException("permission");
			}

			var uow = PermissionRecordRepository.UnitOfWork;
			PermissionRecordRepository.Insert(permission);
			uow.SaveChanges();

			_eventPublisher.EntityInserted(permission);
		}

		public virtual void InsertPermissionRecord(int? store, PermissionRecord[] permissions)
		{
			if (permissions == null)
			{
				throw new ArgumentNullException("permissions");
			}

			var uow = PermissionRecordRepository.UnitOfWork;
			PermissionRecordRepository.Insert(permissions);
			uow.SaveChanges();

			permissions.ToList().ForEach(s => { _eventPublisher.EntityInserted(s); });
		}


		public virtual void UpdatePermissionRecord(PermissionRecord permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException("permission");
			}

			var uow = PermissionRecordRepository.UnitOfWork;
			PermissionRecordRepository.Update(permission);
			uow.SaveChanges();

			_eventPublisher.EntityUpdated(permission);
		}


		public virtual void InstallPermissions(IPermissionProvider permissionProvider)
		{

		}



		public virtual void UninstallPermissions(IPermissionProvider permissionProvider)
		{
			var permissions = permissionProvider.GetPermissions();
			foreach (var permission in permissions)
			{
				var permission1 = GetPermissionRecordBySystemName(permission.SystemName);
				if (permission1 != null)
				{
					DeletePermissionRecord(permission1);
				}
			}
		}

		public virtual bool Authorize(PermissionRecord permission)
		{
			return Authorize(permission, _workContext.CurrentUser);
		}

		public virtual bool Authorize(PermissionRecord permission, User user)
		{
			if (permission == null)
			{
				return false;
			}

			if (user == null)
			{
				return false;
			}

			return Authorize(permission.SystemName, user);
		}

		public virtual bool Authorize(string permissionRecordSystemName, User user)
		{
			if (string.IsNullOrEmpty(permissionRecordSystemName))
			{
				return false;
			}

			//如果角色缺失，在这里重新获取
			var userRoles = user.UserRoles?.Where(cr => cr?.Active ?? false);
			if ((userRoles?.Count() ?? 0) == 0)
			{
				userRoles = _userService.GetUserRolesByUser(user.StoreId, user.Id);
			}

			//permissionRecordSystemName = PublicStoreAllowNavigation code 424 
			//公共区域访问权限为所有注册用户默认携带，否则登录后不能正常访问管理平台
			foreach (var role in userRoles)
			{
				//管理员用户至高权力（注意：将来存在安全隐患）
				if (!string.IsNullOrEmpty(role.SystemName))
				{
					if (role.SystemName.Equals("Administrators", StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}


				if (Authorize(permissionRecordSystemName, role.Id))
				{
					return true;
				}
			}

			//没有找到权限
			return false;
		}
		public virtual bool ManageAuthorize()
		{
			var user = _workContext.CurrentUser;
			if (user.IsSystemAccount && user.IsPlatformCreate)
			{
				return true;
			}

			return false;
		}
		#endregion

		#region 数据和频道权限

		public virtual DataChannelPermission GetDataChannelPermissionById(int permissionId)
		{
			if (permissionId == 0)
			{
				return null;
			}

			return DataChannelPermissionsRepository.ToCachedGetById(permissionId);
		}
		public virtual IList<DataChannelPermission> GetDataChannelPermissionByIds(int[] sIds)
		{
			if (sIds == null || sIds.Length == 0)
			{
				return new List<DataChannelPermission>();
			}

			var query = from c in DataChannelPermissionsRepository.Table
						where sIds.Contains(c.Id)
						select c;
			var permissions = query.ToList();

			var sortedPermission = new List<DataChannelPermission>();
			foreach (int id in sIds)
			{
				var permission = permissions.Find(x => x.Id == id);
				if (permission != null)
				{
					sortedPermission.Add(permission);
				}
			}
			return sortedPermission;
		}
		public virtual IPagedList<DataChannelPermission> GetAllDataChannelPermissions(int? store, int? roleId, int pageIndex = 0, int pageSize = int.MaxValue)
		{
			if (pageSize >= 50)
				pageSize = 50;
			var query = DataChannelPermissionsRepository.Table;
			if (store.HasValue && store.Value != 0)
			{
				query = query.Where(u => u.StoreId == store.Value);
			}

			if (roleId.HasValue && roleId != 0)
			{
				query = query.Where(u => u.UserRoleId == roleId);
			}

			query = query.OrderByDescending(u => u.Id);

			//var permissions = new PagedList<DataChannelPermission>(query.ToList(), pageIndex, pageSize);
			//return permissions;

			//总页数
			var totalCount = query.Count();
			var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
			return new PagedList<DataChannelPermission>(plists, pageIndex, pageSize, totalCount);

		}
		public virtual IList<DataChannelPermission> GetAllDataChannelPermissionsByStore(int? store)
		{
			var query = from pr in DataChannelPermissionsRepository.Table
						where pr.StoreId == (store ?? 0)
						orderby pr.Id
						select pr;
			var permissions = query.ToList();
			return permissions;
		}
		public virtual IList<DataChannelPermission> GetAllDataChannelPermissionsByRoleId(int? store, int? roleId)
		{
			var key = DCMSDefaults.DADAPERMISSIONS_BY_ROLE_KEY.FillCacheKey(store, roleId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pr in DataChannelPermissionsRepository.Table
							where pr.StoreId == (store ?? 0) && pr.UserRoleId == roleId.Value
							orderby pr.Id
							select pr;
				var permissions = query.ToList();
				return permissions;
			});
		}
		public virtual void InsertDataChannelPermission(DataChannelPermission permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException("permission");
			}

			var uow = DataChannelPermissionsRepository.UnitOfWork;
			DataChannelPermissionsRepository.Insert(permission);
			uow.SaveChanges();

			_eventPublisher.EntityInserted(permission);
		}
		public virtual void UpdateDataChannelPermission(DataChannelPermission permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException("permission");
			}

			var uow = DataChannelPermissionsRepository.UnitOfWork;
			DataChannelPermissionsRepository.Update(permission);
			uow.SaveChanges();

			_eventPublisher.EntityUpdated(permission);
		}
		public virtual void DeleteDataChannelPermission(DataChannelPermission permission)
		{
			if (permission == null)
			{
				throw new ArgumentNullException("permission");
			}

			var uow = DataChannelPermissionsRepository.UnitOfWork;
			DataChannelPermissionsRepository.Delete(permission);
			uow.SaveChanges();

			_eventPublisher.EntityDeleted(permission);
		}

		public virtual bool CheckExist(int? store, int? roleId)
		{
			var query = from pr in DataChannelPermissionsRepository.Table
						where pr.StoreId == (store ?? 0) && pr.UserRoleId == roleId.Value
						orderby pr.Id
						select pr;
			return query.ToList().Count() > 0;
		}

		#endregion

		#region 权限角色映射

		/// <summary>
		///根据权限获取权限角色
		/// </summary>
		/// <param name="permissionId"></param>
		/// <param name="userId"></param>
		/// <param name="storeId"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public virtual IPagedList<PermissionRecordRoles> GetPermissionRecordRolesByPermissionId(int permissionId, int? storeId, int pageIndex, int pageSize)
		{
			if (pageSize >= 50)
				pageSize = 50;
			if (permissionId == 0)
			{
				return new PagedList<PermissionRecordRoles>(new List<PermissionRecordRoles>(), pageIndex, pageSize);
			}

			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_PERMISSIONID_KEY.FillCacheKey(storeId, permissionId, pageIndex, pageSize);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in PermissionRecordRolesMappingRepository.Table
							join p in PermissionRecordRepository.Table on pc.PermissionRecord_Id equals p.Id
							where p.StoreId == storeId && pc.PermissionRecord_Id == permissionId
							orderby pc.Id
							select pc;

				//总页数
				var totalCount = query.Count();
				var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
				return new PagedList<PermissionRecordRoles>(plists, pageIndex, pageSize, totalCount);

			});
		}
		public virtual IList<PermissionRecordRoles> GetPermissionRecordRolesByPermissionId(int? store, int permissionId)
		{

			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_SIGN_PERMISSIONID_KEY.FillCacheKey(store ?? 0, permissionId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in PermissionRecordRolesMappingRepository.Table
							join p in PermissionRecordRepository.Table on pc.PermissionRecord_Id equals p.Id
							where pc.PermissionRecord_Id == permissionId
							orderby pc.Id
							select pc;


				return query.ToList();
			});
		}


		/// <summary>
		/// 根据角色获取角色权限
		/// </summary>
		/// <param name="userRoleId"></param>
		/// <param name="storeId"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public virtual IPagedList<PermissionRecordRoles> GetPermissionRecordRolesByUserRoleId(int userRoleId, int? storeId, int pageIndex, int pageSize)
		{
			if (pageSize >= 50)
				pageSize = 50;
			if (userRoleId == 0)
			{
				return new PagedList<PermissionRecordRoles>(new List<PermissionRecordRoles>(), pageIndex, pageSize);
			}

			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_USERROLEID_KEY.FillCacheKey(storeId, userRoleId, pageIndex, pageSize);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in PermissionRecordRolesMappingRepository.Table
							join p in UserRoleRepository.Table on pc.UserRole_Id equals p.Id
							where p.StoreId == storeId && pc.UserRole_Id == userRoleId
							orderby pc.Id
							select pc;

				//总页数
				var totalCount = query.Count();
				var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
				return new PagedList<PermissionRecordRoles>(plists, pageIndex, pageSize, totalCount);

			});
		}
		public virtual IList<PermissionRecordRoles> GetPermissionRecordRolesByUserRoleId(int? store, int userRoleId)
		{

			var key = DCMSDefaults.GET_PERMISSIONRECORDROLESBY_SIGN_USERROLEID_KEY.FillCacheKey(store ?? 0, userRoleId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in PermissionRecordRolesMappingRepository.Table
							join p in UserRoleRepository.Table on pc.UserRole_Id equals p.Id
							where pc.UserRole_Id == userRoleId
							orderby pc.Id
							select pc;

				return query.ToList();
			});
		}


		public virtual PermissionRecordRoles GetPermissionRecordRolesById(int permissionRecordRolesId)
		{
			if (permissionRecordRolesId == 0)
			{
				return null;
			}

			return PermissionRecordRolesMappingRepository.ToCachedGetById(permissionRecordRolesId);
		}
		public virtual void InsertPermissionRecordRoles(PermissionRecordRoles permissionRecordRoles)
		{
			if (permissionRecordRoles == null)
			{
				throw new ArgumentNullException("permissionRecordRoles");
			}

			var uow = PermissionRecordRolesMappingRepository.UnitOfWork;
			PermissionRecordRolesMappingRepository.Insert(permissionRecordRoles);
			uow.SaveChanges();

			_eventPublisher.EntityInserted(permissionRecordRoles);
		}

		public virtual void UpdatePermissionRecordRoles(PermissionRecordRoles permissionRecordRoles)
		{
			try
			{
				if (permissionRecordRoles == null)
				{
					throw new ArgumentNullException("permissionRecordRoles");
				}

				var uow = PermissionRecordRolesMappingRepository.UnitOfWork;
				PermissionRecordRolesMappingRepository.Update(permissionRecordRoles);
				uow.SaveChanges();

				_eventPublisher.EntityUpdated(permissionRecordRoles);
			}
			catch (Exception ex)
			{
				var flag = ex.Message;
			}

		}
		public virtual void DeletePermissionRecordRoles(PermissionRecordRoles permissionRecordRoles)
		{
			if (permissionRecordRoles == null)
			{
				throw new ArgumentNullException("permissionRecordRoles");
			}

			permissionRecordRoles = PermissionRecordRolesMappingRepository.Table.FirstOrDefault(c => c.Id == permissionRecordRoles.Id);

			var uow = PermissionRecordRolesMappingRepository.UnitOfWork;
			PermissionRecordRolesMappingRepository.Delete(permissionRecordRoles);
			uow.SaveChanges();

			_eventPublisher.EntityDeleted(permissionRecordRoles);
		}

		#endregion


		/// <summary>
		/// 获取用户权限码
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public List<string> GetUserAuthorizeCodesByUserId(int storeId, int userId, bool platform = false)
		{

			List<string> codes = new List<string>();
			string sql = string.Format(@"select distinct CONVERT(c.Code,CHAR) as 'Value' from User_UserRole_Mapping a
											inner join PermissionRecord_Role_Mapping b on a.UserRoleId = b.UserRole_Id
											inner join PermissionRecord c on b.PermissionRecord_Id = c.Id
											where  b.StoreId ='{0}' and a.UserId = '{1}'  and Platform = {2} order by 'Value' asc", storeId, userId, platform);

			var key = DCMSDefaults.USERAUTHORIZECODES_BY_USERID_KEY.FillCacheKey(storeId, userId, platform);
			return _cacheManager.Get(key, () => UserRepository.QueryFromSql<StringQueryType>(sql).Select(c => c.Value).ToList());
		}

		/// <summary>
		/// 更新权限配置
		/// </summary>
		/// <param name="storeId"></param>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		/// <param name="psmIds">PC 模块</param>
		/// <param name="pspIds">PC 权限</param>
		/// <param name="asmIds">AAP 模块</param>
		/// <param name="aspIds">AAP 权限</param>
		/// <param name="dcp">频道</param>
		/// <returns></returns>
		public BaseResult PermissionsSave(int storeId,
			int userId,
			int roleId,
			List<int> psmIds,
			List<int> pspIds,
			List<int> asmIds,
			List<int> aspIds,
			DataChannelPermission dcp)
		{
			var uow = PermissionRecordRepository.UnitOfWork;
			ITransaction transaction = null;
			try
			{
				transaction = uow.BeginOrUseTransaction();

				#region 去重

				if (psmIds != null && psmIds.Count > 0)
				{
					psmIds = psmIds.Distinct().ToList();
				}
				if (pspIds != null || pspIds.Count > 0)
				{
					pspIds = pspIds.Distinct().ToList();
				}

				if (asmIds != null && asmIds.Count > 0)
				{
					asmIds = asmIds.Distinct().ToList();
				}
				if (aspIds != null && aspIds.Count > 0)
				{
					aspIds = aspIds.Distinct().ToList();
				}

				#endregion

				#region 更新 Module_Role_Mapping

				//原有选择模块（注意这里暂时不分PC和APP）
				var oldModuleRoles = ModuleRoleRepository.Table.Where(mr => mr.UserRole_Id == roleId).ToList();

				//PC选择模块
				if (psmIds != null && psmIds.Count > 0)
				{
					psmIds.ForEach(pm =>
					{
						//如果旧的记录中不包含新选的的项，则添加新项到记录中
						if (oldModuleRoles == null || oldModuleRoles.Count == 0 || oldModuleRoles.Where(om => om.Module_Id == pm).Count() == 0)
						{
							//插入 Module_Role_Mapping
							ModuleRole moduleRole = new ModuleRole()
							{
								StoreId = storeId,
								Module_Id = pm,
								UserRole_Id = roleId
							};
							ModuleRoleRepository.Insert(moduleRole);

							//缓存
							moduleRole.StoreId = storeId;
							_eventPublisher.EntityInserted(moduleRole);
						}
					});
				}
				if (oldModuleRoles != null && oldModuleRoles.Count > 0)
				{
					oldModuleRoles.ForEach(om =>
					{
						//如果新保存选择的模块在旧的记录中不存在（没有被勾选，则说明移除掉） 则从记录中移除掉该模块
						if (psmIds == null || psmIds.Count == 0 || psmIds.Where(ps => ps == om.Module_Id).Count() == 0)
						{
							//删除 Module_Role_Mapping
							ModuleRoleRepository.Delete(om);

							//缓存
							om.StoreId = storeId;
							_eventPublisher.EntityDeleted(om);
						}
					});
				}
				//APP选择模块（暂时不做） 



				#endregion

				#region 更新 PermissionRecord_Role_Mapping

				//原有选择权限

				//注意判断唯一
				var oldPCPermissionRecordRoles = PermissionRecordRolesMappingRepository.Table
					.Where(pr => pr.UserRole_Id == roleId && pr.Platform == 0 && pr.StoreId == storeId)
					.ToList();

				var oldAPPPermissionRecordRoles = PermissionRecordRolesMappingRepository.Table
				 .Where(pr => pr.UserRole_Id == roleId && pr.Platform == 1 && pr.StoreId == storeId)
				 .ToList();

				//PC选择权限
				if (pspIds != null && pspIds.Count > 0)
				{
					pspIds.ForEach(pp =>
					{
						if (oldPCPermissionRecordRoles == null || oldPCPermissionRecordRoles.Count == 0 || oldPCPermissionRecordRoles.Where(op => op.PermissionRecord_Id == pp).Count() == 0)
						{
							var prr = new PermissionRecordRoles()
							{
								StoreId = storeId,
								PermissionRecord_Id = pp,
								UserRole_Id = roleId,
								Platform = 0
							};
							PermissionRecordRolesMappingRepository.Insert(prr);
						}
					});
				}
				if (oldPCPermissionRecordRoles != null && oldPCPermissionRecordRoles.Count > 0)
				{
					oldPCPermissionRecordRoles.ForEach(op =>
					{
						if (pspIds == null || pspIds.Count == 0 || pspIds.Where(ps => ps == op.PermissionRecord_Id).Count() == 0)
						{
							PermissionRecordRolesMappingRepository.Delete(op);
						}
					});
				}



				//APP选择权限
				if (aspIds != null && aspIds.Count > 0)
				{
					aspIds.ForEach(pp =>
					{
						if (oldAPPPermissionRecordRoles == null || oldAPPPermissionRecordRoles.Count == 0 || oldAPPPermissionRecordRoles.Where(op => op.PermissionRecord_Id == pp).Count() == 0)
						{
							var prr = new PermissionRecordRoles()
							{
								StoreId = storeId,
								PermissionRecord_Id = pp,
								UserRole_Id = roleId,
								Platform = 1
							};
							PermissionRecordRolesMappingRepository.Insert(prr);
						}
					});
				}
				if (oldAPPPermissionRecordRoles != null && oldAPPPermissionRecordRoles.Count > 0)
				{
					oldAPPPermissionRecordRoles.ForEach(op =>
					{
						if (aspIds == null || aspIds.Count == 0 || aspIds.Where(ps => ps == op.PermissionRecord_Id).Count() == 0)
						{
							PermissionRecordRolesMappingRepository.Delete(op);
						}
					});
				}

				#endregion

				#region 更新数据和频道权限
				var permission = GetAllDataChannelPermissionsByRoleId(storeId, roleId).FirstOrDefault();
				if (permission != null)
				{
					permission.ViewPurchasePrice = dcp.ViewPurchasePrice;
					permission.PlaceOrderPricePermitsLowerThanMinPriceOnWeb = dcp.PlaceOrderPricePermitsLowerThanMinPriceOnWeb;
					permission.APPSaleBillsAllowPreferences = dcp.APPSaleBillsAllowPreferences;
					permission.APPAdvanceReceiptFormAllowsPreference = dcp.APPAdvanceReceiptFormAllowsPreference;
					permission.MaximumDiscountAmount = dcp.MaximumDiscountAmount;
					permission.APPSaleBillsAllowArrears = dcp.APPSaleBillsAllowArrears;
					permission.AppOpenChoiceGift = dcp.AppOpenChoiceGift;
					permission.PrintingIsNotAudited = dcp.PrintingIsNotAudited;
					permission.AllowViewReportId = dcp.AllowViewReportId;
					permission.APPAllowModificationUserInfo = dcp.APPAllowModificationUserInfo;
					permission.Auditcompleted = dcp.Auditcompleted;
					permission.EnableSchedulingCompleted = dcp.EnableSchedulingCompleted;
					permission.EnableInventoryCompleted = dcp.EnableInventoryCompleted;
					permission.EnableTransfeCompleted = dcp.EnableTransfeCompleted;
					permission.EnableStockEarlyWarning = dcp.EnableStockEarlyWarning;
					permission.EnableUserLossWarning = dcp.EnableUserLossWarning;
					permission.EnableBillingException = dcp.EnableBillingException;
					permission.EnableCompletionOrCancellationOfAccounts = dcp.EnableCompletionOrCancellationOfAccounts;
					permission.EnableApprovalAcl = dcp.EnableApprovalAcl;
					permission.EnableReceivablesAcl = dcp.EnableReceivablesAcl;
					permission.EnableAccountAcl = dcp.EnableAccountAcl;
					permission.EnableDataAuditAcl = dcp.EnableDataAuditAcl;

					UpdateDataChannelPermission(permission);
				}
				else
				{
					dcp.StoreId = storeId;
					dcp.UserRoleId = roleId;
					InsertDataChannelPermission(dcp);
				}
				#endregion

				//保存事务
				transaction.Commit();

				return new BaseResult { Success = true, Message = "权限创建/更新成功" };
			}
			catch (Exception)
			{
				transaction?.Rollback();
				return new BaseResult { Success = false, Message = "权限创建/更新失败" };
			}
			finally
			{
				_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PERMISSIONS_PK, storeId));
				_cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.USER_PK, storeId));
				using (transaction) { }
			}
		}


		public BaseResult ManagePermissionsSave(int storeId, int roleId, List<int> pcModuleIds, List<int> pcPermissionIds)
		{
			var uow = PermissionRecordRepository.UnitOfWork;
			ITransaction transaction = null;
			try
			{
				transaction = uow.BeginOrUseTransaction();

				#region 去重

				if (pcModuleIds != null && pcModuleIds.Count > 0)
				{
					pcModuleIds = pcModuleIds.Distinct().ToList();
				}
				if (pcPermissionIds != null || pcPermissionIds.Count > 0)
				{
					pcPermissionIds = pcPermissionIds.Distinct().ToList();
				}
				#endregion

				#region 更新 Module_Role_Mapping

				//原有选择模块（注意这里暂时不分PC和APP）
				var oldModuleRoles = ModuleRoleRepository.Table.Where(mr => mr.UserRole_Id == roleId).ToList();
				//PC选择模块
				if (pcModuleIds != null && pcModuleIds.Count > 0)
				{
					pcModuleIds.ForEach(pm =>
					{
						//如果旧的记录中不包含新选的的项，则添加新项到记录中
						if (oldModuleRoles == null || oldModuleRoles.Count == 0 || oldModuleRoles.Where(om => om.Module_Id == pm).Count() == 0)
						{
							//插入 Module_Role_Mapping
							ModuleRole moduleRole = new ModuleRole()
							{
								Module_Id = pm,
								UserRole_Id = roleId
							};
							ModuleRoleRepository.Insert(moduleRole);
						}
					});
				}

				if (oldModuleRoles != null && oldModuleRoles.Count > 0)
				{
					oldModuleRoles.ForEach(om =>
					{
						if (pcModuleIds == null || pcModuleIds.Count == 0 || pcModuleIds.Where(ps => ps == om.Module_Id).Count() == 0)
						{
							//删除 Module_Role_Mapping
							ModuleRoleRepository.Delete(om);
						}
					});
				}
				#endregion

				#region 更新 PermissionRecord_Role_Mapping
				var oldPCPermissionRecordRoles = PermissionRecordRolesMappingRepository.Table
					.Where(pr => pr.UserRole_Id == roleId && pr.Platform == 0 && pr.StoreId == storeId)
					.ToList();

				//PC选择权限
				if (pcPermissionIds != null && pcPermissionIds.Count > 0)
				{
					pcPermissionIds.ForEach(pp =>
					{
						if (oldPCPermissionRecordRoles == null || oldPCPermissionRecordRoles.Count == 0 || oldPCPermissionRecordRoles.Where(op => op.PermissionRecord_Id == pp).Count() == 0)
						{
							var prr = new PermissionRecordRoles()
							{
								StoreId = storeId,
								PermissionRecord_Id = pp,
								UserRole_Id = roleId,
								Platform = 0
							};
							PermissionRecordRolesMappingRepository.Insert(prr);
						}
					});
				}
				if (oldPCPermissionRecordRoles != null && oldPCPermissionRecordRoles.Count > 0)
				{
					oldPCPermissionRecordRoles.ForEach(op =>
					{
						if (pcPermissionIds == null || pcPermissionIds.Count == 0 || pcPermissionIds.Where(ps => ps == op.PermissionRecord_Id).Count() == 0)
						{
							PermissionRecordRolesMappingRepository.Delete(op);
						}
					});
				}

				uow.SaveChanges();
				#endregion

				//保存事务
				transaction.Commit();

				return new BaseResult { Success = true, Message = "权限创建/更新成功" };
			}
			catch (Exception)
			{
				transaction?.Rollback();
				return new BaseResult { Success = false, Message = "权限创建/更新失败" };
			}
			finally
			{
				using (transaction) { }
			}
		}
	}
}