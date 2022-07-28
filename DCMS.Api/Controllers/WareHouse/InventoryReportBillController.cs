using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
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
    /// 用于门店库存上报
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/warehouse")]
    public class InventoryReportBillController : BaseAPIController
    {

        private readonly IUserService _userService;
        private readonly IInventoryReportBillService _inventoryReportBillService;
        private readonly IStockReportService _stockReportService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        
        private readonly static object _MyLock = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="inventoryReportBillService"></param>
        /// <param name="stockReportService"></param>
        /// <param name="productService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name=""></param>
        /// <param name="logger"></param>
        public InventoryReportBillController(
            IUserService userService,
            IInventoryReportBillService inventoryReportBillService,
            IStockReportService stockReportService,
            IProductService productService,
           
            ISpecificationAttributeService specificationAttributeService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _inventoryReportBillService = inventoryReportBillService;
            _stockReportService = stockReportService;
            _productService = productService;
            
            _specificationAttributeService = specificationAttributeService;
        }

        /// <summary>
        /// 获取门店库存上报汇总
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("inventoryreportsummary/getbills/{store}/{businessUserId}/{terminalId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]

        public async Task<APIResult<IList<InventoryReportList>>> AsyncList(int? store, int? makeuserId, int? businessUserId, int? terminalId, int? channelId, int? rankId, int? districtId, int? productId, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<InventoryReportList>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var results = new List<InventoryReportList>();
                try
                {
                    results = _stockReportService.GetInventoryReportListApi(store,
                    makeuserId,
                    businessUserId,
                    terminalId,
                    channelId,
                    rankId,
                    districtId,
                    productId,
                    startTime,
                    endTime,
                    pageIndex,
                    pageSize).ToList();

                    return this.Successful2(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error2<InventoryReportList>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取门店库存上报单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("inventoryreportbill/getInventoryReportBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getInventoryReportBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<InventoryReportBillModel>> GetInventoryReportBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<InventoryReportBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new InventoryReportBillModel();
                try
                {
                    var inventoryReportBill = _inventoryReportBillService.GetInventoryReportBillById(store, billId ?? 0);
                    if (inventoryReportBill != null)
                    {
                        model = inventoryReportBill.ToModel<InventoryReportBillModel>();

                        if (inventoryReportBill.Items != null && inventoryReportBill.Items.Count > 0)
                        {
                            var allProducts = _productService.GetProductsByIds(store ?? 0, inventoryReportBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                            model.Items = inventoryReportBill.Items.Select(si =>
                            {
                                var im = si.ToModel<InventoryReportItemModel>();
                                var product = allProducts.Where(ap => ap.Id == im.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    im.ProductName = product.Name;
                                    im.BigUnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, product.BigUnitId ?? 0);
                                    im.SmallUnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, product.SmallUnitId);
                                }
                                return im;
                            }).ToList();
                        }
                    }

                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<InventoryReportBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 创建门店库存上报单/更新门店库存上报单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("inventoryreportbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(InventoryReportBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(() =>
            {
                User user = null;
                try
                {
                    user = _userService.GetUserById(store ?? 0, userId ?? 0);
                    string errMsg = string.Empty;
                    var results = new BaseResult() { Success = true, Message = "提交成功" };

                    if (data != null)
                    {
                        lock (_MyLock)
                        {
                            //保存事务
                            var inventoryReportBill = new InventoryReportBill();

                            #region 添加门店库存上报单

                            inventoryReportBill.StoreId = store ?? 0;
                            inventoryReportBill.BillNumber =
                            CommonHelper.GetBillNumber(
                                CommonHelper.GetEnumDescription(
                                    BillTypeEnum.InventoryReportBill).Split(',')[1], store ?? 0);
                            inventoryReportBill.TerminalId = data.TerminalId;
                            inventoryReportBill.BusinessUserId = data.BusinessUserId;
                            inventoryReportBill.ReversedUserId = null;
                            inventoryReportBill.ReversedStatus = false;
                            inventoryReportBill.ReversedDate = null;
                            inventoryReportBill.CreatedOnUtc = DateTime.Now;
                            inventoryReportBill.Remark = data.Remark;

                            inventoryReportBill.Operation = 1;//标识操作源

                            _inventoryReportBillService.InsertInventoryReportBill(inventoryReportBill);
                            //主表Id
                            billId = inventoryReportBill.Id;

                            #endregion

                            #region 添加 上报关联商品、商品关联库存量

                            if (data.Items != null && data.Items.Count > 0)
                            {
                                data.Items.ForEach(p =>
                                {
                                    InventoryReportItem inventoryReportItem = new InventoryReportItem
                                    {
                                        StoreId = store ?? 0,
                                        InventoryReportBillId = billId ?? 0,
                                        ProductId = p.ProductId,
                                        BigUnitId = p.BigUnitId,
                                        BigQuantity = p.BigQuantity,
                                        SmallUnitId = p.SmallUnitId,
                                        SmallQuantity = p.SmallQuantity
                                    };

                                    _inventoryReportBillService.InsertInventoryReportItem(inventoryReportItem);

                                    if (p.InventoryReportStoreQuantities != null && p.InventoryReportStoreQuantities.Count > 0)
                                    {
                                        p.InventoryReportStoreQuantities.ToList().ForEach(q =>
                                        {
                                            InventoryReportStoreQuantity inventoryReportStoreQuantity = new InventoryReportStoreQuantity
                                            {
                                                StoreId = q.StoreId,
                                                InventoryReportItemId = inventoryReportItem.Id,
                                                BigStoreQuantity = q.BigStoreQuantity,
                                                SmallStoreQuantity = q.SmallStoreQuantity,
                                                ManufactureDete = (q.ManufactureDete == null || q.ManufactureDete == DateTime.MinValue) ? DateTime.Now : q.ManufactureDete.Value
                                            };

                                            _inventoryReportBillService.InsertInventoryReportStoreQuantity(inventoryReportStoreQuantity);
                                        });
                                    }
                                });
                            }

                            #endregion

                            #region 添加修改上报汇总表
                            if (data.Items != null && data.Items.Count > 0)
                            {
                                var allProducts = _productService.GetProductsByIds(store ?? 0, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                                data.Items.ForEach(it =>
                                {
                                    //根据 经销商、客户、商品 获取库存汇总记录
                                    InventoryReportSummary inventoryReportSummary = _inventoryReportBillService.GetInventoryReportSummaryByTerminalIdProductId(store ?? 0, inventoryReportBill.TerminalId, it.ProductId);

                                    //insert
                                    if (inventoryReportSummary == null)
                                    {
                                        inventoryReportSummary = new InventoryReportSummary
                                        {
                                            StoreId = store ?? 0,
                                            TerminalId = data.TerminalId,
                                            BusinessUserId = data.BusinessUserId,
                                            ProductId = it.ProductId,
                                            BeginDate = DateTime.Now,
                                            EndDate = null,
                                            //期末库存 = 0
                                            EndStoreQuantity = 0
                                        };
                                        var product = allProducts.Where(ap => ap.Id == it.ProductId).FirstOrDefault();

                                        //采购量
                                        inventoryReportSummary.PurchaseQuantity = 0;
                                        if (product != null)
                                        {
                                            int sumPurchaseQuantity = 0;
                                            //采购大单位数量
                                            sumPurchaseQuantity += (product.BigQuantity ?? 0) * it.BigQuantity;
                                            //采购小单位数量
                                            sumPurchaseQuantity += it.SmallQuantity;
                                            inventoryReportSummary.PurchaseQuantity = sumPurchaseQuantity;
                                        }
                                        //期初库存量 = 采购量
                                        inventoryReportSummary.BeginStoreQuantity = inventoryReportSummary.PurchaseQuantity;
                                        //销售量 = 0
                                        inventoryReportSummary.SaleQuantity = 0;

                                        _inventoryReportBillService.InsertInventoryReportSummary(inventoryReportSummary);
                                    }
                                    //update
                                    else
                                    {
                                        //期初库存不变
                                        //期初时间不变
                                        //以前采购量（上次）
                                        int OldPurchaseQuantity = inventoryReportSummary.PurchaseQuantity;

                                        //期末时间
                                        inventoryReportSummary.EndDate = DateTime.Now;
                                        //期末库存量
                                        var product = allProducts.Where(ap => ap.Id == it.ProductId).FirstOrDefault();
                                        inventoryReportSummary.EndStoreQuantity = 0;
                                        if (product != null && it.InventoryReportStoreQuantities != null && it.InventoryReportStoreQuantities.Count > 0)
                                        {
                                            int sumEndStoreQuantity = 0;
                                            it.InventoryReportStoreQuantities.ToList().ForEach(iq =>
                                            {
                                                //大单位库存量
                                                sumEndStoreQuantity += (product.BigQuantity ?? 0) * iq.BigStoreQuantity;
                                                //小单位库存量
                                                sumEndStoreQuantity += iq.SmallStoreQuantity;
                                            });
                                            inventoryReportSummary.EndStoreQuantity = sumEndStoreQuantity;
                                        }
                                        //采购量
                                        inventoryReportSummary.PurchaseQuantity = 0;
                                        if (product != null)
                                        {
                                            int sumPurchaseQuantity = 0;
                                            //采购大单位数量
                                            sumPurchaseQuantity += (product.BigQuantity ?? 0) * it.BigQuantity;
                                            //采购小单位数量
                                            sumPurchaseQuantity += it.SmallQuantity;
                                            inventoryReportSummary.PurchaseQuantity = sumPurchaseQuantity;
                                        }
                                        //采购量累加
                                        inventoryReportSummary.PurchaseQuantity += OldPurchaseQuantity;

                                        //销售量 = 采购量 - 期末库存
                                        inventoryReportSummary.SaleQuantity = inventoryReportSummary.PurchaseQuantity - inventoryReportSummary.EndStoreQuantity;

                                        _inventoryReportBillService.UpdateInventoryReportSummary(inventoryReportSummary);
                                    }

                                });
                            }
                            #endregion
                        }

                        return results.To(results);
                    }
                    else
                    {
                        return results.To(results);
                    }
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }
            });
        }
    }
}