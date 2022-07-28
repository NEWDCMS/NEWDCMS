using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;

namespace DCMS.Services
{

    /// <summary>
    /// 用于凭证创建接口
    /// </summary>
    public interface IVoucherService
    {
        BaseResult CreateVoucher<T, T1>(T bill, int storeId, int makeUserId, Action<int> update, Func<BaseResult> successful, Func<BaseResult> failed) where T : BaseBill<T1> where T1 : BaseEntity;
        bool CancleVoucher<T, T1>(T bill, Action update) where T : BaseBill<T1> where T1 : BaseEntity;
        bool CreateCostVoucher(CostOfSettleEnum type, int storeId, int makeUserId, AccountingCodeEnum Debit, AccountingCodeEnum credit, decimal? debitAmount, decimal? creditAmount, ClosingAccounts period, bool reserve = false);
    }

    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {
        protected readonly IServiceGetter _getter;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IStaticCacheManager _cacheManager;

        public BaseService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher
            )
        {
            _getter = getter;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
        }

        private readonly string AUTH = "AUTH";
        private readonly string CENSUS = "Census";
        private readonly string DCMS = "DCMS";
        private readonly string SKD = "SKD";
        private readonly string CRM = "CRM";
        private readonly string TSS = "TSS";
        private readonly string OCMS = "OCMS";
        private readonly string CSMS = "CSMS";

        protected IUnitOfWork AUTH_UOW => _getter.UOW(AUTH);
        protected IUnitOfWork CENSUS_UOW => _getter.UOW(CENSUS);
        protected IUnitOfWork DCMS_UOW => _getter.UOW(DCMS);
        protected IUnitOfWork SKD_UOW => _getter.UOW(SKD);
        protected IUnitOfWork CRM_UOW => _getter.UOW(CRM);
        protected IUnitOfWork TSS_UOW => _getter.UOW(TSS);
        protected IUnitOfWork OCMS_UOW => _getter.UOW(OCMS);
        protected IUnitOfWork CSMS_UOW => _getter.UOW(CSMS);

    }

}
