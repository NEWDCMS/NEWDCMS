using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;



namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于经销商员工管理
    /// </summary>
    public class UserRoleController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;


        public UserRoleController(IWorkContext workContext,
            IStoreContext storeContext,
            IUserService userService,
            IUserActivityService userActivityService,
            ILogger loggerService,
            INotificationService notificationService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _userActivityService = userActivityService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        /// <summary>
        /// 用户角色管理列表
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.UserRoleView)]
        public IActionResult List(int[] searchUserRoleIds, int? store, string name = "", int pagenumber = 0)
        {

            var listModel = new UserRoleListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var lists = new List<SelectListItem>();
            var stors = Stores;
            foreach (var stor in stors)
            {
                lists.Add(new SelectListItem() { Text = stor.Name, Value = stor.Id.ToString(), Selected = (curStore.Id == stor.Id) });
            }
            listModel.Stores = new SelectList(lists, "Value", "Text");

            var userroles = _userService.GetAllUserRoles(curStore?.Id ?? 0, true, name, pagenumber, 30);
            listModel.PagingFilteringContext.LoadPagedList(userroles);

            if (name != "")
            {
                listModel.UserRoleItems = userroles.Where(n => n.Name.Contains(name)).OrderByDescending(m => m.IsSystemRole).Select(s => s.ToModel<UserRoleModel>()).ToList();
            }
            else
            {
                listModel.UserRoleItems = userroles.OrderByDescending(m => m.IsSystemRole).Select(s => s.ToModel<UserRoleModel>()).ToList();
            }

            listModel.UserRoleItems = userroles.OrderByDescending(m => m.IsSystemRole).Select(s => s.ToModel<UserRoleModel>()).ToList();


            return View(listModel);
        }



        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        [AuthCode(425)]
        public IActionResult Create()
        {
            var model = new UserRoleModel
            {
                Active = true
            };
            var lists = new List<SelectListItem>();
            return View(model);
        }
        [HttpPost]
        //[FormValueRequired("save", "save-continue")]
        [AuthCode(425)]
        public IActionResult Create(UserRoleModel model, bool continueEditing = false)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("", "角色名不能为空");
            }

            if (ModelState.IsValid)
            {
                //

                var userRole = model.ToEntity<UserRole>();
                userRole.StoreId = curStore.Id;
                userRole.UseACLMobile = model.UseACLMobile;
                userRole.UseACLPc = model.UseACLPc;

                if (_userService.GetUserRoleBySystemName(curStore.Id, model.Name) != null)
                {
                    //活动日志
                    _userActivityService.InsertActivity("InsertUserRole", "角色名称已存在", curUser.Id);
                    _notificationService.SuccessNotification("角色名称已存在");
                }
                else
                {
                    _userService.InsertUserRole(userRole);

                    //活动日志
                    _userActivityService.InsertActivity("InsertUserRole", "创建用户角色", curUser.Id);
                    _notificationService.SuccessNotification("创建用户角色成功");
                }
                

                return RedirectToAction("List");
            }

            return View(model);
        }


        /// <summary>
        /// 编辑用户角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode(425)]
        public IActionResult Edit(int? id)
        {
            var model = new UserRoleModel();
            if (id.HasValue)
            {
                var userRole = _userService.GetUserRoleById(id.Value);
                if (userRole != null)
                {
                    model = userRole.ToModel<UserRoleModel>();
                }
            }
            return View(model);
        }
        [HttpPost]
        [AuthCode(425)]
        public IActionResult Edit(UserRoleModel model, bool continueEditing = false)
        {

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("", "角色名不能为空");
            }


            var userRole = _userService.GetUserRoleById(model.Id);
            if (userRole?.IsSystemRole ?? false)
            {
                ModelState.AddModelError("", "系统角色不能被编辑");
            }

            if (ModelState.IsValid)
            {
                if (userRole != null)
                {
                    userRole.UseACLMobile = model.UseACLMobile;
                    userRole.UseACLPc = model.UseACLPc;
                    userRole.Name = model.Name;
                    userRole.SystemName = model.SystemName;
                    userRole.IsSystemRole = model.IsSystemRole;
                    userRole.Active = model.Active;
                    userRole.Description = model.Description;
                    _userService.UpdateUserRole(userRole);
                }

                //活动日志
                _userActivityService.InsertActivity("UpdateUserRole", "更新用户角色", curUser.Id);
                _notificationService.SuccessNotification("更新用户角色成功");

                return RedirectToAction("List");
            }

            return View(model);
        }

        /// <summary>
        /// 异步授权
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.UserRoleAdd)]
        public JsonResult AddRole(int? id)
        {

            var model = new CheckBoxModel
            {
                KeyId = id ?? 0
            };

            var user = _userService.GetUserById(curStore.Id, id ?? 0);
            if (user != null)
            {
                var availableUserRoles = _userService
                .GetAllUserRolesByStore(true, user.StoreId)
                .Select(cr => cr.ToModel<UserRoleModel>())
                .ToList();

                //var selectedUserRoleIds = user.UserRoles.Select(cr => cr.Id).ToArray();
                var selectedUserRoleIds = _userService.GetUserRolesByUser(user.StoreId, user.Id).Select(cr => cr.Id).ToArray(); //_userService.GetUserRoleIds(user);
                availableUserRoles.ForEach(r =>
                {

                    model.Data.Add(new CheckBox()
                    {
                        Name = r.SystemName,
                        Value = r.Id,
                        Discription = r.Name,
                        IsChecked = selectedUserRoleIds.Contains(r.Id)
                    });
                });
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("_SetCheckBox", model)
            });

        }
        /// <summary>
        /// 异步授权保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.UserRoleAdd)]
        public JsonResult AddRole(CheckBoxModel model, IFormCollection form)
        {
            if (!curUser.IsAdmin())
            {
                return Warning("您不是管理员，不能操作角色!");
            }

            if (!string.IsNullOrEmpty(form["SelectedIds"].ToString()))
            {
                try
                {
                    var user = _userService.GetUserById(curStore.Id, model.KeyId);
                    if (user == null)
                    {
                        return Warning("参数错误");
                    }

                    var selectedIds = form["SelectedIds"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();

                    //var admins = _userService.GetAllAdminUsersByUser(user);
                    //如果是平台创建的管理员账号，则不能删除管理员权限
                    //if (user.IsPlatformCreate == true)
                    //{
                    //    var userRoles = _userService.GetAllUserRoles(user.StoreId, true);
                    //    if (userRoles != null && userRoles.Count > 0)
                    //    {
                    //        //当前经销商管理员角色Ids
                    //        var adminIds = userRoles.Where(ur => ur.SystemName == "Administrators").Select(ur => ur.Id).ToList();

                    //        if (adminIds != null && adminIds.Count > 0 && selectedIds.ToList().Intersect(adminIds).ToList().Count == 0)
                    //        {
                    //           return this.Warning( "平台创建的管理员账号不能删除管理员权限");
                    //        }
                    //    }
                    //}

                    var allUserRoles = _userService.GetAllUserRoles(curStore.Id, true);
                    var oldRoles = _userService.GetUserRolesByUser(user.StoreId, user.Id);
                    var newUserRoles = new List<UserRole>();
                    foreach (var userRole in allUserRoles)
                    {
                        if (selectedIds != null && selectedIds.Contains(userRole.Id))
                        {
                            newUserRoles.Add(userRole);
                        }
                    }

                    foreach (var userRole in allUserRoles)
                    {
                        if (selectedIds != null && selectedIds.Contains(userRole.Id))
                        {
                            //new role
                            if (oldRoles.Count(cr => cr.Id == userRole.Id) == 0)
                            {
                                _userService.InsertUserRoleMapping(user.Id, userRole.Id, curStore.Id);
                            }
                        }
                        else
                        {
                            //removed role
                            if (userRole.SystemName != "Administrators" && oldRoles.Count(cr => cr.Id == userRole.Id) > 0)
                            {
                                _userService.RemoveUserRoleMapping(user.Id, userRole.Id, curStore.Id);
                            }
                        }
                    }
                    _userService.UpdateUser(user);

                    return Successful("角色分配成功");
                }
                catch (Exception ex)
                {
                    return Warning(ex.ToString());
                }

            }
            else
            {
                return Warning("参数错误");
            }
        }

    }
}