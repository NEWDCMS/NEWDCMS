using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Finances
{
    public partial class LedgerDetailsService : BaseService, ILedgerDetailsService
    {

        private readonly IAccountingService _accountingService;

        public LedgerDetailsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IAccountingService accountingService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _accountingService = accountingService;
        }

        #region 期间明细账

        /// <summary>
        /// 获取明细账
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountsIds"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public IList<VoucherItem> GetLedgerDetailsByOptions(int? storeId, int[] accountsIds, DateTime? first, DateTime? last)
        {
            if (!first.HasValue || !first.HasValue)
            {
                return null;
            }

            DateTime.TryParse(first.Value.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
            DateTime.TryParse(last.Value.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

            var query = from pc in VoucherItemsRepository.TableNoTracking
                        join vc in RecordingVouchersRepository.TableNoTracking on pc.RecordingVoucherId equals vc.Id
                        join aop in AccountingOptionsRepository.TableNoTracking on pc.AccountingOptionId equals aop.Id
                        where pc.StoreId == storeId
                        && (accountsIds.Contains(pc.AccountingOptionId) || accountsIds.Contains(aop.ParentId ?? 0))
                        && pc.RecordTime >= start
                        && pc.RecordTime <= end
                        orderby pc.Id
                        select new VoucherItem
                        {
                            RecordingVoucherId = pc.RecordingVoucherId,
                            Summary = pc.Summary,
                            AccountingOptionId = pc.AccountingOptionId,
                            AccountingOptionName = pc.AccountingOptionName,
                            DebitAmount = pc.DebitAmount,
                            CreditAmount = pc.CreditAmount,
                            RecordTime = pc.RecordTime,
                            RecordingVoucher = vc
                        };

            return query.ToList();
        }

        #endregion


        #region 资产负债表

        /// <summary>
        /// 获取资产负债表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="AccountOptionId"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        public IList<BalanceSheet> GetBalanceSheets(int? store, DateTime? recordTime)
        {
            var query = BalanceSheetsRepository.TableNoTracking.Where(s => s.StoreId == store && (s.PeriodDate.Year == recordTime.Value.Year && s.PeriodDate.Month == recordTime.Value.Month));
            return query.ToList();
        }

        public void InsertBalanceSheets(IList<BalanceSheet> balanceSheets)
        {

            if (balanceSheets == null)
            {
                throw new ArgumentNullException("balanceSheets");
            }

            var uow = BalanceSheetsRepository.UnitOfWork;

            BalanceSheetsRepository.Insert(balanceSheets);

            uow.SaveChanges();

            balanceSheets.ToList().ForEach(s =>
            {
                //通知
                _eventPublisher.EntityInserted(s);
            });
        }


        public void InsertBalanceSheet(BalanceSheet balanceSheet)
        {

            if (balanceSheet == null)
            {
                throw new ArgumentNullException("balanceSheet");
            }

            var uow = BalanceSheetsRepository.UnitOfWork;

            BalanceSheetsRepository.Insert(balanceSheet);

            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(balanceSheet);
        }



        public void UpdateBalanceSheet(BalanceSheet balanceSheet)
        {
            if (balanceSheet == null)
            {
                throw new ArgumentNullException("balanceSheet");
            }

            var uow = BalanceSheetsRepository.UnitOfWork;
            BalanceSheetsRepository.Update(balanceSheet);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(balanceSheet);
        }

        public BalanceSheet FindBalanceSheet(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate)
        {
            if (!storeId.HasValue || !PeriodDate.HasValue)
            {
                return null;
            }

            if (accountingTypeId == 0)
            {
                return null;
            }

            if (accountingOptionId == 0)
            {
                return null;
            }

            var query = BalanceSheetsRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.AccountingTypeId == accountingTypeId && c.AccountingOptionId == accountingOptionId && c.PeriodDate.ToString("yyyy-MM") == PeriodDate.Value.ToString("yyyy-MM"));
            return query.FirstOrDefault();
        }
        #endregion



        #region 本年利润表

        /// <summary>
        /// 获取本年利润表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="AccountOptionId"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        public IList<ProfitSheet> GetProfitSheets(int? store, DateTime? recordTime)
        {
            var sheets = new List<ProfitSheet>();
            var query = ProfitSheetsRepository.TableNoTracking.Where(s => s.StoreId == store && (s.PeriodDate.Year == recordTime.Value.Year && s.PeriodDate.Month == recordTime.Value.Month));
            return query.ToList();
        }

        public void InsertProfitSheets(IList<ProfitSheet> profitSheets)
        {
            if (profitSheets == null)
            {
                throw new ArgumentNullException("profitSheets");
            }

            var uow = ProfitSheetsRepository.UnitOfWork;
            ProfitSheetsRepository.Insert(profitSheets);
            uow.SaveChanges();
            profitSheets.ToList().ForEach(s =>
            {
                //通知
                _eventPublisher.EntityInserted(s);
            });
        }


        public void InsertProfitSheet(ProfitSheet profitSheet)
        {
            if (profitSheet == null)
            {
                throw new ArgumentNullException("profitSheet");
            }

            var uow = ProfitSheetsRepository.UnitOfWork;

            ProfitSheetsRepository.Insert(profitSheet);

            uow.SaveChanges();
        }


        public void UpdateProfitSheet(ProfitSheet profitSheet)
        {
            if (profitSheet == null)
            {
                throw new ArgumentNullException("profitSheet");
            }
            var uow = ProfitSheetsRepository.UnitOfWork;
            ProfitSheetsRepository.Update(profitSheet);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(profitSheet);
        }


        public ProfitSheet FindProfitSheet(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate)
        {
            if (!storeId.HasValue || !PeriodDate.HasValue)
            {
                return null;
            }

            if (accountingTypeId == 0)
            {
                return null;
            }

            if (accountingOptionId == 0)
            {
                return null;
            }

            var query = ProfitSheetsRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.AccountingTypeId == accountingTypeId && c.AccountingOptionId == accountingOptionId && c.PeriodDate.ToString("yyyy-MM") == PeriodDate.Value.ToString("yyyy-MM"));
            return query.FirstOrDefault();
        }
        #endregion


    }
}
