using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 订单转销售单
    /// </summary>
    public class ChangeReservationController : BasePublicController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly ISaleBillService _saleBillService;
        private readonly ISaleReservationBillService _saleReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IReturnReservationBillService _returnReservationService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IBillConvertService _billConvertService;
        private readonly IChangeReservationBillService _changeReservationBillService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStockService _stockService;
        private readonly IRedLocker _locker;

        public ChangeReservationController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IUserActivityService userActivityService,
            ISaleBillService saleBillService,
            ISaleReservationBillService saleReservationBillService,
            IReturnBillService returnBillService,
            IReturnReservationBillService returnReservationBillService,
            IWareHouseService wareHouseService,
            ISettingService settingService,
            IUserService userService,
            ITerminalService terminalService,
            IBillConvertService billConvertService,
            IChangeReservationBillService changeReservationBillService,
            INotificationService notificationService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IStockService stockService,
            IRedLocker locker) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userActivityService = userActivityService;
            _saleBillService = saleBillService;
            _saleReservationBillService = saleReservationBillService;
            _returnBillService = returnBillService;
            _returnReservationService = returnReservationBillService;
            _wareHouseService = wareHouseService;
            _settingService = settingService;
            _userService = userService;
            _terminalService = terminalService;
            _billConvertService = billConvertService;
            _changeReservationBillService = changeReservationBillService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _stockService = stockService;
            _locker = locker;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        [AuthCode((int)AccessGranularityEnum.OrderToSaleBillsView)]
        public IActionResult List()
        {
            var model = new ChangeReservationListModel
            {
                //业务员
                BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id)),
                //送货员
                DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers),
                StartTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")),
                EndTime = DateTime.Now.AddDays(1),
                BillType = (int)BillTypeEnum.SaleReservationBill
            };
            return View(model);
        }

        /// <summary>
        /// 异步获取订单列表
        /// </summary>
        /// <param name="billType"></param>
        /// <param name="businessUserId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="changedStatus"></param>
        /// <param name="dispatchedStatus"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncList(int billType, int? businessUserId, int? deliveryUserId, DateTime? startTime = null, DateTime? endTime = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pagenumber = 0, int pageSize = 30)
        {
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            return await Task.Run(() =>
            {
                var bills = new List<ChangeReservationModel>();

                #region 销售订单

                var saleReservations = _saleReservationBillService.GetSaleReservationBillToChangeList(curStore?.Id ?? 0,
                      curUser.Id,
                         startTime ?? DateTime.Now.AddDays(-7),
                         endTime ?? DateTime.Now,
                         businessUserId,
                         deliveryUserId,
                         billNumber,
                         remark,
                         changedStatus,
                         dispatchedStatus??false,
                         pagenumber,
                         pageSize: 30);


                var orders = saleReservations.Select(s =>
                {
                    var m = new ChangeReservationModel
                    {
                        BillType = (int)BillTypeEnum.SaleReservationBill
                    };

                    var sale = _saleBillService.GetSaleBillBySaleReservationBillId(curStore.Id, s.Id);
                    m.SaleBillId = sale?.Id ?? 0;
                    m.SaleBillNumber = sale?.BillNumber;

                    m.BillTypeName = CommonHelper.GetEnumDescription(m.BillTypeEnum);
                    m.BillLink = _billConvertService.GenerateBillUrl(m.BillType, s.Id);

                    m.Operation = s.Operation ?? 0;
                    m.BillId = s.Id;
                    m.BillNumber = s.BillNumber;
                    m.CreatedOnUtc = s.CreatedOnUtc;
                    m.BusinessUserId = s.BusinessUserId;
                    m.TerminalId = s.TerminalId;
                    m.SumAmount = s.SumAmount;
                    m.WareHouseId = s.WareHouseId;
                    m.DispatchedStatus = s.DispatchedStatus;

                    m.DeliveryUserId = sale?.DeliveryUserId ?? 0;
                    m.DeliveryUserName = _userService.GetUserName(curStore.Id, sale?.DeliveryUserId ?? 0);
                    m.Remark = s.Remark;

                    //业务员名称
                    m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId);

                    //客户名称
                    var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);

                    m.TerminalName = terminal == null ? "" : terminal.Name;
                    m.TerminalPointCode = terminal == null ? "" : terminal.Code;

                    //仓库名称
                    m.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, m.WareHouseId ?? 0);

                    //应收金额	
                    m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.SaleReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                    //优惠金额
                    m.PreferentialAmount = s.PreferentialAmount;

                    //收款账户
                    m.SaleReservationBillAccountings = s.SaleReservationBillAccountings.Select(sba =>
                {
                    return new SaleReservationBillAccountingModel()
                    {
                        Name = sba?.AccountingOption?.Name,
                        AccountingOptionId = sba?.Id ?? 0,
                        CollectionAmount = sba?.CollectionAmount ?? 0
                    };
                }).ToList();

                    //获取转单后单据的(现金)收款金额
                    m.CollectionAmount = sale?.SaleBillAccountings
                .Where(sa => sa.AccountingOption.AccountCodeTypeId == (int)AccountingCodeEnum.Cash)
                .Select(sa => sa.CollectionAmount)?.FirstOrDefault() ?? 0;

                    //欠款金额	
                    m.OweCash = sale?.OweCash ?? 0;

                    m.ChangedDate = s.ChangedDate;
                    m.ChangedStatus = s.ChangedStatus;
                    m.ChangedUserId = s.ChangedUserId;


                    return m;
                }).ToList();


                bills.AddRange(orders);

                #endregion

                #region 退货订单
                var returns = _returnReservationService.GetReturnReservationBillToChangeList(curStore?.Id ?? 0,
                         startTime ?? DateTime.Now.AddDays(-7),
                         endTime ?? DateTime.Now,
                         businessUserId,
                         deliveryUserId,
                         billNumber,
                         remark,
                         changedStatus,
                         dispatchedStatus??false,
                         pagenumber,
                         pageSize: 30);


                var returnOrders = returns.Select(s =>
                {
                    ChangeReservationModel m = new ChangeReservationModel
                    {
                        BillType = (int)BillTypeEnum.ReturnReservationBill
                    };

                    var r = _returnBillService.GetReturnBillByReturnReservationBillId(curStore.Id, s.Id);

                    m.ReturnBillId = r?.Id ?? 0;
                    m.ReturnBillNumber = r?.BillNumber;

                    m.BillTypeName = CommonHelper.GetEnumDescription(m.BillTypeEnum);
                    m.BillLink = _billConvertService.GenerateBillUrl(m.BillType, s.Id);

                    m.Operation = s.Operation ?? 0;
                    m.BillId = s.Id;
                    m.BillNumber = s.BillNumber;
                    m.CreatedOnUtc = s.CreatedOnUtc;
                    m.BusinessUserId = s.BusinessUserId;
                    m.TerminalId = s.TerminalId;
                    m.SumAmount = -s.SumAmount;
                    m.WareHouseId = s.WareHouseId;
                    m.DispatchedStatus = s.DispatchedStatus;


                    m.DeliveryUserId = r?.DeliveryUserId ?? 0;
                    m.DeliveryUserName = _userService.GetUserName(curStore.Id, r?.DeliveryUserId ?? 0);
                    m.Remark = s.Remark;

                    //业务员名称
                    m.BusinessUserName = _userService.GetUserName(curStore.Id, m.BusinessUserId);

                    //客户名称
                    var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                    m.TerminalName = terminal == null ? "" : terminal.Name;
                    m.TerminalPointCode = terminal == null ? "" : terminal.Code;

                    //仓库名称
                    m.WareHouseName = _wareHouseService.GetWareHouseName(curStore.Id, m.WareHouseId ?? 0);

                    //应收金额	
                    m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.ReturnReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                    //优惠金额
                    m.PreferentialAmount = s.PreferentialAmount;

                    //收款账户
                    m.ReturnReservationBillAccountings = s.ReturnReservationBillAccountings.Select(sba =>
                {
                    return new ReturnReservationBillAccountingModel()
                    {
                        Name = sba?.AccountingOption?.Name,
                        AccountingOptionId = sba?.Id ?? 0,
                        CollectionAmount = sba?.CollectionAmount ?? 0
                    };
                }).ToList();

                    //获取转单后单据的(现金)收款金额
                    m.CollectionAmount = r?.ReturnBillAccountings
                .Where(sa => sa.AccountingOption.AccountCodeTypeId == (int)AccountingCodeEnum.Cash)
                .Select(sa => sa.CollectionAmount)?.FirstOrDefault() ?? 0;

                    //计算获取欠款金额	
                    m.OweCash = -(r?.OweCash ?? 0);

                    m.ChangedDate = s.ChangedDate;
                    m.ChangedStatus = s.ChangedStatus;
                    m.ChangedUserId = s.ChangedUserId;

                    return m;
                }).ToList();

                bills.AddRange(returnOrders);

                #endregion


                return Json(new
                {
                    success = true,
                    total = bills.Count,
                    rows = bills
                });
            });
        }


        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.OrderToSaleBillsTransferBill)]
        public async Task<JsonResult> CreateOrUpdate(ChangeReservationUpdateModel data)
        {

            try
            {

                if (PeriodLocked(DateTime.Now))
                {
                    return Warning("锁账期间,禁止业务操作.");
                }

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("会计期间已结账,禁止业务操作.");
                }

                if (data != null && !string.IsNullOrEmpty(data.Ids))
                {


                    #region 单据验证
                    List<string> idList = data.Ids.Split(',').ToList();

                    string errMsg = string.Empty;
                    bool fg;

                    //销售订单
                    if (data.BillType == (int)BillTypeEnum.SaleReservationBill)
                    {
                        //一次查询所有涉及单据
                        List<int> ids = new List<int>();
                        idList.ForEach(i =>
                        {
                            ids.Add(int.Parse(i));
                        });
                        var saleReservations = _saleReservationBillService.GetSaleReservationBillsByIds(ids.ToArray()).ToList();

                        #region 验证调度
                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                            if (saleReservation != null && saleReservation.DispatchedStatus)
                            {
                                return Warning($"单据：{saleReservation.BillNumber}.已经调度，不能转单!");
                            }
                        }
                        #endregion

                        #region 验证盘点 注意这里要验证 销售订单 仓库，和转单后 仓库 2个仓库是否存在盘点
                        string thisMsg = string.Empty;
                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                            if (saleReservation != null && saleReservation.Items != null && saleReservation.Items.Count > 0)
                            {
                                //销售订单 仓库
                                fg = _wareHouseService.CheckProductInventory(curStore.Id, saleReservation.WareHouseId, saleReservation?.Items.Select(it => it.ProductId).Distinct().ToArray(), out thisMsg);
                                //新仓库 如果不等于 销售订单仓库
                                if (saleReservation.WareHouseId != data.WareHouseId)
                                {
                                    fg = _wareHouseService.CheckProductInventory(curStore.Id, data.WareHouseId, saleReservation?.Items.Select(it => it.ProductId).Distinct().ToArray(), out thisMsg);
                                }
                            }

                            if (!string.IsNullOrEmpty(thisMsg))
                            {
                                return Warning(errMsg);
                            }

                        }
                        #endregion

                        #region 验证库存

                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                            if (saleReservation != null && saleReservation.Items != null && saleReservation.Items.Count > 0)
                            {
                                //将一个单据中 相同商品 数量 按最小单位汇总
                                List<ProductStockItem> productStockItems = new List<ProductStockItem>();
                                var allProducts = _productService.GetProductsByIds(curStore.Id, saleReservation.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                                foreach (SaleReservationItem item in saleReservation.Items)
                                {
                                    if (item.ProductId != 0)
                                    {
                                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                        if (product != null)
                                        {
                                            ProductStockItem productStockItem = productStockItems.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
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
                                    }

                                }
                                //验证库存
                                //验证当前商品库存 注意是新仓库
                                fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.WareHouseId, productStockItems, out errMsg);
                                if (fg == false)
                                {
                                    return Warning(errMsg);
                                }
                            }
                        }

                        #endregion

                        #region 验证收款账户
                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            SaleReservationBill saleReservation = saleReservations.Where(sr => sr.Id == id).FirstOrDefault();
                            if (saleReservation != null && (saleReservation.SaleReservationBillAccountings == null || saleReservation.SaleReservationBillAccountings.Count == 0))
                            {
                                return Warning($"单据：{saleReservation.BillNumber}.收款账户未指定!");
                            }
                        }
                        #endregion

                    }

                    //退货订单
                    if (data.BillType == (int)BillTypeEnum.ReturnReservationBill)
                    {
                        //一次查询所有涉及单据
                        List<int> ids = new List<int>();
                        idList.ForEach(i =>
                        {
                            ids.Add(int.Parse(i));
                        });
                        var returnReservations = _returnReservationService.GetReturnReservationBillsByIds(ids.ToArray()).ToList();

                        #region 验证调度
                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            ReturnReservationBill returnReservation = returnReservations.Where(rr => rr.Id == id).FirstOrDefault();
                            if (returnReservation != null && returnReservation.DispatchedStatus)
                            {
                                return Warning($"单据：{returnReservation.BillNumber}.已经调度，不能转单!");
                            }
                        }
                        #endregion

                        #region 验证盘点 这里只验证 转单后的仓库
                        foreach (var a in idList)
                        {
                            int id = int.Parse(a);
                            ReturnReservationBill returnReservation = returnReservations.Where(rr => rr.Id == id).FirstOrDefault();
                            if (!_wareHouseService.CheckProductInventory(curStore.Id, data.WareHouseId, returnReservation?.Items.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                            {
                                return Warning(thisMsg);
                            }

                        }
                        #endregion
                    }

                    #endregion

                    var dataTo = data.ToEntity<ChangeReservationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.PC;

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _changeReservationBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, dataTo, _userService.IsAdmin(curStore.Id, curUser.Id)));
                    return Json(result);

                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

            _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateSuccessful, curUser.Id);
            _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateSuccessful);
            return Json(new { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful });
        }

    }
}