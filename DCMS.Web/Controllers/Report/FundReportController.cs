using DCMS.Core;
using DCMS.Core.Domain.Report;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Report;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 报表 资金报表
    /// </summary>
    public class FundReportController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IFundReportService _fundReportService;
        private readonly IExportManager _exportManager;

        public FundReportController(
            IStoreContext storeContext,
            INotificationService notificationService,
            IUserService userService,
            IAccountingService accountingService,
            ILogger loggerService,
            IDistrictService districtService,
            IChannelService channelService,
            IManufacturerService manufacturerService,
            IFundReportService fundReportService,
            IExportManager exportManager,
            IWorkContext workContext
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _accountingService = accountingService;
            _channelService = channelService;
            _manufacturerService = manufacturerService;
            _districtService = districtService;
            _fundReportService = fundReportService;
            _exportManager = exportManager;
        }

        #region 客户往来账
        /// <summary>
        /// 客户往来账
        /// </summary>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="billTypeId">单据类型Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">备注</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CustomerCurrentAccountView)]
        public IActionResult FundReportCustomerAccount(int? districtId, int? channelId, int? terminalId, string terminalName, string billNumber,
            int? billTypeId, DateTime? startTime, DateTime? endTime, string remark, int pagenumber = 0)
        {


            var model = new FundReportCustomerAccountListModel();

            #region 绑定数据源

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = districtId ?? null;

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = channelId ?? null;

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //单据编号
            model.BillNumber = billNumber;

            //单据类型
            model.BillTypes = new SelectList(new List<SelectListItem>
            {
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.SaleBill),Value = ((int)BillTypeEnum.SaleBill).ToString() },
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnBill),Value = ((int)BillTypeEnum.ReturnBill).ToString()},
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.CashReceiptBill),Value = ((int)BillTypeEnum.CashReceiptBill).ToString()},
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.AdvanceReceiptBill),Value = ((int)BillTypeEnum.AdvanceReceiptBill).ToString()}
            }, "Value", "Text");
            model.BillTypeId = billTypeId ?? null;
            //备注
            model.Remark = remark;
            model.StartTime = startTime ?? DateTime.Parse((DateTime.Now).ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now;

            #endregion
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _fundReportService.GetFundReportCustomerAccount(curStore?.Id ?? 0, districtId, channelId, terminalId, terminalName, billNumber,
                billTypeId,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                remark);

            var items = new PagedList<CustomerAccountDealings>(sqlDatas, pagenumber, 30);
            model.Items = items;

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //客户往来账导出
        [AuthCode((int)AccessGranularityEnum.CustomerCurrentAccountExport)]
        public FileResult ExportFundReportCustomerAccount(int? districtId, int? channelId, int? terminalId, string terminalName, string billNumber,
            int? billTypeId, DateTime? startTime, DateTime? endTime, string remark)
        {

            #region 查询导出数据

            var datas = _fundReportService.GetFundReportCustomerAccount(
                curStore?.Id ?? 0,
                districtId,
                channelId,
                terminalId,
                terminalName,
                billNumber,
                billTypeId,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1),
                remark
                ).Select(sd =>
                {
                    return sd ?? new CustomerAccountDealings();

                }).AsQueryable().ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportFundReportCustomerAccountToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "客户往来账单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "客户往来账单.xlsx");
            }
            #endregion
        }
        #endregion

        #region 客户应收款
        /// <summary>
        /// 客户应收款
        /// </summary>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="moreDay">账期大于...天</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">整单备注</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.CustomerReceivableView)]
        public IActionResult FundReportCustomerReceiptCash(int? channelId, int? businessUserId, int? districtId, int? terminalId, string terminalName, int? moreDay, DateTime? startTime, DateTime? endTime, string remark, int pagenumber = 0)
        {

            var model = new FundReportCustomerReceiptCashListModel();

            #region 绑定数据源

            //客户渠道
            model.Channels = BindChanneSelection(_channelService.BindChannelList, curStore);
            model.ChannelId = channelId ?? null;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = businessUserId ?? null;

            //客户片区
            model.Districts = BindDistrictSelection(_districtService.BindDistrictList, curStore);
            model.DistrictId = districtId ?? null;

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            //账期大于...天
            model.MoreDay = (moreDay ?? null);

            model.StartTime = startTime ?? DateTime.Parse((DateTime.Now).ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now;

            //备注
            model.Remark = remark;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _fundReportService.GetFundReportCustomerReceiptCash(curStore?.Id ?? 0,
                channelId, businessUserId, districtId, terminalId, terminalName, moreDay, startTime, endTime, remark);
            var items = new PagedList<FundReportCustomerReceiptCash>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //客户应收款导出
        [AuthCode((int)AccessGranularityEnum.CustomerReceivableExport)]
        public FileResult ExportFundReportCustomerReceiptCash(int? channelId, int? businessUserId, int? districtId, int? terminalId, string terminalName, int? moreDay, DateTime? startTime, DateTime? endTime, string remark)
        {

            #region 查询导出数据

            var datas = _fundReportService.GetFundReportCustomerReceiptCash(
                curStore?.Id ?? 0,
                channelId,
                businessUserId,
                districtId,
                terminalId,
                terminalName,
                moreDay,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1), remark
                ).Select(sd =>
                {
                    return sd ?? new FundReportCustomerReceiptCash();

                }).AsQueryable().ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportFundReportCustomerReceiptCashToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "客户应收款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "客户应收款单.xlsx");
            }
            #endregion
        }
        #endregion

        #region 供应商来账
        /// <summary>
        /// 供应商来账
        /// </summary>
        /// <param name="billNumber">单据编号</param>
        /// <param name="billTypeId">单据类型Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SupplierCurrentAccountView)]
        public IActionResult FundReportManufacturerAccount(string billNumber, int? billTypeId, int? manufacturerId, string remark, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {


            var model = new FundReportManufacturerAccountListModel
            {

                #region 绑定数据源

                //单据编号
                BillNumber = billNumber,

                //单据类型
                BillTypes = new SelectList(new List<SelectListItem>
            {
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseBill),Value = ((int)BillTypeEnum.PurchaseBill).ToString() },
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseReturnBill),Value = ((int)BillTypeEnum.PurchaseReturnBill).ToString()},
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.PaymentReceiptBill),Value = ((int)BillTypeEnum.PaymentReceiptBill).ToString()},
                new SelectListItem{Text = CommonHelper.GetEnumDescription(BillTypeEnum.AdvancePaymentBill),Value = ((int)BillTypeEnum.AdvancePaymentBill).ToString()}
            }, "Value", "Text"),
                BillTypeId = billTypeId ?? null,

                //供应商
                Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore),
                ManufacturerId = manufacturerId ?? null,

                //备注
                Remark = remark,

                #endregion

                StartTime = startTime ?? DateTime.Parse((DateTime.Now).ToString("yyyy-MM-01 00:00:00")),
                EndTime = endTime ?? DateTime.Now
            };

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _fundReportService.GetFundReportManufacturerAccount(curStore?.Id ?? 0,
                billNumber, billTypeId, manufacturerId, remark, model.StartTime, model.EndTime);

            var items = new PagedList<FundReportManufacturerAccount>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //供应商往来账导出
        [AuthCode((int)AccessGranularityEnum.SupplierCurrentAccountExport)]
        public FileResult ExportFundReportManufacturerAccount(string billNumber, int? billTypeId, int? manufacturerId, string remark, DateTime? startTime, DateTime? endTime)
        {

            #region 查询导出数据

            var datas = _fundReportService.GetFundReportManufacturerAccount(
                curStore?.Id ?? 0,
                billNumber,
                billTypeId,
                manufacturerId,
                remark,
                startTime ?? DateTime.Now,
                endTime ?? DateTime.Now.AddDays(1)
                ).Select(sd =>
                {
                    return sd ?? new FundReportManufacturerAccount();

                }).AsQueryable().ToList();

            #endregion

            #region 导出
            var ms = _exportManager.ExportFundReportManufacturerAccountToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "供应商往来账单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "供应商往来账单.xlsx");
            }
            #endregion
        }
        #endregion

        #region 供应商应付款
        /// <summary>
        /// 供应商应付款
        /// </summary>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="moreDay">账期大于...天</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.SupplierShouldPayView)]
        public IActionResult FundReportManufacturerPayCash(int? businessUserId, int? moreDay, DateTime? startTime, DateTime? endTime, int pagenumber = 0)
        {

            var model = new FundReportManufacturerPayCashListModel();

            #region 绑定数据源

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));
            model.BusinessUserId = businessUserId ?? null;

            //账期大于...天
            model.MoreDay = (moreDay ?? null);

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            model.StartTime = startTime ?? DateTime.Parse((DateTime.Now).ToString("yyyy-MM-01 00:00:00"));
            model.EndTime = endTime ?? DateTime.Now;


            var sqlDatas = _fundReportService.GetFundReportManufacturerPayCash(curStore?.Id ?? 0, businessUserId, moreDay, model.StartTime, model.EndTime);

            var items = new PagedList<FundReportManufacturerPayCash>(sqlDatas, pagenumber, 30);

            model.Items = items;

            #region 汇总

            if (sqlDatas != null && sqlDatas.ToList().Count > 0)
            {

                //累计欠款（总）
                model.TotalSumOweCase = sqlDatas.Sum(a => a.OweCase ?? 0);

            }

            #endregion

            model.PagingFilteringContext.LoadPagedList(items);

            return View(model);
        }

        //供应商应付款导出
        [AuthCode((int)AccessGranularityEnum.SupplierShouldPayExport)]
        public FileResult ExportFundReportManufacturerPayCash(int? businessUserId, int? moreDay, DateTime? startTime, DateTime? endTime)
        {

            var datas = _fundReportService.GetFundReportManufacturerPayCash(curStore?.Id ?? 0, businessUserId, moreDay, startTime, endTime).ToList();

            #region 导出
            var ms = _exportManager.ExportFundReportManufacturerPayCashToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "供应商应付款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "供应商应付款单.xlsx");
            }
            #endregion
        }
        #endregion

        #region 预收款余额
        /// <summary>
        /// 预收款余额
        /// </summary>
        /// <param name="terminalId">客户Id</param>
        /// <param name="terminalName">客户名称</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrepaidBalanceView)]
        public IActionResult FundReportAdvanceReceiptOverage(int? terminalId, string terminalName, int pagenumber = 0)
        {


            var model = new FundReportAdvanceReceiptOverageListModel();

            #region 绑定数据源

            //客户
            model.TerminalId = terminalId;
            model.TerminalName = terminalName;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var lists = _fundReportService.GetFundReportAdvanceReceiptOverage(curStore?.Id ?? 0, terminalId);
            var items = new PagedList<FundReportAdvanceReceiptOverage>(lists, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            model.DynamicColumns = _accountingService.GetSubAccountingOptionsByAccountCodeTypeIds(curStore?.Id ?? 0, new int[] { (int)AccountingCodeEnum.AdvanceReceipt }).Select(a => a).ToList();

            //动态列补0
            if (items != null && items.Count > 0)
            {
                items.ForEach(it =>
                {
                    if (model.DynamicColumns != null && model.DynamicColumns.Count > 0)
                    {
                        model.DynamicColumns.ForEach(dc =>
                        {
                            //如果动态列中的科目不在当前账户中，当前账户此科目补零
                            if (it.AccountingOptions.Where(ao => ao.AccountingOptionId == dc.Id).Count() == 0)
                            {
                                it.AccountingOptions.Add(new Core.Domain.Configuration.Accounting() { AccountingOptionId = dc.Id, CollectionAmount = 0, Name = dc.Name });
                            }
                        });
                    }

                    if (it.AccountingOptions != null && it.AccountingOptions.Count > 0)
                    {
                        it.AccountingOptions.ForEach(ao =>
                        {
                            if (model.DynamicColumns == null || model.DynamicColumns.Count == 0 || model.DynamicColumns.Where(dc => dc.Id == ao.AccountingOptionId).Count() == 0)
                            {
                                model.DynamicColumns.Add(new Core.Domain.Configuration.AccountingOption() { Id = ao.AccountingOptionId, Name = ao.Name });
                            }
                        });
                    }
                });
            }

            return View(model);
        }

        //预收款余额导出
        [AuthCode((int)AccessGranularityEnum.PrepaidBalanceExport)]
        public FileResult ExportFundReportAdvanceReceiptOverage(int? terminalId, string terminalName)
        {

            #region 查询导出数据

            var datas = _fundReportService.GetFundReportAdvanceReceiptOverage(curStore?.Id ?? 0,
                terminalId).Select(sd =>
                {
                    return sd ?? new FundReportAdvanceReceiptOverage();

                }).AsQueryable().ToList();
            #endregion

            #region 导出
            var ms = _exportManager.ExportFundReportAdvanceReceiptOverageToXlsx(datas, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "预收款余额.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "预收款余额.xlsx");
            }
            #endregion
        }
        #endregion

        #region 预付款余额
        /// <summary>
        /// 预付款余额
        /// </summary>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrepaymentBalanceView)]
        public IActionResult FundReportAdvancePaymentOverage(int? manufacturerId, DateTime? endTime, int pagenumber = 0)
        {

            var model = new FundReportAdvancePaymentOverageListModel();

            #region 绑定数据源

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = (manufacturerId ?? null);
            model.EndTime = endTime ?? DateTime.Now;

            #endregion

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var sqlDatas = _fundReportService.GetFundReportAdvancePaymentOverage(curStore?.Id ?? 0, manufacturerId, model.EndTime);

            var items = new PagedList<FundReportAdvancePaymentOverage>(sqlDatas, pagenumber, 30);
            model.Items = items;
            model.PagingFilteringContext.LoadPagedList(items);

            model.DynamicColumns = _accountingService.GetSubAccountingOptionsByAccountCodeTypeIds(curStore?.Id ?? 0, new int[] { (int)AccountingCodeEnum.Imprest }, true).Select(a => a).ToList();

            //动态列补0
            if (items != null && items.Count > 0)
            {
                items.ForEach(it =>
                {
                    if (model.DynamicColumns != null && model.DynamicColumns.Count > 0)
                    {
                        model.DynamicColumns.ForEach(dc =>
                        {
                            //如果动态列中的科目不在当前账户中，当前账户此科目补零
                            if (it.AccountingOptions.Where(ao => ao.AccountingOptionId == dc.Id).Count() == 0)
                            {
                                it.AccountingOptions.Add(new Core.Domain.Configuration.Accounting() { AccountingOptionId = dc.Id, CollectionAmount = 0, Name = dc.Name });
                            }
                        });
                    }

                    if (it.AccountingOptions != null && it.AccountingOptions.Count > 0)
                    {
                        it.AccountingOptions.ForEach(ao =>
                        {
                            if (model.DynamicColumns == null || model.DynamicColumns.Count == 0 || model.DynamicColumns.Where(dc => dc.Id == ao.AccountingOptionId).Count() == 0)
                            {
                                model.DynamicColumns.Add(new Core.Domain.Configuration.AccountingOption() { Id = ao.AccountingOptionId, Name = ao.Name });
                            }
                        });
                    }


                });
            }

            return View(model);
        }

        //预付款余额
        [AuthCode((int)AccessGranularityEnum.PrepaymentBalanceExport)]
        public FileResult ExportFundReportAdvancePaymentOverage(int? manufacturerId, DateTime? endTime)
        {

            #region 查询导出数据

            var datas = _fundReportService.GetFundReportAdvancePaymentOverage(curStore?.Id ?? 0, manufacturerId, endTime);

            #endregion

            #region 导出
            var ms = _exportManager.ExportFundReportAdvancePaymentOverageToXlsx(datas);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "预付款余额.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "预付款余额.xlsx");
            }
            #endregion
        }
        #endregion


    }
}