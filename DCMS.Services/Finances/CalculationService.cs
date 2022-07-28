using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;

namespace DCMS.Services.Finances
{

    /// <summary>
    /// 用于财务账目计算服务
    /// </summary>
    public partial class CalculationService : BaseService, ICalculationService
    {

        public CalculationService(IServiceGetter getter,
         IStaticCacheManager cacheManager,
         IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        /// <summary>
        /// 获取当前客户的总欠款额度
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public decimal GetCustomerOweCash(int? customerId)
        {
            decimal totalOweCashAmount = decimal.Zero;
            //这里实现业务...
            return totalOweCashAmount;
        }
    }
}
