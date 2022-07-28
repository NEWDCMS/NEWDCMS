using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 用于上次售价管理
    /// </summary>
    public class RecentPriceController : BasePublicController
    {
        private readonly IProductService _productService;
        private readonly IUserActivityService _userActivityService;
        private readonly ITerminalService _terminalService;
        private readonly IRedLocker _locker;

        public RecentPriceController(
            IProductService productService,
            IUserActivityService userActivityService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            ITerminalService terminalService,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _productService = productService;
            _terminalService = terminalService;
            _locker = locker;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 价格列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.LastSalePriceView)]
        public IActionResult List()
        {
            var model = new RecentPriceListModel();
            return View(model);
        }

        public async Task<JsonResult> AsyncRecentPrices(string cumtomerName = "", string productName = "", int pagenumber = 0)
        {

            return await Task.Run(() =>
            {
                if (pagenumber > 0)
                {
                    pagenumber -= 1;
                }

                var recentPrices = _productService.GetAllRecentPrices(curStore?.Id ?? 0, productName, cumtomerName, pagenumber, 30);
                var allProducts = _productService.GetProductsByIds(curStore.Id, recentPrices.Select(pr => pr.ProductId).Distinct().ToArray());
                var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, recentPrices.Select(pr => pr.CustomerId).Distinct().ToArray());
                return Json(new
                {
                    total = recentPrices.TotalCount,
                    rows = recentPrices.Select(r =>
                    {
                        var product = allProducts.Where(ap => ap.Id == r.ProductId).FirstOrDefault();
                        var customer = allTerminal.Where(at => at.Id == r.CustomerId).FirstOrDefault();
                        var m = r.ToModel<RecentPriceModel>();
                        m.ProductName = product != null ? product.Name : "";
                        m.CustomerName = customer != null ? customer.Name : "";
                        return m;
                    }).ToList()
                });
            });
        }

        /// <summary>
        /// 更新价格
        /// </summary>
        /// <param name="list"></param>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.LastSalePriceSave)]
        public async Task<JsonResult> RecentPriceUpdate(RecentPriceUpdateModel data)
        {
            try
            {

                #region 验证
                if (data == null || data.Items == null && data.Items.Count == 0)
                {
                    return Warning("没有需要修改的数据");
                }
                #endregion

                List<RecentPrice> recentPrices = new List<RecentPrice>();
                data.Items.ForEach(p =>
                {
                    RecentPrice recentPrice = _productService.GetRecentPriceById(p.Id);
                    if (recentPrice != null)
                    {
                        //价格有改动则修改
                        if (recentPrice.SmallUnitPrice != p.SmallUnitPrice || recentPrice.StrokeUnitPrice != p.StrokeUnitPrice || recentPrice.BigUnitPrice != p.BigUnitPrice)
                        {
                            recentPrice.SmallUnitPrice = p.SmallUnitPrice;
                            recentPrice.StrokeUnitPrice = p.StrokeUnitPrice;
                            recentPrice.BigUnitPrice = p.BigUnitPrice;
                            recentPrice.UpdateTime = DateTime.Now;
                            recentPrices.Add(recentPrice);
                        }
                    }
                });

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(data.Items)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _productService.UpdateRecentPrice(curStore.Id, curUser.Id, recentPrices));

                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "上次售价更新失败", curUser.Id);
                _notificationService.SuccessNotification("上次售价更新失败");
                return Error(ex.Message);
            }

        }

    }
}
