using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.ViewModel.Models.Purchases;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 采购报表
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/purchases")]
    public class PurchaseReportController : BaseAPIController
    {
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPurchaseReportService _purchaseReportService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="statisticalTypeService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="purchaseReportService"></param>
        public PurchaseReportController(
            IStatisticalTypeService statisticalTypeService,
            IManufacturerService manufacturerService,
            IPurchaseReportService purchaseReportService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _statisticalTypeService = statisticalTypeService;
            _manufacturerService = manufacturerService;

            _purchaseReportService = purchaseReportService;
        }

        /// <summary>
        /// 获取采购明细表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="billNumber"></param>
        /// <param name="purchaseTypeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpGet("purchasereport/getPurchaseReportItem/{store}/{productId}/{categoryId}/{wareHouseId}")]
        [SwaggerOperation("getPurchaseReportItem")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PurchaseReportItem>>> GetPurchaseReportItem(int? store, int? makeuserId, int? productId, string productName, int? categoryId, int? manufacturerId, int? wareHouseId, string billNumber, int? purchaseTypeId, DateTime? startTime, DateTime? endTime, string remark, int pagenumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<PurchaseReportItem>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var results = _purchaseReportService.GetPurchaseReportItem(store != null ? store : 0,
                        makeuserId,
                       productId, productName, categoryId, manufacturerId, wareHouseId,
                       billNumber, purchaseTypeId,
                       startTime,
                       endTime,
                       remark
                     );

                    return this.Successful2("", results);
                }
                catch (Exception ex)
                {
                    return this.Error2<PurchaseReportItem>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取采购汇总（按商品）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageSize"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("purchasereport/getPurchaseReportSummaryProduct/{store}/{productId}")]
        [SwaggerOperation("getPurchaseReportSummaryProduct")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PurchaseReportSummaryProduct>>> GetPurchaseReportSummaryProduct(int? store, int? productId, string productName = null, int? categoryId = 0, int? wareHouseId = 0, int? manufacturerId = 0, DateTime? startTime = null, DateTime? endTime = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<PurchaseReportSummaryProduct>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryProduct(store != null ? store : 0,
                   categoryId, productId, productName, manufacturerId, wareHouseId,
                   startTime, endTime
                   ).Select(sd =>
                   {
                       return sd ?? new PurchaseReportSummaryProduct();

                   }).AsQueryable();

                    var items = new PagedList<PurchaseReportSummaryProduct>(sqlDatas, pagenumber, pageSize).ToList();

                    return this.Successful2(Resources.Successful, items);
                }
                catch (Exception ex)
                {
                    return this.Error2<PurchaseReportSummaryProduct>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 采购汇总（按供应商）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="manufacturerId"></param>
        /// <returns></returns>
        [HttpGet("purchasereport/getPurchaseReportSummaryManufacturer/{store}/{productId}")]
        [SwaggerOperation("getPurchaseReportSummaryManufacturer")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<PurchaseReportSummaryManufacturerListModel>> GetPurchaseReportSummaryManufacturer(int? store, DateTime? startTime, DateTime? endTime, int? manufacturerId, int pagenumber = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PurchaseReportSummaryManufacturerListModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                var model = new PurchaseReportSummaryManufacturerListModel();
                try
                {
                    //商品统计类别动态列
                    var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store != null ? store : 0);
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    if (statisticalTypes != null && statisticalTypes.Count > 0)
                    {
                        //model.DynamicColumns = statisticalTypes.OrderBy(a => a.Id).Select(a => a.Name).ToList();
                        foreach (var c in statisticalTypes)
                        {
                            model.DynamicColumns.Add(c.Name);
                            dic.Add(c.Id, c.Name);
                        }
                    }
                    model.DynamicColumns.Add("其他");
                    dic.Add((int)StatisticalTypeEnum.OtherTypeId, "其他");

                    model.StartTime = startTime ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                    model.EndTime = endTime ?? DateTime.Now.AddDays(1);

                    //供应商
                    model.Manufacturers = BindManufacturerSelection(new Func<int?, IList<Manufacturer>>(_manufacturerService.BindManufacturerList), store);
                    model.ManufacturerId = (manufacturerId ?? null);
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    var sqlDatas = _purchaseReportService.GetPurchaseReportSummaryManufacturer(store != null ? store : 0, model.StartTime, model.EndTime, manufacturerId, dic
                        ).Select(sd =>
                        {
                            return sd == null ? new PurchaseReportSummaryManufacturer() : sd;

                        }).AsQueryable();

                    var items = new PagedList<PurchaseReportSummaryManufacturer>(sqlDatas, pagenumber, 30);

                    model.Items = items;

                    #region 汇总

                    //动态列汇总
                    if (model.DynamicColumns != null && model.DynamicColumns.Count > 0 && dic.Keys != null && dic.Keys.Count > 0)
                    {
                        foreach (var k in dic.Keys)
                        {
                            model.TotalDynamicDatas.Add(new PurchaseReportSumStatisticalType()
                            {
                                StatisticalTypeId = k
                            });
                        }
                    }
                    //动态列
                    if (model.Items != null && model.Items.Count > 0)
                    {
                        model.Items.ToList().ForEach(a =>
                        {
                            if (a.PurchaseReportStatisticalTypes != null && a.PurchaseReportStatisticalTypes.Count > 0)
                            {
                                a.PurchaseReportStatisticalTypes.ToList().ForEach(b =>
                                {
                                    PurchaseReportSumStatisticalType purchaseReportSumStatisticalType = model.TotalDynamicDatas.Where(c => c.StatisticalTypeId == b.StatisticalTypeId).FirstOrDefault();
                                    if (purchaseReportSumStatisticalType != null)
                                    {
                                        purchaseReportSumStatisticalType.PurchaseSmallQuantity = (purchaseReportSumStatisticalType.PurchaseSmallQuantity ?? 0) + (b.SmallQuantity ?? 0);
                                        purchaseReportSumStatisticalType.OrderAmount = (purchaseReportSumStatisticalType.OrderAmount ?? 0) + (b.OrderAmount ?? 0);
                                    }
                                });
                            }
                        });
                    }
                    //主列
                    if (model.Items != null && model.Items.Count > 0)
                    {
                        //采购数量
                        model.TotalSumPurchaseSmallQuantity = model.Items.Sum(m => m.SumPurchaseSmallQuantity ?? 0);
                        //单据金额
                        model.TotalSumOrderAmount = model.Items.Sum(m => m.SumOrderAmount ?? 0);
                    }

                    #endregion

                    return this.Successful3(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<PurchaseReportSummaryManufacturerListModel>(ex.Message);
                }

            });
        }
    }
}