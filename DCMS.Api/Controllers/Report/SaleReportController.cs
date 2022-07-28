using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.ViewModel.Models.Sales;
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
    /// 销售报表
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/reporting")]
    public class SaleReportController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISaleBillService _saleService;
        private readonly ISaleReportService _saleReportService;
        private readonly IReturnBillService _returnService;
        private readonly ISaleReservationBillService _saleReservationBillService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="productService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="saleService"></param>
        /// <param name="saleReportService"></param>
        /// <param name="returnService"></param>
        public SaleReportController(
            ITerminalService terminalService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ISaleBillService saleService,
            ISaleReportService saleReportService,
            IReturnBillService returnService,
            ISaleReservationBillService saleReservationBillService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;

            _saleService = saleService;
            _saleReportService = saleReportService;

            _returnService = returnService;
            _saleReservationBillService = saleReservationBillService;
        }

        /// <summary>
        /// 获取经销商品牌销量汇总
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="brandIds">品牌Id</param>
        /// <param name="businessUserId">业务员Id</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("saleReport/getBrandRanking/{store}")]
        [SwaggerOperation("getBrandRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BrandRankingModel>>> GetBrandRanking(int? store, [FromQuery] int[] brandIds, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false, bool? auditedStatus = true)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<BrandRankingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                var model = new List<BrandRankingModel>();
                try
                {

                    #region 日期格式化
                    if (startTime != null)
                    {
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    }

                    if (endTime != null)
                    {
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    }
                    #endregion

                    model = _saleReportService.GetBrandRanking(store, brandIds, businessUserId, startTime, endTime, force: force, auditedStatus: auditedStatus).Select(u => u.ToModel<BrandRankingModel>()).ToList();

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<BrandRankingModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取经销商业务员排行榜
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="businessUserId">业务员Id</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("saleReport/getBusinessRanking/{store}")]
        [SwaggerOperation("getBusinessRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BusinessRankingModel>>> GetBusinessRanking(int? store, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<BusinessRankingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new List<BusinessRankingModel>();

                try
                {
                    #region 日期格式化
                    if (startTime != null)
                    {
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    }

                    if (endTime != null)
                    {
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    }
                    #endregion

                    model = _saleReportService.GetBusinessRanking(store, businessUserId, startTime, endTime, force: force).Select(s => s.ToModel<BusinessRankingModel>()).ToList();

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<BusinessRankingModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取经销商销量走势图
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="dateType">查询类型：day，week，month</param>
        /// <returns></returns>
        [HttpGet("saleReport/getSaleTrending/{store}/{dateType}")]
        [SwaggerOperation("getSaleTrending")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<SaleTrending>>> GetSaleTrending(int? store, string dateType, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<SaleTrending>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                var model = new List<SaleTrending>();

                try
                {
                    //day,week,month
                    DateTime beginDate = DateTime.Now;
                    switch (dateType.ToLower())
                    {
                        case "day":
                            //当前日期前15天
                            beginDate = DateTime.Now.AddDays(-15);
                            for (int i = 0; i < 15; i++)
                            {
                                SaleTrending saleTrending = new SaleTrending
                                {
                                    DateType = "day",
                                    SaleDate = DateTime.Parse(DateTime.Now.AddDays(i * (-1)).ToString("yyyy-MM-dd")),
                                    SaleAmount = 0,
                                    SaleReturnAmount = 0,
                                    NetAmount = 0
                                };
                                model.Add(saleTrending);
                            }
                            break;
                        case "week":
                            //当前日期前7天
                            beginDate = DateTime.Now.AddDays(-7);
                            for (int i = 0; i < 7; i++)
                            {
                                SaleTrending saleTrending = new SaleTrending
                                {
                                    DateType = "week",
                                    SaleDate = DateTime.Parse(DateTime.Now.AddDays(i * (-1)).ToString("yyyy-MM-dd")),
                                    SaleAmount = 0,
                                    SaleReturnAmount = 0,
                                    NetAmount = 0
                                };
                                model.Add(saleTrending);
                            }
                            break;
                        case "month":
                            //当前日期前12个月
                            beginDate = DateTime.Now.AddMonths(-12);
                            for (int i = 0; i < 12; i++)
                            {
                                SaleTrending saleTrending = new SaleTrending
                                {
                                    DateType = "month",
                                    SaleDate = DateTime.Parse(DateTime.Now.AddMonths(i * (-1)).ToString("yyyy-MM")),
                                    SaleAmount = 0,
                                    SaleReturnAmount = 0,
                                    NetAmount = 0
                                };
                                model.Add(saleTrending);
                            }
                            break;
                        default:
                            break;
                    }

                    //销售
                    var saleBills = new List<SaleBill>();
                    //已审核、未红冲
                    saleBills = _saleService.GetSaleBillsByStoreId(store ?? 0, true, false, beginDate).ToList();

                    //退货
                    var returnBills = new List<ReturnBill>();
                    //已审核、未红冲
                    returnBills = _returnService.GetReturnBillsByStoreId(store ?? 0, true, false, beginDate).ToList();

                    if (model != null && model.Count > 0)
                    {
                        model.ForEach(m =>
                        {
                            //销售
                            if (saleBills != null && saleBills.Count > 0)
                            {
                                saleBills.ForEach(s =>
                                {
                                    switch (dateType.ToLower())
                                    {
                                        case "day":
                                            if (s.TransactionDate != null)
                                            {
                                                s.TransactionDate = DateTime.Parse(((DateTime)s.TransactionDate).ToString("yyyy-MM-dd"));
                                            }
                                            break;
                                        case "week":
                                            if (s.TransactionDate != null)
                                            {
                                                s.TransactionDate = DateTime.Parse(((DateTime)s.TransactionDate).ToString("yyyy-MM-dd"));
                                            }
                                            break;
                                        case "month":
                                            if (s.TransactionDate != null)
                                            {
                                                s.TransactionDate = DateTime.Parse(((DateTime)s.TransactionDate).ToString("yyyy-MM"));
                                            }
                                            break;
                                        default:
                                            break;

                                    }

                                    if (s.TransactionDate == m.SaleDate)
                                    {
                                        m.SaleAmount += s.ReceivableAmount;
                                    }
                                });
                            }

                            //销售退货
                            if (returnBills != null && returnBills.Count > 0)
                            {
                                returnBills.ForEach(r =>
                                {
                                    switch (dateType.ToLower())
                                    {
                                        case "day":
                                            if (r.TransactionDate != null)
                                            {
                                                r.TransactionDate = DateTime.Parse(((DateTime)r.TransactionDate).ToString("yyyy-MM-dd"));
                                            }
                                            break;
                                        case "week":
                                            if (r.TransactionDate != null)
                                            {
                                                r.TransactionDate = DateTime.Parse(((DateTime)r.TransactionDate).ToString("yyyy-MM-dd"));
                                            }
                                            break;
                                        case "month":
                                            if (r.TransactionDate != null)
                                            {
                                                r.TransactionDate = DateTime.Parse(((DateTime)r.TransactionDate).ToString("yyyy-MM"));
                                            }
                                            break;
                                        default:
                                            break;

                                    }

                                    if (r.TransactionDate == m.SaleDate)
                                    {
                                        m.SaleReturnAmount += r.ReceivableAmount;
                                    }
                                });
                            }

                        });
                    }

                    //净额
                    if (model != null && model.Count > 0)
                    {
                        model.ForEach(m =>
                        {
                            m.NetAmount = m.SaleAmount - m.SaleReturnAmount;
                        });
                    }

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<SaleTrending>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取经销商热销排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId">客户</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌</param>
        /// <param name="categoryId">类别</param>
        /// <param name="startTime">开始时间:yyyy-MM-dd 00:00:00</param>
        /// <param name="endTime">结束时间:yyyy-MM-dd 00:00:00</param>
        /// <returns></returns>
        [HttpGet("saleReport/getHotSaleRanking/{store}")]
        [SwaggerOperation("getHotSaleRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<HotSaleRankingModel>>> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<HotSaleRankingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new List<HotSaleRankingModel>();

                try
                {
                    #region 日期格式化

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    #endregion

                    if (store > 0)
                    {
                        model = _saleReportService.GetHotSaleRanking(store, terminalId, businessUserId, brandId, categoryId, startTime, endTime, force: force).Select(s => s.ToModel<HotSaleRankingModel>()).ToList();
                    }

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<HotSaleRankingModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取经销商商品成本利润排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId">客户</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌</param>
        /// <param name="categoryId">类别</param>
        /// <param name="startTime">开始时间:yyyy-MM-dd 00:00:00</param>
        /// <param name="endTime">结束时间:yyyy-MM-dd 00:00:00</param>
        /// <returns></returns>
        [HttpGet("saleReport/getCostProfitRanking/{store}")]
        [SwaggerOperation("getCostProfitRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CostProfitRankingModel>>> GetCostProfitRanking(int? store, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<CostProfitRankingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new List<CostProfitRankingModel>();
                try
                {

                    #region 日期格式化
                    if (startTime != null)
                    {
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    }

                    if (endTime != null)
                    {
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    }
                    #endregion

                    //销售单
                    IList<SaleBill> saleBills = _saleService.GetCostProfitRanking(store, terminalId, businessUserId, startTime, endTime);
                    if (saleBills != null && saleBills.Count > 0)
                    {
                        List<int> allProductIds = new List<int>();
                        saleBills.ToList().ForEach(sr =>
                        {
                            if (sr.Items != null && sr.Items.Count > 0)
                            {
                                allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                            }
                            //去重
                            allProductIds = allProductIds.Distinct().ToList();
                        });
                        var allProducts = _productService.GetProductsByIds(store ?? 0, allProductIds.ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

                        saleBills.ToList().ForEach(sb =>
                        {
                            if (sb != null && sb.Items != null && sb.Items.Count > 0)
                            {
                                sb.Items.ToList().ForEach(item =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        bool fg = true;
                                        //品牌过滤
                                        if (brandId > 0 && product.BrandId != brandId)
                                        {
                                            fg = false;
                                        }
                                        //类别过滤
                                        if (categoryId > 0 && product.CategoryId != categoryId)
                                        {
                                            fg = false;
                                        }

                                        if (fg)
                                        {
                                            //商品转化量
                                            var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = item.Quantity * conversionQuantity;

                                            //只根据商品分组
                                            CostProfitRankingModel costProfitRanking = model.Where(m => m.ProductId == item.ProductId).FirstOrDefault();
                                            if (costProfitRanking != null)
                                            {
                                                costProfitRanking.TotalSumNetQuantity += thisQuantity;
                                                //转换
                                                //costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0, _specificationAttributeService, _productService);
                                                costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0);
                                                costProfitRanking.TotalSumNetAmount += item.Amount;
                                                costProfitRanking.TotalSumProfit += item.Profit;
                                            }
                                            else
                                            {
                                                costProfitRanking = new CostProfitRankingModel
                                                {
                                                    ProductId = item.ProductId,
                                                    ProductName = product.Name,
                                                    BrandId = product.BrandId,
                                                    CategoryId = product.CategoryId,

                                                    TotalSumNetQuantity = thisQuantity
                                                };
                                                //转换
                                                //costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0, _specificationAttributeService, _productService);
                                                costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0);
                                                costProfitRanking.TotalSumNetAmount = item.Amount;
                                                costProfitRanking.TotalSumProfit = item.Profit;

                                                model.Add(costProfitRanking);
                                            }
                                        }
                                    }

                                });
                            }
                        });
                    }

                    //退货单
                    IList<ReturnBill> returnBills = _returnService.GetCostProfitRanking(store, terminalId, businessUserId, startTime, endTime);
                    if (returnBills != null && returnBills.Count > 0)
                    {
                        List<int> allProductIds = new List<int>();
                        returnBills.ToList().ForEach(sr =>
                        {
                            if (sr.Items != null && sr.Items.Count > 0)
                            {
                                allProductIds.AddRange(sr.Items.Select(it => it.ProductId).Distinct().ToList());
                            }
                            //去重
                            allProductIds = allProductIds.Distinct().ToList();
                        });
                        var allProducts = _productService.GetProductsByIds(store ?? 0, allProductIds.ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

                        returnBills.ToList().ForEach(rb =>
                        {
                            if (rb != null && rb.Items != null && rb.Items.Count > 0)
                            {
                                rb.Items.ToList().ForEach(item =>
                                {
                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                    if (product != null)
                                    {
                                        bool fg = true;
                                        //品牌过滤
                                        if (brandId > 0 && product.BrandId != brandId)
                                        {
                                            fg = false;
                                        }
                                        //类别过滤
                                        if (categoryId > 0 && product.CategoryId != categoryId)
                                        {
                                            fg = false;
                                        }

                                        if (fg)
                                        {
                                            //商品转化量
                                            var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                            //库存量增量 = 单位转化量 * 数量
                                            int thisQuantity = item.Quantity * conversionQuantity;

                                            //只根据商品分组
                                            CostProfitRankingModel costProfitRanking = model.Where(m => m.ProductId == item.ProductId).FirstOrDefault();
                                            if (costProfitRanking != null)
                                            {
                                                costProfitRanking.TotalSumNetQuantity -= thisQuantity;
                                                //转换
                                                //costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0, _specificationAttributeService, _productService);
                                                costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0);
                                                costProfitRanking.TotalSumNetAmount -= item.Amount;
                                                costProfitRanking.TotalSumProfit -= item.Profit;
                                            }
                                            else
                                            {
                                                costProfitRanking = new CostProfitRankingModel
                                                {
                                                    ProductId = item.ProductId,
                                                    ProductName = product.Name,
                                                    BrandId = product.BrandId,
                                                    CategoryId = product.CategoryId,

                                                    TotalSumNetQuantity = (-1) * thisQuantity
                                                };
                                                //转换
                                                //costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0, _specificationAttributeService, _productService);
                                                costProfitRanking.TotalSumNetQuantityConversion = product.GetConversionFormat(allOptions, product.SmallUnitId, costProfitRanking.TotalSumNetQuantity ?? 0);

                                                costProfitRanking.TotalSumNetAmount = (-1) * item.Amount;
                                                costProfitRanking.TotalSumProfit = (-1) * item.Profit;

                                                model.Add(costProfitRanking);
                                            }
                                        }
                                    }

                                });
                            }
                        });
                    }

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<CostProfitRankingModel>(ex.Message);
                }


            });
        }

        /// <summary>
        /// 销售额分析API
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">类别Id</param>
        /// <returns></returns>
        [HttpGet("saleReport/getSaleAnalysis/{store}")]
        [SwaggerOperation("getSaleAnalysis")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<SaleAnalysisModel>> GetSaleAnalysis(int? store, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<SaleAnalysisModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {

                    var saleAnalysisQuery = _saleReportService.GetSaleAnalysis(store, businessUserId, brandId, productId, categoryId, force: force);

                    var saleAnalysis = new SaleAnalysisModel();

                    if (saleAnalysisQuery != null)
                    {
                        //业务员
                        saleAnalysis.BusinessUserId = saleAnalysisQuery.BusinessUserId;
                        saleAnalysis.BusinessUserName = saleAnalysisQuery.BusinessUserName;
                        //品牌
                        saleAnalysis.BrandId = saleAnalysisQuery.BrandId;
                        saleAnalysis.BrandName = saleAnalysisQuery.BrandName;
                        //商品
                        saleAnalysis.ProductId = saleAnalysisQuery.ProductId;
                        saleAnalysis.ProductName = saleAnalysisQuery.ProductName;
                        //商品类别
                        saleAnalysis.CategoryId = saleAnalysisQuery.CategoryId;
                        saleAnalysis.CategoryName = saleAnalysisQuery.CategoryName;

                        saleAnalysis.Today.SaleAmount = saleAnalysisQuery.Today.SaleAmount; saleAnalysis.Today.SaleReturnAmount = saleAnalysisQuery.Today.SaleReturnAmount; saleAnalysis.Today.NetAmount = saleAnalysisQuery.Today.NetAmount;

                        saleAnalysis.Yesterday.SaleAmount = saleAnalysisQuery.Yesterday.SaleAmount; saleAnalysis.Yesterday.SaleReturnAmount = saleAnalysisQuery.Yesterday.SaleReturnAmount;
                        saleAnalysis.Yesterday.NetAmount = saleAnalysisQuery.Yesterday.NetAmount;
                        saleAnalysis.BeforeYesterday.SaleAmount = saleAnalysisQuery.BeforeYesterday.SaleAmount; saleAnalysis.BeforeYesterday.SaleReturnAmount = saleAnalysisQuery.BeforeYesterday.SaleReturnAmount; saleAnalysis.BeforeYesterday.NetAmount = saleAnalysisQuery.BeforeYesterday.NetAmount;
                        saleAnalysis.LastWeek.SaleAmount = saleAnalysisQuery.LastWeek.SaleAmount; saleAnalysis.LastWeek.SaleReturnAmount = saleAnalysisQuery.LastWeek.SaleReturnAmount; saleAnalysis.LastWeek.NetAmount = saleAnalysisQuery.LastWeek.NetAmount;
                        saleAnalysis.ThisWeek.SaleAmount = saleAnalysisQuery.ThisWeek.SaleAmount; saleAnalysis.ThisWeek.SaleReturnAmount = saleAnalysisQuery.ThisWeek.SaleReturnAmount; saleAnalysis.ThisWeek.NetAmount = saleAnalysisQuery.ThisWeek.NetAmount;
                        saleAnalysis.LastMonth.SaleAmount = saleAnalysisQuery.LastMonth.SaleAmount; saleAnalysis.LastMonth.SaleReturnAmount = saleAnalysisQuery.LastMonth.SaleReturnAmount;
                        saleAnalysis.LastMonth.NetAmount = saleAnalysisQuery.LastMonth.NetAmount;
                        saleAnalysis.ThisMonth.SaleAmount = saleAnalysisQuery.ThisMonth.SaleAmount; saleAnalysis.ThisMonth.SaleReturnAmount = saleAnalysisQuery.ThisMonth.SaleReturnAmount;
                        saleAnalysis.ThisMonth.NetAmount = saleAnalysisQuery.ThisMonth.NetAmount;
                        saleAnalysis.ThisYear.SaleAmount = saleAnalysisQuery.ThisYear.SaleAmount; saleAnalysis.ThisYear.SaleReturnAmount = saleAnalysisQuery.ThisYear.SaleReturnAmount;
                        saleAnalysis.ThisYear.NetAmount = saleAnalysisQuery.ThisYear.NetAmount;

                    }

                    return this.Successful3("", saleAnalysis);
                }
                catch (Exception ex)
                {
                    return this.Error3<SaleAnalysisModel>(ex.Message);
                }


            });
        }

        /// <summary>
        /// 客户拜访分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <returns></returns>
        [HttpGet("saleReport/getCustomerVistAnalysis/{store}")]
        [SwaggerOperation("getCustomerVistAnalysis")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]

        public async Task<APIResult<CustomerVistAnalysisModel>> GetCustomerVistAnalysis(int? store, int? businessUserId, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<CustomerVistAnalysisModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new CustomerVistAnalysisModel();
                    var data = _saleReportService.GetCustomerVistAnalysis(store, businessUserId, force: force);

                    if (result != null)
                    {
                        //业务员
                        result.BusinessUserId = data.BusinessUserId;
                        result.BusinessUserName = data.BusinessUserName;
                        //result.TotalVist = data.TotalVist;
                        result.TotalCustomer = data.TotalCustomer;
                        result.Today = new CustomerVistAnalysisModel.Vist { VistCount = data.Today.VistCount, Percentage = data.Today.Percentage };
                        //result.LastWeekSame = new CustomerVistAnalysis.Vist() { VistCount = data.SamePeriod.LastWeekVistCount, Percentage = data.SamePeriodLastWeekPercentage };
                        result.Yesterday = new CustomerVistAnalysisModel.Vist { VistCount = data.Yesterday.VistCount, Percentage = data.Yesterday.Percentage };
                        result.BeforeYesterday = new CustomerVistAnalysisModel.Vist { VistCount = data.BeforeYesterday.VistCount, Percentage = data.BeforeYesterday.Percentage };
                        result.LastWeek = new CustomerVistAnalysisModel.Vist { VistCount = data.LastWeek.VistCount, Percentage = data.LastWeek.Percentage };
                        result.ThisWeek = new CustomerVistAnalysisModel.Vist { VistCount = data.ThisWeek.VistCount, Percentage = data.ThisWeek.Percentage };
                        result.LastMonth = new CustomerVistAnalysisModel.Vist { VistCount = data.LastMonth.VistCount, Percentage = data.LastMonth.Percentage };
                        result.ThisMonth = new CustomerVistAnalysisModel.Vist { VistCount = data.ThisMonth.VistCount, Percentage = data.ThisMonth.Percentage };
                        result.ThisYear = new CustomerVistAnalysisModel.Vist { VistCount = data.ThisYear.VistCount, Percentage = data.ThisYear.Percentage };
                    }

                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<CustomerVistAnalysisModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 新增加客户分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <returns></returns>
        [HttpGet("saleReport/getNewCustomerAnalysis/{store}")]
        [SwaggerOperation("getNewCustomerAnalysis")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<NewCustomerAnalysisModel>> GetNewCustomerAnalysis(int? store, int? businessUserId, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<NewCustomerAnalysisModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {

                    var result = new NewCustomerAnalysisModel();

                    var ncaq = _saleReportService.GetNewUserAnalysis(store, businessUserId, force: force);
                    var terms = _terminalService.GetTerminalsAnalysisByCreate(store, businessUserId ?? 0, DateTime.Now.AddMonths(-1));

                    if (ncaq != null)
                    {
                        //业务员
                        result.BusinessUserId = ncaq.BusinessUserId;
                        result.BusinessUserName = ncaq.BusinessUserName;
                        result.TotalCustomer = ncaq.TotalCustomer;


                        result.Today = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.Today.Count,
                            TerminalIds = ncaq.Today.TerminalIds
                        };


                        result.Yesterday = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.Yesterday.Count,
                            TerminalIds = ncaq.Yesterday.TerminalIds
                        };

                        result.BeforeYesterday = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.BeforeYesterday.Count,
                            TerminalIds = ncaq.BeforeYesterday.TerminalIds
                        };

                        result.LastWeek = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.LastWeek.Count,
                            TerminalIds = ncaq.LastWeek.TerminalIds
                        };

                        result.ThisWeek = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.ThisWeek.Count,
                            TerminalIds = ncaq.ThisWeek.TerminalIds
                        };

                        result.LastMonth = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.LastMonth.Count,
                            TerminalIds = ncaq.LastMonth.TerminalIds
                        };

                        result.ThisMonth = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.ThisMonth.Count,
                            TerminalIds = ncaq.ThisMonth.TerminalIds
                        };

                        result.ThisYear = new NewCustomerAnalysisModel.Signin
                        {
                            Count = ncaq.ThisYear.Count,
                            TerminalIds = ncaq.ThisYear.TerminalIds
                        };


                        if (terms != null)
                        {
                            var datas = new Dictionary<string, double>();
                            foreach (IGrouping<DateTime, Terminal> group in terms)
                            {
                                if (datas.Any(x => x.Key == group.Key.ToString("MM/dd")))
                                {
                                    var vm = datas.FirstOrDefault(x => x.Key == group.Key.ToString("MM/dd"));
                                    datas.Remove(group.Key.ToString("MM/dd"));
                                    datas.Add(group.Key.ToString("MM/dd"), group.ToList().Count + vm.Value);
                                }
                                else
                                {
                                    datas.Add(group.Key.ToString("MM/dd"), group.ToList().Count);
                                }
                            }

                            result.ChartDatas = datas;
                        }

                        //这里只是为了在空数据下填充Chart使用：没有任何意义
                        if (!ncaq.ChartDatas.Any() && result.ChartDatas.Count == 0)
                        {
                            var ran = new Random();
                            var datas = new Dictionary<string, double>();
                            var start = DateTime.Now.AddDays(-10);
                            while (start.Day < DateTime.Now.Day)
                            {
                                start = start.AddDays(1);
                                datas.Add(start.ToString("MM/dd"), ran.Next(5, 50));
                            }
                            result.ChartDatas = datas;
                        }
                    }

                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<NewCustomerAnalysisModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 订单额分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌</param>
        /// <param name="productId">商品</param>
        /// <param name="catagoryId">商品类别</param>
        /// <returns></returns>
        [HttpGet("saleReport/getOrderQuantityAnalysis/{store}")]
        [SwaggerOperation("getOrderQuantityAnalysis")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<OrderQuantityAnalysisQuery>> GetOrderQuantityAnalysis(int? store, int? businessUserId, int? brandId, int? productId, int? catagoryId, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<OrderQuantityAnalysisQuery>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = _saleReportService.GetOrderQuantityAnalysis(store, businessUserId, brandId, productId, catagoryId, force: force);
                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<OrderQuantityAnalysisQuery>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取客户排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId">客户</param>
        /// <param name="districtId">片区</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet("saleReport/getCustomerRanking/{store}")]
        [SwaggerOperation("getCustomerRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CustomerRanking>>> GetCustomerRanking(int? store, int? terminalId, int? districtId, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<CustomerRanking>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var model = new List<CustomerRanking>();

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-01 00:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    model = _saleReportService.GetCustomerRanking(store, terminalId, districtId, businessUserId, startTime, endTime, force: force).ToList();
                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<CustomerRanking>(ex.Message);
                }


            });
        }


        /// <summary>
        /// 获取经销商滞销排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌</param>
        /// <param name="categoryId">类别</param>
        /// <param name="startTime">开始时间:yyyy-MM-dd 00:00:00</param>
        /// <param name="endTime">结束时间:yyyy-MM-dd 00:00:00</param>
        /// <returns></returns>
        [HttpGet("saleReport/getUnSaleRanking/{store}")]
        [SwaggerOperation("getUnSaleRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<UnSaleRanking>>> GetUnSaleRanking(int? store, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<UnSaleRanking>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<UnSaleRanking>();

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 01:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    model = _saleReportService.GetUnSaleRanking(store, businessUserId, brandId, categoryId, startTime, endTime, force: force).ToList();

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<UnSaleRanking>(ex.Message);
                }
            });
        }



        /// <summary>
        /// 获取经销商热定(销订)排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId">客户</param>
        /// <param name="businessUserId">业务员</param>
        /// <param name="brandId">品牌</param>
        /// <param name="categoryId">类别</param>
        /// <param name="startTime">开始时间:yyyy-MM-dd 00:00:00</param>
        /// <param name="endTime">结束时间:yyyy-MM-dd 00:00:00</param>
        /// <returns></returns>
        [HttpGet("saleReport/getHotOrderRanking/{store}")]
        [SwaggerOperation("getHotOrderRanking")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<HotSaleRankingModel>>> GetHotOrderRanking(int? store, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<HotSaleRankingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<HotSaleRankingModel>();

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 01:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    model = _saleReportService.GetHotOrderRanking(store, terminalId, businessUserId, brandId, categoryId, startTime, endTime, force: force).Select(s => s.ToModel<HotSaleRankingModel>()).ToList();

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<HotSaleRankingModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 新增订单分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId">业务员</param>
        /// <returns></returns>
        [HttpGet("saleReport/getNewOrderAnalysis/{store}")]
        [SwaggerOperation("getNewOrderAnalysis")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<NewOrderAnalysisModel>> GetNewOrderAnalysis(int? store, int? businessUserId, bool force = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<NewOrderAnalysisModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {

                try
                {
                    var newOrderAnalysis = new NewOrderAnalysisModel();
                    var ncaq = _saleReportService.GetNewOrderAnalysis(store, businessUserId, force: force);
                    var terms = _saleReservationBillService.GetSaleReservationBillsAnalysisByCreate(store, businessUserId ?? 0, DateTime.Now.AddMonths(-1));

                    if (ncaq != null)
                    {
                        //业务员
                        newOrderAnalysis.BusinessUserId = ncaq.BusinessUserId;
                        newOrderAnalysis.BusinessUserName = ncaq.BusinessUserName;
                        newOrderAnalysis.TotalOrders = ncaq.TotalOrders;


                        newOrderAnalysis.Today = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.Today.OrderCount,
                            BillIds = ncaq.Today.BillIds
                        };

                        newOrderAnalysis.Yesterday = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.Yesterday.OrderCount,
                            BillIds = ncaq.Yesterday.BillIds
                        };

                        newOrderAnalysis.BeforeYesterday = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.BeforeYesterday.OrderCount,
                            BillIds = ncaq.BeforeYesterday.BillIds
                        };

                        newOrderAnalysis.LastWeek = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.LastWeek.OrderCount,
                            BillIds = ncaq.LastWeek.BillIds
                        };

                        newOrderAnalysis.ThisWeek = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.ThisWeek.OrderCount,
                            BillIds = ncaq.ThisWeek.BillIds
                        };

                        newOrderAnalysis.LastMonth = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.LastMonth.OrderCount,
                            BillIds = ncaq.LastMonth.BillIds
                        };

                        newOrderAnalysis.ThisMonth = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.ThisMonth.OrderCount,
                            BillIds = ncaq.ThisMonth.BillIds
                        };

                        newOrderAnalysis.ThisYear = new NewOrderAnalysisModel.Order
                        {
                            Count = ncaq.ThisYear.OrderCount,
                            BillIds = ncaq.ThisYear.BillIds
                        };

                        if (terms != null)
                        {
                            var datas = new Dictionary<string, double>();
                            foreach (IGrouping<DateTime, SaleReservationBill> group in terms)
                            {
                                if (datas.Any(x => x.Key == group.Key.ToString("MM/dd")))
                                {
                                    var vm = datas.FirstOrDefault(x => x.Key == group.Key.ToString("MM/dd"));
                                    datas.Remove(group.Key.ToString("MM/dd"));
                                    datas.Add(group.Key.ToString("MM/dd"), group.ToList().Count + vm.Value);
                                }
                                else
                                {
                                    datas.Add(group.Key.ToString("MM/dd"), group.ToList().Count);
                                }
                            }

                            newOrderAnalysis.ChartDatas = datas;
                        }

                        //这里只是为了在空数据下填充Chart使用：没有任何意义
                        if (!ncaq.ChartDatas.Any() && newOrderAnalysis.ChartDatas.Count == 0)
                        {
                            var ran = new Random();
                            var datas = new Dictionary<string, double>();
                            var start = DateTime.Now.AddDays(-10);
                            while (start.Day < DateTime.Now.Day)
                            {
                                start = start.AddDays(1);
                                datas.Add(start.ToString("MM/dd"), ran.Next(5, 50));
                            }
                            newOrderAnalysis.ChartDatas = datas;
                        }
                    }

                    return this.Successful3("", newOrderAnalysis);
                }
                catch (Exception ex)
                {
                    return this.Error3<NewOrderAnalysisModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 业务员综合分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="type"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        [HttpGet("saleReport/getBusinessAnalysis/{store}")]
        [SwaggerOperation("getBusinessAnalysis")]
        public async Task<APIResult<BusinessAnalysis>> GetBusinessAnalysis(int? store, int type, bool force = false,int userId=0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<BusinessAnalysis>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var analysis = _saleReportService.GetBusinessAnalysis(type, store,userId: userId);
                    return this.Successful3("", analysis);
                }
                catch (Exception ex)
                {
                    return this.Error3<BusinessAnalysis>(ex.Message);
                }
            });
        }



        /// <summary>
        /// 销售汇总（按客户/商品）
        /// </summary>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("saleReport/getSaleReportSummaryCustomerProduct/{store}")]
        [SwaggerOperation("getSaleReportSummaryCustomerProduct")]
        public async Task<APIResult<IList<SaleReportSummaryCustomerProduct>>> GetSaleReportSummaryCustomerProduct(int? store, int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? rankId, int? businessUserId, int? deliveryUserId, int? terminalId, string terminalName, string remark, DateTime? startTime, DateTime? endTime, int pagenumber = 0, bool? auditedStatus = true)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<SaleReportSummaryCustomerProduct>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 01:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    var items = _saleReportService.GetSaleReportSummaryCustomerProduct(store ?? 0,
                        wareHouseId, 
                        productId, 
                        productName, 
                        categoryId, 
                        brandId,
                        channelId, 
                        rankId, 
                        businessUserId,
                        deliveryUserId,
                        terminalId, 
                        terminalName, 
                        remark,
                        startTime,
                        endTime,
                        pageIndex: pagenumber,
                        pageSize: 100,
                        auditedStatus: auditedStatus);

                    return this.Successful2("", items?.ToList());
                }
                catch (Exception ex)
                {
                    return this.Error2<SaleReportSummaryCustomerProduct>(ex.Message);
                }
            });

        }

        /// <summary>
        /// 销售明细表
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="payTypeId">支付方式Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("saleReport/getSaleReportItem/{store}")]
        [SwaggerOperation("getSaleReportItem")]
        public async Task<APIResult<IList<SaleReportItem>>> GetSaleReportItem(int? store, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? businessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? costContractProduct, int? districtId, int pagenumber = 0, bool? auditedStatus = true)
        {
            if (pagenumber > 0)
                if (!store.HasValue || store.Value == 0)
                    return this.Error2<SaleReportItem>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    if (pagenumber > 0)
                    {
                        pagenumber -= 1;
                    }

                    if (startTime != null)
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));

                    if (endTime != null)
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));

                    var items = _saleReportService.GetSaleReportItem(store ?? 0,
                        productId,
                        productName,
                        categoryId,
                        brandId,
                        terminalId,
                        terminalName,
                        billNumber,
                        saleTypeId,
                        businessUserId,
                        wareHouseId,
                        payTypeId,
                        deliveryUserId,
                        rankId,
                        remark,
                        channelId,
                        startTime,
                        endTime,
                        costContractProduct ?? false,
                        districtId,
                        pageIndex: pagenumber,
                        pageSize: 30,
                        auditedStatus: auditedStatus);

                    return this.Successful2("", items?.ToList());
                }
                catch (Exception ex)
                {
                    return this.Error2<SaleReportItem>(ex.Message);
                }
            });
        }
    }
}