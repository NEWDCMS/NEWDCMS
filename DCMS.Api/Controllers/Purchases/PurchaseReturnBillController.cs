using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Purchases;
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
    /// 采购退货单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/purchases")]
    public class PurchaseReturnBillController : BaseAPIController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IPurchaseReturnBillService _purchaseReturnBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        
        private readonly IRedLocker _locker;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IPaymentReceiptBillService _paymentReceiptBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userActivityService"></param>
        /// <param name="purchaseReturnBillService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productService"></param>
        /// <param name="stockService"></param>
        /// <param name="settingService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="userService"></param>
        /// <param name="accountingService"></param>
        /// <param name="locker"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="paymentReceiptBillService"></param>
        /// <param name="logger"></param>
        public PurchaseReturnBillController(
            IUserActivityService userActivityService,
            IPurchaseReturnBillService purchaseReturnBillService,
            IManufacturerService manufacturerService,
            IWareHouseService wareHouseService,
            IProductService productService,
            IStockService stockService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            IAccountingService accountingService,
            IRedLocker locker,
            IPurchaseBillService purchaseBillService,
            IPaymentReceiptBillService paymentReceiptBillService, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userActivityService = userActivityService;
            _purchaseReturnBillService = purchaseReturnBillService;
            _manufacturerService = manufacturerService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _settingService = settingService;
            _userService = userService;
            _accountingService = accountingService;
            _locker = locker;
            _purchaseBillService = purchaseBillService;
            _paymentReceiptBillService = paymentReceiptBillService;
        }

        /// <summary>
        /// 获取采购退货单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="makeuserId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="printStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("purchasereturns/getbills/{store}/{businessUserId}/{manufacturerId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PurchaseReturnBillModel>>> AsyncList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId, string billNumber, string remark, bool? printStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var model = new PurchaseReturnBillListModel();
                try
                {
                    var purchaseReturns = _purchaseReturnBillService.GetPurchaseReturnBillList(store != null ? store : 0,
                        makeuserId,
                       businessUserId,
                       manufacturerId,
                       wareHouseId,
                       billNumber,
                       printStatus,
                       startTime,
                       endTime,
                       auditedStatus,
                       null,
                       remark,
                       sortByAuditedTime,
                       showReverse,
                       null,
                       false,//未删除单据
                       pagenumber,
                       pageSize);

                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PurchaseReturnBill);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, purchaseReturns.Select(b => b.BusinessUserId).Distinct().ToArray());
                    var allManufacturer = _manufacturerService.GetManufacturersByIds(store ?? 0, purchaseReturns.Select(b => b.ManufacturerId).Distinct().ToArray());
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store ?? 0, purchaseReturns.Select(b => b.WareHouseId).Distinct().ToArray(), true);

                    #endregion

                    var productIds = new List<int>();

                    purchaseReturns?.ToList().ForEach(s =>
                    {
                        var pids = s?.Items?.Select(s => s.ProductId).ToList();
                        productIds.AddRange(pids);
                    });

                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray());

                    var results = purchaseReturns.Select(s =>
                    {
                        var m = s.ToModel<PurchaseReturnBillModel>();

                        //业务员名称
                        m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                        //供应商名称
                        var manufacturer = allManufacturer.Where(am => am.Id == m.ManufacturerId).FirstOrDefault();
                        m.ManufacturerName = manufacturer == null ? "" : manufacturer.Name;

                        //仓库名称
                        var warehouse = allWareHouses.Where(aw => aw.Id == m.WareHouseId).FirstOrDefault();
                        m.WareHouseName = warehouse == null ? "" : warehouse.Name;

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.PurchaseReturnBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //优惠金额
                        m.PreferentialAmount = s.PreferentialAmount;

                        //付款账户
                        m.PurchaseReturnBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.PurchaseReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new PurchaseReturnBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<PurchaseReturnBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 采购退货单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("purchasereturnbill/getPurchaseReturnBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getPurchaseReturnBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<PurchaseReturnBillModel>> GetPurchaseReturnBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(() =>
            {
                var model = new PurchaseReturnBillModel();
                //获取公司配置
                var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                var bill = _purchaseReturnBillService.GetPurchaseReturnBillById(store ?? 0, billId.Value, true);
                if (bill != null)
                {
                    model = bill.ToModel<PurchaseReturnBillModel>();

                    //获取默认收款账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PurchaseReturnBill);
                    model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                    model.CollectionAmount = bill.PurchaseReturnBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
                    model.PurchaseReturnBillAccountings = bill.PurchaseReturnBillAccountings.Select(s =>
                    {
                        var m = s.ToAccountModel<PurchaseReturnBillAccountingModel>();
                        m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //取单据项目
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, bill.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                    model.Items = bill.Items.Select(o =>
                    {
                        var m = o.ToModel<PurchaseReturnItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<PurchaseReturnItemModel>(m, o.PurchaseReturnBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            m.UnitId = o.UnitId;
                            m.UnitName = o.UnitId == 0 ? m.Units.Keys.Select(k => k).ToArray()[2] : m.Units.Where(q => q.Value == o.UnitId).Select(q => q.Key)?.FirstOrDefault();
                        }
                        return m;
                    }).ToList();

                    #region 绑定数据源

                    model.BillTypeEnumId = (int)BillTypeEnum.PurchaseReturnBill;

                    //业务员
                    model.BusinessUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans,user,true, _userService.IsAdmin(store ?? 0, user));

                    //当前用户为业务员时默认绑定
                    model.BusinessUserId = model.BusinessUserId == 0 ? null : model.BusinessUserId;

                    //仓库
                    model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, store ?? 0, WHAEnum.PurchaseReturnBill, user);
                    model.WareHouseId = (model.WareHouseId);

                    //供应商
                    model.ManufacturerId = model.ManufacturerId;
                    model.ManufacturerName = _manufacturerService.GetManufacturerName(store ?? 0, model.ManufacturerId);

                    #endregion

                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, bill.MakeUserId);
                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //优惠后金额
                    model.PreferentialEndAmount = model.ReceivableAmount;
                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2 ? true : false;

                    //默认进价
                    model.DefaultPurchasePrice = companySetting.DefaultPurchasePrice;
                    //单据合计精度
                    model.AccuracyRounding = companySetting.AccuracyRounding;
                    //启用税务功能
                    model.EnableTaxRate = companySetting.EnableTaxRate;
                    model.TaxRate = companySetting.TaxRate;

                    return this.Successful(Resources.Successful, model);
                }
                else
                {
                    return this.Error<PurchaseReturnBillModel>(false, Resources.Failed);
                }

            });
        }

        /// <summary>
        /// 创建/编辑
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("purchasereturns/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(PurchaseReturnBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(async () =>
          {
              var result = new APIResult<dynamic>();
              try
              {
                  var bill = new PurchaseReturnBill();

                  #region 单据验证

                  if (data == null || data.Items == null)
                      return this.Warning("请录入数据.");

                  if (PeriodLocked(DateTime.Now, store ?? 0))
                      return this.Warning("锁账期间,禁止业务操作.");

                  if (PeriodClosed(DateTime.Now, store ?? 0))
                      return this.Warning("会计期间已结账,禁止业务操作.");

                  if (billId.HasValue && billId.Value != 0)
                  {
                      bill = _purchaseReturnBillService.GetPurchaseReturnBillById(store ?? 0, billId.Value, true);

                      //公共单据验证
                      var commonBillChecking = BillChecking<PurchaseReturnBill, PurchaseReturnItem>(bill, store ?? 0, BillStates.Draft);
                      if (commonBillChecking.Data != null)
                          return commonBillChecking;
                  }

                  var productids = _purchaseReturnBillService.GetProductPurchaseIds(store ?? 0, data.ManufacturerId);
                  foreach (PurchaseReturnItemModel item in data.Items)
                  {
                      if (productids.Contains(item.ProductId) == false && item.ProductId > 0)
                      {
                          return this.Warning("没有在该经销商采购商品！");
                      }
                  }
                  #endregion

                  #region 验证盘点

                  if (data.Items.Any())
                  {
                      if (_wareHouseService.CheckProductInventory(store ?? 0, data.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
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

                      foreach (PurchaseReturnItemModel item in data.Items)
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
                  if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, data.WareHouseId, productStockItemNews, out string errMsg))
                  {
                      return this.Warning(errMsg);
                  }

                  #endregion

                  #region 验证预付款
                  if (data.Accounting != null)
                  {
                      if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                      {
                          return this.Warning("预付款余额不足!");
                      }
                  }
                  #endregion


                  var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                  var dataTo = data.ToEntity<PurchaseReturnBillUpdate>();
                  dataTo.Operation = (int)OperationEnum.APP;
                  if (data.Accounting == null)
                  {
                      return this.Warning("没有默认的付款账号");
                  }
                  dataTo.Accounting = data.Accounting.Select(ac =>
                  {
                      return ac.ToAccountEntity<PurchaseReturnBillAccounting>();
                  }).ToList();
                  dataTo.Items = data.Items.Select(it =>
                  {
                      //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                      var item = it.ToEntity<PurchaseReturnItem>();
                      item.CostPrice = _purchaseBillService.GetReferenceCostPrice(store ?? 0, item.ProductId, item.UnitId);
                      item.CostAmount = item.CostPrice * item.Quantity;
                      return item;

                  }).ToList();

                  //RedLock
                  var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _purchaseReturnBillService.BillCreateOrUpdate(store ?? 0,
                        userId ?? 0,
                        billId, bill, dataTo.Accounting,
                        accountings,
                        dataTo,
                        dataTo.Items, productStockItemNews,
                        _userService.IsAdmin(store ?? 0, userId ?? 0)));

                  return results.To(results);

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
        [HttpGet("purchasereturns/auditing/{store}/{userId}/{id}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? id, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
            {
                return this.Error(Resources.ParameterError);
            }

            return await Task.Run(async () =>
            {
                var result = new APIResult<dynamic>();

                try
                {
                    var bill = new PurchaseReturnBill();

                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _purchaseReturnBillService.GetPurchaseReturnBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<PurchaseReturnBill, PurchaseReturnItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");

                    #region 验证库存

                    var allProducts = new List<Product>();

                    //当前数据
                    var productStockItemNews = new List<ProductStockItem>();
                    if (bill.Items != null && bill.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray()).ToList();
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (PurchaseReturnItem item in bill.Items)
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
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, bill.WareHouseId, productStockItemNews, out string errMsg))
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
                          () => _purchaseReturnBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful("单据审核成功", result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "单据审核失败", userId);
                    return this.Error("单据审核失败", ex.Message);

                }

            });
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("purchasereturns/reverse/{store}/{userId}/{id}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? id, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
            {
                return this.Error(Resources.ParameterError);
            }

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new PurchaseReturnBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲.");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _purchaseReturnBillService.GetPurchaseReturnBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<PurchaseReturnBill, PurchaseReturnItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;


                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作");

                    //验证是否付款
                    var paymentReceipt = _paymentReceiptBillService.CheckBillPaymentReceipt(store ?? 0, (int)BillTypeEnum.PurchaseReturnBill, bill.BillNumber);
                    if (paymentReceipt.Item1)
                    {
                        return this.Warning($"单据在付款单:{paymentReceipt.Item2}中已经付款.");
                    }

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _purchaseReturnBillService.Reverse(userId ?? 0, bill));

                    return Successful("单据红冲成功", result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取初始绑定
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("purchasereturns/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<PurchaseReturnBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PurchaseReturnBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new PurchaseReturnBillModel();

                    //默认账户设置
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PurchaseReturnBill);
                    model.PurchaseReturnBillAccountings.Add(new   PurchaseReturnBillAccountingModel()
                    {
                        AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                        CollectionAmount = 0,
                        Name = defaultAcc.Item1?.Name,
                        AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
                    });

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<PurchaseReturnBillModel>(ex.Message);
                }
            });
        }
    }
}