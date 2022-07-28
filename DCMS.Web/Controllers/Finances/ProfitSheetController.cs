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
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于利润表
    /// </summary>
    public class ProfitSheetController : BasePublicController
    {

        private readonly IAccountingService _accountingService;
        
        private readonly ILedgerDetailsService _ledgerDetailsService;
        private readonly IExportManager _exportManager;
        private readonly IClosingAccountsService _closingAccountsService;
        private readonly IRecordingVoucherService _recordingVoucherService;


        public ProfitSheetController(
            IWorkContext workContext,
            IAccountingService accountingService,
            ILogger loggerService,
            INotificationService notificationService,
            ILedgerDetailsService ledgerDetailsService,
             IClosingAccountsService closingAccountsService,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            IRecordingVoucherService recordingVoucherService,
            IExportManager exportManager) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
            _ledgerDetailsService = ledgerDetailsService;
            
            _exportManager = exportManager;
            _closingAccountsService = closingAccountsService;
            _recordingVoucherService = recordingVoucherService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.ProfitStatementView)]
        [ClearCache("ACCOUNTINGOPTION_PK.{0}")]
        public async Task<IActionResult> List(string recordTime)
        {

            var model = new ProfitSheetListModel();
            var dateList = new List<SelectListItem>();

            model.Dates = BindDatesSelection(_closingAccountsService.GetAll, curStore?.Id);
            model.RecordTime = string.IsNullOrEmpty(recordTime) ? DateTime.Now.ToShortDateString() : recordTime;

            if (string.IsNullOrEmpty(recordTime))
            {
                var defaultDate = model.Dates.FirstOrDefault();
                model.RecordTime = defaultDate?.Value;
            }

            if (string.IsNullOrEmpty(model.RecordTime))
            {
                model.RecordTime = DateTime.Now.ToString("yyyy-MM-dd");
            }

            var profitsheets = _ledgerDetailsService.GetProfitSheets(curStore.Id, DateTime.Parse(model.RecordTime)).Select(x => x).ToList();

            profitsheets.ForEach(s =>
            {
                var amountOfYear = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, s.AccountingOptionId, s.PeriodDate, $"settle{s.PeriodDate.ToString("yyyyMM")}").FirstOrDefault();
                switch (s.AccountingTypeId)
                {
                    //收入类
                    case (int)AccountingEnum.Income:
                        {
                            s.AccumulatedAmountOfYear = amountOfYear?.DebitAmount ?? 0 - amountOfYear?.CreditAmount ?? 0;
                        }
                        break;
                    //支出类
                    case (int)AccountingEnum.Expense:
                        {
                            s.AccumulatedAmountOfYear = amountOfYear?.CreditAmount ?? 0 - amountOfYear?.DebitAmount ?? 0;
                        }
                        break;
                }
            });

            model = await Task.Run(() =>
            {
                model.IncomeTrees = new ProfitSheetTotalModel
                {
                    Trees = _accountingService.PaseOptionsTree<ProfitSheet>(curStore?.Id ?? 0, (int)AccountingEnum.Income, DateTime.Parse(model.RecordTime), profitsheets),
                    TotalAccumulatedAmountOfYear = profitsheets.Where(c => c.AccountingTypeId == (int)AccountingEnum.Income).Sum(c => c.AccumulatedAmountOfYear),
                    TotalCurrentAmount = profitsheets.Where(c => c.AccountingTypeId == (int)AccountingEnum.Income).Sum(c => c.CurrentAmount)
                };

                model.ExpenditureTrees = new ProfitSheetTotalModel
                {
                    Trees = _accountingService.PaseOptionsTree<ProfitSheet>(curStore?.Id ?? 0, (int)AccountingEnum.Expense, DateTime.Parse(model.RecordTime), profitsheets),
                    TotalAccumulatedAmountOfYear = profitsheets.Where(c => c.AccountingTypeId == (int)AccountingEnum.Expense).Sum(c => c.AccumulatedAmountOfYear),
                    TotalCurrentAmount = profitsheets.Where(c => c.AccountingTypeId == 5).Sum(c => c.CurrentAmount)
                };
                return model;
            });
            /*
              直接计算法
　　              税前利润 = 销售收入 - 主营业务成本 - 营业和管理费用 - 折旧与摊销 + 投资收益 + 营业外收入 - 营业外支出
　            调整法
　　              税前利润 = 净利润 + 财务费用 + 所得税 = 利润总额 ＋ 财务费用
             */
            //税前利润
            model.PretaxAccumulatedAmount = model.IncomeTrees.TotalAccumulatedAmountOfYear - model.ExpenditureTrees.TotalAccumulatedAmountOfYear;
            model.PretaxCurrentAmount = model.IncomeTrees.TotalCurrentAmount - model.ExpenditureTrees.TotalCurrentAmount;

            return View(model);
        }


        //科目余额表导出
        [HttpGet]
        [AuthCode(AccessGranularityEnum.SaleDetailsExport)]
        public FileResult ExportProfitSheet(string recordTime)
        {

            #region 查询导出数据
            var profitsheets = _ledgerDetailsService
                .GetProfitSheets(curStore.Id, DateTime.Parse(recordTime ?? DateTime.Now.ToShortDateString()))
                .Select(x => x.ToModel<ProfitSheetModel>()).ToList();
            if (profitsheets != null && profitsheets.Count > 0)
            {
                var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, profitsheets.Select(tb => tb.AccountingOptionId).ToArray());

                profitsheets.ForEach(tr =>
                {
                    var accountingOption = allAccountingOptions.Where(ao => ao.Id == tr.AccountingOptionId).FirstOrDefault();
                    tr.AccountingOptionName = (accountingOption != null) ? accountingOption.Name : "";
                    tr.AccountingOptionCode = (accountingOption != null) ? accountingOption.Code : "";
                });
            }

            var data = profitsheets.Select(tb =>
            {
                var entity = tb.ToEntity<ProfitSheetExport>();
                switch (entity.AccountingTypeId)
                {
                    case 4:
                        entity.AccountingTypeName = "支出";
                        break;
                    case 5:
                        entity.AccountingTypeName = "收入";
                        break;

                    default:
                        break;
                }
                return entity;
            }).ToList();

            if (data != null && data.Count > 0)
            {
                data = data.OrderBy(d => d.AccountingTypeId).ThenBy(a => a.AccountingOptionId).ToList();
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportProfitSheetToXlsx(data);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "利润表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "利润表.xlsx");
            }
            #endregion

        }

    }
}