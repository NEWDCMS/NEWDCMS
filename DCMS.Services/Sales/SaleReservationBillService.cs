using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.Sales
{
    public class SaleReservationBillService : BaseService, ISaleReservationBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILogger _logger;
        private readonly IDistrictService _districtService;
        private readonly ITerminalService _terminalService;

        public SaleReservationBillService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            ISettingService settingService,
            IProductService productService,
            ILogger logger,
            ISpecificationAttributeService specificationAttributeService,
            IDistrictService districtService,
            ITerminalService terminalService
            ) : base(serviceGetter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _settingService = settingService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _productService = productService;
            _logger = logger;
            _specificationAttributeService = specificationAttributeService;
            _districtService = districtService;
            _terminalService = terminalService;
        }

        #region 销售订单

        public bool Exists(int billId)
        {
            return SaleReservationBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        /// <summary>
        /// 查询当前经销商销售订单
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
        public IPagedList<SaleReservationBill> GetSaleReservationBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue, bool platform = false)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in SaleReservationBillsRepository.Table
                         .Include(cr => cr.Items)
                         //.ThenInclude(cr => cr.SaleReservationBill)
                         .Include(cr => cr.SaleReservationBillAccountings)
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
                var terminalIds = _terminalService.GetTerminalIds(store, terminalName, platform);
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

            //审核状态
            if (auditedStatus.HasValue)
            {
                query = query.Where(a => a.AuditedStatus == auditedStatus);
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
                var terminalIds = _terminalService.GetDisTerminalIds(store,districtId ?? 0);
                query = query.Where(a => terminalIds.Contains(a.TerminalId));
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

            //var plist = new PagedList<SaleReservationBill>(query.ToList(), pageIndex, pageSize);
            //return plist;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<SaleReservationBill>(plists, pageIndex, pageSize, totalCount);
        }


        /*
         https://docs.microsoft.com/en-us/ef/core/change-tracking/relationship-changes#relationship-fixup

         System.InvalidOperationException:“An error was generated for warning 'Microsoft.EntityFrameworkCore.Query.NavigationBaseIncludeIgnored': The navigation 'SaleReservationItem.SaleReservationBill' was ignored from 'Include' in the query since the fix-up will automatically populate it. If any further navigations are specified in 'Include' afterwards then they will be ignored. Walking back include tree is not allowed. This exception can be suppressed or logged by passing event ID 'CoreEventId.NavigationBaseIncludeIgnored' to the 'ConfigureWarnings' method in 'DbContext.OnConfiguring' or 'AddDbContext'.”

         */

        public IList<SaleReservationBill> GetSaleReservationBillsByStoreId(int? storeId)
        {

            return _cacheManager.Get(DCMSDefaults.SALERESERVATIONBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = SaleReservationBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<SaleReservationBill> GetSaleReservationBillByStoreIdTerminalId(int storeId, int terminalId)
        {
            var query = SaleReservationBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.TerminalId == terminalId);

            return query.ToList();
        }


        public IList<SaleReservationBill> GetSaleReservationBillsNullWareHouseByStoreId(int storeId)
        {

            return _cacheManager.Get(DCMSDefaults.SALERESERVATIONBILLNULLWAREHOUSE_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = SaleReservationBillsRepository.Table;
               query = query.Where(a => a.StoreId == storeId);
               query = query.Where(a => a.WareHouseId == 0);
               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<SaleReservationBill> GetHotSaleReservationRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
        {
            var query = SaleReservationBillsRepository.Table;

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
        public IList<SaleReservationBill> GetSaleReservationBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null)
        {
            var query = SaleReservationBillsRepository.Table;

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



        public SaleReservationBill GetSaleReservationBillById(int? store, int saleReservationBillId, bool isInclude = false)
        {
            if (saleReservationBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = SaleReservationBillsRepository.Table
                .Include(sb => sb.Items)
                //.ThenInclude(sb => sb.SaleReservationBill)
                .Include(sb => sb.SaleReservationBillAccountings)
                .ThenInclude(sb => sb.AccountingOption);

                return query.FirstOrDefault(s => s.Id == saleReservationBillId);
            }
            return SaleReservationBillsRepository.ToCachedGetById(saleReservationBillId);
        }

        /// <summary>
        /// 根据销售订单ids查询销售订单列表，不要缓存
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        public IList<SaleReservationBill> GetSaleReservationBillsByIds(int[] sIds, bool isInclude = false)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<SaleReservationBill>();
            }

            if (isInclude)
            {
                var query = SaleReservationBillsRepository.Table
                .Include(sb => sb.Items)
                //.ThenInclude(sb => sb.SaleReservationBill)
                .Include(sb => sb.SaleReservationBillAccountings)
                .ThenInclude(sb => sb.AccountingOption)
                .Where(s=> sIds.Contains(s.Id));
                return query.ToList();
            }
            else 
            {
                var query = from c in SaleReservationBillsRepository.Table
                            where sIds.Contains(c.Id)
                            select c;
                return query.ToList();
            }
        }


        public SaleReservationBill GetSaleReservationBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.SALERESERVATIONBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = SaleReservationBillsRepository.Table;
                var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return bill;
            });
        }

        public int GetBillId(int? store, string billNumber)
        {
            var query = SaleReservationBillsRepository.TableNoTracking;
            var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).Select(s => s.Id).FirstOrDefault();
            return bill;
        }

        public void DeleteSaleReservationBill(SaleReservationBill bill)
        {

            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }

            var uow = SaleReservationBillsRepository.UnitOfWork;
            SaleReservationBillsRepository.Delete(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(bill);

        }

        public void InsertSaleReservationBill(SaleReservationBill bill)
        {

            var uow = SaleReservationBillsRepository.UnitOfWork;
            SaleReservationBillsRepository.Insert(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public void UpdateSaleReservationBill(SaleReservationBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }

            var uow = SaleReservationBillsRepository.UnitOfWork;
            SaleReservationBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);

        }

        /// <summary>
        /// 转单状态
        /// </summary>
        /// <param name="billId"></param>
        /// <param name="userId"></param>
        public void ChangedBill(int billId, int userId)
        {
            var uow = SaleReservationBillsRepository.UnitOfWork;
            var bill = SaleReservationBillsRepository.GetById(billId);
            if (bill != null)
            {
                bill.ChangedUserId = userId;
                bill.ChangedStatus = true;
                bill.ChangedDate = DateTime.Now;
                SaleReservationBillsRepository.Update(bill);
            }
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(bill);
        }


        /// <summary>
        /// 设置销售订单价格
        /// </summary>
        /// <param name="saleReservationBillId"></param>
        public void SetSaleReservationBillAmount(int saleReservationBillId)
        {
            SaleReservationBill bill;
            var query = SaleReservationBillsRepository.Table;
            bill = query.Where(a => a.Id == saleReservationBillId).FirstOrDefault();
            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }
            List<SaleReservationItem> saleReservationItems = GetSaleReservationItemList(saleReservationBillId);
            if (saleReservationItems != null && saleReservationItems.Count > 0)
            {
                //总金额
                decimal SumAmount = saleReservationItems.Sum(a => a.Amount);
                //已收金额（会计科目金额）
                decimal accounting = 0;
                IList<SaleReservationBillAccounting> saleReservationAccountings = GetSaleReservationBillAccountingsBySaleReservationBillId(0, saleReservationBillId);
                if (saleReservationAccountings != null && saleReservationAccountings.Count > 0)
                {
                    accounting = saleReservationAccountings.Sum(a => a.CollectionAmount);
                }
                //总金额
                bill.SumAmount = SumAmount;
                //应收金额=总金额-优惠金额
                //saleReservation.ReceivableAmount = SumAmount - (saleReservation.PreferentialAmount ?? 0);
                //欠款金额=总金额-优惠金额-已收金额
                //saleReservation.OweCash = SumAmount - (saleReservation.PreferentialAmount ?? 0) - accounting;

                //总成本价
                decimal SumCostPrice = saleReservationItems.Sum(a => a.CostPrice);
                bill.SumCostPrice = SumCostPrice;
                //总成本金额
                decimal SumCostAmount = saleReservationItems.Sum(a => a.CostAmount);
                bill.SumCostAmount = SumCostAmount;
                //总利润 = 总金额-总成本金额
                bill.SumProfit = bill.SumAmount - SumCostAmount;
                //成本利润率 = 总利润 / 总成本金额
                var amount = (bill.SumCostAmount == 0) ? bill.SumProfit : bill.SumCostAmount;
                if (amount != 0)
                {
                    bill.SumCostProfitRate = (bill.SumProfit / amount) * 100;
                }

                var uow = SaleReservationBillsRepository.UnitOfWork;
                SaleReservationBillsRepository.Update(bill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(bill);
            }

        }

        public virtual IList<IGrouping<DateTime, SaleReservationBill>> GetSaleReservationBillsAnalysisByCreate(int? storeId, int? user, DateTime date)
        {
            if (user.HasValue && user != 0)
            {
                //var query = from p in SaleReservationBillsRepository.Table
                //            where p.StoreId == storeId && p.MakeUserId == user && p.CreatedOnUtc >= date
                //            group p by p.CreatedOnUtc
                //      into cGroup
                //            orderby cGroup.Key
                //            select cGroup;
                //return query.ToList();
                var query = from p in SaleReservationBillsRepository.Table
                            where p.StoreId == storeId && p.MakeUserId == user && p.CreatedOnUtc >= date
                            select p;

                var result = query.AsEnumerable().GroupBy(t => t.CreatedOnUtc).OrderBy(g => g.Key);

                return result.ToList();
            }
            else
            {
                var query = from p in SaleReservationBillsRepository.Table
                            where p.StoreId == storeId && p.CreatedOnUtc >= date
                            select p;

                var result = query.AsEnumerable().GroupBy(t => t.CreatedOnUtc).OrderBy(g => g.Key);

                return result.ToList();
            }
        }

        #endregion

        #region 销售订单明细

        /// <summary>
        /// 根据销售订单获取项目
        /// </summary>
        /// <param name="saleReservationId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<SaleReservationItem> GetSaleReservationItemBySaleReservationBillId(int saleReservationBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (saleReservationBillId == 0)
            {
                return new PagedList<SaleReservationItem>(new List<SaleReservationItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.SALERESERVATIONBILL_ITEM_ALLBY_SALEID_KEY.FillCacheKey(storeId, saleReservationBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in SaleReservationItemsRepository.Table
                            .Include(sr => sr.SaleReservationBill)
                            where pc.SaleReservationBillId == saleReservationBillId
                            orderby pc.Id
                            select pc;
                //var saleItems = new PagedList<SaleReservationItem>(query.ToList(), pageIndex, pageSize);
                //return saleItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<SaleReservationItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<SaleReservationItem> GetSaleReservationItemList(int saleReservationBillId)
        {
            List<SaleReservationItem> saleItems = null;
            var query = SaleReservationItemsRepository.Table.Include(s=>s.SaleReservationBill);
            saleItems = query.Where(a => a.SaleReservationBillId == saleReservationBillId).ToList();
            return saleItems;
        }

        public int SaleReservationItemQtySum(int storeId, int productId, int saleReservationBillId)
        {
            int qty = 0;
            var query = from saleReservation in SaleReservationBillsRepository.Table
                        join saleReservationItem in SaleReservationItemsRepository.Table on saleReservation.Id equals saleReservationItem.SaleReservationBillId
                        where saleReservation.AuditedStatus == true && saleReservationItem.ProductId == productId
                              && saleReservation.Id != saleReservationBillId    //排除当前销售订单数量
                        select saleReservationItem;
            List<SaleReservationItem> saleItems = query.ToList();
            if (saleItems != null && saleItems.Count > 0)
            {
                qty = saleItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public SaleReservationItem GetSaleReservationItemById(int saleItemId)
        {
            SaleReservationItem saleReservationItem;
            var query = SaleReservationItemsRepository.Table;
            saleReservationItem = query.Where(a => a.Id == saleItemId).FirstOrDefault();
            return saleReservationItem;
        }

        public void DeleteSaleReservationItem(SaleReservationItem saleReservationItem)
        {
            if (saleReservationItem == null)
            {
                throw new ArgumentNullException("salereservationitem");
            }

            var uow = SaleReservationItemsRepository.UnitOfWork;
            SaleReservationItemsRepository.Delete(saleReservationItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(saleReservationItem);
        }

        public void InsertSaleReservationItem(SaleReservationItem saleReservationItem)
        {
            var uow = SaleReservationItemsRepository.UnitOfWork;
            SaleReservationItemsRepository.Insert(saleReservationItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(saleReservationItem);
        }

        public void UpdateSaleReservationItem(SaleReservationItem saleReservationItem)
        {
            if (saleReservationItem == null)
            {
                throw new ArgumentNullException("salereservationitem");
            }

            var uow = SaleReservationItemsRepository.UnitOfWork;
            SaleReservationItemsRepository.Update(saleReservationItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(saleReservationItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IList<SaleReservationBillAccounting> GetSaleReservationBillAccountingsBySaleReservationBillId(int? store, int saleReservationBillId)
        {

            var query = from pc in SaleReservationBillAccountingMappingRepository.Table
                        join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                        where pc.BillId == saleReservationBillId
                        orderby pc.Id
                        select pc;


            return query.Distinct().ToList();
        }

        public virtual SaleReservationBillAccounting GetSaleReservationBillAccountingById(int saleReservationBillAccountingId)
        {
            if (saleReservationBillAccountingId == 0)
            {
                return null;
            }

            return SaleReservationBillAccountingMappingRepository.ToCachedGetById(saleReservationBillAccountingId);
        }

        public virtual void InsertSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting)
        {
            if (saleReservationBillAccounting == null)
            {
                throw new ArgumentNullException("salereservationbillaccounting");
            }

            var uow = SaleReservationBillAccountingMappingRepository.UnitOfWork;
            SaleReservationBillAccountingMappingRepository.Insert(saleReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(saleReservationBillAccounting);
        }

        public virtual void UpdateSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting)
        {
            if (saleReservationBillAccounting == null)
            {
                throw new ArgumentNullException("salereservationbillaccounting");
            }

            var uow = SaleReservationBillAccountingMappingRepository.UnitOfWork;
            SaleReservationBillAccountingMappingRepository.Update(saleReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(saleReservationBillAccounting);
        }

        public virtual void DeleteSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting)
        {
            if (saleReservationBillAccounting == null)
            {
                throw new ArgumentNullException("salereservationbillaccounting");
            }

            var uow = SaleReservationBillAccountingMappingRepository.UnitOfWork;
            SaleReservationBillAccountingMappingRepository.Delete(saleReservationBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(saleReservationBillAccounting);
        }

        public IList<SaleReservationBillAccounting> GetAllSaleReservationBillAccountingsByBillIds(int? store, int[] billIds, bool platform = false)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<SaleReservationBillAccounting>();
            }

            var key = DCMSDefaults.SALERESERVATIONBILL_ACCOUNTINGL_BY_SALEID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in
SaleReservationBillAccountingMappingRepository.Table
.Include(sb => sb.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                if (platform == true)
                {
                    query = from pc in
SaleReservationBillAccountingMappingRepository_RO.TableNoTracking
.Include(sb => sb.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                }

                return query.ToList();
            });
        }

        #endregion


        /// <summary>
        /// 获取当前销售订单所有商品 最小单位数量
        /// </summary>
        /// <param name="saleReservationBillId"></param>
        /// <returns></returns>
        public int GetSumQuantityBySaleReservationBillId(int storeId, ISpecificationAttributeService _specificationAttributeService, IProductService _productService, int saleReservationBillId)
        {
            int qty = 0;
            List<SaleReservationItem> saleReservationItems = GetSaleReservationItemList(saleReservationBillId);
            if (saleReservationItems != null && saleReservationItems.Count > 0)
            {
                var allProducts = _productService.GetProductsByIds(storeId, saleReservationItems.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, allProducts.GetProductBigStrokeSmallUnitIds());

                saleReservationItems.ForEach(a =>
                {
                    var product = allProducts.Where(ap => ap.Id == a.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        //商品转化量
                        var conversionQuantity = product.GetConversionQuantity(allOptions, a.UnitId);
                        //库存量增量 = 单位转化量 * 数量
                        int thisQuantity = a.Quantity * conversionQuantity;
                        qty += thisQuantity;
                    }
                });
            }
            return qty;
        }

        public IList<SaleReservationBill> GetSaleReservationBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null)
        {
            var query = SaleReservationBillsRepository.Table;

            //已审核、未红冲、未转单
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
            if (districtId != null && districtId.Value != 0)
            {
                //递归片区查询
                var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                if (distinctIds != null && distinctIds.Count > 0)
                {
                    string inDistinctIds = string.Join("','", distinctIds);
                    query = query.Where(a => distinctIds.Contains(a.DistrictId));
                }
                else
                {
                    query = query.Where(a => a.DistrictId == districtId);
                }
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

            //未调度
            query = query.Where(a => a.DispatchedStatus == false);

            //未转单
            query = query.Where(a => a.ChangedStatus == false);

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.Include(s=>s.Items).ToList();
        }


        /// <summary>
        /// 销售订单转销售单查询
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
        public IPagedList<SaleReservationBill> GetSaleReservationBillToChangeList(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = SaleReservationBillsRepository.Table;

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


            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<SaleReservationBill>(plists, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// 车辆对货单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<SaleReservationBill> GetSaleReservationBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        join c in SaleReservationBillsRepository.Table.Include(sb => sb.Items) on b.BillId equals c.Id
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.SaleReservationBill
                        && (b.SignStatus == (int)SignStatusEnum.Done || b.SignStatus == (int)SignStatusEnum.Rejection)
                        select c;

            //经销商
            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(q => q.StoreId == storeId);
            }
            //业务员
            if (deliveryUserId != null && deliveryUserId != 0)
            {
                query = query.Where(q => q.DeliveryUserId == deliveryUserId);
            }
            //开始时间
            if (start != null)
            {
                query = query.Where(q => q.CreatedOnUtc >= start);
            }

            //结束时间
            if (end != null)
            {
                query = query.Where(q => q.CreatedOnUtc <= end);
            }

            query = query.OrderByDescending(q => q.CreatedOnUtc);
            return query.ToList();

        }

        /// <summary>
        /// 仓库分拣
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="remark"></param>
        /// <param name="pickingFilter"></param>
        /// <param name="wholeScrapStatus"></param>
        /// <param name="scrapStatus"></param>
        /// <param name=""></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<SaleReservationBill> GetSaleReservationBillsToPicking(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, string remark = "", int pickingFilter = 0, int? wholeScrapStatus = 0, int? scrapStatus = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = SaleReservationBillsRepository.Table;

            //审核、未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //业务员
            if (businessUserId != null && businessUserId != 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
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

            if (pickingFilter == 1)
            {
                //待整箱合并拆零拣货
                if (wholeScrapStatus == 1)
                {
                    query = query.Where(a => a.PickingWholeScrapStatus == false || a.PickingWholeScrapStatus == null);
                }
                //已整箱合并拆零拣货
                else if (wholeScrapStatus == 2)
                {
                    query = query.Where(a => a.PickingWholeScrapStatus == true);
                }
                else
                {
                    query = query.Where(a => a.Id == -1);
                }

            }
            else if (pickingFilter == 0)
            {
                //if(scrapStatus==1)
                query = scrapStatus switch
                {
                    //待拆零拣货
                    1 => query.Where(a => a.PickingScrapStatus == false || a.PickingScrapStatus == null),
                    //待整箱拣货
                    2 => query.Where(a => a.PickingWholeStatus == false || a.PickingWholeStatus == null),
                    //已拆零拣货
                    3 => query.Where(a => a.PickingScrapStatus == true),
                    //已整箱拣货
                    4 => query.Where(a => a.PickingWholeStatus == true),
                    _ => query.Where(a => a.Id == -1),
                };
            }


            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }


        /// <summary>
        /// 删除历史单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        public void UpdateSaleReservationBillActive(int? store, int? billId, int? user)
        {
            var query = SaleReservationBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = DCMS_UOW;
                foreach (SaleReservationBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    SaleReservationBillsRepository.Update(bill);
                }
                uow.SaveChanges();
            }
        }




        /// <summary>
        /// V2 单据创建业务逻辑，业务逻辑不应该包含在Controller 中因为不符合OOP/AOP的设计原则
        /// </summary>
        public BaseResult BillCreateOrUpdate(int storeId,
            int userId,
            int? billId,
            SaleReservationBill bill,
            List<SaleReservationBillAccounting> accountingOptions,
            List<AccountingOption> accountings,
            SaleReservationBillUpdate data,
            List<SaleReservationItem> items,
            List<ProductStockItem> productStockItemThiss,
            bool isAdmin = false, bool doAudit = true)
        {
            //var uow = SaleReservationBillsRepository.UnitOfWork;
            var uow = DCMS_UOW;


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
                    #region 更新销售

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.PayTypeId = data.PayTypeId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitSale = data.IsMinUnitSale;
                    bill.Remark = data.Remark;
                    bill.DefaultAmountId = data.DefaultAmountId;
                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.OweCash = data.OweCash;

                    //配送信息
                    bill.DeliverDate = data.DeliverDate;
                    bill.AMTimeRange = data.AMTimeRange;
                    bill.PMTimeRange = data.PMTimeRange;

                    //判断是否有欠款
                    if (data.OweCash == 0)
                    {
                        bill.Receipted = true;
                    }

                    //计算金额
                    if (items != null && items.Count > 0)
                    {
                        //总金额
                        //decimal SumAmount = items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
                        //总金额
                        bill.SumAmount = items.Sum(a => a.Amount);
                        //总成本价
                        decimal SumCostPrice = items.Sum(a => a.CostPrice);
                        bill.SumCostPrice = SumCostPrice;
                        //总成本金额
                        decimal SumCostAmount = items.Sum(a => a.CostAmount);
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

                    UpdateSaleReservationBill(bill);

                    #endregion

                }
                else
                {
                    #region 添加销售

                    bill.BillType = BillTypeEnum.SaleReservationBill;
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

                    var sb = GetSaleReservationBillByNumber(storeId, bill.BillNumber);
                    if (sb != null)
                    {
                        return new BaseResult { Success = false, Message = "操作失败，重复提交" };
                    }

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    //bill.DeliveryUserId = data.DeliveryUserId;
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

                    //判断是否有欠款
                    if (data.OweCash == 0)
                    {
                        bill.Receipted = true;
                    }

                    //计算金额
                    if (items != null && items.Count > 0)
                    {
                        //总金额
                        //decimal SumAmount = items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
                        //总金额
                        bill.SumAmount = items.Sum(a => a.Amount);
                        //总成本价
                        decimal SumCostPrice = items.Sum(a => a.CostPrice);
                        bill.SumCostPrice = SumCostPrice;
                        //总成本金额
                        decimal SumCostAmount = items.Sum(a => a.CostAmount);
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

                    InsertSaleReservationBill(bill);
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
                    //            BillType = BillTypeEnum.SaleReservationBill,
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

                #region 更新销售项目

                items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var sd = GetSaleReservationItemById(p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.SaleReservationBillId = billId.Value;
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
                                InsertSaleReservationItem(item);
                                //不排除
                                p.Id = item.Id;
                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            sd.ProductId = p.ProductId;
                            sd.UnitId = p.UnitId;
                            sd.Quantity = p.Quantity;
                            sd.RemainderQty = p.RemainderQty;
                            sd.Price = p.Price;
                            sd.Amount = p.Amount;
                            //有税率，则价格=含税价格，金额=含税金额
                            if (sd.TaxRate > 0 && companySetting.EnableTaxRate)
                            {
                                sd.Price = p.Price * (1 + p.TaxRate / 100);
                                sd.Amount = p.Amount * (1 + p.TaxRate / 100);
                            }

                            //成本价
                            sd.CostPrice = p.CostPrice;
                            //成本金额
                            sd.CostAmount = p.CostAmount;
                            //利润 = 金额 - 成本金额
                            sd.Profit = sd.Amount - sd.CostAmount;
                            //成本利润率 = 利润 / 成本金额
                            if (sd.CostAmount == 0)
                            {
                                sd.CostProfitRate = 100;
                            }
                            else
                            {
                                sd.CostProfitRate = sd.Profit / sd.CostAmount * 100;
                            }

                            sd.StockQty = p.StockQty;
                            sd.Remark = p.Remark;
                            sd.RemarkConfigId = p.RemarkConfigId;
                            sd.RemainderQty = p.RemainderQty;
                            sd.ManufactureDete = p.ManufactureDete;

                            //2019-07-25
                            sd.IsGifts = p.IsGifts;
                            sd.BigGiftQuantity = p.BigGiftQuantity;
                            sd.SmallGiftQuantity = p.SmallGiftQuantity;

                            //赠送商品信息
                            sd.SaleProductTypeId = p.SaleProductTypeId;
                            sd.GiveTypeId = p.GiveTypeId;
                            sd.CampaignId = p.CampaignId;
                            sd.CampaignBuyProductId = p.CampaignBuyProductId;
                            sd.CampaignGiveProductId = p.CampaignGiveProductId;
                            sd.CostContractId = p.CostContractId;
                            sd.CostContractItemId = p.CostContractItemId;
                            sd.CostContractMonth = p.CostContractMonth;
                            sd.CampaignLinkNumber = p.CampaignLinkNumber;

                            UpdateSaleReservationItem(sd);
                        }
                    }
                });

                #endregion

                #region Grid 移除则从库移除删除项

                bill.Items.ToList().ForEach(p =>
                {
                    if (items.Count(cp => cp.Id == p.Id) == 0)
                    {
                        bill.Items.Remove(p);
                        var sd = GetSaleReservationItemById(p.Id);
                        if (sd != null)
                        {
                            DeleteSaleReservationItem(sd);
                        }
                    }
                });

                #endregion

                #region 收款账户映射

                var saleReservationAccountings = GetSaleReservationBillAccountingsBySaleReservationBillId(storeId, bill.Id);

                accountings.ToList().ForEach(c =>
                {
                    if (data != null && accountingOptions != null && accountingOptions.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!saleReservationAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = accountingOptions.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var saleReservationAccounting = new SaleReservationBillAccounting()
                            {
                                StoreId = storeId,
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                SaleReservationBill = bill,
                                TerminalId = data.TerminalId,
                                BillId = bill.Id
                            };
                            //添加账户
                            InsertSaleReservationBillAccounting(saleReservationAccounting);
                        }
                        else
                        {
                            saleReservationAccountings.ToList().ForEach(acc =>
                            {
                                //var collection = accountingOptions.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                //acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                //acc.TerminalId = data.TerminalId;

                                //更新账户
                                //UpdateSaleReservationBillAccounting(acc);

                                var sbas = saleReservationAccountings.ToList();
                                if (sbas != null && sbas.Any())
                                {
                                    foreach (var sba in sbas)
                                    {
                                        var collection = data.Accounting
                                              .Select(a => a)
                                              .Where(a => a.AccountingOptionId == sba.AccountingOptionId)
                                              .FirstOrDefault();

                                        var cur = SaleReservationBillAccountingMappingRepository.Table.Where(s => s.Id == sba.Id).FirstOrDefault();
                                        if (cur != null)
                                        {
                                            cur.CollectionAmount = collection?.CollectionAmount ?? 0;
                                            cur.TerminalId = data.TerminalId;
                                            UpdateSaleReservationBillAccounting(cur);
                                        }
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        if (saleReservationAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var saleaccountings = saleReservationAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            saleaccountings.ForEach(sa =>
                            {
                                DeleteSaleReservationBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion


                //判断App开单是否自动审核
                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.SaleReservationBill);
                }
                //读取配置自动审核、管理员创建自动审核
                if ((isAdmin && doAudit) || appBillAutoAudits)
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
                            BillType = BillTypeEnum.SaleReservationBill,
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
            finally
            {
                using (transaction) { }
            }

        }

        public BaseResult Auditing(int storeId, int userId, SaleReservationBill bill)
        {
            var uow = DCMS_UOW;

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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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

        public BaseResult AuditingNoTran(int storeId, int userId, SaleReservationBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {

                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                #region 预占库存
                try
                {
                    //如果此销售订单没有转单，
                    //将一个单据中 相同商品 数量 按最小单位汇总
                    var stockProducts = new List<ProductStockItem>();
                    var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                    foreach (SaleReservationItem item in bill.Items)
                    {
                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                        ProductStockItem productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
                        //商品转化量
                        var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                        //库存量增量 = 单位转化量 * 数量
                        int thisQuantity = item.Quantity * conversionQuantity;
                        if (productStockItem != null)
                        {
                            productStockItem.Quantity += thisQuantity;
                        }
                        else
                        {
                            productStockItem = new ProductStockItem
                            {
                                ProductId = item.ProductId,
                                UnitId = item.UnitId,
                                SmallUnitId = product.SmallUnitId,
                                BigUnitId = product.BigUnitId ?? 0,
                                ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                Quantity = thisQuantity
                            };

                            stockProducts.Add(productStockItem);
                        }
                        //修改消单状态
                        product.HasSold = true;
                        _productService.UpdateProduct(product);
                    }

                    List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
                    if (stockProducts != null && stockProducts.Count > 0)
                    {
                        stockProducts.ForEach(psi =>
                        {
                            ProductStockItem productStockItem = new ProductStockItem
                            {
                                ProductId = psi.ProductId,
                                UnitId = psi.UnitId,
                                SmallUnitId = psi.SmallUnitId,
                                BigUnitId = psi.BigUnitId,
                                ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
                                Quantity = psi.Quantity
                            };
                            productStockItemThiss.Add(productStockItem);
                        });
                    }

                    historyDatas1 = _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(bill, DirectionEnum.Out, StockQuantityType.OrderQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

                }
                catch (Exception)
                {
                }
                #endregion

                #region 记录上次售价
                if (bill != null && bill.Items != null && bill.Items.Count > 0 && bill.Items.Where(it => it.Price > 0).Count() > 0)
                {
                    List<RecordProductPrice> recordProductPrices = new List<RecordProductPrice>();
                    bill.Items.ToList().ForEach(it =>
                    {
                        recordProductPrices.Add(new RecordProductPrice()
                        {
                            StoreId = bill.StoreId,
                            TerminalId = bill.TerminalId,
                            ProductId = it.ProductId,
                            UnitId = it.UnitId,
                            //Price = it.Price
                            Price = bill.TaxAmount>0 ? it.Price / (1 + it.TaxRate / 100) : it.Price //注意这里记录税前价格，因为税率用户可以修改，有可能不等于配置税率
                        });
                    });
                    //暂时不用这个
                    //_productService.RecordProductTierPriceLastPrice(recordProductPrices);
                    _productService.RecordRecentPriceLastPrice(bill.StoreId, recordProductPrices);
                }
                #endregion

                #region 修改单据表状态
                bill.AuditedUserId = userId;
                bill.AuditedDate = DateTime.Now;
                bill.AuditedStatus = true;

                //如果欠款小于等于0，则单据已收款
                if (bill.OweCash <= 0)
                {
                    bill.Receipted = true;
                }
                UpdateSaleReservationBill(bill);
                #endregion

                #region 发送通知
                try
                {
                    //制单人、管理员
                    var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };
                    QueuedMessage queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.Audited,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                        Date = bill.CreatedOnUtc,
                        BillType = BillTypeEnum.SaleReservationBill,
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                return failed;
            }

        }

        public BaseResult Reverse(int userId, SaleReservationBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };
            var uow = DCMS_UOW;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                #region 预占释放
                try
                {
                    if (bill.ChangedStatus == false)
                    {
                        //如果此销售订单没有转单，
                        //将一个单据中 相同商品 数量 按最小单位汇总
                        var stockProducts = new List<ProductStockItem>();
                        var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (SaleReservationItem item in bill.Items)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            ProductStockItem productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
                            //商品转化量
                            var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                            //库存量增量 = 单位转化量 * 数量
                            int thisQuantity = item.Quantity * conversionQuantity;
                            if (productStockItem != null)
                            {
                                productStockItem.Quantity += thisQuantity;
                            }
                            else
                            {
                                productStockItem = new ProductStockItem
                                {
                                    ProductId = item.ProductId,
                                    UnitId = item.UnitId,
                                    SmallUnitId = product.SmallUnitId,
                                    BigUnitId = product.BigUnitId ?? 0,
                                    ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                    ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                    Quantity = thisQuantity
                                };

                                stockProducts.Add(productStockItem);
                            }
                        }

                        List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
                        if (stockProducts != null && stockProducts.Count > 0)
                        {
                            stockProducts.ForEach(psi =>
                            {
                                ProductStockItem productStockItem = new ProductStockItem
                                {
                                    ProductId = psi.ProductId,
                                    UnitId = psi.UnitId,
                                    SmallUnitId = psi.SmallUnitId,
                                    BigUnitId = psi.BigUnitId,
                                    ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
                                    ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
                                    Quantity = psi.Quantity * (-1)
                                };
                                productStockItemThiss.Add(productStockItem);
                            });
                        }

                        //销售订单释放预占

                        historyDatas1 = _stockService.AdjustStockQty<SaleReservationBill, SaleReservationItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

                    }
                }
                catch (Exception)
                {
                }
                #endregion

                #region 修改单据表状态
                bill.ReversedUserId = userId;
                bill.ReversedDate = DateTime.Now;
                bill.ReversedStatus = true;
                UpdateSaleReservationBill(bill);
                #endregion

                //保存事务
                transaction.Commit();
                return successful;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

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

        public BaseResult Delete(int userId, SaleReservationBill saleReservationBill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = SaleBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                saleReservationBill.Deleted = true;
                #endregion
                UpdateSaleReservationBill(saleReservationBill);

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
