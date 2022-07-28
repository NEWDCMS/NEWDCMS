using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
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
    public class PurchaseBillService : BaseService, IPurchaseBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICommonBillService _commonBillService;
        


        public PurchaseBillService(IServiceGetter getter,
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
            _settingService = settingService;
            _queuedMessageService = queuedMessageService;
            _stockService = stockService;
            _recordingVoucherService = recordingVoucherService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;

            _commonBillService = commonBillService;
            
        }

        #region 采购单
        public bool Exists(int billId)
        {
            return PurchaseBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
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
        public IPagedList<PurchaseBill> GetPurchaseBillList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? start = null, DateTime? end = null, bool? auditedStatus = null, bool? reversedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int? paymented = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var query = from pc in PurchaseBillsRepository.Table
                          .Include(cr => cr.Items)
                          //.ThenInclude(cr => cr.PurchaseBill)
                          .Include(cr => cr.PurchaseBillAccountings)
                          .ThenInclude(cr => cr.AccountingOption)
                        select pc;

            if (store.HasValue && store != 0)
            {
                query = query.Where(a => a.StoreId == store);
            }

            if (makeuserId.HasValue && makeuserId > 0)
            {
                var userIds = _userService.GetSubordinate(store, makeuserId ?? 0).Where(s => s > 0).ToArray();
                if (userIds.Count() > 0)
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
            if (start.HasValue)
            {
                query = query.Where(a => a.CreatedOnUtc >= startDate);
            }

            //结束时间
            if (end.HasValue)
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
            //var plists = query.ToList();
            //return new PagedList<PurchaseBill>(plists, pageIndex, pageSize);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PurchaseBill>(plists, pageIndex, pageSize, totalCount);

        }

        public IList<PurchaseBill> GetPurchasesBillByStoreId(int? storeId)
        {
            var key = DCMSDefaults.PURCHASEBILL_BY_STOREID_KEY.FillCacheKey(storeId);
            return _cacheManager.Get(key, () =>
            {

                var query = PurchaseBillsRepository.Table;

                if (storeId.HasValue && storeId != 0)
                {
                    query = query.Where(a => a.StoreId == storeId);
                }

                query = query.OrderByDescending(a => a.CreatedOnUtc);

                return query.ToList();

            });

        }

        public IList<PurchaseBill> GetPurchaseBillByStoreIdManufacturerId(int storeId, int manufacturerId)
        {
            var query = PurchaseBillsRepository.Table;

            //已审核，未红冲
            query = query.Where(a => a.AuditedStatus == true && a.ReversedStatus == false);

            //经销商
            query = query.Where(a => a.StoreId == storeId);
            //供应商
            query = query.Where(a => a.ManufacturerId == manufacturerId);

            return query.ToList();

        }


        /// <summary>
        /// 根据单号获取采购单
        /// </summary>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        public PurchaseBill GetPurchaseBillByBillNumber(int? store, string billNumber = "")
        {
            var query = PurchaseBillsRepository.Table.Where(a => a.StoreId == store && a.BillNumber == billNumber);
            var key = DCMSDefaults.PURCHASEBILL_BY_BILLNUMBERID_KEY.FillCacheKey(store, billNumber);
            return _cacheManager.Get(key, () => query.FirstOrDefault());
        }
        
        /// <summary>
        /// 带缓存
        /// </summary>
        /// <param name="store"></param>
        /// <param name="purchaseBillId"></param>
        /// <returns></returns>
        public virtual PurchaseBill GetPurchaseBillById(int? store, int purchaseBillId)
        {
            if (purchaseBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.PURCHASEBILL_BY_ID_KEY.FillCacheKey(store, purchaseBillId);
            return _cacheManager.Get(key, () =>
            {
                return PurchaseBillsRepository.ToCachedGetById(purchaseBillId);
            });
        }

        /// <summary>
        /// 不带缓存
        /// </summary>
        /// <param name="store"></param>
        /// <param name="purchaseBillId"></param>
        /// <param name="isInclude"></param>
        /// <returns></returns>
        public virtual PurchaseBill GetPurchaseBillById(int? store, int purchaseBillId, bool isInclude = false)
        {
            if (purchaseBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = PurchaseBillsRepository.Table
                .Include(pb => pb.Items)
                //.ThenInclude(pb => pb.PurchaseBill)
                .Include(pb => pb.PurchaseBillAccountings)
                .ThenInclude(cr => cr.AccountingOption);

                return query.FirstOrDefault(p => p.Id == purchaseBillId);
            }
            return PurchaseBillsRepository.ToCachedGetById(purchaseBillId);
        }

        public virtual PurchaseBill GetPurchaseBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.PURCHASEBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = PurchaseBillsRepository.Table;
                var @return = query.Where(a => a.StoreId == store && a.BillNumber == billNumber).FirstOrDefault();
                return @return;
            });
        }

        public void DeletePurchaseBill(PurchaseBill purchaseBill)
        {

            if (purchaseBill == null)
            {
                throw new ArgumentNullException("purchasebill");
            }

            var uow = PurchaseBillsRepository.UnitOfWork;
            PurchaseBillsRepository.Delete(purchaseBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseBill);

        }

        public void InsertPurchaseBill(PurchaseBill purchaseBill)
        {
            var uow = PurchaseBillsRepository.UnitOfWork;
            PurchaseBillsRepository.Insert(purchaseBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(purchaseBill);
        }

        public void UpdatePurchaseBill(PurchaseBill purchaseBill)
        {
            if (purchaseBill == null)
            {
                throw new ArgumentNullException("purchasebill");
            }

            var uow = PurchaseBillsRepository.UnitOfWork;
            PurchaseBillsRepository.Update(purchaseBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(purchaseBill);

        }

        /// <summary>
        /// 更新采购单是否开具付款标志位
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdatePaymented(int? store, int billId, PayStatus payStatus)
        {
            var purchase = GetPurchaseBillById(store, billId, false);
            if (purchase != null)
            {
                purchase.PayStatus = (int)payStatus;
                var uow = PurchaseBillsRepository.UnitOfWork;
                PurchaseBillsRepository.Update(purchase);
                uow.SaveChanges();
                //通知
                _eventPublisher.EntityUpdated(purchase);
            }
        }

        /// <summary>
        /// 更新采购单欠款
        /// </summary>
        /// <param name="billNumber"></param>
        public void UpdatePurchaseBillOweCash(string billNumber, decimal? oweCash)
        {
            var purchase = GetPurchaseBillByBillNumber(0, billNumber);
            if (purchase != null)
            {
                purchase.OweCash = oweCash ?? 0;
                var uow = PurchaseBillsRepository.UnitOfWork;
                PurchaseBillsRepository.Update(purchase);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(purchase);
            }
        }

        /// <summary>
        /// 设置采购单价格
        /// </summary>
        /// <param name="purchaseId"></param>
        public void SetPurchaseBillAmount(int purchaseBillId)
        {
            PurchaseBill purchaseBill;
            var query = PurchaseBillsRepository.Table;
            purchaseBill = query.Where(a => a.Id == purchaseBillId).FirstOrDefault();
            if (purchaseBill == null)
            {
                throw new ArgumentNullException("purchasebill");
            }
            List<PurchaseItem> purchaseItems = GetPurchaseItemList(purchaseBillId);
            if (purchaseItems != null && purchaseItems.Count > 0)
            {
                //总金额
                decimal SumAmount = purchaseItems.Sum(a => a.Amount);
                //已收金额（会计科目金额）
                decimal accounting = 0;
                IList<PurchaseBillAccounting> purchaseAccountings = GetPurchaseBillAccountingsByPurchaseBillId(0, purchaseBillId);
                if (purchaseAccountings != null && purchaseAccountings.Count > 0)
                {
                    accounting = purchaseAccountings.Sum(a => a.CollectionAmount);
                }
                //总金额
                purchaseBill.SumAmount = SumAmount;
                //应收金额=总金额-优惠金额
                //purchase.ReceivableAmount = SumAmount - (purchase.PreferentialAmount ?? 0);
                //欠款金额=总金额-优惠金额-已收金额
                //purchase.OweCash = SumAmount - (purchase.PreferentialAmount ?? 0) - accounting;

                var uow = PurchaseBillsRepository.UnitOfWork;
                PurchaseBillsRepository.Update(purchaseBill);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(purchaseBill);
            }

        }

        #endregion

        #region 采购单明细


        /// <summary>
        /// 根据销采购获取项目
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<PurchaseItem> GetPurchaseItemByPurchaseId(int storeId, int userId, int purchaseBillId, int pageIndex, int pageSize)
        {
            if (purchaseBillId == 0)
            {
                return new PagedList<PurchaseItem>(new List<PurchaseItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.PURCHASEBILL_ITEM_ALLBY_PURCHASEID_KEY.FillCacheKey(storeId, purchaseBillId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseItemsRepository.Table
                            .Include(pi => pi.PurchaseBill)
                            where pc.PurchaseBillId == purchaseBillId
                            orderby pc.Id
                            select pc;
                //var purchaseItems = new PagedList<PurchaseItem>(query.ToList(), pageIndex, pageSize);
                //return purchaseItems;
                //总页数

                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PurchaseItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<PurchaseItem> GetPurchaseItemList(int purchaseBillId)
        {
            List<PurchaseItem> purchaseItems = null;
            var query = PurchaseItemsRepository.Table.Include(p=>p.PurchaseBill);
            purchaseItems = query.Where(a => a.PurchaseBillId == purchaseBillId).ToList();
            return purchaseItems;
        }

        public int PurchaseItemQtySum(int storeId, int productId, int purchaseBillId)
        {
            int qty = 0;
            var query = from purchaseBill in PurchaseBillsRepository.Table
                        join purchaseItem in PurchaseItemsRepository.Table on purchaseBill.Id equals purchaseItem.PurchaseBillId
                        where purchaseBill.AuditedStatus == true && purchaseItem.ProductId == productId
                              && purchaseBill.Id != purchaseBillId    //排除当前销采购数量
                        select purchaseItem;
            List<PurchaseItem> purchaseItems = query.ToList();
            if (purchaseItems != null && purchaseItems.Count > 0)
            {
                qty = purchaseItems.Sum(x => x.Quantity);
            }
            return qty;
        }

        public PurchaseItem GetPurchaseItemById(int purchaseItemId)
        {
            PurchaseItem purchaseItem;
            var query = PurchaseItemsRepository.Table;
            purchaseItem = query.Where(a => a.Id == purchaseItemId).FirstOrDefault();
            return purchaseItem;
        }

        public void DeletePurchaseItem(PurchaseItem purchaseItem)
        {
            if (purchaseItem == null)
            {
                throw new ArgumentNullException("purchaseItem");
            }

            var uow = PurchaseItemsRepository.UnitOfWork;
            PurchaseItemsRepository.Delete(purchaseItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseItem);
        }

        /// <summary>
        /// 根据单据删除明细
        /// </summary>
        /// <param name="bill"></param>
        public void DeletePurchaseItemByBillId(PurchaseBill bill)
        {
            var uow = PurchaseItemsRepository.UnitOfWork;
            var items = PurchaseItemsRepository.Table.Where(s => s.StoreId == bill.StoreId && s.PurchaseBillId == bill.Id);
            PurchaseItemsRepository.Delete(items);
            uow.SaveChanges();

            //通知
            items.ToList().ForEach(s => { _eventPublisher.EntityInserted(s); });
        }

        public void InsertPurchaseItem(PurchaseItem purchaseItem)
        {
            var uow = PurchaseItemsRepository.UnitOfWork;
            PurchaseItemsRepository.Insert(purchaseItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(purchaseItem);
        }

        public void UpdatePurchaseItem(PurchaseItem purchaseItem)
        {
            if (purchaseItem == null)
            {
                throw new ArgumentNullException("purchaseItem");
            }

            var uow = PurchaseItemsRepository.UnitOfWork;
            PurchaseItemsRepository.Update(purchaseItem);
            uow.SaveChanges();
            //通知
            _eventPublisher.EntityUpdated(purchaseItem);
        }


        #endregion

        #region 收款账户映射

        public virtual IPagedList<PurchaseBillAccounting> GetPurchaseBillAccountingsByPurchaseBillId(int storeId, int userId, int purchaseId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (purchaseId == 0)
            {
                return new PagedList<PurchaseBillAccounting>(new List<PurchaseBillAccounting>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.PURCHASEBILL_ACCOUNTING_ALLBY_PURCHASEID_KEY.FillCacheKey(storeId, purchaseId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseBillAccountingMappingRepository.Table
                            where pc.BillId == purchaseId
                            select pc;

                var query1 = from pc in query.ToList()
                             join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                             where pc.BillId == purchaseId
                             orderby pc.Id
                             select pc;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<PurchaseBillAccounting>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual IList<PurchaseBillAccounting> GetPurchaseBillAccountingsByPurchaseBillId(int? store, int purchaseId)
        {

            var key = DCMSDefaults.PURCHASEBILL_ACCOUNTINGL_BY_PURCHASEID_KEY.FillCacheKey(store ?? 0, purchaseId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseBillAccountingMappingRepository.Table
                            join p in AccountingOptionsRepository.Table on pc.AccountingOptionId equals p.Id
                            where pc.BillId == purchaseId
                            orderby pc.Id
                            select pc;


                return query.Distinct().ToList();
            });
        }



        /// <summary>
        /// 获取当前单据的所有搜款账户(目的:在查询时不依赖延迟加载,由于获的较高查询性能)
        /// </summary>
        /// <returns></returns>
        public virtual IList<PurchaseBillAccounting> GetAllPurchaseBillAccountingsByBillIds(int? store, int[] billIds)
        {
            if (billIds == null || billIds.Length == 0)
            {
                return new List<PurchaseBillAccounting>();
            }

            var key = DCMSDefaults.PURCHASEBILL_ACCOUNTINGL_BY_BILLIDS_KEY.FillCacheKey(store ?? 0, string.Join("_", billIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in PurchaseBillAccountingMappingRepository.Table
                            .Include(pb => pb.AccountingOption)
                            where billIds.Contains(pc.BillId)
                            select pc;
                return query.ToList();
            });
        }

        public virtual PurchaseBillAccounting GetPurchaseBillAccountingById(int purchaseBillAccountingId)
        {
            if (purchaseBillAccountingId == 0)
            {
                return null;
            }

            return PurchaseBillAccountingMappingRepository.ToCachedGetById(purchaseBillAccountingId);
        }

        public virtual void InsertPurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting)
        {
            if (purchaseBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseBillAccounting");
            }

            var uow = PurchaseBillAccountingMappingRepository.UnitOfWork;
            PurchaseBillAccountingMappingRepository.Insert(purchaseBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(purchaseBillAccounting);
        }

        public virtual void UpdatePurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting)
        {
            if (purchaseBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseBillAccounting");
            }

            var uow = PurchaseBillAccountingMappingRepository.UnitOfWork;
            PurchaseBillAccountingMappingRepository.Update(purchaseBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(purchaseBillAccounting);
        }

        public virtual void DeletePurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting)
        {
            if (purchaseBillAccounting == null)
            {
                throw new ArgumentNullException("purchaseBillAccounting");
            }

            var uow = PurchaseBillAccountingMappingRepository.UnitOfWork;
            PurchaseBillAccountingMappingRepository.Delete(purchaseBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(purchaseBillAccounting);
        }


        #endregion

        public virtual PurchaseItem GetPurchaseItemByProductId(int store, int productId)
        {
            PurchaseItem purchaseItem;
            var query = PurchaseItemsRepository.Table;
            purchaseItem = query.Where(a => a.ProductId == productId && a.StoreId == store).OrderByDescending(a => a.CreatedOnUtc).FirstOrDefault();
            return purchaseItem;
        }

        /// <summary>
        /// 获取商品大、中、小单位 最新以前的采购价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="beforeTax">税前</param>
        /// <returns></returns>
        public virtual Tuple<PurchaseItem, PurchaseItem, PurchaseItem> GetPurchaseItemByProduct(Product product, int storeId, int productId, bool beforeTax = false)
        {
            if (product != null)
            {
                PurchaseItem purchaseItem1 = PurchaseItemsRepository.Table.Where(pr => pr.StoreId == storeId && pr.ProductId == product.Id && pr.UnitId == product.SmallUnitId).OrderByDescending(pr => pr.CreatedOnUtc).FirstOrDefault();
                PurchaseItem purchaseItem2 = PurchaseItemsRepository.Table.Where(pr => pr.StoreId == storeId && pr.ProductId == product.Id && pr.UnitId == product.StrokeUnitId).OrderByDescending(pr => pr.CreatedOnUtc).FirstOrDefault();
                PurchaseItem purchaseItem3 = PurchaseItemsRepository.Table.Where(pr => pr.StoreId == storeId && pr.ProductId == product.Id && pr.UnitId == product.BigUnitId).OrderByDescending(pr => pr.CreatedOnUtc).FirstOrDefault();

                //税前
                if (beforeTax)
                {
                    if (purchaseItem1 != null)
                    {
                        purchaseItem1.Price /= (1 + purchaseItem1.TaxRate / 100); //不含税价格
                    }
                    if (purchaseItem2 != null)
                    {
                        purchaseItem2.Price /= (1 + purchaseItem2.TaxRate / 100);
                    }
                    if (purchaseItem3 != null)
                    {
                        purchaseItem3.Price /= (1 + purchaseItem3.TaxRate / 100);
                    }
                }
                return new Tuple<PurchaseItem, PurchaseItem, PurchaseItem>(purchaseItem1, purchaseItem2, purchaseItem3);
            }
            else
            {
                return null;
            }
        }

        public void UpdatePurchaseBillActive(int? store, int? billId, int? user)
        {
            var query = PurchaseBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.MakeUserId == user && x.AuditedStatus == true && (DateTime.Now.Subtract(x.AuditedDate ?? DateTime.Now).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = PurchaseBillsRepository.UnitOfWork;
                foreach (PurchaseBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    PurchaseBillsRepository.Update(bill);
                }
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// 获取平均采购价格
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="unitId">单位Id</param>
        /// <param name="averageNumber">平均次数</param>
        /// <returns></returns>
        public virtual decimal GetLastAveragePurchasePrice(int storeId, int productId, int unitId, int averageNumber)
        {
            decimal averagePrice = 0;
            var query = from a in PurchaseBillsRepository.Table
                        join b in PurchaseItemsRepository.Table on a.Id equals b.PurchaseBillId
                        where a.StoreId == storeId && a.AuditedStatus == true && a.ReversedStatus == false
                        && b.ProductId == productId
                        && b.UnitId == unitId
                        orderby b.Id descending
                        select b.Price;
            query = query.Take(averageNumber);
            //实际查询行数
            int count = query.Count();
            averagePrice = query.Sum(q => q) / (count == 0 ? 1 : count);
            return averagePrice;
        }

        /// <summary>
        /// 获取商品成本价（参考 GetReferenceCostPrice(int storeId, int productId, int unitId, decimal cprice)）
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> GetReferenceCostPrice(Product product)
        {
            try
            {
                var commpanySetting = _settingService.LoadSetting<CompanySetting>(product.StoreId);
                Dictionary<int, decimal> dic = new Dictionary<int, decimal>();

                List<int> unitIds = new List<int>();
                if (product.SmallUnitId != 0)
                {
                    unitIds.Add(product.SmallUnitId);
                    if (!dic.Keys.Contains(product.SmallUnitId))
                    {
                        dic.Add(product.SmallUnitId, 0);
                    }
                }
                if (product.StrokeUnitId != null && product.StrokeUnitId != 0)
                {
                    unitIds.Add(product.StrokeUnitId.Value);
                    if (!dic.Keys.Contains(product.StrokeUnitId.Value))
                    {
                        dic.Add(product.StrokeUnitId.Value, 0);
                    }
                }
                if (product.BigUnitId != null && product.BigUnitId != 0)
                {
                    unitIds.Add(product.BigUnitId.Value);
                    if (!dic.Keys.Contains(product.BigUnitId.Value))
                    {
                        dic.Add(product.BigUnitId.Value, 0);
                    }
                }

                //预设进价
                if (commpanySetting?.ReferenceCostPrice == 1)
                {
                    //var productPrices = ProductPricesRepository.Table.Where(s => s.StoreId == product.StoreId && s.ProductId == product.Id).ToList();
                    var productPrices = product.ProductPrices.ToList();
                    if (productPrices != null && productPrices.Count > 0)
                    {
                        productPrices.ForEach(pp =>
                        {
                            if (dic.Keys.Contains(pp.UnitId))
                            {
                                dic[pp.UnitId] = pp.CostPrice ?? 0;
                            }
                        });
                    }
                }
                //平均进价
                else if (commpanySetting?.ReferenceCostPrice == 2)
                {
                    dic.Keys.ToList().ForEach(k =>
                    {
                        dic[k] = GetLastAveragePurchasePrice(product.StoreId, product.Id, k, commpanySetting?.AveragePurchasePriceCalcNumber ?? 5);
                    });
                }
                return dic;
            }
            catch (Exception)
            {
                return new Dictionary<int, decimal>();
            }
        }

        /// <summary>
        /// 获取商品成本价（参考 GetReferenceCostPrice(int storeId, int productId, int unitId, decimal cprice)）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public decimal GetReferenceCostPrice(int storeId, int productId, int unitId)
        {
            try
            {
                var commpanySetting = _settingService.LoadSetting<CompanySetting>(storeId);
                decimal costPrice = 0;

                //预设进价
                if (commpanySetting?.ReferenceCostPrice==0 || commpanySetting?.ReferenceCostPrice == 1)
                {
                    var productPrice = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == unitId).FirstOrDefault();
                    costPrice = productPrice?.CostPrice ?? 0;
                }
                //平均进价
                else if (commpanySetting?.ReferenceCostPrice == 2)
                {
                    costPrice = GetLastAveragePurchasePrice(storeId, productId, unitId, commpanySetting?.AveragePurchasePriceCalcNumber ?? 5);
                }
                return costPrice;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取参考成本价，更新商品成本价
        /// </summary>
        /// <returns></returns>
        public decimal GetReferenceCostPrice(int storeId, int productId, int unitId, decimal cprice)
        {
            try
            {
                var commpanySetting = _settingService.LoadSetting<CompanySetting>(storeId);

                //预设进价
                var productPrice = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == unitId).FirstOrDefault();
                var costPrice = productPrice?.CostPrice ?? 0;
                if (costPrice == 0 || costPrice != cprice)
                {
                    costPrice = cprice;
                }

                //平均进价
                if (commpanySetting?.ReferenceCostPrice == 2)
                {
                    var averagePrice = GetLastAveragePurchasePrice(storeId, productId, unitId, commpanySetting?.AveragePurchasePriceCalcNumber ?? 5);
                    if (averagePrice > 0)
                    {
                        costPrice = averagePrice;
                    }
                }
                var product = _productService.GetProductById(storeId, productId);
                decimal bigCost = 0;
                decimal smallCost = 0;
                int bigQty = product.BigQuantity??1; //大单位换算数
                int strokeQty = product.StrokeQuantity ?? 1;//中单位换算数
                //判断价格单位
                if (unitId == product.BigUnitId)  //大单位
                {
                    smallCost = costPrice / bigQty;
                    var smallPrices = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == product.SmallUnitId).FirstOrDefault();
                    if (smallPrices != null) 
                    {
                        //更新小单位成本价
                        UpdateProductCostPrice(smallPrices, smallCost);
                    }
                }
                if (unitId == product.SmallUnitId) //小单位
                {
                    bigCost = costPrice * bigQty;
                    var bigPrices = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == product.BigUnitId).FirstOrDefault();
                    if (bigPrices != null)
                    {
                        //更新大单位成本价
                        UpdateProductCostPrice(bigPrices, bigCost);
                    }
                }
                if (unitId == product.StrokeUnitId) //中单位
                {
                    bigCost = costPrice * (bigQty / strokeQty);
                    var bigPrices = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == product.BigUnitId).FirstOrDefault();
                    if (bigPrices != null)
                    {
                        //更新大单位成本价
                        UpdateProductCostPrice(bigPrices, bigCost);
                    }
                    smallCost = costPrice / strokeQty;
                    var smallPrices = ProductPricesRepository.Table.Where(s => s.StoreId == storeId && s.ProductId == productId && s.UnitId == product.SmallUnitId).FirstOrDefault();
                    if (smallPrices != null)
                    {
                        //更新小单位成本价
                        UpdateProductCostPrice(smallPrices, smallCost);
                    }
                }
                //更新商品成本价
                if (productPrice != null)
                {
                    UpdateProductCostPrice(productPrice, costPrice);
                }
                return costPrice;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 更新商品成本价
        /// </summary>
        /// <param name="productPrice"></param>
        /// <param name="cPrice"></param>
        private void UpdateProductCostPrice(ProductPrice productPrice, decimal cPrice)
        {
            var uow = ProductPricesRepository.UnitOfWork;
            productPrice.CostPrice = cPrice;
            ProductPricesRepository.Update(productPrice);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(productPrice);
        }


        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PurchaseBill bill, List<PurchaseBillAccounting> accountingOptions, List<AccountingOption> accountings, PurchaseBillUpdate data, List<PurchaseItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true)
        {
            var uow = PurchaseBillsRepository.UnitOfWork;
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
                    #region 更新采购

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

                    UpdatePurchaseBill(bill);

                    #endregion

                }
                else
                {
                    #region 添加采购

                    bill.BillType = BillTypeEnum.PurchaseBill;

                    bill.BillNumber = string.IsNullOrEmpty(data.BillNumber) ? bill.GenerateNumber() : data.BillNumber;

                    var sb = GetPurchaseBillByNumber(storeId, bill.BillNumber);
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
                    var remark = data.Remark ?? "";
                    var number_arr = remark.Split("_", StringSplitOptions.RemoveEmptyEntries);
                    if (remark.StartsWith("OCMS订单出库_") && number_arr.Length >= 3) 
                    {
                        bill.HPNumber = number_arr[1];
                        bill.FPNumber = number_arr[2];
                        bill.IsPending = 0;
                    }
                    
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

                    InsertPurchaseBill(bill);
                    //主表Id
                    billId = bill.Id;

                    #endregion

                }

                #region 更新采购项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        _productService.SetSolded(p.ProductId);

                        var purchaseItem = GetPurchaseItemById(p.Id);
                        if (purchaseItem == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.PurchaseBillId = billId.Value;
                                item.StoreId = storeId;
                                //有税率，则价格=含税价格，金额=含税金额
                                if (item.TaxRate > 0 && companySetting.EnableTaxRate)
                                {
                                    item.Price *= (1 + item.TaxRate / 100);
                                    item.Amount *= (1 + item.TaxRate / 100);
                                }
                                //(参考成本价动态计算)成本价（不含税） 进价（含税）
                                item.CostPrice = GetReferenceCostPrice(storeId, p.ProductId, p.UnitId, item.Price);
                                item.CostAmount = item.CostPrice * item.Quantity;

                                item.CreatedOnUtc = DateTime.Now;


                                InsertPurchaseItem(item);

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
                            purchaseItem.ProductId = p.ProductId;
                            purchaseItem.UnitId = p.UnitId;
                            purchaseItem.Quantity = p.Quantity;
                            purchaseItem.RemainderQty = p.RemainderQty;
                            //purchaseItem.Price = p.Price;
                            //purchaseItem.Amount = p.Amount;
                            //有税率，则价格=含税价格，金额=含税金额
                            if (purchaseItem.TaxRate > 0 && companySetting.EnableTaxRate)
                            {
                                purchaseItem.Price = p.Price * (1 + p.TaxRate / 100);
                                purchaseItem.Amount = p.Amount * (1 + p.TaxRate / 100);
                            }

                            //(参考成本价动态计算)成本价（不含税） 进价（含税）
                            purchaseItem.CostPrice = GetReferenceCostPrice(storeId, p.ProductId, p.UnitId, purchaseItem.Price);
                            purchaseItem.CostAmount = purchaseItem.CostPrice * purchaseItem.Quantity;

                            purchaseItem.StockQty = p.StockQty;
                            purchaseItem.Remark = p.Remark;
                            purchaseItem.RemainderQty = p.RemainderQty;
                            purchaseItem.ManufactureDete = p.ManufactureDete;

                            UpdatePurchaseItem(purchaseItem);
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
                        var sd = GetPurchaseItemById(p.Id);
                        if (sd != null)
                        {
                            DeletePurchaseItem(sd);
                        }
                    }
                });

                #endregion

                #region 付款账户映射

                var purchaseAccountings = GetPurchaseBillAccountingsByPurchaseBillId(storeId, bill.Id);
                accountings.ToList().ForEach(c =>
                {
                    if (data != null && data.Accounting != null && data.Accounting.Select(a => a.AccountingOptionId).Contains(c.Id))
                    {
                        if (!purchaseAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == c.Id).FirstOrDefault();
                            var purchaseAccounting = new PurchaseBillAccounting()
                            {
                                StoreId = storeId,
                                //AccountingOption = c,
                                AccountingOptionId = c.Id,
                                CollectionAmount = collection != null ? collection.CollectionAmount : 0,
                                PurchaseBill = bill,
                                ManufacturerId = data.ManufacturerId,
                                BillId = bill.Id
                            };
                            //添加账户
                            InsertPurchaseBillAccounting(purchaseAccounting);
                        }
                        else
                        {
                            purchaseAccountings.ToList().ForEach(acc =>
                            {
                                var collection = data.Accounting.Select(a => a).Where(a => a.AccountingOptionId == acc.AccountingOptionId).FirstOrDefault();
                                acc.CollectionAmount = collection != null ? collection.CollectionAmount : 0;
                                acc.ManufacturerId = data.ManufacturerId;
                                //更新账户
                                UpdatePurchaseBillAccounting(acc);
                            });
                        }
                    }
                    else
                    {
                        if (purchaseAccountings.Select(cc => cc.AccountingOptionId).Contains(c.Id))
                        {
                            var purchaseaccountings = purchaseAccountings.Select(cc => cc).Where(cc => cc.AccountingOptionId == c.Id).ToList();
                            purchaseaccountings.ForEach(sa =>
                            {
                                DeletePurchaseBillAccounting(sa);
                            });
                        }
                    }
                });

                #endregion
                //管理员创建自动审核
                if (isAdmin && doAudit && string.IsNullOrEmpty(bill.HPNumber)) //判断当前登录者是否为管理员,若为管理员，开启自动审核 OC订单出库不自动审核
                {
                    AuditingNoTran(userId, bill);
                }
                else
                {
                    #region 发送通知 管理员
                    try
                    {
                        //制单人、管理员
                        var adminNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();
                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Message,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Message),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.PurchaseBill,
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
                //如果事务不存在或者为控则回滚 {"Unknown column 'a.AuditedUserId' in 'field list'"}
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据创建/更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Auditing(int storeId, int userId, PurchaseBill bill)
        {
            var uow = PurchaseBillsRepository.UnitOfWork;

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

        public BaseResult AuditingNoTran(int userId, PurchaseBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
                return _recordingVoucherService.CreateVoucher<PurchaseBill, PurchaseItem>(bill, bill.StoreId, userId, (voucherId) =>
                {
                    bool chageFailed = false;

                    #region 修改库存
                    try
                    {
                        if (bill.Items != null && bill.Items.Count > 0)
                        {

                            var stockProducts = new List<ProductStockItem>();
                            var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in bill.Items)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();

                                var productStockItem = stockProducts.Where(a => a.ProductId == item.ProductId).FirstOrDefault();

                                item.ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name;

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
                                        Quantity = thisQuantity,
                                        BillItem = item
                                    };

                                    stockProducts.Add(productStockItem);
                                }
                                //修改消单状态
                                product.HasSold = true;
                                _productService.UpdateProduct(product);
                            }
                            historyDatas1 = _stockService.AdjustStockQty<PurchaseBill, PurchaseItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Audited);
                        }
                    }
                    catch (Exception)
                    {
                        chageFailed = true;
                    }
                    #endregion

                    if (!chageFailed)
                    {
                        #region 公司设置->财务管理 修改成本价
                        //if (purchaseBill != null && purchaseBill.Items != null && purchaseBill.Items.Count > 0)
                        //{
                        //    purchaseBill.Items.ToList().ForEach(item =>
                        //    {
                        //        //获取配置平均次数
                        //        CompanySetting companySetting = _settingService.LoadSetting<CompanySetting>(storeId);
                        //        if (companySetting != null && companySetting.AveragePurchasePriceCalcNumber > 0)
                        //        {
                        //            int averageNumber = companySetting.AveragePurchasePriceCalcNumber;
                        //            //获取平均价格
                        //            decimal averageCostPrice = GetLastAveragePurchasePrice(item.ProductId, item.UnitId, averageNumber);
                        //            ProductPrice productPrice = _productService.GetProductPriceByProductIdAndUnitId(item.ProductId, item.UnitId);
                        //            if (productPrice != null)
                        //            {
                        //                productPrice.CostPrice = averageCostPrice;
                        //                _productService.UpdateProductPrice(productPrice);
                        //            }
                        //        }
                        //    });
                        //}
                        #endregion

                        #region 修改单据表状态

                        bill.VoucherId = voucherId;
                        bill.AuditedUserId = userId;
                        bill.AuditedDate = DateTime.Now;
                        bill.AuditedStatus = true;

                        //如果欠款小于等于0，则单据已付款
                        if (bill.OweCash <= 0)
                        {
                            bill.PayStatus = 2;
                        }
                        UpdatePurchaseBill(bill);

                        #endregion
                    }
                },
                () =>
                {
                    #region 发送通知
                    try
                    {
                        //制单人
                        var userNumbers = new List<string>() { _userService.GetMobileNumberByUserId(bill.BusinessUserId) };
                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = bill.StoreId,
                            MType = MTypeEnum.Audited,
                            Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.Audited),
                            Date = bill.CreatedOnUtc,
                            BillType = BillTypeEnum.PurchaseBill,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers.ToList(), queuedMessage);
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
                    _commonBillService.RollBackBill<PurchaseBill, PurchaseItem>(bill);  //回滚单据
                    //回滚
                    if (historyDatas1 != null)
                    {
                        _stockService.RoolBackChanged(historyDatas1); //回滚库存
                    }

                    return failed;
                });

            }
            catch (Exception)
            {
                return failed;
            }

        }

        public BaseResult Reverse(int userId, PurchaseBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = PurchaseBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                //记账凭证
                _recordingVoucherService.CancleVoucher<PurchaseBill, PurchaseItem>(bill, () =>
                {
                    #region 修改库存
                    try
                    {
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

                        //将数量设置成审核的负数
                        if (stockProducts != null && stockProducts.Count > 0)
                        {
                            stockProducts.ForEach(psi =>
                            {
                                psi.Quantity *= (-1);
                            });
                        }

                        historyDatas1 = _stockService.AdjustStockQty<PurchaseBill, PurchaseItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);
                    }
                    catch (Exception)
                    {
                    }

                    #endregion

                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    #endregion

                    bill.VoucherId = 0;
                    UpdatePurchaseBill(bill);
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
        public BaseResult Delete(int userId, PurchaseBill purchaseBill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = PurchaseBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                purchaseBill.Deleted = true;
                #endregion
                UpdatePurchaseBill(purchaseBill);

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
