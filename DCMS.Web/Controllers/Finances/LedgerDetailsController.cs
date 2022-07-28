using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Common;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于明细分类账
    /// </summary>
    public class LedgerDetailsController : BasePublicController
    {

        private readonly IAccountingService _accountingService;
        
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly ILedgerDetailsService _ledgerDetailsService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IBillConvertService _billConvertService;

        public LedgerDetailsController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IAccountingService accountingService,
            IRecordingVoucherService recordingVoucherService,
            ILogger loggerService,
            INotificationService notificationService,
            ILedgerDetailsService ledgerDetailsService,
            ITrialBalanceService trialBalanceService,
            IBillConvertService billConvertService,
            IStaticCacheManager cacheManager) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
            _recordingVoucherService = recordingVoucherService; //记账凭证
            _ledgerDetailsService = ledgerDetailsService;  //明细账
            
            _trialBalanceService = trialBalanceService;
            _billConvertService = billConvertService;
        }


        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.SubsidiaryLedgerView)]
        [ClearCache("ACCOUNTINGOPTION_PK.{0}")]
        public IActionResult List(DateTime? startTime, DateTime? endTime, int? optionid)
        {
            var model = new LedgerDetailsListModel
            {
                Trees = _accountingService.PaseOptionsTree<LedgerDetails>(curStore?.Id ?? 0, DateTime.Now, null)
            };

            if (!startTime.HasValue || !endTime.HasValue || !optionid.HasValue)
            {
                startTime = DateTime.Now;
                endTime = DateTime.Now;
            }

            model.StartTime = startTime.Value.AddDays(1 - startTime.Value.Day);
            model.EndTime = endTime.Value.AddDays(1 - endTime.Value.Day).AddMonths(1).AddDays(-1);

            var items = _ledgerDetailsService.GetLedgerDetailsByOptions(curStore.Id, new int[] { optionid ?? 0 }, startTime, endTime);

            var groupMonths = items.OrderBy(m => m.RecordTime).GroupBy(m => new GroupMonths
            {
                Year = m.RecordTime.Year,
                Month = m.RecordTime.Month,
                LastDay = m.RecordTime.AddDays(1 - m.RecordTime.Day).AddMonths(1).AddDays(-1)
            });

            //获取当前科目期初余额
            var initiallBalance = _recordingVoucherService.GetInitiallBalance(curStore.Id, optionid ?? 0, null, startTime.Value.AddDays(-1), 0);

            //期初方向
            var startDirectionBalance = _billConvertService.JudgmentLending(initiallBalance);

            //期初
            model.StartBalanceDebitAmount = initiallBalance.Item2;
            model.StartBalanceCreditAmount = initiallBalance.Item3;
            model.StartBalanceDirection = startDirectionBalance.Item1;
            model.StartBalanceAmount = startDirectionBalance.Item2;

            var allGroupMonths = new List<GroupMonths>();
            if (groupMonths.Any())
            {
                foreach (var groups in groupMonths)
                {
                    var group = groups.Key;

                    decimal lastBalance = initiallBalance.Item1;

                    group.Items = groups.OrderBy(item => item.RecordTime).Select(item =>
                    {
                        var ldm = new LedgerDetailsModel
                        {
                            RecordTime = item.RecordTime,
                            BillNumber = item.RecordingVoucher.BillNumber,
                            BillLink = _billConvertService.GenerateBillUrl(item.RecordingVoucher.BillTypeId ?? 0, item.RecordingVoucher.BillId),
                            RecordingVoucherId = item.RecordingVoucherId
                        };

                        //当期方向
                        var directionBalance = _billConvertService.JudgmentLending(lastBalance, item.DebitAmount ?? 0, item.CreditAmount ?? 0);

                        ldm.Direction = directionBalance.Item1;

                        ldm.RecordName = item.RecordingVoucher.RecordName;
                        ldm.RecordNumber = item.RecordingVoucher.RecordNumber;

                        ldm.Summary = item.Summary;
                        ldm.AccountingOptionId = item.AccountingOptionId;
                        ldm.AccountingOptionName = item.AccountingOptionName;

                        ldm.DebitAmount = item.DebitAmount;
                        ldm.CreditAmount = item.CreditAmount;

                        //当期余额 = 上期余额 + 当期借 - 当期贷
                        ldm.Balances = directionBalance.Item2;
                        lastBalance = ldm.Balances ?? 0;

                        ldm.Items = _recordingVoucherService.GetVoucherItemsByRecordingVoucherId(curStore.Id, item.RecordingVoucherId).Select(s => { return s.ToModel<VoucherItemModel>(); }).ToList();

                        return ldm;
                    }).ToList();


                    //本期合计
                    group.CurBalanceDebitAmount = group.Items.Sum(s => s.DebitAmount ?? 0);
                    group.CurBalanceCreditAmount = group.Items.Sum(s => s.CreditAmount ?? 0);
                    //本期合计方向
                    var curDirectionBalance = _billConvertService.JudgmentLending(group.CurBalanceDebitAmount ?? 0, group.CurBalanceCreditAmount ?? 0);
                    group.CurBalanceDirection = curDirectionBalance.Item1;
                    //本期合计余额
                    group.CurBalanceAmount = curDirectionBalance.Item2;


                    //本年合计
                    var yearBalance = _recordingVoucherService.GetInitiallBalance(curStore.Id, optionid ?? 0, DateTime.Parse(group.LastDay.ToString("yyyy-01-01")), group.LastDay, 0);
                    var yearDirectionBalance = _billConvertService.JudgmentLending(yearBalance.Item2, yearBalance.Item3);
                    group.YearBalanceDebitAmount = yearBalance.Item2;
                    group.YearBalanceCreditAmount = yearBalance.Item3;
                    //本年合计方向
                    group.YearBalanceDirection = yearDirectionBalance.Item1;
                    //本年合计余额 
                    group.YearBalanceAmount = yearDirectionBalance.Item2;


                    allGroupMonths.Add(group);
                }
            }
            else
            {
                var group = new GroupMonths
                {
                    //本期合计
                    CurBalanceDebitAmount = 0,
                    CurBalanceCreditAmount = 0
                };
                //本期合计方向
                var curDirectionBalance = _billConvertService.JudgmentLending(group.CurBalanceDebitAmount ?? 0, group.CurBalanceCreditAmount ?? 0);
                group.CurBalanceDirection = curDirectionBalance.Item1;
                //本期合计余额
                group.CurBalanceAmount = curDirectionBalance.Item2;


                //本年合计
                var yearBalance = _recordingVoucherService.GetInitiallBalance(curStore.Id, optionid ?? 0, DateTime.Parse(group.LastDay.ToString("yyyy-01-01")), group.LastDay, 0);
                var yearDirectionBalance = _billConvertService.JudgmentLending(yearBalance.Item2, yearBalance.Item3);
                group.YearBalanceDebitAmount = yearBalance.Item2;
                group.YearBalanceCreditAmount = yearBalance.Item3;
                //本年合计方向
                group.YearBalanceDirection = yearDirectionBalance.Item1;
                //本年合计余额 
                group.YearBalanceAmount = yearDirectionBalance.Item2;

                allGroupMonths.Add(group);
            }

            model.GroupMonths = allGroupMonths;

            return View(model);
        }

    }
}