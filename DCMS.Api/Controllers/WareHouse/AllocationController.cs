using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;
using DCMS.ViewModel.Models.Products;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于调拨单管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/warehouse")]
    public class AllocationController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IAllocationBillService _allocationBillService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        
        private readonly IRedLocker _locker;
        private readonly ISettingService _settingService;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userActivityService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="allocationBillService"></param>
        /// <param name="productService"></param>
        /// <param name="stockService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        public AllocationController(
            IUserActivityService userActivityService,
            IWareHouseService wareHouseService,
            IAllocationBillService allocationBillService,
            IProductService productService,
            IStockService stockService,
            ISpecificationAttributeService specificationAttributeService,
           
            IUserService userService,
            IRedLocker locker,
            ISettingService settingService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _userActivityService = userActivityService;
            _wareHouseService = wareHouseService;
            _allocationBillService = allocationBillService;
            _productService = productService;
            _stockService = stockService;
            
            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _settingService = settingService;
        }

        /// <summary>
        /// 调拨单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="shipmentWareHouseId"></param>
        /// <param name="incomeWareHouseId"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="remark"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("allocationbill/getbills/{store}/{businessUserId}/{makeuserId}/{shipmentWareHouseId}/{incomeWareHouseId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<AllocationBillModel>>> AsyncList(int? store, int? makeuserId, int businessUserId, int? shipmentWareHouseId, int? incomeWareHouseId, string billNumber, string remark, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<AllocationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var bills = _allocationBillService.GetAllAllocationBills(store ?? 0,
                        makeuserId ?? 0,
                        businessUserId,
                        shipmentWareHouseId,
                        incomeWareHouseId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        null,
                        null,
                        pagenumber,
                        pageSize);

                    #region 查询需要关联其他表的数据
                    List<int> warehouseIds = new List<int>();
                    warehouseIds.AddRange(bills.Select(b => b.ShipmentWareHouseId).Distinct().ToArray());
                    warehouseIds.AddRange(bills.Select(b => b.IncomeWareHouseId).Distinct().ToArray());
                    var allWarehouses = _wareHouseService.GetWareHouseByIds(store ?? 0, warehouseIds.Distinct().ToArray());

                    #endregion

                    var productIds = new List<int>();
                    bills?.ToList().ForEach(s =>
                    {
                        var pids = s?.Items?.Select(s => s.ProductId).ToList();
                        productIds.AddRange(pids);
                    });

                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

                    var userIds = bills.Select(b => b.MakeUserId).Distinct().ToArray();
                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, userIds.Distinct().ToArray());

                    //商品价格
                    var allProductPrices = _productService.GetProductPricesByProductIds(store ?? 0, productIds.Distinct
().ToArray());
                    var results = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<AllocationBillModel>();

                        var warehouse = allWarehouses.Where(aw => aw.Id == b.ShipmentWareHouseId).FirstOrDefault();
                        m.ShipmentWareHouseName = warehouse == null ? "" : warehouse.Name;

                        var warehouse2 = allWarehouses.Where(aw => aw.Id == b.IncomeWareHouseId).FirstOrDefault();
                        m.IncomeWareHouseName = warehouse2 == null ? "" : warehouse2.Name;

                        m.MakeUserName = allUsers.Where(au => au.Key == b.MakeUserId).Select(au => au.Value).FirstOrDefault();

                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            var option = p.GetProductUnit(allOptions, allProductPrices);

                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                            item.smallOption = option.smallOption.ToModel<SpecificationAttributeOptionModel>();
                            item.strokeOption = option.strokOption.ToModel<SpecificationAttributeOptionModel>();
                            item.bigOption = option.bigOption.ToModel<SpecificationAttributeOptionModel>();
                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                            item.BigUnitId = p?.BigUnitId;
                            item.SmallUnitId = p?.SmallUnitId ?? 0;
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);

                }
                catch (Exception ex)
                {
                    return this.Error<AllocationBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取调拨单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("allocationbill/getAllocationBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getAllocationBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<AllocationBillModel>> GetAllocationBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var model = new AllocationBillModel();

                try
                {
                    //获取配置
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var allocationBill = _allocationBillService.GetAllocationBillById(store ?? 0, billId ?? 0, true);

                    model = allocationBill.ToModel<AllocationBillModel>();


                    var allProductPrices = _productService.GetProductPricesByProductIds(store, allocationBill.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store ?? 0, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, allocationBill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    model.Items = allocationBill.Items.Select(o =>
                    {
                        var m = o.ToModel<AllocationItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<AllocationItemModel>(m, o.AllocationBill.ShipmentWareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            m.UnitId = o.UnitId;
                            m.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, o.UnitId);
                        }
                        return m;
                    }).ToList();

                    var ws = BindWareHouseSelection(_wareHouseService.BindWareHouseList, store ?? 0, WHAEnum.AllocationBill, user);

                    model.ShipmentWareHouses = ws;
                    model.ShipmentWareHouseName = _wareHouseService.GetWareHouseName(store, model.ShipmentWareHouseId);

                    model.IncomeWareHouses = ws;
                    model.IncomeWareHouseName = _wareHouseService.GetWareHouseName(store, model.IncomeWareHouseId);

                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, allocationBill.MakeUserId);

                    model.MakeUserName = mu + " " + allocationBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store ?? 0, allocationBill.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (allocationBill.AuditedDate.HasValue ? allocationBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //显示订单占用库存
                    model.APPShowOrderStock = companySetting.APPShowOrderStock;

                    return this.Successful3(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<AllocationBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("allocationbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(AllocationUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Warning(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new AllocationBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _allocationBillService.GetAllocationBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;

                    }
                    #endregion

                    #region 验证盘点

                    if (data != null && data.Items != null && data.Items.Count > 0)
                    {
                        if (_wareHouseService.CheckProductInventory(store ?? 0, data.IncomeWareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                            return this.Warning(thisMsg);
                    }

                    #endregion

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();

                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (AllocationItemModel item in data.Items)
                        {
                            if (item.ProductId != 0)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                    //商品转化量
                                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                    //库存量增量 = 单位转化量 * 数量
                                    int thisQuantity = item.Quantity * conversionQuantity;
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            //当期选择单位
                                            UnitId = product.SmallUnitId,
                                            //转化单位
                                            SmallUnitId = product.SmallUnitId,
                                            //转化单位
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        productStockItemNews.Add(productStockItem);
                                    }
                                }
                            }

                        }
                    }

                    //验证库存
                    string thisMsg2 = string.Empty;
                    var fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, data.ShipmentWareHouseId, productStockItemNews, out thisMsg2);
                    if (fg == false)
                    {
                        return this.Warning(thisMsg2);
                    }

                    #endregion

                    //业务逻辑
                    //var tonketId = "";
                    //
                    var dataTo = data.ToEntity<AllocationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    dataTo.Items = data.Items.OrderBy(it => it.SortIndex).Select(it =>
                    {
                        return it.ToEntity<AllocationItem>();
                    }).ToList();

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                         TimeSpan.FromSeconds(30),
                         TimeSpan.FromSeconds(10),
                         TimeSpan.FromSeconds(1),
                         () => _allocationBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo, dataTo.Items, productStockItemNews, _userService.IsAdmin(store ?? 0, userId ?? 0)));

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    //活动日志
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId ?? 0);

                    return this.Error(ex.Message);

                }

            });
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("allocationbill/auditing/{store}/{userId}/{billId}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                var result = new APIResult<dynamic>();
                try
                {
                    var bill = new AllocationBill();

                    #region 验证

                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _allocationBillService.GetAllocationBillById(store ?? 0, billId.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();

                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (bill.Items != null && bill.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (AllocationItem item in bill.Items)
                        {
                            if (item.ProductId != 0)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                    //商品转化量
                                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                    //库存量增量 = 单位转化量 * 数量
                                    int thisQuantity = item.Quantity * conversionQuantity;
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            //当期选择单位
                                            UnitId = product.SmallUnitId,
                                            //转化单位
                                            SmallUnitId = product.SmallUnitId,
                                            //转化单位
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        productStockItemNews.Add(productStockItem);
                                    }
                                }
                            }

                        }
                    }

                    //验证库存
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, bill.ShipmentWareHouseId, productStockItemNews, out string errMsg))
                    {
                        return this.Warning(errMsg);
                    }

                    #endregion

                    #endregion

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _allocationBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "单据审核失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpGet("allocationbill/reverse/{store}/{userId}/{billId}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? billId, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var bill = new AllocationBill() { StoreId = store ?? 0 };


                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _allocationBillService.GetAllocationBillById(store ?? 0, billId.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;


                    #region 验证库存
                    //将一个单据中 相同商品 数量 按最小单位汇总
                    List<ProductStockItem> productStockItems = new List<ProductStockItem>();

                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());
                    foreach (AllocationItem item in bill.Items)
                    {
                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                        ProductStockItem productStockItem = productStockItems.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                        //商品转化量
                        var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                        //库存量增量 = 单位转化量 * 数量
                        int thisQuantity = item.Quantity * conversionQuantity;
                        if (productStockItem != null)
                        {
                            productStockItem.Quantity += thisQuantity;
                        }
                        else
                        {
                            productStockItem = new ProductStockItem
                            {
                                ProductId = item.ProductId,
                                //当期选择单位
                                UnitId = product.SmallUnitId,
                                //转化单位
                                SmallUnitId = product.SmallUnitId,
                                //转化单位
                                BigUnitId = product.BigUnitId ?? 0,
                                ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                Quantity = thisQuantity
                            };

                            productStockItems.Add(productStockItem);
                        }
                    }
                    //验证当前商品库存
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, bill.StoreId, bill.IncomeWareHouseId, productStockItems, out string thisMsg2))
                    {
                        return this.Warning(thisMsg2);
                    }
                    #endregion
                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _allocationBillService.Reverse(userId ?? 0, bill));

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }


    }
}