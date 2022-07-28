using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Security;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;


namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于经销商员工管理
    /// </summary>
    public class PermissionController : BasePublicController
    {
        private readonly IUserService _userService;

        private readonly IPermissionService _permissionService;
        private readonly IModuleService _moduleService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PermissionController(IWorkContext workContext,
            IStoreContext storeContext,
            IUserService userService,
            IStaticCacheManager cacheManager,
            IModuleService moduleService,
            ILogger loggerService,
            INotificationService notificationService,
            IPermissionService permissionService, IWebHostEnvironment hostingEnvironment
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _permissionService = permissionService;
            _moduleService = moduleService;
            _hostingEnvironment = hostingEnvironment;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 权限设置列表
        /// </summary>
        /// <returns></returns>
        [Route("Permission/List/{roleId}", Name = "Permissions")]
        [AuthCode((int)AccessGranularityEnum.UserRoleView)]
        public IActionResult List(int[] SearchUserRoleIds, int? store, int? roleId, int flag = 0, int pagenumber = 0)
        {
            if (!roleId.HasValue)
            {
                return RedirectToAction("List", "UserRole");
            }

            var model = new PermissionModel();
            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var userroles = _userService.GetAllUserRoles(curStore?.Id ?? 0, true, "", pagenumber, 100);

            model.PagingFilteringContext.LoadPagedList(userroles);
            model.UserRoleItems = userroles.Select(s => s.ToModel<UserRoleModel>()).ToList();

            List<int> pcpermissionIds = new List<int>();
            List<int> apppermissionIds = new List<int>();
            List<int> roleModules = new List<int>();
            if (flag == 1)
            {
                try
                {
                    string ContentRootPath = _hostingEnvironment.ContentRootPath;
                    string path = ContentRootPath + @"\App_Data\TempUploads\PermissionRecordRoleMapping.json";
                    FileInfo myFile = new FileInfo(path);
                    StreamReader sW5 = myFile.OpenText();
                    var PermissionRecordRoleresult = sW5.ReadToEnd();
                    sW5.Close();

                    //PC端拥有的权限记录
                    pcpermissionIds = JsonConvert.DeserializeObject<List<RoleJurisdiction>>(PermissionRecordRoleresult).Where(s => s.SystemName == userroles.FirstOrDefault(a => a.Id == roleId).SystemName && s.Platform == 0).Select(m => m.jurisdiction_Id).ToList();

                    //app端拥有的权限记录
                    apppermissionIds = JsonConvert.DeserializeObject<List<RoleJurisdiction>>(PermissionRecordRoleresult).Where(s => s.SystemName == userroles.FirstOrDefault(a => a.Id == roleId).SystemName && s.Platform == 1).Select(m => m.jurisdiction_Id).ToList();

                    string path1 = ContentRootPath + @"\App_Data\TempUploads\RoleMouleMappingConfig.json";
                    FileInfo myFileRoleMouleMapping = new FileInfo(path1);
                    StreamReader sW5RoleMouleMapping = myFileRoleMouleMapping.OpenText();
                    var RoleMouleMappingresult = sW5RoleMouleMapping.ReadToEnd();
                    sW5RoleMouleMapping.Close();

                    //获取该角色拥有的模块
                    roleModules = JsonConvert.DeserializeObject<List<RoleJurisdiction>>(RoleMouleMappingresult).Where(s => s.SystemName == userroles.FirstOrDefault(a => a.Id == roleId).SystemName).Select(m => m.jurisdiction_Id).ToList();
                }
                catch (Exception)
                {

                }
            }


            //获取所有权限记录
            var allPRS = _permissionService.GetAllPermissionRecordsByStore(0);
            if (flag == 0)
            {
                //获取当前角色PC端拥有的权限记录
                pcpermissionIds = _userService
                 .GetUserRolePermissionRecords(curStore?.Id ?? 0, roleId.Value, 0)
                 .Select(c => c.Id).ToList();

                //获取当前角色APP端拥有的权限记录
                apppermissionIds = _userService
                .GetUserRolePermissionRecords(curStore?.Id ?? 0, roleId.Value, 1)
                .Select(c => c.Id).ToList();

                //获取该角色拥有的模块
                roleModules = _userService.GetUserRoleModuleRecords(curStore.Id, roleId.Value, false).Select(c => c.Id).ToList();
            }

            try
            {
                //获取所有模块
                //(s) => s.ShowMobile == false
                var allModules = _moduleService.GetModulesByStore(curStore?.Id ?? 0, false);

				model.PCPermissions.MenuTrees = GetModuleTreeList(allPRS, 0, 0, null, pcpermissionIds, allModules, roleModules, 0);


                //APP只显示ShowMobile
                model.APPPermissions.MenuTrees = GetModuleTreeList(allPRS.Where(s => s.ShowMobile == true).ToList(), 0, 0, null, apppermissionIds, allModules, roleModules, 1);


                //获取数据和频道权限
                var permission = _permissionService.GetAllDataChannelPermissionsByRoleId(curStore?.Id ?? 0, roleId).FirstOrDefault();
                if (permission != null)
                {
                    model.PCPermissions.DataChannelPermission = permission.ToModel<DataChannelPermissionModel>();
                }
            }
            catch (Exception)
            {
            }



            model.PCPermissions
                .DataChannelPermission
                .AppOpenChoiceGifts = new SelectList(from a in Enum.GetValues(typeof(OpenChoiceGift)).Cast<OpenChoiceGift>()
                                                     select new SelectListItem
                                                     {
                                                         Text = CommonHelper.GetEnumDescription(a),
                                                         Value = ((int)a).ToString()
                                                     }, "Value", "Text");

            model.PCPermissions
                .DataChannelPermission
                .AllowViewReports = new SelectList(from a in Enum.GetValues(typeof(AllowViewReport)).Cast<AllowViewReport>()
                                                   select new SelectListItem
                                                   {
                                                       Text = CommonHelper.GetEnumDescription(a),
                                                       Value = ((int)a).ToString()
                                                   }, "Value", "Text");
            return View(model);
        }


        /// <summary>
        /// 保存权限设置
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Permission/List/{roleId}", Name = "PostPermissions")]
        [AuthCode(425)]
        public IActionResult PermissionsSave(IFormCollection form, PermissionModel model, int? roleId)
        {
            if (!roleId.HasValue)
            {
                return RedirectToRoute("Permissions");
            }

            try
            {
                //获取系统平台所有模块
                var allModules = _moduleService.GetModulesByStore(0);
                //获取系统平台全部权限记录
                var allPRS = _permissionService.GetAllPermissionRecordsByStore(0);

                var moduleTrees = GetModuleTreeList(allPRS, 0, 0, null, null, allModules, null);

                var pcoptSelectIDS = new OptSelectIDS();
                var appoptSelectIDS = new OptSelectIDS();

                foreach (var tree in moduleTrees)
                {
                    pcoptSelectIDS.mIds.AddRange(GetPCModulePermissionRecordsList(form, tree).mIds);
                    pcoptSelectIDS.pIds.AddRange(GetPCModulePermissionRecordsList(form, tree).pIds);
                }

                pcoptSelectIDS.pIds.Add(424); //添加公共区域权限，防止丢失
                if (curUser.IsAdmin())
                {
                    pcoptSelectIDS.pIds.Add(425); //管理员添加访问控制权限，防止丢失
                }

                foreach (var tree in moduleTrees)
                {
                    appoptSelectIDS.mIds.AddRange(GetAPPModulePermissionRecordsList(form, tree).mIds);
                    appoptSelectIDS.pIds.AddRange(GetAPPModulePermissionRecordsList(form, tree).pIds);
                }

                //保存数据
                _permissionService.PermissionsSave(curStore.Id,
                    curUser.Id, roleId ?? 0,
                    pcoptSelectIDS.mIds,
                    pcoptSelectIDS.pIds,
                    appoptSelectIDS.mIds,
                    appoptSelectIDS.pIds,
                    model.PCPermissions.DataChannelPermission.ToEntity<DataChannelPermission>());


                _userService.ClearUserCache(curStore.Id);

                ////获取角色下的所有用户
                //var lst = _userService.GetUserByStoreIdUserRoleIds(curStore.Id, roleId.ToString());
                ////获取在线用户
                //var onlineUrl = $"{EngineContext.GetWebApiServer}/api/v3/dcms/rtc/online?storeId=0";//{curStore.Id}
                //var online_rst = HttpHelper.HttpGet(onlineUrl);
                //JObject obj = JObject.Parse(online_rst);
                //var online_lst = obj["data"].ToObject<List<ClientInfo>>();
                ////lst = lst.Where(w=> online_lst.Select(s => s.UserId).Contains(w.Id)).ToList();
                //var user = new User();
                //user.Id = 5813;
                //lst.Add(user);
                //if (lst != null && lst.Count > 0)
                //{
                //    LoopTask(lst);
                //}
            }
            catch (Exception)
            {
            }
            return RedirectToRoute("Permissions", new { roleId });
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> ClearCache()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _userService.ClearUserCache(curStore.Id);

                    return Json(new { Success = true });
                }
                catch (Exception ex)
                {
                    return Error(ex.Message);
                }
            });
        }


        /// <summary>
        /// 遍历所有模块选择的PC端权限记录
        /// </summary>
        /// <param name="form"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        [NonAction]
        private OptSelectIDS GetPCModulePermissionRecordsList(IFormCollection form, ModuleTree<ModuleModel> tree)
        {
            var optSelectIDS = new OptSelectIDS();
            string mformKey = "pc_module_" + tree.Module.Id + "";
            var moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
            if (int.Parse(moduleId) != 0)
            {
                optSelectIDS.mIds.Add(int.Parse(moduleId));
            }

            foreach (var node in tree.Children)
            {
                foreach (var pr in node.PermissionRecords)
                {
                    string formKey = "pc_allow_" + pr.ModuleId + "_" + pr.Id + "";
                    var permissionId = string.IsNullOrEmpty(form[formKey]) ? "0" : form[formKey].ToString();
                    if (int.Parse(permissionId) != 0)
                    {
                        optSelectIDS.pIds.Add(int.Parse(permissionId));
                    }

                    mformKey = "pc_module_" + pr.ModuleId + "";
                    moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
                    if (int.Parse(moduleId) != 0)
                    {
                        optSelectIDS.mIds.Add(int.Parse(moduleId));
                    }

                    //System.Diagnostics.Debug.Print(formKey + "" + (!string.IsNullOrEmpty(permissionId) ? "=" + permissionId : ""));
                }

                var toptSelectIDS = GetPCModulePermissionRecordsList(form, node);
                optSelectIDS.pIds.AddRange(toptSelectIDS.pIds);
                optSelectIDS.mIds.AddRange(toptSelectIDS.mIds);
            }
            return optSelectIDS;
        }

        /// <summary>
        /// 遍历所有模块选择的APP端权限记录
        /// </summary>
        /// <param name="form"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        [NonAction]
        private OptSelectIDS GetAPPModulePermissionRecordsList(IFormCollection form, ModuleTree<ModuleModel> tree)
        {
            var optSelectIDS = new OptSelectIDS();
            string mformKey = "ad_module_" + tree.Module.Id + "";
            var moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
            if (int.Parse(moduleId) != 0)
            {
                optSelectIDS.mIds.Add(int.Parse(moduleId));
            }

            foreach (var node in tree.Children)
            {
                foreach (var pr in node.PermissionRecords)
                {
                    string formKey = "ad_allow_" + pr.ModuleId + "_" + pr.Id + "";
                    var permissionId = string.IsNullOrEmpty(form[formKey]) ? "0" : form[formKey].ToString();
                    if (int.Parse(permissionId) != 0)
                    {
                        optSelectIDS.pIds.Add(int.Parse(permissionId));
                    }

                    mformKey = "ad_module_" + pr.ModuleId + "";
                    moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
                    if (int.Parse(moduleId) != 0)
                    {
                        optSelectIDS.mIds.Add(int.Parse(moduleId));
                    }
                }

                var toptSelectIDS = GetAPPModulePermissionRecordsList(form, node);
                optSelectIDS.pIds.AddRange(toptSelectIDS.pIds);
                optSelectIDS.mIds.AddRange(toptSelectIDS.mIds);
            }
            return optSelectIDS;
        }


        /// <summary>
        /// 递归获取模块树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        private List<ModuleTree<ModuleModel>> GetModuleTreeList(IList<PermissionRecord> allPrs,
            int? store,
            int Id,
            int? position,
            List<int> permissionIds,
            List<Module> allModules,
            List<int> modules,
            int platform = 0)
        {

            var trees = new List<ModuleTree<ModuleModel>>();
            var perentList = _moduleService.GetNotPaltformModulesByParentId(allModules, store.Value, Id, position);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetModuleTreeList(allPrs, store.Value, b.Id, position, permissionIds, allModules, modules, platform);

                    var records = allPrs.Where(a => a.ModuleId == b.Id).ToList();

                    var model = b.ToModel<ModuleModel>();
                    model.Selected = modules != null && modules.Contains(b.Id);

                    var node = new ModuleTree<ModuleModel>
                    {
                        Module = model,
                        Children = new List<ModuleTree<ModuleModel>>(),
                        PermissionRecords = records.Select(s =>
                        {
                            return new PermissionRecordModel()
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code,
                                SystemName = s.SystemName,
                                StoreId = s.StoreId,
                                ModuleId = s.ModuleId,
                                ModuleName = b == null ? "" : b.Name,
                                Description = s.Description,
                                Enabled = s.Enabled,
                                CreatedOn = s.CreatedOn,
                                Selected = permissionIds != null && permissionIds.Count > 0 && permissionIds.Contains(s.Id)
                            };
                        }).OrderBy(p => p.Name).ToList()
                    };

                    //if (tempList.Count > 0)
                    //{
                    //    model.Selected = modules != null ? modules.Contains(b.Id) : false;
                    //}
                    //else
                    //{
                    //    model.Selected = modules != null && node.PermissionRecords.Count>0 && node.PermissionRecords.Count(c=>c.Selected==true)>0 ? modules.Contains(b.Id) : false;
                    //}

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }
                    trees.Add(node);
                }
            }

            return trees;
        }

        [NonAction]
        private List<ModuleTree<ModuleModel>> GetAPPModuleTreeList(IList<PermissionRecord> allPrs,
            int? store,
            int Id,
            int? position,
            List<Module> allModules,
             List<int> modules,
            int platform = 0)
        {

            var trees = new List<ModuleTree<ModuleModel>>();
            var perentList = _moduleService.GetNotPaltformModulesByParentId(allModules, store.Value, Id, position);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetAPPModuleTreeList(allPrs, store.Value, b.Id, position, allModules, modules, platform);

                    var records = allPrs.Where(a => a.ModuleId == b.Id).ToList();

                    var model = b.ToModel<ModuleModel>();
                    model.Selected = modules != null && modules.Contains(b.Id);
                    model.ShowMobile = b.ShowMobile ?? false;

                    var node = new ModuleTree<ModuleModel>
                    {
                        Module = model,
                        Children = new List<ModuleTree<ModuleModel>>(),
                        PermissionRecords = records.Select(s =>
                        {
                            return new PermissionRecordModel()
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code,
                                SystemName = s.SystemName,
                                StoreId = s.StoreId,
                                ModuleId = s.ModuleId,
                                ModuleName = b == null ? "" : b.Name,
                                Description = s.Description,
                                Enabled = s.Enabled,
                                CreatedOn = s.CreatedOn,
                                Selected = false
                            };
                        }).OrderBy(p => p.Name).ToList()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }
                    trees.Add(node);
                }
            }

            return trees;
        }


        /// <summary>
        /// app模块访问控制
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult AppModuleAcl(int? id)
        {
            var user = _userService.GetUserById(curStore.Id, id ?? 0);
            if (user == null)
            {
                return Json(new { Success = false, Message = "用户不存在" });
            }

            var acls = new List<int>();
            if (!string.IsNullOrEmpty(user.AppModuleAcl))
                acls = JsonConvert.DeserializeObject<List<int>>(user.AppModuleAcl);

            //获取所有权限记录
            var allPRS = _permissionService.GetAllPermissionRecordsByStore(0);

            //获取所有模块
            var allModules = _moduleService.GetModulesByStore(0);

            //APP只显示ShowMobile
            var ams = GetAPPModuleTreeList(allPRS.Where(s => s.ShowMobile == true).ToList(), 0, 0, null, allModules, acls, 1);

            var acl = new UserModuleACLModel()
            {
                UserId = id ?? 0,
                MenuTrees = ams
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("_AppModuleAcl", acl)
            });
        }
        [HttpPost]
        public JsonResult AppModuleAcl(IFormCollection form, UserModuleACLModel model)
        {
            try
            {
                //获取系统平台所有模块
                var allModules = _moduleService.GetModulesByStore(0);

                //获取系统平台全部权限记录
                var allPRS = _permissionService.GetAllPermissionRecordsByStore(0);

                var moduleTrees = GetModuleTreeList(allPRS, 0, 0, null, null, allModules, null);

                var appoptSelectIDS = new OptSelectIDS();

                foreach (var tree in moduleTrees)
                {
                    appoptSelectIDS.mIds.AddRange(GetAPPModules(form, tree).mIds);
                }

                var acls = new List<int>();
                foreach (var s in appoptSelectIDS.mIds)
                {
                    if (!acls.Contains(s))
                    {
                        acls.Add(s);
                    }
                }

                //保存数据
                var user = _userService.GetUserById(curStore.Id, model.UserId);
                user.AppModuleAcl = JsonConvert.SerializeObject(acls);
                _userService.UpdateUser(user);

                return Json(new { Success = true, Message = "授权成功" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = true, Message = ex.Message });
            }
        }
        [NonAction]
        private OptSelectIDS GetAPPModules(IFormCollection form, ModuleTree<ModuleModel> tree)
        {
            var optSelectIDS = new OptSelectIDS();
            string mformKey = "ad_module_" + tree.Module.Id + "";
            var moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
            if (int.Parse(moduleId) != 0)
            {
                var id = int.Parse(moduleId);
                if (!optSelectIDS.mIds.Contains(id))
                    optSelectIDS.mIds.Add(id);
            }

            foreach (var node in tree.Children)
            {
                foreach (var pr in node.PermissionRecords)
                {
                    mformKey = "ad_module_" + pr.ModuleId + "";
                    moduleId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();

                    if (int.Parse(moduleId) != 0)
                    {
                        var id = int.Parse(moduleId);
                        if (!optSelectIDS.mIds.Contains(id))
                            optSelectIDS.mIds.Add(id);
                    }
                }

                var toptSelectIDS = GetAPPModules(form, node);
                optSelectIDS.mIds.AddRange(toptSelectIDS.mIds);
            }
            return optSelectIDS;
        }





        /// <summary>
        /// 指派下级用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Subordinates(int? id)
        {
            var user = _userService.GetUserById(curStore.Id, id ?? 0);
            if (user == null)
            {
                return Json(new { Success = false, Message = "用户不存在" });
            }

            var acls = new List<int>();
            if (!string.IsNullOrEmpty(user.Subordinates))
                acls = JsonConvert.DeserializeObject<List<int>>(user.Subordinates);

            var users = _userService.GetAllUsers(curStore.Id).Where(s => s.Id != user.Id && !s.IsSystemAccount).ToList();

            //var leaderUser = _userService.GetAllLaderUser(curStore.Id, id ?? 0);
            var models = users.Select(s => s.ToModel<UserModel>()).ToList();
            models.ToList().ForEach(u => {
                if (u.Subordinates != null)
                    u.Subordinates = $",{u.Subordinates.Trim(new char[] { '[', ']' })}";
                else
                    u.Subordinates = string.Empty;
                u.Selected = acls.Contains(u.Id);
            });
            var leaderUsers = this.GetLeaderUsers(models, id??0).Distinct().ToList();
            foreach (var item in leaderUsers)
            {
                models.Remove(item);
            }
            //foreach (var u in models)
            //{
            //    u.Selected = acls.Contains(u.Id);
            //}

            var subs = new SubordinatesModel()
            {
                UserId = id ?? 0,
                Subordinates = models
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("_Subordinates", subs)
            });
        }

        private IList<UserModel> GetLeaderUsers(IList<UserModel> lst,int userId) 
        {
            var strUserId = $",{userId}";
            var lst1 = lst.Where(w=> w.Subordinates != null && w.Subordinates.Contains(strUserId));
            return lst1.Distinct().ToList().Concat(lst1.Distinct().ToList().SelectMany(t => GetLeaderUsers(lst, t.Id))).Distinct().ToList();
        }

        [HttpPost]
        public JsonResult Subordinates(IFormCollection form, SubordinatesModel model)
        {
            try
            {

                var ids = new List<int>();
                var users = _userService.GetAllUsers(curStore.Id);
                var user = users.Where(s => s.Id == model.UserId).FirstOrDefault();

                foreach (var u in users)
                {
                    string mformKey = "subordinates_" + u.Id + "";
                    var userId = string.IsNullOrEmpty(form[mformKey]) ? "0" : form[mformKey].ToString();
                    if (int.Parse(userId) != 0)
                    {
                        var id = int.Parse(userId);
                        if (!ids.Contains(id))
                            ids.Add(id);
                    }
                }

                if (user != null)
                {
                    var subordinates = string.Empty;
                    if (ids.Any()) 
                    {
                        subordinates = JsonConvert.SerializeObject(ids);
                    }
                    user.Subordinates = subordinates;
                    _userService.UpdateUser(user);
                }

                return Json(new { Success = true, Message = "指派成功" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = true, Message = ex.Message });
            }
        }

    }
}