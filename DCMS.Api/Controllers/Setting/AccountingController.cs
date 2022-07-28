using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Services.Configuration;
using DCMS.Services.Settings;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.ViewModel.Models.Configuration;
using System.Linq;
using DCMS.Api.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 财务科目
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/config")]
    public class AccountingController : BaseAPIController
    {
        private readonly ISettingService _settingService;
        private readonly IAccountingService _accountingService;
        

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="accountingService"></param>
        public AccountingController(
            ISettingService settingService,
            IAccountingService accountingService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _settingService = settingService;
            _accountingService = accountingService;
            
        }


        /// <summary>
        /// 获取指定单据的默认收付款方式
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billTypeId"></param>
        /// <returns></returns>
        [HttpGet("accounting/getpaymentmethods/{store}")]
        [SwaggerOperation("GetPaymentMethods")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<AccountingOptionModel>>> GetPaymentMethods(int? store, int? billTypeId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error2<AccountingOptionModel>(Resources.ParameterError);

            //获取收款方式
            var codeTypeIds = new List<int>();
            switch ((BillTypeEnum)billTypeId)
            {
                case BillTypeEnum.SaleReservationBill:
                    //资产类：预收账款
                    //type.(int)AccountingEnum.Assets;
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.SaleBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnReservationBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PurchaseReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.PurchaseBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.CashReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PaymentReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    //资产类：库存现金,银行存款,其他账户
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.AdvancePaymentBill:
                    //资产类：库存现金,银行存款,其他账户
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.CostContractBill:
                    //(费用类别)
                    //损益类（支出）：销售费用,管理费用,财务费用
                    codeTypeIds.Add((int)AccountingCodeEnum.SaleFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.ManageFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.FinanceFees);
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                default:
                    break;
            }

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<AccountingOptionModel>();
                    var options = _accountingService.GetAllAccounts(store ?? 0, null, codeTypeIds.ToArray()).ToList();
                    var numbers = options.Where(s => codeTypeIds?.Contains(s.AccountCodeTypeId ?? 0) ?? true).Select(s => s.Number).ToList();
                    model = options
                    .Where(m => (numbers?.Contains(m.ParentId ?? 0) ?? true) || (numbers?.Contains(m.Number) ?? true))
                    .OrderBy(m => m.ParentId)
                    .ThenBy(m => m.DisplayOrder)
                    .Select(m =>
                    {
                        var p = m.ToModel<AccountingOptionModel>();
                        return p;
                    }).ToList();

                    return this.Successful2("", model);
                }
                catch (Exception ex)
                {
                    return this.Error2<AccountingOptionModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取指定单据类型的初始默认科目账户
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billTypeId"></param>
        /// <returns>
        /// item1:单据默认收付款科目
        /// item1:单据默认初始科目项目
        /// item3:单据的收付款账户
        /// </returns>
        [HttpGet("accounting/getdefaultaccounting/{store}")]
        [SwaggerOperation("GetDefaultAccounting")]
        //[AuthBaseFilter]
        public async Task<APIResult<Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>>>> GetDefaultAccounting(int? store, int? billTypeId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>>> (Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var allOptions = _accountingService.GetAllAccounts(store ?? 0).ToList();
                    var accounts = _accountingService.GetDefaultAccounting(store ?? 0, (BillTypeEnum)billTypeId, 0, allOptions);
                    var accountReceipts = _accountingService.GetReceiptAccounting(store ?? 0, (BillTypeEnum)billTypeId, 0, allOptions);

                    int defaultCodeType = 0;
                    switch ((BillTypeEnum)billTypeId)
                    {
                        case BillTypeEnum.SaleReservationBill:
                            //默认：预收款
                            defaultCodeType = (int)AccountingCodeEnum.AdvancesReceived;
                            break;
                        case BillTypeEnum.SaleBill:
                            //默认：现金
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.ReturnReservationBill:
                            //默认：预收款
                            defaultCodeType = (int)AccountingCodeEnum.AdvancesReceived;
                            break;
                        case BillTypeEnum.ReturnBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.PurchaseReturnBill:
                            //默认：现金,
                            //defaultCodeType = (int)AccountingCodeEnum.Cash;
                            //默认：预付款
                            defaultCodeType = (int)AccountingCodeEnum.Imprest;
                            break;
                        case BillTypeEnum.PurchaseBill:
                            //默认：现金,
                            //defaultCodeType = (int)AccountingCodeEnum.Cash;
                            //默认：预付款
                            defaultCodeType = (int)AccountingCodeEnum.Imprest;
                            break;
                        case BillTypeEnum.CashReceiptBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.PaymentReceiptBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.AdvanceReceiptBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.AdvancePaymentBill:
                            //默认：现金
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.CostExpenditureBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        case BillTypeEnum.CostContractBill:
                            //默认（无）
                            defaultCodeType = 0;
                            break;
                        case BillTypeEnum.FinancialIncomeBill:
                            //默认：现金,
                            defaultCodeType = (int)AccountingCodeEnum.Cash;
                            break;
                        default:
                            break;
                    }

                    //默认科目
                    var defaultAccount = accountReceipts.Item1
                        .Select(s => s)
                        .Where(s => s.AccountCodeTypeId == defaultCodeType)
                        .FirstOrDefault();

                    var result = new Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>>(defaultAccount, accounts, accountReceipts.Item1, accountReceipts.Item2);

                    return this.Successful3("", result);
                }
                catch (Exception ex)
                {
                    return this.Error3<Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>>>(ex.Message);
                }
            });

        }

    }
}