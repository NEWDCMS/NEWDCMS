using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Services.Report
{
    /// <summary>
    /// 用于表示采购报表服务
    /// </summary>
    public class PurchaseReportService : BaseService, IPurchaseReportService
    {
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public PurchaseReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            ICategoryService categoryService,
            IUserService userService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _categoryService = categoryService;
            _userService = userService;
        }

        /// <summary>
        /// 采购明细表
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="purchaseTypeId">单据类型Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">明细备注</param>
        /// <returns></returns>
        public IList<PurchaseReportItem> GetPurchaseReportItem(int? storeId, int? makeuserId, int? productId, string productName, int? categoryId, int? manufacturerId, int? wareHouseId, string billNumber, int? purchaseTypeId, DateTime? startTime, DateTime? endTime, string remark)
        {
            try
            {
                var key = DCMSDefaults.PURCHASE_GETPURCHASE_REPORTITEM_KEY.FillCacheKey(storeId, productId, productName, categoryId, manufacturerId, wareHouseId,
                     billNumber, purchaseTypeId, startTime, endTime, remark);
                return _cacheManager.Get(key, () =>
                {
                    var reporting = new List<PurchaseReportItem>();
                    productName = CommonHelper.Filter(productName);
                    billNumber = CommonHelper.Filter(billNumber);
                    remark = CommonHelper.Filter(remark);

                    string whereQuery1 = $" a.StoreId= {storeId ?? 0}";
                    string whereQuery2 = $" a.StoreId= {storeId ?? 0}";

                    if (makeuserId.HasValue && makeuserId.Value > 0)
                    {
                        var userIds = _userService.GetSubordinate(storeId, makeuserId ?? 0);
                        string str = string.Join(",", userIds);

                        whereQuery1 += $" and FIND_IN_SET(a.MakeUserId, '{str}')";
                        whereQuery2 += $" and FIND_IN_SET(a.MakeUserId, '{str}')";
                    }

                    if (productId.HasValue && productId.Value != 0)
                    {
                        whereQuery1 += $" and b.ProductId = '{productId}' ";
                        whereQuery2 += $" and b.ProductId = '{productId}' ";
                    }
                    if (productName != null)
                    {
                        whereQuery1 += $" and p.Name like '%{productName}%' ";
                        whereQuery2 += $" and p.Name like '%{productName}%' ";
                    }

                    if (categoryId.HasValue && categoryId.Value != 0)
                    {
                        //递归商品类别查询
                        var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                        if (categoryIds != null && categoryIds.Count > 0)
                        {
                            string incategoryIds = string.Join("','", categoryIds);
                            whereQuery1 += $" and p.CategoryId in ('{incategoryIds}') ";
                            whereQuery2 += $" and p.CategoryId in ('{incategoryIds}') ";
                        }
                        else
                        {
                            whereQuery1 += $" and p.CategoryId = '{categoryId}' ";
                            whereQuery2 += $" and p.CategoryId = '{categoryId}' ";
                        }
                    }

                    if (manufacturerId.HasValue && manufacturerId.Value != 0)
                    {
                        whereQuery1 += $" and a.ManufacturerId = '{manufacturerId}' ";
                        whereQuery2 += $" and a.ManufacturerId = '{manufacturerId}' ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value != 0)
                    {
                        whereQuery1 += $" and a.WareHouseId = '{wareHouseId}' ";
                        whereQuery2 += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (!string.IsNullOrEmpty(billNumber))
                    {
                        whereQuery1 += $" and a.BillNumber like '%{billNumber}%' ";
                        whereQuery2 += $" and a.BillNumber like '%{billNumber}%' ";
                    }

                    //PurchaseBill PurchaseReturnBill
                    if (purchaseTypeId.HasValue && purchaseTypeId.Value > 0)
                    {
                        //单据类型为采购单，则过滤采购退货单
                        if (purchaseTypeId.Value == (int)BillTypeEnum.PurchaseBill)
                        {
                            whereQuery2 += $" and 1=2 ";
                        }
                        //单据条件为采购退货单，则过滤采购单
                        else if (purchaseTypeId.Value == (int)BillTypeEnum.PurchaseReturnBill)
                        {
                            whereQuery1 += $" and 1=2 ";
                        }
                    }


                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        whereQuery1 += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                        whereQuery2 += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        whereQuery1 += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                        whereQuery2 += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                    }

                    //MSSQL
                    //string sqlString = $"(select a.Id BillId,a.BillNumber,{(int)BillTypeEnum.PurchaseBill} BillTypeId,'采购' BillTypeName,a.ManufacturerId,m.Name ManufacturerName,a.TransactionDate,a.AuditedDate,a.WareHouseId, w.Name WareHouseName, b.ProductId,p.Sku ProductSKU, p.Name ProductName, p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) BarCode,'' UnitConversion,b.Quantity Quantity, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) UnitName,b.Price,b.Amount,b.Remark from PurchaseBills a inner join PurchaseItems b on a.Id = b.PurchaseBillId left join Manufacturer m on a.ManufacturerId = m.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0') UNION ALL(select a.Id BillId, a.BillNumber,{(int)BillTypeEnum.PurchaseReturnBill} BillTypeId,'退购' BillTypeName,a.ManufacturerId,m.Name ManufacturerName, a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName, b.ProductId,p.Sku ProductSKU, p.Name ProductName, p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName, p.StrokeUnitId,pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode   else '' end ) BarCode,'' UnitConversion,b.Quantity Quantity, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) UnitName,b.Price,b.Amount,b.Remark from PurchaseReturnBills a inner join PurchaseReturnItems b on a.Id = b.PurchaseReturnBillId left join Manufacturer m on a.ManufacturerId = m.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";


                    //MYSQL
                    string sqlString = $"(select distinct a.Id BillId,a.BillNumber,{(int)BillTypeEnum.PurchaseBill} BillTypeId,'采购' BillTypeName,a.ManufacturerId,m.Name ManufacturerName,a.TransactionDate,a.AuditedDate,a.WareHouseId, w.Name WareHouseName, b.ProductId,p.Sku ProductSKU, p.Name ProductName, p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName, p.BigUnitId as BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) BarCode,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,b.Quantity Quantity,'' QuantityConversion, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) UnitName,b.Price,b.Amount,b.Remark from PurchaseBills a inner join PurchaseItems b on a.Id = b.PurchaseBillId left join Manufacturer m on a.ManufacturerId = m.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and pp.StoreId = {storeId ?? 0} where {whereQuery1} and a.AuditedStatus = '1' and a.ReversedStatus = '0') UNION ALL(select distinct a.Id BillId, a.BillNumber,{(int)BillTypeEnum.PurchaseReturnBill} BillTypeId,'退购' BillTypeName,a.ManufacturerId,m.Name ManufacturerName, a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName, b.ProductId,p.Sku ProductSKU, p.Name ProductName, p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName, p.StrokeUnitId,pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode   else '' end ) BarCode,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,b.Quantity Quantity,'' QuantityConversion, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) UnitName,b.Price,b.Amount,b.Remark from PurchaseReturnBills a inner join PurchaseReturnItems b on a.Id = b.PurchaseReturnBillId left join Manufacturer m on a.ManufacturerId = m.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and pp.StoreId = {storeId ?? 0} where {whereQuery2} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";

                    reporting = PurchaseBillsRepository_RO.QueryFromSql<PurchaseReportItem>(sqlString).ToList();

                    //将单位整理
                    if (reporting != null && reporting.Count > 0)
                    {
                        foreach (PurchaseReportItem item in reporting)
                        {
                            Product product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                            Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                            item.QuantityConversion = product.GetConversionFormat(dic, item.UnitId ?? 0, item.Quantity ?? 0);
                        }
                    }

                    return reporting.OrderByDescending(r=>r.TransactionDate).ToList();
                });
            }
            catch (Exception)
            {
                return new List<PurchaseReportItem>();
            }
        }

        /// <summary>
        /// 采购汇总（按商品）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <returns></returns>
        public IList<PurchaseReportSummaryProduct> GetPurchaseReportSummaryProduct(int? storeId, int? categoryId, int? productId, string productName, int? manufacturerId, int? wareHouseId, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.PURCHASE_GETPURCHASE_REPORTSUMMARY_PRODUCT_KEY.FillCacheKey(storeId, categoryId, productId, productName, manufacturerId, wareHouseId, startTime, endTime), () =>
                {
                    productName = CommonHelper.Filter(productName);

                    var reporting = new List<PurchaseReportSummaryProduct>();

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";

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

                    if (manufacturerId.HasValue && manufacturerId.Value != 0)
                    {
                        whereQuery += $" and a.ManufacturerId = '{manufacturerId}' ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value != 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                    }
                    //MSSQL
                    //string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity,'' PurchaseQuantityConversion,b.Amount PurchaseAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 PurchaseReturnSmallQuantity,0 PurchaseReturnStrokeQuantity,0 PurchaseReturnBigQuantity,'' PurchaseReturnQuantityConversion,0.00 PurchaseReturnAmount,0 SumSmallQuantity,0 SumStrokeQuantity,0 SumBigQuantity,'' SumQuantityConversion,0.00 SumAmount from PurchaseBills a inner join PurchaseItems b on a.Id=b.PurchaseBillId inner join Products p on b.ProductId=p.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where  {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL ( select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 PurchaseSmallQuantity,0 PurchaseStrokeQuantity,0 PurchaseBigQuantity,'' PurchaseQuantityConversion,0.00 PurchaseAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) PurchaseReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) PurchaseReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) PurchaseReturnBigQuantity,'' PurchaseReturnQuantityConversion,b.Amount PurchaseReturnAmount,0 SumSmallQuantity,0 SumStrokeQuantity,0 SumBigQuantity,'' SumQuantityConversion,0.00 SumAmount from PurchaseReturnBills a inner join PurchaseReturnItems b on a.Id=b.PurchaseReturnBillId inner join Products p on b.ProductId=p.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";

                    //MYSQL
                    string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) PurchaseSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) PurchaseStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) PurchaseBigQuantity,'' PurchaseQuantityConversion,b.Amount PurchaseAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 PurchaseReturnSmallQuantity,0 PurchaseReturnStrokeQuantity,0 PurchaseReturnBigQuantity,'' PurchaseReturnQuantityConversion,0.00 PurchaseReturnAmount,0 SumSmallQuantity,0 SumStrokeQuantity,0 SumBigQuantity,'' SumQuantityConversion,0.00 SumAmount from PurchaseBills a inner join PurchaseItems b on a.Id=b.PurchaseBillId inner join Products p on b.ProductId=p.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where  {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL ( select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,0 PurchaseSmallQuantity,0 PurchaseStrokeQuantity,0 PurchaseBigQuantity,'' PurchaseQuantityConversion,0.00 PurchaseAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) PurchaseReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) PurchaseReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) PurchaseReturnBigQuantity,'' PurchaseReturnQuantityConversion,b.Amount PurchaseReturnAmount,0 SumSmallQuantity,0 SumStrokeQuantity,0 SumBigQuantity,'' SumQuantityConversion,0.00 SumAmount from PurchaseReturnBills a inner join PurchaseReturnItems b on a.Id=b.PurchaseReturnBillId inner join Products p on b.ProductId=p.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";


                    var items = PurchaseBillsRepository_RO.QueryFromSql<PurchaseReportSummaryProduct>(sqlString).ToList();

                    if (items != null && items.Count > 0)
                    {
                        items.ToList().ForEach(a =>
                        {
                            var prsp = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                            if (prsp != null)
                            {
                                prsp.PurchaseSmallQuantity = (prsp.PurchaseSmallQuantity ?? 0) + (a.PurchaseSmallQuantity ?? 0);
                                prsp.PurchaseStrokeQuantity = (prsp.PurchaseStrokeQuantity ?? 0) + (a.PurchaseStrokeQuantity ?? 0);
                                prsp.PurchaseBigQuantity = (prsp.PurchaseBigQuantity ?? 0) + (a.PurchaseBigQuantity ?? 0);
                                prsp.PurchaseAmount = (prsp.PurchaseAmount ?? 0) + (a.PurchaseAmount ?? 0);

                                prsp.GiftSmallQuantity = (prsp.GiftSmallQuantity ?? 0) + (a.GiftSmallQuantity ?? 0);
                                prsp.GiftStrokeQuantity = (prsp.GiftStrokeQuantity ?? 0) + (a.GiftStrokeQuantity ?? 0);
                                prsp.GiftBigQuantity = (prsp.GiftBigQuantity ?? 0) + (a.GiftBigQuantity ?? 0);

                                prsp.PurchaseReturnSmallQuantity = (prsp.PurchaseReturnSmallQuantity ?? 0) + (a.PurchaseReturnSmallQuantity ?? 0);
                                prsp.PurchaseReturnStrokeQuantity = (prsp.PurchaseReturnStrokeQuantity ?? 0) + (a.PurchaseReturnStrokeQuantity ?? 0);
                                prsp.PurchaseReturnBigQuantity = (prsp.PurchaseReturnBigQuantity ?? 0) + (a.PurchaseReturnBigQuantity ?? 0);
                                prsp.PurchaseReturnAmount = (prsp.PurchaseReturnAmount ?? 0) + (a.PurchaseReturnAmount ?? 0);

                                prsp.SumSmallQuantity = (prsp.PurchaseSmallQuantity ?? 0) + ((prsp.GiftSmallQuantity ?? 0) - (prsp.PurchaseReturnSmallQuantity ?? 0));
                                prsp.SumStrokeQuantity = (prsp.PurchaseStrokeQuantity ?? 0) + ((prsp.GiftStrokeQuantity ?? 0) - (prsp.PurchaseReturnStrokeQuantity ?? 0));
                                prsp.SumBigQuantity = (prsp.PurchaseBigQuantity ?? 0) + ((prsp.GiftBigQuantity ?? 0) - (prsp.PurchaseReturnBigQuantity ?? 0));
                                prsp.SumAmount = (prsp.PurchaseAmount ?? 0) - (prsp.PurchaseReturnAmount ?? 0);

                            }
                            else
                            {
                                prsp = new PurchaseReportSummaryProduct
                                {
                                    ProductId = a.ProductId,
                                    ProductCode = a.ProductCode,
                                    ProductName = a.ProductName,
                                    SmallBarCode = a.SmallBarCode,
                                    StrokeBarCode = a.StrokeBarCode,
                                    BigBarCode = a.BigBarCode,
                                    SmallUnitId = a.SmallUnitId,
                                    SmallUnitName = a.SmallUnitName,
                                    StrokeUnitId = a.StrokeUnitId,
                                    StrokeUnitName = a.StrokeUnitName,
                                    BigUnitId = a.BigUnitId,
                                    BigUnitName = a.BigUnitName,
                                    BigQuantity = a.BigQuantity,
                                    StrokeQuantity = a.StrokeQuantity,
                                    UnitConversion = a.UnitConversion

                                };

                                prsp.PurchaseSmallQuantity = a.PurchaseSmallQuantity;
                                prsp.PurchaseStrokeQuantity = a.PurchaseStrokeQuantity;
                                prsp.PurchaseBigQuantity = a.PurchaseBigQuantity;
                                prsp.PurchaseAmount = a.PurchaseAmount;

                                prsp.GiftSmallQuantity = a.GiftSmallQuantity;
                                prsp.GiftStrokeQuantity = a.GiftStrokeQuantity;
                                prsp.GiftBigQuantity = a.GiftBigQuantity;

                                prsp.PurchaseReturnSmallQuantity = a.PurchaseReturnSmallQuantity;
                                prsp.PurchaseReturnStrokeQuantity = a.PurchaseReturnStrokeQuantity;
                                prsp.PurchaseReturnBigQuantity = a.PurchaseReturnBigQuantity;
                                prsp.PurchaseReturnAmount = a.PurchaseReturnAmount;

                                prsp.SumSmallQuantity = (prsp.PurchaseSmallQuantity ?? 0) + ((prsp.GiftSmallQuantity ?? 0) - (prsp.PurchaseReturnSmallQuantity ?? 0));
                                prsp.SumStrokeQuantity = (prsp.PurchaseStrokeQuantity ?? 0) + ((prsp.GiftStrokeQuantity ?? 0) - (prsp.PurchaseReturnStrokeQuantity ?? 0));
                                prsp.SumBigQuantity = (prsp.PurchaseBigQuantity ?? 0) + ((prsp.GiftBigQuantity ?? 0) - (prsp.PurchaseReturnBigQuantity ?? 0));
                                prsp.SumAmount = (prsp.PurchaseAmount ?? 0) + (prsp.PurchaseReturnAmount ?? 0);

                                reporting.Add(prsp);
                            }
                        });
                    }

                    //将单位整理
                    if (reporting != null && reporting.Count > 0)
                    {
                        foreach (PurchaseReportSummaryProduct item in reporting)
                        {
                            Product product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                            Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                            //采购
                            int sumPurchaseQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumPurchaseQuantity += (item.PurchaseBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumPurchaseQuantity += (item.PurchaseStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumPurchaseQuantity += (item.PurchaseSmallQuantity ?? 0);

                            var purchasequantity = Pexts.StockQuantityFormat(sumPurchaseQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.PurchaseBigQuantity = purchasequantity.Item1;
                            item.PurchaseStrokeQuantity = purchasequantity.Item2;
                            item.PurchaseSmallQuantity = purchasequantity.Item3;
                            item.PurchaseQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumPurchaseQuantity);

                            //赠送
                            int sumGiftQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumGiftQuantity += (item.GiftBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumGiftQuantity += (item.GiftStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumGiftQuantity += (item.GiftSmallQuantity ?? 0);

                            var giftquantity = Pexts.StockQuantityFormat(sumGiftQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.GiftBigQuantity = giftquantity.Item1;
                            item.GiftStrokeQuantity = giftquantity.Item2;
                            item.GiftSmallQuantity = giftquantity.Item3;
                            item.GiftQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumGiftQuantity);

                            //退购
                            int sumPurchaseReturnQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumPurchaseReturnQuantity += (item.PurchaseReturnBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumPurchaseReturnQuantity += (item.PurchaseReturnStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumPurchaseReturnQuantity += (item.PurchaseReturnSmallQuantity ?? 0);

                            var purchasereturnquantity = Pexts.StockQuantityFormat(sumPurchaseReturnQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.PurchaseReturnBigQuantity = purchasereturnquantity.Item1;
                            item.PurchaseReturnStrokeQuantity = purchasereturnquantity.Item2;
                            item.PurchaseReturnSmallQuantity = purchasereturnquantity.Item3;
                            item.PurchaseReturnQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumPurchaseReturnQuantity);

                            //小计
                            int sumSumQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumSumQuantity += (item.SumBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumSumQuantity += (item.SumStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumSumQuantity += (item.SumSmallQuantity ?? 0);

                            var sumquantity = Pexts.StockQuantityFormat(sumSumQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.SumBigQuantity = sumquantity.Item1;
                            item.SumStrokeQuantity = sumquantity.Item2;
                            item.SumSmallQuantity = sumquantity.Item3;
                            item.SumQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSumQuantity);
                        }
                    }

                    return reporting;
                });
            }
            catch (Exception)
            {
                return new List<PurchaseReportSummaryProduct>();
            }

        }

        /// <summary>
        /// 采购汇总（按供应商）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <returns></returns>
        public IList<PurchaseReportSummaryManufacturer> GetPurchaseReportSummaryManufacturer(int? storeId, DateTime? startTime, DateTime? endTime, int? manufacturerId, Dictionary<int, string> dic)
        {

            try
            {
                var key = DCMSDefaults.PURCHASE_GETPURCHASE_REPORTSUMMARY_MANUFACTURER_KEY.FillCacheKey(storeId, startTime, endTime, manufacturerId, string.Join("-", dic.Select(s => s.Key)));
                return _cacheManager.Get(key, () =>
                {
                    var reporting = new List<PurchaseReportSummaryManufacturer>();

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";

                    if (manufacturerId.HasValue && manufacturerId.Value != 0)
                    {
                        whereQuery += $" and a.ManufacturerId = '{manufacturerId}' ";
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                    }

                    //MSSQL
                    //string sqlString = $"select a.ManufacturerId ,m.Name ManufacturerName ,b.ProductId ,p.ProductCode ,p.Name ProductName,p.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,b.Quantity PurchaseQuantity,b.UnitId PurchaseUnitId,b.Amount PurchaseAmount from PurchaseBills a inner join PurchaseItems b on a.Id=b.PurchaseBillId inner join Products p on b.ProductId=p.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and  a.AuditedStatus='1' and a.ReversedStatus='0'";

                    //MYSQL
                    string sqlString = $"select a.ManufacturerId ,m.Name ManufacturerName ,b.ProductId ,p.ProductCode ,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,b.Quantity PurchaseQuantity,b.UnitId PurchaseUnitId,b.Amount PurchaseAmount from PurchaseBills a inner join PurchaseItems b on a.Id=b.PurchaseBillId inner join Products p on b.ProductId=p.Id left join Categories c on p.CategoryId=c.Id left join Manufacturer m on a.ManufacturerId=m.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and  a.AuditedStatus='1' and a.ReversedStatus='0'";

                    var items = PurchaseBillsRepository_RO.QueryFromSql<PurchaseReportSummaryManufacturerQuery>(sqlString).ToList();

                    if (items != null && items.Count > 0)
                    {
                        items.ToList().ForEach(a =>
                        {
                            //如果商品没有配置统计类别,则统计类别默认其他
                            var disKeys = dic.Keys.ToList();
                            if (!disKeys.Contains(a.StatisticalTypeId ?? 0))
                            {
                                a.StatisticalTypeId = (int)StatisticalTypeEnum.OtherTypeId;
                            }

                            //按客户查询
                            var prsm = reporting.Where(s => s.ManufacturerId == a.ManufacturerId).FirstOrDefault();
                            if (prsm != null)
                            {
                                prsm.PurchaseReportStatisticalTypes.ToList().ForEach(s =>
                                {

                                    if (s.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //数量
                                        int purchaseQuantity = (a.PurchaseQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.PurchaseUnitId ?? 0);
                                        s.SmallQuantity = (s.SmallQuantity??0) + purchaseQuantity;
                                        s.SumPurchaseSmallUnitConversion = s.SmallQuantity + a.SmallUnitName;
                                        //单据金额
                                        s.OrderAmount = (s.OrderAmount ?? 0) + (a.PurchaseAmount ?? 0);

                                        //主列
                                        //采购数量
                                        prsm.SumPurchaseSmallQuantity = (prsm.SumPurchaseSmallQuantity ?? 0) + purchaseQuantity;
                                        prsm.SumPurchaseSmallUnitConversion = prsm.SumPurchaseSmallQuantity + a.SmallUnitName;
                                        //单据金额
                                        prsm.SumOrderAmount = (prsm.SumOrderAmount ?? 0) + (a.PurchaseAmount ?? 0);
                                    }
                                });
                            }
                            else
                            {
                                prsm = new PurchaseReportSummaryManufacturer
                                {
                                    ManufacturerId = a.ManufacturerId,
                                    ManufacturerName = a.ManufacturerName
                                };

                                //添加动态列
                                if (dic != null && dic.Count > 0)
                                {
                                    dic.ToList().ForEach(d =>
                                    {
                                        prsm.PurchaseReportStatisticalTypes.Add(new PurchaseReportStatisticalType() { StatisticalTypeId = d.Key });
                                    });
                                }
                                prsm.PurchaseReportStatisticalTypes.ToList().ForEach(s2 =>
                                {
                                    if (s2.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //数量
                                        int purchaseQuantity = (a.PurchaseQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.PurchaseUnitId ?? 0);
                                        s2.SmallQuantity = purchaseQuantity;
                                        s2.SumPurchaseSmallUnitConversion = s2.SmallQuantity + a.SmallUnitName;
                                        //单据金额
                                        s2.OrderAmount = (a.PurchaseAmount ?? 0);

                                        //主列
                                        //采购数量
                                        prsm.SumPurchaseSmallQuantity = purchaseQuantity;
                                        prsm.SumPurchaseSmallUnitConversion = prsm.SumPurchaseSmallQuantity + a.SmallUnitName;
                                        //单据金额
                                        prsm.SumOrderAmount = (s2.OrderAmount ?? 0);

                                    }
                                });
                                reporting.Add(prsm);
                            }
                        });
                    }

                    return reporting;
                });
            }
            catch (Exception)
            {
                return new List<PurchaseReportSummaryManufacturer>();
            }

        }


    }
}
