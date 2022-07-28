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
    /// 用于表示商品报损单服务
    /// </summary>
    public partial class ScrapProductBillService : BaseService, IScrapProductBillService
    {

        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IUserService _userService;

        public ScrapProductBillService(IServiceGetter getter,
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
            return ScrapProductBillsRepository.TableNoTracking.Where(a => a.Id == billId).Count() > 0;
        }

        public virtual IPagedList<ScrapProductBill> GetAllScrapProductBills(int? store, int? makeuserId, int? chargePerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = ScrapProductBillsRepository.Table;

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

            if (!string.IsNullOrWhiteSpace(remark))
            {
                query = query.Where(c => c.Remark.Contains(remark));
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);


            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ScrapProductBill>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<ScrapProductBill> GetAllScrapProductBills()
        {
            var query = from c in ScrapProductBillsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual ScrapProductBill GetScrapProductBillById(int? store, int scrapProductBillId)
        {
            if (scrapProductBillId == 0)
            {
                return null;
            }

            var key = DCMSDefaults.SCRAPPRODUCTBILL_BY_ID_KEY.FillCacheKey(store ?? 0, scrapProductBillId);
            return _cacheManager.Get(key, () =>
            {
                return ScrapProductBillsRepository.ToCachedGetById(scrapProductBillId);
            });
        }

        public virtual ScrapProductBill GetScrapProductBillById(int? store, int scrapProductBillId, bool isInclude = false)
        {
            if (scrapProductBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = ScrapProductBillsRepository_RO.Table.Include(sp => sp.Items);
                return query.FirstOrDefault(s => s.Id == scrapProductBillId);
            }
            return ScrapProductBillsRepository.ToCachedGetById(scrapProductBillId);
        }


        public virtual ScrapProductBill GetScrapProductBillByNumber(int? store, string billNumber)
        {
            var key = DCMSDefaults.SCRAPPRODUCTBILL_BY_NUMBER_KEY.FillCacheKey(store ?? 0, billNumber);
            return _cacheManager.Get(key, () =>
            {
                var query = ScrapProductBillsRepository.Table;
                var scrapProductBill = query.Where(a => a.BillNumber == billNumber).FirstOrDefault();
                return scrapProductBill;
            });
        }



        public virtual void InsertScrapProductBill(ScrapProductBill scrapProductBill)
        {
            if (scrapProductBill == null)
            {
                throw new ArgumentNullException("scrapProductBill");
            }

            var uow = ScrapProductBillsRepository.UnitOfWork;
            ScrapProductBillsRepository.Insert(scrapProductBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(scrapProductBill);
        }

        public virtual void UpdateScrapProductBill(ScrapProductBill scrapProductBill)
        {
            if (scrapProductBill == null)
            {
                throw new ArgumentNullException("scrapProductBill");
            }

            var uow = ScrapProductBillsRepository.UnitOfWork;
            ScrapProductBillsRepository.Update(scrapProductBill);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(scrapProductBill);
        }

        public virtual void DeleteScrapProductBill(ScrapProductBill scrapProductBill)
        {
            if (scrapProductBill == null)
            {
                throw new ArgumentNullException("scrapProductBill");
            }

            var uow = ScrapProductBillsRepository.UnitOfWork;
            ScrapProductBillsRepository.Delete(scrapProductBill);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(scrapProductBill);
        }


        #endregion

        #region 单据项目


        public virtual IPagedList<ScrapProductItem> GetScrapProductItemsByScrapProductBillId(int scrapProductBillId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (scrapProductBillId == 0)
            {
                return new PagedList<ScrapProductItem>(new List<ScrapProductItem>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.SCRAPPRODUCTBILLITEM_ALL_KEY.FillCacheKey(storeId, scrapProductBillId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in ScrapProductItemsRepository.Table
                            where pc.ScrapProductBillId == scrapProductBillId
                            orderby pc.Id
                            select pc;
                //var productScrapProductBills = new PagedList<ScrapProductItem>(query.ToList(), pageIndex, pageSize);
                //return productScrapProductBills;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<ScrapProductItem>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<ScrapProductItem> GetScrapProductItemList(int scrapProductBillId)
        {
            List<ScrapProductItem> scrapProductItems = null;
            var query = ScrapProductItemsRepository.Table;
            scrapProductItems = query.Where(a => a.ScrapProductBillId == scrapProductBillId).ToList();
            return scrapProductItems;
        }

        public virtual ScrapProductItem GetScrapProductItemById(int? store, int scrapProductItemId)
        {
            if (scrapProductItemId == 0)
            {
                return null;
            }

            return ScrapProductItemsRepository.ToCachedGetById(scrapProductItemId);
        }

        public virtual void InsertScrapProductItem(ScrapProductItem scrapProductItem)
        {
            if (scrapProductItem == null)
            {
                throw new ArgumentNullException("scrapProductItem");
            }

            var uow = ScrapProductItemsRepository.UnitOfWork;
            ScrapProductItemsRepository.Insert(scrapProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(scrapProductItem);
        }

        public virtual void UpdateScrapProductItem(ScrapProductItem scrapProductItem)
        {
            if (scrapProductItem == null)
            {
                throw new ArgumentNullException("scrapProductItem");
            }

            var uow = ScrapProductItemsRepository.UnitOfWork;
            ScrapProductItemsRepository.Update(scrapProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(scrapProductItem);
        }

        public virtual void DeleteScrapProductItem(ScrapProductItem scrapProductItem)
        {
            if (scrapProductItem == null)
            {
                throw new ArgumentNullException("scrapProductItem");
            }

            var uow = ScrapProductItemsRepository.UnitOfWork;
            ScrapProductItemsRepository.Delete(scrapProductItem);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(scrapProductItem);
        }


        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ScrapProductBill scrapProductBill, ScrapProductBillUpdate data, List<ScrapProductItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false)
        {
            var uow = ScrapProductBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();


                if (billId.HasValue && billId.Value != 0)
                {
                    #region 更新报损单
                    if (scrapProductBill != null)
                    {
                        scrapProductBill.ChargePerson = data.ChargePerson;
                        scrapProductBill.WareHouseId = data.WareHouseId;
                        scrapProductBill.ScrapByBaseUnit = data.ScrapByBaseUnit;
                        scrapProductBill.Remark = data.Remark;
                        UpdateScrapProductBill(scrapProductBill);
                    }

                    #endregion
                }
                else
                {
                    #region 添加报损单

                    scrapProductBill.ChargePerson = data.ChargePerson;
                    scrapProductBill.WareHouseId = data.WareHouseId;
                    scrapProductBill.ScrapByBaseUnit = data.ScrapByBaseUnit;
                    scrapProductBill.Remark = data.Remark;

                    scrapProductBill.StoreId = storeId;
                    //开单日期
                    scrapProductBill.CreatedOnUtc = DateTime.Now;
                    //单据编号
                    scrapProductBill.BillNumber = CommonHelper.GetBillNumber("BSD", storeId);
                    //制单人
                    scrapProductBill.MakeUserId = userId;
                    //状态(审核)
                    scrapProductBill.AuditedStatus = false;
                    scrapProductBill.AuditedDate = null;
                    //红冲状态
                    scrapProductBill.ReversedStatus = false;
                    scrapProductBill.ReversedDate = null;
                    //备注
                    scrapProductBill.Remark = data.Remark;
                    scrapProductBill.Operation = data.Operation;//标识操作源

                    InsertScrapProductBill(scrapProductBill);

                    #endregion
                }

                #region 更新报损项目

                data.Items.ForEach(p =>
                {
                    if (p.ProductId != 0)
                    {
                        var sd = GetScrapProductItemById(storeId, p.Id);
                        if (sd == null)
                        {
                            //追加项
                            if (scrapProductBill.Items.Count(cp => cp.Id == p.Id) == 0)
                            {
                                var item = p;
                                item.StoreId = storeId;
                                item.ScrapProductBillId = scrapProductBill.Id;
                                item.CreatedOnUtc = DateTime.Now;

                                item.CostPrice = p.CostPrice ?? 0;
                                item.CostAmount = p.CostAmount ?? 0;
                                item.TradePrice = p.TradePrice ?? 0;
                                item.TradeAmount = p.TradeAmount ?? 0;

                                InsertScrapProductItem(item);
                                //不排除
                                p.Id = item.Id;
                                if (!scrapProductBill.Items.Select(s => s.Id).Contains(item.Id))
                                {
                                    scrapProductBill.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            //存在则更新
                            sd.ScrapProductBillId = scrapProductBill.Id;
                            sd.ProductId = p.ProductId;
                            sd.UnitId = p.UnitId;
                            sd.Quantity = p.Quantity;

                            sd.CostPrice = p.CostPrice ?? 0;
                            sd.CostAmount = p.CostAmount ?? 0;
                            sd.TradePrice = p.TradePrice ?? 0;
                            sd.TradeAmount = p.TradeAmount ?? 0;

                            UpdateScrapProductItem(sd);
                        }
                    }
                });

                #endregion

                #region Grid 移除则从库移除删除项

                scrapProductBill.Items.ToList().ForEach(p =>
                {
                    if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                    {
                        scrapProductBill.Items.Remove(p);
                        var item = GetScrapProductItemById(storeId, p.Id);
                        if (item != null)
                        {
                            DeleteScrapProductItem(item);
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

        public BaseResult Auditing(int storeId, int userId, ScrapProductBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据审核成功" };
            var failed = new BaseResult { Success = false, Message = "单据审核失败" };

            try
            {
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                return _recordingVoucherService.CreateVoucher<ScrapProductBill, ScrapProductItem>(bill, storeId, userId, (voucherId) =>
                {
                    #region 修改库存
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

                    //减少现货
                    historyDatas1 = _stockService.AdjustStockQty<ScrapProductBill, ScrapProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.Out, StockQuantityType.CurrentQuantity, bill.WareHouseId, productStockItemThiss, StockFlowChangeTypeEnum.Audited);

                    #endregion

                    #region 修改单据表状态
                    bill.VoucherId = voucherId;
                    bill.AuditedUserId = userId;
                    bill.AuditedDate = DateTime.Now;
                    bill.AuditedStatus = true;
                    UpdateScrapProductBill(bill);
                    #endregion
                },
               () =>
               {
                   return successful;
               },
               () => { return failed; });

            }
            catch (Exception)
            {
                return failed;
            }
        }

        public BaseResult Reverse(int userId, ScrapProductBill bill)
        {
            var successful = new BaseResult { Success = true, Message = "单据红冲成功" };
            var failed = new BaseResult { Success = false, Message = "单据红冲失败" };

            var uow = ScrapProductBillsRepository.UnitOfWork;
            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();
                //历史库存记录
                Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> historyDatas1 = null;

                #region 红冲记账凭证

                _recordingVoucherService.CancleVoucher<ScrapProductBill, ScrapProductItem>(bill, () =>
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

                        historyDatas1 = _stockService.AdjustStockQty<ScrapProductBill, ScrapProductItem>(bill, _productService, _specificationAttributeService, DirectionEnum.In, StockQuantityType.CurrentQuantity, bill.WareHouseId, stockProducts, StockFlowChangeTypeEnum.Reversed);
                    }
                    catch (Exception)
                    {
                    }

                    #endregion

                    #region 修改单据表状态
                    bill.ReversedUserId = userId;
                    bill.ReversedDate = DateTime.Now;
                    bill.ReversedStatus = true;
                    //UpdateScrapProductBill(scrapProductBill);
                    #endregion

                    bill.VoucherId = 0;
                    UpdateScrapProductBill(bill);
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



    }
}
