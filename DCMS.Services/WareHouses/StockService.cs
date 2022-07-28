using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching;
using DCMS.Services.Events;
using DCMS.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.WareHouses
{
    /// <summary>
    /// 用于表示库存服务
    /// </summary>
    public class StockService : BaseService, IStockService
    {

        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public StockService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
             IProductService productService,
             ISpecificationAttributeService specificationAttributeService,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
        }

        #region 库存


        public virtual IPagedList<Stock> GetAllStocks(int? store, int? wareHouseId, int? productId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = StocksRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }
            else
            {
                return null;
            }

            if (wareHouseId.HasValue && wareHouseId.Value != 0)
            {
                query = query.Where(c => c.WareHouseId == wareHouseId);
            }

            if (productId.HasValue && productId.Value != 0)
            {
                query = query.Where(c => c.ProductId == productId);
            }

            if (start.HasValue)
            {
                query = query.Where(o => start.Value <= o.CreateTime);
            }

            if (end.HasValue)
            {
                query = query.Where(o => end.Value >= o.CreateTime);
            }

            query = query.OrderByDescending(c => c.Id);


            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Stock>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<Stock> GetAllStocks(int? store)
        {
            var query = from c in StocksRepository.Table
                        where c.StoreId == store
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual IList<Stock> GetAllStocksByProductIds(int? store, int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return new List<Stock>();
            }

            var key = DCMSDefaults.STOCKS_BY_PRODUCTIDS_KEY.FillCacheKey(store, string.Join("_", productIds.OrderBy(a => a)));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in StocksRepository.Table
                            where c.StoreId == store &&
                             productIds.Contains(c.ProductId)
                            select c;
                var categories = query.ToList();
                return categories;
            });
        }


        public virtual Stock GetStockById(int? store, int stockId)
        {
            if (stockId == 0)
            {
                return null;
            }

            return StocksRepository.ToCachedGetById(stockId);
        }

        public virtual void InsertStock(Stock stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException("stock");
            }

            var uow = StocksRepository.UnitOfWork;
            StocksRepository.Insert(stock);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(stock);
        }

        public virtual void UpdateStock(Stock stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException("stock");
            }

            var uow = StocksRepository.UnitOfWork;
            StocksRepository.Update(stock);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(stock);
        }


        public virtual void UpdateStocks(List<Stock> stocks)
        {
            if (stocks == null)
            {
                throw new ArgumentNullException("stocks");
            }

            var uow = StocksRepository.UnitOfWork;
            StocksRepository.Update(stocks);
            uow.SaveChanges();

            stocks.ForEach(s => { _eventPublisher.EntityUpdated(s); });

        }

        public virtual void DeleteStock(Stock stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException("stock");
            }

            var uow = StocksRepository.UnitOfWork;
            StocksRepository.Delete(stock);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(stock);
        }

        public virtual void DeleteStocks(List<Stock> stocks)
        {
            if (stocks == null)
            {
                throw new ArgumentNullException("stocks");
            }

            var uow = StocksRepository.UnitOfWork;
            StocksRepository.Delete(stocks);
            uow.SaveChanges();

            stocks.ForEach(s => { _eventPublisher.EntityDeleted(s); });

        }

        public virtual IList<StockQty> GetAllStockProducts(int? store)
        {

            string sqlString = @"SELECT 
                                ProductId,
                                0 Direction,
                                p.Name as ProductName,
                                p.ProductCode as ProductCode,
                                p.SmallUnitId as UnitId,
                                p.SmallUnitId,
                                p.BigUnitId,
                                0 BillId,
                                0 BillType,
                                '' BillCode,
                                ssao.Name as UnitName,
                                ssao.Name as SmallUnitName,
                                bsao.Name  as BigUnitName,
                                0 Price,
                                p.StrokeQuantity,
                                p.BigQuantity,
                                0 Quantity,
                                now() as CreatedOnUtc
                                FROM dcms.Stocks  as stc
                                left join Products as p on stc.ProductId = p.Id
                                left join SpecificationAttributeOptions as ssao on p.SmallUnitId = ssao.Id
                                left join SpecificationAttributeOptions as bsao on p.BigUnitId = bsao.Id
                                where stc.StoreId = " + store + " group by ProductId";

            var lists = StocksRepository.QueryFromSql<StockQty>(sqlString).ToList();
            return lists.ToList();

        }

        public virtual bool StockExist(int? store, int? wareHouseId, int? productId)
        {
            var query = StocksRepository.Table;
            query = query.Where(c => c.StoreId == store && c.WareHouseId == wareHouseId && c.ProductId == productId);
            return query.ToList().Count > 0;
        }
        public virtual Stock GetCurrentStock(int? store, int? wareHouseId, int? productId)
        {
            var query = StocksRepository.Table;
            query = query.Where(c => c.StoreId == store && c.WareHouseId == wareHouseId && c.ProductId == productId);
            return query.FirstOrDefault();
        }

        /// <summary>
        ///调整库存(乐观锁)
        /// </summary>
        /// <param name="newStock"></param>
        /// <param name="oldStock"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual bool AdjustmentStock(Stock newStock, Stock oldStock, int? store, int? userId, int? wareHouseId, int? productId)
        {
            try
            {
                string sqlString = $"update dcms.Stocks set " +
                  $" CurrentQuantity = {newStock.CurrentQuantity}," +
                  $" OrderQuantity = {newStock.OrderQuantity}," +
                  $" LockQuantity = {newStock.LockQuantity}," +
                  $" UsableQuantity = {newStock.UsableQuantity}," +
                  $" UpdaterId = {newStock.UpdaterId}," +
                  $" TimeStamp = '{newStock.UpdateTime.Value:yyyy-MM-dd HH:mm:ss}', " +
                  $" UpdateTime = '{newStock.UpdateTime.Value:yyyy-MM-dd HH:mm:ss}', " +
                  $" Version = {oldStock.Version + 1}" +
                  $" where Id ='{oldStock.Id}' and Version = {oldStock.Version} ";
                var result = StocksRepository.ExecuteSqlScript(sqlString);

                //通知
                _eventPublisher.EntityInserted(newStock);
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region 修改验证库存新方法

        /// <summary>
        /// 验证当前商品库存
        /// </summary>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockProducts"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public virtual bool CheckStockQty(IProductService _productService, ISpecificationAttributeService _specificationAttributeService, int storeId, int wareHouseId, List<ProductStockItem> stockProducts, out string errMsg, bool enableOrderQuantity = true)
        {
            bool fg = true;
            errMsg = string.Empty;
            if (stockProducts != null && stockProducts.Count > 0)
            {
                var allProducts = _productService.GetProductsByIds(storeId, stockProducts.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                foreach (ProductStockItem p in stockProducts)
                {
                    if (p.Quantity > 0)
                    {
                        Stock stock = GetCurrentStock(storeId, wareHouseId, p.ProductId);
                        var wareHouse = WareHousesRepository.GetById(wareHouseId);
                        var product = allProducts.Where(ap => ap.Id == p.ProductId).FirstOrDefault();

                        if (stock == null)
                        {
                            fg = false;
                            errMsg += $"[{wareHouse?.Name}] 商品 [{product?.Name}],库存不存在";
                        }
                        else
                        {
                            //商品转化量
                            var conversionQuantity = product.GetConversionQuantity(allOptions, p.UnitId);

                            //当前库存需求量 = 单位转化量 * 数量
                            int currentIncrement = p.Quantity * conversionQuantity;

                            //可用库存量
                            int usableQuantity = stock.UsableQuantity ?? 0;  

                            //现货库存量
                            int currentQuantity = (stock.CurrentQuantity ?? 0);

                            //预占库存量
                            int orderQuantity = stock.OrderQuantity ?? 0;

                            //锁定库存量
                            int lockQuantity = stock.LockQuantity ?? 0;

                            //是否预占量
                            if (enableOrderQuantity)
                            {
                                //可用库存量 = 现货库存量 - 预占库存量 - 锁定库存量
                                usableQuantity = currentQuantity - orderQuantity - lockQuantity;
                            }
                            else
                            {
                                //可用库存量 = 现货库存量- 锁定库存量
                                usableQuantity = currentQuantity - lockQuantity;
                            }

                            if (Math.Abs(usableQuantity) < currentIncrement)
                            {
                                //单位名称
                                string unitName = _specificationAttributeService.GetSpecificationAttributeOptionName(storeId, product.SmallUnitId);
                                fg = false;
                                errMsg += $"[{wareHouse?.Name}] 商品 [{product?.Name}],库存不足！";
                            }
                        }
                    }
                }
            }

            return fg;
        }

        /// <summary>
        /// 调整商品库存数据,并返回库存更改失败项目，用于友好提示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="bill"></param>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="directionEnum"></param>
        /// <param name="stockQuantityType">要调整的库存数量类型</param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockProducts"></param>
        /// <param name="stockFlowChangeTypeEnum"></param>
        /// <returns>历史数据</returns>
        public Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> AdjustStockQty<T, T1>(T bill,
            IProductService _productService,
            ISpecificationAttributeService _specificationAttributeService,
            DirectionEnum directionEnum,
            StockQuantityType stockQuantityType,
            int wareHouseId,
            List<ProductStockItem> stockProducts,
            StockFlowChangeTypeEnum stockFlowChangeTypeEnum) where T : BaseBill<T1> where T1 : BaseEntity
        {
            return AdjustStockQty<T, T1>(bill, directionEnum, stockQuantityType, wareHouseId, stockProducts, stockFlowChangeTypeEnum);
        }


        #region 库存修改


        /// <summary>
        /// 调整库存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="bill"></param>
        /// <param name="directionEnum"></param>
        /// <param name="stockQuantityType"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockProducts"></param>
        /// <param name="stockFlowChangeTypeEnum"></param>
        /// <returns></returns>
        public Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> AdjustStockQty<T, T1>(T bill,
            DirectionEnum directionEnum,
            StockQuantityType stockQuantityType,
            int wareHouseId,
            List<ProductStockItem> stockProducts,
            StockFlowChangeTypeEnum stockFlowChangeTypeEnum) where T : BaseBill<T1> where T1 : BaseEntity
        {
            var faileds = new List<ProductStockItem>();

            //历史记录
            var old_StockInOutRecords = new StockInOutRecord();
            var old_StockFlows = new List<StockFlow>();
            var old_SRSFs = new List<StockInOutRecordStockFlow>();
            var old_Stocks = new List<Stock>();

            //变化记录
            var new_StockInOutRecords = new StockInOutRecord();
            var new_StockFlows = new List<StockFlow>();
            var new_SRSFs = new List<StockInOutRecordStockFlow>();
            var new_Stocks = new List<Stock>();

            var historyDatas = new Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>>(faileds, new Tuple<StockInOutRecord, StockInOutRecord>(old_StockInOutRecords, new_StockInOutRecords), new Tuple<List<StockFlow>, List<StockFlow>>(old_StockFlows, new_StockFlows), new Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>(old_SRSFs, new_SRSFs), new Tuple<List<Stock>, List<Stock>>(old_Stocks, new_Stocks));

            //项目商品总量
            int totalQuantity = 0;

            if (stockProducts.Count == 0)
            {
                return historyDatas;
            }

            try
            {
                //Step 1. 初始商品库存 Stocks
                var allProducts = _productService.GetProductsByIds(bill.StoreId, stockProducts.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, allProducts.GetProductBigStrokeSmallUnitIds());

                //注意：这里进入的量应该是转化后的量，如果是小单位则不用转化
                totalQuantity = stockProducts.Sum(s => s.Quantity);
                foreach (var item in stockProducts)
                {
                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        //初始库存,不存在则创建
                        if (!StockExist(bill.StoreId, wareHouseId, item.ProductId))
                        {
                            var stock = new Stock()
                            {
                                StoreId = bill.StoreId,
                                WareHouseId = wareHouseId,
                                ProductId = item.ProductId,
                                PositionCode = "A001_01_01",
                                //可用库存量
                                UsableQuantity = 0,
                                //现货库存量
                                CurrentQuantity = 0,
                                //预占库存量
                                OrderQuantity = 0,
                                //锁定库存量
                                LockQuantity = 0,
                                //创建人
                                CreaterId = bill.MakeUserId,
                                //创建时间
                                CreateTime = DateTime.Now,
                                UpdaterId = 0,
                                UpdateTime = DateTime.Now,
                                TimeStamp = DateTime.Now  //当前时间
                            };
                            //添加库
                            InsertStock(stock);
                        }
                    }
                }

                //Step 2. 库存出入记录 StockInOutRecords
                //同一个单据可能会涉及1个仓库（销售单、采购单...），也可能会涉及2个仓库（调拨单...）
                var stockInOutRecord = GetStockInOutRecordByBillCode(bill.StoreId, bill.BillType, bill.BillNumber, directionEnum, wareHouseId);
                old_StockInOutRecords = stockInOutRecord;

                //不存在 插入
                if (stockInOutRecord == null)
                {
                    stockInOutRecord = new StockInOutRecord()
                    {
                        StoreId = bill.StoreId,
                        BillCode = bill.BillNumber,
                        BillId = bill.Id,
                        BillType = bill.BillTypeId ?? 0,
                        Quantity = totalQuantity,
                        Direction = (int)directionEnum,
                        OutWareHouseId = (int)directionEnum == (int)DirectionEnum.Out ? wareHouseId : 0,
                        InWareHouseId = (int)directionEnum == (int)DirectionEnum.In ? wareHouseId : 0,
                        CreatedOnUtc = DateTime.Now
                    };

                    new_StockInOutRecords = stockInOutRecord;
                    InsertStockInOutRecord(stockInOutRecord);
                }
                //存在 更新
                else
                {
                    stockInOutRecord.Quantity = totalQuantity;
                    stockInOutRecord.Direction = (int)directionEnum;
                    stockInOutRecord.OutWareHouseId = (int)directionEnum == (int)DirectionEnum.Out ? wareHouseId : 0;
                    stockInOutRecord.InWareHouseId = (int)directionEnum == (int)DirectionEnum.In ? wareHouseId : 0;
                    new_StockInOutRecords = stockInOutRecord;
                    UpdateStockInOutRecord(stockInOutRecord);
                }

                //Step 3. 记录出入库明细记录
                var siods = new List<StockInOutDetails>();
                foreach (var item in bill.Items)
                {
                    var siod = new StockInOutDetails();
                    //销售
                    if (item is SaleItem sbi)
                    {
                        //先进先出
                        int? remainderQuantity = 0;
                        var fifoProduct = GetFIFOProduct(bill.StoreId, wareHouseId, sbi.ProductId);
                        if (fifoProduct != null)
                        {
                            fifoProduct.RemainderQuantity -= sbi?.Quantity ?? 0;
                            remainderQuantity = fifoProduct.RemainderQuantity;
                            //更新量剩余量
                            UpdateStockInOutDetails(fifoProduct);
                        }

                        //商品的当前库的库存
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, sbi.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.StockId = currentStock.Id;
                        siod.WareHouseId = wareHouseId;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = sbi?.ProductId ?? 0;
                        siod.ProductName = sbi?.ProductName;
                        siod.Quantity = sbi?.Quantity ?? 0;
                        siod.RemainderQuantity = remainderQuantity;
                        siod.Direction = fifoProduct == null ? 0 : 2;//销售出库
                        siod.ProductionBatch = sbi?.ProductionBatch;
                        siod.DealerStockId = sbi?.DealerStockId ?? 0;
                        siod.DateOfManufacture = sbi?.ManufactureDete ?? DateTime.Now;
                    }
                    //采购
                    else if (item is PurchaseItem pbi)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, pbi.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = pbi?.ProductId ?? 0;
                        siod.ProductName = pbi?.ProductName;
                        siod.Quantity = pbi?.Quantity ?? 0;
                        siod.RemainderQuantity = pbi?.Quantity ?? 0;
                        siod.Direction = 1;//采购入库
                        siod.ProductionBatch = pbi?.ProductionBatch;
                        siod.DealerStockId = pbi?.DealerStockId ?? 0;
                        siod.DateOfManufacture = pbi?.ManufactureDete ?? DateTime.Now;
                    }
                    //采购退货
                    else if (item is PurchaseReturnItem pri)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, pri.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = pri?.ProductId ?? 0;
                        siod.ProductName = "";
                        siod.Quantity = pri?.Quantity ?? 0;
                        siod.RemainderQuantity = pri?.Quantity ?? 0;
                        siod.Direction = 1;//采购入库
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = pri?.ManufactureDete ?? DateTime.Now;
                    }
                    //销订
                    else if (item is SaleReservationItem srb)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, srb.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = srb?.ProductId ?? 0;
                        siod.ProductName = srb?.ProductName;
                        siod.Quantity = srb?.Quantity ?? 0;
                        siod.RemainderQuantity = srb?.Quantity ?? 0;
                        siod.Direction = 2;
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = srb?.ManufactureDete ?? DateTime.Now;
                    }
                    //调拨
                    else if (item is AllocationItem ab)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, ab.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = ab?.ProductId ?? 0;
                        siod.ProductName = "";
                        siod.Quantity = ab?.Quantity ?? 0;
                        siod.RemainderQuantity = ab?.Quantity ?? 0;
                        siod.Direction = (int)directionEnum;
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = ab?.ManufactureDete ?? DateTime.Now;
                    }
                    //换货
                    else if (item is ExchangeItem eci)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, eci.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = eci?.ProductId ?? 0;
                        siod.ProductName = "";
                        siod.Quantity = eci?.Quantity ?? 0;
                        siod.RemainderQuantity = eci?.Quantity ?? 0;
                        siod.Direction = 2;
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = eci?.ManufactureDete ?? DateTime.Now;
                    }
                    //退货
                    else if (item is ReturnItem ri)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, ri.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = ri?.ProductId ?? 0;
                        siod.ProductName = "";
                        siod.Quantity = ri?.Quantity ?? 0;
                        siod.RemainderQuantity = ri?.Quantity ?? 0;
                        siod.Direction = 1;
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = ri?.ManufactureDete ?? DateTime.Now;
                    }
                    //退订
                    else if (item is ReturnReservationItem rri)
                    {
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, rri.ProductId);
                        siod.StoreId = bill.StoreId;
                        siod.WareHouseId = wareHouseId;
                        siod.StockId = currentStock.Id;
                        siod.StockInOutRecordId = new_StockInOutRecords.Id;
                        siod.ProductId = rri?.ProductId ?? 0;
                        siod.ProductName = "";
                        siod.Quantity = rri?.Quantity ?? 0;
                        siod.RemainderQuantity = rri?.Quantity ?? 0;
                        siod.Direction = 1;
                        siod.ProductionBatch = "";
                        siod.DealerStockId = 0;
                        siod.DateOfManufacture = rri?.ManufactureDete ?? DateTime.Now;
                    }

                    //借货
                    //TODO...

                    //还货
                    //TODO...

                    siod.InOutDate = DateTime.Now;
                    siods.Add(siod);
                }

                if (siods.Any())
                    InsertStockInOutDetails(siods);

                //Step 4. 库存流水 StockFlows
                //原来保存库存记录信息
                //List<StockFlow> stockFlowOlds = GetStockFlowByBillCode(directionEnum, storeId, billTypeEnum, billNumber);
                foreach (var item in stockProducts)
                {
                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        //商品的当前库的库存
                        var currentStock = GetCurrentStock(bill.StoreId, wareHouseId, item.ProductId);

                        int thisQuantity = item.Quantity;

                        //现货库存增量
                        int currentIncrement = 0;
                        currentIncrement = (stockQuantityType == StockQuantityType.CurrentQuantity ? thisQuantity : 0);

                        //预占库存量增量
                        int orderIncrement = 0;
                        orderIncrement = (stockQuantityType == StockQuantityType.OrderQuantity ? thisQuantity : 0);

                        //锁定库存量增量
                        int lockIncrement = 0;
                        lockIncrement = (stockQuantityType == StockQuantityType.LockQuantity ? thisQuantity : 0);


                        //可用库存量增量 = 现货库存量增量-预占库存量增量-锁定库存量增量
                        int usableIncrement = currentIncrement - orderIncrement - lockIncrement;

                        //现货库存量
                        int currentQuantityBefor = 0;
                        int currentQuantityAfter = 0;

                        //预占库存量
                        int orderQuantityBefor = 0;
                        int orderQuantityAfter = 0;

                        //锁定库存量
                        int lockQuantityBefor = 0;
                        int lockQuantityAfter = 0;

                        //可用库存量
                        int usableQuantityBefor = 0;
                        int usableQuantityAfter = 0;

                        if (currentStock != null)
                        {
                            currentQuantityBefor = currentStock.CurrentQuantity ?? 0;
                            currentQuantityAfter = currentStock.CurrentQuantity ?? 0;

                            orderQuantityBefor = currentStock.OrderQuantity ?? 0;
                            orderQuantityAfter = currentStock.OrderQuantity ?? 0;

                            lockQuantityBefor = currentStock.LockQuantity ?? 0;
                            lockQuantityAfter = currentStock.LockQuantity ?? 0;

                            usableQuantityBefor = currentStock.UsableQuantity ?? 0;
                            usableQuantityAfter = currentStock.UsableQuantity ?? 0;
                        }

                        //记录流水
                        var stockFlow = new StockFlow()
                        {
                            StockId = currentStock.Id,
                            StoreId = bill.StoreId,
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode ?? "",
                            ProductName = item.ProductName,
                            UnitId = item.UnitId,
                            SmallUnitId = item.SmallUnitId,
                            BigUnitId = item.BigUnitId,
                            Quantity = thisQuantity,

                            //现货库存量修改前的值 = 上次现货库存量修改后的值
                            CurrentQuantityBefor = currentQuantityAfter,
                            //现货库存量修改后的值 = 上次现货库存量修改后的值 + 现货库存量增量
                            CurrentQuantityAfter = currentQuantityAfter + currentIncrement,
                            //现货库存量改变值 （增加为正，减少为负） = (上次现货库存量修改后的值 + 现货库存量增量 - 上次现货库存量修改后的值) = 现货库存量增量
                            CurrentQuantityChange = ((currentQuantityAfter + currentIncrement) - currentQuantityAfter),

                            //预占库存量修改前的值 = 上次预占库存量修改后的值
                            OrderQuantityBefor = orderQuantityAfter,
                            //预占库存量修改后的值 = 上次预占库存量修改后的值 + 预占库存量增量
                            OrderQuantityAfter = orderQuantityAfter + orderIncrement,
                            //预占库存量改变值 （增加为正，减少为负） = (上次预占库存量修改后的值 + 预占库存量增量 - 上次预占库存量修改后的值) = 预占库存量增量
                            OrderQuantityChange = ((orderQuantityAfter + orderIncrement) - orderQuantityAfter),

                            //锁定库存量修改前的值 = 上次锁定库存量修改后的值
                            LockQuantityBefor = lockQuantityAfter,
                            //锁定库存量修改后的值 = 上次锁定库存量修改后的值 + 锁定库存量增量
                            LockQuantityAfter = lockQuantityAfter + lockIncrement,
                            //锁定库存量改变值 （增加为正，减少为负） = (上次锁定库存量修改后的值 + 锁定库存量增量 - 上次锁定库存量修改后的值) = 锁定库存量增量
                            LockQuantityChange = ((lockQuantityAfter + lockIncrement) - lockQuantityAfter),

                            //可用库存量修改前的值
                            UsableQuantityBefor = currentQuantityAfter - orderQuantityAfter - lockQuantityAfter,
                            UsableQuantityAfter = (currentQuantityAfter - orderQuantityAfter - lockQuantityAfter) + usableIncrement,
                            UsableQuantityChange = usableIncrement,

                            ChangeType = (int)stockFlowChangeTypeEnum,

                            //创建人
                            CreaterId = bill.MakeUserId,
                            //创建时间
                            CreateTime = DateTime.Now,
                            TimeStamp = DateTime.Now
                        };

                        new_StockFlows.Add(stockFlow);

                        //StockFlows
                        InsertStockFlow(stockFlow);

                        //出入库记录和流水关系映射
                        var stockInOutRecordStockFlow = new StockInOutRecordStockFlow()
                        {
                            StoreId = bill.StoreId,
                            StockFlowId = stockFlow.Id,
                            StockInOutRecordId = stockInOutRecord.Id
                        };

                        new_SRSFs.Add(stockInOutRecordStockFlow);
                        InsertStockInOutRecordStockFlow(stockInOutRecordStockFlow);

                    }
                }

                //Step 5. 更新库存(增/减) Stocks
                foreach (var item in stockProducts)
                {
                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    if (product != null)
                    {

                        int thisQuantity = item.Quantity;

                        //商品的当前库的库存
                        //***下面的sql语句进行的库存修改，只能根据sql语句查询进行跟踪
                        var sqlString = $"select Id,storeId,WareHouseId,ProductId,PositionCode,UsableQuantity,CurrentQuantity,OrderQuantity,LockQuantity,CreaterId,CreateTime,UpdaterId,UpdateTime,TimeStamp,Version from Stocks where StoreId='{bill.StoreId}' and WareHouseId='{wareHouseId}' and ProductId='{item.ProductId}'";
                        var oldStockQuery = StocksRepository.QueryFromSql<StockQuery>(sqlString).FirstOrDefault();

                        if (oldStockQuery != null)
                        {
                            var oldStock = new Stock()
                            {
                                Id = oldStockQuery.Id,
                                StoreId = oldStockQuery.StoreId,
                                WareHouseId = oldStockQuery.WareHouseId,
                                ProductId = oldStockQuery.ProductId,
                                PositionCode = oldStockQuery.PositionCode,
                                UsableQuantity = oldStockQuery.UsableQuantity,
                                CurrentQuantity = oldStockQuery.CurrentQuantity,
                                OrderQuantity = oldStockQuery.OrderQuantity,
                                LockQuantity = oldStockQuery.LockQuantity,
                                CreaterId = oldStockQuery.CreaterId,
                                CreateTime = oldStockQuery.CreateTime,
                                UpdaterId = oldStockQuery.UpdaterId,
                                UpdateTime = oldStockQuery.UpdateTime,
                                TimeStamp = oldStockQuery.TimeStamp,
                                Version = oldStockQuery.Version
                            };

                            var newStock = DeepClon<Stock, Stock>.Trans(oldStock);

                            int? tempCurrentQuantity = 0;
                            int? tempOrderQuantity = 0;
                            int? tempLockQuantity = 0;

                            //现货
                            if (oldStock.CurrentQuantity == 0)
                                tempCurrentQuantity = oldStock.CurrentQuantity + (stockQuantityType == StockQuantityType.CurrentQuantity ? Math.Abs(thisQuantity) : 0);
                            else
                                tempCurrentQuantity = oldStock.CurrentQuantity + (stockQuantityType == StockQuantityType.CurrentQuantity ? thisQuantity : 0);

                            //预占
                            if (oldStock.OrderQuantity == 0)
                                tempOrderQuantity = oldStock.OrderQuantity + (stockQuantityType == StockQuantityType.OrderQuantity ? Math.Abs(thisQuantity) : 0);
                            else
                                tempOrderQuantity = oldStock.OrderQuantity + (stockQuantityType == StockQuantityType.OrderQuantity ? thisQuantity : 0);

                            //锁定
                            if (oldStock.LockQuantity == 0)
                                tempLockQuantity = oldStock.LockQuantity + (stockQuantityType == StockQuantityType.LockQuantity ? Math.Abs(thisQuantity) : 0);
                            else
                                tempLockQuantity = oldStock.LockQuantity + (stockQuantityType == StockQuantityType.LockQuantity ? thisQuantity : 0);

                            //销售单（影响预占时）
                            if (bill.BillType == BillTypeEnum.SaleBill)
                            {
                                //现货不够时增加预占
                                if ((stockQuantityType == StockQuantityType.OrderQuantity) && Math.Abs(thisQuantity) >= Math.Abs(tempCurrentQuantity ?? 0) && Math.Abs(tempCurrentQuantity ?? 0) > 0)
                                {
                                    tempOrderQuantity = Math.Abs(thisQuantity) - Math.Abs(tempCurrentQuantity ?? 0);
                                }
                            }
                            else if (bill.BillType != BillTypeEnum.ReturnBill && bill.BillType != BillTypeEnum.PurchaseBill && bill.BillType != BillTypeEnum.AllocationBill)
                            {
                                //现货不够时增加预占
                                if ((stockQuantityType == StockQuantityType.CurrentQuantity) && Math.Abs(thisQuantity) >= Math.Abs(tempCurrentQuantity ?? 0) && Math.Abs(tempCurrentQuantity ?? 0) > 0)
                                {
                                    tempOrderQuantity = Math.Abs(thisQuantity) - Math.Abs(tempCurrentQuantity ?? 0);
                                }
                            }

                            //增加现货时，处理预占量
                            //if (stockQuantityType == StockQuantityType.CurrentQuantity && thisQuantity > 0 && Math.Abs(tempOrderQuantity ?? 0) > 0)
                            //{
                            //    tempOrderQuantity = Math.Abs(Math.Abs(thisQuantity) - Math.Abs(tempOrderQuantity ?? 0));
                            //}

                            var tempUsableQuantity = tempCurrentQuantity - tempOrderQuantity - tempLockQuantity;

                            //现货库存量
                            newStock.CurrentQuantity = tempCurrentQuantity;
                            //预占库存量
                            newStock.OrderQuantity = tempOrderQuantity;
                            //锁定库存量
                            newStock.LockQuantity = tempLockQuantity;
                            //可用库存量
                            newStock.UsableQuantity = tempUsableQuantity;

                            newStock.UpdaterId = bill.MakeUserId;
                            newStock.UpdateTime = DateTime.Now;
                            newStock.TimeStamp = DateTime.Now;  //当前时间
                            newStock.StoreId = bill.StoreId;

                            old_Stocks.Add(oldStock);
                            new_Stocks.Add(newStock);

                            //更新库
                            var result = AdjustmentStock(newStock, oldStock, bill.StoreId, bill.MakeUserId, wareHouseId, item.ProductId);

                            //记录失败项目
                            if (!result)
                            {
                                faileds.Add(item);
                            }

                        }
                    }
                }


                historyDatas = new Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>>(faileds, new Tuple<StockInOutRecord, StockInOutRecord>(old_StockInOutRecords, new_StockInOutRecords), new Tuple<List<StockFlow>, List<StockFlow>>(old_StockFlows, new_StockFlows), new Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>(old_SRSFs, new_SRSFs), new Tuple<List<Stock>, List<Stock>>(old_Stocks, new_Stocks));

                return historyDatas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 回滚库存更改
        /// </summary>
        /// <param name="tuple"></param>
        public void RoolBackChanged(Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> tuple)
        {
            try
            {
                //更改前
                var old_StockInOutRecords = tuple?.Item2?.Item1 ?? new StockInOutRecord();
                var old_StockFlows = tuple?.Item3?.Item1 ?? new List<StockFlow>();
                var old_SRSFs = tuple?.Item4?.Item1 ?? new List<StockInOutRecordStockFlow>();
                var old_Stocks = tuple?.Item5?.Item1 ?? new List<Stock>();
                //更改后
                var new_StockInOutRecords = tuple?.Item2?.Item2 ?? new StockInOutRecord();
                var new_StockFlows = tuple?.Item3?.Item2 ?? new List<StockFlow>();
                var new_SRSFs = tuple?.Item4?.Item2 ?? new List<StockInOutRecordStockFlow>();
                var new_Stocks = tuple?.Item5?.Item2 ?? new List<Stock>();

                if (old_StockInOutRecords == null && new_StockInOutRecords != null)
                {
                    //删除新加
                    DeleteStockInOutRecord(new_StockInOutRecords);
                }
                else if (old_StockInOutRecords != null && new_StockInOutRecords != null)
                {
                    //更新为旧
                    UpdateStockInOutRecord(old_StockInOutRecords);
                }

                if (new_StockFlows.Any())
                {
                    //删除新流水
                    DeleteStockFlows(new_StockFlows);
                }

                if (new_SRSFs.Any())
                {
                    //删除新流水映射
                    DeleteStockInOutRecordStockFlows(new_SRSFs);
                }

                if (new_Stocks.Any() && !old_Stocks.Any())
                {
                    //更新成旧库存
                    UpdateStocks(old_Stocks);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"回滚库存失败:{ex.Message}");
            }

        }

        #endregion

        #endregion



        #endregion

        #region 库存流水(历史)记录


        public virtual IPagedList<StockFlow> GetStockFlowsByStockId(int stockId, int? userId, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (stockId == 0)
            {
                return new PagedList<StockFlow>(new List<StockFlow>(), pageIndex, pageSize);
            }

            var key = DCMSDefaults.STOCKFLOW_ALL_KEY.FillCacheKey(storeId, stockId, pageIndex, pageSize, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pc in StockFlowsRepository.Table
                            where pc.StockId == stockId
                            orderby pc.Id
                            select pc;
                //var productStocks = new PagedList<StockFlow>(query.ToList(), pageIndex, pageSize);
                //return productStocks;
                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<StockFlow>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public virtual StockFlow GetStockFlowById(int? store, int stockFlowId)
        {
            if (stockFlowId == 0)
            {
                return null;
            }

            return StockFlowsRepository.ToCachedGetById(stockFlowId);
        }

        public virtual List<StockFlow> GetStockFlowByBillCode(DirectionEnum directionEnum, int storeId, BillTypeEnum billTypeEnum, string billCode)
        {
            if (storeId == 0 || billTypeEnum == 0 || string.IsNullOrEmpty(billCode))
            {
                return null;
            }

            var query = from stockInOutRecordStockFlow in StockInOutRecordsStockFlowsMappingRepository.Table
                        join stockInOutRecord in StockInOutRecordsRepository.Table on stockInOutRecordStockFlow.StockInOutRecordId equals stockInOutRecord.Id
                        join stockFlow in StockFlowsRepository.Table on stockInOutRecordStockFlow.StockFlowId equals stockFlow.Id
                        where stockInOutRecord.Direction == (int)(directionEnum)       //出入库类型
                         && stockInOutRecord.StoreId == storeId                        //经销商ID
                         && stockInOutRecord.BillType == (int)billTypeEnum             //单据类型
                         && stockInOutRecord.BillCode == billCode                      //单据编号
                        select stockFlow;
            return query.ToList();
        }

        public virtual void InsertStockFlow(StockFlow stockFlow)
        {
            if (stockFlow == null)
            {
                throw new ArgumentNullException("stockFlow");
            }

            var uow = StockFlowsRepository.UnitOfWork;
            StockFlowsRepository.Insert(stockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(stockFlow);

        }

        public virtual void UpdateStockFlow(StockFlow stockFlow)
        {
            if (stockFlow == null)
            {
                throw new ArgumentNullException("stockFlow");
            }

            var uow = StockFlowsRepository.UnitOfWork;
            StockFlowsRepository.Update(stockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(stockFlow);
        }

        public virtual void DeleteStockFlow(StockFlow stockFlow)
        {
            if (stockFlow == null)
            {
                throw new ArgumentNullException("stockFlow");
            }

            var uow = StockFlowsRepository.UnitOfWork;
            StockFlowsRepository.Delete(stockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(stockFlow);
        }

        public virtual void DeleteStockFlows(List<StockFlow> stockFlows)
        {
            if (stockFlows == null)
            {
                throw new ArgumentNullException("stockFlows");
            }

            var uow = StockFlowsRepository.UnitOfWork;
            StockFlowsRepository.Delete(stockFlows);
            uow.SaveChanges();

            //通知
            stockFlows.ForEach(s => { _eventPublisher.EntityDeleted(s); });

        }

        /// <summary>
        ///获取库存商品最近次流水记录
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="stock"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual StockFlow GetLastTimeStockFlow(int storeId, int stockId, int productId)
        {
            //该方法为正确取值
            //var query = from a in _stockFlowsRepository.Table
            //            where (from b in _stockFlowsRepository.Table
            //                   where b.ProductId == a.ProductId
            //                   select new
            //                   {
            //                       ts = b.TimeStamp
            //                   }).Max(q => q.ts) == a.TimeStamp && a.StoreId == storeId && a.StockId == stockId && a.ProductId == productId
            //            select a;

            var query = from a in StockFlowsRepository.Table
                        where (a.StoreId == storeId && a.StockId == stockId && a.ProductId == productId)
                        select a;
            query = query.OrderByDescending(a => a.Id);

            return query.FirstOrDefault();
        }


        #endregion

        #region 出入库记录

        public virtual IPagedList<StockInOutRecord> GetAllStockInOutRecords(int? store, string billCode, int? billType, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = StockInOutRecordsRepository.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }
            else
            {
                return null;
            }

            if (!string.IsNullOrEmpty(billCode))
            {
                query = query.Where(c => c.BillCode == billCode);
            }

            if (billType.HasValue && billType.Value != 0)
            {
                query = query.Where(c => c.BillType == billType);
            }

            if (start.HasValue)
            {
                query = query.Where(o => start.Value <= o.CreatedOnUtc);
            }

            if (end.HasValue)
            {
                query = query.Where(o => end.Value >= o.CreatedOnUtc);
            }

            query = query.OrderByDescending(c => c.Id);

            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<StockInOutRecord>(plists, pageIndex, pageSize, totalCount);
        }

        public virtual IList<StockInOutRecord> GetAllStockInOutRecords()
        {
            var query = from c in StockInOutRecordsRepository.Table
                        orderby c.Id
                        select c;

            var categories = query.ToList();
            return categories;
        }

        public virtual StockInOutRecord GetStockInOutRecordById(int? store, int stockInOutRecordId)
        {
            if (stockInOutRecordId == 0)
            {
                return null;
            }

            return StockInOutRecordsRepository.ToCachedGetById(stockInOutRecordId);
        }

        public virtual StockInOutRecord GetStockInOutRecordByBillCode(int storeId, BillTypeEnum billTypeEnum, string billCode)
        {
            if (storeId == 0 || billTypeEnum == 0 || string.IsNullOrEmpty(billCode))
            {
                return null;
            }

            var query = from c in StockInOutRecordsRepository.Table
                        where c.StoreId == storeId && c.BillType == (int)billTypeEnum && c.BillCode == billCode
                        select c;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// ***测试单据是否有预占***
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeEnum"></param>
        /// <param name="billCode"></param>
        /// <returns></returns>
        public virtual bool CheckOrderQuantity(int storeId, BillTypeEnum billTypeEnum, string billCode, int wareHouseId)
        {
            if (storeId == 0 || billTypeEnum == 0 || string.IsNullOrEmpty(billCode) || wareHouseId == 0)
            {
                return false;
            }

            var sior = from c in StockInOutRecordsRepository.TableNoTracking
                        where c.StoreId == storeId && c.BillType == (int)billTypeEnum && c.BillCode == billCode
                        && (c.OutWareHouseId == wareHouseId || c.InWareHouseId == wareHouseId)
                        select c;
            return sior.Count() > 0;
        }


        public virtual bool CheckOrderQuantity(int storeId, BillTypeEnum billTypeEnum, string billCode, int wareHouseId, int productId = 0)
        {
            if (storeId == 0 || billTypeEnum == 0 || string.IsNullOrEmpty(billCode) || wareHouseId == 0)
            {
                return false;
            }

            var sior = from c in StockInOutRecordsRepository.TableNoTracking
                       where c.StoreId == storeId && c.BillType == (int)billTypeEnum && c.BillCode == billCode
                       && (c.OutWareHouseId == wareHouseId || c.InWareHouseId == wareHouseId)
                       select c;
            var count = sior.Count() > 0;

            var sr = from s in StocksRepository.TableNoTracking
                     where s.StoreId == storeId && s.WareHouseId == wareHouseId && s.ProductId == productId
                     select s.OrderQuantity;

            var stock = sr.FirstOrDefault();
            if ((stock ?? 0) > 0 && count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual StockInOutRecord GetStockInOutRecordByBillCode(int storeId, BillTypeEnum billTypeEnum, string billCode, DirectionEnum directionEnum, int wareHouseId)
        {
            if (storeId == 0 || billTypeEnum == 0 || string.IsNullOrEmpty(billCode) || wareHouseId == 0)
            {
                return null;
            }

            var query = from c in StockInOutRecordsRepository.Table
                        where c.StoreId == storeId && c.BillType == (int)billTypeEnum && c.BillCode == billCode
                        select c;

            if (directionEnum == DirectionEnum.Out)
            {
                query = query.Where(a => a.OutWareHouseId == wareHouseId);
            }

            if (directionEnum == DirectionEnum.In)
            {
                query = query.Where(a => a.InWareHouseId == wareHouseId);
            }

            return query.FirstOrDefault();
        }


        public virtual void InsertStockInOutRecord(StockInOutRecord stockInOutRecord)
        {
            if (stockInOutRecord == null)
            {
                throw new ArgumentNullException("stockInOutRecord");
            }

            var uow = StockInOutRecordsRepository.UnitOfWork;
            StockInOutRecordsRepository.Insert(stockInOutRecord);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(stockInOutRecord);
        }


        public virtual void InsertStockInOutDetails(StockInOutDetails stockInOutDetail)
        {
            if (stockInOutDetail == null)
                throw new ArgumentNullException("stockInOutDetail");

            var uow = StockInOutDetailsRepository.UnitOfWork;

            StockInOutDetailsRepository.Insert(stockInOutDetail);

            uow.SaveChanges();
        }

        public virtual void InsertStockInOutDetails(List<StockInOutDetails> stockInOutDetails)
        {
            if (stockInOutDetails == null)
                throw new ArgumentNullException("stockInOutDetails");

            var uow = StockInOutDetailsRepository.UnitOfWork;

            StockInOutDetailsRepository.Insert(stockInOutDetails);

            uow.SaveChanges();
        }

        public virtual void UpdateStockInOutRecord(StockInOutRecord stockInOutRecord)
        {
            if (stockInOutRecord == null)
            {
                throw new ArgumentNullException("stockInOutRecord");
            }

            var uow = StockInOutRecordsRepository.UnitOfWork;
            StockInOutRecordsRepository.Update(stockInOutRecord);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(stockInOutRecord);
        }

        public virtual void DeleteStockInOutRecord(StockInOutRecord stockInOutRecord)
        {
            if (stockInOutRecord == null)
            {
                throw new ArgumentNullException("stockInOutRecord");
            }

            var uow = StockInOutRecordsRepository.UnitOfWork;
            StockInOutRecordsRepository.Delete(stockInOutRecord);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(stockInOutRecord);
        }

        //获取商品的采购入库记录
        public IQueryable<StockInOutRecord> getStockInOutRecordsByProductAndStock(int store, int stockId, int productId)
        {
            #region delete sql语句
            //var sqlString = "select d.* from [DCMS].dbo.Stocks as a " +
            //                     "inner join [DCMS].dbo.StockFlows as b " +
            //                     "on a.ProductId = b.ProductId and a.Id=b.StockId " +
            //                     "inner join [DCMS].dbo.StockInOutRecords_StockFlows_Mapping as c " +
            //                     "on b.Id = c.StockFlowId " +
            //                     "inner join [DCMS].dbo.StockInOutRecords as d " +
            //                     "on c.StockInOutRecordId = d.Id " +
            //                     "where a.StoreId=" + store + " and a.ProductId = " + productId + " and a.WareHouseId=" + stockId + " and d.BillType = 22 and a.UsableQuantity > 0 " +
            //                     "order by d.CreatedOnUtc";

            //var query = _stockInOutRecordsRepository.QueryFromSql<StockInOutRecord>(sqlString);
            //return query.AsQueryable();
            #endregion

            var query = from a in StocksRepository.Table
                        join b in StockFlowsRepository.Table on new { a.ProductId, StockId = a.Id } equals new { b.ProductId, b.StockId }
                        join c in StockInOutRecordsStockFlowsMappingRepository.Table on b.Id equals c.StockFlowId
                        join d in StockInOutRecordsRepository.Table on c.StockInOutRecordId equals d.Id
                        where a.StoreId == store
                        && a.ProductId == productId
                        && a.WareHouseId == stockId
                        && d.BillType == (int)BillTypeEnum.PurchaseBill
                        && a.UsableQuantity > 0
                        orderby d.CreatedOnUtc
                        select d;
            return query.ToList().AsQueryable();

        }
        #endregion

        #region 出入库流水 _stockInOutRecordsStockFlowsMappingRepository


        public virtual IList<StockInOutRecordStockFlow> GetStockInOutRecordStockFlowByStockInOutRecordId(int stockInOutRecordId, int? userId, int? storeId)
        {
            if (stockInOutRecordId == 0)
            {
                return new List<StockInOutRecordStockFlow>();
            }

            var key = DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_BY_RECORED_ID_KEY.FillCacheKey(storeId, stockInOutRecordId, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in StockInOutRecordsStockFlowsMappingRepository.Table
                            join c in StockInOutRecordsRepository.Table on pc.StockInOutRecordId equals c.Id
                            where pc.StockInOutRecordId == stockInOutRecordId
                            orderby pc.Id
                            select pc;

                var result = query.ToList();
                return result;
            });
        }

        public virtual StockInOutRecordStockFlow GetStockInOutRecordStockFlowById(int stockInOutRecordStockFlowId)
        {
            if (stockInOutRecordStockFlowId == 0)
            {
                return null;
            }

            return StockInOutRecordsStockFlowsMappingRepository.ToCachedGetById(stockInOutRecordStockFlowId);
        }

        public virtual StockInOutRecordStockFlow GetStockInOutRecordStockFlowByStockFlowId(int stockFlowId)
        {
            if (stockFlowId == 0)
            {
                return null;
            }

            var query = StockInOutRecordsStockFlowsMappingRepository.Table;
            return query.FirstOrDefault(a => a.StockFlowId == stockFlowId);
        }


        public virtual void InsertStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow)
        {
            if (stockInOutRecordStockFlow == null)
            {
                throw new ArgumentNullException("stockInOutRecordStockFlow");
            }

            var uow = StockInOutRecordsStockFlowsMappingRepository.UnitOfWork;
            StockInOutRecordsStockFlowsMappingRepository.Insert(stockInOutRecordStockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(stockInOutRecordStockFlow);
        }

        public virtual void UpdateStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow)
        {
            if (stockInOutRecordStockFlow == null)
            {
                throw new ArgumentNullException("stockInOutRecordStockFlow");
            }

            var uow = StockInOutRecordsStockFlowsMappingRepository.UnitOfWork;
            StockInOutRecordsStockFlowsMappingRepository.Update(stockInOutRecordStockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(stockInOutRecordStockFlow);
        }

        public virtual void DeleteStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow)
        {
            if (stockInOutRecordStockFlow == null)
            {
                throw new ArgumentNullException("stockInOutRecordStockFlow");
            }

            var uow = StockInOutRecordsStockFlowsMappingRepository.UnitOfWork;
            StockInOutRecordsStockFlowsMappingRepository.Delete(stockInOutRecordStockFlow);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(stockInOutRecordStockFlow);
        }

        public virtual void DeleteStockInOutRecordStockFlows(List<StockInOutRecordStockFlow> stockInOutRecordStockFlows)
        {
            if (stockInOutRecordStockFlows == null)
            {
                throw new ArgumentNullException("stockInOutRecordStockFlows");
            }

            var uow = StockInOutRecordsStockFlowsMappingRepository.UnitOfWork;
            StockInOutRecordsStockFlowsMappingRepository.Delete(stockInOutRecordStockFlows);
            uow.SaveChanges();

            //通知
            stockInOutRecordStockFlows.ForEach(s => { _eventPublisher.EntityDeleted(s); });

        }



        #endregion

        /// <summary>
        /// 单例获取库存量（现货）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        public int GetProductCurrentQuantity(int storeId, int productId, int wareHouseId)
        {
            var query = StocksRepository.TableNoTracking;

            if (wareHouseId == 0)
            {
                return query.Where(s => s.ProductId == productId).Sum(c => c.CurrentQuantity) ?? 0;
                //return query.Sum(c => c.CurrentQuantity) ?? 0;
            }
            else
            {
                return query.Where(c => c.StoreId == storeId && c.ProductId == productId && c.WareHouseId == wareHouseId).Sum(s => s.CurrentQuantity) ?? 0;
            }
        }

        /// <summary>
        /// 单例获取库存量（可用）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        public int GetProductUsableQuantity(int storeId, int productId, int wareHouseId)
        {
            var query = StocksRepository.Table;

            if (wareHouseId == 0)
            {
                return query.Where(s => s.ProductId == productId).Sum(c => c.UsableQuantity) ?? 0;
                //return query.Sum(c => c.UsableQuantity) ?? 0;
            }
            else
            {
                return query.Where(c => c.StoreId == storeId && c.ProductId == productId && c.WareHouseId == wareHouseId).Sum(s => s.UsableQuantity) ?? 0;
            }
        }

        /// <summary>
        /// 获取指定期间商品出入库存量汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypes">12, 14, 22, 24, 32, 33, 34, 37, 38</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<StockQtySummery> GetStockQTYSummery(int storeId, int[] billTypes, DateTime start, DateTime end)
        {
            string sqlString = $"";
            sqlString += $" select *,(select CAST(alls.Price * alls.Quantity AS DECIMAL(8, 4))) as Cost from(";
            sqlString += $" select";
            sqlString += $" sf.ProductId,";
            sqlString += $" sior.Direction,";
            sqlString += $" sf.UnitId,";
            sqlString += $" group_concat(sior.BillId separator ',') as 'Bills',";
            sqlString += $" group_concat(sior.BillCode separator ',') as 'BillNumbers',";
            sqlString += $" IFNULL(p.Name, '') as ProductName,";
            sqlString += $" IFNULL(sao.Name, '') as UnitName,";
            sqlString += $" IFNULL(p.ProductCode, '') as ProductCode,";
            sqlString += $" sum(IFNULL((CASE";

            sqlString += $" WHEN sior.BillType = 12 THEN #销售单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from SaleItems as si where si.SaleBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 14 THEN #退货单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from ReturnItems as si where si.ReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 22 THEN #采购单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from PurchaseItems as si where si.PurchaseBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 24 THEN #采购退货";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from PurchaseReturnItems as si where si.PurchaseReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 32 THEN #盘点盈亏单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from InventoryProfitLossItems as si where si.InventoryProfitLossBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 33 THEN #成本调价单";
            sqlString += $" (select IFNULL(si.AdjustedPrice, 0) from CostAdjustmentItems as si where si.CostAdjustmentBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 34 THEN #报损单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from ScrapProductItems as si where si.ScrapProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 37 THEN #组合单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from CombinationProductItems as si where si.CombinationProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 38 THEN #拆分单";
            sqlString += $" (select IFNULL(si.CostPrice, 0) from SplitProductItems as si where si.SplitProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $"  END), 0)) as Price,";
            sqlString += $" sum(IFNULL((CASE";

            sqlString += $" WHEN sior.BillType = 12 THEN #销售单";
            sqlString += $"    (select IFNULL(si.Quantity, 0) from SaleItems as si where si.SaleBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 14 THEN #退货单";
            sqlString += $"    (select IFNULL(si.Quantity, 0) from ReturnItems as si where si.ReturnBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 22 THEN #采购单";
            sqlString += $"   (select IFNULL(si.Quantity, 0) from PurchaseItems as si where si.PurchaseBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 24 THEN #采购退货";
            sqlString += $"    (select IFNULL(si.Quantity, 0) from PurchaseReturnItems as si where si.PurchaseReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 32 THEN #盘点盈亏单";
            sqlString += $"   (select IFNULL(si.Quantity, 0) from InventoryProfitLossItems as si where si.InventoryProfitLossBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 33 THEN #成本调价单";
            sqlString += $" (select count(*) from CostAdjustmentItems as si where si.CostAdjustmentBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 34 THEN #报损单";
            sqlString += $"   (select IFNULL(si.Quantity, 0) from ScrapProductItems as si where si.ScrapProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 37 THEN #组合单";
            sqlString += $"   (select IFNULL(si.Quantity, 0) from CombinationProductItems as si where si.CombinationProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";

            sqlString += $" WHEN  sior.BillType = 38 THEN #拆分单";
            sqlString += $"   (select IFNULL(si.Quantity, 0) from SplitProductItems as si where si.SplitProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId or si.UnitId = p.BigUnitId))";
            sqlString += $" END), 0)) as Quantity,";
            sqlString += $" sior.CreatedOnUtc";

            sqlString += $" from StockInOutRecords as sior";
            sqlString += $" left join StockInOutRecords_StockFlows_Mapping as ssm on sior.Id = ssm.StockInOutRecordId";
            sqlString += $" left join StockFlows as sf on ssm.StockFlowId = sf.id";
            sqlString += $" left join Products as p on sf.ProductId = p.Id";
            sqlString += $" left join SpecificationAttributeOptions as sao on sf.UnitId = sao.Id";
            sqlString += $" where sior.StoreId = {storeId} and sior.BillType in ({string.Join(",", billTypes)}) and(sior.CreatedOnUtc >= '{start:yyyy-MM-dd 00:00:00}' and sior.CreatedOnUtc <= '{end:yyyy-MM-dd 23:59:59}')";
            sqlString += $" group by sf.ProductId,sior.Direction,sf.UnitId) as alls";

            var lists = StocksRepository.QueryFromSql<StockQtySummery>(sqlString).ToList();
            return lists.ToList();

        }

        /// <summary>
        /// 获取指定期间商品出入库存量
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypes">12, 14, 22, 24, 32, 33, 34, 37, 38</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<StockQty> GetStockQTY(int storeId, int[] billTypes, DateTime start, DateTime end)
        {
            string sqlString = @"select 
                                sf.ProductId ,
                                sior.Direction,
                                sf.UnitId,
                                p.SmallUnitId,
                                p.BigUnitId,
                                sior.BillId,
                                sior.BillType,
                                sior.BillCode,
                                IFNULL(p.Name,'') as ProductName,
                                IFNULL(sao.Name,'') as UnitName,
                                IFNULL(ssao.Name,'') as SmallUnitName,
                                IFNULL(bsao.Name,'') as BigUnitName,
                                IFNULL(p.ProductCode,'') as ProductCode,
                                IFNULL((CASE 
		                                WHEN sior.BillType=12 THEN #销售单
			                                (select IFNULL(si.CostPrice,0) from SaleItems as si where si.SaleBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=14 THEN #退货单
			                                (select IFNULL(si.CostPrice,0) from ReturnItems as si where si.ReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=22 THEN #采购单
			                                (select IFNULL(si.CostPrice,0) from PurchaseItems as si where si.PurchaseBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=24 THEN #采购退货
			                                (select IFNULL(si.CostPrice,0) from PurchaseReturnItems as si where si.PurchaseReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=32 THEN #盘点盈亏单
			                                (select IFNULL(si.CostPrice,0) from InventoryProfitLossItems as si where si.InventoryProfitLossBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
	                                    WHEN  sior.BillType=33 THEN #成本调价单
                                            (select IFNULL(si.AdjustedPrice, 0) from CostAdjustmentItems as si where si.CostAdjustmentBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=34 THEN #报损单
			                                (select IFNULL(si.CostPrice,0) from ScrapProductItems as si where si.ScrapProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=37 THEN #组合单
			                                (select IFNULL(si.CostPrice,0) from CombinationProductItems as si where si.CombinationProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=38 THEN #拆分单
			                                (select IFNULL(si.CostPrice,0) from SplitProductItems as si where si.SplitProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
                                END ),0) as Price,
                                p.StrokeQuantity,
                                p.BigQuantity,
                                IFNULL((CASE 
		                                WHEN sior.BillType=12 THEN #销售单
			                                (select IFNULL(si.Quantity,0) from SaleItems as si where si.SaleBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=14 THEN #退货单
			                                (select IFNULL(si.Quantity,0) from ReturnItems as si where si.ReturnBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=22 THEN #采购单
			                                (select IFNULL(si.Quantity,0) from PurchaseItems as si where si.PurchaseBillId = sior.BillId and si.ProductId = sf.ProductId  and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=24 THEN #采购退货
			                                (select IFNULL(si.Quantity,0) from PurchaseReturnItems as si where si.PurchaseReturnBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=32 THEN #盘点盈亏单
			                                (select IFNULL(si.Quantity,0) from InventoryProfitLossItems as si where si.InventoryProfitLossBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
	                                    WHEN  sior.BillType=33 THEN #成本调价单
                                            (select count(*) from CostAdjustmentItems as si where si.CostAdjustmentBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=34 THEN #报损单
			                                (select IFNULL(si.Quantity,0) from ScrapProductItems as si where si.ScrapProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=37 THEN #组合单
			                                (select IFNULL(si.Quantity,0) from CombinationProductItems as si where si.CombinationProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId ))
		                                WHEN  sior.BillType=38 THEN #拆分单
			                                (select IFNULL(si.Quantity,0) from SplitProductItems as si where si.SplitProductBillId = sior.BillId and si.ProductId = sf.ProductId and (si.UnitId = sf.UnitId))
                                 END ),0) as Quantity,
                                 sior.CreatedOnUtc
                                from StockInOutRecords as sior 
                                left join StockInOutRecords_StockFlows_Mapping as ssm on sior.Id = ssm.StockInOutRecordId
                                left join StockFlows as sf on  ssm.StockFlowId = sf.id
                                left join Products as p on sf.ProductId = p.Id
                                left join SpecificationAttributeOptions as sao on sf.UnitId = sao.Id
                                left join SpecificationAttributeOptions as ssao on p.SmallUnitId = ssao.Id
                                left join SpecificationAttributeOptions as bsao on p.BigUnitId = bsao.Id
                                where sior.StoreId = " + storeId + " and sf.ChangeType = " + (int)StockFlowChangeTypeEnum.Audited + " and sior.BillType in (" + string.Join(",", billTypes) + ") and  (sior.CreatedOnUtc >= '" + start.ToString("yyyy-MM-dd 00:00:00") + "' and sior.CreatedOnUtc <= '" + end.ToString("yyyy-MM-dd 23:59:59") + "')";

            var lists = StocksRepository.QueryFromSql<StockQty>(sqlString).ToList();
            return lists.ToList();

        }


        /// <summary>
        /// 获取先进先出记录
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public StockInOutDetails GetFIFOProduct(int storeId, int wareHouseId, int productId)
        {
            var query = StockInOutDetailsRepository.TableNoTracking;
            query = query.Where(s => s.StoreId == storeId && s.Direction == 1 && s.WareHouseId == wareHouseId && s.ProductId == productId && s.RemainderQuantity > 0).OrderBy(s => s.InOutDate);
            return query.ToList().FirstOrDefault();
        }

        public virtual void UpdateStockInOutDetails(StockInOutDetails stockInOutDetail)
        {
            if (stockInOutDetail != null)
            {
                var uow = StockInOutDetailsRepository.UnitOfWork;
                StockInOutDetailsRepository.Update(stockInOutDetail);
                uow.SaveChanges();
                //Not being tracked by the context
                StockInOutDetailsRepository.Detached(stockInOutDetail);
            }
        }

    }
}
