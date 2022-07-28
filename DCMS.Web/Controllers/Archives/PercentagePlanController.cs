using DCMS.Core;
using DCMS.Core.Domain.Plan;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.plan;
using DCMS.ViewModel.Models.Plan;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于表示提成方案
    /// </summary>
    public class PercentagePlanController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IPercentagePlanService _percentagePlanService;


        public PercentagePlanController(
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPercentagePlanService percentagePlanService,
            ILogger loggerService,
            INotificationService notificationService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _percentagePlanService = percentagePlanService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        [AuthCode((int)AccessGranularityEnum.PercentagePlanView)]
        public IActionResult List(string name, int pagenumber = 0)
        {

            var listModel = new PercentagePlanListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var percentagePlans = _percentagePlanService.GetAllPercentagePlans(curStore?.Id ?? 0, name, pageIndex: pagenumber, pageSize: 10);
            listModel.PagingFilteringContext.LoadPagedList(percentagePlans);

            listModel.Items = percentagePlans.Select(s =>
            {

                var m = s.ToModel<PercentagePlanModel>();
                m.PlanTypeName = CommonHelper.GetEnumDescription(m.PlanType);
                return m;
            }).ToList();


            return View(listModel);
        }


        /// <summary>
        /// 异步方案搜索
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonResult AsyncSearch(string name = "", int pagenumber = 0)
        {
            var listModel = new PercentagePlanListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var percentagePlans = _percentagePlanService.GetAllPercentagePlans(curStore?.Id ?? 0, name, pageIndex: pagenumber, pageSize: 10);
            listModel.PagingFilteringContext.LoadPagedList(percentagePlans);

            listModel.Items = percentagePlans.Select(s =>
            {

                var m = s.ToModel<PercentagePlanModel>();
                m.PlanTypeName = CommonHelper.GetEnumDescription(m.PlanType);
                return m;
            }).ToList();

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncSearch", listModel)
            });
        }

        /// <summary>
        /// 异步方案列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncList(string name = "", int pagenumber = 0, int pageSize = 10)
        {

            return await Task.Run(() =>
            {
                if (pagenumber > 0)
                {
                    pagenumber -= 1;
                }

                var percentagePlans = _percentagePlanService.GetAllPercentagePlans(curStore?.Id ?? 0, name, pageIndex: pagenumber, pageSize: pageSize);

                return Json(new
                {
                    Success = true,
                    total = percentagePlans.TotalCount,
                    rows = percentagePlans.Select(s => s.ToModel<PercentagePlanModel>()).ToList()
                });
            });
        }

        [AuthCode((int)AccessGranularityEnum.PercentagePlan)]
        public JsonResult Create()
        {
            var model = new PercentagePlanModel();
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }


        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PercentagePlan)]
        public JsonResult Create(PercentagePlanModel model, bool continueEditing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                if (ModelState.IsValid)
                {
                    var percentagePlan = model.ToEntity<PercentagePlan>();
                    percentagePlan.StoreId = curStore?.Id ?? 0;
                    percentagePlan.CreatedOnUtc = DateTime.Now;
                    percentagePlan.PlanTypeId = model.PlanTypeId;
                    if(_percentagePlanService.PercentagePlanId(curStore?.Id ?? 0, model.Name) > 0)
                    {
                        _userActivityService.InsertActivity("InsertPercentagePlans", "方案名称已被使用", curUser.Id);
                        _notificationService.SuccessNotification("方案名称已被使用");
                        return Warning("方案名称已被使用");
                    }
                    else
                    {
                        _percentagePlanService.InsertPercentagePlan(percentagePlan);

                        //活动日志
                        _userActivityService.InsertActivity("InsertPercentagePlans", "创建方案", curUser.Id);
                        _notificationService.SuccessNotification("创建方案成功");
                        return Successful("添加成功");
                    }

                    
                }
                else
                {
                    return Warning("数据验证失败");
                }
            }
            catch (Exception ex)
            {

                _userActivityService.InsertActivity("InsertPercentagePlans", "创建方案", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.PercentagePlan)]
        public JsonResult Edit(int? id)
        {

            var model = new PercentagePlanModel();

            if (id.HasValue)
            {
                var percentagePlan = _percentagePlanService.GetPercentagePlanById(curStore.Id, id.Value);
                if (percentagePlan != null)
                {
                    //只能操作当前经销商数据
                    if (percentagePlan.StoreId != curStore.Id)
                    {
                        return Warning("数据不存在!");
                    }

                    model = percentagePlan.ToModel<PercentagePlanModel>();
                }
            }
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PercentagePlan)]
        public JsonResult Edit(PercentagePlanModel model)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                if (ModelState.IsValid)
                {

                    var percentagePlan = _percentagePlanService.GetPercentagePlanById(curStore.Id, model.Id);
                    if (percentagePlan != null)
                    {
                        percentagePlan.Name = model.Name;
                        percentagePlan.IsByReturn = model.IsByReturn;
                        percentagePlan.PlanTypeId = model.PlanTypeId;

                        _percentagePlanService.UpdatePercentagePlan(percentagePlan);
                        //活动日志
                        _userActivityService.InsertActivity("UpdatePercentagePlans", "编辑方案", curUser.Id);
                        _notificationService.SuccessNotification("编辑方案成功");
                    }

                    return Successful("编辑成功");
                }
                else
                {
                    return Warning("数据验证失败");
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("UpdatePercentagePlans", "编辑方案", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.PercentagePlan)]
        public IActionResult Delete(string ids)
        {

            if (!string.IsNullOrEmpty(ids))
            {

                var percentage = _percentagePlanService.GetPercentagePlans(int.Parse(ids));
                if (percentage.Count > 0)
                {

                }
                else
                {
                    int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                    var percentagePlans = _percentagePlanService.GetPercentagePlansByIds(sids);
                    foreach (var percentagePlan in percentagePlans)
                    {
                        if (percentagePlan != null)
                        {
                            _percentagePlanService.DeletePercentagePlan(percentagePlan);
                        }
                    }
                    //活动日志
                    _userActivityService.InsertActivity("DeletePercentagePlans", "删除方案", curUser, ids);
                    _notificationService.SuccessNotification("删除方案成功");
                }

            }

            return RedirectToAction("List");
        }


    }
}
