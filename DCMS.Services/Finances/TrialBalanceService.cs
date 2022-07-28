using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.Finances
{
    public class TrialBalanceService : BaseService, ITrialBalanceService
    {
        public TrialBalanceService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }

        public IList<TrialBalance> GetAll(int? storeId)
        {
            var query = TrialBalancesRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            return query.ToList();
        }


        public TrialBalance FindTrialBalance(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate)
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

            var query = TrialBalancesRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.AccountingTypeId == accountingTypeId && c.AccountingOptionId == accountingOptionId && c.PeriodDate.ToString("yyyy-MM") == PeriodDate.Value.ToString("yyyy-MM"));
            return query.FirstOrDefault();
        }

        public TrialBalance GetTrialBalanceById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return TrialBalancesRepository.ToCachedGetById(id);
        }


        public IPagedList<TrialBalance> GetTrialBalances(int? storeId, int? accountOptionId = 0, int? accountTypeId = 0, DateTime? periodTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = TrialBalancesRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (accountOptionId > 0)
            {
                query = query.Where(t => t.AccountingOptionId == accountOptionId);
            }

            if (accountTypeId > 0)
            {
                query = query.Where(t => t.AccountingTypeId == accountTypeId);
            }

            if (periodTime != null)
            {
                query = query.Where(t => t.PeriodDate == periodTime);
            }

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<TrialBalance>(plists, pageIndex, pageSize, totalCount);
        }

        public IList<TrialBalance> GetTrialBalanceByIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<TrialBalance>();
            }

            var query = from c in TrialBalancesRepository.Table
                        where idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();
            return list;
        }

        public void InsertTrialBalance(TrialBalance trialBalance)
        {
            if (trialBalance == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = TrialBalancesRepository.UnitOfWork;
            TrialBalancesRepository.Insert(trialBalance);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(trialBalance);
        }

        public void InsertTrialBalances(IList<TrialBalance> trialBalances)
        {
            if (trialBalances == null)
            {
                throw new ArgumentNullException("trialBalances");
            }
            var uow = TrialBalancesRepository.UnitOfWork;
            TrialBalancesRepository.Insert(trialBalances);
            uow.SaveChanges();

        }


        public void DeleteTrialBalance(TrialBalance trialBalance)
        {
            if (trialBalance == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = TrialBalancesRepository.UnitOfWork;
            TrialBalancesRepository.Delete(trialBalance);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(trialBalance);
        }


        public void UpdateTrialBalance(TrialBalance trialBalance)
        {
            if (trialBalance == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = TrialBalancesRepository.UnitOfWork;
            TrialBalancesRepository.Update(trialBalance);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(trialBalance);
        }

        public void UpdateTrialBalances(IList<TrialBalance> trialBalances)
        {
            if (trialBalances == null)
            {
                throw new ArgumentNullException("trialBalances");
            }

            var uow = TrialBalancesRepository.UnitOfWork;
            TrialBalancesRepository.Update(trialBalances);
            uow.SaveChanges();

        }


        public TrialBalance GetTrialBalance(int store, int accountingTypeId, int accountingOptionId, DateTime periodDate)
        {
            var trialBalance = FindTrialBalance(store, accountingTypeId, accountingOptionId, periodDate);
            if (trialBalance != null)
            {
                return trialBalance;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取指定科目的年初余额
        /// </summary>
        /// <param name="store"></param>
        /// <param name="accountingTypeId"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="periodDate"></param>
        /// <returns></returns>
        public TrialBalance GetYearBegainTrialBalance(int? store, int accountingOptionId, DateTime? periodDate)
        {
            if (!store.HasValue || store.Value == 0 || !periodDate.HasValue)
            {
                return null;
            }

            if (accountingOptionId == 0)
            {
                return null;
            }

            var lastYearEndMonth = periodDate.Value
                .AddYears(-1)
                .AddMonths(12 - periodDate.Value.Month)
                .AddDays(1 - periodDate.Value.Day)
                .AddMonths(1)
                .AddDays(-1);

            var query = TrialBalancesRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == store && c.AccountingOptionId == accountingOptionId && c.PeriodDate.ToString("yyyy-MM") == lastYearEndMonth.ToString("yyyy-MM"));
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 获取科目余额表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="AccountOptionId"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        public IList<TrialBalance> GetTrialBalances(int? store, int? accountOptionId, DateTime? recordTime)
        {
            if (store == 0)
            {
                return null;
            }

            var query = TrialBalancesRepository.TableNoTracking;

            if (accountOptionId.HasValue && accountOptionId.Value != 0)
            {
                query = query.Where(s => s.AccountingOptionId == accountOptionId);
            }

            if (recordTime.HasValue)
            {
                query = query.Where(s => s.PeriodDate.Year == recordTime.Value.Year && s.PeriodDate.Month == recordTime.Value.Month);
            }

            return query.ToList();
        }

        /// <summary>
        /// 取得指定期间的科目余额,不存在则构建初始期间科目余额
        /// </summary>
        /// <param name="store"></param>
        /// <param name="AccountOptionId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<TrialBalance> TryGetTrialBalances(int? storeId, DateTime? period)
        {
            if (!storeId.HasValue || !period.HasValue)
            {
                return null;
            }

            var trialbalances = new List<TrialBalance>();
            var accountingOptions = new List<AccountingOption>();

            //获取科目
            var accTypeQuery = AccountingTypesRepository.TableNoTracking.Where(s => s.StoreId == 0 && s.DisplayOrder == 0).ToList();
            if (accTypeQuery != null)
            {
                var types = accTypeQuery.Select(s => s.Id).ToArray();
                if (types != null)
                {
                    accountingOptions = AccountingOptionsRepository.TableNoTracking.Where(c => c.StoreId == storeId.Value && types.Contains(c.AccountingTypeId)).ToList();
                }
            }

            trialbalances = TrialBalancesRepository.TableNoTracking.Where(c => c.StoreId == storeId && c.PeriodDate.ToString("yyyy-MM-dd") == period.Value.ToString("yyyy-MM-dd")).ToList();
            if (trialbalances != null)
            {
                trialbalances.ForEach(b =>
                {
                    b.AccountingOption = AccountingOptionsRepository.ToCachedGetById(b.AccountingOptionId);
                    //根据科目的类型，判断余额方向是借方或者贷方
                    b.DirectionsType = CommonHelper.GetAccountingDirections(b.AccountingTypeId);
                });

                //新增加科目
                var newLines = new List<TrialBalance>();
                accountingOptions.ForEach(acc =>
                {
                    if (!trialbalances.Select(s => s.AccountingOptionId).Contains(acc.Id))
                    {
                        newLines.Add(new TrialBalance()
                        {
                            Id = 0,
                            StoreId = storeId ?? 0,
                            //关联科目
                            AccountingOption = acc,
                            DirectionsType = CommonHelper.GetAccountingDirections(acc.AccountingTypeId),
                            AccountingTypeId = acc.AccountingTypeId,
                            AccountingOptionId = acc.Id,
                            AccountingOptionName = acc.Name,
                            PeriodDate = period.Value,

                            InitialBalanceDebit = 0,
                            InitialBalanceCredit = 0,

                            PeriodBalanceDebit = 0,
                            PeriodBalanceCredit = 0,

                            EndBalanceDebit = 0,
                            EndBalanceCredit = 0,

                            CreatedOnUtc = DateTime.Now
                        });
                    }
                });

                //如果有新增加的科目则追加
                if (newLines.Any())
                {
                    trialbalances.AddRange(newLines);
                }

                return trialbalances;
            }
            else
            {
                //创建当期
                accountingOptions.ForEach(acc =>
                {
                    var trialbalance = new TrialBalance()
                    {
                        Id = 0,
                        StoreId = storeId ?? 0,
                        //关联科目
                        AccountingOption = acc,
                        DirectionsType = CommonHelper.GetAccountingDirections(acc.AccountingTypeId),
                        AccountingTypeId = acc.AccountingTypeId,
                        AccountingOptionId = acc.Id,
                        AccountingOptionName = acc.Name,
                        PeriodDate = period.Value,

                        InitialBalanceDebit = 0,
                        InitialBalanceCredit = 0,

                        PeriodBalanceDebit = 0,
                        PeriodBalanceCredit = 0,

                        EndBalanceDebit = 0,
                        EndBalanceCredit = 0,

                        CreatedOnUtc = DateTime.Now
                    };
                    trialbalances.Add(trialbalance);
                });

                return trialbalances;
            }
        }


        private List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.ParentId == pid && c.StoreId == store.Value);
            return query.ToList();
        }

        public bool HasChilds(int accountingOptionId)
        {
            var query = from c in AccountingOptionsRepository.Table where c.ParentId == accountingOptionId select c;
            return query.ToList().Count > 0;
        }



    }
}
