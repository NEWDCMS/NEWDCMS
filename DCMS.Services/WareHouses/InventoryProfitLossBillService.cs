using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Finances;
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
    /// 用于表示盘点盈亏单服务
    /// </summary>
    public partial class InventoryProfitLossBillService : BaseService, IInventoryProfitLossBillService
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IUserService _userService;

        public InventoryProfitLossBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IStockService stockService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IRecordingVoucherService recordingVoucherService,
            IUserService userService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _stockService = stockService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _recordingVoucherService = recordingVoucherService;
            _userService = userService;
        }

        #region 单据

        public bool Exists(int billId)
        {
            return InventoryProfitLossBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<InventoryProfitLossBill> GetAllInventoryProfitLossBills(int? store, int? makeuserId, int? chargePerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, bool? deleted = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = InventoryProfitLossBillsRepository.Table;

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


            if (chargePerson.HasValue && chargePerson.Value > 0)
            {
                query = query.Where(c => c.ChargePerson == chargePerson);
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

            if (!string.IsNullOrEmpty(remark))
            {
                query = query.Where(o => o.Remark.Contains(remark));
            }

            if (deleted.HasValue)
            {
                query = query.Where(a => a.Deleted == deleted);
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
            return new PagedList<InventoryProfitLossBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<InventoryProfitLossBill> GetAllInventoryProfitLossBills()
        {
            var query = from c in InventoryProfitLossBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual InventoryProfitLossBill GetInventoryProfitLossBillById(int? store, int inventoryProfitLossBillId)
        {
            if (inventoryProfitLossBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.INVENTORPROFITLOSSBILL_BY_ID_KEY.FillCacheKey(store ?? 0, inventoryProfitLossBillId);
            return _cacheManager.Get(key, () =>
            {
                return InventoryProfitLossBillsRepository.ToCachedGetById(inventoryProfitLossBillId);
            });
        }

        public virtual InventoryProfitLossBill GetInventoryProfitLossBillById(int? store, int inventoryProfitLossBillId, bool isInclude = false)
        {
            if (inventoryProfitLossBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = InventoryProfitLossBillsRepository_RO.Table.Include(it => it.Items);
                return query.FirstOrDefault(c => c.Id == inventoryProfitLossBillId);
            }
            return InventoryProfitLossBillsRepository.ToCachedGetById(inventoryProfitLossBillId);
        }


        public virtual InventoryProfitLossBill GetInventoryProfitLossBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.INVENTORPROFITLOSSBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = InventoryProfitLossBillsRepository.Table;
                var bill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return bill;
            });
        }



        public virtual void InsertInventoryProfitLossBill(InventoryProfitLossBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = InventoryProfitLossBillsRepository.UnitOfWork;
            InventoryProfitLossBillsRepository.Insert(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(bill);
        }

        public virtual void UpdateInventoryProfitLossBill(InventoryProfitLossBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = InventoryProfitLossBillsRepository.UnitOfWork;
            InventoryProfitLossBillsRepository.Update(bill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(bill);
        }

        public virtual void DeleteInventoryProfitLossBill(InventoryProfitLossBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = InventoryProfitLossBillsRepository.UnitOfWork;
            InventoryProfitLossBillsRepository.Delete(bill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(bill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<InventoryProfitLossItem> GetInventoryProfitLossItemsByInventoryProfitLossBillId(int inventoryProfitLossBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (inventoryProfitLossBillId == 0)
            {
                return new PagedList<InventoryProfitLossItem>(new List<InventoryProfitLossItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.INVENTORPROFITLOSSBILLITEM_ALL_KEY.FillCacheKey(storeId, inventoryProfitLossBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in InventoryProfitLossItemsRepository.Table
                            where pc.InventoryProfitLossBillId == inventoryProfitLossBillId
                            orderby pc.Id
                            select pc;
                //var productInventoryProfitLossBills = new PagedList<InventoryProfitLossItem>(query.ToList(), pageIndex, pageSize);
                //return productInventoryProfitLossBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<InventoryProfitLossItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<InventoryProfitLossItem> GetInventoryProfitLossItemList(int inventoryProfitLossBillId)
        {
            List<InventoryProfitLossItem> inventoryProfitLossItems = null;
            var query = InventoryProfitLossItemsRepository.Table;
            inventoryProfitLossItems = query.Where(a => a.InventoryProfitLossBillId == inventoryProfitLossBillId).ToList();
            return inventoryProfitLossItems;
        }


        public virtual InventoryProfitLossItem GetInventoryProfitLossItemById(int? store, int inventoryProfitLossItemId)
        {
            if (inventoryProfitLossItemId == 0)
            {
                return null;
            }

            return InventoryProfitLossItemsRepository.ToCachedGetById(inventoryProfitLossItemId);
        }

        public virtual void InsertInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem)
        {
            if (inventoryProfitLossItem == null)
            {
                throw new ArgumentNullException("inventoryProfitLossItem");
            }

            var uow = InventoryProfitLossItemsRepository.UnitOfWork;
            InventoryProfitLossItemsRepository.Insert(inventoryProfitLossItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(inventoryProfitLossItem);
        }

        public virtual void UpdateInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem)
        {
            if (inventoryProfitLossItem == null)
            {
                throw new ArgumentNullException("inventoryProfitLossItem");
            }

            var uow = InventoryProfitLossItemsRepository.UnitOfWork;
            InventoryProfitLossItemsRepository.Update(inventoryProfitLossItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(inventoryProfitLossItem);
        }

        public virtual void DeleteInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem)
        {
            if (inventoryProfitLossItem == null)
            {
                throw new ArgumentNullException("inventoryProfitLossItem");
            }

            var uow = InventoryProfitLossItemsRepository.UnitOfWork;
            InventoryProfitLossItemsRepository.Delete(inventoryProfitLossItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(inventoryProfitLossItem);
        }


        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryProfitLossBill bill, InventoryProfitLossBillUpdate data, List<InventoryProfitLossItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false)
        {
            var uow = InventoryProfitLossBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新盘点盈亏单

                    if (bill != null)
                    {

                        bill.ChargePerson = data.ChargePerson;
                        bill.WareHouseId = data.WareHouseId;
                        bill.InventoryByMinUnit = data.InventoryByMinUnit;
                        bill.InventoryDate = DateTime.Now;
                        bill.Remark = data.Remark;
                        UpdateInventoryProfitLossBill(bill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加盘点盈亏单

                    bill.ChargePerson = data.ChargePerson;
                    bill.WareHouseId = data.WareHouseId;
                    bill.InventoryByMinUnit = data.InventoryByMinUnit;
                    bill.InventoryDate = DateTime.Now;
                    bill.Remark = data.Remark;

                    bill.StoreId = storeId;
                    //开单日期
                    bill.CreatedOnUtc = DateTime.Now;
                    //单据编号
                    bill.BillNumber = CommonHelper.GetBillNumber("PDYK", storeId);
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

                    InsertInventoryProfitLossBill(bill);

                    #endregion
                }

                #region 更新盘点盈亏项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var sd = GetInventoryProfitLossItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (bill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.StoreId = storeId;
                                item.InventoryProfitLossBillId = bill.Id;
                                item.CostPrice = p.CostPrice ?? 0;
                                item.CostAmount = p.CostAmount ?? 0;
                                item.CreatedOnUtc = DateTime.Now;
                                InsertInventoryProfitLossItem(item);
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
                            sd.InventoryProfitLossBillId = bill.Id;
                            sd.ProductId = p.ProductId;
                            sd.UnitId = p.UnitId;
                            sd.Quantity = p.Quantity;
                            sd.CostPrice = p.CostPrice ?? 0;
                            sd.CostAmount = p.CostAmount ?? 0;
                            sd.StockQty = p.StockQty;
                            UpdateInventoryProfitLossItem(sd);
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
                        var item = GetInventoryProfitLossItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteInventoryProfitLossItem(item);
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

        public BaseResult Auditing(int storeId, int userId, InventoryProfitLossBill bill)
        {
            var uow = InventoryProfitLossBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

                #region 修改库存
                var stockProducts = new List<ProductStockItem>();
                var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());
                foreach (InventoryProfitLossItem item in bill.Items)
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

                //盘盈现货
                List<ProductStockItem> productStockItems22 = new List<ProductStockItem>();
                //盘亏现货
                List<ProductStockItem> productStockItems32 = new List<ProductStockItem>();

                if (stockProducts != null && stockProducts.Count > 0)
                {
                    stockProducts.ForEach(p =>
                    {
                        if (p.Quantity >= 0)
                        {
                            productStockItems22.Add(new ProductStockItem()
                            {
                                ProductId = p.ProductId,
                                Quantity = p.Quantity,
                                ProductName = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.ProductCode,
                                UnitId = p.UnitId,
                                SmallUnitId = p.SmallUnitId,
                                BigUnitId = p.BigUnitId
                            });

                        }
                        if (p.Quantity < 0)
                        {
                            productStockItems32.Add(new ProductStockItem()
                            {
                                ProductId = p.ProductId,
                                Quantity = p.Quantity,
                                ProductName = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.ProductCode,
                                UnitId = p.UnitId,
                                SmallUnitId = p.SmallUnitId,
                                BigUnitId = p.BigUnitId
                            });
                        }
                    });
                }

                //盘盈
                //现货（增加）
                historyDatas1 = _stockService.AdjustStockQty<InventoryProfitLossBill, InventoryProfitLossItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems22, StockFlowChangeTypeEnum.Audited);

                //盘亏
                //现货（减少）
                historyDatas2 = _stockService.AdjustStockQty<InventoryProfitLossBill, InventoryProfitLossItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems32, StockFlowChangeTypeEnum.Audited);

                #endregion

                #region 修改单据表状态
                bill.AuditedUserId = userId;
                bill.AuditedDate = DateTime.Now;
                bill.AuditedStatus = true;

                UpdateInventoryProfitLossBill(bill);
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

        public BaseResult Reverse(int userId, InventoryProfitLossBill bill)
        {
            var uow = InventoryProfitLossBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas2 = null;

                #region 修改库存
                var stockProducts = new List<ProductStockItem>();

                var allProducts = _productService.GetProductsByIds(bill.StoreId, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());
                foreach (InventoryProfitLossItem item in bill.Items)
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

                List<ProductStockItem> productStockItems2 = new List<ProductStockItem>();
                List<ProductStockItem> productStockItems3 = new List<ProductStockItem>();
                if (stockProducts != null && stockProducts.Count > 0)
                {
                    stockProducts.ForEach(p =>
                    {
                        if (p.Quantity >= 0)
                        {
                            productStockItems2.Add(new ProductStockItem()
                            {
                                ProductId = p.ProductId,
                                Quantity = p.Quantity * (-1),
                                ProductName = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.ProductCode,
                                UnitId = p.UnitId,
                                SmallUnitId = p.SmallUnitId,
                                BigUnitId = p.BigUnitId
                            });
                        }
                        if (p.Quantity < 0)
                        {
                            productStockItems3.Add(new ProductStockItem()
                            {
                                ProductId = p.ProductId,
                                Quantity = p.Quantity * (-1),
                                ProductName = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.Name,
                                ProductCode = allProducts.Where(s => s.Id == p.ProductId).FirstOrDefault()?.ProductCode,
                                UnitId = p.UnitId,
                                SmallUnitId = p.SmallUnitId,
                                BigUnitId = p.BigUnitId
                            });
                        }
                    });
                }
                //注意这里（+）出库
                historyDatas1 = _stockService.AdjustStockQty<InventoryProfitLossBill, InventoryProfitLossItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems2, StockFlowChangeTypeEnum.Reversed);
                //注意这里（-）入库
                historyDatas2 = _stockService.AdjustStockQty<InventoryProfitLossBill, InventoryProfitLossItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItems3, StockFlowChangeTypeEnum.Reversed);

                #endregion

                #region 修改单据表状态
                bill.ReversedUserId = userId;
                bill.ReversedDate = DateTime.Now;
                bill.ReversedStatus = true;
                UpdateInventoryProfitLossBill(bill);
                #endregion

                #region 红冲记账凭证
                #region delete
                ////红冲记账凭证
                //List<RecordingVoucher> recordingVouchers = _recordingVoucherService.GetRecordingVouchers(bill.StoreId, (int)BillTypeEnum.InventoryProfitLossBill, bill.BillNumber).ToList();
                //if (recordingVouchers != null && recordingVouchers.Count > 0)
                //{
                //    recordingVouchers.ForEach(rv =>
                //    {
                //        //凭证
                //        var recordingVoucher = new RecordingVoucher()
                //        {
                //            //单据编号
                //            BillNumber = bill.BillNumber,
                //            //单据类型
                //            BillTypeId = (int)BillTypeEnum.InventoryProfitLossBill,
                //            //系统生成
                //            GenerateMode = (int)GenerateMode.Auto
                //        };

                //        if (rv.Items != null && rv.Items.Count > 0)
                //        {
                //            rv.Items.ToList().ForEach(it =>
                //            {
                //                var voucherItem = new VoucherItem()
                //                {
                //                    Summary = it.Summary,
                //                    AccountingOptionId = it.AccountingOptionId,
                //                    DebitAmount = it.DebitAmount * (-1),
                //                    CreditAmount = it.CreditAmount * (-1)
                //                };
                //                //添加项
                //                recordingVoucher.Items.Add(voucherItem);
                //            });
                //        }
                //        //创建
                //        //自动审核凭证
                //        recordingVoucher.AuditedUserId = userId;
                //        recordingVoucher.AuditedDate = DateTime.Now;
                //        recordingVoucher.AuditedStatus = true;
                //        _recordingVoucherService.CreateRecordingVoucher(bill.StoreId, userId, recordingVoucher);

                //    });

                //} 
                #endregion

                //_recordingVoucherService.CancleVoucher<InventoryProfitLossBill, InventoryProfitLossItem>(bill, () =>
                //{
                //    bill.VoucherId = 0;
                //    UpdateInventoryProfitLossBill(bill);
                //});

                #endregion

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
        /// <summary>
        /// 单据作废 2021-09-03 mu 添加
        /// /// </summary>
        /// <param name="userId"></param>
        /// <param name="bill"></param>
        /// <returns></returns>
        public BaseResult Delete(int userId, InventoryProfitLossBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据作废成功" };
            var failed = new BaseResult { Success = false, Message = "单据作废失败" };

            var uow = InventoryProfitLossBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();
                #region 修改单据表状态
                bill.Deleted = true;
                #endregion
                UpdateInventoryProfitLossBill(bill);
                //提交事务
                transaction.Commit();
                return successful;
            }
            catch (Exception)
            {
                //异常回滚事务
                transaction?.Rollback();
                return failed;
            }
            finally
            {
                //关闭事务
                using (transaction) { }
            }
        }
    }
}
