using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;

namespace DCMS.Services.Tax
{

    public partial class TaxService : BaseService, ITaxService
    {
        private readonly ISettingService _settingService;

        public TaxService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            ISettingService settingService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _settingService = settingService;
        }


        /// <summary>
        /// 获取进项税额
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="amount">金额</param>
        /// <returns></returns>
        public decimal GetTaxRateAmount(int? store, decimal amount)
        {
            if (store == null || store == 0)
            {
                return 0;
            }
            //启用税务功能
            CompanySetting companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
            if (companySetting != null && companySetting.EnableTaxRate && companySetting.TaxRate > 0)
            {
                decimal.TryParse(companySetting.TaxRate.ToString(), out decimal deTaxRate);
                return amount * deTaxRate / 100;
            }
            else
            {
                return 0;
            }
        }
    }
}