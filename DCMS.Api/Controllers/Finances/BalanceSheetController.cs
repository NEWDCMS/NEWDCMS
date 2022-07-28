using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.Finances;
using DCMS.Services.Settings;
using DCMS.ViewModel.Models.Finances;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;


namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于资产负债表
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class BalanceSheetController : BaseAPIController
    {
        private readonly IAccountingService _accountingService;

        private readonly ILedgerDetailsService _ledgerDetailsService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="accountingService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="ledgerDetailsService"></param>
        public BalanceSheetController(
            IAccountingService accountingService,
             ILedgerDetailsService ledgerDetailsService,
             IStaticCacheManager cacheManager, ILogger<BaseAPIController> logger) : base(logger)
        {
            _accountingService = accountingService;

            _ledgerDetailsService = ledgerDetailsService;
        }

        /// <summary>
        /// 获取资产负债表数据
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("balancesheet/getbills/{store}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<BalanceSheetListModel>> AsyncList(int? store, string recordTime, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<BalanceSheetListModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new BalanceSheetListModel();
                try
                {
                    if (string.IsNullOrEmpty(recordTime))
                    {
                        var defaultDate = model.Dates.FirstOrDefault();
                        model.RecordTime = defaultDate?.Value;
                    }

                    if (string.IsNullOrEmpty(model.RecordTime))
                        model.RecordTime = DateTime.Now.ToString("yyyy-MM-dd");

                    //获取当期资产负债
                    var balancesheets = _ledgerDetailsService.GetBalanceSheets(store ?? 0, DateTime.Parse(model.RecordTime)).Select(x => x).ToList();

                    model.AssetsTrees = new BalanceSheetTotalModel
                    {
                        Trees = _accountingService.PaseOptionsTree(store ?? 0, 1, DateTime.Parse(model.RecordTime), balancesheets),
                        TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 1).Sum(x => x.EndBalance),
                        TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 1).Sum(x => x.InitialBalance)
                    };

                    model.LiabilitiesTrees = new BalanceSheetTotalModel
                    {
                        Trees = _accountingService.PaseOptionsTree(store ?? 0, 2, DateTime.Parse(model.RecordTime), balancesheets),
                        TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 2).Sum(x => x.EndBalance),
                        TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 2).Sum(x => x.InitialBalance)
                    };

                    model.EquitiesTrees = new BalanceSheetTotalModel
                    {
                        Trees = _accountingService.PaseOptionsTree(store ?? 0, 3, DateTime.Parse(model.RecordTime), balancesheets),
                        TotalEndBalance = balancesheets.Where(x => x.AccountingTypeId == 3).Sum(x => x.EndBalance),
                        TotalInitialBalance = balancesheets.Where(x => x.AccountingTypeId == 3).Sum(x => x.InitialBalance)
                    };
                    return this.Successful3("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<BalanceSheetListModel>(ex.Message);
                }
            });
        }
    }
}