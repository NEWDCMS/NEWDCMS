using DCMS.Core;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Sales;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Terminals;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Settings;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于应收款初期
    /// </summary>
    public class ReceivableController : BasePublicController
    {
        private readonly IReceivableService _receivableService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISaleBillService _saleBillService;
        private readonly IAccountingService _accountingService;

        public ReceivableController(IWorkContext workContext,
            IReceivableService receivableService,
            IUserService userService,
            ITerminalService terminalService,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            IUserActivityService userActivityService,
            IAccountingService accountingService,
            ISaleBillService saleBillService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _receivableService = receivableService;
            _userService = userService;
            _terminalService = terminalService;
            _userActivityService = userActivityService;
            _saleBillService = saleBillService;
            _accountingService = accountingService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesView)]
        public IActionResult List(int? terminalId, string terminalName, int pagenumber = 0)
        {

            var model = new ReceivableListModel();

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            #region 绑定数据源

            #endregion

            var receivables = _receivableService.GetReceivables(terminalId, terminalName, curStore?.Id ?? 0, pagenumber, pageSize: 30);
            model.PagingFilteringContext.LoadPagedList(receivables);

            model.Lists = receivables.Select(s =>
            {
                var m = s.ToModel<ReceivableModel>();

                //业务员名称
                m.OperationUserName = _userService.GetUserName(curStore.Id, m.OperationUserId);
                //客户名称
                var terminal = _terminalService.GetTerminalById(curStore.Id, m.TerminalId);
                m.TerminalName = terminal?.Name;
                m.BossName = terminal?.BossName;
                m.BossCall = terminal?.BossCall;

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加应收款初期
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesSave)]
        public IActionResult Create()
        {
            var model = new ReceivableModel();
            //业务员
            model.OperationUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans);
    
            return View(model);
        }

        /// <summary>
        /// 编辑应收款初期
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesSave)]
        public IActionResult Edit(int? id)
        {
            var model = new ReceivableModel();

            if (!id.HasValue)
                return RedirectToAction("List");

            var receivable = _receivableService.GetReceivableById(curStore.Id, id ?? 0);
            if (receivable != null)
            {
                model = receivable.ToModel<ReceivableModel>();
                //获取客户名称
                Terminal terminal = _terminalService.GetTerminalById(curStore.Id, model.TerminalId);
                model.TerminalName = ((terminal != null && terminal.Name != null) ? terminal.Name : "");
                //业务员
                model.OperationUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans);
            }
            return View(model);
        }

        /// <summary>
        /// 更新/编辑应收款初期
        /// </summary>
        /// <param name="data"></param>
        /// <param name="saleId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesSave)]
        public JsonResult CreateOrUpdate(ReceivableModel data, int? billId)
        {
            if (PeriodClosed(DateTime.Now))
            {
                return Warning("会计期间已经锁定,禁止业务操作.");
            }

            try
            {
                if (data != null)
                {

                    Receivable receivable = new Receivable();

                    if (billId.HasValue && billId.Value != 0 && receivable != null)
                    {
                        receivable = _receivableService.GetReceivableById(curStore.Id, billId.Value);

                        receivable.StoreId = curStore.Id;
                        receivable.TerminalId = data.TerminalId;
                        receivable.OweCash = data.OweCash;
                        receivable.AdvanceCash = 0;
                        receivable.OperationUserId = data.OperationUserId;
                        receivable.Remark = data.Remark;
                        receivable.Status = false;
                        receivable.Deleted = false;
                        receivable.Inited = false;
                        receivable.BalanceDate = data.BalanceDate;
                        receivable.CreatedUserId = curUser.Id;
                        receivable.CreatedOnUtc = DateTime.Now;

                        _receivableService.UpdateReceivable(receivable);
                    }
                    else
                    {
                        #region 添加

                        //var sale = new SaleBill
                        //{
                        //    StoreId = curStore.Id,
                        //    TerminalId = data.TerminalId,
                        //    BusinessUserId = data.OperationUserId
                        //};
                        //var number = sale.GenerateNumber();

                        receivable.StoreId = curStore.Id;
                        receivable.TerminalId = data.TerminalId;
                        receivable.OweCash = data.OweCash;
                        receivable.AdvanceCash = 0;
                        receivable.OperationUserId = data.OperationUserId;
                        //receivable.Remark = data.Remark+"A/" + number; //方便删除时关联单据
                        receivable.Remark = data.Remark;
                        receivable.Status = false;
                        receivable.Deleted = false;
                        receivable.Inited = false;
                        receivable.BalanceDate = data.BalanceDate;
                        receivable.CreatedUserId = curUser.Id;
                        receivable.CreatedOnUtc = DateTime.Now;

                        _receivableService.InsertReceivable(receivable);

                        
                        //sale.OweCash = data.OweCash;
                        //sale.Remark = "应收款销售单/应收款期初备注";
                        //sale.MakeUserId = curUser.Id;
                        //sale.CreatedOnUtc = DateTime.Now;

                        //sale.DeliveryUserId = 0;
                        //sale.DepartmentId = 0;
                        //sale.DistrictId = 0;
                        //sale.WareHouseId = 0;
                        //sale.PayTypeId = 0;
                        //sale.TransactionDate = DateTime.Now;
                        //sale.IsMinUnitSale = false;
                        //sale.DefaultAmountId = "";
                        //sale.PaymentMethodType = 0;
                        //sale.SumAmount = 0;
                        //sale.ReceivableAmount = 0;
                        //sale.PreferentialAmount = 0;
                        //sale.SumCostAmount = 0;
                        //sale.SumProfit = 0;
                        //sale.SumCostProfitRate = 0;
                        //sale.SumCostPrice = 0;
                        //sale.PrintNum = 0;
                        //sale.ReceiptStatus = 0;
                        //sale.Operation = 0;
                        //sale.VoucherId = 0;
                        //sale.TaxAmount = 0;
                        //sale.AuditedUserId = 0;
                        //sale.AuditedStatus = true;
                        //sale.AuditedDate = DateTime.Now;
                        //sale.ReversedStatus = false;
                        //sale.ReversedUserId = 0;
                        //_saleBillService.InsertSaleBill(sale);
                        #endregion
                    }
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

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesSave)]
        public JsonResult Init(string selectData)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 验证

                var receivables = new List<Receivable>();
                string datas = string.Empty;

                if (string.IsNullOrEmpty(selectData))
                {
                    errMsg += "没有选择单号";
                }
                else
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        Receivable receivable = _receivableService.GetReceivableById(curStore.Id, int.Parse(id));
                        string[] idArray = receivable.Remark?.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (receivable == null)
                        {
                            errMsg += "信息不存在";
                        }
                        else
                        {
                            if (receivable.StoreId != curStore.Id)
                            {
                                errMsg += "只能初始化自己单据";
                            }
                            else
                            {
                                if (idArray != null && idArray.Length > 2)
                                {
                                    var salebill = _saleBillService.GetSaleBillByNumber(curStore.Id, idArray[1]);
                                    if (salebill != null && salebill.ReceiptStatus != 0)
                                        errMsg += "该项欠款已收钱!";
                                    else
                                        receivables.Add(receivable);
                                }
                                else
                                    receivables.Add(receivable);
                            }
                        }
                    }
                }

                #endregion

                #region 修改数据

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                else
                {

                    var res = receivables.Where(s => s.Inited == false).ToList();
                    if (res != null && res.Any())
                    {
                        foreach (var d in res)
                        {
                            var bill = new SaleBill
                            {
                                StoreId = curStore.Id,
                                TerminalId = d.TerminalId,
                                BusinessUserId = d.OperationUserId
                            };
                            bill.BillNumber = bill.GenerateNumber();
                            bill.OweCash = d.OweCash;
                            bill.Remark = "应收款销售单/应收款期初备注";
                            bill.MakeUserId = curUser.Id;
                            bill.CreatedOnUtc = DateTime.Now;
                            bill.DeliveryUserId = 0;
                            bill.DepartmentId = 0;
                            bill.DistrictId = 0;
                            bill.WareHouseId = 0;
                            bill.PayTypeId = 0;
                            bill.TransactionDate = DateTime.Now;
                            bill.IsMinUnitSale = false;
                            bill.DefaultAmountId = "";
                            bill.PaymentMethodType = 0;
                            bill.SumAmount = 0;
                            bill.ReceivableAmount = d.OweCash;
                            bill.PreferentialAmount = 0;
                            bill.SumCostAmount = 0;
                            bill.SumProfit = 0;
                            bill.SumCostProfitRate = 0;
                            bill.SumCostPrice = 0;
                            bill.PrintNum = 0;
                            bill.ReceiptStatus = 0;
                            bill.Operation = 0;
                            bill.VoucherId = 0;
                            bill.TaxAmount = 0;
                            bill.AuditedUserId = 0;
                            bill.AuditedStatus = true;
                            bill.AuditedDate = DateTime.Now;
                            bill.ReversedStatus = false;
                            bill.ReversedUserId = 0;

                            _saleBillService.InsertSaleBill(bill);

                            #region 收款账户映射

                            //获取默认收款账户
                            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleBill);
                            var saleAccounting = new SaleBillAccounting()
                            {
                                StoreId = curStore.Id,
                                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                                CollectionAmount = 0,
                                SaleBill = bill,
                                TerminalId = d.TerminalId,
                                BillId = bill.Id
                            };
                            //添加账户
                            _saleBillService.InsertSaleBillBillAccounting(saleAccounting);

                            #endregion

                            d.Remark = d.Remark + "A/" + bill.BillNumber;

                            //初始化状态
                            d.Inited = true;
                            _receivableService.UpdateReceivable(d);
                        }
                    }
                }

                if (fg)
                {
                    return Successful("初始化成功");
                }
                else
                {
                    return Warning(errMsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.EarlyReceivablesSave)]
        public JsonResult Delete(string selectData)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 验证

                List<Receivable> receivables = new List<Receivable>();
                string datas = string.Empty;

                if (string.IsNullOrEmpty(selectData))
                {
                    errMsg += "没有选择单号";
                }
                else
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        Receivable receivable = _receivableService.GetReceivableById(curStore.Id, int.Parse(id));
                        string[] idArray = receivable.Remark?.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (receivable == null)
                        {
                            errMsg += "信息不存在";
                        }
                        else
                        {
                            if (receivable.StoreId != curStore.Id)
                            {
                                errMsg += "只能删除自己单据";
                            }
                            else
                            {
                                if (idArray!=null && idArray[1] != null)
                                {
                                    var salebill = _saleBillService.GetSaleBillByNumber(curStore.Id, idArray[1]);
                                    if (salebill != null && salebill.ReceiptStatus != 0)
                                        errMsg += "该项欠款已收钱!";
                                    else
                                        receivables.Add(receivable);
                                }
                                else
                                    receivables.Add(receivable);
                            }
                        }
                    }
                }

                #endregion

                #region 修改数据
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                else
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in receivables)
                    {
                        string[] idArray = d.Remark?.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (idArray != null && idArray[1] != null)
                        {
                            var salebill = _saleBillService.GetSaleBillByNumber(curStore.Id, idArray[1]);
                            if (salebill != null)
                                _saleBillService.DeleteSaleBill(salebill);
                        }

                        d.Deleted = true;
                        _receivableService.UpdateReceivable(d);
                    }
                    #endregion
                }

                if (fg)
                {
                    return Successful("删除成功");
                }
                else
                {
                    return Warning(errMsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }



    }
}