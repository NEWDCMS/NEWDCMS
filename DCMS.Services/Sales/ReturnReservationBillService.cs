using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Sales
{
    public class ReturnReservationBillService : BaseService, IReturnReservationBillService
    {

        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly ISettingService _settingService;
        private readonly ITerminalService _terminalService;
        


        public ReturnReservationBillService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            ISettingService settingService,
            ITerminalService terminalService
            ) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _settingService = settingService;
            _queuedMessageService = queuedMessageService;
            _terminalService = terminalService;
            
        }


        #region 退货订单
        public bool Exists(int billId)
        {
            return ReturnReservationBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        /// <summary>
        /// 查询当前经销商退货订单
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="auditingStatus"></param>
        /// <param name="districtId"></param>
        /// <param name="departmentId"></param>
        /// <param name="remark"></param>
        /// <param name="billNumber"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<ReturnReservationBill> GetReturnReservationBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in ReturnReservationBillsRepository.Table
                         .Include(cr => cr.Items)
                         //.ThenInclude(cr => cr.ReturnReservationBill)
                         .Include(cr => cr.ReturnReservationBillAccountings)
                         .ThenInclude(cr => cr.AccountingOption)
                        select pc;

            if (store.HasValue && store != 0)
            {
                query = query.Where(a => a.StoreId == store);
            }

            if (makeuserId.HasValue && makeuserId > 0)
            {
                var userIds = _userService.GetSubordinate(store, makeuserId ?? 0)?.Where(s => s > 0).ToList();
                if (userIds.Count > 0)
                    query = query.Where(x => userIds.Contains(x.MakeUserId));
            }

            //客户
            if (terminalId.HasValue && terminalId != 0)
            {
                query = query.Where(a => a.TerminalId == terminalId);
            }

            //客户名称检索
            if (!string.IsNullOrEmpty(terminalName))
            {
                var terminalIds = _terminalService.GetTerminalIds(store, terminalName);
                query = query.Where(a => terminalIds.Contains(a.TerminalId));
            }

            //业务员
            if (businessUserId.HasValue && businessUserId != 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            //送货员
            if (deliveryUserId.HasValue && deliveryUserId != 0)
            {
                query = query.Where(a => a.DeliveryUserId == deliveryUserId);
            }

            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }

            //仓库
            if (wareHouseId.HasValue && wareHouseId != 0)
            {
                query = query.Where(a => a.WareHouseId == wareHouseId);
            }

            //备注
            if (!string.IsNullOrEmpty(remark))
            {
                query = query.Where(a => a.Remark.Contains(remark));
            }

            //开始时间
            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= startDate);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= endDate);
            }

            //片区
            if (districtId.HasValue && districtId != 0)
            {
                var terminals = _terminalService.GetDisTerminalIds(store, districtId ?? 0);
                query = query.Where(a => terminals.Contains(a.TerminalId));
            }

            //审核状态
            if (auditedStatus != null)
            {
                query = query.Where(a => a.AuditedStatus == auditedStatus);
            }

            if (showReverse != null)
            {
                query = query.Where(a => a.ReversedStatus == showReverse);
            }

            if (deleted.HasValue)
            {
                query = query.Where(a => a.Deleted == deleted);
            }

            //部门
            //if (departmentId.HasValue && departmentId != 0)
            //    query = query.Where(a => a.DepartmentId == departmentId);

            //按审核排序
            if (sortByAuditedTime.HasValue && sortByAuditedTime.Value == true)
            {
                query = query.OrderByDescending(c => c.AuditedDate);
            }

            //默认创建时间
            else
            {
                query = query.OrderByDescending(c => c.CreatedOnUtc);
            }

            //var plist = new PagedList<ReturnReservationBill>(query.ToList(), pageIndex, pageSize);
            //return plist;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ReturnReservationBill>(plists, pageIndex, pageSize, totalCount);
        }


        public IList<ReturnReservationBill> GetReturnReservationBillsByStoreId(int? storeId)
        {

            return _cacheManager.Get(DCMSDefaults.RETURNRESERVATIONBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ReturnReservationBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<ReturnReservationBill> GetReturnReservationBillByStoreIdTerminalId(int storeId, int terminalId)
        {
            var query = ReturnReservationBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.TerminalId == terminalId);

            return query.ToList();
        }

        public IList<ReturnReservationBill> GetReturnReservationBillsNullWareHouseByStoreId(int storeId)
        {

            return _cacheManager.Get(DCMSDefaults.RETURNRESERVATIONBILLNULLWAREHOUSE_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ReturnReservationBillsRepository.Table;

               query = query.Where(a => a.StoreId == storeId);
               query = query.Where(a => a.WareHouseId == 0);
               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<ReturnReservationBill> GetHotSaleReservationRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
        {
            var query = ReturnReservationBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            if (store > 0)
            {
                query = query.Where(a => a.StoreId == store);
            }

            //客户
            if (terminalId > 0)
            {
                query = query.Where(a => a.TerminalId == terminalId);
            }

            //业务员
            if (businessUserId > 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }
            //开始日期
            if (startTime != null)
            {
                query = query.Where(a => a.TransactionDate > startTime);
            }

            //结束日期
            if (endTime != null)
            {
                query = query.Where(a => a.TransactionDate < endTime);
            }

            return query.ToList();
        }

        /// <summary>
        /// 获取收款对账单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="status"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public IList<ReturnReservationBill> GetReturnReservationBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null)
        {
            var query = ReturnReservationBillsRepository.Table;

            //经销商
            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //状态

            //开始时间
            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= start);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= end);
            }

            //业务员
            if (businessUserId.HasValue)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }
        public ReturnReservationBill GetReturnReservationBillById(int? store, int returnReservationBillId, bool isInclude = false)
        {

            if (returnReservationBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = ReturnReservationBillsRepository.Table
                .Include(rr => rr.Items)
                //.ThenInclude(rr => rr.ReturnReservationBill)
                .Include(rr => rr.ReturnReservationBillAccountings);

                return query.FirstOrDefault(rr => rr.Id == returnReservationBillId);
            }
            return ReturnReservationBillsRepository.ToCachedGetById(returnReservationBillId);
        }

        /// <summary>
        /// 根据退货订单ids查询退货订单列表，不要缓存
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        public IList<ReturnReservationBill> GetReturnReservationBillsByIds(int[] sIds, bool isInclude = false)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<ReturnReservationBill>();
            }

            if (isInclude)
            {
                var query = ReturnReservationBillsRepository.Table
                .Include(sb => sb.Items)
                //.ThenInclude(sb => sb.ReturnReservationBill)
                .Include(sb => sb.ReturnReservationBillAccountings)
                .ThenInclude(sb => sb.AccountingOption)
                .Where(s => sIds.Contains(s.Id)); ;
                return query.ToList();
            }
            else
            {
                var query = from c in ReturnReservationBillsRepository.Table
                            where sIds.Contains(c.Id)
                            select c;
                var list = query.ToList();
                return list;
            }
        }

        public ReturnReservationBill GetReturnReservationBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.RETURNRESERVATIONBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = ReturnReservationBillsRepository.Table;
                var returnReservationBill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return returnReservationBill;
            });
        }

        public int GetBillId(int? store, string billNumber)
        {
            var query = ReturnReservationBillsRepository.TableNoTracking;
            var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).Select(s => s.Id).FirstOrDefault();
            return bill;
        }


        public void DeleteReturnReservationBill(ReturnReservationBill returnReservationBill)
        {

            if (returnReservationBill == null)
            {
                throw new ArgumentNullException("returnreservationbill");
            }

            var uow = ReturnReservationBillsRepository.UnitOfWork;
            ReturnReservationBillsRepository.Delete(returnReservationBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnReservationBill);

        }

        public void InsertReturnReservationBill(ReturnReservationBill returnReservationBill)
        {
            var uow = ReturnReservationBillsRepository.UnitOfWork;
            ReturnReservationBillsRepository.Insert(returnReservationBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(returnReservationBill);
        }

        public void UpdateReturnReservationBill(ReturnReservationBill returnReservationBill)
        {
            if (returnReservationBill == null)
            {
                throw new ArgumentNullException("returnreservationbill");
            }

            var uow = ReturnReservationBillsRepository.UnitOfWork;
            ReturnReservationBillsRepository.Update(returnReservationBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(returnReservationBill);

        }

        public void ChangedBill(int billId, int userId)
        {
            var uow = ReturnReservationBillsRepository.UnitOfWork;
            var bill = ReturnReservationBillsRepository.ToCachedGetById(billId);
            if (bill != null)
            {
                bill.ChangedUserId = userId;
                bill.ChangedStatus = true;
                bill.ChangedDate = DateTime.Now;
                ReturnReservationBillsRepository.Update(bill);
            }
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(bill);
        }

        public void UpdateReturnReservationBill(IList<ReturnReservationBill> returnReservationBills)
        {
            if (returnReservationBills == null)
            {
                throw new ArgumentNullException("returnReservationBills");
            }

            var uow = ReturnReservationBillsRepository.UnitOfWork;
            ReturnReservationBillsRepository.Update(returnReservationBills);
            uow.SaveChanges();

            returnReservationBills.ToList().ForEach(s => { _eventPublisher.EntityUpdated(s); });
        }

        public IList<ReturnReservationBillAccounting> GetAllReservationBillAccountingsByBillIds(int? store, int[] billIds)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<ReturnReservationBillAccounting>();
            }

            var key = DCMSDefaults.RETURNRESERVATIONACCOUNTINGL_BY_RETURNRESERVATIONID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnReservationBillAccountingMappingRepository.Table
                            .Include(rr => rr.ReturnReservationBill)
                            where billIds.Contains(pc.BillId)
                            select pc;
                return query.ToList();
            });
        }

        /// <summary>
        /// 设置退货订单金额
        /// </summary>
        /// <param name="returnReservationBillId"></param>
        public void SetReturnReservationBillAmount(int returnReservationBillId)
        {
            ReturnReservationBill returnReservationBill;
            var query = ReturnReservationBillsRepository.Table;
            returnReservationBill = query.Where(a => a.Id == returnReservationBillId).FirstOrDefault();
            if (returnReservationBill == null)
            {
                throw new ArgumentNullException("returnreservationbill");
            }
            List<ReturnReservationItem> returnReservationItems = GetReturnReservationItemList(returnReservationBillId);
            if (returnReservationItems != null && returnReservationItems.Count > 0)
            {
                //总金额
                decimal SumAmount = returnReservationItems.Sum(a => a.Amount);
                //已收金额（会计科目金额）
                decimal accounting = 0;
                IList<ReturnReservationBillAccounting> returnReservationAccountings = GetReturnReservationBillAccountingsByReturnReservationId(0, returnReservationBillId);
                if (returnReservationAccountings != null && returnReservationAccountings.Count > 0)
                {
                    accounting = returnReservationAccountings.Sum(a => a.CollectionAmount);
                }
                //总金额
                returnReservationBill.SumAmount = SumAmount;
                //应收金额=总金额-优惠金额
                //returnReservation.ReceivableAmount = SumAmount - (returnReservation.PreferentialAmount ?? 0);
                //欠款金额=总金额-优惠金额-已收金额
                //returnReservation.OweCash = SumAmount - (returnReservation.PreferentialAmount ?? 0) - accounting;

                //总成本价
                decimal SumCostPrice = returnReservationItems.Sum(a => a.CostPrice);
                returnReservationBill.SumCostPrice = SumCostPrice;
                //总成本金额
                decimal SumCostAmount = returnReservationItems.Sum(a => a.CostAmount);
                returnReservationBill.SumCostAmount = SumCostAmount;
                //总利润 = 总金额-总成本金额
                returnReservationBill.SumProfit = returnReservationBill.SumAmount - SumCostAmount;
                //成本利润率 = 总利润 / 总成本金额
                var amount = (returnReservationBill.SumCostAmount == 0) ? returnReservationBill.SumProfit : returnReservationBill.SumCostAmount;
                if (amount != 0)
                {
                    returnReservationBill.SumCostProfitRate = (returnReservationBill.SumProfit / amount) * 100;
                }
                var uow = ReturnReservationBillsRepository.UnitOfWork;
                ReturnReservationBillsRepository.Update(returnReservationBill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(returnReservationBill);
            }

        }

        #endregion

        #region 退货订单明细

        /// <summary>
        /// 根据退货订单获取项目
        /// </summary>
        /// <param name="returnReservationBillId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<ReturnReservationItem> GetReturnReservationItemByReturnReservationId(int returnReservationBillId, int? userId, int storeId, int pageIndex, int pageSize)
        {
            if (returnReservationBillId == 0)
            {
                return new PagedList<ReturnReservationItem>(new List<ReturnReservationItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.RETURNRESERVATIONBILL_ITEM_ALLBY_RETURNRESERVATIONID_KEY.FillCacheKey(storeId, returnReservationBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnReservationItemsRepository.Table
                            .Include(ri => ri.ReturnReservationBill)
                            where pc.ReturnReservationBillId == returnReservationBillId
                            orderby pc.Id
                            select pc;
                //var returnReservationItems = new PagedList<ReturnReservationItem>(query.ToList(), pageIndex, pageSize);
                //return returnReservationItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ReturnReservationItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<ReturnReservationItem> GetReturnReservationItemList(int returnReservationId)
        {
            List<ReturnReservationItem> returnReservationItems = null;
            var query = ReturnReservationItemsRepository.Table.Include(r=>r.ReturnReservationBill);
            returnReservationItems = query.Where(a => a.ReturnReservationBillId == returnReservationId).ToList();
            return returnReservationItems;
        }

        public int ReturnReservationItemQtySum(int storeId, int productId, int returnReservationBillId)
        {
            int qty = 0;
            var query = from returnReservation in ReturnReservationBillsRepository.Table
                        join returnReservationItem in ReturnReservationItemsRepository.Table on returnReservation.Id equals returnReservationItem.ReturnReservationBillId
                        where returnReservation.AuditedStatus == true && returnReservationItem.ProductId == productId
                              && returnReservation.Id != returnReservationBillId    //排除当前退货订单数量
                        select returnReservationItem;
            List<ReturnReservationItem> returnReservationItems = query.ToList();
            if (returnReservationItems != null && returnReservationItems.Count > 0)
            {
                qty = returnReservationItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public ReturnReservationItem GetReturnReservationItemById(int returnReservationItemId)
        {
            ReturnReservationItem returnReservationItem;
            var query = ReturnReservationItemsRepository.Table;
            returnReservationItem = query.Where(a => a.Id == returnReservationItemId).FirstOrDefault();
            return returnReservationItem;
        }

        public void DeleteReturnReservationItem(ReturnReservationItem returnReservationItem)
        {
            if (returnReservationItem == null)
            {
                throw new ArgumentNullException("returnReservationItem");
            }

            var uow = ReturnReservationItemsRepository.UnitOfWork;
            ReturnReservationItemsRepository.Delete(returnReservationItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnReservationItem);
        }

        public void InsertReturnReservationItem(ReturnReservationItem returnReservationItem)
        {
            var uow = ReturnReservationItemsRepository.UnitOfWork;
            ReturnReservationItemsRepository.Insert(returnReservationItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(returnReservationItem);
        }

        public void UpdateReturnReservationItem(ReturnReservationItem returnReservationItem)
        {
            if (returnReservationItem == null)
            {
                throw new ArgumentNullException("returnReservationItem");
            }

            var uow = ReturnReservationItemsRepository.UnitOfWork;
            ReturnReservationItemsRepository.Update(returnReservationItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(returnReservationItem);
        }

        #endregion

        #region 收款账户映射

        public virtual IPagedList<ReturnReservationBillAccounting> GetReturnReservationBillAccountingsByReturnReservationId(int storeId, int userId, int returnReservationId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (returnReservationId == 0)
            {
                return new PagedList<ReturnReservationBillAccounting>(new List<ReturnReservationBillAccounting>(), pageIndex, pageSize);
            }

            //string key = string.Format(RETURNRESERVATIONACCOUNTING_ALLBY_RETURNRESERVATIONID_KEY.FillCacheKey( returnReservationId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.RETURNRESERVATIONACCOUNTING_ALLBY_RETURNRESERVATIONID_KEY.FillCacheKey(storeId, returnReservationId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnReservationBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == returnReservationId
                            orderby pc.Id
                            select pc;


                //var returnReservationAccountings = new PagedList<ReturnReservationBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return returnReservationAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ReturnReservationBillAccounting>(plists, pageIndex, pageSize, totalCount);

            });
        }

        public virtual IList<ReturnReservationBillAccounting> GetReturnReservationBillAccountingsByReturnReservationId(int? store, int returnReservationBillId)
        {

            var key = DCMSDefaults.RETURNRESERVATIONACCOUNTINGL_BY_RETURNRESERVATIONID_KEY.FillCacheKey(store ?? 0, returnReservationBillId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnReservationBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == returnReservationBillId
                            orderby pc.Id
                            select pc;


                return query.Distinct().ToList();
            });
        }

        public virtual ReturnReservationBillAccounting GetReturnReservationBillAccountingById(int returnReservationBillAccountingId)
        {
            if (returnReservationBillAccountingId == 0)
            {
                return null;
            }

            return ReturnReservationBillAccountingMappingRepository.ToCachedGetById(returnReservationBillAccountingId);
        }

        public virtual void InsertReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationBillAccounting)
        {
            if (returnReservationBillAccounting == null)
            {
                throw new ArgumentNullException("returnReservationbillaccounting");
            }

            var uow = ReturnReservationBillAccountingMappingRepository.UnitOfWork;
            ReturnReservationBillAccountingMappingRepository.Insert(returnReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(returnReservationBillAccounting);
        }

        public virtual void UpdateReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationBillAccounting)
        {
            if (returnReservationBillAccounting == null)
            {
                throw new ArgumentNullException("returnReservationbillaccounting");
            }

            var uow = ReturnReservationBillAccountingMappingRepository.UnitOfWork;
            ReturnReservationBillAccountingMappingRepository.Update(returnReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(returnReservationBillAccounting);
        }

        public virtual void DeleteReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationBillAccounting)
        {
            if (returnReservationBillAccounting == null)
            {
                throw new ArgumentNullException("returnReservationbillaccounting");
            }

            var uow = ReturnReservationBillAccountingMappingRepository.UnitOfWork;
            ReturnReservationBillAccountingMappingRepository.Delete(returnReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnReservationBillAccounting);
        }

        #endregion


        /// <summary>
        /// 退货订单转销售单查询
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="changedStatus"></param>
        /// <param name="dispatchedStatus"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<ReturnReservationBill> GetReturnReservationBillToChangeList(int? storeId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ReturnReservationBillsRepository.Table;
            if (storeId.HasValue && storeId.Value > 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //***必须已经审核的销售订单才能转换成销售单
            query = query.Where(a => a.AuditedStatus == true);
            //***必须未红冲核的销售订单才能转换成销售单
            query = query.Where(a => a.ReversedStatus == false);

            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= start);
            }

            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= end);
            }

            //业务员
            if (businessUserId.HasValue && businessUserId != 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            //if(deliveryUserId.HasValue && deliveryUserId!=0)
            //    query = query.Where(a => a.deliveryUserId == deliveryUserId);

            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }

            //备注
            if (!string.IsNullOrEmpty(remark))
            {
                query = query.Where(a => a.Remark.Contains(remark));
            }

            if (changedStatus != null)
            {
                query = query.Where(a => a.ChangedStatus == changedStatus);
            }

            if (dispatchedStatus != null)
            {
                query = query.Where(a => a.DispatchedStatus == dispatchedStatus);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            //var plist = new PagedList<ReturnReservationBill>(query.ToList(), pageIndex, pageSize);
            //return plist;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ReturnReservationBill>(plists, pageIndex, pageSize, totalCount);
        }

        public IList<ReturnReservationBill> GetReturnReservationBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null)
        {
            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        join c in ReturnReservationBillsRepository.Table.Include(sb => sb.Items) on b.BillId equals c.Id
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.ReturnReservationBill
                        && (b.SignStatus == (int)SignStatusEnum.Done || b.SignStatus == (int)SignStatusEnum.Rejection)
                        select c;
            //var query = ReturnReservationBillsRepository.Table;

            query = query.Where(a => a.AuditedStatus == true);

            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= start);
            }

            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= end);
            }

            //业务员
            if (deliveryUserId.HasValue && deliveryUserId > 0)
            {
                query = query.Where(a => a.BusinessUserId == deliveryUserId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }


        public IList<ReturnReservationBill> GetReturnReservationBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null)
        {
            var query = ReturnReservationBillsRepository.Table;

            //已审核、未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false && a.ChangedStatus==false);

            //经销商
            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //开始时间
            if (start != null)
            {
                query = query.Where(a => a.CreatedOnUtc >= start);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(a => a.CreatedOnUtc <= end);
            }

            if (businessUserId != null && businessUserId > 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }
            //片区
            if (districtId != null && districtId != 0)
            {
                var TerminalIds = _terminalService.GetDisTerminalIds(storeId, districtId ?? 0);
                query = query.Where(a => TerminalIds.Contains(a.TerminalId));
            }

            //终端
            if (terminalId != null && terminalId > 0)
            {
                query = query.Where(a => a.TerminalId == terminalId);
            }
            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }
            //送货员 DeliveryUserId
            //客户渠道 channelId
            //客户等级 RankId
            if (rankId != null && rankId != 0)
            {
                var terminals = _terminalService.GetRankTerminalIds(storeId,rankId ?? 0);
                query = query.Where(a => terminals.Contains(a.TerminalId));
            }

            //未调度
            query = query.Where(a => a.DispatchedStatus == false);
            

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.Include(s=>s.Items).ToList();
        }


        public void UpdateReturnReservationBillActive(int? store, int? billId, int? user)
        {
            var query = ReturnReservationBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = ReturnReservationBillsRepository.UnitOfWork;
                foreach (ReturnReservationBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    ReturnReservationBillsRepository.Update(bill);
                }
                uow.SaveChanges();
            }
        }

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ReturnReservationBill bill, List<ReturnReservationBillAccounting> accountingOptions, List<AccountingOption> accountings, ReturnReservationBillUpdate data, List<ReturnReservationItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false,bool doAudit = true)
        {
            var uow = ReturnReservationBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;
                if (!(bill.Id > 0))
                {
                    bill.MakeUserId = userId;
                }


                //业务员
                string adminMobileNumbers = _userService.GetMobileNumberByUserId(data.BusinessUserId);
                var companySetting = _settingService.LoadSetting<CompanySetting>(storeId);

                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新退货

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    //returnReservationBill.DeliveryUserId = data.DeliveryUserId;
                    bill.PayTypeId = data.PayTypeId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitSale = data.IsMinUnitSale;
                    bill.Remark = data.Remark;
                    bill.DefaultAmountId = data.DefaultAmountId;

                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.OweCash = data.OweCash;

                    //计算金额
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        //总金额
                        //decimal SumAmount = data.Items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
                        //总金额
                        bill.SumAmount = data.Items.Sum(a => a.Amount);

                        //总成本价
                        decimal SumCostPrice = data.Items.Sum(a => a.CostPrice);
                        bill.SumCostPrice = SumCostPrice;
                        //总成本金额
                        decimal SumCostAmount = data.Items.Sum(a => a.CostAmount);
                        bill.SumCostAmount = SumCostAmount;
                        //总利润 = 总金额-总成本金额
                        bill.SumProfit = bill.SumAmount - SumCostAmount;
                        //成本利润率 = 总利润 / 总成本金额
                        if (bill.SumCostAmount == 0)
                        {
                            bill.SumCostProfitRate = 100;
                        }
                        else
                        {
                            bill.SumCostProfitRate = (bill.SumProfit / bill.SumCostAmount) * 100;
                        }

                        if (companySetting.EnableTaxRate)
                        {
                            //总税额
                            bill.TaxAmount = Math.Round(data.Items.Sum(a => a.Amount * (a.TaxRate / 100)), 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            bill.TaxAmount = 0;
                        }
                    }
                    else
                    {
                        bill.SumAmount = 0;
                        bill.SumCostPrice = 0;
                        bill.SumCostAmount = 0;
                        bill.SumProfit = 0;
                        bill.SumCostProfitRate = 0;
                    }

                    UpdateReturnReservationBill(bill);

                    #endregion

                }
                else
                {
                    #region 添加退货

                    bill.StoreId = storeId;
                    bill.BillType = BillTypeEnum.ReturnReservationBill;
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

                    var sb = GetReturnReservationBillByNumber(storeId, bill.BillNumber);
                    if (sb != null)
                    {
                        return new BaseResult { Success = false, Message = "操作失败，重复提交" };
                    }

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.DeliveryUserId = data.DeliveryUserId;
                    bill.PayTypeId = data.PayTypeId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitSale = data.IsMinUnitSale;
                    bill.Remark = data.Remark;
                    bill.DefaultAmountId = data.DefaultAmountId;

                    bill.SumAmount = 0;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.OweCash = data.OweCash;
                    bill.SumCostPrice = 0;
                    bill.SumCostAmount = 0;
                    bill.SumProfit = 0;
                    bill.SumCostProfitRate = 0;

                    bill.MakeUserId = userId;
                    bill.CreatedOnUtc = DateTime.Now;
                    bill.AuditedStatus = false;
                    bill.ReversedStatus = false;

                    bill.ChangedStatus = false;
                    bill.DispatchedStatus = false;
                    bill.Operation = data.Operation;//标识操作源

                    //计算金额
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        //总金额
                        //decimal SumAmount = data.Items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
                        //总金额
                        bill.SumAmount = data.Items.Sum(a => a.Amount);

                        //总成本价
                        decimal SumCostPrice = data.Items.Sum(a => a.CostPrice);
                        bill.SumCostPrice = SumCostPrice;
                        //总成本金额
                        decimal SumCostAmount = data.Items.Sum(a => a.CostAmount);
                        bill.SumCostAmount = SumCostAmount;
                        //总利润 = 总金额-总成本金额
                        bill.SumProfit = bill.SumAmount - SumCostAmount;
                        //成本利润率 = 总利润 / 总成本金额
                        if (bill.SumCostAmount == 0)
                        {
                            bill.SumCostProfitRate = 100;
                        }
                        else
                        {
                            bill.SumCostProfitRate = (bill.SumProfit / bill.SumCostAmount) * 100;
                        }

                        if (companySetting.EnableTaxRate)
                        {
                            //总税额
                            bill.TaxAmount = Math.Round(data.Items.Sum(a => a.Amount * (a.TaxRate / 100)), 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            bill.TaxAmount = 0;
                        }
                    }
                    else
                    {
                        bill.SumAmount = 0;
                        bill.SumCostPrice = 0;
                        bill.SumCostAmount = 0;
                        bill.SumProfit = 0;
                        bill.SumCostProfitRate = 0;
                    }

                    InsertReturnReservationBill(bill);
                    //主表Id
                    billId = bill.Id;

                    #endregion

                    #region 当前用户判断今天是否交账
                    //if (_financeReceiveAccountBillService.CheckTodayReceive(storeId, userId))
                    //{
                    //    #region 发送通知
                    //    try
                    //    {
                    //        var queuedMessage = new QueuedMessage()
                    //        {
                    //            StoreId = storeId,
                    //            MType = MTypeEnum.LedgerWarning,
                    //            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.LedgerWarning),
                    //            Date = bill.CreatedOnUtc,
                    //            BillType = BillTypeEnum.PurchaseReturnReservationBill,
                    //            BillNumber = bill.BillNumber,
                    //            BillId = bill.Id,
                    //            CreatedOnUtc = DateTime.Now,
                    //            ToUsers = adminMobileNumbers,
                    //            BusinessUser = _userService.GetUserName(storeId, userId)
                    //        };
                    //        _queuedMessageService.InsertQueuedMessage(queuedMessage);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _queuedMessageService.WriteLogs(ex.Message);
                    //    }
                    //    #endregion
                    //}
                    #endregion
                }

                #region 更新退货项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var returnReservationItem = GetReturnReservationItemById(p.Id);
                        if (returnReservationItem == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.ReturnReservationBillId = billId.Value;
                                item.StoreId = storeId;
                                //有税率，则价格=含税价格，金额=含税金额
                                if (item.TaxRate > 0 && companySetting.EnableTaxRate)
                                {
                                    item.Price *= (1 + item.TaxRate / 100);
                                    item.Amount *= (1 + item.TaxRate / 100);
                                }
                                //利润 = 金额 - 成本金额
                                item.Profit = item.Amount - item.CostAmount;
                                //成本利润率 = 利润 / 成本金额
                                if (item.CostAmount == 0)
                                {
                                    item.CostProfitRate = 100;
                                }
                                else
                                {
                                    item.CostProfitRate = item.Profit / item.CostAmount * 100;
                                }

                                item.CreatedOnUtc = DateTime.Now;
                                InsertReturnReservationItem(item);
                                //不排除
                                p.Id = item.Id;
                                //returnReservationBill.Items.Add(item);
                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            returnReservationItem.ProductId = p.ProductId;
                            returnReservationItem.UnitId = p.UnitId;
                            returnReservationItem.Quantity = p.Quantity;
                            returnReservationItem.RemainderQty = p.RemainderQty;
                            returnReservationItem.Price = p.Price;
                            returnReservationItem.Amount = p.Amount;
                            //有税率，则价格=含税价格，金额=含税金额
                            if (returnReservationItem.TaxRate > 0 && companySetting.EnableTaxRate)
                            {
                                returnReservationItem.Price = p.Price * (1 + p.TaxRate / 100);
                                returnReservationItem.Amount = p.Amount * (1 + p.TaxRate / 100);
                            }

                            //成本价
                            returnReservationItem.CostPrice = p.CostPrice;
                            //成本金额
                            returnReservationItem.CostAmount = p.CostAmount;
                            //利润 = 金额 - 成本金额
                            returnReservationItem.Profit = returnReservationItem.Amount - returnReservationItem.CostAmount;
                            //成本利润率 = 利润 / 成本金额
                            if (returnReservationItem.CostAmount == 0)
                            {
                                returnReservationItem.CostProfitRate = 100;
                            }
                            else
                            {
                                returnReservationItem.CostProfitRate = (returnReservationItem.Profit / returnReservationItem.CostAmount) * 100;
                            }

                            returnReservationItem.StockQty = p.StockQty;
                            returnReservationItem.Remark = p.Remark;
                            returnReservationItem.RemainderQty = p.RemainderQty;
                            returnReservationItem.ManufactureDete = p.ManufactureDete;

                            //2019-07-25
                            returnReservationItem.IsGifts = p.IsGifts;
                            returnReservationItem.BigGiftQuantity = p.BigGiftQuantity;
                            returnReservationItem.SmallGiftQuantity = p.SmallGiftQuantity;

                            UpdateReturnReservationItem(returnReservationItem);
                        }
                    }
                });

                #endregion

                #region Grid 移除则从库移除删除项

                bill.Items.ToList().ForEach(p =>
                {
                    if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                    {
                        bill.Items.Remove(p);
                        var sd = GetReturnReservationItemById(p.Id);
                        if (sd != null)
                        {
                            DeleteReturnReservationItem(sd);
                        }
                    }
                });

                #endregion

                #region 收款账户映射

                var returnReservationAccountings = GetReturnReservationBillAccountingsByReturnReservationId(storeId, bill.Id);

                accountings.ToList().ForEach(c =>
                {
                    if (data != null && data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!returnReservationAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var returnReservationAccounting = new ReturnReservationBillAccounting()
                            {
                                StoreId = storeId,
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                ReturnReservationBill = bill,
                                BillId = bill.Id,
                                TerminalId = data.TerminalId
                            };
                            //添加账户
                            InsertReturnReservationBillAccounting(returnReservationAccounting);
                        }
                        else
                        {
                            returnReservationAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.TerminalId = data.TerminalId;
                                //更新账户
                                UpdateReturnReservationBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (returnReservationAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var returnreservationaccountings = returnReservationAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            returnreservationaccountings.ForEach(sa =>
                            {
                                DeleteReturnReservationBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                //判断App开单是否自动审核
                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.ReturnReservationBill);
                }
                //读取配置自动审核、管理员创建自动审核
                if ((isAdmin && doAudit) || appBillAutoAudits) //判断当前登录者是否为管理员,若为管理员，开启自动审核
                {
                    AuditingNoTran(storeId, userId, bill);
                }
                else
                {
                    #region 发送通知 管理员
                    try
                    {
                        //制单人、管理员
                        //string userNumbers = _userService.GetAllAdminMobileNumbersAndThisUsersByUser(bill.MakeUserId, new List<int> { bill.MakeUserId });
                        var userNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();

                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Message,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.ReturnReservationBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                    #endregion
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = billId ?? 0, Message = Resources.Bill_CreateOrUpdateSuccessful, Code = bill.Id };
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

        public BaseResult Auditing(int storeId, int userId, ReturnReservationBill bill)
        {
            var uow = ReturnReservationBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;

                AuditingNoTran(storeId, userId, bill);


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

        public BaseResult AuditingNoTran(int storeId, int userId, ReturnReservationBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };
            try
            {

                #region 修改单据表状态
                bill.AuditedUserId = userId;
                bill.AuditedDate = DateTime.Now;
                bill.AuditedStatus = true;
                UpdateReturnReservationBill(bill);
                #endregion

                #region 发送通知
                try
                {
                    //制单人、管理员
                    var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };

                    var queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.Audited,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                        Date = bill.CreatedOnUtc,
                        BillType = BillTypeEnum.ReturnReservationBill,
                        BillNumber = bill.BillNumber,
                        BillId = bill.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }
                #endregion

                return successful;
            }
            catch (Exception)
            {
                return failed;
            }

        }

        public BaseResult Reverse(int userId, ReturnReservationBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = ReturnReservationBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                #region 修改单据表状态
                bill.ReversedUserId = userId;
                bill.ReversedDate = DateTime.Now;
                bill.ReversedStatus = true;
                UpdateReturnReservationBill(bill);
                #endregion

                //保存事务
                transaction.Commit();

                return successful;
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return failed;
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }
        public BaseResult Delete(int userId, ReturnReservationBill returnReservationBill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = SaleBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                returnReservationBill.Deleted = true;
                #endregion
                UpdateReturnReservationBill(returnReservationBill);

                //保存事务
                transaction.Commit();

                return successful;
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                //return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
                return failed;
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }
    }
}
