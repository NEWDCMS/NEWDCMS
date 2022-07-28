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
    public class ExchangeBillService : BaseService, IExchangeBillService
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

        public ExchangeBillService(IServiceGetter serviceGetter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IFinanceReceiveAccountBillService financeReceiveAccountBillService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            ISettingService settingService,
            IProductService productService,
            ILogger logger,
            ISpecificationAttributeService specificationAttributeService,
            ICostContractBillService costContractBillService,
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

        #region 换货单

        public bool Exists(int billId)
        {
            return ExchangeBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public IPagedList<ExchangeBill> GetExchangeBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue, bool platform = false)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in ExchangeBillsRepository.Table
                         .Include(cr => cr.Items)
                         //.ThenInclude(cr => cr.ExchangeBill)
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
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ExchangeBill>(plists, pageIndex, pageSize, totalCount);
        }

        public IList<ExchangeBill> GetExchangeBillsByStoreId(int? storeId)
        {

            return _cacheManager.Get(DCMSDefaults.EXCHANGEBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ExchangeBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<ExchangeBill> GetExchangeBillByStoreIdTerminalId(int storeId, int terminalId)
        {
            var query = ExchangeBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.TerminalId == terminalId);

            return query.ToList();
        }


        public IList<ExchangeBill> GetExchangeBillsNullWareHouseByStoreId(int storeId)
        {
            return _cacheManager.Get(DCMSDefaults.EXCHANGEBILLNULLWAREHOUSE_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ExchangeBillsRepository.Table;
               query = query.Where(a => a.StoreId == storeId);
               query = query.Where(a => a.WareHouseId == 0);
               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

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
        public IList<ExchangeBill> GetExchangeBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null)
        {
            var query = ExchangeBillsRepository.Table;

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



        public ExchangeBill GetExchangeBillById(int? store, int exchangeBillId, bool isInclude = false)
        {
            if (exchangeBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = ExchangeBillsRepository.Table
                .Include(sb => sb.Items);
                //.ThenInclude(sb => sb.ExchangeBill);

                return query.FirstOrDefault(s => s.Id == exchangeBillId);
            }
            return ExchangeBillsRepository.ToCachedGetById(exchangeBillId);
        }

        /// <summary>
        /// 根据销售订单ids查询销售订单列表，不要缓存
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        public IList<ExchangeBill> GetExchangeBillsByIds(int[] sIds, bool isInclude = false)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<ExchangeBill>();
            }

            if (isInclude)
            {
                var query = ExchangeBillsRepository.Table
                .Include(sb => sb.Items)
                .Where(s => sIds.Contains(s.Id)); ;
                return query.ToList();
            }
            else
            {
                var query = from c in ExchangeBillsRepository.Table
                            where sIds.Contains(c.Id)
                            select c;
                var list = query.ToList();
                return list;
            }
        }


        public ExchangeBill GetExchangeBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.EXCHANGEBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = ExchangeBillsRepository.Table;
                var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return bill;
            });
        }

        public int GetBillId(int? store, string billNumber)
        {
            var query = ExchangeBillsRepository.TableNoTracking;
            var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).Select(s => s.Id).FirstOrDefault();
            return bill;
        }

        public void DeleteExchangeBill(ExchangeBill bill)
        {

            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }

            var uow = ExchangeBillsRepository.UnitOfWork;
            ExchangeBillsRepository.Delete(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(bill);

        }

        public void InsertExchangeBill(ExchangeBill bill)
        {

            var uow = ExchangeBillsRepository.UnitOfWork;
            ExchangeBillsRepository.Insert(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public void UpdateExchangeBill(ExchangeBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }

            var uow = ExchangeBillsRepository.UnitOfWork;
            ExchangeBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);

        }


        public void ChangedBill(int billId, int userId)
        {
            var uow = ExchangeBillsRepository.UnitOfWork;
            var bill = ExchangeBillsRepository.GetById(billId);
            if (bill != null)
            {
                bill.ChangedUserId = userId;
                bill.ChangedStatus = true;
                bill.ChangedDate = DateTime.Now;
                ExchangeBillsRepository.Update(bill);
            }
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(bill);
        }


        /// <summary>
        /// 设置销售订单价格
        /// </summary>
        /// <param name="exchangeBillId"></param>
        public void SetExchangeBillAmount(int exchangeBillId)
        {
            ExchangeBill bill;
            var query = ExchangeBillsRepository.Table;
            bill = query.Where(a => a.Id == exchangeBillId).FirstOrDefault();
            if (bill == null)
            {
                throw new ArgumentNullException("salereservationbill");
            }
            List<ExchangeItem> exchangeItems = GetExchangeItemList(exchangeBillId);
            if (exchangeItems != null && exchangeItems.Count > 0)
            {
                //总金额
                decimal SumAmount = exchangeItems.Sum(a => a.Amount);
                bill.SumAmount = SumAmount;
                //总成本价
                decimal SumCostPrice = exchangeItems.Sum(a => a.CostPrice);
                bill.SumCostPrice = SumCostPrice;
                //总成本金额
                decimal SumCostAmount = exchangeItems.Sum(a => a.CostAmount);
                bill.SumCostAmount = SumCostAmount;
                //总利润 = 总金额-总成本金额
                bill.SumProfit = bill.SumAmount - SumCostAmount;
                //成本利润率 = 总利润 / 总成本金额
                var amount = (bill.SumCostAmount == 0) ? bill.SumProfit : bill.SumCostAmount;
                if (amount != 0)
                {
                    bill.SumCostProfitRate = (bill.SumProfit / amount) * 100;
                }

                var uow = ExchangeBillsRepository.UnitOfWork;
                ExchangeBillsRepository.Update(bill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(bill);
            }

        }

        public virtual IList<IGrouping<DateTime, ExchangeBill>> GetExchangeBillsAnalysisByCreate(int? storeId, int? user, DateTime date)
        {
            if (user.HasValue && user != 0)
            {
                var query = from p in ExchangeBillsRepository.Table
                            where p.StoreId == storeId && p.MakeUserId == user && p.CreatedOnUtc >= date
                            select p;

                var result = query.AsEnumerable().GroupBy(t => t.CreatedOnUtc).OrderBy(g => g.Key);

                return result.ToList();
            }
            else
            {
                var query = from p in ExchangeBillsRepository.Table
                            where p.StoreId == storeId && p.CreatedOnUtc >= date
                            select p;

                var result = query.AsEnumerable().GroupBy(t => t.CreatedOnUtc).OrderBy(g => g.Key);

                return result.ToList();
            }
        }

        #endregion

        #region 换货明细

        /// <summary>
        /// 根据销售订单获取项目
        /// </summary>
        /// <param name="exchangeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<ExchangeItem> GetExchangeItemByExchangeBillId(int exchangeBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (exchangeBillId == 0)
            {
                return new PagedList<ExchangeItem>(new List<ExchangeItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.EXCHANGEBILL_ITEM_ALLBY_SALEID_KEY.FillCacheKey(storeId, exchangeBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ExchangeItemsRepository.Table
                            .Include(sr => sr.ExchangeBill)
                            where pc.ExchangeBillId == exchangeBillId
                            orderby pc.Id
                            select pc;
                //var exchangeItems = new PagedList<ExchangeItem>(query.ToList(), pageIndex, pageSize);
                //return exchangeItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ExchangeItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<ExchangeItem> GetExchangeItemList(int exchangeBillId)
        {
            List<ExchangeItem> exchangeItems = null;
            var query = ExchangeItemsRepository.Table.Include(s => s.ExchangeBill);
            exchangeItems = query.Where(a => a.ExchangeBillId == exchangeBillId).ToList();
            return exchangeItems;
        }

        public int ExchangeItemQtySum(int storeId, int productId, int exchangeBillId)
        {
            int qty = 0;
            var query = from exchange in ExchangeBillsRepository.Table
                        join exchangeItem in ExchangeItemsRepository.Table on exchange.Id equals exchangeItem.ExchangeBillId
                        where exchange.AuditedStatus == true && exchangeItem.ProductId == productId
                              && exchange.Id != exchangeBillId
                        select exchangeItem;
            List<ExchangeItem> exchangeItems = query.ToList();
            if (exchangeItems != null && exchangeItems.Count > 0)
            {
                qty = exchangeItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public ExchangeItem GetExchangeItemById(int exchangeItemId)
        {
            ExchangeItem exchangeItem;
            var query = ExchangeItemsRepository.Table;
            exchangeItem = query.Where(a => a.Id == exchangeItemId).FirstOrDefault();
            return exchangeItem;
        }

        public void DeleteExchangeItem(ExchangeItem exchangeItem)
        {
            if (exchangeItem == null)
            {
                throw new ArgumentNullException("salereservationitem");
            }

            var uow = ExchangeItemsRepository.UnitOfWork;
            ExchangeItemsRepository.Delete(exchangeItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(exchangeItem);
        }

        public void InsertExchangeItem(ExchangeItem exchangeItem)
        {
            var uow = ExchangeItemsRepository.UnitOfWork;
            ExchangeItemsRepository.Insert(exchangeItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(exchangeItem);
        }

        public void UpdateExchangeItem(ExchangeItem exchangeItem)
        {
            if (exchangeItem == null)
            {
                throw new ArgumentNullException("salereservationitem");
            }

            var uow = ExchangeItemsRepository.UnitOfWork;
            ExchangeItemsRepository.Update(exchangeItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(exchangeItem);
        }


        #endregion


        public int GetSumQuantityByExchangeBillId(int storeId, ISpecificationAttributeService _specificationAttributeService, IProductService _productService, int exchangeBillId)
        {
            int qty = 0;
            List<ExchangeItem> exchangeItems = GetExchangeItemList(exchangeBillId);
            if (exchangeItems != null && exchangeItems.Count > 0)
            {
                var allProducts = _productService.GetProductsByIds(storeId, exchangeItems.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, allProducts.GetProductBigStrokeSmallUnitIds());

                exchangeItems.ForEach(a =>
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

        /// <summary>
        /// 获取调度
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="makeuserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="districtId"></param>
        /// <param name="terminalId"></param>
        /// <param name="billNumber"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="showDispatchReserved"></param>
        /// <param name="dispatchStatus"></param>
        /// <returns></returns>
        public IList<ExchangeBill> GetExchangeBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null)
        {
            var query = ExchangeBillsRepository.Table;

            //已审核、未红冲、未转单
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false && a.ChangedStatus == false);

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

            return query.Include(s => s.Items).ToList();
        }


        public IPagedList<ExchangeBill> GetExchangeBillToChangeList(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ExchangeBillsRepository.Table;

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
            return new PagedList<ExchangeBill>(plists, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// 车辆对货单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<ExchangeBill> GetExchangeBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var query = from a in DispatchBillsRepository.Table
                        join b in DispatchItemsRepository.Table on a.Id equals b.DispatchBillId
                        join c in ExchangeBillsRepository.Table.Include(sb => sb.Items) on b.BillId equals c.Id
                        where a.ReversedStatus == false
                        && b.BillTypeId == (int)BillTypeEnum.ExchangeBill
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
        public IList<ExchangeBill> GetExchangeBillsToPicking(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, string remark = "", int pickingFilter = 0, int? wholeScrapStatus = 0, int? scrapStatus = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = ExchangeBillsRepository.Table;

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

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }


        /// <summary>
        /// 删除历史单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        public void UpdateExchangeBillActive(int? store, int? billId, int? user)
        {
            var query = ExchangeBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = DCMS_UOW;
                foreach (ExchangeBill bill in result)
                {
                    if (!bill.Deleted)
                    {
                        bill.Deleted = true;
                        ExchangeBillsRepository.Update(bill);
                    }
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
            ExchangeBill bill,
            List<AccountingOption> accountings,
            ExchangeBillUpdate data,
            List<ExchangeItem> items,
            List<ProductStockItem> productStockItemThiss,
            bool isAdmin = false, bool doAudit = true)
        {
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
                    bill.DeliveryUserId = data.DeliveryUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.PayTypeId = data.PayTypeId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitSale = data.IsMinUnitSale;
                    bill.Remark = data.Remark;
                    bill.DefaultAmountId = data.DefaultAmountId;
                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.OweCash = data.OweCash;

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
                    }
                    else
                    {
                        bill.SumAmount = 0;
                        bill.SumCostPrice = 0;
                        bill.SumCostAmount = 0;
                        bill.SumProfit = 0;
                        bill.SumCostProfitRate = 0;
                    }

                    UpdateExchangeBill(bill);

                    #endregion

                }
                else
                {
                    #region 添加销售

                    bill.BillType = BillTypeEnum.ExchangeBill;

                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

                    var sb = GetExchangeBillByNumber(storeId, bill.BillNumber);
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
                    bill.Operation = data.Operation;

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
                    }
                    else
                    {
                        bill.SumAmount = 0;
                        bill.SumCostPrice = 0;
                        bill.SumCostAmount = 0;
                        bill.SumProfit = 0;
                        bill.SumCostProfitRate = 0;
                    }

                    InsertExchangeBill(bill);
                    billId = bill.Id;

                    #endregion

                }

                #region 更新项目

                items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var sd = GetExchangeItemById(p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.ExchangeBillId = billId.Value;
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
                                InsertExchangeItem(item);
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

                            UpdateExchangeItem(sd);
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
                        var sd = GetExchangeItemById(p.Id);
                        if (sd != null)
                        {
                            DeleteExchangeItem(sd);
                        }
                    }
                });

                #endregion

                //判断App开单是否自动审核
                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.ExchangeBill);

                }
                //读取配置自动审核、管理员创建自动审核
                if ((isAdmin && doAudit) || appBillAutoAudits || companySetting.SubmitExchangeBillAutoAudits)
                {
                    AuditingNoTran(storeId, userId, bill);
                }
                else
                {
                    #region 发送通知 管理员
                    try
                    {
                        var userNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();

                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Message,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.ExchangeBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage);
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

        public BaseResult Auditing(int storeId, int userId, ExchangeBill bill)
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

        public BaseResult AuditingNoTran(int storeId, int userId, ExchangeBill bill)
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
                    foreach (ExchangeItem item in bill.Items)
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
                                Quantity = psi.Quantity
                            };
                            productStockItemThiss.Add(productStockItem);
                        });
                    }

                    historyDatas1 = _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(bill, DirectionEnum.Out, StockQuantityType.OrderQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

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
                            Price = it.Price
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
                    //bill.Receipted = true;
                }
                UpdateExchangeBill(bill);
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
                        BillType = BillTypeEnum.ExchangeBill,
                        BillNumber = bill.BillNumber,
                        BillId = bill.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage);
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

        public BaseResult Reverse(int userId, ExchangeBill bill)
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
                        foreach (ExchangeItem item in bill.Items)
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

                        historyDatas1 = _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Save);

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
                UpdateExchangeBill(bill);
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

        public BaseResult Delete(int userId, ExchangeBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = SaleBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                bill.Deleted = true;
                #endregion
                UpdateExchangeBill(bill);

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

        /// <summary>
        /// 用于换货单换货确认
        /// </summary>
        /// <param name="bill"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public BaseResult ExchangeSignIn(int storeId, int userId, ExchangeBill bill, DispatchItem dispatchItem, List<RetainPhoto> photos, string signature)
        {
            var uow = DCMS_UOW;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                if (bill.Id > 0 && bill.Operation == (int)OperationEnum.APP && dispatchItem != null)
                {
                    #region 更新调度项目的签收状态

                    dispatchItem.SignStatus = 1;
                    dispatchItem.SignUserId = userId;
                    dispatchItem.SignDate = DateTime.Now;
                    dispatchItem.TerminalId = bill.TerminalId;
                    //更新状态
                    UpdateDispatchItem(dispatchItem);

                    #endregion

                    if (dispatchItem.SignStatus == 1)
                    {
                        var chageFailed = false;

                        #region 释放换货预占库存，增加当前库存

                        //换货单 
                        if (bill != null && bill.Items != null && bill.Items.Count > 0)
                        {
                            //将一个单据中 相同商品 数量 按最小单位汇总
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService
                                .GetProductsByIds(storeId, bill.Items.Select(pr => pr.ProductId)
                                .Distinct()
                                .ToArray());

                            var allOptions = _specificationAttributeService
                                .GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in bill.Items)
                            {
                                //当前商品
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                //商品库存
                                var productStockItem = stockProducts
                                    .Where(b => b.ProductId == item.ProductId)
                                    .FirstOrDefault();

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

                            //合集
                            var temps = new List<ProductStockItem>();
                            if (stockProducts != null && stockProducts.Count > 0)
                            {
                                stockProducts.ForEach(psi =>
                                {
                                    var productStockItem = new ProductStockItem
                                    {
                                        ProductId = psi.ProductId,
                                        IsExpiredGood = false,
                                        UnitId = psi.UnitId,
                                        SmallUnitId = psi.SmallUnitId,
                                        BigUnitId = psi.BigUnitId,
                                        ProductName = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.Name,
                                        ProductCode = allProducts.Where(s => s.Id == psi.ProductId).FirstOrDefault()?.ProductCode,
                                        Quantity = psi.Quantity * (-1)
                                    };
                                    temps.Add(productStockItem);
                                });
                            }

                            //车库
                            var carId = dispatchItem?.DispatchBill?.CarId ?? 0;

                            #region //1. 清零车库预占量
                            try
                            {
                                //验证是否有预占（调度后的换货预占已经转移至车舱）
                                var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.ExchangeBill, bill.BillNumber, carId);
                                if (checkOrderQuantity)
                                {
                                    if (carId > 0)
                                    {
                                        //记录出库，清零预占量
                                        _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(bill,
                                            _productService,
                                            _specificationAttributeService,
                                            DirectionEnum.Out,
                                            StockQuantityType.OrderQuantity,
                                            carId,
                                            temps,
                                            StockFlowChangeTypeEnum.Audited);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                chageFailed = true;
                            }
                            #endregion


                            #region  //2. 换货商品入库(标注过期商品库存)

                            try
                            {
                                if (!chageFailed)
                                {
                                    temps.ForEach(p =>
                                    {
                                        //标注入库商品为过期商品
                                        p.IsExpiredGood = true;
                                    });

                                    _stockService.AdjustStockQty<ExchangeBill, ExchangeItem>(bill,
                                        _productService,
                                        _specificationAttributeService,
                                        DirectionEnum.In,
                                        StockQuantityType.CurrentQuantity,
                                        bill.WareHouseId,
                                        temps,
                                        StockFlowChangeTypeEnum.Audited);
                                }
                            }
                            catch (Exception)
                            {
                                chageFailed = true;
                            }
                            #endregion
                        }

                        #endregion

                        #region 添加送货签收记录
                        if (!chageFailed)
                        {
                            var deliverySign = new DeliverySign
                            {
                                StoreId = storeId,
                                BillTypeId = dispatchItem.BillTypeId,
                                //订单Id
                                BillId = bill.Id,
                                BillNumber = bill.BillNumber,
                                //转单后销售单Id
                                ToBillId = bill.Id,
                                ToBillNumber = bill.BillNumber,
                                TerminalId = bill.TerminalId,
                                BusinessUserId = bill.BusinessUserId,
                                DeliveryUserId = bill.DeliveryUserId,
                                Latitude = 0,
                                Longitude = 0,
                                SignUserId = userId,
                                SignedDate = DateTime.Now,
                                SumAmount = bill.SumAmount,
                                Signature = signature,
                                SignStatus = 1
                            };


                            //留存照片
                            if (photos != null && photos.Any())
                            {
                                photos.ForEach(p =>
                                {
                                    deliverySign.RetainPhotos.Add(p);
                                });
                            }

                            InsertDeliverySign(deliverySign);
                        }

                        #endregion
                    }

                    //保存事务
                    transaction.Commit();

                    return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful, Code = bill.Id };
                }
                else
                {
                    return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
                }
  
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
            finally
            {
                using (transaction) { }
            }
        }


        /// <summary>
        /// 更新调度
        /// </summary>
        /// <param name="dispatchItem"></param>
        private void UpdateDispatchItem(DispatchItem dispatchItem)
        {
            try
            {
                if (dispatchItem == null)
                {
                    throw new ArgumentNullException("dispatchItem");
                }

                var uow = DispatchItemsRepository.UnitOfWork;
                DispatchItemsRepository.Update(dispatchItem);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(dispatchItem);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 提交调度记录
        /// </summary>
        /// <param name="deliverySign"></param>
        private void InsertDeliverySign(DeliverySign deliverySign)
        {
            var uow = DeliverySignsRepository.UnitOfWork;
            DeliverySignsRepository.Insert(deliverySign);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(deliverySign);
        }



    }

}
