using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Finances;
using DCMS.Services.Products;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Purchases
{
    public class PurchaseReturnBillService : BaseService, IPurchaseReturnBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICommonBillService _commonBillService;
        


        public PurchaseReturnBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            IStockService stockService,
            ISettingService settingService,
            IRecordingVoucherService recordingVoucherService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            ICommonBillService commonBillService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _settingService = settingService;
            _recordingVoucherService = recordingVoucherService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _commonBillService = commonBillService;
            
        }

        #region 采购退货单
        public bool Exists(int billId)
        {
            return PurchaseReturnBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        /// <summary>
        /// 获取当前经销商采购单
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="businessUserId">员工</param>
        /// <param name="manufacturerId">供应商</param>
        /// <param name="wareHouseId">仓库</param>
        /// <param name="billNumber">单据号</param>
        /// <param name="printStatus">打印状态</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="auditedStatus">审核状态</param>
        /// <param name="remark">备注</param>
        /// <param name="sortByAuditedTime">按审核时间</param>
        /// <param name="showReverse">显示红冲的数据</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<PurchaseReturnBill> GetPurchaseReturnBillList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? start = null, DateTime? end = null, bool? auditedStatus = null, bool? reversedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int? paymented = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in PurchaseReturnBillsRepository.Table
                          .Include(cr => cr.Items)
                          //.ThenInclude(cr => cr.PurchaseReturnBill)
                          .Include(cr => cr.PurchaseReturnBillAccountings)
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

            //员工
            if (businessUserId.HasValue && businessUserId != 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            //供应商
            if (manufacturerId.HasValue && manufacturerId != 0)
            {
                query = query.Where(a => a.ManufacturerId == manufacturerId);
            }

            //仓库
            if (wareHouseId.HasValue && wareHouseId != 0)
            {
                query = query.Where(a => a.WareHouseId == wareHouseId);
            }

            //单据号
            if (!string.IsNullOrEmpty(billNumber))
            {
                query = query.Where(a => a.BillNumber.Contains(billNumber));
            }

            //打印状态
            if (printStatus.HasValue)
            {
                if (printStatus.Value)
                {
                    query = query.Where(a => a.PrintNum > 0);
                }
                else
                {
                    query = query.Where(a => a.PrintNum == 0);
                }
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

            //审核状态
            if (auditedStatus.HasValue)
            {
                query = query.Where(a => a.AuditedStatus == auditedStatus);
            }
            //红冲状态
            if (reversedStatus.HasValue)
            {
                query = query.Where(a => a.ReversedStatus == reversedStatus);
            }

            //备注
            if (!string.IsNullOrEmpty(remark))
            {
                query = query.Where(a => a.Remark.Contains(remark));
            }

            //红冲状态
            if (showReverse.HasValue)
            {
                query = query.Where(a => a.ReversedStatus == showReverse);
            }

            if (paymented.HasValue)
            {
                query = query.Where(a => a.PayStatus == paymented);
            }

            //if (deleted.HasValue)
            //{
            //    query = query.Where(a => a.Deleted == deleted);
            //}

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

            //var plist = new PagedList<PurchaseReturnBill>(query.ToList(), pageIndex, pageSize);
            //return plist;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PurchaseReturnBill>(plists, pageIndex, pageSize, totalCount);
        }


        public IList<PurchaseReturnBill> GetPurchaseReturnBillsByStoreId(int? storeId)
        {

            return _cacheManager.Get(DCMSDefaults.PURCHASERETURNBILL_BY_STOREID_KEY.FillCacheKey(storeId), () =>
           {

               var query = PurchaseReturnBillsRepository.Table;

               if (storeId.HasValue && storeId != 0)
               {
                   query = query.Where(a => a.StoreId == storeId);
               }

               query = query.OrderByDescending(a => a.CreatedOnUtc);

               return query.ToList();

           });

        }

        public IList<PurchaseReturnBill> GetPurchaseReturnBillByStoreIdManufacturerId(int storeId, int manufacturerId)
        {
            var query = PurchaseReturnBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.ManufacturerId == manufacturerId);

            return query.ToList();

        }

        public virtual PurchaseReturnBill GetPurchaseReturnBillById(int? store, int purchaseReturnBillId)
        {
            if (purchaseReturnBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.PURCHASERETURNBILL_BY_ID_KEY.FillCacheKey(store ?? 0, purchaseReturnBillId);
            return _cacheManager.Get(key, () =>
            {
                return PurchaseReturnBillsRepository.ToCachedGetById(purchaseReturnBillId);
            });
        }

        public virtual PurchaseReturnBill GetPurchaseReturnBillById(int? store, int purchaseReturnBillId, bool isInclude = false)
        {
            if (purchaseReturnBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = PurchaseReturnBillsRepository.Table
                .Include(pb => pb.Items)
                //.ThenInclude(pb => pb.PurchaseReturnBill)
                .Include(pb => pb.PurchaseReturnBillAccountings)
                .ThenInclude(cr => cr.AccountingOption);

                return query.FirstOrDefault(p => p.Id == purchaseReturnBillId);
            }
            return PurchaseReturnBillsRepository.ToCachedGetById(purchaseReturnBillId);
        }

        public virtual PurchaseReturnBill GetPurchaseReturnBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.PURCHASERETURNBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = PurchaseReturnBillsRepository.Table;
                var @return = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return @return;
            });
        }

        public void DeletePurchaseReturnBill(PurchaseReturnBill purchaseReturnBill)
        {

            if (purchaseReturnBill == null)
            {
                throw new ArgumentNullException("purchaseReturnBill");
            }

            var uow = PurchaseReturnBillsRepository.UnitOfWork;
            PurchaseReturnBillsRepository.Delete(purchaseReturnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseReturnBill);

        }

        public void InsertPurchaseReturnBill(PurchaseReturnBill purchaseReturnBill)
        {
            var uow = PurchaseReturnBillsRepository.UnitOfWork;
            PurchaseReturnBillsRepository.Insert(purchaseReturnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(purchaseReturnBill);
        }

        public void UpdatePurchaseReturnBill(PurchaseReturnBill purchaseReturnBill)
        {
            if (purchaseReturnBill == null)
            {
                throw new ArgumentNullException("purchaseReturnBill");
            }

            var uow = PurchaseReturnBillsRepository.UnitOfWork;
            PurchaseReturnBillsRepository.Update(purchaseReturnBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(purchaseReturnBill);

        }


        /// <summary>
        /// 更新采购退货单是否开具付款标志位
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdatePaymented(int? store, int billId, PayStatus payStatus)
        {
            var purchaseReturn = GetPurchaseReturnBillById(store, billId, false);
            if (purchaseReturn != null)
            {
                var uow = PurchaseReturnBillsRepository.UnitOfWork;
                purchaseReturn.PayStatus = (int)payStatus;
                PurchaseReturnBillsRepository.Update(purchaseReturn);
                uow.SaveChanges();
            }
        }


        /// <summary>
        /// 更新采购退货单欠款
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdatePurchaseReturnBillOweCash(string billNumber, decimal? oweCash)
        {
            var purchaseReturn = GetPurchaseReturnBillByNumber(0, billNumber);
            if (purchaseReturn != null)
            {
                var uow = PurchaseReturnBillsRepository.UnitOfWork;
                purchaseReturn.OweCash = oweCash ?? 0;
                PurchaseReturnBillsRepository.Update(purchaseReturn);
                uow.SaveChanges();
            }
        }


        /// <summary>
        /// 设置采购退货单价格
        /// </summary>
        /// <param name="purchaseReturnId"></param>
        public void SetPurchaseReturnBillAmount(int purchaseReturnBillId)
        {
            PurchaseReturnBill purchaseReturnBill;
            var query = PurchaseReturnBillsRepository.Table;
            purchaseReturnBill = query.Where(a => a.Id == purchaseReturnBillId).FirstOrDefault();
            if (purchaseReturnBill == null)
            {
                throw new ArgumentNullException("purchaseReturnBillId");
            }
            List<PurchaseReturnItem> purchaseReturnItems = GetPurchaseReturnItemList(purchaseReturnBillId);
            if (purchaseReturnItems != null && purchaseReturnItems.Count > 0)
            {
                //总金额
                decimal SumAmount = purchaseReturnItems.Sum(a => a.Amount);
                //已收金额（会计科目金额）
                decimal accounting = 0;
                IList<PurchaseReturnBillAccounting> purchaseReturnBillAccountings = GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(0, purchaseReturnBillId);
                if (purchaseReturnBillAccountings != null && purchaseReturnBillAccountings.Count > 0)
                {
                    accounting = purchaseReturnBillAccountings.Sum(a => a.CollectionAmount);
                }
                //总金额
                purchaseReturnBill.SumAmount = SumAmount;
                //应收金额=总金额-优惠金额
                //purchaseReturn.ReceivableAmount = SumAmount - (purchaseReturn.PreferentialAmount ?? 0);
                //欠款金额=总金额-优惠金额-已收金额
                //purchaseReturn.OweCash = SumAmount - (purchaseReturn.PreferentialAmount ?? 0) - accounting;

                var uow = PurchaseReturnBillsRepository.UnitOfWork;
                PurchaseReturnBillsRepository.Update(purchaseReturnBill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(purchaseReturnBill);
            }

        }



        #endregion

        #region 采购退货单明细


        /// <summary>
        /// 根据采购退货单获取项目
        /// </summary>
        /// <param name="purchaseReturnId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<PurchaseReturnItem> GetPurchaseReturnItemByPurchaseReturnBillId(int purchaseReturnBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (purchaseReturnBillId == 0)
            {
                return new PagedList<PurchaseReturnItem>(new List<PurchaseReturnItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.PURCHASERETURNBILL_ITEM_ALLBY_SALEID_KEY.FillCacheKey(storeId, purchaseReturnBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseReturnItemsRepository.Table
                            .Include(pr => pr.PurchaseReturnBill)
                            where pc.PurchaseReturnBillId == purchaseReturnBillId
                            orderby pc.Id
                            select pc;
                //var purchaseReturnItems = new PagedList<PurchaseReturnItem>(query.ToList(), pageIndex, pageSize);
                //return purchaseReturnItems;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PurchaseReturnItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<PurchaseReturnItem> GetPurchaseReturnItemList(int purchaseReturnBillId)
        {
            List<PurchaseReturnItem> purchaseReturnItems = null;
            var query = PurchaseReturnItemsRepository.Table.Include(p=>p.PurchaseReturnBill);
            purchaseReturnItems = query.Where(a => a.PurchaseReturnBillId == purchaseReturnBillId).ToList();
            return purchaseReturnItems;
        }

        public int PurchaseReturnItemQtySum(int storeId, int productId, int purchaseReturnBillId)
        {
            int qty = 0;
            var query = from purchaseReturn in PurchaseReturnBillsRepository.Table
                        join purchaseReturnItem in PurchaseReturnItemsRepository.Table on purchaseReturn.Id equals purchaseReturnItem.PurchaseReturnBillId
                        where purchaseReturn.AuditedStatus == true && purchaseReturnItem.ProductId == productId
                              && purchaseReturn.Id != purchaseReturnBillId    //排除当前采购退货单数量
                        select purchaseReturnItem;
            List<PurchaseReturnItem> purchaseReturnItems = query.ToList();
            if (purchaseReturnItems != null && purchaseReturnItems.Count > 0)
            {
                qty = purchaseReturnItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public PurchaseReturnItem GetPurchaseReturnItemById(int purchaseReturnItemId)
        {
            PurchaseReturnItem purchaseReturnItem;
            var query = PurchaseReturnItemsRepository.Table;
            purchaseReturnItem = query.Where(a => a.Id == purchaseReturnItemId).FirstOrDefault();
            return purchaseReturnItem;
        }

        public void DeletePurchaseReturnItem(PurchaseReturnItem purchaseReturnItem)
        {
            if (purchaseReturnItem == null)
            {
                throw new ArgumentNullException("purchaseReturnItem");
            }

            var uow = PurchaseReturnItemsRepository.UnitOfWork;
            PurchaseReturnItemsRepository.Delete(purchaseReturnItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseReturnItem);
        }

        public void InsertPurchaseReturnItem(PurchaseReturnItem purchaseReturnItem)
        {
            var uow = PurchaseReturnItemsRepository.UnitOfWork;
            PurchaseReturnItemsRepository.Insert(purchaseReturnItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(purchaseReturnItem);
        }

        public void UpdatePurchaseReturnBillItem(PurchaseReturnItem purchaseReturnItem)
        {
            if (purchaseReturnItem == null)
            {
                throw new ArgumentNullException("purchaseReturnItem");
            }

            var uow = PurchaseReturnItemsRepository.UnitOfWork;
            PurchaseReturnItemsRepository.Update(purchaseReturnItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(purchaseReturnItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IPagedList<PurchaseReturnBillAccounting> GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(int storeId, int userId, int purchaseReturnBillId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (purchaseReturnBillId == 0)
            {
                return new PagedList<PurchaseReturnBillAccounting>(new List<PurchaseReturnBillAccounting>(), pageIndex, pageSize);
            }

            //var key = DCMSDefaults.PURCHASERETURNBILL_ACCOUNTING_ALLBY_SALEID_KEY.FillCacheKey( purchaseReturnBillId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.PURCHASERETURNBILL_ACCOUNTING_ALLBY_SALEID_KEY.FillCacheKey(storeId, purchaseReturnBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseReturnBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == purchaseReturnBillId
                            orderby pc.Id
                            select pc;

                //var purchaseReturnBillAccountings = new PagedList<PurchaseReturnBillAccounting>(query.ToList(), pageIndex, pageSize);
                //return purchaseReturnBillAccountings;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PurchaseReturnBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<PurchaseReturnBillAccounting> GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(int? store, int purchaseReturnBillId)
        {

            var key = DCMSDefaults.PURCHASERETURNBILL_ACCOUNTINGL_BY_SALEID_KEY.FillCacheKey(store ?? 0, purchaseReturnBillId);
            return _cacheManager.Get(key, () =>
            {
                try
                {
                    var query = from pc in PurchaseReturnBillAccountingMappingRepository.Table
                                join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                                where pc.BillId == purchaseReturnBillId
                                orderby pc.Id
                                select pc;

                    return query.Distinct().ToList();
                }
                catch (Exception)
                {
                    return new List<PurchaseReturnBillAccounting>();
                }
            });
        }

        public virtual PurchaseReturnBillAccounting GetPurchaseReturnBillAccountingById(int purchaseReturnBillAccountingId)
        {
            if (purchaseReturnBillAccountingId == 0)
            {
                return null;
            }

            return PurchaseReturnBillAccountingMappingRepository.ToCachedGetById(purchaseReturnBillAccountingId);
        }

        public virtual void InsertPurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting)
        {
            if (purchaseReturnBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseReturnBillAccounting");
            }

            var uow = PurchaseReturnBillAccountingMappingRepository.UnitOfWork;
            PurchaseReturnBillAccountingMappingRepository.Insert(purchaseReturnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(purchaseReturnBillAccounting);
        }

        public virtual void UpdatePurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting)
        {
            if (purchaseReturnBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseReturnBillAccounting");
            }

            var uow = PurchaseReturnBillAccountingMappingRepository.UnitOfWork;
            PurchaseReturnBillAccountingMappingRepository.Update(purchaseReturnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(purchaseReturnBillAccounting);
        }

        public virtual void DeletePurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting)
        {
            if (purchaseReturnBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseReturnBillAccounting");
            }

            var uow = PurchaseReturnBillAccountingMappingRepository.UnitOfWork;
            PurchaseReturnBillAccountingMappingRepository.Delete(purchaseReturnBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseReturnBillAccounting);
        }

        /// <summary>
        /// 获取当前单据的所有搜款账户(目的:在查询时不依赖延迟加载,由于获的较高查询性能)
        /// </summary>
        /// <returns></returns>
        public virtual IList<PurchaseReturnBillAccounting> GetAllPurchaseReturnBillAccountingsByBillIds(int? store, int[] billIds)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<PurchaseReturnBillAccounting>();
            }

            var key = DCMSDefaults.PURCHASERETURNBILL_ACCOUNTING_BY_ID_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseReturnBillAccountingMappingRepository.Table
                            .Include(pr => pr.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                return query.ToList();
            });
        }
        #endregion


        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PurchaseReturnBill bill, List<PurchaseReturnBillAccounting> accountingOptions, List<AccountingOption> accountings, PurchaseReturnBillUpdate data, List<PurchaseReturnItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false,bool doAudit = true)
        {
            var uow = PurchaseReturnBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;
                if (!(bill.Id > 0))
                { 
                    bill.MakeUserId = userId;
                }

                var companySetting = _settingService.LoadSetting<CompanySetting>(storeId);
                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新采购退货
                    bill.ManufacturerId = data.ManufacturerId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitPurchase = data.IsMinUnitPurchase;
                    bill.Remark = data.Remark;

                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.OweCash = data.OweCash;
                    if (data.OweCash < data.PreferentialEndAmount)
                    {
                        bill.PaymentStatus = PayStatus.Part;
                    }
                    if (data.OweCash == data.PreferentialEndAmount)
                    {
                        bill.PaymentStatus = PayStatus.None;
                    }
                    //计算金额
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        //总金额
                        //decimal SumAmount = data.Items.Sum(a => a.Amount * (1 + a.TaxRate / 100));
                        bill.SumAmount = data.Items.Sum(a => a.Amount);

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

                    UpdatePurchaseReturnBill(bill);

                    #endregion

                }
                else
                {
                    #region 添加采购

                    bill.BillType = BillTypeEnum.PurchaseReturnBill;

                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;
                    var sb = GetPurchaseReturnBillByNumber(storeId, bill.BillNumber);
                    if (sb != null)
                    {
                        return new BaseResult { Success = false, Message = "操作失败，重复提交" };
                    }

                    bill.ManufacturerId = data.ManufacturerId;
                    bill.BusinessUserId = data.BusinessUserId;
                    bill.WareHouseId = data.WareHouseId;
                    bill.TransactionDate = data.TransactionDate;
                    bill.IsMinUnitPurchase = data.IsMinUnitPurchase;
                    bill.Remark = data.Remark;

                    bill.SumAmount = 0;
                    bill.ReceivableAmount = data.PreferentialEndAmount;
                    bill.PreferentialAmount = data.PreferentialAmount;
                    bill.OweCash = data.OweCash;

                    bill.MakeUserId = userId;
                    bill.CreatedOnUtc = DateTime.Now;
                    bill.AuditedStatus = false;
                    bill.ReversedStatus = false;
                    bill.Operation = data.Operation;//标识操作源
                    if (data.OweCash < data.PreferentialEndAmount)
                    {
                        bill.PaymentStatus = PayStatus.Part;
                    }
                    if (data.OweCash == data.PreferentialEndAmount)
                    {
                        bill.PaymentStatus = PayStatus.None;
                    }
                    //计算金额
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        //总金额
                        bill.SumAmount = data.Items.Sum(a => a.Amount);

                        if (companySetting.EnableTaxRate)
                        {
                            bill.TaxAmount = Math.Round(data.Items.Sum(a => a.Amount * (a.TaxRate / 100)), 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            bill.TaxAmount = 0;
                        }
                    }

                    InsertPurchaseReturnBill(bill);
                    //主表Id
                    billId = bill.Id;

                    #endregion

                }

                #region 更新采购退货项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var purchaseReturnItem = GetPurchaseReturnItemById(p.Id);
                        if (purchaseReturnItem == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.PurchaseReturnBillId = billId.Value;
                                item.StoreId = storeId;
                                //有税率，则价格=含税价格，金额=含税金额
                                if (item.TaxRate > 0 && companySetting.EnableTaxRate)
                                {
                                    item.Price *= (1 + item.TaxRate / 100);
                                    item.Amount *= (1 + item.TaxRate / 100);
                                }

                                item.CreatedOnUtc = DateTime.Now;
                                InsertPurchaseReturnItem(item);
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
                            purchaseReturnItem.ProductId = p.ProductId;
                            purchaseReturnItem.UnitId = p.UnitId;
                            purchaseReturnItem.Quantity = p.Quantity;
                            purchaseReturnItem.RemainderQty = p.RemainderQty;
                            purchaseReturnItem.Price = p.Price;
                            purchaseReturnItem.Amount = p.Amount;
                            //有税率，则价格=含税价格，金额=含税金额
                            if (purchaseReturnItem.TaxRate > 0 && companySetting.EnableTaxRate)
                            {
                                purchaseReturnItem.Price = p.Price * (1 + p.TaxRate / 100);
                                purchaseReturnItem.Amount = p.Amount * (1 + p.TaxRate / 100);
                            }
                            purchaseReturnItem.StockQty = p.StockQty;
                            purchaseReturnItem.Remark = p.Remark;
                            purchaseReturnItem.RemainderQty = p.RemainderQty;
                            purchaseReturnItem.ManufactureDete = p.ManufactureDete;

                            purchaseReturnItem.CostPrice = p.CostPrice;
                            purchaseReturnItem.CostAmount = p.CostAmount;

                            UpdatePurchaseReturnBillItem(purchaseReturnItem);
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
                        var sd = GetPurchaseReturnItemById(p.Id);
                        if (sd != null)
                        {
                            DeletePurchaseReturnItem(sd);
                        }
                    }
                });

                #endregion

                #region 付款账户映射

                var purchaseReturnBillAccountings = GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data != null && data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!purchaseReturnBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var purchaseReturnBillAccounting = new PurchaseReturnBillAccounting()
                            {
                                StoreId = storeId,
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                PurchaseReturnBill = bill,
                                ManufacturerId = data.ManufacturerId,
                                BillId = bill.Id
                            };
                            //添加账户
                            InsertPurchaseReturnBillAccounting(purchaseReturnBillAccounting);
                        }
                        else
                        {
                            purchaseReturnBillAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.ManufacturerId = data.ManufacturerId;
                                //更新账户
                                UpdatePurchaseReturnBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (purchaseReturnBillAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var purchaseReturnaccountings = purchaseReturnBillAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            purchaseReturnaccountings.ForEach(sa =>
                            {
                                DeletePurchaseReturnBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion

                //管理员创建自动审核
                if (isAdmin && doAudit) //判断当前登录者是否为管理员,若为管理员，开启自动审核
                {
                    AuditingNoTran(storeId, userId, bill);
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
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.PurchaseReturnBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
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

                return new BaseResult { Success = true, Return = billId ?? 0, Message = "单据创建/更新成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据创建/更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Auditing(int storeId, int userId, PurchaseReturnBill bill)
        {
            var uow = PurchaseReturnBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                bill.StoreId = storeId;
                //bill.MakeUserId = userId;

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

        public BaseResult AuditingNoTran(int storeId, int userId, PurchaseReturnBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                return _recordingVoucherService.CreateVoucher<PurchaseReturnBill, PurchaseReturnItem>(bill, storeId, userId, (voucherId) =>
                {
                    bool chageFailed = false;
                    #region 修改库存
                    try
                    {
                        if (bill.Items != null && bill.Items.Count > 0)
                        {
                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in bill.Items)
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

                            List<ProductStockItem> productStockItemThiss2 = new List<ProductStockItem>();
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
                                    productStockItemThiss2.Add(productStockItem);
                                });
                            }

                            //减少现货
                            historyDatas1 = _stockService.AdjustStockQty<PurchaseReturnBill, PurchaseReturnItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItemThiss2, StockFlowChangeTypeEnum.Audited);

                        }
                    }
                    catch (Exception)
                    {
                        chageFailed = true;
                    }
                    #endregion

                    #region 修改单据表状态

                    if (!chageFailed)
                    {
                        bill.VoucherId = voucherId;
                        bill.AuditedUserId = userId;
                        bill.AuditedDate = DateTime.Now;
                        bill.AuditedStatus = true;

                        //如果欠款小于等于0，则单据已付款
                        if (bill.OweCash <= 0)
                        {
                            bill.PayStatus = 2;
                        }
                        UpdatePurchaseReturnBill(bill);
                    }
                    #endregion

                },
                () =>
                {
                    #region 发送通知 制单人
                    try
                    {
                        //制单人
                        var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };
                        var queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.PurchaseReturnBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(),queuedMessage);
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
                    _commonBillService.RollBackBill<PurchaseReturnBill, PurchaseReturnItem>(bill);  //回滚单据
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

        public BaseResult Reverse(int userId, PurchaseReturnBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = PurchaseReturnBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                _recordingVoucherService.CancleVoucher<PurchaseReturnBill, PurchaseReturnItem>(bill, () =>
                {
                    #region 修改库存
                    var stockProducts = new List<ProductStockItem>();
                    var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (var item in bill.Items)
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

                    historyDatas1 = _stockService.AdjustStockQty<PurchaseReturnBill, PurchaseReturnItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);
                    #endregion

                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdatePurchaseReturnBill(bill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdatePurchaseReturnBill(bill);
                });

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Brandid"></param>
        /// <returns></returns>
        public virtual IList<int> GetProductPurchaseIds(int StoreId, int manufacterId = 0)
        {
            if (StoreId == 0)
            {
                return new List<int>();
            }
            var query = from a in PurchaseBillsRepository.Table
                        join b in PurchaseItemsRepository.Table on a.Id equals b.PurchaseBillId
                        where a.ManufacturerId == manufacterId && a.StoreId == StoreId && b.StoreId == StoreId
                        select b.ProductId;
            var productids = query.ToList();
            return productids;
        }
    }
}