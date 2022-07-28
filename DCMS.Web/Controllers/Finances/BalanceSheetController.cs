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
    /// 用于资产负债表
    /// </summary>
    public class BalanceSheetController : BasePublicController
    {

        private readonly IAccountingService _accountingService;
        
        private readonly ILedgerDetailsService _ledgerDetailsService;
        private readonly IExportManager _exportManager;
        private readonly IClosingAccountsService _closingAccountsService;

        public BalanceSheetController(
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            IAccountingService accountingService,
            ILogger loggerService,
            INotificationService notificationService,
            ILedgerDetailsService ledgerDetailsService,
            IClosingAccountsService closingAccountsService,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _accountingService = accountingService;
            _ledgerDetailsService = ledgerDetailsService;
            
            _exportManager = exportManager;
            _closingAccountsService = closingAccountsService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.BalanceSheetView)]
        [ClearCache("ACCOUNTINGOPTION_PK.{0}")]
        public async Task<IActionResult> List(string recordTime)
        {
            var model = new BalanceSheetListModel();
            var dateList = new List<SelectListItem>();

            model.Dates = BindDatesSelection(_closingAccountsService.GetAll, curStore?.Id);

            if (string.IsNullOrEmpty(recordTime))
            {
                var defaultDate = model.Dates.FirstOrDefault();
                model.RecordTime = defaultDate?.Value;
            }

            if (string.IsNullOrEmpty(model.RecordTime))
            {
                model.RecordTime = DateTime.Now.ToString("yyyy-MM-dd");
            }

            //获取当期资产负债
            var balancesheets = _ledgerDetailsService.GetBalanceSheets(curStore.Id, DateTime.Parse(model.RecordTime)).Select(x => x).ToList();

            model = await Task.Run(() =>
            {
                model.AssetsTrees = new BalanceSheetTotalModel
                {
                    Trees = _accountingService.PaseOptionsTree(curStore?.Id ?? 0, 1, DateTime.Parse(model.RecordTime), balancesheets),
                    TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 1).Sum(x => x.EndBalance),
                    TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 1).Sum(x => x.InitialBalance)
                };

                model.LiabilitiesTrees = new BalanceSheetTotalModel
                {
                    Trees = _accountingService.PaseOptionsTree(curStore?.Id ?? 0, 2, DateTime.Parse(model.RecordTime), balancesheets),
                    TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 2).Sum(x => x.EndBalance),
                    TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 2).Sum(x => x.InitialBalance)
                };

                model.EquitiesTrees = new BalanceSheetTotalModel
                {
                    Trees = _accountingService.PaseOptionsTree(curStore?.Id ?? 0, 3, DateTime.Parse(model.RecordTime), balancesheets),
                    TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 3).Sum(x => x.EndBalance),
                    TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 3).Sum(x => x.InitialBalance)
                };

                return model;
            });

            return View(model);
        }


        //用于资产负债表导出
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.BalanceSheetExport)]

        public FileResult ExportBalanceSheet(string recordTime)
        {

            #region 查询导出数据

            var balancesheets = _ledgerDetailsService
                .GetBalanceSheets(curStore.Id, DateTime.Parse(recordTime ?? DateTime.Now.ToShortDateString()))
                .Select(x => x.ToModel<BalanceSheetModel>()).ToList();
            if (balancesheets != null && balancesheets.Count > 0)
            {
                var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, balancesheets.Select(tb => tb.AccountingOptionId).ToArray());

                balancesheets.ForEach(tr =>
                {
                    var accountingOption = allAccountingOptions.Where(ao => ao.Id == tr.AccountingOptionId).FirstOrDefault();
                    tr.AccountingOptionName = (accountingOption != null) ? accountingOption.Name : "";
                    tr.AccountingOptionCode = (accountingOption != null) ? accountingOption.Code : "";
                });
            }

            var data = balancesheets.Select(tb =>
            {
                var entity = tb.ToEntity<BalanceSheetExport>();
                switch (entity.AccountingTypeId)
                {
                    case 1:
                        entity.AccountingTypeName = "资产";
                        break;
                    case 2:
                        entity.AccountingTypeName = "负债";
                        break;
                    case 3:
                        entity.AccountingTypeName = "权益";
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
            var ms = _exportManager.ExportBalanceSheetToXlsx(data);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "资产负债表.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "资产负债表.xlsx");
            }
            #endregion

        }


    }
}