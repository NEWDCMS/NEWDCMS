using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.WareHouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;


namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于库存表管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/reporting")]
    public class StockReportController : BaseAPIController
    {
        private readonly IWareHouseService _wareHouseService;
        private readonly ICategoryService _productCategoryService;
        private readonly IBrandService _brandService;
        private readonly IStockReportService _stockReportService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storeContext"></param>
        /// <param name="notificationService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productCategoryService"></param>
        /// <param name="brandService"></param>
        /// <param name="stockReportService"></param>
        /// <param name="logger"></param>
        /// <param name="workContext"></param>
        public StockReportController(
             IStoreContext storeContext,
             INotificationService notificationService,
             IWareHouseService wareHouseService,
             ICategoryService productCategoryService,
             IBrandService brandService,
             IStockReportService stockReportService,
             ILogger<BaseAPIController> logger,
             IWorkContext workContext) : base(logger)
        {
            _wareHouseService = wareHouseService;
            _productCategoryService = productCategoryService;
            _brandService = brandService;
            _stockReportService = stockReportService;
        }



        /// <summary>
        /// 获取商品实时库存
        /// </summary>
        /// <param name="store"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <param name="productName"></param>
        /// <param name="brandId"></param>
        /// <param name="status"></param>
        /// <param name="maxStock"></param>
        /// <param name="showZeroStack"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("stock/getStocks/{store}")]
        [SwaggerOperation("getStocks")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<StockReportProduct>>> GetStocks(int? store, int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pagenumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<StockReportProduct>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var categories = _productCategoryService.BindCategoryList(store).ToList();
                    var wareHouses = _wareHouseService.BindWareHouseList(store,null,0).ToList();
                    var brands = _brandService.BindBrandList(store).ToList();

                    //获取商品库存信息
                    var productStocks = _stockReportService.GetStockReportProduct(store,
                        wareHouseId,
                        categoryId,
                        productId,
                        productName,
                        brandId,
                        status,
                        maxStock,
                        showZeroStack, pagenumber, 50);

                    foreach (var item in productStocks)
                    {
                        item.CurrentQuantityPart = Pexts.StockQuantityFormat(item.CurrentQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                        item.UsableQuantityPart = Pexts.StockQuantityFormat(item.UsableQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                        item.OrderQuantityPart = Pexts.StockQuantityFormat(item.OrderQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                    }

                    return this.Successful2("", productStocks);
                }
                catch (Exception ex)
                {
                    return this.Error2<StockReportProduct>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 库存预警表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        [HttpGet("stock/getEarlyWarning/{store}/{wareHouseId}")]
        [SwaggerOperation("getEarlyWarning")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<EarlyWarning>>> GetEarlyWarning(int? store, int? wareHouseId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<EarlyWarning>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var earlyWarnings = new List<EarlyWarning>();
                    earlyWarnings = _stockReportService.GetEarlyWarning(store, wareHouseId).ToList();

                    return this.Successful2("", earlyWarnings);
                }
                catch (Exception ex)
                {
                    return this.Error2<EarlyWarning>(ex.Message);
                }
            });
        }


    }
}