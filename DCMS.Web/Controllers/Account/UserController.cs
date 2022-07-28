using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.plan;
using DCMS.Services.Security;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.UI;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
	public partial class UserController : BasePublicController
	{
		private readonly UserSettings _userSettings;
		private readonly IUserActivityService _userActivityService;
		private readonly IUserRegistrationService _userRegistrationService;
		private readonly IUserService _userService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IPermissionService _permissionService;
		private readonly IBranchService _branchService;
		private readonly IDistrictService _districtService;
		private readonly IPercentagePlanService _percentagePlanService;
		private readonly IEncryptionService _encryptionService;


		public UserController(
			UserSettings userSettings,
			IUserActivityService userActivityService,
			IUserRegistrationService userRegistrationService,
			IUserService userService,
			IGenericAttributeService genericAttributeService,
			IStoreContext storeContext,
			IWorkContext workContext,
			ILogger loggerService,
			INotificationService notificationService,
			IPermissionService permissionService,
			IBranchService branchService,
			IEncryptionService encryptionService,
			IDistrictService districtService,
			IPercentagePlanService percentagePlanService) : base(workContext, loggerService, storeContext, notificationService)
		{
			_userSettings = userSettings;
			_userActivityService = userActivityService;
			_userRegistrationService = userRegistrationService;
			_userService = userService;
			_genericAttributeService = genericAttributeService;
			_branchService = branchService;
			_districtService = districtService;
			_permissionService = permissionService;
			_percentagePlanService = percentagePlanService;
			_encryptionService = encryptionService;
		}


		public IActionResult Index()
		{
			return RedirectToAction("List");
		}


		[NonAction]
		protected UserModel PrepareUserModelForList(Dictionary<int, string> allUserRoles,User user)
		{
			var userRole = allUserRoles.Where(s => s.Key == user.Id).FirstOrDefault();
			return new UserModel()
			{
				Id = user.Id,
				Email = user.Email,
				Username = user.Username,
				MobileNumber = user.MobileNumber,
				UserRealName = user.UserRealName,
				Gender = user.GetAttribute<string>(DCMSDefaults.GenderAttribute),
				UserRoleNames = userRole.Value,
				Active = user.Active,
				AccountType = user.StoreId,
				CreatedOn = user.CreatedOnUtc,
				LastActivityDate = user.LastActivityDateUtc,
				UseACLMobile = user.UseACLMobile,
				UseACLPc = user.UseACLPc
			};
		}

		/// <summary>
		/// 获取用户角色名
		/// </summary>
		/// <param name="userRoles"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		[NonAction]
		protected string GetUserRolesNames(IList<UserRole> userRoles, string separator = ",")
		{
			var sb = new StringBuilder();
			for (int i = 0; i < userRoles.Count; i++)
			{
				sb.Append(userRoles[i].Name);
				if (i != userRoles.Count - 1)
				{
					sb.Append(separator);
					sb.Append(" ");
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// 验证用户角色
		/// </summary>
		/// <param name="userRoles"></param>
		/// <returns></returns>
		[NonAction]
		protected string ValidateUserRoles(IList<UserRole> userRoles)
		{
			if (userRoles == null)
			{
				throw new ArgumentNullException("userRoles");
			}

			bool isInEmployeesRole = userRoles.FirstOrDefault(cr => cr.SystemName == DCMSDefaults.Employees) != null;
			if (!isInEmployeesRole)
			{
				return "用户最小角色应为员工";
			}

			return "";
		}


		/// <summary>
		/// 用户管理列表
		/// </summary>
		/// <returns></returns>
		[AuthCode((int)AccessGranularityEnum.UserListView)]
		public IActionResult List(int[] searchUserRoleIds, string SearchUsername, DateTime? StartTime = null, DateTime? EndTime = null, int Branchid = 0, int pagenumber = 0)
		{
			var defaultRoleIds = new[] { _userService.GetUserRoleBySystemName(curStore.Id, DCMSDefaults.Employees)?.Id ?? 0 };
			var listModel = new UserListModel()
			{
				UsernamesEnabled = _userSettings.UsernamesEnabled,
				AvailableUserRoles = _userService.GetAllUserRoles(0, true).Select(cr => cr.ToModel<UserRoleModel>()).ToList(),
				SearchUserRoleIds = defaultRoleIds,
			};
			listModel.StartTime = ((StartTime == null) ? DateTime.Parse(DateTime.Now.ToString("2010-MM-01")) : StartTime) ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
			listModel.EndTime = ((EndTime == null) ? DateTime.Now.AddDays(1) : EndTime) ?? DateTime.Now.AddDays(1);
			listModel.SearchUsername = SearchUsername;
			return View(listModel);
		}

		public async Task<JsonResult> UserSimplifiedList(int[] searchUserRoleIds, string SearchUsername, DateTime? StartTime = null, DateTime? EndTime = null, int Branchid = 0, int pagenumber = 0)
		{
			return await Task.Run(() =>
			{
				int totalCount = 0;
				if (pagenumber > 1)
				{
					pagenumber -= 1;
				}

				var users = _userService.GetAllUsers(
					store: curStore?.Id ?? 0,
					userRoleIds: searchUserRoleIds,
					username: SearchUsername,
					realName: "",
					branchid: Branchid,
					createdFromUtc: ((StartTime == null) ? DateTime.Parse(DateTime.Now.ToString("2010-MM-01")) : StartTime) ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")),
					createdToUtc: ((EndTime == null) ? DateTime.Now.AddDays(1) : EndTime) ?? DateTime.Now.AddDays(1),
					pageIndex: pagenumber,
					pageSize: 30);
				totalCount = users.TotalCount;


				var allUserRoles = _userService.GetAllUserRolesByUser(curStore.Id);

				var userModel = users.Select(s=> PrepareUserModelForList(allUserRoles, s)).ToList();

				return Json(new
				{
					Success = true,
					total = totalCount,
					rows = userModel
				});
			});
		}

		/// <summary>
		/// 创建用户
		/// </summary>
		/// <returns></returns>   
		[HttpGet]
		[AuthCode((int)AccessGranularityEnum.UserListUpdate)]
		public IActionResult Create(int? store)
		{

			var model = new UserModel
			{
				UsernamesEnabled = _userSettings.UsernamesEnabled,
				AllowUsersToChangeUsernames = _userSettings.AllowUsersToChangeUsernames,
				MobileNumber = curUser.GetAttribute<string>(DCMSDefaults.PhoneAttribute)
			};

			var lists = new List<SelectListItem>
			{
				new SelectListItem() { Text = curStore.Name, Value = curStore.Id.ToString(), Selected = (curStore.Id == curStore.Id) }
			};
			model.AccountType = store ?? curStore.Id;
			model.AccountTypes = new SelectList(lists, "Value", "Text");


			//组织机构
			model = BindDropDownList<UserModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);

			model.Dirtleaders = BindUserSelection(_userService.BindUserList, curStore, "");

			//业务员提成方案
			model.SalesmanExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindBusinessPercentagePlanList, curStore);

			//送货员提成方案
			model.DeliverExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindDeliverPercentagePlanList, curStore);

			//用户角色
			model.AvailableUserRoles = _userService
				.GetAllUserRolesByStore(true, curStore?.Id ?? 0)
				.Select(cr => cr.ToModel<UserRoleModel>())
				.ToList();

			//添加员工时增加默认角色  zsh20200417
			//if (model.AvailableUserRoles.Count > 0)
			//{
			//    model.SelectedUserRoleIds = new int[] { model.AvailableUserRoles.Select(r => r).Where(r => r.SystemName == "Employees").First().Id };
			//}

			model.AllowManagingUserRoles = _permissionService.Authorize(StandardPermissionProvider.ManageUserRoles);

			//片区
			var areas = _districtService.GetAllDistrictByStoreId(curStore?.Id ?? 0);
			model.AvailableUserDistricts = areas.Select(a =>
			{
				return new UserDistrictsModel()
				{
					Id = a.Id,
					DistrictsName = a.Name,
					DistrictsId = a.Id,
					UserId = curUser.Id,
					UserName = curUser.Username
				};
			}).ToList();


			//表单字段
			model.GenderEnabled = true;
			model.DateOfBirthEnabled = true;
			model.StreetAddressEnabled = true;
			model.CityEnabled = true;
			model.CountryEnabled = true;
			model.StateProvinceEnabled = true;
			model.PhoneEnabled = true;
			//default value
			model.Active = true;


			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[AuthCode((int)AccessGranularityEnum.UserListUpdate)]
		public IActionResult Create(UserModel model, bool continueEditing)
		{
			if (string.IsNullOrWhiteSpace(model.Password))
			{
				ModelState.AddModelError("", "密码不能为空");
			}

			if (!string.IsNullOrWhiteSpace(model.Email))
			{
				var cust2 = _userService.GetUserByEmail(curStore.Id, model.Email, includeDelete : false);
				if (cust2 != null)
				{
					ModelState.AddModelError("", "邮箱已经注册");
				}
			}

			if (!string.IsNullOrWhiteSpace(model.Username))
			{
				var cust2 = _userService.GetUserByUsername(curStore.Id, model.Username, includeDelete: false);
				if (cust2 != null)
				{
					ModelState.AddModelError("", "用户名已经注册");
				}
			}

			if (!string.IsNullOrWhiteSpace(model.MobileNumber))
			{
				var cust2 = _userService.GetUserByMobileNamber(curStore.Id, model.MobileNumber, includeDelete: false);
				if (cust2 != null)
				{
					ModelState.AddModelError("", "手机号已经注册");
				}
			}


			//如果选择用户存在下级继承，那么该选择用户的上级不能被指定为任何下级成员
			//如果选择用户不存在下级继承，则上级不能指定为自己
			var subs = _userService.GetAllSubordinateByUserId(model.Id, curStore.Id);
			if (subs != null && subs.Any())
			{
				if (subs.Contains(model.Dirtleader ?? 0))
				{
					ModelState.AddModelError("", "不能把自己的下级指定为上级");
				}
			}
			else
			{
				if (model.Dirtleader == model.Id)
				{
					ModelState.AddModelError("", "不能自己指定自己");
				}
			}

			//validate user roles SelectedUserRoleIds
			//var allUserRoles = _userService.GetAllUserRoles(0, true);        // -----修改前的功能
			var allUserRoles = _userService.GetAllUserRolesByStore(true, curStore?.Id ?? 0).Distinct().ToList();
			var newUserRoles = new List<UserRole>();
			foreach (var userRole in allUserRoles)
			{
				if (model.SelectedUserRoleIds != null && model.SelectedUserRoleIds.Contains(userRole.Id))
				{
					newUserRoles.Add(userRole);
				}
			}

			//var userRolesError = ValidateUserRoles(newUserRoles);
			//if (!String.IsNullOrEmpty(userRolesError))
			//{
			//    ModelState.AddModelError("", userRolesError);
			//    _notificationService.ErrorNotification(userRolesError, false);
			//}
			bool allowManagingUserRoles = _permissionService.Authorize(StandardPermissionProvider.ManageUserRoles);


			if (ModelState.IsValid)
			{
				var user = new User()
				{
					UserGuid = Guid.NewGuid().ToString(),
					StoreId = curStore?.Id ?? 0,
					Email = model.Email,
					Username = model.Username,
					UserRealName = model.UserRealName,
					Dirtleader = model.Dirtleader ?? 0,//直接上级
					AdminComment = "",
					IsTaxExempt = false,
					Active = model.Active,
					CreatedOnUtc = DateTime.UtcNow,
					MobileNumber = model.MobileNumber,
					AccountType = model.AccountType ?? 0,
					BranchId = model.BranchId,
					SalesmanExtractPlanId = model.SalesmanExtractPlanId,
					DeliverExtractPlanId = model.DeliverExtractPlanId,
					MaxAmountOfArrears = model.MaxAmountOfArrears,
					LastActivityDateUtc = DateTime.UtcNow,
					UseACLMobile= model.UseACLMobile,
					UseACLPc = model.UseACLPc
				};

				//表单字段控制
				_genericAttributeService.SaveAttribute(user, DCMSDefaults.GenderAttribute, model.Gender, curStore?.Id ?? 0);
				_genericAttributeService.SaveAttribute(user, DCMSDefaults.RealNameAttribute, model.UserRealName, curStore?.Id ?? 0);

				//密码
				if (!string.IsNullOrWhiteSpace(model.Password))
				{
					string saltKey = _encryptionService.CreateSaltKey(5);
					user.PasswordSalt = saltKey;
					user.Password = _encryptionService.CreatePasswordHash(model.Password, saltKey, _userSettings.HashedPasswordFormat);
					user.PasswordFormat = PasswordFormat.Hashed;
					user.PasswordFormatId = (int)PasswordFormat.Hashed;
				}

				//添加用户
				_userService.InsertUser(user);

				//用户角色
				if (allowManagingUserRoles)
				{
					foreach (var userRole in newUserRoles)
					{
						user.UserRoles.Add(userRole);
					}

					foreach (var userRole in user.UserRoles.Distinct().ToList())
					{
						_userService.InsertUserRoleMapping(user.Id, userRole.Id, curStore.Id);
					}

					_userService.UpdateUser(user);
				}

				//用户存储
				//var result = _userService.InitializeData(user);

				//活动日志
				_userActivityService.InsertActivity("AddNewUser", "添加新用户", curUser, curUser.Id);

				_notificationService.SuccessNotification("添加新用户");
				return continueEditing ? RedirectToAction("Edit", new { id = curUser.Id }) : RedirectToAction("List");
			}

			model.UsernamesEnabled = _userSettings.UsernamesEnabled;
			model.AllowUsersToChangeUsernames = _userSettings.AllowUsersToChangeUsernames;


			//用户角色
			//model.AvailableUserRoles = _userService
			//    .GetAllUserRoles(true)
			//    .Select(cr => cr.ToModel<UserRoleModel>())
			//    .ToList();

			model.AvailableUserRoles = _userService
				.GetAllUserRolesByStore(true, curStore?.Id ?? 0)
				.Select(cr => cr.ToModel<UserRoleModel>())
				.ToList();
			model.AllowManagingUserRoles = allowManagingUserRoles;

			var lists = new List<SelectListItem>
			{
				new SelectListItem() 
				{ 
					Text = curStore.Name, 
					Value = curStore.Id.ToString(), 
					Selected = (curStore.Id == curStore.Id) 
				}
			};
			model.AccountType = curStore.Id;
			model.AccountTypes = new SelectList(lists, "Value", "Text");

			//组织机构
			model = BindDropDownList<UserModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);

			model.Dirtleaders = BindUserSelection(_userService.BindUserList, curStore, "");

			//业务员提成方案
			model.SalesmanExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindBusinessPercentagePlanList, curStore);

			//送货员提成方案
			model.DeliverExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindDeliverPercentagePlanList, curStore);

			//片区
			var areas = _districtService.GetAllDistrictByStoreId(curStore?.Id ?? 0);
			model.AvailableUserDistricts = areas.Select(a =>
			{
				return new UserDistrictsModel()
				{
					Id = a.Id,
					DistrictsName = a.Name,
					DistrictsId = a.Id,
					UserId = model.Id,
					UserName = model.Username
				};
			}).ToList();

			//这里将来做到配置管理
			model.GenderEnabled = true;
			model.DateOfBirthEnabled = true;
			model.StreetAddressEnabled = true;
			model.CityEnabled = true;
			model.CountryEnabled = true;
			model.StateProvinceEnabled = true;
			model.PhoneEnabled = true;

			return View(model);
		}


		/// <summary>
		/// 编辑用户
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AuthCode((int)AccessGranularityEnum.UserListUpdate)]
		public IActionResult Edit(int id = 0)
		{
			var user = _userService.GetUserById(curStore.Id, id);
			if (user == null || user.Deleted)
			{
				return RedirectToAction("List");
			}

			var model = new UserModel
			{
				Id = user.Id,
				Email = user.Email,
				Username = user.Username,
				//Gender=user.ge
				Dirtleader = user.Dirtleader,

				Active = user.Active,
				UsernamesEnabled = _userSettings.UsernamesEnabled,
				AllowUsersToChangeUsernames = _userSettings.AllowUsersToChangeUsernames,
				CreatedOn = user.CreatedOnUtc,
				LastActivityDate = user.LastActivityDateUtc,
				LastIpAddress = user.LastIpAddress,
				LastVisitedPage = user.GetAttribute<string>(DCMSDefaults.LastVisitedPageAttribute),


				UserRealName = string.IsNullOrEmpty(user.UserRealName) ? user.GetAttribute<string>(DCMSDefaults.RealNameAttribute) : user.UserRealName,
				Gender = user.GetAttribute<string>(DCMSDefaults.GenderAttribute),
				DateOfBirth = user.GetAttribute<DateTime?>(DCMSDefaults.DateOfBirthAttribute),
				StreetAddress = user.GetAttribute<string>(DCMSDefaults.StreetAddressAttribute),
				City = user.GetAttribute<string>(DCMSDefaults.CityAttribute),
				CountryId = user.GetAttribute<int>(DCMSDefaults.CurrencyIdAttribute),
				StateProvinceId = user.GetAttribute<int>(DCMSDefaults.StateProvinceIdAttribute),
				MobileNumber = string.IsNullOrEmpty(user.MobileNumber) ? user.GetAttribute<string>(DCMSDefaults.PhoneAttribute) : user.MobileNumber,

				//maxiaotong
				GenderEnabled = true,
				DateOfBirthEnabled = true,
				StreetAddressEnabled = true,
				CityEnabled = true,
				CountryEnabled = true,
				StateProvinceEnabled = true,
				PhoneEnabled = true,

				UseACLMobile = user.UseACLMobile,
				UseACLPc = user.UseACLPc,

				BranchId = user.BranchId ?? 0,
				SalesmanExtractPlanId = user.SalesmanExtractPlanId,
				DeliverExtractPlanId = user.DeliverExtractPlanId,
				MaxAmountOfArrears = user.MaxAmountOfArrears
			};

			var lists = new List<SelectListItem>
			{
				new SelectListItem() { Text = curStore.Name, Value = curStore.Id.ToString(), Selected = (curStore.Id == curStore.Id) }
			};
			model.AccountType = user.AccountType;
			model.AccountTypes = new SelectList(lists, "Value", "Text");

			//用户角色 !store.HasValue ? curStore.Id : store.Value
			model.AvailableUserRoles = _userService
				.GetAllUserRolesByStore(true, curStore?.Id ?? 0)
				.Select(cr => cr.ToModel<UserRoleModel>())
				.ToList();
			model.SelectedUserRoleIds = _userService.GetUserRolesByUser(user.StoreId, user.Id).Select(cr => cr.Id).ToArray();
			model.AllowManagingUserRoles = _permissionService.Authorize(StandardPermissionProvider.ManageUserRoles);


			//组织机构
			model = BindDropDownList<UserModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);

			model.Dirtleaders = BindUserSelection(_userService.BindUserList, curStore, "");

			//业务员提成方案
			model.SalesmanExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindBusinessPercentagePlanList, curStore);

			//送货员提成方案
			model.DeliverExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindDeliverPercentagePlanList, curStore);

			//片区
			var areas = _districtService.GetAllDistrictByStoreId(curStore?.Id ?? 0);
			model.AvailableUserDistricts = areas.Select(a =>
			{
				return new UserDistrictsModel()
				{
					Id = a.Id,
					DistrictsName = a.Name,
					DistrictsId = a.Id,
					UserId = curUser.Id,
					UserName = user.Username
				};
			}).ToList();

			var districts = _userService
				.GetAllUserDistrictsByUserId(curStore.Id, user.Id)?.Select(cr => cr.DistrictId)?.ToList()?.ToArray();

			model.SelectedUserDistricts = districts ?? new int[] { };

			var nodes = _districtService.GetListZTreeVM(curStore?.Id ?? 0);
			//是否拥有片区
			foreach (var node in nodes)
			{
				if (model.SelectedUserDistricts.Contains(Convert.ToInt32(node.id)))
				{
					node.@checked = true;
				}
			}
			model.UserDistrictsZTree = JsonConvert.SerializeObject(nodes);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[AuthCode((int)AccessGranularityEnum.UserListUpdate)]
		public IActionResult Edit(UserModel model, bool continueEditing)
		{

			model.Dirtleader = model.Dirtleader ?? 0;
			var user = _userService.GetUserById(curStore.Id, model.Id);
			if (user == null || user.Deleted)
			{
				return RedirectToAction("List");
			}
			
			var allUserRoles = _userService.GetAllUserRoles(curStore.Id, true);
			var userRoles = _userService.GetUserRolesByUser(user.StoreId, user.Id);
			var newUserRoles = new List<UserRole>();
			foreach (var userRole in allUserRoles)
			{
				if (model.SelectedUserRoleIds != null && model.SelectedUserRoleIds.Contains(userRole.Id))
				{
					newUserRoles.Add(userRole);
				}
			}

			//如果选择用户存在下级继承，那么该选择用户的上级不能被指定为任何下级成员
			//如果选择用户不存在下级继承，则上级不能指定为自己
			var subs = _userService.GetAllSubordinateByUserId(model.Id, curStore.Id);
			if (subs != null && subs.Any())
			{
				if (subs.Contains(model.Dirtleader ?? 0))
				{
					//ModelState.AddModelError("", "不能把自己的下级指定为上级");
					return RedirectToAction("List");
				}
			}
			else
			{
				if (model.Dirtleader == model.Id)
				{
					//ModelState.AddModelError("", "不能自己指定自己");
					return RedirectToAction("List");
				}
			}

			bool allowManagingUserRoles = _permissionService.Authorize(StandardPermissionProvider.ManageUserRoles);


			if (ModelState.IsValid)
			{
				var areas = _userService.GetAllUserDistrictsByUserId(curStore.Id, curUser.Id);
				try
				{
					user.Active = model.Active;
					user.Email = model.Email;
					user.Username = model.Username;
					user.AccountType = model.AccountType ?? 0;
					user.MobileNumber = model.MobileNumber;
					user.UserRealName = model.UserRealName;

					_genericAttributeService.SaveAttribute(user, DCMSDefaults.GenderAttribute, model.Gender);
					_genericAttributeService.SaveAttribute(user, DCMSDefaults.RealNameAttribute, model.UserRealName);


					if (allowManagingUserRoles)
					{
						foreach (var userRole in allUserRoles)
						{

							if (model.SelectedUserRoleIds != null && model.SelectedUserRoleIds.Contains(userRole.Id))
							{
								//new role
								if (userRoles.Count(cr => cr.Id == userRole.Id) == 0)
								{
									_userService.InsertUserRoleMapping(user.Id, userRole.Id, curStore.Id);
								}
							}
							else
							{
								//removed role
								if (userRoles.Count(cr => cr.Id == userRole.Id) > 0)
								{
									_userService.RemoveUserRoleMapping(user.Id, userRole.Id, curStore.Id);
								}
							}
						}

					}

					user.UseACLMobile = model.UseACLMobile;
					user.UseACLPc = model.UseACLPc;
					user.BranchId = model.BranchId;
					user.SalesmanExtractPlanId = model.SalesmanExtractPlanId;
					user.DeliverExtractPlanId = model.DeliverExtractPlanId;
					user.MaxAmountOfArrears = model.MaxAmountOfArrears;
					user.Dirtleader = model.Dirtleader; //直接上级

					_userService.UpdateUser(user);

					////密码
					//if (!String.IsNullOrWhiteSpace(model.Password))
					//{
					//    var changePassRequest = new ChangePasswordRequest(user.Email, false, PasswordFormat.Hashed, model.Password);
					//    var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
					//    if (!changePassResult.Success)
					//    {
					//        foreach (var changePassError in changePassResult.Errors)
					//        {
					//            _notificationService.ErrorNotification(changePassError);
					//        }
					//    }
					//}


					//活动日志
					_userActivityService.InsertActivity("EditUser", "编辑用户", curUser, curUser.Id);
					_notificationService.ErrorNotification("用户修改成功");
					return continueEditing ? RedirectToAction("Edit", curUser.Id) : RedirectToAction("List");
				}
				catch (Exception exc)
				{
					_notificationService.ErrorNotification(exc.Message, false);
				}
			}

			model.UsernamesEnabled = _userSettings.UsernamesEnabled;
			model.AllowUsersToChangeUsernames = _userSettings.AllowUsersToChangeUsernames;
			model.CreatedOn = user.CreatedOnUtc;
			model.LastActivityDate = user.LastActivityDateUtc;
			model.LastIpAddress = model.LastIpAddress;
			model.LastVisitedPage = user.GetAttribute<string>(DCMSDefaults.LastVisitedPageAttribute);

			model.GenderEnabled = true;
			model.DateOfBirthEnabled = true;
			model.StreetAddressEnabled = true;
			model.CityEnabled = true;
			model.CountryEnabled = true;
			model.StateProvinceEnabled = true;
			model.PhoneEnabled = true;

			model.UseACLMobile = user.UseACLMobile;
			model.UseACLPc = user.UseACLPc;

			//用户角色 !store.HasValue ? curStore.Id : store.Value
			model.AvailableUserRoles = _userService
				.GetAllUserRolesByStore(true, curStore?.Id ?? 0)
				.Select(cr => cr.ToModel<UserRoleModel>())
				.ToList();

			

			model.SelectedUserRoleIds = user.UserRoles.Select(cr => cr.Id).ToArray();
			model.AllowManagingUserRoles = _permissionService.Authorize(StandardPermissionProvider.ManageUserRoles);

			var lists = new List<SelectListItem>
			{
				new SelectListItem() { Text = curStore.Name, Value = curStore.Id.ToString(), Selected = (curStore.Id == curStore.Id) }
			};
			model.AccountType = curStore.Id;
			model.AccountTypes = new SelectList(lists, "Value", "Text");

			//组织机构
			model = BindDropDownList<UserModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);

			//业务员提成方案
			model.SalesmanExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindBusinessPercentagePlanList, curStore);

			//送货员提成方案
			model.DeliverExtractPlans = BindPercentagePlanSelection(_percentagePlanService.BindDeliverPercentagePlanList, curStore);

			//片区
			var curAreas = _districtService.GetAllDistrictByStoreId(curStore?.Id ?? 0);
			model.AvailableUserDistricts = curAreas.Select(a =>
			{
				return new UserDistrictsModel()
				{
					Id = a.Id,
					DistrictsName = a.Name,
					DistrictsId = a.Id,
					UserId = curUser.Id,
					UserName = user.Username
				};
			}).ToList();

			return View(model);
		}

		/// <summary>
		/// 删除用户
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[AuthCode((int)AccessGranularityEnum.UserListUpdate)]
		public JsonResult Delete(string ids)
		{

			try
			{
				if (!string.IsNullOrEmpty(ids))
				{
					int[] uids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
					var users = _userService.GetUsersByIds(curStore.Id, uids);
					foreach (var user in users)
					{
						if (user != null)
						{
							_userService.DeleteUser(user);
						}
					}
					//活动日志
					_userActivityService.InsertActivity("DeleteUser", "删除用户", curUser, ids);
					_notificationService.ErrorNotification("删除用户成功");
				}
				return Successful("用户删除成功");
			}
			catch (Exception ex)
			{
				return Error(ex.Message);
			}
		}


		/// <summary>
		/// Ztree异步更新片区
		/// </summary>
		/// <param name="userId">需要编辑的员工Id</param>
		/// <param name="ids">片区Id,逗号分隔</param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult SetDistricts(int userId, string ids)
		{


			if (curUser == null || curUser.Id == 0)
			{
				return Successful("更新失败");
			}

			string[] idArray = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			double[] idsDoubles = Array.ConvertAll<string, double>(idArray, s => Convert.ToDouble(s));
			int[] idInts = Array.ConvertAll<double, int>(idsDoubles, s => Convert.ToInt32(s));

			try
			{
				var allAreas = _districtService.GetAllDistrictByStoreId(curStore?.Id ?? 0);
				var userAreas = _userService.GetAllUserDistrictsByUserId(curStore.Id, userId).Select(a => a.DistrictId).ToList();

				foreach (var area in allAreas)
				{
					if (idInts != null && idInts.Contains(area.Id))
					{
						if (!userAreas.Contains(area.Id))
						{
							_userService.InsertUserDistricts(
								new UserDistricts() 
								{ 
									StoreId=curStore.Id,
									UserId = userId, 
									DistrictId = area.Id 
								});
						}
					}
					else
					{
						if (userAreas.Contains(area.Id))
						{
							_userService.DeleteUserDistricts(userId, area.Id);
						}
					}
				}
				return Successful("更新成功");
			}
			catch
			{
				return Successful("更新失败");
			}
		}



		[HttpGet]
		public JsonResult ResetPassword(int? id)
		{
			var user = _userService.GetUserById(curStore.Id, id ?? 0).ToModel<UserModel>();

			return Json(new
			{
				Success = true,
				RenderHtml = RenderPartialViewToString("_ResetPassword", user)
			});
		}

		/// <summary>
		/// 修改密码
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult ResetPassword(UserModel model)
		{

			var user = _userService.GetUserById(curStore.Id, model.Id);
			if (user == null)
			{
				return Json(new { Success = false, Message = "用户不存在" });
			}

			if (string.IsNullOrEmpty(model.Password))
			{
				return Json(new { Success = false, Message = "密码不能为空" });
			}

			var ps = CommonHelper.PasswordStrength(model.Password);
			if (!ps.RSL)
			{
				return Json(new { Success = false, Message = ps.MSG });
			}

			var changePassRequest = new ChangePasswordRequest(user.Email,false, PasswordFormat.Hashed, model.Password);
			var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
			if (changePassResult.Success)
			{
				_notificationService.SuccessNotification("密码已更改");
			}
			else
			{
				foreach (var error in changePassResult.Errors)
				{
					_notificationService.ErrorNotification(error);
				}
			}

			return Json(new { Success = true, Message = "重置成功" });
		}




		public IActionResult UserSetting()
		{
			var model = new UserModel();
			var user = _userService.GetUserById(curStore.Id, curUser.Id);
			model.Id = curUser.Id;
			model.Email = user.Email;
			model.UserRealName = user.UserRealName;
			model.Username = user.Username;
			model.FaceImage = user.FaceImage == "" || user.FaceImage == null ? "" : LayoutExtensions.ResourceServerUrl(":9100/HRXHJS/document/image/" + user.FaceImage);
			model.MobileNumber = user.MobileNumber;

			return View(model);
		}

		[HttpPost]
		public IActionResult UserSetting(UserModel model)
		{
			var user = _userService.GetUserById(curStore.Id, model.Id);
			if (!string.IsNullOrWhiteSpace(model.Email))
			{
				var cust2 = _userService.GetUserByEmail(curStore.Id, model.Email, true);
				if (cust2 != null && cust2.Id != user.Id)
				{
					ModelState.AddModelError("", "邮箱已经注册");
				}
			}

			if (!string.IsNullOrWhiteSpace(model.MobileNumber))
			{
				var cust2 = _userService.GetUserByMobileNamber(curStore.Id, model.MobileNumber, true);
				if (cust2 != null && cust2.Id != user.Id)
				{
					ModelState.AddModelError("", "电话已经注册");
				}
			}

			if (ModelState.IsValid)
			{
				try
				{
					IFormFile file = Request.Form.Files["image"];
					if (file.FileName != "")
					{
						using var client = new HttpClient();
						using var content = new MultipartFormDataContent();
						byte[] Bytes = new byte[file.OpenReadStream().Length];
						file.OpenReadStream().Read(Bytes, 0, Bytes.Length);
						// 设置当前流的位置为流的开始 
						file.OpenReadStream().Seek(0, SeekOrigin.Begin);

						var fileContent = new ByteArrayContent(Bytes);
						//设置请求头中的附件为文件名称，以便在WebAPi中进行获取
						fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("p_w_upload") { FileName = file.FileName };
						content.Add(fileContent, "\"file\"", $"\"{file.FileName}\"");

						var uploadServiceBaseAddress = LayoutExtensions.ResourceServerUrl(":9100/document/reomte/fileupload/HRXHJS");
						var result = client.PostAsync(uploadServiceBaseAddress, content).Result;
						if (result.StatusCode == HttpStatusCode.OK)
						{
							//获取到上传文件地址，并渲染到视图中进行访问   
							var m = result.Content.ReadAsStringAsync().Result.ToString();
							var list = JsonConvert.DeserializeObject(m);
							model.FaceImage = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)list).Last).Value.ToString();
						}
					}
				}
				catch (Exception)
				{

				}

				model.Id = curUser.Id;
				user.Email = model.Email;
				user.MobileNumber = model.MobileNumber;
				user.FaceImage = model.FaceImage;

				_userService.UpdateUser(user);
			}

			return View(model);
		}

		/*
		#endregion     

		#region 账户中心

		[HttpsRequirement(SslRequirement.Yes)]
		public virtual IActionResult Info()
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			var model = new UserInfoModel();
			model = _userModelFactory.PrepareUserInfoModel(model, _workContext.CurrentUser, false);

			return View(model);
		}

		[HttpPost]
		[PublicAntiForgery]
		public virtual IActionResult Info(UserInfoModel model, IFormCollection form)
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			var oldUserModel = new UserInfoModel();

			var user = _workContext.CurrentUser;

			//get user info model before changes for gdpr log
			if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
				oldUserModel = _userModelFactory.PrepareUserInfoModel(oldUserModel, user, false);

			//custom user attributes
			var userAttributesXml = ParseCustomUserAttributes(form);
			var userAttributeWarnings = _userAttributeParser.GetAttributeWarnings(userAttributesXml);
			foreach (var error in userAttributeWarnings)
			{
				ModelState.AddModelError("", error);
			}

			try
			{
				if (ModelState.IsValid)
				{
					//username 
					if (_userSettings.UsernamesEnabled && _userSettings.AllowUsersToChangeUsernames)
					{
						if (
							!user.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
						{
							//change username
							_userRegistrationService.SetUsername(user, model.Username.Trim());

							//re-authenticate
							//do not authenticate users in impersonation mode
							if (_workContext.OriginalUserIfImpersonated == null)
								_authenticationService.SignIn(user, true);
						}
					}
					//email
					if (!user.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
					{
						//change email
						var requireValidation = _userSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
						_userRegistrationService.SetEmail(user, model.Email.Trim(), requireValidation);

						//do not authenticate users in impersonation mode
						if (_workContext.OriginalUserIfImpersonated == null)
						{
							//re-authenticate (if usernames are disabled)
							if (!_userSettings.UsernamesEnabled && !requireValidation)
								_authenticationService.SignIn(user, true);
						}
					}

					//properties
					if (_dateTimeSettings.AllowUsersToSetTimeZone)
					{
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.TimeZoneIdAttribute,
							model.TimeZoneId);
					}
					//VAT number
					if (_taxSettings.EuVatEnabled)
					{
						var prevVatNumber = _genericAttributeService.GetAttribute<string>(user, NopUserDefaults.VatNumberAttribute);

						_genericAttributeService.SaveAttribute(user, NopUserDefaults.VatNumberAttribute,
							model.VatNumber);
						if (prevVatNumber != model.VatNumber)
						{
							var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out string _, out string vatAddress);
							_genericAttributeService.SaveAttribute(user, NopUserDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
							//send VAT number admin notification
							if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
								_workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(user,
									model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
						}
					}

					//form fields
					if (_userSettings.GenderEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.GenderAttribute, model.Gender);
					_genericAttributeService.SaveAttribute(user, NopUserDefaults.FirstNameAttribute, model.FirstName);
					_genericAttributeService.SaveAttribute(user, NopUserDefaults.LastNameAttribute, model.LastName);
					if (_userSettings.DateOfBirthEnabled)
					{
						var dateOfBirth = model.ParseDateOfBirth();
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.DateOfBirthAttribute, dateOfBirth);
					}
					if (_userSettings.CompanyEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.CompanyAttribute, model.Company);
					if (_userSettings.StreetAddressEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.StreetAddressAttribute, model.StreetAddress);
					if (_userSettings.StreetAddress2Enabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.StreetAddress2Attribute, model.StreetAddress2);
					if (_userSettings.ZipPostalCodeEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
					if (_userSettings.CityEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.CityAttribute, model.City);
					if (_userSettings.CountyEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.CountyAttribute, model.County);
					if (_userSettings.CountryEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.CountryIdAttribute, model.CountryId);
					if (_userSettings.CountryEnabled && _userSettings.StateProvinceEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.StateProvinceIdAttribute, model.StateProvinceId);
					if (_userSettings.PhoneEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.PhoneAttribute, model.MobileNumber);
					if (_userSettings.FaxEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.FaxAttribute, model.Fax);

					//newsletter
					if (_userSettings.NewsletterEnabled)
					{
						//save newsletter value
						var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(user.Email, _storeContext.CurrentStore.Id);
						if (newsletter != null)
						{
							if (model.Newsletter)
							{
								var wasActive = newsletter.Active;
								newsletter.Active = true;
								_newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
							}
							else
							{
								_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
							}
						}
						else
						{
							if (model.Newsletter)
							{
								_newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
								{
									NewsLetterSubscriptionGuid = Guid.NewGuid(),
									Email = user.Email,
									Active = true,
									StoreId = _storeContext.CurrentStore.Id,
									CreatedOnUtc = DateTime.UtcNow
								});
							}
						}
					}

					if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
						_genericAttributeService.SaveAttribute(user, NopUserDefaults.SignatureAttribute, model.Signature);

					//save user attributes
					_genericAttributeService.SaveAttribute(_workContext.CurrentUser,
						NopUserDefaults.CustomUserAttributes, userAttributesXml);

					//GDPR
					if (_gdprSettings.GdprEnabled)
						LogGdpr(user, oldUserModel, model, form);

					return RedirectToRoute("UserInfo");
				}
			}
			catch (Exception exc)
			{
				ModelState.AddModelError("", exc.Message);
			}

			//If we got this far, something failed, redisplay form
			model = _userModelFactory.PrepareUserInfoModel(model, user, true, userAttributesXml);
			return View(model);
		}

		[HttpPost]
		[PublicAntiForgery]
		public virtual IActionResult RemoveExternalAssociation(int id)
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			//ensure it's our record
			var ear = _workContext.CurrentUser.ExternalAuthenticationRecords.FirstOrDefault(x => x.Id == id);

			if (ear == null)
			{
				return Json(new
				{
					redirect = Url.Action("Info"),
				});
			}

			_externalAuthenticationService.DeleteExternalAuthenticationRecord(ear);

			return Json(new
			{
				redirect = Url.Action("Info"),
			});
		}

		[HttpsRequirement(SslRequirement.Yes)]
		[CheckAccessPublicStore(true)]
		public virtual IActionResult EmailRevalidation(string token, string email)
		{
			var user = _userService.GetUserByEmail(email);
			if (user == null)
				return RedirectToRoute("Homepage");

			var cToken = _genericAttributeService.GetAttribute<string>(user, NopUserDefaults.EmailRevalidationTokenAttribute);
			if (string.IsNullOrEmpty(cToken))
				return View(new EmailRevalidationModel
				{
					Result = _localizationService.GetResource("Account.EmailRevalidation.AlreadyChanged")
				});

			if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
				return RedirectToRoute("Homepage");

			if (string.IsNullOrEmpty(user.EmailToRevalidate))
				return RedirectToRoute("Homepage");

			if (_userSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
				return RedirectToRoute("Homepage");

			//change email
			try
			{
				_userRegistrationService.SetEmail(user, user.EmailToRevalidate, false);
			}
			catch (Exception exc)
			{
				return View(new EmailRevalidationModel
				{
					Result = _localizationService.GetResource(exc.Message)
				});
			}
			user.EmailToRevalidate = null;
			_userService.UpdateUser(user);
			_genericAttributeService.SaveAttribute(user, NopUserDefaults.EmailRevalidationTokenAttribute, "");

			//re-authenticate (if usernames are disabled)
			if (!_userSettings.UsernamesEnabled)
			{
				_authenticationService.SignIn(user, true);
			}

			var model = new EmailRevalidationModel()
			{
				Result = _localizationService.GetResource("Account.EmailRevalidation.Changed")
			};
			return View(model);
		}

		#endregion

		#region 安全

		[HttpsRequirement(SslRequirement.Yes)]
		public virtual IActionResult ChangePassword()
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			var model = _userModelFactory.PrepareChangePasswordModel();

			//display the cause of the change password 
			if (_userService.PasswordIsExpired(_workContext.CurrentUser))
				ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

			return View(model);
		}

		[HttpPost]
		[PublicAntiForgery]
		public virtual IActionResult ChangePassword(ChangePasswordModel model)
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			var user = _workContext.CurrentUser;

			if (ModelState.IsValid)
			{
				var changePasswordRequest = new ChangePasswordRequest(user.Email,
					true, _userSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
				var changePasswordResult = _userRegistrationService.ChangePassword(changePasswordRequest);
				if (changePasswordResult.Success)
				{
					model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
					return View(model);
				}

				//errors
				foreach (var error in changePasswordResult.Errors)
					ModelState.AddModelError("", error);
			}

			//If we got this far, something failed, redisplay form
			return View(model);
		}

		#endregion

		#region 头像

		[HttpsRequirement(SslRequirement.Yes)]
		public virtual IActionResult Avatar()
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			if (!_userSettings.AllowUsersToUploadAvatars)
				return RedirectToRoute("UserInfo");

			var model = new UserAvatarModel();
			model = _userModelFactory.PrepareUserAvatarModel(model);
			return View(model);
		}

		[HttpPost, ActionName("Avatar")]
		[PublicAntiForgery]
		[FormValueRequired("upload-avatar")]
		public virtual IActionResult UploadAvatar(UserAvatarModel model, IFormFile uploadedFile)
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			if (!_userSettings.AllowUsersToUploadAvatars)
				return RedirectToRoute("UserInfo");

			var user = _workContext.CurrentUser;

			if (ModelState.IsValid)
			{
				try
				{
					var userAvatar = _pictureService.GetPictureById(_genericAttributeService.GetAttribute<int>(user, NopUserDefaults.AvatarPictureIdAttribute));
					if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
					{
						var avatarMaxSize = _userSettings.AvatarMaximumSizeBytes;
						if (uploadedFile.Length > avatarMaxSize)
							throw new DCMSException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

						var userPictureBinary = _downloadService.GetDownloadBits(uploadedFile);
						if (userAvatar != null)
							userAvatar = _pictureService.UpdatePicture(userAvatar.Id, userPictureBinary, uploadedFile.ContentType, null);
						else
							userAvatar = _pictureService.InsertPicture(userPictureBinary, uploadedFile.ContentType, null);
					}

					var userAvatarId = 0;
					if (userAvatar != null)
						userAvatarId = userAvatar.Id;

					_genericAttributeService.SaveAttribute(user, NopUserDefaults.AvatarPictureIdAttribute, userAvatarId);

					model.AvatarUrl = _pictureService.GetPictureUrl(
						_genericAttributeService.GetAttribute<int>(user, NopUserDefaults.AvatarPictureIdAttribute),
						_mediaSettings.AvatarPictureSize,
						false);
					return View(model);
				}
				catch (Exception exc)
				{
					ModelState.AddModelError("", exc.Message);
				}
			}

			//If we got this far, something failed, redisplay form
			model = _userModelFactory.PrepareUserAvatarModel(model);
			return View(model);
		}

		[HttpPost, ActionName("Avatar")]
		[PublicAntiForgery]
		[FormValueRequired("remove-avatar")]
		public virtual IActionResult RemoveAvatar(UserAvatarModel model)
		{
			if (!_workContext.CurrentUser.IsRegistered())
				return Challenge();

			if (!_userSettings.AllowUsersToUploadAvatars)
				return RedirectToRoute("UserInfo");

			var user = _workContext.CurrentUser;

			var userAvatar = _pictureService.GetPictureById(_genericAttributeService.GetAttribute<int>(user, NopUserDefaults.AvatarPictureIdAttribute));
			if (userAvatar != null)
				_pictureService.DeletePicture(userAvatar);
			_genericAttributeService.SaveAttribute(user, NopUserDefaults.AvatarPictureIdAttribute, 0);

			return RedirectToRoute("UserAvatar");
		}

		#endregion
		*/

	}
}