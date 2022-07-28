using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Settings;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 用于表示记账凭证服务
    /// </summary>
    public partial class RecordingVoucherService : BaseService, IRecordingVoucherService
    {

        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IAccountingService _accountingService;

        public RecordingVoucherService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IAccountingService accountingService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _accountingService = accountingService;
        }

        #region 单据


        public virtual IPagedList<RecordingVoucher> GetAllRecordingVouchers(int? store, int? makeuserId, int? generateMode, string billNumber = "", string summary = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? billTypeId = null, string recordName = "", int? accountingOptionId = null, int pageIndex = 0, int pageSize = 30)
        {
            if (pageSize >= 50)
                pageSize = 50;
            DateTime.TryParse(start.Value.ToString("yyyy-MM-dd 00:00:00"), out DateTime first);
            DateTime.TryParse(end.Value.ToString("yyyy-MM-dd 23:59:59"), out DateTime last);

            var query = from rv in RecordingVouchersRepository.Table
                        .Include(r => r.Items)
                        select rv;

            if (store.HasValue)
            {
                query = query.Where(x => x.StoreId == store);
            }

            if (start.HasValue)
            {
                query = query.Where(x => x.RecordTime >= first);
            }

            if (end.HasValue)
            {
                query = query.Where(x => x.RecordTime <= last);
            }

            if (makeuserId.HasValue && makeuserId > 0)
            {
                query = query.Where(x => x.MakeUserId == makeuserId);
            }

            if (generateMode.HasValue)
            {
                query = query.Where(c => c.GenerateMode == generateMode);
            }

            if (!string.IsNullOrWhiteSpace(billNumber))
            {
                query = query.Where(c => c.BillNumber.Contains(billNumber));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.AuditedStatus == status);
            }

            if (billTypeId.HasValue && billTypeId > 0)
            {
                query = query.Where(t => t.BillTypeId == billTypeId);
            }

            if (!string.IsNullOrWhiteSpace(recordName))
            {
                //query = query.Where(t => (t.RecordName + "-" + t.RecordNumber).Contains(recordName));
                query = query.Where(t => recordName.Contains(t.RecordNumber.ToString()));
            }

            if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
            {
                query = query.Where(x => x.Items.Count(s => s.AccountingOptionId == accountingOptionId) > 0);
            }

            if (!string.IsNullOrWhiteSpace(summary))
            {
                query = query.Where(x => x.Items.Count(s => s.Summary.Contains(summary)) > 0);
            }

            query = query.OrderByDescending(c => c.RecordTime);

            var plists = query.ToList();

            //分页
            return new PagedList<RecordingVoucher>(plists, pageIndex, pageSize);
        }

        public virtual IList<RecordingVoucher> GetAllRecordingVouchers(int? store)
        {
            var query = from c in RecordingVouchersRepository.Table
                        where c.StoreId == store
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        /// <summary>
        /// 根据经销商、单据状态、单据类型获取单据凭证
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="deleteStatus"></param>
        /// <param name="billTypeEnum"></param>
        /// <returns></returns>
        public virtual IList<RecordingVoucher> GetAllRecordingVouchersByStoreIdBillType(int storeId, bool? deleteStatus, BillTypeEnum billTypeEnum)
        {
            var query = RecordingVouchersRepository.Table;

            //经销商
            query = query.Where(c => c.StoreId == storeId);

            //单据未删除
            if (deleteStatus == null || deleteStatus == false)
            {
                query = query.Where(c => c.Deleted == false);
            }
            //单据删除
            else
            {
                query = query.Where(c => c.Deleted == deleteStatus);
            }

            //单据类型
            query = query.Where(c => c.BillTypeId == (int)billTypeEnum);

            var categories = query.ToList();
            return categories;
        }

        public virtual RecordingVoucher GetRecordingVoucherById(int? store, int recordingVoucherId, bool isInclulude = false)
        {
            if (recordingVoucherId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.RECORDINGVOUCHER_BY_ID_KEY.FillCacheKey(store ?? 0, recordingVoucherId);
            return _cacheManager.Get(key, () => 
            {
                if (isInclulude)
                {
                    var query = RecordingVouchersRepository_RO.Table.Include(rv => rv.Items);
                    return query.FirstOrDefault(r => r.Id == recordingVoucherId);
                }

                return RecordingVouchersRepository.ToCachedGetById(recordingVoucherId); 
            });
        }

        public virtual RecordingVoucher GetRecordingVoucher(int storeId, int billTypeId, string billNumber)
        {
            if (storeId == 0 || billTypeId == 0 || string.IsNullOrEmpty(billNumber))
            {
                return null;
            }

            var query = RecordingVouchersRepository.Table;
            query = query.Where(a => a.StoreId == storeId);
            query = query.Where(a => a.BillTypeId == billTypeId);
            query = query.Where(a => a.BillNumber == billNumber);
            return query.FirstOrDefault();

        }

        public virtual List<RecordingVoucher> GetRecordingVouchers(int storeId, int billTypeId, string billNumber)
        {
            if (storeId == 0 || billTypeId == 0 || string.IsNullOrEmpty(billNumber))
            {
                return null;
            }

            var query = RecordingVouchersRepository.Table;
            query = query.Where(a => a.StoreId == storeId);
            query = query.Where(a => a.BillTypeId == billTypeId);
            query = query.Where(a => a.BillNumber == billNumber);
            return query.ToList();

        }

        public virtual void InsertRecordingVoucher(RecordingVoucher recordingVoucher)
        {
            if (recordingVoucher == null)
            {
                throw new ArgumentNullException("recordingVoucher");
            }

            var uow = RecordingVouchersRepository.UnitOfWork;
            RecordingVouchersRepository.Insert(recordingVoucher);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(recordingVoucher);
        }

        public virtual void UpdateRecordingVoucher(RecordingVoucher recordingVoucher)
        {
            if (recordingVoucher == null)
            {
                throw new ArgumentNullException("recordingVoucher");
            }

            var uow = RecordingVouchersRepository.UnitOfWork;
            RecordingVouchersRepository.Update(recordingVoucher);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(recordingVoucher);
        }

        public virtual void DeleteRecordingVoucher(RecordingVoucher recordingVoucher)
        {
            if (recordingVoucher == null)
            {
                throw new ArgumentNullException("recordingVoucher");
            }

            var uow = RecordingVouchersRepository.UnitOfWork;
            RecordingVouchersRepository.Delete(recordingVoucher);
            uow.SaveChanges();


            //event notification
            _eventPublisher.EntityDeleted(recordingVoucher);
        }


        public virtual void RollBackRecordingVoucher(RecordingVoucher recordingVoucher)
        {
            if (recordingVoucher != null)
            {
                DeleteRecordingVoucher(recordingVoucher);
                DeleteVoucherItemWithVoucher(recordingVoucher);
            }
        }




        public virtual void DeleteRecordingVoucherFromPeriod(int? storeId, DateTime? period, string billNumber)
        {
            var uow = RecordingVouchersRepository.UnitOfWork;
            var rvs = GetLikeRecordingVoucherFromPeriod(storeId, period, billNumber);
            var rvs_ids = rvs.Select(s => s.Id);

            var query = from pc in VoucherItemsRepository.Table
                        where rvs_ids.Contains(pc.RecordingVoucherId)
                        orderby pc.Id
                        select pc;
            var vis = query.ToList();

            if (vis != null && vis.Any())
            {
                VoucherItemsRepository.Delete(vis);
            }

            if (rvs != null && rvs.Any())
            {
                RecordingVouchersRepository.Delete(rvs);
            }

            if (vis != null && vis.Any() || rvs != null && rvs.Any())
            {
                uow.SaveChanges();

                //event notification
                rvs.ToList().ForEach(r =>
                {
                    _eventPublisher.EntityDeleted(r);
                });
            }
        }

        /// <summary>
        /// 获取指定期间的凭证
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<RecordingVoucher> GetRecordingVoucherFromPeriod(int? storeId, DateTime? period)
        {
            if (period.HasValue)
            {
                //当月第一天
                var first = period.Value.AddDays(1 - period.Value.Day);
                //当月最后一天
                var last = period.Value.AddDays(1 - period.Value.Day).AddMonths(1).AddDays(-1);

                DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            where rv.StoreId == storeId
                            && rv.RecordTime >= start
                            && rv.RecordTime <= end
                            orderby rv.Id
                            select rv;
                return query.ToList();
            }

            return null;
        }

        /// <summary>
        /// 获取指定期间的凭证
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="period"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IList<RecordingVoucher> GetLikeRecordingVoucherFromPeriod(int? storeId, DateTime? period, string billNumber)
        {
            if (period.HasValue)
            {
                //当月第一天
                var first = period.Value.AddDays(1 - period.Value.Day);
                //当月最后一天
                var last = period.Value.AddDays(1 - period.Value.Day).AddMonths(1).AddDays(-1);

                DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            where rv.StoreId == storeId
                            && rv.RecordTime >= start
                            && rv.RecordTime <= end
                            && rv.BillNumber.Contains(billNumber)
                            orderby rv.Id
                            select rv;
                return query.ToList();
            }

            return null;
        }


        #endregion

        #region 单据项目


        /// <summary>
        /// 获取指定科目的期初余额
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Tuple<decimal, decimal, decimal> GetInitiallBalance(int? storeId, int accountingOptionId, DateTime? first, DateTime? last, decimal balance)
        {
            if (!storeId.HasValue)
            {
                return new Tuple<decimal, decimal, decimal>(0, 0, 0);
            }

            if (accountingOptionId == 0)
            {
                return new Tuple<decimal, decimal, decimal>(0, 0, 0);
            }

            var query = from pc in VoucherItemsRepository.TableNoTracking
                        join aop in AccountingOptionsRepository.TableNoTracking on pc.AccountingOptionId equals aop.Id
                        where pc.StoreId == storeId && (accountingOptionId == pc.AccountingOptionId || accountingOptionId == (aop.ParentId ?? 0))
                        orderby pc.Id
                        select pc;

            if (first.HasValue)
            {
                DateTime.TryParse(first.Value.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                query = query.Where(c => c.RecordTime >= start);
            }

            if (last.HasValue)
            {
                DateTime.TryParse(last.Value.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);
                query = query.Where(c => c.RecordTime <= end);
            }

            var items = query.ToList();

            decimal balance_debit = 0;
            decimal balance_credit = 0;

            if (items != null && items.Any())
            {
                balance_debit = items.Sum(s => s.DebitAmount ?? 0);
                balance_credit = items.Sum(s => s.CreditAmount ?? 0);
                balance += balance_debit - balance_credit;
            }

            return new Tuple<decimal, decimal, decimal>(balance, balance_debit, balance_credit);
        }

        public virtual IPagedList<VoucherItem> GetVoucherItemsByRecordingVoucherId(int recordingVoucherId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (recordingVoucherId == 0)
            {
                return new PagedList<VoucherItem>(new List<VoucherItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.RECORDINGVOUCHERITEM_ALL_KEY.FillCacheKey(storeId, recordingVoucherId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in VoucherItemsRepository.Table
                            where pc.RecordingVoucherId == recordingVoucherId
                            orderby pc.Id
                            select pc;
                //var recordingVouchers = new PagedList<VoucherItem>(query.ToList(), pageIndex, pageSize);
                //return recordingVouchers;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<VoucherItem>(plists, pageIndex, pageSize, totalCount);
            });
        }


        public virtual IList<VoucherItem> GetVoucherItemsByRecordingVoucherId(int recordingVoucherId)
        {
            var query = from pc in VoucherItemsRepository.Table
                        where pc.RecordingVoucherId == recordingVoucherId
                        orderby pc.Id
                        select pc;

            return query.ToList();
        }

        public virtual VoucherItem GetVoucherItemById(int? store, int voucherItemId)
        {
            if (voucherItemId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.RECORDINGVOUCHERITEM_BY_ID_KEY.FillCacheKey(store ?? 0, voucherItemId);
            return _cacheManager.Get(key, () => { return VoucherItemsRepository.ToCachedGetById(voucherItemId); });
        }

        public virtual void InsertVoucherItem(VoucherItem voucherItem)
        {
            if (voucherItem == null)
            {
                throw new ArgumentNullException("voucherItem");
            }

            var uow = VoucherItemsRepository.UnitOfWork;

            VoucherItemsRepository.Insert(voucherItem);

            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(voucherItem);
        }

        public virtual void InsertVoucherItems(List<VoucherItem> voucherItems)
        {
            if (voucherItems == null)
            {
                throw new ArgumentNullException("voucherItems");
            }

            var uow = VoucherItemsRepository.UnitOfWork;
            VoucherItemsRepository.Insert(voucherItems);
            uow.SaveChanges();


            voucherItems.ForEach(s => { _eventPublisher.EntityInserted(s); });

        }



        public virtual void UpdateVoucherItem(VoucherItem voucherItem)
        {
            if (voucherItem == null)
            {
                throw new ArgumentNullException("voucherItem");
            }

            var uow = VoucherItemsRepository.UnitOfWork;
            VoucherItemsRepository.Update(voucherItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(voucherItem);
        }

        public virtual void DeleteVoucherItem(VoucherItem voucherItem)
        {
            if (voucherItem == null)
            {
                throw new ArgumentNullException("voucherItem");
            }

            var uow = VoucherItemsRepository.UnitOfWork;
            VoucherItemsRepository.Delete(voucherItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(voucherItem);
        }

        public virtual void DeleteVoucherItemWithVoucher(RecordingVoucher recordingVoucher)
        {
            var uow = VoucherItemsRepository.UnitOfWork;
            var items = VoucherItemsRepository.Table.Where(s => s.StoreId == recordingVoucher.StoreId && s.RecordingVoucherId == recordingVoucher.Id);
            VoucherItemsRepository.Delete(items);
            uow.SaveChanges();

            //通知
            items.ToList().ForEach(s => { _eventPublisher.EntityInserted(s); });
        }

        /// <summary>
        /// 获取指定期间科目明细
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountsIds"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public virtual IList<VoucherItem> GetVoucherItemsByAccounts(int? storeId, int[] accountsIds, DateTime? first, DateTime? last)
        {
            if (!first.HasValue || !first.HasValue)
            {
                return null;
            }

            DateTime.TryParse(first.Value.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
            DateTime.TryParse(last.Value.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

            var query = from pc in VoucherItemsRepository.TableNoTracking
                        where pc.StoreId == storeId
                        && accountsIds.Contains(pc.AccountingOptionId)
                        && pc.RecordTime >= start
                        && pc.RecordTime <= end
                        orderby pc.Id
                        select pc;

            return query.ToList();
        }

        public virtual IList<VoucherItem> GetVoucherItemsByRecordingVoucherId(int? storeId, int recordingVoucherId)
        {
            var query = from pc in VoucherItemsRepository.TableNoTracking
                        where pc.StoreId == storeId
                        && pc.RecordingVoucherId == recordingVoucherId
                        orderby pc.Id
                        select pc;

            return query.ToList();
        }

        /// <summary>
        /// 根据科目Id获取指定期间凭证明细
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? period)
        {
            if (period.HasValue)
            {
                //当月第一天
                var first = period.Value.AddDays(1 - period.Value.Day);
                //当月最后一天
                var last = period.Value.AddDays(1 - period.Value.Day).AddMonths(1).AddDays(-1);

                DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            join vi in VoucherItemsRepository.TableNoTracking on rv.Id equals vi.RecordingVoucherId
                            where rv.StoreId == storeId
                            && vi.AccountingOptionId == accountingOptionId
                            && rv.RecordTime >= start
                            && rv.RecordTime <= end
                            orderby vi.Id
                            select vi;

                return query.ToList();
            }

            return null;
        }

        /// <summary>
        /// 根据科目枚举类型码获取指定期间凭证明细
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountCodeTypeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public IList<VoucherItem> GetVoucherItemsByAccountCodeTypeIdFromPeriod(int? storeId, int accountCodeTypeId, DateTime? period)
        {
            if (period.HasValue)
            {
                //当月第一天
                var first = period.Value.AddDays(1 - period.Value.Day);
                //当月最后一天
                var last = period.Value.AddDays(1 - period.Value.Day).AddMonths(1).AddDays(-1);

                DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            join vi in VoucherItemsRepository.TableNoTracking on rv.Id equals vi.RecordingVoucherId
                            join aor in AccountingOptionsRepository.TableNoTracking on vi.AccountingOptionId equals aor.Id
                            where rv.StoreId == storeId
                            && aor.AccountCodeTypeId == accountCodeTypeId
                            && rv.RecordTime >= start
                            && rv.RecordTime <= end
                            orderby vi.Id
                            select vi;

                return query.ToList();
            }

            return null;
        }

        /// <summary>
        /// 获取指定科目的结转凭证项目
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="period"></param>
        /// <param name="numberName"></param>
        /// <returns></returns>
        public IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? period, string numberName)
        {
            if (period.HasValue)
            {
                //当月第一天
                var first = period.Value.AddDays(1 - period.Value.Day);
                //当月最后一天
                var last = period.Value.AddDays(1 - period.Value.Day).AddMonths(1).AddDays(-1);

                DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
                DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            join vi in VoucherItemsRepository.TableNoTracking on rv.Id equals vi.RecordingVoucherId
                            where rv.StoreId == storeId
                            && rv.BillNumber.Contains(numberName)
                            && vi.AccountingOptionId == accountingOptionId
                            && rv.RecordTime >= start
                            && rv.RecordTime <= end
                            orderby vi.Id
                            select vi;

                return query.ToList();
            }

            return null;
        }


        public IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? _start, DateTime? _end, string numberName)
        {
            if (!_start.HasValue)
            {
                return null;
            }

            if (!_end.HasValue)
            {
                return null;
            }

            var first = _start.Value.AddDays(1 - _start.Value.Day);
            var last = _end.Value.AddDays(1 - _end.Value.Day).AddMonths(1).AddDays(-1);

            DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
            DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

            var query = from rv in RecordingVouchersRepository.TableNoTracking
                        join vi in VoucherItemsRepository.TableNoTracking on rv.Id equals vi.RecordingVoucherId
                        where rv.StoreId == storeId
                        && rv.BillNumber.Contains(numberName)
                        && vi.AccountingOptionId == accountingOptionId
                        && rv.RecordTime >= start
                        && rv.RecordTime <= end
                        orderby vi.Id
                        select vi;

            return query.ToList();

        }


        /// <summary>
        /// 获取指定科目指定期间期末损益结转凭证项
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="period"></param>
        /// <param name="numberName"></param>
        /// <returns></returns>
        public VoucherItem GetPeriodLossSettle(int? storeId, int accountingOptionId, DateTime? period)
        {
            if (period.HasValue)
            {
                var query = from rv in RecordingVouchersRepository.TableNoTracking
                            join vi in VoucherItemsRepository.TableNoTracking on rv.Id equals vi.RecordingVoucherId
                            where rv.StoreId == storeId
                            && rv.BillNumber.Contains($"settle{period.Value.ToString("yyyyMM")}")
                            && vi.AccountingOptionId == accountingOptionId
                            && rv.RecordTime.ToString("yyyy-MM") == period.Value.ToString("yyyy-MM")
                            orderby vi.Id
                            select vi;

                return query.FirstOrDefault();
            }
            return null;
        }
        #endregion

        #region 公共


        /// <summary>
        /// 获取凭证号
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public int GetRecordingVoucherNumber(int? store, DateTime period)
        {
            if (period == null)
            {
                period = DateTime.Now;
            }

            //当月第一天
            var first = period.AddDays(1 - period.Day);
            //当月最后一天
            var last = period.AddDays(1 - period.Day).AddMonths(1).AddDays(-1);

            DateTime.TryParse(first.ToString("yyyy-MM-dd 00:00:00"), out DateTime start);
            DateTime.TryParse(last.ToString("yyyy-MM-dd 23:59:59"), out DateTime end);

            var maxRecordingVoucher = RecordingVouchersRepository.TableNoTracking
                .Where(r => r.StoreId == store && r.RecordTime >= start && r.RecordTime <= end)
                .OrderByDescending(r => r.Id)
                .Count();

            return maxRecordingVoucher += 1;
        }

        /// <summary>
        /// 创建录入凭证
        /// </summary>
        /// <param name="store"></param>
        /// <param name="makeUserId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CreateRecordingVoucher(int? store, int? makeUserId, RecordingVoucher recordingVoucher)
        {
            try
            {
                #region 添加凭证

                //经销商
                recordingVoucher.StoreId = store ?? 0;
                //凭证字(记)
                recordingVoucher.RecordName = string.IsNullOrEmpty(recordingVoucher.RecordName) ? "记" : recordingVoucher.RecordName;
                //凭证号
                recordingVoucher.RecordNumber = GetRecordingVoucherNumber(store, recordingVoucher.RecordTime);

                //记账日期
                if (recordingVoucher.RecordTime == null)
                {
                    recordingVoucher.RecordTime = DateTime.Now;
                }

                //单据编号
                recordingVoucher.BillNumber = recordingVoucher.BillNumber;
                //制单人
                recordingVoucher.MakeUserId = makeUserId ?? 0;

                if (recordingVoucher.AuditedStatus)
                {
                    recordingVoucher.AuditedUserId = makeUserId ?? 0;
                    //状态(审核)
                    recordingVoucher.AuditedStatus = true;
                    //审核时间
                    recordingVoucher.AuditedDate = DateTime.Now;
                }

                //单据类型
                recordingVoucher.BillTypeId = recordingVoucher.BillTypeId;
                //手工生成
                recordingVoucher.GenerateMode = recordingVoucher.GenerateMode;

                InsertRecordingVoucher(recordingVoucher);

                #endregion

                #region 凭证项目

                var voucherItems = recordingVoucher.Items;
                foreach (var item in voucherItems)
                {
                    if (!recordingVoucher.Items.Select(s => s.Id).Contains(item.Id))
                    {
                        item.StoreId = store ?? 0;
                        item.RecordingVoucherId = recordingVoucher.Id;
                        item.RecordTime = item.RecordTime == null ? DateTime.Now : item.RecordTime;
                        item.AccountingOptionName = string.IsNullOrEmpty(item.AccountingOptionName) ? "" : item.AccountingOptionName;

                        //if (item.AccountingOptionId != 0)
                        //    InsertVoucherItem(item);
                        //借、贷 不能都为0
                        if (item.AccountingOptionId != 0 && (item.DebitAmount != 0 || item.CreditAmount != 0))
                        {
                            InsertVoucherItem(item);
                        }

                    }
                }

                #endregion

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 工作流

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? voucherId, RecordingVoucher recordingVoucher, RecordingVoucherUpdate data, List<VoucherItem> items, bool isAdmin = false)
        {
            var uow = RecordingVouchersRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                if (voucherId.HasValue && voucherId.Value != 0)
                {
                    #region 更新凭证 
                    if (recordingVoucher != null)
                    {

                        //凭证字(记)
                        recordingVoucher.RecordName = data.RecordName;
                        //凭证号
                        recordingVoucher.RecordNumber = data.RecordNumber;
                        //记账日期
                        recordingVoucher.RecordTime = data.RecordTime == null ? DateTime.Now : data.RecordTime;

                        UpdateRecordingVoucher(recordingVoucher);
                    }

                    #endregion
                }
                else
                {
                    #region 添加凭证

                    recordingVoucher.StoreId = storeId;
                    //凭证字(记)
                    recordingVoucher.RecordName = data.RecordName;
                    //凭证号
                    recordingVoucher.RecordNumber = data.RecordNumber;
                    //记账日期
                    recordingVoucher.RecordTime = data.RecordTime == null ? DateTime.Now : data.RecordTime;
                    //单据编号
                    recordingVoucher.BillNumber = CommonHelper.GetBillNumber("PZ", storeId);
                    //制单人
                    recordingVoucher.MakeUserId = userId;
                    //状态(审核)
                    recordingVoucher.AuditedStatus = false;
                    recordingVoucher.AuditedDate = null;
                    //单据类型
                    recordingVoucher.BillTypeId = 0;
                    //手工生成
                    recordingVoucher.GenerateMode = 0;
                    InsertRecordingVoucher(recordingVoucher);

                    #endregion
                }

                #region 更新凭证项目

                data.Items.ForEach(p =>
                {
                    if (p.AccountingOptionId != 0)
                    {
                        var sd = GetVoucherItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (recordingVoucher.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.StoreId = storeId;
                                item.RecordTime = DateTime.Now;
                                item.RecordingVoucherId = recordingVoucher.Id;
                                item.RecordTime = recordingVoucher.RecordTime;
                                InsertVoucherItem(item);
                                //不排除
                                p.Id = item.Id;
                                //recordingVoucher.Items.Add(item);
                                if (!recordingVoucher.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    recordingVoucher.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            sd.Summary = p.Summary;
                            sd.AccountingOptionId = p.AccountingOptionId;
                            sd.DebitAmount = p.DebitAmount;
                            sd.CreditAmount = sd.CreditAmount;
                            UpdateVoucherItem(sd);
                        }
                    }
                });

                #endregion

                #region Grid 移除则从库移除删除项

                recordingVoucher.Items.ToList().ForEach(p =>
                {
                    if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                    {
                        recordingVoucher.Items.Remove(p);
                        var item = GetVoucherItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteVoucherItem(item);
                        }
                    }
                });

                #endregion

                //1.管理员创建 自动审核
                //2.自动生成凭证 自动审核
                if (isAdmin || recordingVoucher.GenerateMode == (int)GenerateMode.Auto) //判断当前登录者是否为管理员,若为管理员，开启自动审核
                {

                    #region 修改单据表状态
                    recordingVoucher.AuditedUserId = userId;
                    recordingVoucher.AuditedDate = DateTime.Now;
                    recordingVoucher.AuditedStatus = true;

                    UpdateRecordingVoucher(recordingVoucher);
                    #endregion

                    #region 发送通知 制单人
                    try
                    {
                        //制单人
                        var userNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { recordingVoucher.MakeUserId });
                        var queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = recordingVoucher.AuditedDate ?? DateTime.Now,
                            BillType = BillTypeEnum.RecordingVoucher,
                            BillNumber = recordingVoucher.BillNumber,
                            BillId = recordingVoucher.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(),queuedMessage);
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                    #endregion
                }
                else
                {
                    #region 发送通知 管理员
                    try
                    {
                        //管理员
                        var adminNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();
                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Message,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
                            Date = recordingVoucher.AuditedDate ?? DateTime.Now,
                            BillType = BillTypeEnum.PurchaseBill,
                            BillNumber = recordingVoucher.BillNumber,
                            BillId = recordingVoucher.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(adminNumbers,queuedMessage);
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                    #endregion
                }

                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = voucherId ?? 0, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Auditing(int storeId, int userId, RecordingVoucher recordingVoucher)
        {
            var uow = RecordingVouchersRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 修改单据表状态
                recordingVoucher.AuditedUserId = userId;
                recordingVoucher.AuditedDate = DateTime.Now;
                recordingVoucher.AuditedStatus = true;

                UpdateRecordingVoucher(recordingVoucher);
                #endregion

                #region 发送通知
                try
                {
                    //制单人
                    var userNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { recordingVoucher.MakeUserId });
                    QueuedMessage queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.Audited,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                        Date = recordingVoucher.AuditedDate ?? DateTime.Now,
                        BillType = BillTypeEnum.RecordingVoucher,
                        BillNumber = recordingVoucher.BillNumber,
                        BillId = recordingVoucher.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(),queuedMessage);
                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }
                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "单据审核成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据审核失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Reverse(int userId, RecordingVoucher recordingVoucher)
        {
            var uow = RecordingVouchersRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();





                //保存事务
                transaction.Commit();
                return new BaseResult { Success = true, Message = "单据红冲成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据红冲失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        #endregion

        #region 财务记账


        /// <summary>
        /// 业务单据记账
        /// 销售单 SaleBills
        /// 退货单 ReturnBills
        /// 采购单 PurchaseBills
        /// 采购退货单 PurchaseReturnBills
        /// 收款单 CashReceiptBills
        /// 预收款单 AdvanceReceiptBills
        /// 付款单 PaymentReceiptBills
        /// 预付款单 AdvancePaymentBills
        /// 其他收入 FinancialIncomeBills
        /// 成本调价单 CostAdjustmentBills
        /// 报损单 ScrapProductBills
        /// 费用支出 CostExpenditureBills
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bill">单据</param>
        public BaseResult CreateVoucher<T, T1>(T bill, int storeId, int makeUserId, Action<int> update, Func<BaseResult> successful, Func<BaseResult> failed) where T : BaseBill<T1> where T1 : BaseEntity
        {
            try
            {
                //销售单
                if (bill is SaleBill sb)
                {
                    #region
                    /*
                    销售单:  开单时，收款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付），  预收账款】
                    */
                    if (sb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = sb.Id,
                            //单据编号
                            BillNumber = sb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.SaleBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方（收款账户不固定）：

                        //1.优惠
                        var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = preferential?.Name,
                            AccountingOptionName = preferential?.Name,
                            AccountingOptionId = preferential?.Id ?? 0,
                            DebitAmount = sb.PreferentialAmount
                        });

                        if (sb.OweCash > 0)
                        {
                            //2.应收账款（收的是欠款）
                            var accountsReceivable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsReceivable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = accountsReceivable?.Name,
                                AccountingOptionName = accountsReceivable?.Name,
                                AccountingOptionId = accountsReceivable?.Id ?? 0,
                                //DebitAmount = sb.ReceivableAmount
                                DebitAmount = sb.OweCash
                            });
                        }

                        //3.收款账户（库存现金，银行存款，其他账户（指：三方支付），预收账款）
                        if (sb.SaleBillAccountings?.Any() ?? false)
                        {
                            sb.SaleBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 0,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        DebitAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }

                        #endregion

                        #region 贷方（固定）：

                        //1.主营业务收入
                        var mainIncome = _accountingService.Parse(storeId, AccountingCodeEnum.MainIncome);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = mainIncome?.Name,
                            AccountingOptionName = mainIncome?.Name,
                            AccountingOptionId = mainIncome?.Id ?? 0,
                            CreditAmount = sb.SumAmount
                        });

                        //2.销项税额（启用税率后） 
                        var outputTax = _accountingService.Parse(storeId, AccountingCodeEnum.OutputTax);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = outputTax?.Name,
                            AccountingOptionName = outputTax?.Name,
                            AccountingOptionId = outputTax?.Id ?? 0,
                            CreditAmount = sb.TaxAmount
                        });

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);


                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }

                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }

                    #endregion
                }
                //退货单
                else if (bill is ReturnBill rb)
                {
                    #region
                    /*
                    销售退货单:  开单时，收款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付），  预收账款】
                    */
                    if (rb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = rb.Id,
                            //单据编号
                            BillNumber = rb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.ReturnBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };


                        var accountings = rb.ReturnBillAccountings;

                        #region 借方

                        //1.主营业务收入
                        var mainincome = _accountingService.Parse(storeId, AccountingCodeEnum.MainIncome);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = mainincome?.Name,
                            AccountingOptionName = mainincome?.Name,
                            AccountingOptionId = mainincome?.Id ?? 0,
                            DebitAmount = rb.SumAmount
                        });

                        //2.销项税额（启用税率后）   
                        var outputtax = _accountingService.Parse(storeId, AccountingCodeEnum.OutputTax);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = outputtax?.Name,
                            AccountingOptionName = outputtax?.Name,
                            AccountingOptionId = outputtax?.Id ?? 0,
                            DebitAmount = rb.TaxAmount
                        });
                        #endregion

                        #region 贷方

                        //1.优惠
                        var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = preferential?.Name,
                            AccountingOptionName = preferential?.Name,
                            AccountingOptionId = preferential?.Id ?? 0,
                            CreditAmount = rb.PreferentialAmount
                        });

                        if (rb.OweCash > 0)
                        {
                            //2.应收账款（收的是欠款）
                            var accountsreceivable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsReceivable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = accountsreceivable?.Name,
                                AccountingOptionName = accountsreceivable?.Name,
                                AccountingOptionId = accountsreceivable?.Id ?? 0,
                                CreditAmount = rb.OweCash
                            });
                        }

                        //3.收款账户（库存现金，银行存款，其他账户（指：三方支付），预收账款）   
                        if (rb.ReturnBillAccountings?.Any() ?? false)
                        {
                            rb.ReturnBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 1,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        CreditAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }
                        #endregion


                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }

                    #endregion
                }
                //采购单 
                else if (bill is PurchaseBill pb)
                {
                    #region
                    /*
                    采购单：开单时，付款账户科目默认选择列表：【 库存现金， 银行存款，   其他账户（指：三方支付），  预付账款】 
                    */
                    if (pb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = pb.Id,
                            //单据编号
                            BillNumber = pb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.PurchaseBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };


                        var accountings = pb.PurchaseBillAccountings;

                        #region 借方

                        //1.进项税额（启用税率后） 
                        var inputtax = _accountingService.Parse(storeId, AccountingCodeEnum.InputTax);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = inputtax?.Name,
                            AccountingOptionName = inputtax?.Name,
                            AccountingOptionId = inputtax?.Id ?? 0,
                            DebitAmount = pb.TaxAmount
                        });

                        //2.库存商品
                        var inventorygoods = _accountingService.Parse(storeId, AccountingCodeEnum.InventoryGoods);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = inventorygoods?.Name,
                            AccountingOptionName = inventorygoods?.Name,
                            AccountingOptionId = inventorygoods?.Id ?? 0,
                            DebitAmount = pb.SumAmount
                        });

                        #endregion

                        #region 贷方

                        //--2020.04.15  pxh
                        //1.优惠
                        if (pb.PreferentialAmount > 0)
                        {
                            var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = preferential?.Name,
                                AccountingOptionName = preferential?.Name,
                                AccountingOptionId = preferential?.Id ?? 0,
                                CreditAmount = pb.PreferentialAmount
                            });
                        }
                        //--pxh

                        if (pb.OweCash > 0)
                        {
                            //1.应付账款
                            var accountspayable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsPayable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = accountspayable?.Name,
                                AccountingOptionName = accountspayable?.Name,
                                AccountingOptionId = accountspayable?.Id ?? 0,
                                CreditAmount = pb.OweCash
                            });

                        }
                        
                        //2.付款账户（库存现金，银行存款，其他账户（指：三方支付），预付账款）   
                        if (pb.PurchaseBillAccountings?.Any() ?? false)
                        {
                            pb.PurchaseBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 1,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        CreditAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }
                        #endregion


                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);

                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }

                    #endregion
                }
                //采购退货单 
                else if (bill is PurchaseReturnBill prb)
                {
                    #region
                    /*
                     采购退货单：开单时，付款账户科目默认选择列表：【 库存现金， 银行存款，   其他账户（指：三方支付），  预付账款】
                     */
                    if (prb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = prb.Id,
                            //单据编号
                            BillNumber = prb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.PurchaseReturnBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //--2020.04.15  pxh
                        //1.优惠
                        if (prb.PreferentialAmount > 0)
                        {
                            var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = preferential?.Name,
                                AccountingOptionName = preferential?.Name,
                                AccountingOptionId = preferential?.Id ?? 0,
                                DebitAmount = prb.PreferentialAmount
                            });
                        }
                        //--pxh

                        if (prb.OweCash > 0)
                        {
                            //1.应付账款
                            var accountspayable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsPayable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = accountspayable?.Name,
                                AccountingOptionName = accountspayable?.Name,
                                AccountingOptionId = accountspayable?.Id ?? 0,
                                DebitAmount = prb.OweCash
                            });
                        }

                        //2.付款账户（库存现金，银行存款，其他账户（指：三方支付），预付账款）
                        if (prb.PurchaseReturnBillAccountings?.Any() ?? false)
                        {
                            prb.PurchaseReturnBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 0,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        DebitAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }
                        #endregion

                        #region 贷方

                        //1.进项税额（启用税率后）   
                        var inputtax = _accountingService.Parse(storeId, AccountingCodeEnum.InputTax);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = inputtax?.Name,
                            AccountingOptionName = inputtax?.Name,
                            AccountingOptionId = inputtax?.Id ?? 0,
                            CreditAmount = prb.TaxAmount
                        });

                        //2.库存商品
                        var inventorygoods = _accountingService.Parse(storeId, AccountingCodeEnum.InventoryGoods);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = inventorygoods?.Name,
                            AccountingOptionName = inventorygoods?.Name,
                            AccountingOptionId = inventorygoods?.Id ?? 0,
                            CreditAmount = prb.SumAmount
                        });

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //收款单 
                else if (bill is CashReceiptBill crb)
                {
                    #region
                    /*
                    收款单：开单时，收款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付），  预收账款】
                    */
                    if (crb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = crb.Id,
                            //单据编号
                            BillNumber = crb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.CashReceiptBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //1.优惠
                        var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = preferential?.Name,
                            AccountingOptionName = preferential?.Name,
                            AccountingOptionId = preferential?.Id ?? 0,
                            //DebitAmount = crb.CashReceiptBillAccountings.Sum(s => s.CollectionAmount ?? 0)
                            DebitAmount = crb.Items.Sum(s => s.DiscountAmountOnce ?? 0)
                        });

                        //2.收款账户（库存现金，银行存款，其他账户（指：三方支付），预收账款）
                        if (crb.CashReceiptBillAccountings?.Any() ?? false)
                        {
                            crb.CashReceiptBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 0,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        DebitAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }

                        #endregion

                        #region 贷方




                        //1.应收账款
                        var accountsreceivable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsReceivable);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = accountsreceivable?.Name,
                            AccountingOptionName = accountsreceivable?.Name,
                            AccountingOptionId = accountsreceivable?.Id ?? 0,
                            CreditAmount = crb.CashReceiptBillAccountings.Sum(s => s.CollectionAmount ) + crb.Items.Sum(s => s.DiscountAmountOnce ?? 0)
                        });

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //预收款单 
                else if (bill is AdvanceReceiptBill arb)
                {
                    #region
                    /*
                    预收款单：开单时，收款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付）】
                    */
                    if (arb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = arb.Id,
                            //单据编号
                            BillNumber = arb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.AdvanceReceiptBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //--2020.04.15  pxh
                        //1.优惠
                        if (arb.DiscountAmount > 0)
                        {
                            var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = preferential?.Name,
                                AccountingOptionName = preferential?.Name,
                                AccountingOptionId = preferential?.Id ?? 0,
                                DebitAmount = arb.DiscountAmount
                            });
                        }
                        //--pxh

                        //2.收款账户（库存现金，银行存款，其他账户（指：三方支付））
                        if (arb.Items?.Any() ?? false)
                        {
                            arb.Items.Where(s => s.Copy == false).ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 0,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        DebitAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }

                        #endregion

                        #region 贷方

                        //1.预收账款 （该科目下子科目） 
                        var advancereceipt = _accountingService.ParseChilds(storeId, AccountingCodeEnum.AdvanceReceipt, arb.AccountingOptionId ?? 0);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = advancereceipt?.Name,
                            AccountingOptionName = advancereceipt?.Name,
                            AccountingOptionId = advancereceipt?.Id ?? 0,
                            CreditAmount = arb.AdvanceAmount
                        });

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }

                    #endregion
                }
                //付款单 
                else if (bill is PaymentReceiptBill prcb)
                {
                    #region
                    /*
                    付款单：开单时，付款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付），  预付账款】
                    */
                    if (prcb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = prcb.Id,
                            //单据编号
                            BillNumber = prcb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.PaymentReceiptBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //1.应付账款
                        var accountspayable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsPayable);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = accountspayable?.Name,
                            AccountingOptionName = accountspayable?.Name,
                            AccountingOptionId = accountspayable?.Id ?? 0,
                            DebitAmount = prcb.AmountOwedAfterReceipt
                        });

                        #endregion

                        #region 贷方

                        //1.优惠
                        if (prcb.DiscountAmount > 0)
                        {
                            var preferential = _accountingService.Parse(storeId, AccountingCodeEnum.Preferential);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = preferential?.Name,
                                AccountingOptionName = preferential?.Name,
                                AccountingOptionId = preferential?.Id ?? 0,
                                CreditAmount = prcb.DiscountAmount
                            });
                        }

                        //1.付款账户（库存现金，银行存款，其他账户（指：三方支付），预付账款）
                        if (prcb.PaymentReceiptBillAccountings?.Any() ?? false)
                        {
                            prcb.PaymentReceiptBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 1,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        CreditAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }

                    #endregion

                }
                //预付款单 
                else if (bill is AdvancePaymentBill apb)
                {
                    #region
                    /*
                    预收款单：开单时，付款账户科目默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付）】
                    */
                    if (apb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = apb.Id,
                            //单据编号
                            BillNumber = apb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.AdvancePaymentBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //1.预付款 （该科目下子科目）
                        //将预付款Imprest修改为预付账款AdvancePayment,不然取不到子科目
                        var imprest = _accountingService.ParseChilds(storeId, AccountingCodeEnum.AdvancePayment, apb.AccountingOptionId ?? 0);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = imprest?.Name,
                            AccountingOptionName = imprest?.Name,
                            AccountingOptionId = imprest?.Id ?? 0,
                            DebitAmount = apb.AdvanceAmount
                        });

                        #endregion

                        #region 贷方

                        //1.付款账户（库存现金，银行存款，其他账户（指：三方支付）） 
                        if (apb.Items?.Any() ?? false)
                        {
                            apb.Items.Where(s => s.Copy == false).ToList().ForEach(a =>
                                {
                                    if (a.CollectionAmount != 0)
                                    {
                                        //添加明细
                                        recordingVoucher.Items.Add(new VoucherItem()
                                        {
                                            StoreId = storeId,
                                            Direction = 1,
                                            RecordTime = DateTime.Now,
                                            Summary = a.AccountingOption?.Name,
                                            AccountingOptionName = a.AccountingOption?.Name,
                                            AccountingOptionId = a.AccountingOptionId,
                                            CreditAmount = a.CollectionAmount
                                        });
                                    }
                                });
                        }

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //其他收入 
                else if (bill is FinancialIncomeBill fib)
                {
                    #region
                    /*
                    其它收入单：开单时，收款方式 默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付）】
                    */
                    if (fib != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = fib.Id,
                            //单据编号
                            BillNumber = fib.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.FinancialIncomeBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        if (fib.OweCash > 0)
                        {
                            //1.应付账款
                            var accountspayable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsPayable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = accountspayable?.Name,
                                AccountingOptionName = accountspayable?.Name,
                                AccountingOptionId = accountspayable?.Id ?? 0,
                                DebitAmount = fib.OweCash
                            });
                        }

                        //1.收款方式 （库存现金，银行存款，其他账户（指：三方支付））
                        if (fib.FinancialIncomeBillAccountings?.Any() ?? false)
                        {
                            fib.FinancialIncomeBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 0,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        DebitAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }

                        #endregion

                        #region 贷方

                        //1.其他业务收入 （该科目下子科目(明细)）    
                        if (fib.Items?.Any() ?? false)
                        {
                            fib.Items.ToList().ForEach(a =>
                            {
                                var otherincome = _accountingService.GetAccountingOptionById(a.AccountingOptionId);
                                //添加明细
                                recordingVoucher.Items.Add(new VoucherItem()
                                {
                                    StoreId = storeId,
                                    Direction = 1,
                                    RecordTime = DateTime.Now,
                                    Summary = otherincome?.Name,
                                    AccountingOptionName = otherincome?.Name,
                                    AccountingOptionId = a.AccountingOptionId,
                                    //DebitAmount = a.Amount
                                    CreditAmount = a.Amount
                                });
                            });
                        }

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //成本调价单 
                else if (bill is CostAdjustmentBill cab)
                {
                    #region

                    /*
                     成本调价单：开单时，不记账，结算时

                    调价损失，记账凭证为：

  	                    借方（固定）：
                            1.库存商品

                        贷方（固定）：
 	                        1.成本调价损失


                     调价收入，记账凭证为：

  	                   借方（固定）：
                            1.库存商品

                       贷方（固定）：
       	                    1.成本调价收入

                     */
                    if (cab != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = cab.Id,
                            //单据编号
                            BillNumber = cab.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.CostAdjustmentBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };


                        var adjustmentPriceBefore = cab.Items.Sum(s => s.AdjustmentPriceBefore ?? 0);
                        var adjustedPrice = cab.Items.Sum(s => s.AdjustedPrice ?? 0);
                        var check = adjustedPrice - adjustmentPriceBefore;

                        #region 借方

                        //1.库存商品
                        var inventorygoods = _accountingService.Parse(storeId, AccountingCodeEnum.InventoryGoods);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 0,
                            RecordTime = DateTime.Now,
                            Summary = inventorygoods?.Name,
                            AccountingOptionName = inventorygoods?.Name,
                            AccountingOptionId = inventorygoods?.Id ?? 0,
                            CreditAmount = check
                        });

                        #endregion

                        #region 贷方

                        //收入
                        if (check > 0)
                        {
                            //1.成本调价收入
                            var costincome = _accountingService.Parse(storeId, AccountingCodeEnum.CostIncome);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = costincome?.Name,
                                AccountingOptionName = costincome?.Name,
                                AccountingOptionId = costincome?.Id ?? 0,
                                CreditAmount = check
                            });

                        }
                        //损失
                        else
                        {
                            //1.成本调价损失
                            var costloss = _accountingService.Parse(storeId, AccountingCodeEnum.CostLoss);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = costloss?.Name,
                                AccountingOptionName = costloss?.Name,
                                AccountingOptionId = costloss?.Id ?? 0,
                                DebitAmount = check
                            });
                        }

                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //报损单 
                else if (bill is ScrapProductBill spb)
                {
                    #region
                    /*
                    报损单：开单时

                    如果单据类型为：营业内

                                记账凭证为：

                    借方（固定）：
                            1.自定义费用科目：（管理费用科目下）

                                贷方（固定）：
                    1.库存商品

                    如果单据类型为：营业外

                                记账凭证为：

                    借方（固定）：
                            1.营业外支出

                                贷方（固定）：
                    1.库存商品    
                    */
                    if (spb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = spb.Id,
                            //单据编号
                            BillNumber = spb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.ScrapProductBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        var costAmount = spb.Items.Sum(s => s.CostAmount ?? 0);

                        #region 借方
                        //营业内
                        if (spb.Reason == 0)
                        {
                            // 1.自定义费用科目：（管理费用科目下）
                            var managefees = _accountingService.Parse(storeId, AccountingCodeEnum.ManageFees);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = managefees?.Name,
                                AccountingOptionName = managefees?.Name,
                                AccountingOptionId = managefees?.Id ?? 0,
                                DebitAmount = costAmount
                            });
                        }
                        //营业外
                        else if (spb.Reason == 1)
                        {
                            //1.营业外支出
                            var nonOperatingExpenses = _accountingService.Parse(storeId, AccountingCodeEnum.NonOperatingExpenses);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 0,
                                RecordTime = DateTime.Now,
                                Summary = nonOperatingExpenses?.Name,
                                AccountingOptionName = nonOperatingExpenses?.Name,
                                AccountingOptionId = nonOperatingExpenses?.Id ?? 0,
                                DebitAmount = costAmount
                            });
                        }

                        #endregion

                        #region 贷方

                        //1.库存商品
                        var inventoryGoods = _accountingService.Parse(storeId, AccountingCodeEnum.InventoryGoods);
                        recordingVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = storeId,
                            Direction = 1,
                            RecordTime = DateTime.Now,
                            Summary = inventoryGoods?.Name,
                            AccountingOptionName = inventoryGoods?.Name,
                            AccountingOptionId = inventoryGoods?.Id ?? 0,
                            CreditAmount = costAmount
                        });


                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }
                //费用支出 
                else if (bill is CostExpenditureBill ceb)
                {
                    #region
                    /*
                    费用支出单：开单时，付款方式 默认选择列表：【 库存现金（默认）， 银行存款，   其他账户（指：三方支付）】
                   
                    */
                    if (ceb != null)
                    {
                        //凭证
                        var recordingVoucher = new RecordingVoucher()
                        {
                            BillId = ceb.Id,
                            //单据编号
                            BillNumber = ceb.BillNumber,
                            //单据类型
                            BillTypeId = (int)BillTypeEnum.CostExpenditureBill,
                            //系统生成
                            GenerateMode = (int)GenerateMode.Auto,
                            //审核时间
                            AuditedDate = DateTime.Now,
                            RecordTime = DateTime.Now,
                            //自动审核
                            AuditedStatus = true,
                            //审核人
                            AuditedUserId = makeUserId
                        };

                        #region 借方

                        //1.销售费用，管理费用，财务费用(任何子科目)
                        if (ceb.Items?.Any() ?? false)
                        {
                            ceb.Items.ToList().ForEach(a =>
                            {
                                var cost = _accountingService.GetAccountingOptionById(a.AccountingOptionId);
                                //添加费用
                                recordingVoucher.Items.Add(new VoucherItem()
                                {
                                    StoreId = storeId,
                                    Direction = 0,
                                    RecordTime = DateTime.Now,
                                    Summary = cost?.Name,
                                    AccountingOptionName = cost?.Name,
                                    AccountingOptionId = cost?.Id ?? 0,
                                    DebitAmount = a.Amount
                                });
                            });
                        }

                        #endregion

                        #region 贷方

                        if (ceb.OweCash > 0)
                        {
                            //1.应付账款
                            var accountspayable = _accountingService.Parse(storeId, AccountingCodeEnum.AccountsPayable);
                            recordingVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = storeId,
                                Direction = 1,
                                RecordTime = DateTime.Now,
                                Summary = accountspayable?.Name,
                                AccountingOptionName = accountspayable?.Name,
                                AccountingOptionId = accountspayable?.Id ?? 0,
                                CreditAmount = ceb.OweCash
                            });
                        }

                        //1.付款方式（库存现金（默认）， 银行存款，   其他账户（指：三方支付））
                        if (ceb.CostExpenditureBillAccountings?.Any() ?? false)
                        {
                            ceb.CostExpenditureBillAccountings.ToList().ForEach(a =>
                            {
                                if (a.CollectionAmount != 0)
                                {
                                    //添加明细
                                    recordingVoucher.Items.Add(new VoucherItem()
                                    {
                                        StoreId = storeId,
                                        Direction = 1,
                                        RecordTime = DateTime.Now,
                                        Summary = a.AccountingOption?.Name,
                                        AccountingOptionName = a.AccountingOption?.Name,
                                        AccountingOptionId = a.AccountingOptionId,
                                        CreditAmount = a.CollectionAmount
                                    });
                                }
                            });
                        }
                        #endregion

                        //创建
                        CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);
                        try
                        {
                            //更新
                            update?.Invoke(recordingVoucher.Id);
                        }
                        catch (Exception)
                        {
                            //出错时回滚凭证
                            RollBackRecordingVoucher(recordingVoucher);
                            return failed?.Invoke();
                        }
                        //返回凭证
                        var result = recordingVoucher.Id > 0;

                        if (!result)
                        {
                            return failed?.Invoke();
                        }
                        else
                        {
                            return successful?.Invoke();
                        }
                    }
                    #endregion
                }

                return failed?.Invoke();

            }
            catch (Exception)
            {
                return failed?.Invoke();
            }
        }

        /// <summary>
        /// 删除凭证和记录明细
        /// </summary>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        public bool DeleteVoucher(int voucherId)
        {
            //var voucher = RecordingVouchersRepository.ToCachedGetById(voucherId);
            //if (voucher != null)
            //{
            //    DeleteVoucherItemWithVoucher(voucher);
            //    DeleteRecordingVoucher(voucher);
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            var voucher = RecordingVouchersRepository.ToCachedGetById(voucherId);
            if (voucher != null)
            {
                try
                {
                    DeleteVoucherItemWithVoucher(voucher);
                    DeleteRecordingVoucher(voucher);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                //如果凭证不存在，不需要删除
                return true;
            }


        }

        /// <summary>
        /// 业务单据取消记账
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bill"></param>
        /// <param name="voucherId"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public bool CancleVoucher<T, T1>(T bill, Action update) where T : BaseBill<T1> where T1 : BaseEntity
        {
            try
            {

                var result = false;

                //销售单
                if (bill is SaleBill sb)
                {
                    if (sb != null)
                    {
                        result = DeleteVoucher(sb.VoucherId);
                    }
                }
                //退货单
                else if (bill is ReturnBill rb)
                {
                    if (rb != null)
                    {
                        result = DeleteVoucher(rb.VoucherId);
                    }
                }
                //采购单 
                else if (bill is PurchaseBill pb)
                {
                    if (pb != null)
                    {
                        result = DeleteVoucher(pb.VoucherId);
                    }
                }
                //采购退货单 
                else if (bill is PurchaseReturnBill prb)
                {
                    if (prb != null)
                    {
                        result = DeleteVoucher(prb.VoucherId);
                    }
                }
                //收款单 
                else if (bill is CashReceiptBill crb)
                {
                    if (crb != null)
                    {
                        result = DeleteVoucher(crb.VoucherId);
                    }
                }
                //预收款单 
                else if (bill is AdvanceReceiptBill arb)
                {
                    if (arb != null)
                    {
                        result = DeleteVoucher(arb.VoucherId);
                    }
                }
                //付款单 
                else if (bill is PaymentReceiptBill prcb)
                {
                    if (prcb != null)
                    {
                        result = DeleteVoucher(prcb.VoucherId);
                    }
                }
                //预付款单 
                else if (bill is AdvancePaymentBill apb)
                {
                    if (apb != null)
                    {
                        result = DeleteVoucher(apb.VoucherId);
                    }
                }
                //其他收入 
                else if (bill is FinancialIncomeBill fib)
                {
                    if (fib != null)
                    {
                        result = DeleteVoucher(fib.VoucherId);
                    }
                }
                //成本调价单 
                else if (bill is CostAdjustmentBill cab)
                {
                    if (cab != null)
                    {
                        result = DeleteVoucher(cab.VoucherId);
                    }
                }
                //报损单 
                else if (bill is ScrapProductBill spb)
                {
                    if (spb != null)
                    {
                        result = DeleteVoucher(spb.VoucherId);
                    }
                }
                //费用支出 
                else if (bill is CostExpenditureBill ceb)
                {
                    if (ceb != null)
                    {
                        result = DeleteVoucher(ceb.VoucherId);
                    }
                }
                //盘点盈亏单
                else if (bill is InventoryProfitLossBill ipb)
                {
                    if (ipb != null)
                    {
                        result = DeleteVoucher(ipb.VoucherId);
                    }
                }

                //凭证删除成功执行回调函数
                if (result)
                {
                    update?.Invoke();
                }

                return result;

            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 成本结转记账
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storeId"></param>
        /// <param name="makeUserId"></param>
        /// <param name="debit"></param>
        /// <param name="credit"></param>
        /// <param name="debitAmount"></param>
        /// <param name="creditAmount"></param>
        /// <param name="period"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public bool CreateCostVoucher(CostOfSettleEnum type, int storeId, int makeUserId, AccountingCodeEnum debit, AccountingCodeEnum credit, decimal? debitAmount, decimal? creditAmount, ClosingAccounts period, bool reserve = false)
        {
            try
            {
                var date = period.ClosingAccountDate;
                //凭证
                var recordingVoucher = new RecordingVoucher()
                {
                    BillId = 0,
                    //单据类型
                    BillTypeId = 0,
                    //系统生成
                    GenerateMode = (int)GenerateMode.Auto,
                    RecordTime = date,
                    //审核时间
                    AuditedDate = DateTime.Now,
                    //自动审核
                    AuditedStatus = true
                };

                switch (type)
                {
                    case CostOfSettleEnum.CostOfPriceAdjust:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfPriceAdjust;
                            recordingVoucher.BillNumber = $"CostOfPriceAdjust{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfPriceAdjustReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                    case CostOfSettleEnum.CostOfPurchaseReject:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfPurchaseReject;
                            recordingVoucher.BillNumber = $"CostOfPurchaseReject{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfPurchaseRejectReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                    case CostOfSettleEnum.CostOfJointGoods:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfJointGoods;
                            recordingVoucher.BillNumber = $"CostOfJointGoods{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfJointGoodsReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                    case CostOfSettleEnum.CostOfStockAdjust:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfStockAdjust;
                            recordingVoucher.BillNumber = $"CostOfStockAdjust{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfStockAdjustReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                    case CostOfSettleEnum.CostOfStockLoss:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfStockLoss;
                            recordingVoucher.BillNumber = $"CostOfStockLoss{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfStockLossReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                    //销售类
                    case CostOfSettleEnum.CostOfSales:
                        {
                            recordingVoucher.BillTypeId = (int)CostOfSettleEnum.CostOfSales;
                            recordingVoucher.BillNumber = $"CostOfSales{date.ToString("yyyyMM")}";
                            if (reserve)
                            {
                                recordingVoucher.BillNumber = $"CostOfSalesReserve{date.ToString("yyyyMM")}";
                            }
                        }
                        break;
                }


                #region 借方

                var debitAcc = _accountingService.Parse(storeId, debit);
                recordingVoucher.Items.Add(new VoucherItem()
                {
                    StoreId = storeId,
                    RecordTime = date,
                    Direction = 0,
                    Summary = reserve == false ? $"{date.ToString("yyyyMM")}月结" : $"{date.ToString("yyyyMM")}反向月结",
                    AccountingOptionName = $"{debitAcc?.Name}:{debitAcc?.Code}",
                    AccountingOptionId = debitAcc?.Id ?? 0,
                    DebitAmount = debitAmount ?? 0
                });

                #endregion

                #region 贷方


                var creditAcc = _accountingService.Parse(storeId, credit);
                recordingVoucher.Items.Add(new VoucherItem()
                {
                    StoreId = storeId,
                    RecordTime = date,
                    Direction = 1,
                    Summary = reserve == false ? $"{date.ToString("yyyyMM")}月结" : $"{date.ToString("yyyyMM")}反向月结",
                    AccountingOptionName = $"{creditAcc?.Name}:{creditAcc?.Code}",
                    AccountingOptionId = creditAcc?.Id ?? 0,
                    CreditAmount = creditAmount ?? 0
                });

                #endregion

                //创建
                CreateRecordingVoucher(storeId, makeUserId, recordingVoucher);

                //返回凭证
                return recordingVoucher.Id > 0;

            }
            catch (Exception)
            {
                return false;
            }

        }

        #endregion


    }
}
