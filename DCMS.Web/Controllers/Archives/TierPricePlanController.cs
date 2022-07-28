using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    public class TierPricePlanController : BasePublicController
    {
        private readonly IProductService _productService;
        private readonly IUserActivityService _userActivityService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;

        public TierPricePlanController(
            IProductService productService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IProductTierPricePlanService productTierPricePlanService,
            ILogger loggerService,
            INotificationService notificationService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _productService = productService;
            _productTierPricePlanService = productTierPricePlanService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        [AuthCode((int)AccessGranularityEnum.PricesPlanView)]
        public IActionResult List(string name, int pagenumber = 0)
        {

            var listModel = new ProductTierPricePlanListModel();

            if (pagenumber > 1)
            {
                pagenumber -= 1;
            }

            var plans = _productService.GetAllPlans(curStore == null ? 0 : curStore.Id, name, pageIndex: pagenumber, pageSize: 10);
            listModel.PagingFilteringContext.LoadPagedList(plans);
            listModel.Items = plans.Select(s => s.ToModel<ProductTierPricePlanModel>()).ToList();

            return View(listModel);
        }

        [AuthCode((int)AccessGranularityEnum.PricesPlanSave)]
        public JsonResult Create()
        {
            var model = new ProductTierPricePlanModel();
            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Create", model)
            });
        }


        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PricesPlanSave)]
        public JsonResult Create(ProductTierPricePlanModel model, bool continueEditing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                var specs = new List<string>() { "进价", "成本价", "批发价", "零售价", "最低售价" };

                if (specs.Contains(model.Name))
                {
                    ModelState.AddModelError("", "名称不规范");
                }

                if (ModelState.IsValid)
                {
                    var productTierPricePlan = model.ToEntity<ProductTierPricePlan>();
                    productTierPricePlan.StoreId = curStore?.Id ?? 0;
                    if(_productTierPricePlanService.ProductTierPricePlansId(curStore?.Id ?? 0, model.Name) > 0)
                    {
                        //活动日志
                        _userActivityService.InsertActivity("InsertProductTierPricePlans", "方案名已被使用", curUser.Id);
                        _notificationService.SuccessNotification("方案名已被使用");

                        return Warning("方案名已被使用");
                    }
                    else
                    {
                        _productTierPricePlanService.InsertProductTierPricePlan(productTierPricePlan);

                        //活动日志
                        _userActivityService.InsertActivity("InsertProductTierPricePlans", "创建方案", curUser.Id);
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
                //活动日志
                _userActivityService.InsertActivity("InsertProductTierPricePlans", "创建方案", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.PricesPlanView)]
        public JsonResult Edit(int? id)
        {

            var model = new ProductTierPricePlanModel();

            if (id.HasValue)
            {
                var productTierPricePlan = _productTierPricePlanService.GetProductTierPricePlanById(curStore.Id, id.Value);
                if (productTierPricePlan != null)
                {
                    //只能操作当前经销商数据
                    if (productTierPricePlan.StoreId != curStore.Id)
                    {
                        return Warning("权限不足!");
                    }

                    model = productTierPricePlan.ToModel<ProductTierPricePlanModel>();
                }
            }

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("Edit", model)
            });
        }

        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PricesPlanSave)]
        public JsonResult Edit(ProductTierPricePlanModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("", "名称不能为空");
                }

                var specs = new List<string>() { "进价", "成本价", "批发价", "零售价", "最低售价" };

                if (specs.Contains(model.Name))
                {
                    ModelState.AddModelError("", "名称不规范");
                }

                if (ModelState.IsValid)
                {

                    var productTierPricePlan = _productTierPricePlanService.GetProductTierPricePlanById(curStore.Id, model.Id);
                    if (productTierPricePlan != null)
                    {
                        productTierPricePlan.Name = model.Name;
                        _productTierPricePlanService.UpdateProductTierPricePlan(productTierPricePlan);
                        //活动日志
                        _userActivityService.InsertActivity("UpdateProductTierPricePlans", "编辑方案", curUser.Id);
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
                _userActivityService.InsertActivity("UpdateProductTierPricePlans", "编辑方案", curUser.Id);
                _notificationService.SuccessNotification(ex.Message);
                return Warning(ex.ToString());
            }

        }

        [AuthCode((int)AccessGranularityEnum.PricesPlanSave)]
        public JsonResult Delete(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var ProductTierPricePlans = _productTierPricePlanService.GetProductTierPricePlans(int.Parse(ids));
                if (ProductTierPricePlans.Count > 0)
                {
                    return Warning("方案已被使用");
                }

                int[] sids = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => int.Parse(u)).ToArray();
                var plans = _productTierPricePlanService.GetProductTierPricePlansByIds(sids);
                foreach (var productTierPricePlan in plans)
                {
                    if (productTierPricePlan != null)
                    {
                        _productTierPricePlanService.DeleteProductTierPricePlan(productTierPricePlan);
                    }
                }

                //活动日志
                _userActivityService.InsertActivity("DeleteProductTierPricePlans", "删除方案", curUser, ids);
                return Successful("删除方案成功");
            }
            else
                return Warning("方案未找到");
        }



        /// <summary>
        /// 获取价格方案
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> AsyncGetAllPricePlan()
        {
            return await Task.Run(() =>
            {

                var items = _productTierPricePlanService.GetAllPricePlan(curStore?.Id ?? 0).Select(o =>
                  {
                      return o;

                  }).ToList();

                return Json(new
                {
                    Success = true,
                    data = items
                });
            });
        }

    }
}
