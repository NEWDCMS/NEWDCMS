using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
#pragma warning disable CS0105 // “DCMS.Core.Domain.WareHouses”的 using 指令以前在此命名空间中出现过
using DCMS.Core.Domain.WareHouses;
#pragma warning restore CS0105 // “DCMS.Core.Domain.WareHouses”的 using 指令以前在此命名空间中出现过
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Report
{
    /// <summary>
    /// 用于表示库存报表服务
    /// </summary>
    public class StockReportService : BaseService, IStockReportService
    {
        #region 构造
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDistrictService _districtService;
        private readonly ICategoryService _categoryService;

        public StockReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IDistrictService districtService,
            ICategoryService categoryService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _districtService = districtService;
            _categoryService = categoryService;
        }
        #endregion

        /// <summary>
        /// 获取库存变化表(汇总)
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="price"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public StockChangeSummary GetStockChangeSummary(int? store, int? productId, int? wareHouseId, double? price, DateTime? startTime = null, DateTime? endTime = null)
        {
            var reporting = new StockChangeSummary();

            try
            {
                if (!store.HasValue)
                {
                    return null;
                }

                if (!productId.HasValue)
                {
                    return null;
                }

                if (startTime.HasValue)
                {
                    startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                }
                else
                {
                    return null;
                }

                if (endTime.HasValue)
                {
                    endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                }
                else
                {
                    return null;
                }

                var product = ProductsRepository_RO.TableNoTracking.FirstOrDefault(p => p.StoreId == store && p.Id == productId);
                if (product == null)
                {
                    return null;
                }
                //
                //商品期初数量
                var initialQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where p.StoreId = {store ?? 0} and p.Id = {productId} and s.WareHouseId = {wareHouseId} and sf.CreateTime <= '{startTime}'").ToList().FirstOrDefault().Value;
                //期初金额  
                var initialAmount = initialQuantity * price ?? 0;

                //期末数量
                var endQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where p.StoreId = {store ?? 0} and p.Id = {productId} and s.WareHouseId = {wareHouseId} and sf.CreateTime <= '{endTime}'").ToList().FirstOrDefault().Value;
                //期末金额 
                var endAmount = endQuantity * price ?? 0;

                //本期采购量
                var currentPurchaseQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (22) and sr.Direction = 1) and p.Id = {productId} and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期退购量
                var currentReturnQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value' from Products as p inner join Stocks as s  on p.Id = s.ProductId  inner join StockFlows as sf on s.Id = sf.StockId  where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (24) and sr.Direction = 2) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期调入量
                var currentAllocationInQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId  inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (31) and sr.Direction = 1) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期调出量
                var currentAllocationOutQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value' from Products as p inner join Stocks as s  on p.Id = s.ProductId  inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and p.StoreId = {store ?? 0} and BillType in (31) and sr.Direction = 2) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <='{endTime}')").ToList().FirstOrDefault().Value;
                //本期销售量
                var currentSaleQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId  inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (12) and sr.Direction = 2) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期退售量
                var currentSaleReturnQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId  where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (14) and sr.Direction = 1) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期组合量
                var currentCombinationQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId   inner join StockFlows as sf on s.Id = sf.StockId  where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (37) and sr.Direction = 1) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期拆分量
                var currentSplitReturnQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value' from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (38) and sr.Direction = 2) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期报损量
                var currentWasteQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId  where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (34) and sr.Direction = 1) and p.Id = {productId}  and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期盘盈量
                var currentVolumeQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId  inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (32, 35, 36) and sr.Direction = 1) and p.Id = {productId}  and(sf.CreateTime >='{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;
                //本期盘亏量
                var currentLossesQuantity = StocksRepository_RO.QueryFromSql<IntQueryType>($"select (case when SUM(sf.UsableQuantityChange) IS NULL then 0 else SUM(sf.UsableQuantityChange) end) as 'Value'  from Products as p inner join Stocks as s  on p.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  where p.StoreId = {store ?? 0} and BillType in (32, 35, 36) and sr.Direction = 2) and p.Id = {productId} and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}')").ToList().FirstOrDefault().Value;


                reporting.ProductName = product.Name;
                reporting.ProductSKU = product.Sku;
                reporting.BarCode = product.SmallBarCode;
                reporting.UnitId = product.SmallUnitId;
                reporting.BrandId = product.BrandId;
                reporting.UnitName = product.Name;
                reporting.UnitConversion = product.GetProductUnit(_specificationAttributeService, _productService).smallOption.UnitConversion;


                reporting.SmallUnitId = product.SmallUnitId;
                reporting.StrokeUnitId = product.StrokeUnitId;
                reporting.BigUnitId = product.BigUnitId ?? 0;

                reporting.StoreId = store ?? 0;
                reporting.ProductId = productId ?? 0;

                reporting.InitialQuantity = initialQuantity ?? 0;
                reporting.InitialQuantityName = product.StockQuantityFormat(initialQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.InitialAmount = (decimal)initialAmount;

                reporting.EndQuantity = endQuantity ?? 0;
                reporting.EndQuantityName = product.StockQuantityFormat(endQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.EndAmount = (decimal)endAmount;

                reporting.CurrentPurchaseQuantity = currentPurchaseQuantity ?? 0;
                reporting.CurrentReturnQuantity = currentReturnQuantity ?? 0;
                reporting.CurrentAllocationInQuantity = currentAllocationInQuantity ?? 0;
                reporting.CurrentAllocationOutQuantity = currentAllocationOutQuantity ?? 0;
                reporting.CurrentSaleQuantity = currentSaleQuantity ?? 0;
                reporting.CurrentSaleReturnQuantity = currentSaleReturnQuantity ?? 0;
                reporting.CurrentCombinationQuantity = currentCombinationQuantity ?? 0;
                reporting.CurrentSplitReturnQuantity = currentSplitReturnQuantity ?? 0;
                reporting.CurrentWasteQuantity = currentWasteQuantity ?? 0;
                reporting.CurrentVolumeQuantity = currentVolumeQuantity ?? 0;
                reporting.CurrentLossesQuantity = currentLossesQuantity ?? 0;

                reporting.CurrentPurchaseQuantityName = product.StockQuantityFormat(currentPurchaseQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentReturnQuantityName = product.StockQuantityFormat(currentReturnQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentAllocationInQuantityName = product.StockQuantityFormat(currentAllocationInQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentAllocationOutQuantityName = product.StockQuantityFormat(currentAllocationOutQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentSaleQuantityName = product.StockQuantityFormat(currentSaleQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentSaleReturnQuantityName = product.StockQuantityFormat(currentSaleReturnQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentCombinationQuantityName = product.StockQuantityFormat(currentCombinationQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentSplitReturnQuantityName = product.StockQuantityFormat(currentSplitReturnQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentWasteQuantityName = product.StockQuantityFormat(currentWasteQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentVolumeQuantityName = product.StockQuantityFormat(currentVolumeQuantity ?? 0, _specificationAttributeService, _productService);
                reporting.CurrentLossesQuantityName = product.StockQuantityFormat(currentLossesQuantity ?? 0, _specificationAttributeService, _productService);

                return reporting;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return reporting;
            }
        }

        /// <summary>
        /// 获取全部库存变化表(汇总)
        /// </summary>
        /// <param name="store"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="price"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<StockChangeSummary> GetAllStockChangeSummary(int? store, int? wareHouseId, double? price, DateTime? startTime = null, DateTime? endTime = null)
        {
            var reporting = new List<StockChangeSummary>();

            try
            {
                if (!store.HasValue)
                {
                    return null;
                }

                if (startTime.HasValue)
                {
                    startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                }
                else
                {
                    return null;
                }

                if (endTime.HasValue)
                {
                    endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                }
                else
                {
                    return null;
                }

                //获取可用库存商品
                var query = from p in ProductsRepository_RO.Table
                            join stk in StocksRepository_RO.Table on p.Id equals stk.ProductId
                            where p.StoreId == store && stk.UsableQuantity > 0
                            select p;

                if (wareHouseId.HasValue && wareHouseId != 0)
                {
                    query = from p in ProductsRepository_RO.Table
                            join stk in StocksRepository_RO.Table on p.Id equals stk.ProductId
                            where p.StoreId == store && stk.UsableQuantity > 0 && stk.WareHouseId == wareHouseId
                            select p;
                }
                //去重
                query = from p in query group p by p.Id into pGroup orderby pGroup.Key select pGroup.FirstOrDefault();

                var allProducts = query.ToList();
                allProducts.ForEach(p =>
                {
                    var stockChange = GetStockChangeSummary(store, p.Id, wareHouseId, price, startTime, endTime);
                    if (stockChange != null)
                    {
                        reporting.Add(stockChange);
                    }
                });
                return reporting;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return reporting;
            }
        }

        /// <summary>
        /// 获取全部库存变化表(汇总) 减少查询次数
        /// </summary>
        /// <param name="store"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="productId"></param>
        /// <param name="brandId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<StockChangeSummary> GetAllStockChangeSummary(int? storeId, int? wareHouseId, int? productId, string productName, int? brandId, DateTime? startTime = null, DateTime? endTime = null)
        {
            productName = CommonHelper.Filter(productName);

            var reporting = new List<StockChangeSummary>();

            try
            {
                if (!storeId.HasValue)
                {
                    return reporting;
                }

                if (startTime.HasValue)
                {
                    startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                }
                else
                {
                    return reporting;
                }

                if (endTime.HasValue)
                {
                    endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                }
                else
                {
                    return reporting;
                }

                string whereQuery1 = $" p.StoreId= {storeId ?? 0}";
                string whereQuery2 = $" 1=1 ";
                string whereQuery3 = $" 1=1 ";
                if (wareHouseId.HasValue && wareHouseId.Value > 0)
                {
                    whereQuery1 += $" and s.WareHouseId = '{wareHouseId}' ";
                    whereQuery2 += $" and s.WareHouseId = '{wareHouseId}' ";
                    whereQuery3 += $" and (sr.OutWareHouseId='{wareHouseId}' or sr.InWareHouseId='{wareHouseId}') ";
                }
                if (productId.HasValue && productId.Value > 0)
                {
                    whereQuery1 += $" and p.Id = '{productId}' ";
                }
                if (productName != null)
                {
                    whereQuery1 += $" and p.Name like '%{productName}%' ";
                }
                if (brandId.HasValue && brandId.Value > 0)
                {
                    whereQuery1 += $" and p.BrandId = '{brandId}' ";
                }

                //string sqlString = $"select pt.StoreId,pt.ProductId,pt.ProductName,pt.ProductSKU,pt.BarCode,pt.SmallUnitId UnitId,pt.BrandId,'' UnitName,'' UnitConversion,pt.Price,'' PriceName" +
                //    $",pt.SmallUnitId,pt.SmallUnitName,pt.StrokeUnitId,pt.StrokeUnitName,pt.BigUnitId,pt.BigUnitName,pt.BigQuantity,ifnull(pt.StrokeQuantity,0) StrokeQuantity" +
                //    $",ifnull(t1.InitialQuantity,0) InitialQuantity,'' InitialQuantityName,'0.00' InitialAmount,ifnull(t2.EndQuantity,0) EndQuantity,'' EndQuantityName,'0.00' EndAmount" +
                //    $",ifnull(t3.CurrentPurchaseQuantity,0) CurrentPurchaseQuantity,ifnull(t4.CurrentReturnQuantity,0) CurrentReturnQuantity,ifnull(t5.CurrentAllocationInQuantity,0) CurrentAllocationInQuantity" +
                //    $",ifnull(t6.CurrentAllocationOutQuantity,0) CurrentAllocationOutQuantity,ifnull(t7.CurrentSaleQuantity,0) CurrentSaleQuantity,ifnull(t8.CurrentSaleReturnQuantity,0) CurrentSaleReturnQuantity" +
                //    $",ifnull(t9.CurrentCombinationQuantity,0) CurrentCombinationQuantity,ifnull(t10.CurrentSplitReturnQuantity,0) CurrentSplitReturnQuantity,ifnull(t11.CurrentWasteQuantity,0) CurrentWasteQuantity" +
                //    $",ifnull(t12.CurrentVolumeQuantity,0) CurrentVolumeQuantity,ifnull(t13.CurrentLossesQuantity,0) CurrentLossesQuantity,''CurrentPurchaseQuantityName,'' CurrentReturnQuantityName" +
                //    $",'' CurrentAllocationInQuantityName,'' CurrentAllocationOutQuantityName,'' CurrentSaleQuantityName,'' CurrentSaleReturnQuantityName,'' CurrentCombinationQuantityName" +
                //    $",'' CurrentSplitReturnQuantityName,'' CurrentWasteQuantityName,'' CurrentVolumeQuantityName,'' CurrentLossesQuantityName from (select distinct p.StoreId, p.Id ProductId, p.Name ProductName" +
                //    $", p.Sku ProductSKU, p.SmallBarCode BarCode, p.BrandId, p.SmallUnitId, sp1.Name SmallUnitName,p.StrokeUnitId, sp2.Name StrokeUnitName, p.BigUnitId, sp3.Name BigUnitName, p.BigQuantity,0 Price," +
                //    $" p.StrokeQuantity from Products p " +
                //    $"inner join Stocks s on p.Id = s.ProductId " +
                //    $"left join SpecificationAttributeOptions sp1 on p.SmallUnitId = sp1.Id " +
                //    $"left join SpecificationAttributeOptions sp2 on p.StrokeUnitId = sp1.Id " +
                //    $"left join SpecificationAttributeOptions sp3 on p.BigUnitId = sp3.Id where {whereQuery1} and s.UsableQuantity > 0) pt " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange, 0)) InitialQuantity, p1.Id ProductId, p1.StoreId from Products as p1 " +
                //    $"inner join Stocks as s  on p1.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where {whereQuery2} and sf.CreateTime <= '{startTime}' " +
                //    $"group by p1.Id, p1.StoreId) t1 on t1.ProductId = pt.ProductId and t1.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) " +
                //    $"EndQuantity,p2.Id ProductId, p2.StoreId from Products as p2 inner join Stocks as s  on p2.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId " +
                //    $"where {whereQuery2} and sf.CreateTime <= '{endTime}' group by p2.Id,p2.StoreId) t2 on t2.ProductId = pt.ProductId and t2.StoreId = pt.StoreId " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentPurchaseQuantity,p3.Id ProductId, p3.StoreId from Products as p3 " +
                //    $"inner join Stocks as s  on p3.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (22) and sr.Direction = 1 and {whereQuery3}) and(sf.CreateTime >= '{startTime}' " +
                //    $"and sf.CreateTime <= '{endTime}') group by p3.Id,p3.StoreId) t3 on t3.ProductId = pt.ProductId and t3.StoreId = pt.StoreId " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentReturnQuantity,p4.Id ProductId, p4.StoreId from Products as p4 " +
                //    $"inner join Stocks as s  on p4.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId " +
                //    $"from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (24) and " +
                //    $"sr.Direction = 2 and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p4.Id,p4.StoreId) t4 on t4.ProductId = pt.ProductId " +
                //    $"and t4.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentAllocationInQuantity,p5.Id ProductId, p5.StoreId from Products as p5 " +
                //    $"inner join Stocks as s  on p5.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (31) and sr.Direction = 1 and {whereQuery3}) and(sf.CreateTime >= '{startTime}' " +
                //    $"and sf.CreateTime <= '{endTime}') group by p5.Id,p5.StoreId) t5 on t5.ProductId = pt.ProductId and t5.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentAllocationOutQuantity" +
                //    $",p6.Id ProductId, p6.StoreId from Products as p6 inner join Stocks as s  on p6.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId " +
                //    $"where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (31) and sr.Direction = 2 " +
                //    $"and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p6.Id,p6.StoreId) t6 on t6.ProductId = pt.ProductId and t6.StoreId = pt.StoreId " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentSaleQuantity,p7.Id ProductId, p7.StoreId from Products as p7 inner join Stocks as s  on p7.Id = s.ProductId " +
                //    $"inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId  " +
                //    $"inner join dcms.SaleBills sb on sb.id = sr.BillId and sb.ReversedStatus != 1 where BillType in (12) and sr.Direction = 2 and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') " +
                //    $"group by p7.Id,p7.StoreId) t7 on t7.ProductId = pt.ProductId and t7.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentSaleReturnQuantity,p8.Id ProductId, p8.StoreId " +
                //    $"from Products as p8 inner join Stocks as s  on p8.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (14) and sr.Direction = 1 and {whereQuery3}) and(sf.CreateTime >= '{startTime}' " +
                //    $"and sf.CreateTime <= '{endTime}') group by p8.Id,p8.StoreId) t8 on t8.ProductId = pt.ProductId and t8.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentCombinationQuantity" +
                //    $",p9.Id ProductId, p9.StoreId from Products as p9 inner join Stocks as s  on p9.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId " +
                //    $"from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (37) and sr.Direction = 1 " +
                //    $"and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p9.Id,p9.StoreId) t9 on t9.ProductId = pt.ProductId and t9.StoreId = pt.StoreId " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentSplitReturnQuantity,p10.Id ProductId, p10.StoreId from Products as p10 inner join Stocks as s  " +
                //    $"on p10.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (38) and sr.Direction = 2 " +
                //    $"and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p10.Id,p10.StoreId) t10 on t10.ProductId = pt.ProductId " +
                //    $"and t10.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentWasteQuantity,p11.Id ProductId, p11.StoreId from Products as p11 " +
                //    $"inner join Stocks as s  on p11.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (34) and sr.Direction = 1 and {whereQuery3}) " +
                //    $"and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p11.Id,p11.StoreId) t11 on t11.ProductId = pt.ProductId and t11.StoreId = pt.StoreId " +
                //    $"left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentVolumeQuantity,p12.Id ProductId, p12.StoreId from Products as p12 inner join Stocks as s  " +
                //    $"on p12.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId from StockInOutRecords as sr " +
                //    $"inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (32, 35, 36) and sr.Direction = 1 " +
                //    $"and {whereQuery3}) and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p12.Id,p12.StoreId) t12 on t12.ProductId = " +
                //    $"pt.ProductId and t12.StoreId = pt.StoreId left join (select SUM(ifnull(sf.UsableQuantityChange,0)) CurrentLossesQuantity,p13.Id ProductId, p13.StoreId " +
                //    $"from Products as p13 inner join Stocks as s  on p13.Id = s.ProductId inner join StockFlows as sf on s.Id = sf.StockId where sf.Id in (select srsf.StockFlowId " +
                //    $"from StockInOutRecords as sr inner join StockInOutRecords_StockFlows_Mapping as srsf on sr.Id = srsf.StockInOutRecordId where BillType in (32, 35, 36) and sr.Direction = 2 and {whereQuery3})" +
                //    $" and(sf.CreateTime >= '{startTime}' and sf.CreateTime <= '{endTime}') group by p13.Id,p13.StoreId) t13 on t13.ProductId = pt.ProductId and t13.StoreId = pt.StoreId";

                var sqlString = @"SELECT pt.StoreId, pt.ProductId, pt.ProductName, pt.ProductSKU, pt.BarCode
	                        , pt.SmallUnitId AS UnitId, pt.BrandId, '' AS UnitName, '' AS UnitConversion, pt.Price
	                        , '' AS PriceName, pt.SmallUnitId, pt.SmallUnitName, pt.StrokeUnitId, pt.StrokeUnitName
	                        , pt.BigUnitId, pt.BigUnitName, pt.BigQuantity
	                        , ifnull(pt.StrokeQuantity, 0) AS StrokeQuantity
	                        , ifnull(t1.InitialQuantity, 0) AS InitialQuantity, '' AS InitialQuantityName
	                        , '0.00' AS InitialAmount, ifnull(t2.EndQuantity, 0) AS EndQuantity, '' AS EndQuantityName
	                        , '0.00' AS EndAmount, ifnull(t3.CurrentPurchaseQuantity, 0) AS CurrentPurchaseQuantity
	                        , ifnull(t4.CurrentReturnQuantity, 0) AS CurrentReturnQuantity
	                        , ifnull(t5.CurrentAllocationInQuantity, 0) AS CurrentAllocationInQuantity
	                        , ifnull(t6.CurrentAllocationOutQuantity, 0) AS CurrentAllocationOutQuantity
	                        , ifnull(t7.CurrentSaleQuantity, 0) AS CurrentSaleQuantity
	                        , ifnull(t8.CurrentSaleReturnQuantity, 0) AS CurrentSaleReturnQuantity
	                        , ifnull(t9.CurrentCombinationQuantity, 0) AS CurrentCombinationQuantity
	                        , ifnull(t10.CurrentSplitReturnQuantity, 0) AS CurrentSplitReturnQuantity
	                        , ifnull(t11.CurrentWasteQuantity, 0) AS CurrentWasteQuantity
	                        , ifnull(t12.CurrentVolumeQuantity, 0) AS CurrentVolumeQuantity
	                        , ifnull(t13.CurrentLossesQuantity, 0) AS CurrentLossesQuantity, '' AS CurrentPurchaseQuantityName
	                        , '' AS CurrentReturnQuantityName, '' AS CurrentAllocationInQuantityName, '' AS CurrentAllocationOutQuantityName, '' AS CurrentSaleQuantityName, '' AS CurrentSaleReturnQuantityName
	                        , '' AS CurrentCombinationQuantityName, '' AS CurrentSplitReturnQuantityName, '' AS CurrentWasteQuantityName, '' AS CurrentVolumeQuantityName, '' AS CurrentLossesQuantityName,t14.GiftQuantity
                        FROM (
	                        SELECT DISTINCT p.StoreId, p.Id AS ProductId, p.Name AS ProductName, p.Sku AS ProductSKU, p.SmallBarCode AS BarCode
		                        , p.BrandId, p.SmallUnitId, sp1.Name AS SmallUnitName, p.StrokeUnitId, sp2.Name AS StrokeUnitName
		                        , p.BigUnitId, sp3.Name AS BigUnitName, p.BigQuantity, 0 AS Price, p.StrokeQuantity
	                        FROM Products p
		                        INNER JOIN Stocks s ON p.Id = s.ProductId
		                        LEFT JOIN SpecificationAttributeOptions sp1 ON p.SmallUnitId = sp1.Id
		                        LEFT JOIN SpecificationAttributeOptions sp2 ON p.StrokeUnitId = sp1.Id
		                        LEFT JOIN SpecificationAttributeOptions sp3 ON p.BigUnitId = sp3.Id
	                        WHERE {0}
		                        AND s.UsableQuantity > 0
                        ) pt
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS InitialQuantity, p1.Id AS ProductId
			                        , p1.StoreId
		                        FROM Products p1
			                        INNER JOIN Stocks s ON p1.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE {1}
			                        AND sf.CreateTime <= '{2}'
		                        GROUP BY p1.Id, p1.StoreId
	                        ) t1
	                        ON t1.ProductId = pt.ProductId
		                        AND t1.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS EndQuantity, p2.Id AS ProductId
			                        , p2.StoreId
		                        FROM Products p2
			                        INNER JOIN Stocks s ON p2.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE {1}
			                        AND sf.CreateTime <= '{3}'
		                        GROUP BY p2.Id, p2.StoreId
	                        ) t2
	                        ON t2.ProductId = pt.ProductId
		                        AND t2.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentPurchaseQuantity, p3.Id AS ProductId
			                        , p3.StoreId
		                        FROM Products p3
			                        INNER JOIN Stocks s ON p3.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (22)
					                        AND sr.Direction = 1
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p3.Id, p3.StoreId
	                        ) t3
	                        ON t3.ProductId = pt.ProductId
		                        AND t3.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentReturnQuantity, p4.Id AS ProductId
			                        , p4.StoreId
		                        FROM Products p4
			                        INNER JOIN Stocks s ON p4.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (24)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p4.Id, p4.StoreId
	                        ) t4
	                        ON t4.ProductId = pt.ProductId
		                        AND t4.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentAllocationInQuantity, p5.Id AS ProductId
			                        , p5.StoreId
		                        FROM Products p5
			                        INNER JOIN Stocks s ON p5.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (31)
					                        AND sr.Direction = 1
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p5.Id, p5.StoreId
	                        ) t5
	                        ON t5.ProductId = pt.ProductId
		                        AND t5.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentAllocationOutQuantity, p6.Id AS ProductId
			                        , p6.StoreId
		                        FROM Products p6
			                        INNER JOIN Stocks s ON p6.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (31)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p6.Id, p6.StoreId
	                        ) t6
	                        ON t6.ProductId = pt.ProductId
		                        AND t6.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentSaleQuantity, p7.Id AS ProductId
			                        , p7.StoreId
		                        FROM Products p7
			                        INNER JOIN Stocks s ON p7.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
					                        INNER JOIN dcms.SaleBills sb
					                        ON sb.id = sr.BillId
						                        AND sb.ReversedStatus != 1
				                        WHERE BillType IN (12)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p7.Id, p7.StoreId
	                        ) t7
	                        ON t7.ProductId = pt.ProductId
		                        AND t7.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentSaleReturnQuantity, p8.Id AS ProductId
			                        , p8.StoreId
		                        FROM Products p8
			                        INNER JOIN Stocks s ON p8.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (14)
					                        AND sr.Direction = 1
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p8.Id, p8.StoreId
	                        ) t8
	                        ON t8.ProductId = pt.ProductId
		                        AND t8.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentCombinationQuantity, p9.Id AS ProductId
			                        , p9.StoreId
		                        FROM Products p9
			                        INNER JOIN Stocks s ON p9.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (37)
					                        AND sr.Direction = 1
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p9.Id, p9.StoreId
	                        ) t9
	                        ON t9.ProductId = pt.ProductId
		                        AND t9.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentSplitReturnQuantity, p10.Id AS ProductId
			                        , p10.StoreId
		                        FROM Products p10
			                        INNER JOIN Stocks s ON p10.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (38)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p10.Id, p10.StoreId
	                        ) t10
	                        ON t10.ProductId = pt.ProductId
		                        AND t10.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentWasteQuantity, p11.Id AS ProductId
			                        , p11.StoreId
		                        FROM Products p11
			                        INNER JOIN Stocks s ON p11.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (34)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p11.Id, p11.StoreId
	                        ) t11
	                        ON t11.ProductId = pt.ProductId
		                        AND t11.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentVolumeQuantity, p12.Id AS ProductId
			                        , p12.StoreId
		                        FROM Products p12
			                        INNER JOIN Stocks s ON p12.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (32, 35, 36)
					                        AND sr.Direction = 1
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p12.Id, p12.StoreId
	                        ) t12
	                        ON t12.ProductId = pt.ProductId
		                        AND t12.StoreId = pt.StoreId
	                        LEFT JOIN (
		                        SELECT SUM(ifnull(sf.UsableQuantityChange, 0)) AS CurrentLossesQuantity, p13.Id AS ProductId
			                        , p13.StoreId
		                        FROM Products p13
			                        INNER JOIN Stocks s ON p13.Id = s.ProductId
			                        INNER JOIN StockFlows sf ON s.Id = sf.StockId
		                        WHERE sf.Id IN (
				                        SELECT srsf.StockFlowId
				                        FROM StockInOutRecords sr
					                        INNER JOIN StockInOutRecords_StockFlows_Mapping srsf ON sr.Id = srsf.StockInOutRecordId
				                        WHERE BillType IN (32, 35, 36)
					                        AND sr.Direction = 2
					                        AND {4}
			                        )
			                        AND (sf.CreateTime >= '{2}'
				                        AND sf.CreateTime <= '{3}')
		                        GROUP BY p13.Id, p13.StoreId
	                        ) t13
	                        ON t13.ProductId = pt.ProductId
		                        AND t13.StoreId = pt.StoreId
	                        LEFT JOIN(
		                        SELECT  SUM(CASE WHEN si.UnitId=pro.BigUnitId THEN si.Quantity*pro.BigQuantity
								                        WHEN si.UnitId=pro.StrokeUnitId THEN si.Quantity*pro.StockQuantity
								                        ELSE si.Quantity END) AS GiftQuantity,si.ProductId,si.StoreId
                         FROM SaleItems si
			                        INNER JOIN SaleBills sb on si.SaleBillId=sb.Id
			                        INNER JOIN Products pro ON pro.Id=si.ProductId
		                        WHERE sb.AuditedDate>='{2}'
		                          AND sb.AuditedDate<='{3}'
			                        AND si.IsGifts=1
		                        GROUP BY si.ProductId,si.StoreId
	                        ) t14
	                        ON t14.ProductId=pt.ProductId
		                        AND t14.StoreId=pt.StoreId";
                sqlString = string.Format(sqlString,whereQuery1,whereQuery2,startTime,endTime,whereQuery3);
                reporting = StocksRepository_RO.QueryFromSql<StockChangeSummary>(sqlString).ToList();
                if (reporting != null && reporting.Count > 0)
                {
                    reporting.ForEach(it =>
                    {
                        it.PriceName = it.Price.ToString("#.00");
                        it.InitialQuantityName = Pexts.StockQuantityFormat(it.InitialQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.EndQuantityName = Pexts.StockQuantityFormat(it.EndQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);

                        it.CurrentPurchaseQuantityName = Pexts.StockQuantityFormat(it.CurrentPurchaseQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentReturnQuantityName = Pexts.StockQuantityFormat(it.CurrentReturnQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentAllocationInQuantityName = Pexts.StockQuantityFormat(it.CurrentAllocationInQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentAllocationOutQuantityName = Pexts.StockQuantityFormat(it.CurrentAllocationOutQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentSaleQuantityName = Pexts.StockQuantityFormat(it.CurrentSaleQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentSaleReturnQuantityName = Pexts.StockQuantityFormat(it.CurrentSaleReturnQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentCombinationQuantityName = Pexts.StockQuantityFormat(it.CurrentCombinationQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentSplitReturnQuantityName = Pexts.StockQuantityFormat(it.CurrentSplitReturnQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentWasteQuantityName = Pexts.StockQuantityFormat(it.CurrentWasteQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentVolumeQuantityName = Pexts.StockQuantityFormat(it.CurrentVolumeQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);
                        it.CurrentLossesQuantityName = Pexts.StockQuantityFormat(it.CurrentLossesQuantity, it.StrokeQuantity ?? 0, it.BigQuantity ?? 0, it.SmallUnitName, it.StrokeUnitName, it.BigUnitName);

                    });
                }

                return reporting;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return reporting;
            }
        }

        /// <summary>
        /// 获取全部库存变化表(按单据)
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="billType"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="billCode"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<StockChangeSummaryOrder> GetStockChangeSummaryByOrder(int? store, int? productId, string productName, int? categoryId, int? billType, int? wareHouseId, string billCode = "", DateTime? startTime = null, DateTime? endTime = null, bool crossMonth = true)
        {
            var reporting = new List<StockChangeSummaryOrder>();
            billCode = CommonHelper.Filter(billCode);

            try
            {
                string queryString = "select * from ( select tmp.StoreId,p.id as ProductId,p.Name as ProductName,p.Sku as ProductSKU,p.SmallBarCode as BarCode,0 as UnitId, '' as UnitName,p.BrandId,p.CategoryId,'' as UnitConversion, 0 Price,'' as PriceName,p.SmallUnitId,IFNULL(p.StrokeUnitId,0) as StrokeUnitId,IFNULL(p.BigUnitId,0) as BigUnitId,tmp.UsableQuantityChange,'' as UsableQuantityChangeConversion,tmp.UsableQuantityAfter,'' as UsableQuantityAfterConversion,tmp.StockFlowId,tmp.StockInOutRecordId,tmp.CreateTime,sior.BillType,'' as BillTypeName,sior.BillCode,'' as LinkUrl,sior.Direction,(case when sior.BillType=22 then (select b22.Id from PurchaseBills b22 where b22.BillNumber=sior.BillCode limit 1) when sior.BillType = 24 then(select b24.Id from PurchaseReturnBills b24 where b24.BillNumber = sior.BillCode limit 1) when sior.BillType = 12 then(select b12.Id from SaleBills b12 where b12.BillNumber = sior.BillCode limit 1) when sior.BillType = 14 then(select b14.Id from ReturnBills b14 where b14.BillNumber = sior.BillCode limit 1) when sior.BillType = 37 then(select b37.Id from CombinationProductBills b37 where b37.BillNumber = sior.BillCode limit 1) when sior.BillType = 38 then(select b38.Id from SplitProductBills b38 where b38.BillNumber = sior.BillCode limit 1) when sior.BillType = 34 then(select b34.Id from ScrapProductBills b34 where b34.BillNumber = sior.BillCode limit 1) when sior.BillType = 31 then(select b31.Id from AllocationBills b31 where b31.BillNumber = sior.BillCode limit 1) when sior.BillType = 32 then(select b32.Id from InventoryProfitLossBills b32 where b32.BillNumber = sior.BillCode limit 1) else 0 end) BillId,(case when sior.BillType = 22 then(select m.Name from Manufacturer m where m.Id = (select b22.ManufacturerId from PurchaseBills b22 where b22.BillNumber = sior.BillCode limit 1) limit 1) when sior.BillType = 24 then(select m.Name from Manufacturer m where m.Id = (select b24.ManufacturerId from PurchaseReturnBills b24 where b24.BillNumber = sior.BillCode limit 1) limit 1) when sior.BillType = 12 then(select t.Name from dcms_crm.CRM_Terminals t where t.Id = (select b12.TerminalId from SaleBills b12 where b12.BillNumber = sior.BillCode limit 1) limit 1) when sior.BillType = 14 then(select t.Name from dcms_crm.CRM_Terminals t where t.Id = (select b14.TerminalId from ReturnBills b14 where b14.BillNumber = sior.BillCode limit 1) limit 1) else '' end) CustomerSupplier,(case when sior.BillType = 22 then(select b22.CreatedOnUtc from PurchaseBills b22 where b22.BillNumber = sior.BillCode limit 1) when sior.BillType = 24 then(select b24.CreatedOnUtc from PurchaseReturnBills b24 where b24.BillNumber = sior.BillCode limit 1) when sior.BillType = 12 then(select b12.CreatedOnUtc from SaleBills b12 where b12.BillNumber = sior.BillCode limit 1) when sior.BillType = 14 then(select b14.CreatedOnUtc from ReturnBills b14 where b14.BillNumber = sior.BillCode limit 1) when sior.BillType = 37 then(select b37.CreatedOnUtc from CombinationProductBills b37 where b37.BillNumber = sior.BillCode limit 1) when sior.BillType = 38 then(select b38.CreatedOnUtc from SplitProductBills b38 where b38.BillNumber = sior.BillCode limit 1) when sior.BillType = 34 then(select b34.CreatedOnUtc from ScrapProductBills b34 where b34.BillNumber = sior.BillCode limit 1) when sior.BillType = 31 then(select b31.CreatedOnUtc from AllocationBills b31 where b31.BillNumber = sior.BillCode limit 1) when sior.BillType = 32 then(select b32.CreatedOnUtc from InventoryProfitLossBills b32 where b32.BillNumber = sior.BillCode limit 1) else null end) CreatedOnUtc,(case when sior.BillType = 22 then(select b22.AuditedDate from PurchaseBills b22 where b22.BillNumber = sior.BillCode limit 1) when sior.BillType = 24 then(select b24.AuditedDate from PurchaseReturnBills b24 where b24.BillNumber = sior.BillCode limit 1) when sior.BillType = 12 then(select b12.AuditedDate from SaleBills b12 where b12.BillNumber = sior.BillCode limit 1) when sior.BillType = 14 then(select b14.AuditedDate from ReturnBills b14 where b14.BillNumber = sior.BillCode limit 1) when sior.BillType = 37 then(select b37.AuditedDate from CombinationProductBills b37 where b37.BillNumber = sior.BillCode limit 1) when sior.BillType = 38 then(select b38.AuditedDate from SplitProductBills b38 where b38.BillNumber = sior.BillCode limit 1) when sior.BillType = 34 then(select b34.AuditedDate from ScrapProductBills b34 where b34.BillNumber = sior.BillCode limit 1) when sior.BillType = 31 then(select b31.AuditedDate from AllocationBills b31 where b31.BillNumber = sior.BillCode limit 1) when sior.BillType = 32 then(select b32.AuditedDate from InventoryProfitLossBills b32 where b32.BillNumber = sior.BillCode limit 1) else null end) AuditedDate, (if(sior.InWareHouseId=0,sior.OutWareHouseId,sior.InWareHouseId)) WareHouseId from Products as p inner join(select sf.Id, sf.StoreId, sf.StockId, sf.ProductId, sf.UsableQuantityChange, sf.UsableQuantityAfter,sf.CreateTime, srsf.StockFlowId, srsf.StockInOutRecordId from StockFlows as sf inner join StockInOutRecords_StockFlows_Mapping as srsf on sf.Id = srsf.StockFlowId) as tmp  on p.Id = tmp.ProductId inner join StockInOutRecords as sior on tmp.StockInOutRecordId = sior.Id where BillType in (22, 24, 12, 14, 37, 38, 34, 31, 32)";


                if (crossMonth)
                {
                    if (startTime.HasValue)
                    {
                        queryString += " and tmp.CreateTime >= '" + startTime + "'";
                    }

                    if (endTime.HasValue)
                    {
                        queryString += $" and tmp.CreateTime <= '{endTime}'";
                    }
                }

                if (productId.HasValue && productId >0 )
                {
                    queryString += $" and ProductId = '{productId}'";
                }

                if (productName != "")
                {
                    queryString += $" and p.name like '%{productName}%'";
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(store ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        string incategoryIds = string.Join("','", categoryIds);
                        queryString += $" and CategoryId in ('{incategoryIds}') ";
                    }
                    else
                    {
                        queryString += " and CategoryId = '" + categoryId + "'";
                    }
                }

                if (wareHouseId.HasValue && wareHouseId > 0)
                {
                    queryString += " and (sior.InWareHouseId='" + wareHouseId + "' or sior.OutWareHouseId = '" + wareHouseId + "')";
                }

                if (billType.HasValue && billType == 311)//调入
                {
                    queryString += " and (BillType=31 and Direction=1) ";
                }
                else if (billType.HasValue && billType == 312)//调出
                {
                    queryString += " and (BillType=31 and Direction=2) ";
                }
                else if (billType.HasValue && billType == 321)//盘盈
                {
                    queryString += " and (BillType=32 and Direction=1) ";
                }
                else if (billType.HasValue && billType == 322)//盘亏
                {
                    queryString += " and (BillType=32 and Direction=2) ";
                }
                else if (billType.HasValue && billType.Value > 0)
                {
                    queryString += " and BillType= '" + billType + "' ";
                }

                if (!string.IsNullOrEmpty(billCode))
                {
                    queryString += " and BillCode = '" + billCode + "'";
                }

                queryString += $" and tmp.StoreId = { store ?? 0} order by tmp.Id desc ) temp2";


                queryString += " where temp2.BillId is not null ";

                if (!crossMonth)
                {
                    if (startTime.HasValue)
                    {
                        queryString += " and CreatedOnUtc >= '" + startTime + "'";
                    }

                    if (endTime.HasValue)
                    {
                        queryString += $" and CreatedOnUtc <= '{endTime}'";
                    }
                }

                var query = StocksRepository_RO.QueryFromSql<StockChangeSummaryOrder>(queryString);

                reporting = query.ToList();

                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 门店库存上报表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<InventoryReportList> GetInventoryReportList(int? store, int? businessUserId, int? terminalId, string terminalName, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null)
        {

            try
            {
                return _cacheManager.Get(DCMSDefaults.STOCKREPORTSERVICE_GETINVENTORYREPORTLIST_KEY.FillCacheKey(store, businessUserId, terminalId, terminalName, channelId, rankId, districtId, productId, productName, startTime, endTime), () =>
                    {

                        terminalName = CommonHelper.Filter(terminalName);
                        productName = CommonHelper.Filter(productName);

                        var reporting = new List<InventoryReportList>();

                        string whereQuery = $" a.StoreId = {store ?? 0} ";
                        string whereQuery2 = $" alls.StoreId = {store ?? 0} ";

                        if (businessUserId.HasValue && businessUserId.Value != 0)
                        {
                            whereQuery += $" and a.BusinessUserId = '{businessUserId}' ";
                        }

                        if (terminalId.HasValue && terminalId.Value != 0)
                        {
                            whereQuery += $" and a.TerminalId = '{terminalId}' ";
                        }

                        if (terminalName != null)
                        {
                            whereQuery2 += $" and t.Name like '%{terminalName}%' ";
                        }
                        if (productId.HasValue && productId.Value != 0)
                        {
                            whereQuery += $" and ab.ProductId = '{productId}' ";
                        }
                        if (productName != null)
                        {
                            whereQuery2 += $" and p.Name like '%{productName}%' ";
                        }
                        if (districtId.HasValue && districtId.Value != 0)
                        {
                            //递归片区查询
                            var distinctIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);
                            if (distinctIds != null && distinctIds.Count > 0)
                            {
                                string inDistinctIds = string.Join("','", distinctIds);
                                whereQuery2 += $" and t.DistrictId in ('{inDistinctIds}') ";
                            }
                            else
                            {
                                whereQuery2 += $" and t.DistrictId = '{districtId}' ";
                            }
                        }
                        if (rankId.HasValue && rankId.Value != 0)
                        {
                            whereQuery2 += $" and t.RankId = '{rankId}' ";
                        }

                        if (channelId.HasValue && channelId.Value != 0)
                        {
                            whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                        }

                        if (startTime.HasValue)
                        {
                            startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                            whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                        }

                        if (endTime.HasValue)
                        {
                            endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                            whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                        }

                        string mySqlString = $"select alls.StoreId, alls.TerminalId,t.Name as TerminalName, alls.BusinessUserId,'' BusinessUserName, alls.ProductId, p.Name as ProductName," +
                        $" p.Sku as ProductCode, p.SmallBarCode, p.StrokeBarCode, p.BigBarCode, CONCAT('1', CAST(b.Name AS char), '=', p.BigQuantity, CAST(s.Name AS char)) UnitConversion," +
                        $" p.StrokeQuantity, p.BigQuantity, s.Name as SmallUnitName, m.Name as StrokeUnitName, b.Name as BigUnitName, p.BigUnitId as BigUnitId, p.StrokeUnitId as StrokeUnitId," +
                        $" p.SmallUnitId as SmallUnitId, (select(IFNULL(ab.SmallQuantity, 0) + (IFNULL(ab.StrokeQuantity, 0) * p.StrokeQuantity) +(IFNULL(ab.BigQuantity, 0)) * p.BigQuantity) " +
                        $"FROM InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId  where {whereQuery} and a.Id in " +
                        $"(select a.Id FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId " +
                        $"and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc LIMIT 1) BeginStoreQuantity, (select CONCAT(ab.BigQuantity," +
                        $" CAST(b.Name AS CHAR), ab.StrokeQuantity, CAST(m.Name AS CHAR), ab.SmallQuantity, CAST(s.Name AS CHAR))  FROM InventoryReportBills as a " +
                        $"inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId  where {whereQuery} and a.Id in (select a.Id FROM  InventoryReportBills as a " +
                        $"inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId and a.BusinessUserId = alls.BusinessUserId " +
                        $"and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc LIMIT 1) BeginStoreQuantityConversion, (select a.CreatedOnUtc FROM  InventoryReportBills as a " +
                        $"inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where {whereQuery} and a.Id in (select a.Id FROM  InventoryReportBills as a " +
                        $"inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId and a.BusinessUserId = alls.BusinessUserId " +
                        $"and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc LIMIT 1) BeginDate, (select(IFNULL(ab.SmallQuantity, 0) + (IFNULL(ab.StrokeQuantity, 0) * p.StrokeQuantity) + (IFNULL(ab.BigQuantity, 0)) * p.BigQuantity) " +
                        $"FROM InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where {whereQuery} and " +
                        $"a.Id in (select a.Id FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId " +
                        $"and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc desc LIMIT 1) EndStoreQuantity, (select CONCAT(ab.BigQuantity, " +
                        $"CAST(b.Name AS CHAR), ab.StrokeQuantity, CAST(m.Name AS CHAR), ab.SmallQuantity,CAST(s.Name AS CHAR))  FROM InventoryReportBills as a inner join InventoryReportItems as ab " +
                        $"on a.Id = ab.InventoryReportBillId where {whereQuery} and a.Id in (select a.Id FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId " +
                        $"where a.TerminalId = alls.TerminalId and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc desc LIMIT 1) EndStoreQuantityConversion, " +
                        $"(select  a.CreatedOnUtc FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where {whereQuery} and " +
                        $"a.Id in (select a.Id FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId " +
                        $"and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId ) order by  a.CreatedOnUtc desc LIMIT 1) EndDate, " +
                        $"(select(sum(IFNULL(ab.SmallQuantity, 0)) + (sum(IFNULL(ab.StrokeQuantity, 0)) * p.StrokeQuantity) + (sum(IFNULL(ab.BigQuantity, 0))) * p.BigQuantity) " +
                        $"FROM InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where {whereQuery} and a.Id in (select a.Id FROM  " +
                        $"InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId where a.TerminalId = alls.TerminalId " +
                        $"and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId )) PurchaseQuantity, (select CONCAT(sum(ab.BigQuantity), CAST(b.Name AS CHAR), " +
                        $"sum(ab.StrokeQuantity), CAST(m.Name AS CHAR), sum(ab.SmallQuantity), CAST(s.Name AS CHAR))  FROM InventoryReportBills as a inner join InventoryReportItems as ab " +
                        $"on a.Id = ab.InventoryReportBillId where {whereQuery} and a.Id in (select a.Id FROM  InventoryReportBills as a inner join InventoryReportItems as ab on a.Id = ab.InventoryReportBillId " +
                        $"where a.TerminalId = alls.TerminalId and a.BusinessUserId = alls.BusinessUserId and ab.ProductId = alls.ProductId )) PurchaseQuantityConversion,(SELECT ac.ManufactureDete " +
                        $"FROM InventoryReportBills a INNER JOIN InventoryReportItems ab ON a.Id = ab.InventoryReportBillId INNER JOIN InventoryReportStoreQuantities ac ON ab.Id = ac.InventoryReportItemId " +
                        $"WHERE { whereQuery} AND a.Id IN(SELECT a.Id FROM InventoryReportBills a INNER JOIN InventoryReportItems ab ON a.Id = ab.InventoryReportBillId WHERE a.TerminalId = alls.TerminalId AND " +
                        $"a.BusinessUserId = alls.BusinessUserId AND ab.ProductId = alls.ProductId) ORDER BY a.CreatedOnUtc LIMIT 1) AS ManufactureDete,  0 SaleQuantity, '' SaleQuantityConversion " +
                        $"from(select a.StoreId, a.TerminalId, a.BusinessUserId, ab.ProductId from InventoryReportBills as a inner join  InventoryReportItems as ab on a.Id = ab.InventoryReportBillId " +
                        $"where {whereQuery} group by a.StoreId,a.TerminalId,a.BusinessUserId,ab.ProductId) as alls left join Products as p  on alls.ProductId = p.Id left join dcms_crm.CRM_Terminals " +
                        $"as t on alls.TerminalId = t.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id " +
                        $"left join SpecificationAttributeOptions as b on p.BigUnitId = b.id  where {whereQuery2}";

                        var datas = SaleBillsRepository_RO.QueryFromSql<InventoryReportList>(mySqlString).ToList();

                        var users = _userService.GetUsersByIds(store, datas.Select(s => s.BusinessUserId).ToArray());
                        datas.ForEach(r =>
                        {
                            r.BusinessUserName = users.Where(s => s.Id == r.BusinessUserId).Select(s => s.UserRealName).FirstOrDefault() ?? "";
                            r.SaleQuantity = r.BeginStoreQuantity + r.PurchaseQuantity - r.EndStoreQuantity;
                            r.SaleQuantityConversion = $"{Pexts.StockQuantityFormat(r.SaleQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                            r.PurchaseQuantityConversion= $"{Pexts.StockQuantityFormat(r.PurchaseQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";

                            r.BeginStoreQuantityConversion = $"{Pexts.StockQuantityFormat(r.BeginStoreQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                            r.EndStoreQuantityConversion = $"{Pexts.StockQuantityFormat(r.EndStoreQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                        });

                        reporting = datas;

                        return reporting;
                    });
            }
            catch (Exception ex)
            {
                return new List<InventoryReportList>();
            }
        }

        /// <summary>
        /// 门店库存上报表Api
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IPagedList<InventoryReportList> GetInventoryReportListApi(int? store, int? makeuserId, int? businessUserId, int? terminalId, int? channelId, int? rankId, int? districtId, int? productId, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            DateTime.TryParse(startTime?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(endTime?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            var reporting = new List<InventoryReportList>();

            var lists = GetInventoryReportList(store,
                businessUserId, 
                terminalId, "", 
                channelId, 
                rankId,
                districtId, 
                productId, 
                "", 
                startDate,
                endDate).ToList();

            var users = _userService.GetUsersByIds(store, lists.Select(s => s.BusinessUserId).ToArray());

            lists.ForEach(r =>
            {
                r.BusinessUserName = users.Where(s => s.Id == r.BusinessUserId).Select(s => s.UserRealName).First();
                r.SaleQuantity = r.BeginStoreQuantity + r.PurchaseQuantity - r.EndStoreQuantity;
                r.SaleQuantityConversion = $"{Pexts.StockQuantityFormat(r.SaleQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
            });

            reporting = lists.Select(s =>
            {
                return new InventoryReportList
                {
                    StoreId = s.StoreId,

                    ProductId = s.ProductId,
                    ProductCode = s.ProductCode,
                    ProductName = s.ProductName,
                    SmallBarCode = s.SmallBarCode,
                    StrokeBarCode = s.StrokeBarCode,
                    BigBarCode = s.BigBarCode,

                    SmallUnitId = s.SmallUnitId,
                    SmallUnitName = s.SmallUnitName,
                    StrokeUnitId = s.StrokeUnitId,
                    StrokeUnitName = s.StrokeUnitName,
                    BigUnitId = s.BigUnitId,
                    BigUnitName = s.BigUnitName,

                    BigQuantity = s.BigQuantity,
                    StrokeQuantity = s.StrokeQuantity,
                    UnitConversion = s.UnitConversion,

                    BeginStoreQuantity = s.BeginStoreQuantity,
                    BeginStoreQuantityConversion = s.BeginStoreQuantityConversion,
                    BeginDate = s.BeginDate,
                    EndStoreQuantity = s.EndStoreQuantity,
                    EndStoreQuantityConversion = s.EndStoreQuantityConversion,
                    EndDate = s.EndDate,

                    PurchaseQuantity = s.PurchaseQuantity,
                    PurchaseQuantityConversion = s.PurchaseQuantityConversion,
                    SaleQuantity = s.SaleQuantity,
                    SaleQuantityConversion = s.SaleQuantityConversion,

                    TerminalId = s.TerminalId,
                    TerminalName = s.TerminalName,

                    BusinessUserId = s.BusinessUserId,
                    BusinessUserName = s.BusinessUserName

                };
            }).ToList();

            return new PagedList<InventoryReportList>(reporting, pageIndex, pageSize);
        }

        /// <summary>
        /// 门店库存上报汇总表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="channelId"></param>
        /// <param name="rankId"></param>
        /// <param name="districtId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<InventoryReportList> GetInventoryReportSummaryList(int? store, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                var key = DCMSDefaults.STOCKREPORTSERVICE_GETINVENTORYREPORTSUMMARYLIST_KEY.FillCacheKey(store, channelId, rankId, districtId, productId, productName, startTime, endTime);

                return _cacheManager.Get(key, () =>
                  {
                      productName = CommonHelper.Filter(productName);

                      var reporting = new List<InventoryReportList>();

                      string whereQuery = $" a.StoreId = {store ?? 0} ";

                      if (productId.HasValue && productId.Value != 0)
                      {
                          whereQuery += $" and a.ProductId = '{productId}' ";
                      }
                      if (productName != null)
                      {
                          whereQuery += $" and p.Name like '%{productName}%' ";
                      }
                      if (districtId.HasValue && districtId.Value != 0)
                      {
                          //递归片区查询
                          var distinctIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);
                          if (distinctIds != null && distinctIds.Count > 0)
                          {
                              string inDistinctIds = string.Join("','", distinctIds);
                              whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
                          }
                          else
                          {
                              whereQuery += $" and t.DistrictId = '{districtId}' ";
                          }
                      }

                      if (rankId.HasValue && rankId.Value != 0)
                      {
                          whereQuery += $" and t.RankId = '{rankId}' ";
                      }

                      if (channelId.HasValue && channelId.Value != 0)
                      {
                          whereQuery += $" and t.ChannelId = '{channelId}' ";
                      }


                      //string sqlString =$"select a.TerminalId,t.Name as TerminalName,a.BusinessUserId,'' as BusinessUserName, a.ProductId,p.Name as ProductName,p.Sku as ProductCode,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,UnitConversion = CONCAT('1', CAST(b.Name AS nvarchar(max)), '=', p.BigQuantity, CAST(s.Name AS nvarchar(max))),p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName,a.BeginStoreQuantity,'' as BeginStoreQuantityConversion,a.BeginDate,a.EndStoreQuantity,'' as EndStoreQuantityConversion,a.EndDate,a.PurchaseQuantity,'' as PurchaseQuantityConversion,a.SaleQuantity,'' SaleQuantityConversion from InventoryReportSummaries as a inner join  Products as p on a.ProductId = p.Id inner join dcms_crm.CRM_Terminals as t on a.TerminalId = t.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id  left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery} ";


                      //MYSQL
                      string sqlString = $"select a.StoreId, a.TerminalId,t.Name as TerminalName,a.BusinessUserId,'' as BusinessUserName, a.ProductId,p.Name as ProductName,p.Sku as ProductCode,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode, CONCAT('1', CAST(b.Name AS char), '=', p.BigQuantity, CAST(s.Name AS char)) as UnitConversion,p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName,a.BeginStoreQuantity,'' as BeginStoreQuantityConversion,a.BeginDate,a.EndStoreQuantity,'' as EndStoreQuantityConversion,a.EndDate,a.PurchaseQuantity,'' as PurchaseQuantityConversion,a.SaleQuantity,'' SaleQuantityConversion, p.BigUnitId as BigUnitId, p.StrokeUnitId as StrokeUnitId, p.SmallUnitId as SmallUnitId,,NULL ManufactureDete from InventoryReportSummaries as a inner join  Products as p on a.ProductId = p.Id inner join dcms_crm.CRM_Terminals as t on a.TerminalId = t.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id  left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery}";


                      var datas = SaleBillsRepository_RO.QueryFromSql<InventoryReportList>(sqlString).ToList();

                      if (startTime.HasValue)
                      {
                          datas = datas.Where(s => s.BeginDate >= startTime).ToList();
                      }
                      if (endTime.HasValue)
                      {
                          datas = datas.Where(s => s.BeginDate <= endTime).ToList();
                      }
                      datas.ForEach(r =>
                      {
                          r.BeginStoreQuantityConversion = $"{Pexts.StockQuantityFormat(r.BeginStoreQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                          r.EndStoreQuantityConversion = $"{Pexts.StockQuantityFormat(r.EndStoreQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                          r.PurchaseQuantityConversion = $"{Pexts.StockQuantityFormat(r.PurchaseQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";

                          r.SaleQuantity = r.BeginStoreQuantity + r.PurchaseQuantity - r.EndStoreQuantity;
                          r.SaleQuantityConversion = $"{Pexts.StockQuantityFormat(r.SaleQuantity, r.StrokeQuantity ?? 0, r.BigQuantity ?? 0, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                      });

                      reporting = datas;

                      return reporting;
                  });
            }
            catch (Exception ex)
            {
                return new List<InventoryReportList>();
            }
        }

        /// <summary>
        /// 调拨明细表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="shipmentWareHouseId"></param>
        /// <param name="incomeWareHouseId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="billNumber"></param>
        /// <param name="StatusId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<AllocationDetailsList> GetAllocationDetails(int? store, int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, string billNumber = "", int? StatusId = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.STOCKREPORTSERVICE_GETALLOCATIONDETAILS_KEY.FillCacheKey(store, shipmentWareHouseId, incomeWareHouseId, productId, productName, categoryId, billNumber, StatusId, startTime, endTime), () =>
                   {
                       productName = CommonHelper.Filter(productName);
                       billNumber = CommonHelper.Filter(billNumber);

                       var reporting = new List<AllocationDetailsList>();

                       string whereQuery = $" a.StoreId = {store ?? 0} ";

                       if (shipmentWareHouseId.HasValue && shipmentWareHouseId.Value != 0)
                       {
                           whereQuery += $" and a.ShipmentWareHouseId = '{shipmentWareHouseId}' ";
                       }

                       if (incomeWareHouseId.HasValue && incomeWareHouseId.Value != 0)
                       {
                           whereQuery += $" and a.IncomeWareHouseId = '{incomeWareHouseId}' ";
                       }

                       if (productId.HasValue && productId.Value != 0)
                       {
                           whereQuery += $" and b.ProductId = '{productId}' ";
                       }

                       if (productName != null)
                       {
                           whereQuery += $" and p.Name like '%{productName}%' ";
                       }

                       if (categoryId.HasValue && categoryId.Value != 0)
                       {
                           //递归商品类别查询
                           var categoryIds = _categoryService.GetSubCategoryIds(store ?? 0, categoryId ?? 0);
                           if (categoryIds != null && categoryIds.Count > 0)
                           {
                               string incategoryIds = string.Join("','", categoryIds);
                               whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and p.CategoryId = '{categoryId}' ";
                           }
                       }

                       if (!string.IsNullOrEmpty(billNumber))
                       {
                           whereQuery += $" and a.BillNumber like '%{billNumber}%' ";
                       }

                       if (StatusId.HasValue)
                       {
                           whereQuery += $" and a.AuditedStatus = '{StatusId}' ";
                       }

                       if (startTime.HasValue)
                       {
                           startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                           whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                       }

                       if (endTime.HasValue)
                       {
                           endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                           whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                       }

                       whereQuery += $" order by a.CreatedOnUtc desc ";

                       //MYSQL
                       string mySqlString = $"select a.Id,a.StoreId,a.ShipmentWareHouseId,a.IncomeWareHouseId,(select name from WareHouses where a.shipmentwarehouseid = id) ShipmentWareHouseName,(select name from WareHouses where a.incomewarehouseid = id) IncomeWareHouseName,  a.BillNumber,a.AuditedDate, a.AuditedStatus, a.AuditedUserId, a.CreatedOnUtc, b.ProductId, b.Quantity,b.UnitId, {CommonHelper.GetSqlUnitConversion("p")} as UnitConversion,b.OutgoingStock,b.WarehousingStock,p.name as ProductName,p.CategoryId, p.SmallBarCode as BarCode,p.BrandId as BrandId, 0 Price,'' as PriceName,p.Sku as ProductSKU,p.BigQuantity as QuantityConversion,(case when b.UnitId=p.SmallUnitId then pa1.Name when b.UnitId=p.StrokeUnitId then pa2.Name when b.UnitId=p.BigUnitId then pa3.Name else '' end ) as UnitName from AllocationBills a inner join AllocationItems b on a.id = b.allocationbillid inner join Products p on b.productid = p.id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery}";

                       var datas = SaleBillsRepository_RO.QueryFromSql<AllocationDetailsList>(mySqlString).ToList();
                       reporting = datas;
                       return reporting;
                   });
            }
            catch (Exception)
            {
                return new List<AllocationDetailsList>();
            }
        }

        public IList<AllocationDetailsList> GetAllocationDetailsByProducts(int? store, int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, DateTime? startTime = null, DateTime? endTime = null)
        {

            try
            {
                return _cacheManager.Get(DCMSDefaults.STOCKREPORTSERVICE_GETALLOCATIONDETAILSBYPRODUCTS_KEY.FillCacheKey(store, shipmentWareHouseId, incomeWareHouseId, productId, productName, categoryId, startTime, endTime), () =>
                  {
                      productName = CommonHelper.Filter(productName);

                      var reporting = new List<AllocationDetailsList>();

                      string whereQuery = $" a.StoreId = {store ?? 0} ";

                      if (shipmentWareHouseId.HasValue && shipmentWareHouseId.Value != 0)
                      {
                          whereQuery += $" and a.ShipmentWareHouseId = '{shipmentWareHouseId}' ";
                      }

                      if (incomeWareHouseId.HasValue && incomeWareHouseId.Value != 0)
                      {
                          whereQuery += $" and a.IncomeWareHouseId = '{incomeWareHouseId}' ";
                      }

                      if (productId.HasValue && productId.Value != 0)
                      {
                          whereQuery += $" and c.Id = '{productId}' ";
                      }

                      if (productName != null)
                      {
                          whereQuery += $" and c.name like '%{productName}%' ";
                      }

                      if (categoryId.HasValue && categoryId.Value != 0)
                      {
                          //递归商品类别查询
                          var categoryIds = _categoryService.GetSubCategoryIds(store ?? 0, categoryId ?? 0);
                          if (categoryIds != null && categoryIds.Count > 0)
                          {
                              string incategoryIds = string.Join("','", categoryIds);
                              whereQuery += $" and c.CategoryId in ('{incategoryIds}') ";
                          }
                          else
                          {
                              whereQuery += $" and c.CategoryId = '{categoryId}' ";
                          }
                      }

                      if (startTime.HasValue)
                      {
                          startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                          whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                      }

                      if (endTime.HasValue)
                      {
                          endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                          whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                      }

                      //MSSQL
                      //string sqlString = $"select a.Id,a.StoreId,a.ShipmentWareHouseId,a.IncomeWareHouseId,(select name from warehouses where a.shipmentwarehouseid=id) ShipmentWareHouseName,(select name from warehouses where a.incomewarehouseid=id) IncomeWareHouseName,  a.BillNumber,a.AuditedDate, a.AuditedStatus, a.AuditedUserId, a.CreatedOnUtc, b.ProductId, b.Quantity,b.UnitId, UnitConversion = CONCAT('1', CAST(c.Name AS nvarchar(max)), '=', c.BigQuantity, CAST(c.Name AS nvarchar(max))),b.OutgoingStock,b.WarehousingStock,c.name as ProductName,c.CategoryId from AllocationBills a inner join AllocationItems b on a.id = b.allocationbillid inner join Products c on b.productid = c.id where {whereQuery}";

                      //MYSQL
                      string mySqlString = $"select a.Id,a.StoreId,a.ShipmentWareHouseId,a.IncomeWareHouseId,(select name from WareHouses where a.shipmentwarehouseid = id) ShipmentWareHouseName,(select name from WareHouses where a.incomewarehouseid = id) IncomeWareHouseName,  a.BillNumber,a.AuditedDate, a.AuditedStatus, a.AuditedUserId, a.CreatedOnUtc, b.ProductId, b.Quantity,b.UnitId, CONCAT('1', CAST(c.Name AS char), '=', c.BigQuantity, CAST(c.Name AS char)) UnitConversion,b.OutgoingStock,b.WarehousingStock,c.name as ProductName,c.CategoryId, c.SmallBarCode as BarCode,c.BrandId as BrandId,0 as Price,'' as PriceName,c.Sku as ProductSKU,c.BigQuantity as QuantityConversion,'' as UnitName from AllocationBills a inner join AllocationItems b on a.id = b.allocationbillid inner join Products c on b.productid = c.id where {whereQuery}";

                      var datas = SaleBillsRepository_RO.QueryFromSql<AllocationDetailsList>(mySqlString).ToList();
                      reporting = datas;
                      return reporting;
                  });
            }
            catch (Exception)
            {
                return new List<AllocationDetailsList>();
            }
        }

        /// <summary>
        /// 库存滞销报表API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="brandIds"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<StockUnsalable> GetStockUnsalableAPI(int? storeId, int[] brandIds, int? productId, int? categoryId, DateTime? startTime = null, DateTime? endTime = null)
        {
            var reporting = new List<StockUnsalable>();

            try
            {

                if (startTime.HasValue)
                {
                    startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                }

                if (endTime.HasValue)
                {
                    endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                }

                //（1）获取可用库存商品
                var query = from p in ProductsRepository_RO.Table
                            join stk in StocksRepository_RO.Table on p.Id equals stk.ProductId
                            where p.StoreId == storeId && stk.UsableQuantity > 0
                            select p;

                if (brandIds != null && brandIds.Length > 0 && !brandIds.Contains(0))
                {
                    query = query.Where(p => brandIds.Contains(p.BrandId));
                }

                if (categoryId.HasValue && categoryId != 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        query = query.Where(p => categoryIds.Contains(p.CategoryId));
                    }
                    else
                    {
                        query = query.Where(p => p.CategoryId == categoryId);
                    }
                }

                //去重
                query = from p in query group p by p.Id into pGroup orderby pGroup.Key select pGroup.FirstOrDefault();

                var allProducts = query.ToList();
                var productIds = allProducts.Select(sd => sd.Id).ToArray();
                var uctos = _productService.UnitConversions(storeId, allProducts, productIds);

                var products = allProducts.Select(p =>
                {
                    var utc = uctos.Where(s => s.Product.Id == p.Id).FirstOrDefault();
                    var sqt = p.Stocks.Select(s => s.UsableQuantity ?? 0).Sum();
                    return new StockUnsalable()
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        ProductCode = p.ProductCode,
                        SmallBarCode = p.SmallBarCode,
                        StrokeBarCode = p.StrokeBarCode,
                        BigBarCode = p.BigBarCode,
                        UnitConversion = utc != null ? utc.Option.smallOption.UnitConversion : "",
                        StockQuantity = p.Stocks.Select(s => s.UsableQuantity ?? 0).Sum(),
                        StockQuantityConversion = p.StockQuantityFormat(utc.Option, sqt),
                        SaleQuantity = 0,
                        SaleQuantityConversion = "",
                        ReturnQuantity = 0,
                        ReturnQuantityConversion = "",
                        NetQuantity = 0,
                        NetQuantityConversion = "",
                        SaleAmount = 0,
                        ReturnAmount = 0,
                        NetAmount = 0
                    };
                }).ToList();


                //（2）获取指定时段可用库存商品的销售情况
                var sbs = from p in SaleBillsRepository_RO.Table where p.StoreId == storeId select p;
                if (startTime.HasValue)
                {
                    sbs = sbs.Where(p => p.CreatedOnUtc >= startTime);
                }

                if (endTime.HasValue)
                {
                    sbs = sbs.Where(p => p.CreatedOnUtc <= endTime);
                }
                //销售项目
                var saleItems = new List<SaleItem>();
                sbs.ToList().ForEach(sb =>
                {
                    saleItems.AddRange(sb.Items);
                });
                var allspds = saleItems.Where(s => productIds.Contains(s.ProductId));


                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allspds.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var saleQuantity = curps.Select(s => s.Quantity).Sum();
                    var saleAmount = curps.Select(s => s.Amount).Sum();

                    p.SaleQuantity = saleQuantity;
                    p.SaleQuantityConversion = cup.StockQuantityFormat(utc.Option, saleQuantity);
                    p.SaleAmount = saleAmount;

                    p.ReturnQuantity = 0;
                    p.ReturnQuantityConversion = "";
                    p.ReturnAmount = 0;

                    p.NetQuantity = 0;
                    p.NetQuantityConversion = "";
                    p.NetAmount = 0;
                });


                //（3）获取指定时段可用库存商品的退货情况
                var rbs = from p in ReturnBillsRepository_RO.Table where p.StoreId == storeId select p;
                if (startTime.HasValue)
                {
                    rbs = rbs.Where(p => p.CreatedOnUtc >= startTime);
                }

                if (endTime.HasValue)
                {
                    rbs = rbs.Where(p => p.CreatedOnUtc <= endTime);
                }
                //退货项目
                var returnItems = new List<ReturnItem>();
                rbs.ToList().ForEach(sb =>
                {
                    returnItems.AddRange(sb.Items);
                });
                var allrps = returnItems.Where(s => productIds.Contains(s.ProductId));
                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allrps.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var returnQuantity = curps.Select(s => s.Quantity).Sum();
                    var returnAmount = curps.Select(s => s.Amount).Sum();

                    p.ReturnQuantity = returnQuantity;
                    p.ReturnQuantityConversion = cup.StockQuantityFormat(utc.Option, returnQuantity);
                    p.ReturnAmount = returnAmount;

                    p.NetQuantity = 0;
                    p.NetQuantityConversion = "";
                    p.NetAmount = 0;
                });


                //（3）计算净额
                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allspds.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var netQuantity = p.SaleQuantity - p.ReturnQuantity;
                    p.NetQuantity = netQuantity;
                    p.NetQuantityConversion = cup.StockQuantityFormat(utc.Option, netQuantity);
                    p.NetAmount = p.SaleAmount - p.ReturnAmount;
                });

                //排序
                reporting = products.OrderByDescending(s => s.NetAmount).ToList();
                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 库存滞销报表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="lessNetSaleQuantity"></param>
        /// <returns></returns>
        public IList<StockUnsalable> GetStockUnsalable(int? storeId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null, int? wareHouseId = 0, int? brandId = 0, int? categoryId = 0,
              int? lessNetSaleQuantity = 0)
        {
            productName = CommonHelper.Filter(productName);

            var reporting = new List<StockUnsalable>();

            try
            {

                if (startTime.HasValue)
                {
                    startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                }

                if (endTime.HasValue)
                {
                    endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                }

                //1.获取库存表可用库存商品
                var queryStock = StocksRepository_RO.Table;
                queryStock = queryStock.Where(q => q.StoreId == storeId);
                queryStock = queryStock.Where(q => q.UsableQuantity > 0);
                if (wareHouseId != null && wareHouseId > 0)
                {
                    queryStock = queryStock.Where(q => q.WareHouseId == wareHouseId);
                }
                var stockProductIds = queryStock.Select(q => q.ProductId).Distinct().ToList();
                //2.根据库存表的商品，查询商品信息
                var query = from p in ProductsRepository_RO.Table
                            where p.StoreId == storeId && stockProductIds.Contains(p.Id)
                            select p;

                if (categoryId.HasValue && categoryId != 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        query = query.Where(p => categoryIds.Contains(p.CategoryId));
                    }
                    else
                    {
                        query = query.Where(p => p.CategoryId == categoryId);
                    }
                }

                //去重
                //query = from p in query group p by p.Id into pGroup orderby pGroup.Key select pGroup.FirstOrDefault();
                //query = query.GroupBy(n => n.Id).Select(n => n.First());

                var allProducts = query.ToList();
                var productIds = allProducts.Select(sd => sd.Id).ToArray();
                var uctos = _productService.UnitConversions(storeId, allProducts, productIds);

                //3.查询库存数量
                var queryStocks = StocksRepository_RO.Table;
                queryStocks = queryStocks.Where(q => productIds.Contains(q.ProductId));
                if (wareHouseId != null && wareHouseId > 0)
                {
                    queryStocks = queryStocks.Where(q => q.WareHouseId == wareHouseId);
                }
                var allStocks = queryStocks.ToList();

                var products = allProducts.Select(p =>
                {
                    var utc = uctos.Where(s => s.Product.Id == p.Id).FirstOrDefault();

                    utc.Option.smallOption.ConvertedQuantity = 1;
                    utc.Option.strokOption.ConvertedQuantity = p.StrokeQuantity;
                    utc.Option.bigOption.ConvertedQuantity = p.BigQuantity;

                    utc.Option.smallOption.UnitConversion = string.Format("1{0} = {1}{2}", utc.Option.bigOption != null ? utc.Option.bigOption.Name : "/", p.BigQuantity, utc.Option.smallOption != null ? utc.Option.smallOption.Name : "/");

                    var sqt = allStocks.Where(a => a.ProductId == p.Id).Select(a => a.UsableQuantity ?? 0).Sum();
                    return new StockUnsalable()
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        ProductCode = p.ProductCode,
                        SmallBarCode = p.SmallBarCode,
                        StrokeBarCode = p.StrokeBarCode,
                        BigBarCode = p.BigBarCode,
                        UnitConversion = utc != null ? utc.Option.smallOption.UnitConversion : "",
                        StockQuantity = sqt,
                        StockQuantityConversion = p.StockQuantityFormat(utc.Option, sqt),
                        SaleQuantity = 0,
                        SaleQuantityConversion = "",
                        ReturnQuantity = 0,
                        ReturnQuantityConversion = "",
                        NetQuantity = 0,
                        NetQuantityConversion = "",
                        SaleAmount = 0,
                        ReturnAmount = 0,
                        NetAmount = 0
                    };
                }).ToList();


                //4.获取指定时段可用库存商品的销售情况
                var sbs = from p in SaleBillsRepository_RO.Table.Include(s=>s.Items) where p.StoreId == storeId select p;
                if (startTime.HasValue)
                {
                    sbs = sbs.Where(p => p.CreatedOnUtc >= startTime);
                }

                if (endTime.HasValue)
                {
                    sbs = sbs.Where(p => p.CreatedOnUtc <= endTime);
                }
                //销售项目
                var saleItems = new List<SaleItem>();
                sbs.ToList().ForEach(sb =>
                {
                    saleItems.AddRange(sb.Items);
                });
                var allspds = saleItems.Where(s => productIds.Contains(s.ProductId));

                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allspds.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var saleQuantity = curps.Select(s => s.Quantity).Sum();
                    var saleAmount = curps.Select(s => s.Amount).Sum();

                    p.SaleQuantity = saleQuantity;
                    p.SaleQuantityConversion = cup.StockQuantityFormat(utc.Option, saleQuantity);
                    p.SaleAmount = saleAmount;

                    p.ReturnQuantity = 0;
                    p.ReturnQuantityConversion = "";
                    p.ReturnAmount = 0;

                    p.NetQuantity = 0;
                    p.NetQuantityConversion = "";
                    p.NetAmount = 0;
                });


                //5.获取指定时段可用库存商品的退货情况
                var rbs = from p in ReturnBillsRepository_RO.Table.Include(r=>r.Items) where p.StoreId == storeId select p;
                if (startTime.HasValue)
                {
                    rbs = rbs.Where(p => p.CreatedOnUtc >= startTime);
                }

                if (endTime.HasValue)
                {
                    rbs = rbs.Where(p => p.CreatedOnUtc <= endTime);
                }
                //退货项目
                var returnItems = new List<ReturnItem>();
                rbs.ToList().ForEach(sb =>
                {
                    returnItems.AddRange(sb.Items);
                });
                var allrps = returnItems.Where(s => productIds.Contains(s.ProductId));
                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allrps.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var returnQuantity = curps.Select(s => s.Quantity).Sum();
                    var returnAmount = curps.Select(s => s.Amount).Sum();

                    p.ReturnQuantity = returnQuantity;
                    p.ReturnQuantityConversion = cup.StockQuantityFormat(utc.Option, returnQuantity);
                    p.ReturnAmount = returnAmount;

                    p.NetQuantity = 0;
                    p.NetQuantityConversion = "";
                    p.NetAmount = 0;
                });


                //6.计算净额
                products.ForEach(p =>
                {
                    //cup 不可能为空
                    var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allspds.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var netQuantity = p.SaleQuantity - p.ReturnQuantity;
                    p.NetQuantity = netQuantity;
                    p.NetQuantityConversion = cup.StockQuantityFormat(utc.Option, netQuantity);
                    p.NetAmount = p.SaleAmount - p.ReturnAmount;
                });

                //净销数量过滤
                if (lessNetSaleQuantity != null && lessNetSaleQuantity > 0)
                {
                    products = products.Where(p => p.NetQuantity < lessNetSaleQuantity).ToList();
                }
                if (productId != null && productId != 0)
                {
                    products = products.Where(s => s.ProductId == productId).ToList();
                }
                if (productName != null)
                {
                    products = products.Where(s => s.ProductName.Contains(productName)).ToList();
                }
                //排序
                reporting = products.OrderByDescending(s => s.NetAmount).ToList();
                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 库存预警表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="brandId"></param>
        /// <param name="unitShowTypeId"></param>
        /// <returns></returns>
        public IList<EarlyWarning> GetEarlyWarning(int? storeId, int? wareHouseId = 0, int? categoryId = 0, int? brandId = 0, int? unitShowTypeId = 1)
        {
            try
            {
                var key = DCMSDefaults.MAINPAGEREPORTSERVICE_GETEARLYWARNING_KEY.FillCacheKey(storeId, wareHouseId, categoryId, brandId, unitShowTypeId);
                key.CacheTime = 1;
                return _cacheManager.Get(key, () =>
                   {
                       var reporting = new List<EarlyWarning>();

                       string whereQuery = $" p.StoreId = {storeId ?? 0} ";

                       if (wareHouseId.HasValue && wareHouseId.Value != 0)
                       {
                           whereQuery += $" and ps.WareHouseId = '{wareHouseId}' ";
                       }

                       if (brandId.HasValue && brandId.Value != 0)
                       {
                           whereQuery += $" and ps.ProductId = '{brandId}' ";
                       }

                       if (categoryId.HasValue && categoryId.Value != 0)
                       {
                           //递归商品类别查询
                           var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                           if (categoryIds != null && categoryIds.Count > 0)
                           {
                               string incategoryIds = string.Join("','", categoryIds);
                               whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and p.CategoryId = '{categoryId}' ";
                           }
                       }

                       if (unitShowTypeId.HasValue)
                       {
                           if (unitShowTypeId.Value == 0)
                           {
                               whereQuery += $" and s.Name is not null ";
                           }
                           else if (unitShowTypeId.Value == 1)
                           {
                               whereQuery += $" and b.Name is not null ";
                           }
                       }

                       //string sqlString = $" select ps.WareHouseId,ps.ProductId,p.CategoryId,  ISNULL(p.Sku ,'') as ProductCode,p.Name as ProductName,ISNULL(p.SmallBarCode,'') as SmallBarCode,ISNULL(p.StrokeBarCode,'') as StrokeBarCode,ISNULL(p.BigBarCode,'') as BigBarCode,UnitConversion = CONCAT('1', CAST(s.Name AS nvarchar(max)), '=', p.BigQuantity, CAST(b.Name AS nvarchar(max))),ISNULL(p.ExpirationDays,0) as ExpirationDays,ISNULL(p.AdventDays,0) as AdventDays,p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName,";

                       //sqlString += $"  StockQuantity = isnull((select sum(isnull(st.CurrentQuantity, 0)) as StockQuantity  from Stocks st  inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId  group by st.ProductId,st.StoreId,st.WareHouseId,w.Name ),0),";

                       //sqlString += $"  LessQuantity = isnull((select MoreQuantity = ISNULL((select (case ISNULL(sw.ShortageWarningQuantity, 0) when 0 then 0 else (ISNULL(sw.ShortageWarningQuantity, 0) - sum(isnull(st.CurrentQuantity, 0))) end)   from StockEarlyWarnings as sw where sw.StoreId = {storeId ?? 0}  and sw.WareHouseId = ps.WareHouseId  and sw.ProductId = ps.ProductId),0) from Stocks st  inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId  group by st.ProductId,st.StoreId,st.WareHouseId,w.Name),0),  ";

                       //sqlString += $"  MoreQuantity = isnull((select MoreQuantity = sum(isnull(st.CurrentQuantity, 0)) - ISNULL((select ISNULL(sw.BacklogWarningQuantity, 0) from StockEarlyWarnings as sw where sw.StoreId = {storeId ?? 0}  and sw.WareHouseId = ps.WareHouseId  and sw.ProductId = ps.ProductId),0) from Stocks st  inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId  group by st.ProductId,st.StoreId,st.WareHouseId,w.Name),0)     ";

                       //sqlString += $"   from(select a.WareHouseId, ab.ProductId from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id where a.StoreId = {storeId ?? 0}  group by a.WareHouseId, ab.ProductId) as ps left join Products as p on ps.ProductId = p.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery} ";

                       string mySqlString = $"select p.StoreId, ps.WareHouseId,ps.ProductId,p.CategoryId,  IFNULL(p.Sku ,'') as ProductCode,p.Name as ProductName,IFNULL(p.SmallBarCode, '') as SmallBarCode,IFNULL(p.StrokeBarCode, '') as StrokeBarCode,IFNULL(p.BigBarCode, '') as BigBarCode,CONCAT('1', CAST(s.Name AS CHAR), '=', p.BigQuantity, CAST(b.Name AS CHAR)) UnitConversion,IFNULL(p.ExpirationDays, 0) as ExpirationDays,IFNULL(p.AdventDays, 0) as AdventDays,p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName, IFNULL((select sum(IFNULL(st.CurrentQuantity, 0)) as StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId group by st.ProductId, st.StoreId, st.WareHouseId, w.Name),0) StockQuantity, '' as StockQuantityConversion,  IFNULL((select IFNULL((select(case IFNULL(sw.ShortageWarningQuantity, 0) when 0 then 0 else (IFNULL(sw.ShortageWarningQuantity, 0) - sum(IFNULL(st.CurrentQuantity, 0))) end)  from StockEarlyWarnings as sw where sw.StoreId = {storeId ?? 0} and sw.WareHouseId = ps.WareHouseId  and sw.ProductId = ps.ProductId),0) MoreQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId group by st.ProductId,st.StoreId,st.WareHouseId,w.Name),0) LessQuantity, '' as LessQuantityConversion, IFNULL((select sum(IFNULL(st.CurrentQuantity, 0)) - IFNULL((select IFNULL(sw.BacklogWarningQuantity, 0) from StockEarlyWarnings as sw where sw.StoreId = {storeId ?? 0}  and sw.WareHouseId = ps.WareHouseId  and sw.ProductId = ps.ProductId), 0) MoreQuantity  from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.WareHouseId = ps.WareHouseId and st.ProductId = ps.ProductId group by st.ProductId,st.StoreId,st.WareHouseId,w.Name),0) MoreQuantity, '' as MoreQuantityConversion from(select a.WareHouseId, ab.ProductId from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id where a.StoreId = {storeId ?? 0} group by a.WareHouseId, ab.ProductId) as ps left join Products as p on ps.ProductId = p.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery} ";

                       var datas = SaleBillsRepository_RO.QueryFromSql<EarlyWarning>(mySqlString).ToList();

                       var stockEarnings = StockEarlyWarningsRepository_RO.Table.Where(s => s.StoreId == storeId).ToList();

                       var allProducts = _productService.GetProductsByIds(storeId??0, datas.Select(s => s.ProductId??0)?.Distinct()?.ToArray());

                       datas.ForEach(r =>
                       {
                           var warning = stockEarnings.FirstOrDefault(e => e.ProductId == r.ProductId && r.WareHouseId == r.WareHouseId);

                           var product = allProducts.FirstOrDefault(p=>p.Id==r.ProductId);

                           if (warning != null)
                           {
                               if (warning.UnitId == product.BigUnitId) //大单位转小单位
                               {
                                   warning.BacklogWarningQuantity = warning.BacklogWarningQuantity * product.BigQuantity ?? 0;
                                   warning.ShortageWarningQuantity = warning.ShortageWarningQuantity * product.BigQuantity ?? 0;
                               }
                               else if (warning.UnitId == product.StrokeUnitId) //中单位转小单位
                               {
                                   warning.BacklogWarningQuantity = warning.BacklogWarningQuantity * product.StrokeQuantity ?? 0;
                                   warning.ShortageWarningQuantity = warning.ShortageWarningQuantity * product.StrokeQuantity ?? 0;
                               }


                               if (warning.ShortageWarningQuantity - r.StockQuantity > 0)
                               {
                                   r.LessQuantity = warning.ShortageWarningQuantity - r.StockQuantity;
                               }
                               else
                               {
                                   r.LessQuantity = 0;
                               }

                               if (r.StockQuantity - warning.BacklogWarningQuantity > 0)
                               {
                                   r.MoreQuantity = r.StockQuantity - warning.BacklogWarningQuantity;
                               }
                               else
                               {
                                   r.MoreQuantity = 0;
                               }
                           }

                           if (unitShowTypeId.Value == 0)
                           {
                               r.StockQuantityConversion = r.StockQuantity + r.SmallUnitName;
                               r.LessQuantityConversion = r.LessQuantity + r.SmallUnitName;
                               r.MoreQuantityConversion = r.MoreQuantity + r.SmallUnitName;
                           }
                           else
                           {
                               r.StockQuantityConversion = $"{Pexts.StockQuantityFormat(r.StockQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                               r.LessQuantityConversion = $"{Pexts.StockQuantityFormat(r.LessQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                               r.MoreQuantityConversion = $"{Pexts.StockQuantityFormat(r.MoreQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                           }
                       });

                       reporting = datas;


                       return reporting;
                   });
            }
            catch (Exception)
            {
                return new List<EarlyWarning>();
            }
        }


        /// <summary>
        /// 临期预警表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IList<ExpirationWarning> GetExpirationWarning(int? storeId, int? wareHouseId = 0, int? categoryId = 0, int? productId = 0, string productName = null)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.MAINPAGEREPORTSERVICE_GETEXPIRATIONWARNING_KEY.FillCacheKey(storeId, wareHouseId, categoryId, productId, productName), () =>
                   {
                       productName = CommonHelper.Filter(productName);

                       var reporting = new List<ExpirationWarning>();

                       string whereQuery = $" p.StoreId = {storeId ?? 0} ";

                       if (wareHouseId.HasValue && wareHouseId.Value != 0)
                       {
                           whereQuery += $" and ps.WareHouseId = '{wareHouseId}' ";
                       }

                       if (productId.HasValue && productId.Value != 0)
                       {
                           whereQuery += $" and ps.ProductId = '{productId}' ";
                       }
                       if (productName != null)
                       {
                           whereQuery += $" and p.Name like '%{productName}%' ";
                       }

                       if (categoryId.HasValue && categoryId.Value != 0)
                       {
                           //递归商品类别查询
                           var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                           if (categoryIds != null && categoryIds.Count > 0)
                           {
                               string incategoryIds = string.Join("','", categoryIds);
                               whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and p.CategoryId = '{categoryId}' ";
                           }
                       }

                       //string sqlString = $"select ps.WareHouseId,ps.ProductId,p.CategoryId,  ISNULL(p.Sku, '') as ProductCode,p.Name as ProductName,ISNULL(p.SmallBarCode, '') as SmallBarCode,ISNULL(p.StrokeBarCode, '') as StrokeBarCode,ISNULL(p.BigBarCode, '') as BigBarCode,UnitConversion = CONCAT('1', CAST(s.Name AS nvarchar(max)), '=', p.BigQuantity, CAST(b.Name AS nvarchar(max))),ISNULL(p.ExpirationDays, 0) as ExpirationDays,ISNULL(p.AdventDays, 0) as AdventDays,p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName,OneThirdDay = ISNULL(p.ExpirationDays, 0) * 1 / 3,TwoThirdDay = ISNULL(p.ExpirationDays, 0) * 2 / 3, ";

                       //sqlString += $" --//1/3 库存量 ";
                       //sqlString += $" OneThirdQuantity = (select otq.StockQuantity from(select isnull(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(isnull(sum(alls.SmallStockQuantity), 0), '小', isnull(sum(alls.StrokeStockQuantity), 0), '中', isnull(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId, SmallManufactureDete = case ab.UnitId when p.SmallUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,StrokeManufactureDete = case ab.UnitId when p.StrokeUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,BigManufactureDete = case ab.UnitId when p.BigUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,SmallManufactureDeteDiffDay = case ab.UnitId when p.SmallUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StrokeManufactureDeteDiffDay = case ab.UnitId when p.StrokeUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,BigManufactureDeteDiffDay = case ab.UnitId when p.BigUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StockQuantity = w.StockQuantity,SmallStockQuantity = (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) ,StrokeStockQuantity = (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ),BigStockQuantity = (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(isnull(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.StoreId = {storeId??0} group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where a.StoreId = {storeId ?? 0} and w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and((SmallManufactureDeteDiffDay >= 0 and SmallManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 1 / 3)   or(StrokeManufactureDeteDiffDay >= 0 and StrokeManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 1 / 3) or(BigManufactureDeteDiffDay >= 0 and BigManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 1 / 3))) as otq), ";

                       //sqlString += $" --//2/3 库存量 ";
                       //sqlString += $" TwoThirdQuantity = (select wtq.StockQuantity from(select isnull(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(isnull(sum(alls.SmallStockQuantity), 0), '小', isnull(sum(alls.StrokeStockQuantity), 0), '中', isnull(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId, SmallManufactureDete = case ab.UnitId when p.SmallUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,StrokeManufactureDete = case ab.UnitId when p.StrokeUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,BigManufactureDete = case ab.UnitId when p.BigUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,SmallManufactureDeteDiffDay = case ab.UnitId when p.SmallUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StrokeManufactureDeteDiffDay = case ab.UnitId when p.StrokeUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,BigManufactureDeteDiffDay = case ab.UnitId when p.BigUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StockQuantity = w.StockQuantity,SmallStockQuantity = (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) ,StrokeStockQuantity = (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ),BigStockQuantity = (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(isnull(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.StoreId = {storeId ?? 0} group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where a.StoreId = {storeId ?? 0} and w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and((SmallManufactureDeteDiffDay >= 0 and SmallManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3)   or(StrokeManufactureDeteDiffDay >= 0 and StrokeManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3) or(BigManufactureDeteDiffDay >= 0 and BigManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3))) as wtq), ";

                       //sqlString += $" --//预警量 ";
                       //sqlString += $" WarningQuantity = (select wqt.StockQuantity from(select isnull(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(isnull(sum(alls.SmallStockQuantity), 0), '小', isnull(sum(alls.StrokeStockQuantity), 0), '中', isnull(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId, SmallManufactureDete = case ab.UnitId when p.SmallUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,StrokeManufactureDete = case ab.UnitId when p.StrokeUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,BigManufactureDete = case ab.UnitId when p.BigUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,SmallManufactureDeteDiffDay = case ab.UnitId when p.SmallUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StrokeManufactureDeteDiffDay = case ab.UnitId when p.StrokeUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,BigManufactureDeteDiffDay = case ab.UnitId when p.BigUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StockQuantity = w.StockQuantity,SmallStockQuantity = (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) ,StrokeStockQuantity = (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ),BigStockQuantity = (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end),p.AdventDays from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(isnull(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId)as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and(alls.SmallManufactureDeteDiffDay >= alls.AdventDays or alls.StrokeManufactureDeteDiffDay >= alls.AdventDays or alls.BigManufactureDeteDiffDay >= alls.AdventDays)) as wqt), ";

                       //sqlString += $" --//过期量 ";
                       //sqlString += $" ExpiredQuantity = (select eqt.StockQuantity from(select isnull(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(isnull(sum(alls.SmallStockQuantity), 0), '小', isnull(sum(alls.StrokeStockQuantity), 0), '中', isnull(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId, SmallManufactureDete = case ab.UnitId when p.SmallUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,StrokeManufactureDete = case ab.UnitId when p.StrokeUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,BigManufactureDete = case ab.UnitId when p.BigUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,SmallManufactureDeteDiffDay = case ab.UnitId when p.SmallUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StrokeManufactureDeteDiffDay = case ab.UnitId when p.StrokeUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,BigManufactureDeteDiffDay = case ab.UnitId when p.BigUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StockQuantity = w.StockQuantity,SmallStockQuantity = (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) ,StrokeStockQuantity = (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ),BigStockQuantity = (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end),p.ExpirationDays from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(isnull(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId)as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and(alls.ExpirationDays <= alls.SmallManufactureDeteDiffDay or alls.ExpirationDays <= alls.StrokeManufactureDeteDiffDay or alls.ExpirationDays <= alls.BigManufactureDeteDiffDay)) as eqt)  TwoThirdQuantity = (select wtq.StockQuantity from(select isnull(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(isnull(sum(alls.SmallStockQuantity), 0), '小', isnull(sum(alls.StrokeStockQuantity), 0), '中', isnull(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId, SmallManufactureDete = case ab.UnitId when p.SmallUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,StrokeManufactureDete = case ab.UnitId when p.StrokeUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,BigManufactureDete = case ab.UnitId when p.BigUnitId then ISNULL(convert(varchar, ab.ManufactureDete, 120), '') else '' end,SmallManufactureDeteDiffDay = case ab.UnitId when p.SmallUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StrokeManufactureDeteDiffDay = case ab.UnitId when p.StrokeUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,BigManufactureDeteDiffDay = case ab.UnitId when p.BigUnitId then(case ISNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(day, ISNULL(convert(varchar, ab.ManufactureDete, 120), ''), GETDATE()) end) else -1 end,StockQuantity = w.StockQuantity,SmallStockQuantity = (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) ,StrokeStockQuantity = (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ),BigStockQuantity = (case when ISNULL(p.BigQuantity,0) = 0 then 0 else ISNULL(w.StockQuantity, 0) / ISNULL(p.BigQuantity, 0) end) from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(isnull(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.StoreId = {storeId ?? 0} group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where a.StoreId = {storeId ?? 0} and w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and((SmallManufactureDeteDiffDay >= 0 and SmallManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3)   or(StrokeManufactureDeteDiffDay >= 0 and StrokeManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3) or(BigManufactureDeteDiffDay >= 0 and BigManufactureDeteDiffDay <= ISNULL(p.ExpirationDays, 0) * 2 / 3))) as wtq), ";

                       //sqlString += $" from(select a.WareHouseId, ab.ProductId from PurchaseBills as a  inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id where a.StoreId = {storeId ?? 0} group by a.WareHouseId, ab.ProductId) as ps left join Products as p  on ps.ProductId = p.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery}";

                       //MYSQL
                       string mySqlString = $"select ps.WareHouseId, '' as WareHouseName, ps.ProductId,p.CategoryId,  IFNULL(p.Sku, '') as ProductCode,p.Name as ProductName,IFNULL(p.SmallBarCode, '') as SmallBarCode,IFNULL(p.StrokeBarCode, '') as StrokeBarCode,IFNULL(p.BigBarCode, '') as BigBarCode, CONCAT('1', CAST(s.Name AS char), '=',p.BigQuantity, CAST(b.Name AS char)) AS UnitConversion, IFNULL(p.ExpirationDays, 0) as ExpirationDays, IFNULL(p.AdventDays, 0) as AdventDays,p.StrokeQuantity,p.BigQuantity,s.Name as SmallUnitName,m.Name as StrokeUnitName,b.Name as BigUnitName,IFNULL(p.ExpirationDays, 0) * 1 / 3 AS OneThirdDay, IFNULL(p.ExpirationDays, 0) *2 / 3 AS TwoThirdDay,(select otq.StockQuantity from(select IFNULL(sum(alls.StockQuantity), 0) as StockQuantity,CONCAT(IFNULL(sum(alls.SmallStockQuantity), 0), '小',IFNULL(sum(alls.StrokeStockQuantity), 0), '中', IFNULL(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId,case ab.UnitId when p.SmallUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as SmallManufactureDete,case ab.UnitId when p.StrokeUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as StrokeManufactureDete,case ab.UnitId when p.BigUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as BigManufactureDete,case ab.UnitId when p.SmallUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as SmallManufactureDeteDiffDay,case ab.UnitId when p.StrokeUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as StrokeManufactureDeteDiffDay,case ab.UnitId when p.BigUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as BigManufactureDeteDiffDay,w.StockQuantity as StockQuantity,(w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) as SmallStockQuantity,(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ) as StrokeStockQuantity,(case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) as BigStockQuantity from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(IFNULL(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.StoreId = {storeId ?? 0} group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where a.StoreId = {storeId ?? 0} and w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and((SmallManufactureDeteDiffDay >= 0 and SmallManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 1 / 3)   or(StrokeManufactureDeteDiffDay >= 0 and StrokeManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 1 / 3) or(BigManufactureDeteDiffDay >= 0 and BigManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 1 / 3))) as otq) as OneThirdQuantity, '' as OneThirdQuantityUnitConversion, (select wtq.StockQuantity from(select IFNULL(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(IFNULL(sum(alls.SmallStockQuantity), 0), '小', IFNULL(sum(alls.StrokeStockQuantity), 0), '中',IFNULL(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from (select a.WareHouseId, ab.ProductId, ab.UnitId,case ab.UnitId when p.SmallUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as SmallManufactureDete,case ab.UnitId when p.StrokeUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as StrokeManufactureDete,case ab.UnitId when p.BigUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as BigManufactureDete,case ab.UnitId when p.SmallUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as SmallManufactureDeteDiffDay,case ab.UnitId when p.StrokeUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as StrokeManufactureDeteDiffDay,case ab.UnitId when p.BigUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as BigManufactureDeteDiffDay, w.StockQuantity as StockQuantity, (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) as SmallStockQuantity,(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ) as StrokeStockQuantity,(case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) as BigStockQuantity from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(IFNULL(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id where st.StoreId = {storeId ?? 0} group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where a.StoreId = {storeId ?? 0} and w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and((SmallManufactureDeteDiffDay >= 0 and SmallManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 2 / 3)   or(StrokeManufactureDeteDiffDay >= 0 and StrokeManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 2 / 3) or(BigManufactureDeteDiffDay >= 0 and BigManufactureDeteDiffDay <= IFNULL(p.ExpirationDays, 0) * 2 / 3))) as wtq) as TwoThirdQuantity, '' as TwoThirdQuantityUnitConversion, (select wqt.StockQuantity from(select IFNULL(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(IFNULL(sum(alls.SmallStockQuantity), 0), '小',IFNULL(sum(alls.StrokeStockQuantity), 0), '中',IFNULL(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId,case ab.UnitId when p.SmallUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end AS SmallManufactureDete,case ab.UnitId when p.StrokeUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end AS StrokeManufactureDete,case ab.UnitId when p.BigUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as BigManufactureDete,case ab.UnitId when p.SmallUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as SmallManufactureDeteDiffDay, case ab.UnitId when p.StrokeUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as StrokeManufactureDeteDiffDay, case ab.UnitId when p.BigUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as BigManufactureDeteDiffDay, w.StockQuantity as StockQuantity,(w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) as SmallStockQuantity,(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ) as StrokeStockQuantity,(case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) as BigStockQuantity, p.AdventDays from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(IFNULL(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId)as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and(alls.SmallManufactureDeteDiffDay >= alls.AdventDays or alls.StrokeManufactureDeteDiffDay >= alls.AdventDays or alls.BigManufactureDeteDiffDay >= alls.AdventDays)) as wqt) as WarningQuantity, '' as WarningQuantityUnitConversion, (select eqt.StockQuantity from(select IFNULL(sum(alls.StockQuantity),0) as StockQuantity,CONCAT(IFNULL(sum(alls.SmallStockQuantity), 0), '小', IFNULL(sum(alls.StrokeStockQuantity), 0), '中',IFNULL(sum(alls.BigStockQuantity), 0), '大') as StockQuantityConversion from(select a.WareHouseId, ab.ProductId, ab.UnitId,case ab.UnitId when p.SmallUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as SmallManufactureDete, case ab.UnitId when p.StrokeUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as StrokeManufactureDete, case ab.UnitId when p.BigUnitId then IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), '') else '' end as BigManufactureDete, case ab.UnitId when p.SmallUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as SmallManufactureDeteDiffDay,case ab.UnitId when p.StrokeUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as StrokeManufactureDeteDiffDay, case ab.UnitId when p.BigUnitId then(case IFNULL(ab.ManufactureDete,-1) when - 1 then - 1 else DATEDIFF(IFNULL(DATE_FORMAT(ab.ManufactureDete, '%Y-%m-%d %H:%i:%s'), ''), now()) end) else -1 end as BigManufactureDeteDiffDay, w.StockQuantity as StockQuantity,(w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) -(case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end )) as SmallStockQuantity , (case when p.StrokeQuantity = 0 then 0 else (w.StockQuantity - (case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end)) / p.StrokeQuantity end ) as StrokeStockQuantity,(case when IFNULL(p.BigQuantity,0) = 0 then 0 else IFNULL(w.StockQuantity, 0) / IFNULL(p.BigQuantity, 0) end) as BigStockQuantity, p.ExpirationDays from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id left join(select st.ProductId, st.StoreId, st.WareHouseId, w.Name WareHouseName, sum(IFNULL(st.CurrentQuantity,0)) StockQuantity from Stocks st inner join WareHouses w on st.WareHouseId = w.Id group by st.ProductId,st.StoreId,st.WareHouseId,w.Name)  as w on a.WareHouseId = w.WareHouseId left join StockEarlyWarnings as se on ab.ProductId = se.ProductId left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where w.ProductId = ab.ProductId and w.WareHouseId = a.WareHouseId) as alls where alls.ProductId = ps.ProductId and alls.WareHouseId = ps.WareHouseId and(alls.ExpirationDays <= alls.SmallManufactureDeteDiffDay or alls.ExpirationDays <= alls.StrokeManufactureDeteDiffDay or alls.ExpirationDays <= alls.BigManufactureDeteDiffDay)) as eqt) as ExpiredQuantity, '' as ExpiredQuantityUnitConversion, 0 as Expired_BTotalQuantity, 0 as Expired_STotalQuantity, 0 as Expired_MTotalQuantity, 0 as Warning_BTotalQuantity, 0 as Warning_STotalQuantity, 0 as Warning_MTotalQuantity, 0 as TwoThird_BTotalQuantity, 0 as TwoThird_STotalQuantity, 0 as TwoThird_MTotalQuantity, 0 as OneThird_BTotalQuantity, 0 as OneThird_STotalQuantity, 0 as OneThird_MTotalQuantity, 0 as StockQuantity from(select a.WareHouseId, ab.ProductId from PurchaseBills as a inner join PurchaseItems as ab on a.Id = ab.PurchaseBillId inner join Products as p  on ab.ProductId = p.Id where a.StoreId = {storeId ?? 0} group by a.WareHouseId, ab.ProductId) as ps left join Products as p  on ps.ProductId = p.Id left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id where {whereQuery}";


                       var datas = SaleBillsRepository_RO.QueryFromSql<ExpirationWarning>(mySqlString).ToList();

                       datas.ForEach(r =>
                       {
                           r.OneThirdQuantityUnitConversion = $"{Pexts.StockQuantityFormat(r.OneThirdQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                           r.TwoThirdQuantityUnitConversion = $"{Pexts.StockQuantityFormat(r.TwoThirdQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                           r.WarningQuantityUnitConversion = $"{Pexts.StockQuantityFormat(r.WarningQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";
                           r.ExpiredQuantityUnitConversion = $"{Pexts.StockQuantityFormat(r.ExpiredQuantity, r.StrokeQuantity, r.BigQuantity, r.SmallUnitName, r.StrokeUnitName, r.BigUnitName)}";

                           r.OneThird_BTotalQuantity = Pexts.StockQuantityFormat(r.OneThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item1;
                           r.OneThird_MTotalQuantity = Pexts.StockQuantityFormat(r.OneThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item2;
                           r.OneThird_STotalQuantity = Pexts.StockQuantityFormat(r.OneThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item3;

                           r.TwoThird_BTotalQuantity = Pexts.StockQuantityFormat(r.TwoThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item1;
                           r.TwoThird_MTotalQuantity = Pexts.StockQuantityFormat(r.TwoThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item2;
                           r.TwoThird_STotalQuantity = Pexts.StockQuantityFormat(r.TwoThirdQuantity, r.StrokeQuantity, r.BigQuantity).Item3;

                           r.Warning_BTotalQuantity = Pexts.StockQuantityFormat(r.WarningQuantity, r.StrokeQuantity, r.BigQuantity).Item1;
                           r.Warning_MTotalQuantity = Pexts.StockQuantityFormat(r.WarningQuantity, r.StrokeQuantity, r.BigQuantity).Item2;
                           r.Warning_STotalQuantity = Pexts.StockQuantityFormat(r.WarningQuantity, r.StrokeQuantity, r.BigQuantity).Item3;

                           r.Expired_BTotalQuantity = Pexts.StockQuantityFormat(r.ExpiredQuantity, r.StrokeQuantity, r.BigQuantity).Item1;
                           r.Expired_MTotalQuantity = Pexts.StockQuantityFormat(r.ExpiredQuantity, r.StrokeQuantity, r.BigQuantity).Item2;
                           r.Expired_STotalQuantity = Pexts.StockQuantityFormat(r.ExpiredQuantity, r.StrokeQuantity, r.BigQuantity).Item3;

                       });

                       reporting = datas;

                       return reporting;
                   });
            }
            catch (Exception)
            {
                return new List<ExpirationWarning>();
            }
        }


        /// <summary>
        /// 库存表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <param name="productName"></param>
        /// <param name="brandId"></param>
        /// <param name="status"></param>
        /// <param name="maxStock"></param>
        /// <param name="showZeroStack"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<StockReportProduct> GetStockReportProduct(int? storeId, int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pageIndex, int pageSize = 50)
        {
            if (pageSize >= 50)
                pageSize = 50;
            try
            {
                productName = CommonHelper.Filter(productName);

                var reporting = new List<StockReportProduct>();

                string sqlString = @"SELECT
                                        p.Id ProductId,
                                        IFNULL(p.ProductCode,'') as ProductCode, 

                                        (CASE
	                                    WHEN
                                           p.MnemonicCode IS NOT NULL
	                                    THEN
	                                       IFNULL(p.MnemonicCode,p.Name)
	                                    ELSE  p.Name END) as ProductName,

                                        IFNULL(p.SmallBarCode,'') as SmallBarCode,
                                        IFNULL(p.StrokeBarCode,'') as StrokeBarCode,
                                        IFNULL(p.BigBarCode,'') as BigBarCode,
                                        p.SmallUnitId,
                                        IFNULL(pa1.Name,'无') as  SmallUnitName,
                                        p.StrokeUnitId,
                                        IFNULL(pa2.Name,'无') as  StrokeUnitName,
                                        p.BigUnitId,
                                        IFNULL(pa3.Name,'无') as  BigUnitName,
                                        p.BigQuantity,
                                        p.StrokeQuantity,
                                        (CASE
                                            WHEN
                                                p.BigUnitId IS NOT NULL
                                                    AND p.BigUnitId != 0
                                            THEN
                                                CONCAT('1',
                                                        (SELECT 
                                                                s1.`Name`
                                                            FROM
                                                                SpecificationAttributeOptions s1
                                                            WHERE
                                                                s1.Id = p.BigUnitId and s1.StoreId = {0}),
                                                        ' = ',
                                                        p.BigQuantity,
                                                        (SELECT 
                                                                s1.`Name`
                                                            FROM
                                                                SpecificationAttributeOptions s1
                                                            WHERE
                                                                s1.Id = p.SmallUnitId and s1.StoreId = {0}))
                                            WHEN
                                                p.StrokeUnitId IS NOT NULL
                                                    AND p.StrokeUnitId != 0
                                            THEN
                                                CONCAT('1',
                                                        (SELECT 
                                                                s1.`Name`
                                                            FROM
                                                                SpecificationAttributeOptions s1
                                                            WHERE
                                                                s1.Id = p.StrokeUnitId and s1.StoreId = {0}),
                                                        ' = ',
                                                        p.BigQuantity,
                                                        (SELECT 
                                                                s1.`Name`
                                                            FROM
                                                                SpecificationAttributeOptions s1
                                                            WHERE
                                                                s1.Id = p.SmallUnitId and s1.StoreId = {0}))
                                            ELSE CONCAT('1',
                                                    (SELECT 
                                                            s1.`Name`
                                                        FROM
                                                            SpecificationAttributeOptions s1
                                                        WHERE
                                                            s1.Id = p.SmallUnitId and s1.StoreId = {0}),
                                                    ' = ',
                                                    '1',
                                                    (SELECT 
                                                            s1.`Name`
                                                        FROM
                                                            SpecificationAttributeOptions s1
                                                        WHERE
                                                            s1.Id = p.SmallUnitId and s1.StoreId = {0}))
                                        END) UnitConversion,
                                        p.CategoryId,
                                        c.Name CategoryName,
                                        p.BrandId,
                                        b.Name BrandName,
                                        IFNULL(st.WareHouseId, 0) as WareHouseId,
                                        IFNULL(st.CurrentQuantity, 0) as CurrentQuantity,
                                        '' CurrentQuantityConversion,
                                        IFNULL(st.UsableQuantity, 0) as UsableQuantity,
                                        '' UsableQuantityConversion,
                                        IFNULL(st.OrderQuantity, 0) as OrderQuantity,
                                        '' OrderQuantityConversion,
                                        IFNULL(pp.CostPrice, 0.00) CostPrice,
                                        0.00 CostAmount,
                                        IFNULL(pp.TradePrice, 0.00) TradePrice,
                                        0.00 TradeAmount
                                    FROM
                                        Products p
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa1 ON p.SmallUnitId = pa1.Id and p.SmallUnitId >0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa2 ON p.StrokeUnitId = pa2.Id and p.StrokeUnitId >0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa3 ON p.BigUnitId = pa3.Id and p.BigUnitId >0
                                            LEFT JOIN
                                        Categories c ON p.CategoryId = c.Id
                                            LEFT JOIN
                                        Brands b ON p.BrandId = b.Id
                                            LEFT JOIN
                                        (
                                        select 
                                        s.ProductId,
                                        s.WareHouseId,
                                        SUM(IFNULL(s.CurrentQuantity, 0)) CurrentQuantity,
                                        SUM(IFNULL(s.UsableQuantity, 0)) UsableQuantity,
                                        SUM(IFNULL(s.OrderQuantity, 0)) OrderQuantity ";



                sqlString += " from Stocks s where s.StoreId = " + storeId + " group by s.ProductId, s.WareHouseId) as st on st.ProductId = p.Id";
                sqlString += " LEFT JOIN ProductPrices pp ON p.Id = pp.ProductId  AND p.SmallUnitId = pp.UnitId and pp.ProductId >0 and pp.StoreId = " + storeId + " WHERE ";

                sqlString += " p.StoreId = " + storeId + " ";

                //仓库
                if (wareHouseId.HasValue && wareHouseId.Value > 0)
                {
                    sqlString += $" and st.WareHouseId = '{wareHouseId}' ";
                }

                //商品类别
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        string incategoryIds = string.Join("','", categoryIds);
                        sqlString += $" and p.CategoryId in ('{incategoryIds}') ";
                    }
                    else
                    {
                        sqlString += $" and p.CategoryId = '{categoryId}' ";
                    }
                }

                //商品
                if (productId.HasValue && productId.Value > 0)
                {
                    sqlString += $" and p.Id = '{productId}' ";
                }
                else
                {
                    if (productName != null)
                    {
                        sqlString += $" and p.Name like '%{productName}%' ";
                    }
                }
                //品牌
                if (brandId.HasValue && brandId.Value > 0)
                {
                    sqlString += $" and p.BrandId = '{brandId}' ";
                }

                //商品状态
                if (status.HasValue && status.Value)
                {
                    sqlString += $" and p.Status = {status.Value} ";
                }

                sqlString += " order by p.Id,p.StoreId ";


                var query = SaleBillsRepository_RO.QueryFromSql<StockReportProduct>(string.Format(sqlString, storeId)).ToList();

                //将单位整理
                if (query != null && query.Count > 0)
                {
                    //数量小于
                    //if (maxStock > 0)
                    //{
                    //    query = query.Where(q => q.CurrentQuantity <= maxStock).ToList();
                    //}

                    //显示0库存
                    if (showZeroStack.HasValue && showZeroStack == true)
                    {
                        query = query.Where(q => q.CurrentQuantity >= 0).ToList();
                    }
                    if (showZeroStack.HasValue && showZeroStack == false)
                    {
                        query = query.Where(q => q.CurrentQuantity > 0).ToList();
                    }

                    foreach (StockReportProduct item in query)
                    {
                        Product product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                        Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                        //现货库存数量
                        int thisCurrentQuantity = item.CurrentQuantity;
                        item.CurrentQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, thisCurrentQuantity);

                        //可用库存数量
                        int thisUsableQuantity = item.UsableQuantity;
                        item.UsableQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, thisUsableQuantity);

                        //预占库存数量
                        int thisOrderQuantity = item.OrderQuantity;
                        item.OrderQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, thisOrderQuantity);

                        //成本金额(元)
                        item.CostAmount = thisCurrentQuantity * item.CostPrice;
                        //批发金额(元)
                        item.TradeAmount = thisCurrentQuantity * item.TradePrice;
                    }
                }
                //排序
                query = query.OrderBy(q => q.CategoryName).ThenBy(q => q.ProductName).ToList();


                return new PagedList<StockReportProduct>(query, pageIndex, pageSize);

            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return new PagedList<StockReportProduct>(null, pageIndex, pageSize);
            }
        }


        public Tuple<IList<StockInOutRecordQuery>,int> AsyncStockInOutRecords(int? store, int? productId, int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                string sqlString = @"SELECT 
                                *
                            FROM
                                dcms.StockInOutRecords
                            WHERE
                                StoreId = {0}
                                    AND id IN (SELECT 
                                        StockInOutRecordId
                                    FROM
                                        dcms.StockInOutRecords_StockFlows_Mapping
                                    WHERE
                                        StoreId = {0}
                                            AND StockFlowId IN (SELECT 
                                                id
                                            FROM
                                                dcms.StockFlows
                                            WHERE
                                                StoreId = {0}
                                                    AND StockId IN (SELECT 
                                                        id
                                                    FROM
                                                        dcms.Stocks
                                                    WHERE
                                                        StoreId = {0} AND ProductId = {1})))";


                var sqlFormat = string.Format(sqlString, store, productId);

                var sbCount = $"SELECT COUNT(1) as `Value` FROM ({sqlFormat}) as alls;";
                int totalCount = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sbCount.ToString()).ToList().FirstOrDefault().Value ?? 0;

                string sbQuery = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Id) AS RowNum, alls.* FROM({sqlFormat}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY Id asc,CreatedOnUtc desc;";

                var lists = SaleBillsRepository_RO.QueryFromSql<StockInOutRecordQuery>(sbQuery).ToList();

                return new Tuple<IList<StockInOutRecordQuery>, int>(lists, totalCount);
            }
            catch (Exception)
            {
                return new Tuple<IList<StockInOutRecordQuery>, int>(new List<StockInOutRecordQuery>(), 0);
            }
        }


        public Tuple<IList<StockFlowQuery>, int> AsyncStockFlows(int? store, int? productId, int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                string sqlString = @"SELECT * FROM dcms.StockFlows where StoreId = {0} and  ProductId = {1}";

                var sqlFormat = string.Format(sqlString, store, productId);

                var sbCount = $"SELECT COUNT(1) as `Value` FROM ({sqlFormat}) as alls;";
                int totalCount = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sbCount.ToString()).ToList().FirstOrDefault().Value ?? 0;

                string sbQuery = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Id) AS RowNum, alls.*,0 AS Version FROM({sqlFormat}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY Id asc,CreateTime desc;";

                var lists = SaleBillsRepository_RO.QueryFromSql<StockFlowQuery>(sbQuery).ToList();

                return new Tuple<IList<StockFlowQuery>, int>(lists, totalCount);
            }
            catch (Exception ex)
            {
                return new Tuple<IList<StockFlowQuery>, int>(new List<StockFlowQuery>(), 0);
            }
        }
    }
}
