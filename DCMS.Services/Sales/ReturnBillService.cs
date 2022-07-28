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
using DCMS.Services.Finances;
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
    public class ReturnBillService : BaseService, IReturnBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ITerminalService _terminalService;
        


        public ReturnBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            ISettingService settingService,
            IRecordingVoucherService recordingVoucherService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ITerminalService terminalService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _settingService = settingService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _recordingVoucherService = recordingVoucherService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _terminalService = terminalService;
            
        }


        #region 退货单

        public bool Exists(int billId)
        {
            return ReturnBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        /// <summary>
        /// 查询当前经销商退货单
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
        public IPagedList<ReturnBill> GetReturnBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, int? paymentMethodType = null, int? billSourceType = null, bool? receipted = null, bool? deleted = null, bool? handleStatus = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in ReturnBillsRepository.Table
                         .Include(cr => cr.Items)
                         //.ThenInclude(cr => cr.ReturnBill)
                         .Include(cr => cr.ReturnBillAccountings)
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

            if (productId.HasValue && productId > 0)
            {
                query = query.Where(a => a.Items.Where(s => s.ProductId == productId).Count() > 0);
            }

            //客户
            if (terminalId.HasValue && terminalId > 0)
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
            if (businessUserId.HasValue && businessUserId > 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            //送货员
            if (deliveryUserId.HasValue && deliveryUserId > 0)
            {
                query = query.Where(a => a.DeliveryUserId == deliveryUserId);
            }

            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }

            //仓库
            if (wareHouseId.HasValue && wareHouseId > 0)
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
            if (districtId.HasValue && districtId > 0)
            {
                var terminals = _terminalService.GetDisTerminalIds(store,districtId ?? 0);
                query = query.Where(a => terminals.Contains(a.TerminalId));
            }

            //单据来源 billSourceType
            if (billSourceType != null)
            {
                if (billSourceType == (int)ReturnBillSourceType.Order)
                {
                    query = query.Where(a => a.ReturnReservationBillId > 0);
                }

                if (billSourceType == (int)ReturnBillSourceType.UnOrder)
                {
                    query = query.Where(a => a.ReturnReservationBillId == 0);
                }
            }

            //是否收款
            if (receipted.HasValue)
            {
                query = query.Where(a => a.ReceiptStatus == 2);
            }

            if (deleted != null)
            {
                query = query.Where(a => a.Deleted == deleted);
            }

            if (handleStatus.HasValue)
            {
                if (handleStatus.Value)
                {
                    query = query.Where(c => c.HandInStatus == handleStatus);
                }
                else
                {
                    query = query.Where(c => (c.HandInStatus == handleStatus || c.HandInStatus == null) && c.HandInDate == null);
                }
            }

            if (paymentMethodType != null)
            {
                if (paymentMethodType == (int)SaleBillPaymentMethodType.AlreadyBill)
                {
                    query = query.Where(a => a.ReceiptStatus == 2);
                }

                if (paymentMethodType == (int)SaleBillPaymentMethodType.OweBill)
                {
                    query = query.Where(a => a.ReceiptStatus == 0);
                }
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
            return new PagedList<ReturnBill>(plists, pageIndex, pageSize, totalCount);
        }


        public IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null)
        {

            return _cacheManager.Get(DCMSDefaults.RETURNBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ReturnBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               //审核
               if (auditedStatus != null)
               {
                   query = query.Where(a => a.AuditedStatus == auditedStatus);
               }

               //红冲
               if (reversedStatus != null)
               {
                   query = query.Where(a => a.ReversedStatus == reversedStatus);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int? businessUserId = null, DateTime? startTime = null, DateTime? endTime = null)
        {

            return _cacheManager.Get(DCMSDefaults.RETURNBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = ReturnBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               //审核
               if (auditedStatus != null)
               {
                   query = query.Where(a => a.AuditedStatus == auditedStatus);
               }

               //红冲
               if (reversedStatus != null)
               {
                   query = query.Where(a => a.ReversedStatus == reversedStatus);
               }

               //员工
               if (businessUserId != null && businessUserId != 0)
               {
                   query = query.Where(a => a.BusinessUserId == businessUserId);
               }

               //开始时间
               if (startTime != null)
               {
                   query = query.Where(a => a.TransactionDate >= startTime);
               }

               //结束时间
               if (endTime != null)
               {
                   query = query.Where(a => a.TransactionDate <= endTime);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, DateTime? beginDate = null)
        {
            // return _cacheManager.Get(DCMSDefaults.RETURNBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
            //{
            //});
            var query = ReturnBillsRepository.Table;

            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //审核
            if (auditedStatus != null)
            {
                query = query.Where(a => a.AuditedStatus == auditedStatus);
            }

            //红冲
            if (reversedStatus != null)
            {
                query = query.Where(a => a.ReversedStatus == reversedStatus);
            }

            //日期
            if (beginDate != null)
            {
                query = query.Where(a => a.TransactionDate >= beginDate);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }

        public IList<ReturnBill> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
        {
            var query = ReturnBillsRepository.Table;

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

        public IList<ReturnBill> GetCostProfitRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime)
        {
            var query = from rb in ReturnBillsRepository.Table.Include(r=>r.Items) select rb;

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
                var userIds = _userService.GetSubordinate(store, businessUserId ?? 0);
                query = query.Where(a => userIds.Contains(a.BusinessUserId));
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

        public IList<ReturnBill> GetReturnBillByStoreIdTerminalId(int storeId, int terminalId)
        {
            var query = ReturnBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.TerminalId == terminalId);

            return query.ToList();
        }

        /// <summary>
        /// 更新退货单是否开具收款标志位
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdateReturnBillReceipted(string billNumber)
        {
            var returnBill = GetReturnBillByNumber(0, billNumber);
            if (returnBill != null)
            {
                var uow = ReturnBillsRepository.UnitOfWork;
                returnBill.ReceiptStatus = 2;
                ReturnBillsRepository.Update(returnBill);
                uow.SaveChanges();

                _eventPublisher.EntityUpdated(returnBill);

            }
        }


        /// <summary>
        /// 更新退货单欠款
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdateReturnBillOweCash(string billNumber, decimal oweCash)
        {
            var returnBill = GetReturnBillByNumber(0, billNumber);
            if (returnBill != null)
            {
                returnBill.OweCash = oweCash;

                //如果欠款为0，则已收款
                if (oweCash == 0)
                {
                    returnBill.ReceiptStatus = 2;
                }

                var uow = ReturnBillsRepository.UnitOfWork;
                ReturnBillsRepository.Update(returnBill);
                uow.SaveChanges();

                _eventPublisher.EntityUpdated(returnBill);
            }
        }

        public virtual ReturnBill GetReturnBillById(int? store, int returnBillId)
        {
            if (returnBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.RETURNBILL_BY_ID_KEY.FillCacheKey(store ?? 0, returnBillId);
            return _cacheManager.Get(key, () =>
            {
                return ReturnBillsRepository.ToCachedGetById(returnBillId);
            });
        }

        public virtual ReturnBill GetReturnBillById(int? store, int returnBillId, bool isInclude = false)
        {
            if (returnBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = ReturnBillsRepository.Table
                .Include(rb => rb.Items)
                //.ThenInclude(rb => rb.ReturnBill)
                .Include(rb => rb.ReturnBillAccountings)
                .ThenInclude(cr => cr.AccountingOption);

                return query.FirstOrDefault(r => r.Id == returnBillId);
            }
            return ReturnBillsRepository.ToCachedGetById(returnBillId);
        }

        public virtual ReturnBill GetReturnBillByReturnReservationBillId(int? store, int returnReservationBillId)
        {
            if (returnReservationBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.RETURNBILL_BY_RESERVATIONID_KEY.FillCacheKey(store ?? 0, returnReservationBillId);
            return _cacheManager.Get(key, () => { return ReturnBillsRepository.Table.Where(a => a.ReturnReservationBillId == returnReservationBillId).FirstOrDefault(); });
        }

        public virtual IList<ReturnBill> GetReturnBillsByReturnReservationIds(int? store, int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<ReturnBill>();
            }

            var query = from c in ReturnBillsRepository.Table
                        where sIds.Contains(c.ReturnReservationBillId ?? 0) && c.StoreId == store
                        orderby c.Id
                        select c;
            var list = query.ToList();
            return list;
        }


        public virtual ReturnBill GetReturnBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.RETURNBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = ReturnBillsRepository.Table;
                var @return = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return @return;
            });
        }

        public int GetBillId(int? store, string billNumber)
        {
            var query = ReturnBillsRepository.TableNoTracking;
            var bill = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).Select(s => s.Id).FirstOrDefault();
            return bill;
        }


        public void DeleteReturnBill(ReturnBill returnBill)
        {

            if (returnBill == null)
            {
                throw new ArgumentNullException("returnbill");
            }

            var uow = ReturnBillsRepository.UnitOfWork;
            ReturnBillsRepository.Delete(returnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnBill);

        }

        public void InsertReturnBill(ReturnBill returnBill)
        {
            var uow = ReturnBillsRepository.UnitOfWork;
            ReturnBillsRepository.Insert(returnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(returnBill);
        }

        public void UpdateReturnBill(ReturnBill returnBill)
        {
            if (returnBill == null)
            {
                throw new ArgumentNullException("returnbill");
            }

            var uow = ReturnBillsRepository.UnitOfWork;
            ReturnBillsRepository.Update(returnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(returnBill);

        }

        /// <summary>
        /// 设置退货单价格
        /// </summary>
        /// <param name="returnBillId"></param>
        public void SetReturnBillAmount(int returnBillId)
        {
            ReturnBill returnBill;
            var query = ReturnBillsRepository.Table;
            returnBill = query.Where(a => a.Id == returnBillId).FirstOrDefault();
            if (returnBill == null)
            {
                throw new ArgumentNullException("returnbill");
            }
            List<ReturnItem> returnItems = GetReturnItemList(returnBillId);
            if (returnItems != null && returnItems.Count > 0)
            {
                //总金额
                decimal SumAmount = returnItems.Sum(a => a.Amount);
                //已收金额（会计科目金额）
                decimal accounting = 0;
                IList<ReturnBillAccounting> returnAccountings = GetReturnBillAccountingsByReturnBillId(0, returnBillId);
                if (returnAccountings != null && returnAccountings.Count > 0)
                {
                    accounting = returnAccountings.Sum(a => a.CollectionAmount);
                }
                //总金额
                returnBill.SumAmount = SumAmount;
                //应收金额=总金额-优惠金额
                //r.ReceivableAmount = SumAmount - (r.PreferentialAmount ?? 0);
                //欠款金额=总金额-优惠金额-已收金额
                //r.OweCash = SumAmount - (r.PreferentialAmount ?? 0) - accounting;

                //总成本价
                decimal SumCostPrice = returnItems.Sum(a => a.CostPrice);
                returnBill.SumCostPrice = SumCostPrice;
                //总成本金额
                decimal SumCostAmount = returnItems.Sum(a => a.CostAmount);
                returnBill.SumCostAmount = SumCostAmount;
                //总利润 = 总金额-总成本金额
                returnBill.SumProfit = returnBill.SumAmount - SumCostAmount;
                //成本利润率 = 总利润 / 总成本金额
                var amount = (returnBill.SumCostAmount == 0) ? returnBill.SumProfit : returnBill.SumCostAmount;
                if (amount != 0)
                {
                    returnBill.SumCostProfitRate = (returnBill.SumProfit / amount) * 100;
                }
                var uow = ReturnBillsRepository.UnitOfWork;
                ReturnBillsRepository.Update(returnBill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(returnBill);
            }

        }

        #endregion

        #region 退货单明细

        /// <summary>
        /// 根据退货单获取项目
        /// </summary>
        /// <param name="returnId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<ReturnItem> GetReturnItemByReturnBillId(int returnBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (returnBillId == 0)
            {
                return new PagedList<ReturnItem>(new List<ReturnItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.RETURNBILL_ITEM_ALLBY_RETURNID_KEY.FillCacheKey(storeId, returnBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnItemsRepository.Table
                            .Include(r => r.ReturnBill)
                            where pc.ReturnBillId == returnBillId
                            orderby pc.Id
                            select pc;
                //var returnItems = new PagedList<ReturnItem>(query.ToList(), pageIndex, pageSize);
                //return returnItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ReturnItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<ReturnItem> GetReturnItemList(int returnBillId)
        {
            List<ReturnItem> returnItems = null;
            var query = ReturnItemsRepository.Table.Include(r=>r.ReturnBill);
            returnItems = query.Where(a => a.ReturnBillId == returnBillId).ToList();
            return returnItems;
        }

        public int ReturnItemQtySum(int storeId, int productId, int returnBillId)
        {
            int qty = 0;
            var query = from r in ReturnBillsRepository.Table
                        join returnItem in ReturnItemsRepository.Table on r.Id equals returnItem.ReturnBillId
                        where r.AuditedStatus == true && returnItem.ProductId == productId
                              && r.Id != returnBillId    //排除当前退货单数量
                        select returnItem;
            List<ReturnItem> returnItems = query.ToList();
            if (returnItems != null && returnItems.Count > 0)
            {
                qty = returnItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public ReturnItem GetReturnItemById(int returnItemId)
        {
            ReturnItem returnItem;
            var query = ReturnItemsRepository.Table;
            returnItem = query.Where(a => a.Id == returnItemId).FirstOrDefault();
            return returnItem;
        }

        public void DeleteReturnItem(ReturnItem returnItem)
        {
            if (returnItem == null)
            {
                throw new ArgumentNullException("returnitem");
            }

            var uow = ReturnItemsRepository.UnitOfWork;
            ReturnItemsRepository.Delete(returnItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnItem);
        }

        public void InsertReturnItem(ReturnItem returnItem)
        {
            var uow = ReturnItemsRepository.UnitOfWork;
            ReturnItemsRepository.Insert(returnItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(returnItem);
        }

        public void UpdateReturnItem(ReturnItem returnItem)
        {
            if (returnItem == null)
            {
                throw new ArgumentNullException("returnitem");
            }

            var uow = ReturnItemsRepository.UnitOfWork;
            ReturnItemsRepository.Update(returnItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(returnItem);
        }

        #endregion

        #region 收款账户映射

        public virtual IPagedList<ReturnBillAccounting> GetReturnBillAccountingsByReturnBillId(int storeId, int userId, int returnBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (returnBillId == 0)
            {
                return new PagedList<ReturnBillAccounting>(new List<ReturnBillAccounting>(), pageIndex, pageSize);
            }

            //string key = string.Format(RETURNBILL_ACCOUNTING_ALLBY_RETURNID_KEY.FillCacheKey( returnBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.RETURNBILL_ACCOUNTING_ALLBY_RETURNID_KEY.FillCacheKey(storeId, returnBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnBillAccountingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == returnBillId
                            orderby pc.Id
                            select pc;



                //var returnAccountings = new PagedList<ReturnBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return returnAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ReturnBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<ReturnBillAccounting> GetReturnBillAccountingsByReturnBillId(int? store, int returnBillId)
        {

            var key = DCMSDefaults.RETURNBILL_ACCOUNTINGL_BY_RETURNID_KEY.FillCacheKey(store ?? 0, returnBillId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnBillAccountingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == returnBillId
                            orderby pc.Id
                            select pc;

                return query.ToList();
            });
        }

        public virtual ReturnBillAccounting GetReturnBillAccountingById(int returnBillAccountingId)
        {
            if (returnBillAccountingId == 0)
            {
                return null;
            }

            return ReturnBillAccountingRepository.ToCachedGetById(returnBillAccountingId);
        }

        public virtual void InsertReturnBillAccounting(ReturnBillAccounting returnBillAccounting)
        {
            if (returnBillAccounting == null)
            {
                throw new ArgumentNullException("returnbillaccounting");
            }

            var uow = ReturnBillAccountingRepository.UnitOfWork;
            ReturnBillAccountingRepository.Insert(returnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(returnBillAccounting);
        }

        public virtual void UpdateReturnBillAccounting(ReturnBillAccounting returnBillAccounting)
        {
            if (returnBillAccounting == null)
            {
                throw new ArgumentNullException("returnbillaccounting");
            }

            var uow = ReturnBillAccountingRepository.UnitOfWork;
            ReturnBillAccountingRepository.Update(returnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(returnBillAccounting);
        }

        public virtual void DeleteReturnBillAccounting(ReturnBillAccounting returnBillAccounting)
        {
            if (returnBillAccounting == null)
            {
                throw new ArgumentNullException("returnbillaccounting");
            }

            var uow = ReturnBillAccountingRepository.UnitOfWork;
            ReturnBillAccountingRepository.Delete(returnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(returnBillAccounting);
        }

        public IList<ReturnBillAccounting> GetAllReturnBillAccountingsByBillIds(int? store, int[] billIds)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<ReturnBillAccounting>();
            }

            var key = DCMSDefaults.RETURNBILL_ACCOUNTINGL_BY_RETURNID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ReturnBillAccountingRepository.Table
                            .Include(rb => rb.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                return query.ToList();
            });
        }
        #endregion

        /// <summary>
        /// 获取收款对账单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="status"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public IList<ReturnBill> GetReturnBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? payeer = null)
        {
            var query = ReturnBillsRepository.Table;

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

        /// <summary>
        /// 清理历史单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        public void UpdateReturnBillActive(int? store, int? billId, int? user)
        {
            var query = ReturnBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = ReturnBillsRepository.UnitOfWork;
                foreach (ReturnBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    ReturnBillsRepository.Update(bill);
                }

                uow.SaveChanges();
            }
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
        public IList<ReturnBill> GetReturnBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null)
        {
            var query = ReturnBillsRepository.Table;

            //经销商
            if (storeId.HasValue && storeId != 0)
            {
                query = query.Where(a => a.StoreId == storeId);
            }

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

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
            if (employeeId.HasValue)
            {
                query = query.Where(a => a.DeliveryUserId == employeeId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query.ToList();
        }


        public IList<BaseItem> GetReturnBillsByBusinessUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] businessUserIds = null, DateTime? startTime = null, DateTime? endTime = null)
        {

            return _cacheManager.Get(DCMSDefaults.GETRETURNBILLSBYBUSINESSUSERS.FillCacheKey(storeId, auditedStatus, reversedStatus, string.Join("-", businessUserIds), startTime, endTime), () =>
           {
               var query = from sb in ReturnBillsRepository.Table
                           join si in ReturnItemsRepository.Table on sb.Id equals si.ReturnBillId
                           join pr in ProductsRepository.Table on si.ProductId equals pr.Id
                           where sb.StoreId == storeId && sb.AuditedStatus == auditedStatus && sb.ReversedStatus == reversedStatus && businessUserIds.Contains(sb.BusinessUserId) && sb.CreatedOnUtc >= startTime && sb.CreatedOnUtc <= endTime
                           select new BaseItem
                           {
                               Id = si.Id,
                               StoreId = sb.StoreId,
                               BusinessUserId = sb.BusinessUserId,
                               DeliveryUserId = sb.DeliveryUserId,
                               ProductId = si.ProductId,
                               Quantity = si.Quantity,
                               Price = si.Price,
                               Amount = si.Amount,
                               ProductName = pr.Name,
                               CategoryId = pr.CategoryId,
                               PercentageType = 0,
                               Profit = si.Profit,
                               CostPrice = si.CostPrice,
                               CostAmount = si.CostAmount,
                               CostProfitRate = si.CostProfitRate
                           };
               return query.ToList();
           });
        }

        public IList<BaseItem> GetReturnBillsByDeliveryUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] deliveryUserIds = null, DateTime? startTime = null, DateTime? endTime = null)
        {

            return _cacheManager.Get(DCMSDefaults.GERETURNBILLSBYDELIVERYUSERS.FillCacheKey(storeId, auditedStatus, reversedStatus, string.Join("-", deliveryUserIds), startTime, endTime), () =>
            {
                var query = from sb in ReturnBillsRepository.Table
                            join si in ReturnItemsRepository.Table on sb.Id equals si.ReturnBillId
                            join pr in ProductsRepository.Table on si.ProductId equals pr.Id
                            where sb.StoreId == storeId && sb.AuditedStatus == auditedStatus && sb.ReversedStatus == reversedStatus && deliveryUserIds.Contains(sb.DeliveryUserId) && sb.CreatedOnUtc >= startTime && sb.CreatedOnUtc <= endTime
                            select new BaseItem
                            {
                                Id = si.Id,
                                StoreId = sb.StoreId,
                                BusinessUserId = sb.BusinessUserId,
                                DeliveryUserId = sb.DeliveryUserId,
                                ProductId = si.ProductId,
                                Quantity = si.Quantity,
                                Price = si.Price,
                                Amount = si.Amount,
                                ProductName = pr.Name,
                                CategoryId = pr.CategoryId,
                                PercentageType = 1,
                                Profit = si.Profit,
                                CostPrice = si.CostPrice,
                                CostAmount = si.CostAmount,
                                CostProfitRate = si.CostProfitRate
                            };
                return query.ToList();

            });
        }

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ReturnBill bill, List<ReturnBillAccounting> accountingOptions, List<AccountingOption> accountings, ReturnBillUpdate data, List<ReturnItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false,bool doAudit = true)
        {
            var uow = ReturnBillsRepository.UnitOfWork;

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

                #region 退货单据
                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新退货

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.DeliveryUserId = data.DeliveryUserId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitSale = data.IsMinUnitSale;
                    bill.Remark = data.Remark;
                    bill.DefaultAmountId = data.DefaultAmountId;

                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.OweCash = data.OweCash;

                    //判断是否有欠款
                    //return.Receipted = false;
                    if (data.OweCash < data.PreferentialEndAmount)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Part;
                    }
                    if (data.OweCash == data.PreferentialEndAmount)
                    {
                        bill.ReceivedStatus = ReceiptStatus.None;
                    }
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

                    if (data.OrderId > 0)
                    {
                        bill.ReturnReservationBillId = data.OrderId;
                    }

                    UpdateReturnBill(bill);
                    #endregion
                }
                else
                {
                    #region 添加退货

                    bill.StoreId = storeId;

                    bill.BillType = BillTypeEnum.ReturnBill;
                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

                    var sb = GetReturnBillByNumber(storeId, bill.BillNumber);
                    if (sb != null)
                    {
                        return new BaseResult { Success = false, Message = "操作失败，重复提交" };
                    }

                    //退货订单Id 默认0
                    bill.ReturnReservationBillId = 0;

                    bill.TerminalId = data.TerminalId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.DeliveryUserId = data.DeliveryUserId;
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
                    bill.Operation = data.Operation;//标识操作源

                    //判断是否有欠款
                    //sale.Receipted = false;
                    if (data.OweCash < data.PreferentialEndAmount)
                    {
                        bill.ReceivedStatus = ReceiptStatus.Part;
                    }
                    if (data.OweCash == data.PreferentialEndAmount)
                    {
                        bill.ReceivedStatus = ReceiptStatus.None;
                    }
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

                    if (data.OrderId > 0)
                    {
                        bill.ReturnReservationBillId = data.OrderId;
                    }

                    InsertReturnBill(bill);
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
                    //            BillType = BillTypeEnum.ReturnBill,
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
                #endregion

                #region 更新退货项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var returnItem = GetReturnItemById(p.Id);
                        if (returnItem == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.ReturnBillId = billId.Value;
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
                                InsertReturnItem(item);
                                //不排除
                                p.Id = item.Id;
                                //returnBill.Items.Add(item);
                                if (!bill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    bill.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            returnItem.ProductId = p.ProductId;
                            returnItem.UnitId = p.UnitId;
                            returnItem.Quantity = p.Quantity;
                            returnItem.RemainderQty = p.RemainderQty;
                            returnItem.Price = p.Price;
                            returnItem.Amount = p.Amount;
                            //有税率，则价格=含税价格，金额=含税金额
                            if (returnItem.TaxRate > 0 && companySetting.EnableTaxRate)
                            {
                                returnItem.Price = p.Price * (1 + p.TaxRate / 100);
                                returnItem.Amount = p.Amount * (1 + p.TaxRate / 100);
                            }

                            //成本价
                            returnItem.CostPrice = p.CostPrice;
                            //成本金额
                            returnItem.CostAmount = p.CostAmount;
                            //利润 = 金额 - 成本金额
                            returnItem.Profit = returnItem.Amount - returnItem.CostAmount;
                            //成本利润率 = 利润 / 成本金额
                            if (returnItem.CostAmount == 0)
                            {
                                returnItem.CostProfitRate = 100;
                            }
                            else
                            {
                                returnItem.CostProfitRate = returnItem.Profit / returnItem.CostAmount * 100;
                            }

                            returnItem.StockQty = p.StockQty;
                            returnItem.Remark = p.Remark;
                            returnItem.RemainderQty = p.RemainderQty;
                            returnItem.ManufactureDete = p.ManufactureDete;

                            //2019-07-25
                            returnItem.IsGifts = p.IsGifts;
                            returnItem.BigGiftQuantity = p.BigGiftQuantity;
                            returnItem.SmallGiftQuantity = p.SmallGiftQuantity;

                            UpdateReturnItem(returnItem);
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
                        var sd = GetReturnItemById(p.Id);
                        if (sd != null)
                        {
                            DeleteReturnItem(sd);
                        }
                    }
                });

                #endregion

                #region 收款账户映射
                //bill中已经有ReturnBillAccounting故注释下句
                //var returnAccountings = GetReturnBillAccountingsByReturnBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data != null && data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        //if (!returnAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        if (!bill.ReturnBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var returnAccounting = new ReturnBillAccounting()
                            {
                                StoreId = storeId,
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                ReturnBill = bill,
                                BillId = bill.Id,
                                TerminalId = data.TerminalId
                            };
                            //添加账户
                            InsertReturnBillAccounting(returnAccounting);
                        }
                        else
                        {
                            //returnAccountings.ForEach(acc =>
                            bill.ReturnBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.TerminalId = data.TerminalId;
                                //更新账户
                                UpdateReturnBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        //if (returnAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        if (bill.ReturnBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            // var returnaccountings = returnAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            var returnaccountings = bill.ReturnBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            returnaccountings.ForEach(sa =>
                            {
                                DeleteReturnBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion


                #region APP签收转单逻辑

                if (bill.ReturnReservationBillId > 0 && bill.Operation == (int)OperationEnum.APP && data.dispatchItem != null)
                {
                    #region 更新调度项目的签收状态

                    var dispatchItem = data.dispatchItem;
                    dispatchItem.SignStatus = 1;
                    dispatchItem.SignUserId = userId;
                    dispatchItem.SignDate = DateTime.Now;
                    dispatchItem.TerminalId = data.TerminalId;
                    //更新状态
                    UpdateDispatchItem(dispatchItem);

                    #endregion

                    if (dispatchItem.SignStatus == 1)
                    {
                        #region 释放订单预占库存

                        var order = GetReturnReservationBillById(storeId, dispatchItem.BillId);
                        //销售订单 
                        if (order != null && order.Items != null && order.Items.Count > 0)
                        {
                            //将一个单据中 相同商品 数量 按最小单位汇总
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(storeId, order.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in order.Items)
                            {
                                //当前商品
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                //商品库存
                                var productStockItem = stockProducts.Where(b => b.ProductId == item.ProductId).FirstOrDefault();
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
                                    temps.Add(productStockItem);
                                });
                            }

                            //验证是否有预占
                            var checkOrderQuantity = _stockService.CheckOrderQuantity(storeId, BillTypeEnum.ReturnReservationBill, order.BillNumber, order.WareHouseId);
                            if (checkOrderQuantity)
                            {
                                //车库
                                var carId = data?.dispatchBill?.CarId ?? 0;
                                if (carId > 0)
                                {
                                    //记录入库，增加预占量
                                    _stockService.AdjustStockQty<ReturnReservationBill, ReturnReservationItem>(order, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.OrderQuantity, carId, temps, StockFlowChangeTypeEnum.Audited);
                                }
                            }
                        }

                        #endregion

                        #region 添加送货签收记录

                        var deliverySign = new DeliverySign
                        {
                            StoreId = storeId,
                            BillTypeId = dispatchItem.BillTypeId,
                            //订单Id
                            BillId = order.Id,
                            BillNumber = order.BillNumber,
                            //转单后销售单Id
                            ToBillId = bill.Id,
                            ToBillNumber = bill.BillNumber,
                            TerminalId = data.TerminalId,
                            BusinessUserId = data.BusinessUserId,
                            DeliveryUserId = data.DeliveryUserId,
                            Latitude = data.Latitude,
                            Longitude = data.Longitude,
                            SignUserId = userId,
                            SignedDate = DateTime.Now,
                            SumAmount = order.SumAmount
                        };
                        InsertDeliverySign(deliverySign);

                        #endregion
                    }
                }

                #endregion

                //判断App开单是否自动审核
                bool appBillAutoAudits = false;
                if (data.Operation == (int)OperationEnum.APP)
                {
                    appBillAutoAudits = _settingService.AppBillAutoAudits(storeId, BillTypeEnum.ReturnBill);
                }
                //读取配置自动审核、管理员创建自动审核,是保存还是保存并编辑
                if ((isAdmin && doAudit) || appBillAutoAudits) //判断当前登录者是否为管理员,若为管理员，开启自动审核
                {
                    AuditingNoTran(userId, bill);
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
                            BillType = BillTypeEnum.ReturnBill,
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

                return new BaseResult { Success = true, Return = billId ?? 0, Message = "单据创建/更新成功", Code = bill.Id };
            }
            catch (Exception ex)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = ex.Message };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Auditing(int storeId, int userId, ReturnBill bill)
        {
            var uow = ReturnBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;
                //bill.MakeUserId = userId;

                AuditingNoTran(userId, bill);


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

        private BaseResult AuditingNoTran(int userId, ReturnBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                return _recordingVoucherService.CreateVoucher<ReturnBill, ReturnItem>(bill, bill.StoreId, userId, (voucherId) =>
                {
                    #region 修改库存
                    if (bill != null && bill.Items != null && bill.Items.Count > 0)
                    {
                        var stockProducts = new List<ProductStockItem>();

                        var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (ReturnItem item in bill.Items)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            ProductStockItem productStockItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
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

                        //退货增加现货
                        historyDatas1 = _stockService.AdjustStockQty<ReturnBill, ReturnItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);
                    }
                    #endregion

                    #region 修改单据表状态

                    bill.VoucherId = voucherId;
                    bill.AuditedUserId = userId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedStatus = true;

                    //如果欠款小于等于0，则单据已收款
                    if (bill.OweCash <= 0)
                    {
                        bill.ReceiptStatus = 2;
                    }
                    UpdateReturnBill(bill);
                    #endregion
                },
                () =>
                {
                    #region 发送通知
                    try
                    {
                        //制单人、管理员
                        var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };

                        if (bill.ReturnReservationBillId == 0)
                        {
                            var queuedMessage = new QueuedMessage()
                            {
                                StoreId = bill.StoreId,
                                MType = MTypeEnum.Audited,
                                Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                                Date = bill.CreatedOnUtc,
                                BillType = BillTypeEnum.ReturnBill,
                                BillNumber = bill.BillNumber,
                                BillId = bill.Id,
                                CreatedOnUtc = DateTime.Now
                            };
                            _queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage);
                        }
                        else
                        {
                            var queuedMessage2 = new QueuedMessage() //转单完成消息体
                            {
                                StoreId = bill.StoreId,
                                MType = MTypeEnum.TransferCompleted,
                                Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.TransferCompleted),
                                Date = bill.CreatedOnUtc,
                                BillType = BillTypeEnum.ReturnBill,
                                BillNumber = bill.BillNumber,
                                BillId = bill.Id,
                                CreatedOnUtc = DateTime.Now
                            };
                            _queuedMessageService.InsertQueuedMessage(userNumbers, queuedMessage2);
                        }
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                    #endregion
                    return successful;
                },
                () =>
                {
                    //回滚库存
                    if (historyDatas1 != null)
                    {
                        _stockService.RoolBackChanged(historyDatas1);
                    }

                    return failed;
                });
            }
            catch (Exception)
            {
                return failed;
            }

        }

        public BaseResult Reverse(int userId, ReturnBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = ReturnBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                //记账凭证
                #region 红冲记账凭证
                _recordingVoucherService.CancleVoucher<ReturnBill, ReturnItem>(bill, () =>
                {
                    #region 修改库存
                    try
                    {
                        //现货 数量设置负数
                        var stockProducts = new List<ProductStockItem>();
                        var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

                        foreach (ReturnItem item in bill.Items)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            ProductStockItem productStockItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
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

                        //将数量设置成审核的负数
                        if (stockProducts != null && stockProducts.Count > 0)
                        {
                            stockProducts.ForEach(psi =>
                            {
                                psi.Quantity *= (-1);
                            });
                        }

                        //减少现货
                        historyDatas1 = _stockService.AdjustStockQty<ReturnBill, ReturnItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);
                    }
                    catch (Exception)
                    {
                    }

                    #endregion

                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdateReturnBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdateReturnBill(bill);
                });

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

        public BaseResult Delete(int userId, ReturnBill returnBill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = SaleBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                returnBill.Deleted = true;
                #endregion
                UpdateReturnBill(returnBill);

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
        /// 更新单据收款状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="receiptStatus"></param>
        public void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus)
        {
            var bill = GetReturnBillById(store, billId, false);
            if (bill != null)
            {
                bill.ReceiptStatus = (int)receiptStatus;
                var uow = ReturnBillsRepository.UnitOfWork;
                ReturnBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
            }
        }


        /// <summary>
        /// 更新单据交账状态
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="handInStatus"></param>
        public void UpdateHandInStatus(int? store, int billId, bool handInStatus)
        {
            var bill = GetReturnBillById(store, billId, false);
            if (bill != null)
            {
                bill.HandInStatus = handInStatus;
                var uow = ReturnBillsRepository.UnitOfWork;
                ReturnBillsRepository.Update(bill);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(bill);
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

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="returnReservationBillId"></param>
        /// <param name="isInclude"></param>
        /// <returns></returns>
        private ReturnReservationBill GetReturnReservationBillById(int? store, int returnReservationBillId, bool isInclude = false)
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
    }
}
