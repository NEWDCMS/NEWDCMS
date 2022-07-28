using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    public class ClosingAccountsService : BaseService, IClosingAccountsService
    {
        public ClosingAccountsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher) { }


        #region 期末结账

        public IList<ClosingAccounts> GetAll(int? storeId)
        {
            var query = ClosingAccountsRepository.Table;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            return query.ToList();
        }
        public ClosingAccounts GetClosingAccountsById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return ClosingAccountsRepository.ToCachedGetById(id);
        }
        public IPagedList<ClosingAccounts> GetClosingAccounts(int? storeId, DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ClosingAccountsRepository.Table;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (date.HasValue)
            {
                query = query.Where(t => (t.ClosingAccountDate.Year == date.Value.Year && t.ClosingAccountDate.Month == date.Value.Month));
            }


            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ClosingAccounts>(plists, pageIndex, pageSize, totalCount);
        }
        public bool CheckClosingAccounts(int? storeId, DateTime? date = null)
        {
            var query = ClosingAccountsRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && (c.ClosingAccountDate.Year == date.Value.Year && c.ClosingAccountDate.Month == date.Value.Month) && c.LockStatus == true && c.CheckStatus == true);
            return query.ToList().Count > 0;
        }
        public void InsertClosingAccounts(ClosingAccounts closingAccounts)
        {
            if (closingAccounts == null)
            {
                throw new ArgumentNullException("closingAccounts");
            }

            var uow = ClosingAccountsRepository.UnitOfWork;
            ClosingAccountsRepository.Insert(closingAccounts);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(closingAccounts);
        }
        public void DeleteClosingAccounts(ClosingAccounts closingAccounts)
        {
            if (closingAccounts == null)
            {
                throw new ArgumentNullException("closingAccounts");
            }

            var uow = ClosingAccountsRepository.UnitOfWork;
            ClosingAccountsRepository.Delete(closingAccounts);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(closingAccounts);
        }
        public void UpdateClosingAccounts(ClosingAccounts closingAccounts)
        {
            if (closingAccounts == null)
            {
                throw new ArgumentNullException("closingAccounts");
            }

            var uow = ClosingAccountsRepository.UnitOfWork;
            ClosingAccountsRepository.Update(closingAccounts);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(closingAccounts);
        }


        /// <summary>
        /// 获取当期结转
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public ClosingAccounts GetClosingAccountsByPeriod(int? storeId, DateTime? date = null)
        {
            if (!storeId.HasValue)
            {
                return null;
            }

            if (!date.HasValue)
            {
                return null;
            }

            var query = ClosingAccountsRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId);
            query = query.Where(t => (t.ClosingAccountDate.Year == date.Value.Year && t.ClosingAccountDate.Month == date.Value.Month));
            var closingAccounts = query.FirstOrDefault();

            if (closingAccounts == null)
            {
                return new ClosingAccounts() { ClosingAccountDate = date.Value, CheckStatus = false, LockStatus = false };
            }
            else
            {
                return closingAccounts;
            }
        }


        public bool IsClosed(int? storeId, DateTime? date = null)
        {
            var acc = GetClosingAccountsByPeriod(storeId, date);
            return acc.LockStatus == true && acc.CheckStatus == true;
        }

        public bool IsLocked(int? storeId, DateTime? date = null)
        {
            var acc = GetClosingAccountsByPeriod(storeId, date);
            return acc.LockStatus == true;
        }


        /// <summary>
        /// 取得参数区间的上个期间
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public ClosingAccounts ComputeLastPeriod(int? storeId, ClosingAccounts period = null)
        {
            if (period == null)
            {
                return null;
            }

            var current = GetClosingAccountsByPeriod(storeId, period.ClosingAccountDate.AddMonths(-1));
            if (current != null)
            {
                return current;
            }
            else
            {

                return new ClosingAccounts
                {
                    ClosingAccountDate = period.ClosingAccountDate.AddMonths(-1),
                    LockStatus = false,
                    CheckUserId = period.CheckUserId,
                    CheckStatus = false,
                    CheckDate = null
                };
            }

        }

        /// <summary>
        /// 取得输入期间的下一个期间
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public ClosingAccounts ComputeNextPeriod(int? storeId, ClosingAccounts period = null)
        {
            if (period == null)
            {
                return null;
            }

            var current = GetClosingAccountsByPeriod(storeId, period.ClosingAccountDate.AddMonths(1));
            if (current != null)
            {
                return current;
            }
            else
            {
                return new ClosingAccounts
                {
                    ClosingAccountDate = period.ClosingAccountDate.AddMonths(1),
                    LockStatus = false,
                    CheckUserId = period.CheckUserId,
                    CheckStatus = false,
                    CheckDate = null
                };
            }

        }


        #endregion

        #region 成本汇总表


        public IList<CostPriceSummery> GetCostPriceSummeries(int? storeId)
        {
            var query = CostPriceSummeryRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            return query.ToList();
        }
        public CostPriceSummery CostCostPriceSummery(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return CostPriceSummeryRepository.ToCachedGetById(id);
        }
        public IPagedList<CostPriceSummery> GetCostPriceSummeries(int? storeId, int? productId = 0, string productName = "", DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = CostPriceSummeryRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.Date == date);
            }

            if (productId > 0)
            {
                query = query.Where(t => t.ProductId == productId);
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(t => t.ProductName.Contains(productName));
            }
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CostPriceSummery>(plists, pageIndex, pageSize, totalCount);
        }

        public IList<CostPriceSummery> ExportCostPriceSummery(int? storeId, int? productId = 0, string productName = "", DateTime? date = null)
        {
            var query = CostPriceSummeryRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.Date == date);
            }

            if (productId > 0)
            {
                query = query.Where(t => t.ProductId == productId);
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(t => t.ProductName.Contains(productName));
            }
            return query.ToList();
        }

        public void InsertCostPriceSummery(CostPriceSummery costPriceSummery)
        {
            if (costPriceSummery == null)
            {
                throw new ArgumentNullException("costPriceSummery");
            }

            var uow = CostPriceSummeryRepository.UnitOfWork;
            CostPriceSummeryRepository.Insert(costPriceSummery);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(costPriceSummery);
        }
        public void DeleteCostPriceSummery(CostPriceSummery costPriceSummery)
        {
            if (costPriceSummery == null)
            {
                throw new ArgumentNullException("costPriceSummery");
            }

            var uow = CostPriceSummeryRepository.UnitOfWork;
            CostPriceSummeryRepository.Delete(costPriceSummery);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(costPriceSummery);
        }
        public void UpdateCostPriceSummery(CostPriceSummery costPriceSummery)
        {
            if (costPriceSummery == null)
            {
                throw new ArgumentNullException("costPriceSummery");
            }

            var uow = CostPriceSummeryRepository.UnitOfWork;
            CostPriceSummeryRepository.Update(costPriceSummery);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(costPriceSummery);
        }

        /// <summary>
        /// 根据商品获取指定当期成本变化表（低效率）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<CostPriceSummery> GetCostPriceSummeriesByProductId(int? storeId, DateTime period, int productId)
        {
            var query = CostPriceSummeryRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.ProductId == productId && c.Date == period);
            return query.ToList();
        }


        public CostPriceSummery GetCostPriceSummeriesByProductId(int? storeId, DateTime period, int productId, int unitId)
        {
            var query = CostPriceSummeryRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.ProductId == productId && (c.Date.Year == period.Year && c.Date.Month == period.Month && c.Date.Day == period.Day) && c.UnitId == unitId);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 获取指定当期成本变化汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<CostPriceSummery> GetCostPriceSummeries(int? storeId, DateTime period)
        {
            var query = CostPriceSummeryRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && c.Date == period);
            return query.ToList();
        }

        /// <summary>
        /// 获取指定上期成本变化汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<CostPriceSummery> GetLastCostPriceSummeries(int? storeId, DateTime period)
        {
            var last = period.AddMonths(-1).AddDays(1 - period.AddMonths(-1).Day).AddMonths(1).AddDays(-1);
            var query = CostPriceSummeryRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId && (c.Date.Year == period.Year && c.Date.Month == last.Month && c.Date.Day == last.Day));
            return query.ToList();
        }



        public void DeleteCostPriceSummery(int? storeId, DateTime period)
        {
            var cprs = CostPriceSummeryRepository.TableNoTracking.Where(c => c.StoreId == storeId && c.Date == period);
            var ids = cprs.Select(s => s.Id).ToList();

            var uow = CostPriceSummeryRepository.UnitOfWork;

            CostPriceSummeryRepository.Delete(cprs.ToList());

            var cprrs = CostPriceChangeRecordsRepository.TableNoTracking.Where(c => c.StoreId == storeId && ids.Contains(c.CostPriceSummeryId));

            CostPriceChangeRecordsRepository.Delete(cprrs);

            uow.SaveChanges();
            //通知
            cprs.ToList().ForEach(s => { _eventPublisher.EntityDeleted(s); });
        }

        #endregion

        #region 成本变化明细记录


        public IList<CostPriceChangeRecords> GetChangeRecords(int? storeId)
        {
            var query = CostPriceChangeRecordsRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            return query.ToList();
        }
        public CostPriceChangeRecords CostPriceChangeRecords(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return CostPriceChangeRecordsRepository.ToCachedGetById(id);
        }


        public CostPriceChangeRecords CostPriceChangeRecords(int storeId, DateTime createdOnUtc, int billTypeId, int billId, int productId, int unitId)
        {
            var query = CostPriceChangeRecordsRepository.TableNoTracking;
            query = query.Where(c => c.StoreId == storeId);
            query = query.Where(c => c.CreatedOnUtc == createdOnUtc);
            query = query.Where(c => c.BillTypeId == billTypeId);
            query = query.Where(c => c.BillId == billId);
            query = query.Where(c => c.ProductId == productId);
            query = query.Where(c => c.UnitId == unitId);
            return query.FirstOrDefault();
        }


        public IPagedList<CostPriceChangeRecords> GetCostPriceChangeRecordss(int? storeId, int? costPriceSummeryId = 0, DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = CostPriceChangeRecordsRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (!date.HasValue)
            {
                query = query.Where(t => t.Date == date);
            }

            if (costPriceSummeryId > 0)
            {
                query = query.Where(t => t.CostPriceSummeryId == costPriceSummeryId);
            }


            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CostPriceChangeRecords>(plists, pageIndex, pageSize, totalCount);
        }

        public IList<CostPriceChangeRecords> ExportGetCostPriceChangeRecordss(int? storeId, int? costPriceSummeryId = 0, DateTime? date = null)
        {
            var query = CostPriceChangeRecordsRepository.TableNoTracking;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (!date.HasValue)
            {
                query = query.Where(t => t.Date == date);
            }

            if (costPriceSummeryId > 0)
            {
                query = query.Where(t => t.CostPriceSummeryId == costPriceSummeryId);
            }
            return query.ToList();
        }
        public void InsertCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords)
        {
            if (costPriceChangeRecords == null)
            {
                throw new ArgumentNullException("costPriceChangeRecords");
            }

            var uow = CostPriceChangeRecordsRepository.UnitOfWork;
            CostPriceChangeRecordsRepository.Insert(costPriceChangeRecords);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(costPriceChangeRecords);
        }

        public void InsertCostPriceChangeRecords(List<CostPriceChangeRecords> costPriceChangeRecords)
        {
            if (costPriceChangeRecords == null)
            {
                throw new ArgumentNullException("costPriceChangeRecords");
            }

            var uow = CostPriceChangeRecordsRepository.UnitOfWork;
            CostPriceChangeRecordsRepository.Insert(costPriceChangeRecords);
            uow.SaveChanges();

            //通知
            costPriceChangeRecords.ForEach(s =>
            {
                _eventPublisher.EntityInserted(s);
            });
        }


        public void DeleteCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords)
        {
            if (costPriceChangeRecords == null)
            {
                throw new ArgumentNullException("costPriceChangeRecords");
            }

            var uow = CostPriceChangeRecordsRepository.UnitOfWork;
            CostPriceChangeRecordsRepository.Delete(costPriceChangeRecords);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(costPriceChangeRecords);
        }
        public void UpdateCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords)
        {
            if (costPriceChangeRecords == null)
            {
                throw new ArgumentNullException("costPriceChangeRecords");
            }

            var uow = CostPriceChangeRecordsRepository.UnitOfWork;
            CostPriceChangeRecordsRepository.Update(costPriceChangeRecords);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(costPriceChangeRecords);
        }

        #endregion

    }
}
