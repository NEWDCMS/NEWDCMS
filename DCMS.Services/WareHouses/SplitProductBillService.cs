using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.WareHouses
{
    /// <summary>
    /// 用于表示库存商品拆分单服务
    /// </summary>
    public partial class SplitProductBillService : BaseService, ISplitProductBillService
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserService _userService;

        public SplitProductBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IStockService stockService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _stockService = stockService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _userService = userService;
        }

        #region 单据
        public bool Exists(int billId)
        {
            return SplitProductBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<SplitProductBill> GetAllSplitProductBills(int? store, int? makeuserId, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = SplitProductBillsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }
            else
            {
                return null;
            }

            if (makeuserId.HasValue && makeuserId > 0)
            {
                var userIds = _userService.GetSubordinate(store, makeuserId ?? 0)?.Where(s => s > 0).ToList();
                if (userIds.Count > 0)
                    query = query.Where(x => userIds.Contains(x.MakeUserId));
            }


            if (wareHouseId.HasValue && wareHouseId.Value > 0)
            {
                query = query.Where(c => c.WareHouseId == wareHouseId);
            }

            if (!string.IsNullOrWhiteSpace(billNumber))
            {
                query = query.Where(c => c.BillNumber.Contains(billNumber));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.AuditedStatus == status);
            }

            if (start.HasValue)
            {
                query = query.Where(o => start.Value <= o.CreatedOnUtc);
            }

            if (end.HasValue)
            {
                query = query.Where(o => end.Value >= o.CreatedOnUtc);
            }

            if (isShowReverse.HasValue)
            {
                query = query.Where(c => c.ReversedStatus == isShowReverse);
            }

            if (!string.IsNullOrWhiteSpace(remark))
            {
                query = query.Where(c => c.Remark.Contains(remark));
            }

            //按审核时间排序
            if (sortByAuditedTime.HasValue && sortByAuditedTime.Value == true)
            {
                query = query.OrderByDescending(c => c.AuditedDate);
            }
            //默认创建时间排序
            else
            {
                query = query.OrderByDescending(c => c.CreatedOnUtc);
            }


            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<SplitProductBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<SplitProductBill> GetAllSplitProductBills()
        {
            var query = from c in SplitProductBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual SplitProductBill GetSplitProductBillById(int? store, int splitProductBillId)
        {
            if (splitProductBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.SPLITPRODUCTBILL_BY_ID_KEY.FillCacheKey(store ?? 0, splitProductBillId);
            return _cacheManager.Get(key, () =>
            {
                return SplitProductBillsRepository.ToCachedGetById(splitProductBillId);
            });
        }

        public virtual SplitProductBill GetSplitProductBillById(int? store, int splitProductBillId, bool isInclude = false)
        {
            if (splitProductBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = SplitProductBillsRepository_RO.Table.Include(sp => sp.Items);
                return query.FirstOrDefault(s => s.Id == splitProductBillId);
            }
            return SplitProductBillsRepository.ToCachedGetById(splitProductBillId);
        }


        public virtual SplitProductBill GetSplitProductBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.SPLITPRODUCTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = SplitProductBillsRepository.Table;
                var bill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return bill;
            });
        }



        public virtual void InsertSplitProductBill(SplitProductBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = SplitProductBillsRepository.UnitOfWork;
            SplitProductBillsRepository.Insert(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public virtual void UpdateSplitProductBill(SplitProductBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = SplitProductBillsRepository.UnitOfWork;
            SplitProductBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);
        }

        public virtual void DeleteSplitProductBill(SplitProductBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = SplitProductBillsRepository.UnitOfWork;
            SplitProductBillsRepository.Delete(bill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(bill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<SplitProductItem> GetSplitProductItemsBySplitProductBillId(int splitProductBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (splitProductBillId == 0)
            {
                return new PagedList<SplitProductItem>(new List<SplitProductItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.SPLITPRODUCTBILLITEM_ALL_KEY.FillCacheKey(storeId, splitProductBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in SplitProductItemsRepository.Table
                            where pc.SplitProductBillId == splitProductBillId
                            orderby pc.Id
                            select pc;
                //var productSplitProductBills = new PagedList<SplitProductItem>(query.ToList(), pageIndex, pageSize);
                //return productSplitProductBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<SplitProductItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<SplitProductItem> GetSplitProductItemList(int splitProductBillId)
        {
            List<SplitProductItem> splitProductItems = null;
            var query = SplitProductItemsRepository.Table;
            splitProductItems = query.Where(a => a.SplitProductBillId == splitProductBillId).ToList();
            return splitProductItems;
        }

        public virtual SplitProductItem GetSplitProductItemById(int? store, int splitProductItemId)
        {
            if (splitProductItemId == 0)
            {
                return null;
            }

            return SplitProductItemsRepository.ToCachedGetById(splitProductItemId);
        }

        public virtual void InsertSplitProductItem(SplitProductItem splitProductItem)
        {
            if (splitProductItem == null)
            {
                throw new ArgumentNullException("splitProductItem");
            }

            var uow = SplitProductItemsRepository.UnitOfWork;
            SplitProductItemsRepository.Insert(splitProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(splitProductItem);
        }

        public virtual void UpdateSplitProductItem(SplitProductItem splitProductItem)
        {
            if (splitProductItem == null)
            {
                throw new ArgumentNullException("splitProductItem");
            }

            var uow = SplitProductItemsRepository.UnitOfWork;
            SplitProductItemsRepository.Update(splitProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(splitProductItem);
        }

        public virtual void DeleteSplitProductItem(SplitProductItem splitProductItem)
        {
            if (splitProductItem == null)
            {
                throw new ArgumentNullException("splitProductItem");
            }

            var uow = SplitProductItemsRepository.UnitOfWork;
            SplitProductItemsRepository.Delete(splitProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(splitProductItem);
        }


        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, SplitProductBill bill, SplitProductBillUpdate data, List<SplitProductItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false)
        {
            var uow = SplitProductBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新
                    if (bill != null)
                    {
                        bill.WareHouseId = data.WareHouseId;
                        bill.ProductId = data.ProductId;
                        bill.Quantity = data.Quantity;
                        bill.ProductCost = data.ProductCost;
                        bill.ProductCostAmount = data.ProductCostAmount;
                        bill.Remark = data.Remark;
                        bill.CostDifference = data.CostDifference;
                        UpdateSplitProductBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加

                    bill.WareHouseId = data.WareHouseId;
                    bill.ProductId = data.ProductId;
                    bill.Quantity = data.Quantity;
                    bill.ProductCost = data.ProductCost ?? 0;
                    bill.ProductCostAmount = data.ProductCostAmount ?? 0;
                    bill.Remark = data.Remark;
                    bill.CostDifference = data.CostDifference ?? 0;

                    bill.StoreId = storeId;
                    //开单日期
                    bill.CreatedOnUtc = DateTime.Now;
                    //单据编号
                    bill.BillNumber = CommonHelper.GetBillNumber("CFD", storeId);
                    //制单人
                    bill.MakeUserId = userId;
                    //状态(审核)
                    bill.AuditedStatus = false;
                    bill.AuditedDate = null;
                    //红冲状态
                    bill.ReversedStatus = false;
                    bill.ReversedDate = null;
                    //备注
                    bill.Remark = data.Remark;
                    bill.Operation = data.Operation;//标识操作源

                    InsertSplitProductBill(bill);

                    #endregion
                }

                #region 更新项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var sd = GetSplitProductItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.StoreId = storeId;
                                item.SplitProductBillId = bill.Id;
                                item.CreatedOnUtc = DateTime.Now;
                                InsertSplitProductItem(item);
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
                            sd.SplitProductBillId = bill.Id;
                            sd.ProductId = p.ProductId;
                            sd.SubProductQuantity = p.SubProductQuantity;
                            sd.UnitId = p.UnitId;
                            sd.SubProductUnitId = p.SubProductUnitId;
                            sd.Quantity = p.Quantity;
                            sd.CostPrice = p.CostPrice;
                            sd.CostAmount = p.CostAmount;
                            sd.Stock = p.Stock;
                            sd.Remark = p.Remark;
                            UpdateSplitProductItem(sd);
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
                        var item = GetSplitProductItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteSplitProductItem(item);
                        }
                    }
                });

                #endregion

                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = billId ?? 0, Message = Resources.Bill_CreateOrUpdateSuccessful };
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

        public BaseResult Auditing(int storeId, int userId, SplitProductBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            var uow = SplitProductBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

                #region 修改库存
                try
                {
                    //主商品 出库
                    List<ProductStockItem> productStockItems1 = new List<ProductStockItem>();
                    //主商品单位
                    var p = _productService.GetProductById(storeId, bill.ProductId);
                    productStockItems1.Add(new ProductStockItem()
                    {
                        ProductId = bill.ProductId,
                        Quantity = bill.Quantity.Value * (-1),
                        ProductName = p?.Name,
                        ProductCode = p?.ProductCode,
                        UnitId = p.SmallUnitId,
                        SmallUnitId = p.SmallUnitId,
                        BigUnitId = p.BigUnitId ?? 0,
                    });

                    //减少现货
                    historyDatas1 = _stockService.AdjustStockQty<SplitProductBill, SplitProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems1, StockFlowChangeTypeEnum.Audited);

                    //子商品 入库
                    List<ProductStockItem> productStockItems2 = new List<ProductStockItem>();
                    var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                    foreach (var item in bill.Items)
                    {
                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                        ProductStockItem productStockItem = productStockItems2.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                        //商品转化量
                        var conversionQuantity = product.GetConversionQuantity(allOptions, (item.SubProductUnitId ?? 0));
                        //库存量增量 = 单位转化量 * 数量
                        int thisQuantity = (item.Quantity ?? 0) * conversionQuantity;
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

                            productStockItems2.Add(productStockItem);
                        }

                    }

                    historyDatas2 = _stockService.AdjustStockQty<SplitProductBill, SplitProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems2, StockFlowChangeTypeEnum.Audited);
                }
                catch (Exception)
                {
                }

                #endregion

                #region 修改单据表状态
                bill.AuditedUserId = userId;
                bill.AuditedDate = DateTime.Now;
                bill.AuditedStatus = true;

                UpdateSplitProductBill(bill);
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

        public BaseResult Reverse(int userId, SplitProductBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = SplitProductBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

                #region 修改库存


                try
                {
                    //主商品 入库
                    List<ProductStockItem> productStockItems1 = new List<ProductStockItem>();
                    //主商品单位
                    var p = _productService.GetProductById(bill.StoreId, bill.ProductId);
                    productStockItems1.Add(new ProductStockItem()
                    {
                        ProductId = bill.ProductId,
                        Quantity = bill.Quantity.Value,
                        ProductName = p?.Name,
                        ProductCode = p?.ProductCode,
                        UnitId = p.SmallUnitId,
                        SmallUnitId = p.SmallUnitId,
                        BigUnitId = p.BigUnitId ?? 0,
                    });

                    historyDatas1 = _stockService.AdjustStockQty<SplitProductBill, SplitProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems1, StockFlowChangeTypeEnum.Reversed);

                    //子商品 出库
                    List<ProductStockItem> productStockItems2 = new List<ProductStockItem>();
                    var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());
                    foreach (var item in bill.Items)
                    {
                        var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                        ProductStockItem productStockItem = productStockItems2.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                        //商品转化量
                        var conversionQuantity = product.GetConversionQuantity(allOptions, (item.SubProductUnitId ?? 0));
                        //库存量增量 = 单位转化量 * 数量
                        int thisQuantity = (item.Quantity ?? 0) * conversionQuantity;
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

                            productStockItems2.Add(productStockItem);
                        }
                    }

                    List<ProductStockItem> productStockItemThiss = new List<ProductStockItem>();
                    if (productStockItems2 != null && productStockItems2.Count > 0)
                    {
                        productStockItems2.ForEach(psi =>
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

                    historyDatas2 = _stockService.AdjustStockQty<SplitProductBill, SplitProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Reversed);
                }
                catch (Exception)
                {
                }

                #endregion

                #region 修改单据表状态
                bill.ReversedUserId = userId;
                bill.ReversedDate = DateTime.Now;
                bill.ReversedStatus = true;

                UpdateSplitProductBill(bill);
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

    }
}
