using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.WareHouses
{
    /// <summary>
    /// 用于门店库存上报服务
    /// </summary>
    public partial class InventoryReportBillService : BaseService, IInventoryReportBillService
    {
        //private readonly IInventoryReportBillService _inventoryReportBillService;
        public InventoryReportBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher
) : base(getter, cacheManager, eventPublisher)
        {
            //_inventoryReportBillService = inventoryReportBillService;
        }


        #region 单据

        public IPagedList<InventoryReportBill> GetInventoryReportBillList(int? store, int? terminalId, int? businessUserId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;


            var query = InventoryReportBillsRepository.Table;

            if (store.HasValue && store != 0)
            {
                query = query.Where(a => a.StoreId == store);
            }

            //客户
            if (terminalId.HasValue && terminalId != 0)
            {
                query = query.Where(a => a.TerminalId == terminalId);
            }

            //业务员
            if (businessUserId.HasValue && businessUserId != 0)
            {
                query = query.Where(a => a.BusinessUserId == businessUserId);
            }

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            //var plist = new PagedList<InventoryReportBill>(query.ToList(), pageIndex, pageSize);
            //return plist;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<InventoryReportBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual InventoryReportBill GetInventoryReportBillById(int? store, int inventoryReportBillId)
        {
            if (inventoryReportBillId == 0)
            {
                return null;
            }
            return InventoryReportBillsRepository.ToCachedGetById(inventoryReportBillId);
        }


        public virtual InventoryReportBill GetInventoryReportBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.INVENTORYREPORTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = InventoryReportBillsRepository.Table;
                var allocationBill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return allocationBill;
            });
        }



        public virtual void InsertInventoryReportBill(InventoryReportBill inventoryReportBill)
        {
            if (inventoryReportBill == null)
            {
                throw new ArgumentNullException("inventoryReportBill");
            }

            var uow = InventoryReportBillsRepository.UnitOfWork;
            InventoryReportBillsRepository.Insert(inventoryReportBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryReportBill);
        }

        public virtual void UpdateInventoryReportBill(InventoryReportBill inventoryReportBill)
        {
            if (inventoryReportBill == null)
            {
                throw new ArgumentNullException("inventoryReportBill");
            }

            var uow = InventoryReportBillsRepository.UnitOfWork;
            InventoryReportBillsRepository.Update(inventoryReportBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryReportBill);
        }

        public virtual void DeleteInventoryReportBill(InventoryReportBill inventoryReportBill)
        {
            if (inventoryReportBill == null)
            {
                throw new ArgumentNullException("inventoryReportBill");
            }

            var uow = InventoryReportBillsRepository.UnitOfWork;
            InventoryReportBillsRepository.Delete(inventoryReportBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryReportBill);
        }


        #endregion

        #region 单据项目

        public virtual InventoryReportItem GetInventoryReportItemById(int? store, int inventoryReportItemId)
        {
            if (inventoryReportItemId == 0)
            {
                return null;
            }

            return InventoryReportItemsRepository.ToCachedGetById(inventoryReportItemId);
        }

        public virtual void InsertInventoryReportItem(InventoryReportItem inventoryReportItem)
        {
            if (inventoryReportItem == null)
            {
                throw new ArgumentNullException("inventoryReportItem");
            }

            var uow = InventoryReportItemsRepository.UnitOfWork;
            InventoryReportItemsRepository.Insert(inventoryReportItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryReportItem);
        }

        public virtual void UpdateInventoryReportItem(InventoryReportItem inventoryReportItem)
        {
            if (inventoryReportItem == null)
            {
                throw new ArgumentNullException("inventoryReportItem");
            }

            var uow = InventoryReportItemsRepository.UnitOfWork;
            InventoryReportItemsRepository.Update(inventoryReportItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryReportItem);
        }

        public virtual void DeleteInventoryReportItem(InventoryReportItem inventoryReportItem)
        {
            if (inventoryReportItem == null)
            {
                throw new ArgumentNullException("inventoryReportItem");
            }

            var uow = InventoryReportItemsRepository.UnitOfWork;
            InventoryReportItemsRepository.Delete(inventoryReportItem);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryReportItem);
        }


        //---------------------------------

        public virtual InventoryReportStoreQuantity GetInventoryReportStoreQuantityById(int? store, int inventoryReportStoreQuantityId)
        {
            if (inventoryReportStoreQuantityId == 0)
            {
                return null;
            }

            return InventoryReportStoreQuantitiesRepository.ToCachedGetById(inventoryReportStoreQuantityId);
        }

        public virtual void InsertInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity)
        {
            if (inventoryReportStoreQuantity == null)
            {
                throw new ArgumentNullException("inventoryReportStoreQuantity");
            }

            var uow = InventoryReportStoreQuantitiesRepository.UnitOfWork;
            InventoryReportStoreQuantitiesRepository.Insert(inventoryReportStoreQuantity);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryReportStoreQuantity);
        }

        public virtual void UpdateInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity)
        {
            if (inventoryReportStoreQuantity == null)
            {
                throw new ArgumentNullException("inventoryReportStoreQuantity");
            }

            var uow = InventoryReportStoreQuantitiesRepository.UnitOfWork;
            InventoryReportStoreQuantitiesRepository.Update(inventoryReportStoreQuantity);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryReportStoreQuantity);
        }

        public virtual void DeleteInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity)
        {
            if (inventoryReportStoreQuantity == null)
            {
                throw new ArgumentNullException("inventoryReportStoreQuantity");
            }

            var uow = InventoryReportStoreQuantitiesRepository.UnitOfWork;
            InventoryReportStoreQuantitiesRepository.Delete(inventoryReportStoreQuantity);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryReportStoreQuantity);
        }

        #endregion


        #region 上报汇总表
        public virtual InventoryReportSummary GetInventoryReportSummaryById(int? store, int inventoryReportSummaryId)
        {
            if (inventoryReportSummaryId == 0)
            {
                return null;
            }

            return InventoryReportSummariesRepository.ToCachedGetById(inventoryReportSummaryId);
        }

        public InventoryReportSummary GetInventoryReportSummaryByTerminalIdProductId(int store, int terminalId, int productId)
        {
            var query = InventoryReportSummariesRepository.Table;

            query = query.Where(a => a.StoreId == store);
            query = query.Where(a => a.TerminalId == terminalId);
            query = query.Where(a => a.ProductId == productId);
            return query.ToList().FirstOrDefault();
        }


        public virtual void InsertInventoryReportSummary(InventoryReportSummary inventoryReportSummary)
        {
            if (inventoryReportSummary == null)
            {
                throw new ArgumentNullException("inventoryReportSummary");
            }

            var uow = InventoryReportSummariesRepository.UnitOfWork;
            InventoryReportSummariesRepository.Insert(inventoryReportSummary);

            uow.SaveChanges();
            //通知
            _eventPublisher.EntityInserted(inventoryReportSummary);
        }

        public virtual void UpdateInventoryReportSummary(InventoryReportSummary inventoryReportSummary)
        {
            if (inventoryReportSummary == null)
            {
                throw new ArgumentNullException("inventoryReportSummary");
            }

            var uow = InventoryReportSummariesRepository.UnitOfWork;
            InventoryReportSummariesRepository.Update(inventoryReportSummary);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryReportSummary);
        }

        public virtual void DeleteInventoryReportSummary(InventoryReportSummary inventoryReportSummary)
        {
            if (inventoryReportSummary == null)
            {
                throw new ArgumentNullException("inventoryReportSummary");
            }

            var uow = InventoryReportSummariesRepository.UnitOfWork;
            InventoryReportSummariesRepository.Delete(inventoryReportSummary);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(inventoryReportSummary);
        }

        #endregion

        public void UpdateInventoryReportBillActive(int? store, int? billId, int? user)
        {
            var query = InventoryReportBillsRepository.Table.ToList();

            query = query.Where(x => x.StoreId == store && x.BusinessUserId == user && (DateTime.Now.Subtract(x.CreatedOnUtc).Duration().TotalDays > 30)).ToList();

            if (billId.HasValue && billId.Value > 0)
            {
                query = query.Where(x => x.Id == billId).ToList();
            }

            var result = query;

            if (result != null && result.Count > 0)
            {
                var uow = InventoryReportBillsRepository.UnitOfWork;
                foreach (InventoryReportBill bill in result)
                {
                    if ((bill.AuditedStatus && !bill.ReversedStatus) || bill.Deleted) continue;
                    bill.Deleted = true;
                    InventoryReportBillsRepository.Update(bill);
                }
                uow.SaveChanges();
            }


        }


        //public BaseResult BillCreateOrUpdate(int? store, int userId, int? billId, InventoryReportBill inventoryReportBill, InventoryReportBillUpdateModel data, bool isAdmin = false)
        //{
        //    var uow = AllocationBillsRepository.UnitOfWork;
        //    
        //    ITransaction transaction = null;
        //    try
        //    {
        //        
        //        transaction = uow.BeginOrUseTransaction();
        //        #region 修改门店库存上报单


        //        inventoryReportBill.StoreId = store ?? 0;
        //        inventoryReportBill.BillNumber =
        //        CommonHelper.GetBillNumber(
        //            CommonHelper.GetEnumDescription(
        //                BillTypeEnum.InventoryReportBill).Split(',')[1], store ?? 0);
        //        inventoryReportBill.TerminalId = data.TerminalId;
        //        inventoryReportBill.BusinessUserId = data.BusinessUserId;
        //        inventoryReportBill.ReversedUserId = null;
        //        inventoryReportBill.ReversedStatus = false;
        //        inventoryReportBill.ReversedDate = null;
        //        inventoryReportBill.CreatedOnUtc = DateTime.Now;
        //        inventoryReportBill.Remark = data.Remark;

        //        inventoryReportBill.Operation = 1;//标识操作源

        //        InsertInventoryReportBill(inventoryReportBill);

        //        //主表Id
        //        billId = inventoryReportBill.Id;

        //        #endregion

        //        #region 添加 上报关联商品、商品关联库存量

        //        if (data.Items != null && data.Items.Count > 0)
        //        {
        //            data.Items.ForEach(p =>
        //            {
        //                InventoryReportItem inventoryReportItem = new InventoryReportItem
        //                {
        //                    StoreId = store ?? 0,
        //                    InventoryReportBillId = billId ?? 0,
        //                    ProductId = p.ProductId,
        //                    BigUnitId = p.BigUnitId,
        //                    BigQuantity = p.BigQuantity,
        //                    SmallUnitId = p.SmallUnitId,
        //                    SmallQuantity = p.SmallQuantity
        //                };

        //                InsertInventoryReportItem(inventoryReportItem);

        //                if (p.InventoryReportStoreQuantities != null && p.InventoryReportStoreQuantities.Count > 0)
        //                {
        //                    p.InventoryReportStoreQuantities.ToList().ForEach(q =>
        //                    {
        //                        InventoryReportStoreQuantity inventoryReportStoreQuantity = new InventoryReportStoreQuantity
        //                        {
        //                            StoreId = q.StoreId,
        //                            InventoryReportItemId = inventoryReportItem.Id,
        //                            BigStoreQuantity = q.BigStoreQuantity,
        //                            SmallStoreQuantity = q.SmallStoreQuantity,
        //                            ManufactureDete = (q.ManufactureDete == null || q.ManufactureDete == DateTime.MinValue) ? DateTime.Now : q.ManufactureDete.Value
        //                        };

        //                        InsertInventoryReportStoreQuantity(inventoryReportStoreQuantity);
        //                    });
        //                }
        //            });
        //        }

        //        #endregion

        //        #region 添加修改上报汇总表
        //        if (data.Items != null && data.Items.Count > 0)
        //        {
        //            var allProducts = _productService.GetProductsByIds(data.Items.Select(pr => pr.ProductId).Distinct().ToArray());

        //            data.Items.ForEach(it =>
        //            {
        //                //根据 经销商、客户、商品 获取库存汇总记录
        //                InventoryReportSummary inventoryReportSummary = _inventoryReportBillService.GetInventoryReportSummaryByTerminalIdProductId(store ?? 0, inventoryReportBill.TerminalId, it.ProductId);

        //                //insert
        //                if (inventoryReportSummary == null)
        //                {
        //                    inventoryReportSummary = new InventoryReportSummary
        //                    {
        //                        StoreId = store ?? 0,
        //                        TerminalId = data.TerminalId,
        //                        BusinessUserId = data.BusinessUserId,
        //                        ProductId = it.ProductId,
        //                        BeginDate = DateTime.Now,
        //                        EndDate = null,
        //                        //期末库存 = 0
        //                        EndStoreQuantity = 0
        //                    };
        //                    var product = allProducts.Where(ap => ap.Id == it.ProductId).FirstOrDefault();

        //                    //采购量
        //                    inventoryReportSummary.PurchaseQuantity = 0;
        //                    if (product != null)
        //                    {
        //                        int sumPurchaseQuantity = 0;
        //                        //采购大单位数量
        //                        sumPurchaseQuantity += (product.BigQuantity ?? 0) * it.BigQuantity;
        //                        //采购小单位数量
        //                        sumPurchaseQuantity += it.SmallQuantity;
        //                        inventoryReportSummary.PurchaseQuantity = sumPurchaseQuantity;
        //                    }
        //                    //期初库存量 = 采购量
        //                    inventoryReportSummary.BeginStoreQuantity = inventoryReportSummary.PurchaseQuantity;
        //                    //销售量 = 0
        //                    inventoryReportSummary.SaleQuantity = 0;

        //                    InsertInventoryReportSummary(inventoryReportSummary);
        //                }
        //                //update
        //                else
        //                {
        //                    //期初库存不变
        //                    //期初时间不变
        //                    //以前采购量（上次）
        //                    int OldPurchaseQuantity = inventoryReportSummary.PurchaseQuantity;

        //                    //期末时间
        //                    inventoryReportSummary.EndDate = DateTime.Now;
        //                    //期末库存量
        //                    var product = allProducts.Where(ap => ap.Id == it.ProductId).FirstOrDefault();
        //                    inventoryReportSummary.EndStoreQuantity = 0;
        //                    if (product != null && it.InventoryReportStoreQuantities != null && it.InventoryReportStoreQuantities.Count > 0)
        //                    {
        //                        int sumEndStoreQuantity = 0;
        //                        it.InventoryReportStoreQuantities.ToList().ForEach(iq =>
        //                        {
        //                            //大单位库存量
        //                            sumEndStoreQuantity += (product.BigQuantity ?? 0) * iq.BigStoreQuantity;
        //                            //小单位库存量
        //                            sumEndStoreQuantity += iq.SmallStoreQuantity;
        //                        });
        //                        inventoryReportSummary.EndStoreQuantity = sumEndStoreQuantity;
        //                    }
        //                    //采购量
        //                    inventoryReportSummary.PurchaseQuantity = 0;
        //                    if (product != null)
        //                    {
        //                        int sumPurchaseQuantity = 0;
        //                        //采购大单位数量
        //                        sumPurchaseQuantity += (product.BigQuantity ?? 0) * it.BigQuantity;
        //                        //采购小单位数量
        //                        sumPurchaseQuantity += it.SmallQuantity;
        //                        inventoryReportSummary.PurchaseQuantity = sumPurchaseQuantity;
        //                    }
        //                    //采购量累加
        //                    inventoryReportSummary.PurchaseQuantity += OldPurchaseQuantity;

        //                    //销售量 = 采购量 - 期末库存
        //                    inventoryReportSummary.SaleQuantity = inventoryReportSummary.PurchaseQuantity - inventoryReportSummary.EndStoreQuantity;

        //                    UpdateInventoryReportSummary(inventoryReportSummary);
        //                }

        //            });
        //        }
        //        #endregion
        //        transaction.Commit();

        //        return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
        //    }
        //    catch (Exception)
        //    {
        //        //如果事务不存在或者为控则回滚
        //        transaction?.Rollback();
        //        return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
        //    }
        //    finally
        //    {
        //        //不管怎样最后都会关闭掉这个事务
        //        using (transaction) { }
        //    }
        //}
    }
}
