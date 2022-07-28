using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于科目余额表
    /// </summary>
    public class TrialBalanceController : BasePublicController
    {
        private readonly IAccountingService _accountingService;
        
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IExportManager _exportManager;
        private readonly IClosingAccountsService _closingAccountsService;

        public TrialBalanceController(
            IStoreContext storeContext,
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            IAccountingService accountingService,
           ILogger loggerService,
            INotificationService notificationService,
            ITrialBalanceService trialBalanceService,
            IClosingAccountsService closingAccountsService,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            
            _accountingService = accountingService;
            _trialBalanceService = trialBalanceService;
            _exportManager = exportManager;
            _closingAccountsService = closingAccountsService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.AccountBalanceView)]
        [ClearCache("ACCOUNTINGOPTION_PK.{0}")]
        public IActionResult List(string recordTime)
        {
            var dateList = new List<SelectListItem>();
            var model = new TrialBalanceListModel
            {
                Dates = BindDatesSelection(_closingAccountsService.GetAll, curStore?.Id),
                RecordTime = string.IsNullOrEmpty(recordTime) ? DateTime.Now.ToShortDateString() : recordTime
            };

            if (string.IsNullOrEmpty(recordTime))
            {
                var defaultDate = model.Dates.FirstOrDefault();
                model.RecordTime = defaultDate?.Value;
            }

            if (string.IsNullOrEmpty(model.RecordTime))
            {
                model.RecordTime = DateTime.Now.ToString("yyyy-MM-dd");
            }

            var triabalances = _trialBalanceService.GetTrialBalances(curStore.Id, 0, DateTime.Parse(model.RecordTime)).Select(x => x).ToList();

            model.Trees = _accountingService.PaseOptionsTree<TrialBalance>(curStore?.Id ?? 0, DateTime.Parse(model.RecordTime), triabalances);

            model.TotalInitialBalanceCredit = triabalances.Sum(x => x.InitialBalanceCredit) ?? 0;
            model.TotalInitialBalanceDebit = triabalances.Sum(x => x.InitialBalanceDebit) ?? 0;
            model.TotalPeriodBalanceCredit = triabalances.Sum(x => x.PeriodBalanceCredit) ?? 0;
            model.TotalPeriodBalanceDebit = triabalances.Sum(x => x.PeriodBalanceDebit) ?? 0;
            model.TotalEndBalanceCredit = triabalances.Sum(x => x.EndBalanceCredit) ?? 0;
            model.TotalEndBalanceDebit = triabalances.Sum(x => x.EndBalanceDebit) ?? 0;

            return View(model);
        }


        //科目余额表导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AccountBalanceExport)]
        public FileResult ExportTrialBalance(string recordTime)
        {

            #region 查询导出数据
            var triabalances = _trialBalanceService.GetTrialBalances(curStore.Id, 0, DateTime.Parse(recordTime ?? DateTime.Now.ToShortDateString())).Select(x => x.ToModel<TrialBalanceModel>()).ToList();

            if (triabalances != null && triabalances.Count > 0)
            {
                var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, triabalances.Select(tb => tb.AccountingOptionId).ToArray());

                triabalances.ForEach(tr =>
                {
                    var accountingOption = allAccountingOptions.Where(ao => ao.Id == tr.AccountingOptionId).FirstOrDefault();
                    tr.AccountingOptionName = (accountingOption != null) ? accountingOption.Name : "";
                    tr.AccountingOptionCode = (accountingOption != null) ? accountingOption.Code : "";
                });
            }

            #endregion

            var data = triabalances.Select(tb =>
            {
                return tb.ToEntity<TrialBalanceExport>();
            }).ToList();

            #region 导出
            var ms = _exportManager.ExportTrialBalanceToXlsx(data);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "科目余额表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "科目余额表.xlsx");
            }
            #endregion

        }


    }
}