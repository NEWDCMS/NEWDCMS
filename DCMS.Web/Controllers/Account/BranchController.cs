using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于经销商组织机构管理
    /// </summary>
    public partial class BranchController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IBranchService _branchService;
        

        public BranchController(
             IWorkContext workContext,
             IStoreContext storeContext,
             IBranchService branchService,
             IUserActivityService userActivityService,
             INotificationService notificationService,
             ILogger loggerService,
             IStaticCacheManager cacheManager) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _branchService = branchService;
            
        }


        /// <summary>
        /// 异步获取FancyTree
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetFancyTree()
        {

            return await Task.Run(() =>
            {
                var trees = GetBranchList(curStore?.Id ?? 0, 0);
                return Json(trees);
            });
        }


        /// <summary>
        /// 异步获取树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetBranchTree(int? store)
        {
            return await Task.Run(() =>
            {
                IList<ZTree> nodes = _branchService.GetBranchZTree(store.Value);
                return Json(nodes);
            });
        }


        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public JsonResult AsyncGetList(int? store, int page = 0, int rows = 1000)
        {
            if (!store.HasValue)
            {
                store = curStore?.Id ?? 0;
            }

            var gridModel = _branchService.GetAllBranchs(store: store.Value, pageIndex: page, pageSize: rows);

            return Json(new
            {
                total = gridModel.TotalCount,
                rows = gridModel.Select(m => { return m.ToModel<BranchModel>(); }).OrderBy(m => m.ParentId).ThenBy(m => m.DisplayOrder).ToList()
            });
        }


        [HttpGet]
        public JsonResult Create(int id = 0)
        {

            var model = new BranchModel();

            //经销商
            var storeList = new List<SelectListItem>();
            model.StoreId = curStore?.Id ?? 0;
            var stores = _storeContext.Stores;
            storeList.Add(new SelectListItem { Text = curStore.Name, Value = curStore.Id.ToString() });
            model.StoreList = new SelectList(storeList, "Value", "Text");
            model.Status = true;
            //分级
            model = BindDropDownList<BranchModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);


            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }

        [HttpPost]
        public JsonResult Create(IFormCollection form)
        {
            try
            {

                var Id = !string.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : "0";
                var StoreId = !string.IsNullOrEmpty(form["StoreId"]) ? form["StoreId"].ToString() : "0";
                var DeptCode = !string.IsNullOrEmpty(form["DeptCode"]) ? form["DeptCode"].ToString() : "0";
                var DeptName = !string.IsNullOrEmpty(form["DeptName"]) ? form["DeptName"].ToString() : "";
                var DeptDesc = !string.IsNullOrEmpty(form["DeptDesc"]) ? form["DeptDesc"].ToString() : "";
                var ParentId = !string.IsNullOrEmpty(form["ParentId"]) ? form["ParentId"].ToString() : "0";
                var TreePath = !string.IsNullOrEmpty(form["TreePath"]) ? form["TreePath"].ToString() : "";
                var BranchLeader = !string.IsNullOrEmpty(form["BranchLeader"]) ? form["BranchLeader"].ToString() : "";
                var DisplayOrder = !string.IsNullOrEmpty(form["DisplayOrder"]) ? form["DisplayOrder"].ToString() : "0";
                var Status = !string.IsNullOrEmpty(form["Status"]) ? form["Status"].Contains("true").ToString() : "false";

                var branch = new Branch()
                {
                    //Id = -1,//(大家注意)自关联表要主动赋值
                    StoreId = curStore?.Id ?? 0,
                    DeptCode = int.Parse(DeptCode),
                    DeptName = DeptName,
                    DeptShort = 0,
                    DeptDesc = DeptDesc,
                    ParentId = int.Parse(ParentId),
                    TreePath = TreePath,
                    BranchLeader = BranchLeader,
                    DisplayOrder = int.Parse(DisplayOrder),
                    Status = bool.Parse(Status),
                    CreateDateTime = DateTime.Now
                };

                _branchService.InsertBranch(branch);

                _userActivityService.InsertActivity("InsertBranch", "创建部门", curUser, curUser.Id);
                _notificationService.SuccessNotification("创建部门成功");

                return Successful("创建部门成功");

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }


        [HttpGet]
        public JsonResult Edit(int id = 0)
        {
            var model = new BranchModel();

            //分级
            var branch = _branchService.GetBranchById(id);
            if (branch != null)
            {
                model = branch.ToModel<BranchModel>();
            }

            //经销商
            var storeList = new List<SelectListItem>();
            model.StoreId = curStore?.Id ?? 0;
            var stores = _storeContext.Stores;
            storeList.Add(new SelectListItem { Text = curStore.Name, Value = curStore.Id.ToString() });
            model.StoreList = new SelectList(storeList, "Value", "Text");

            //分级
            //model.ParentId = branch.Id;
            model = BindDropDownList<BranchModel>(model, _branchService.GetBranchsByParentId, curStore?.Id ?? 0, 0);

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });
        }

        [HttpPost]
        public JsonResult Edit(IFormCollection form)
        {
            try
            {


                var Id = !string.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : "0";
                var StoreId = !string.IsNullOrEmpty(form["StoreId"]) ? form["StoreId"].ToString() : "0";
                var DeptCode = !string.IsNullOrEmpty(form["DeptCode"]) ? form["DeptCode"].ToString() : "0";
                var DeptName = !string.IsNullOrEmpty(form["DeptName"]) ? form["DeptName"].ToString() : "";
                var DeptDesc = !string.IsNullOrEmpty(form["DeptDesc"]) ? form["DeptDesc"].ToString() : "";
                var ParentId = !string.IsNullOrEmpty(form["ParentId"]) ? form["ParentId"].ToString() : "0";
                var TreePath = !string.IsNullOrEmpty(form["TreePath"]) ? form["TreePath"].ToString() : "";
                var BranchLeader = !string.IsNullOrEmpty(form["BranchLeader"]) ? form["BranchLeader"].ToString() : "";
                var DisplayOrder = !string.IsNullOrEmpty(form["DisplayOrder"]) ? form["DisplayOrder"].ToString() : "0";
                var Status = !string.IsNullOrEmpty(form["Status"]) ? form["Status"].Contains("true").ToString() : "false";

                var branch = _branchService.GetBranchById(int.Parse(Id));
                if (branch != null)
                {
                    branch.StoreId = curStore?.Id ?? 0;
                    branch.DeptCode = int.Parse(DeptCode);
                    branch.DeptName = DeptName;
                    branch.DeptShort = 0;
                    branch.DeptDesc = DeptDesc;
                    branch.ParentId = int.Parse(ParentId);
                    branch.TreePath = TreePath;
                    branch.BranchLeader = BranchLeader;
                    branch.DisplayOrder = int.Parse(DisplayOrder);
                    branch.Status = bool.Parse(Status);
                }
                _branchService.UpdateBranch(branch);

                _userActivityService.InsertActivity("UpdateBranch", "更新部门", curUser, curUser.Id);
                _notificationService.SuccessNotification("更新部门成功");

                return Successful("更新部门成功");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {


            try
            {
                bool hasChilds = false;
                var module = _branchService.GetBranchById(id);
                if (module != null)
                {
                    hasChilds = _branchService.HasChilds(module);
                    if (!hasChilds)
                    {
                        _branchService.DeleteBranch(module);
                    }
                }

                if (!hasChilds)
                {
                    _userActivityService.InsertActivity("DeleteBranch", "删除部门", curUser, curUser.Id);
                    _notificationService.SuccessNotification("部门删除成功");

                    return Successful("部门删除成功");
                }
                else
                {
                    return Warning("删除失败，存在子部门");
                }

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 递归获取组织机构树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        public List<FancyTree> GetBranchList(int? store, int Id)
        {
            List<FancyTree> fancyTrees = new List<FancyTree>();
            var perentList = _branchService.GetBranchsByParentId(store.Value, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<FancyTree> tempList = GetBranchList(store.Value, b.Id);
                    var node = new FancyTree
                    {
                        id = b.Id,
                        title = b.DeptName,
                        expanded = true,
                        children = new List<FancyTree>()
                    };

                    if (tempList.Count > 0)
                    {
                        node.folder = true;
                        node.children = tempList;
                    }
                    fancyTrees.Add(node);
                }
            }
            return fancyTrees;
        }
    }
}