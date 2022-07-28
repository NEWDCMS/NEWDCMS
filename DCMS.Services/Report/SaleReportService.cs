using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Report
{
    /// <summary>
    /// 用于表示销售报表服务
    /// </summary>
    public class SaleReportService : BaseService, ISaleReportService
    {
        private readonly IProductService _productService;
        private readonly IGiveQuotaService _giveQuotaService;
        private readonly IUserService _userService;
        private readonly IBrandService _brandService;
        private readonly ICategoryService _categoryService;
        private readonly IDistrictService _districtService;

        public SaleReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IProductService productService,
            IGiveQuotaService giveQuotaService,
            IBrandService brandService,
            ICategoryService categoryService,
            IUserService userService,
            IDistrictService districtService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _productService = productService;
            _giveQuotaService = giveQuotaService;
            _userService = userService;
            _brandService = brandService;
            _categoryService = categoryService;
            _districtService = districtService;
        }

        /// <summary>
        /// 销售明细表
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="payTypeId">支付方式Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="exchange">费用合同兑现商品</param>
        /// <param name="districtId">客户片区Id</param>
        /// <returns></returns>
        public PagedList<SaleReportItem> GetSaleReportItem(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? bussinessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? exchange, int? districtId, bool force = false, int pageIndex = 0,
            int pageSize = int.MaxValue,bool? auditedStatus = null)
        {
            try
            {
                var reporting = new List<SaleReportItem>();
                    productName = CommonHelper.Filter(productName);
                    terminalName = CommonHelper.Filter(terminalName);
                    billNumber = CommonHelper.Filter(billNumber);
                    remark = CommonHelper.Filter(remark);
                reporting = this.GetSaleReportData(storeId, productId, productName, categoryId, brandId, terminalId, terminalName, billNumber, saleTypeId, bussinessUserId, wareHouseId, payTypeId, deliveryUserId, rankId, remark, channelId, startTime, endTime, exchange, districtId, force, auditedStatus).ToList();
                return new PagedList<SaleReportItem>(reporting, pageIndex, pageSize);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IList<SaleReportItem> GetSaleReportData(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? bussinessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? exchange, int? districtId, bool force = false,bool? auditedStatus= null) 
        {
            try
            {
                var key = DCMSDefaults.SALEBILL_GETSALE_REPORTITEM_KEY.FillCacheKey(storeId, productId, productName, categoryId, brandId, terminalId, terminalName, billNumber, saleTypeId, bussinessUserId, wareHouseId, payTypeId, deliveryUserId, rankId, remark, channelId, startTime, endTime, exchange, districtId, auditedStatus);
                return _cacheManager.Get(key, () =>
                {
                    var reporting = new List<SaleReportItem>();
                    productName = CommonHelper.Filter(productName);
                    terminalName = CommonHelper.Filter(terminalName);
                    billNumber = CommonHelper.Filter(billNumber);
                    remark = CommonHelper.Filter(remark);

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";
                    string whereQuery2 = $" a.StoreId= {storeId ?? 0}";

                    if (productId.HasValue && productId.Value > 0)
                    {
                        whereQuery += $" and b.ProductId = '{productId}' ";
                        whereQuery2 += $" and b.ProductId = '{productId}' ";
                    }

                    if (productName != null)
                    {
                        whereQuery += $" and p.Name like'%{productName}%' ";
                        whereQuery2 += $" and p.Name like'%{productName}%' ";
                    }

                    if (categoryId.HasValue && categoryId.Value > 0)
                    {
                        //递归商品类别查询
                        var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                        if (categoryIds != null && categoryIds.Count > 0)
                        {
                            string incategoryIds = string.Join("','", categoryIds);
                            whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                            whereQuery2 += $" and p.CategoryId in ('{incategoryIds}') ";
                        }
                        else
                        {
                            whereQuery += $" and p.CategoryId = '{categoryId}' ";
                            whereQuery2 += $" and p.CategoryId = '{categoryId}' ";
                        }
                    }

                    if (brandId.HasValue && brandId.Value > 0)
                    {
                        whereQuery += $" and p.BrandId = '{brandId}' ";
                        whereQuery2 += $" and p.BrandId = '{brandId}' ";
                    }

                    if (bussinessUserId.HasValue && bussinessUserId.Value > 0)
                    {
                        whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                        whereQuery2 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value > 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                        whereQuery2 += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (terminalId.HasValue && terminalId.Value > 0)
                    {
                        whereQuery += $" and a.TerminalId = '{terminalId}' ";
                        whereQuery2 += $" and a.TerminalId = '{terminalId}' ";
                    }
                    if (terminalName != null)
                    {
                        whereQuery += $" and t.Name like '%{terminalName}%' ";
                        whereQuery2 += $" and t.Name like '%{terminalName}%' ";
                    }

                    if (!string.IsNullOrEmpty(billNumber))
                    {
                        whereQuery += $" and a.BillNumber like '%{billNumber}%' ";
                        whereQuery2 += $" and a.BillNumber like '%{billNumber}%' ";
                    }

                    //SaleReportItemSaleTypeEnum
                    //saleTypeId
                    //销售类型
                    if (saleTypeId > 0)
                    {
                        //销售商品
                        if (saleTypeId == (int)SaleReportItemSaleTypeEnum.SaleProduct)
                        {
                            whereQuery2 += $" and 1=2 ";
                        }
                        //退货商品
                        else if (saleTypeId == (int)SaleReportItemSaleTypeEnum.ReturnProduct)
                        {
                            whereQuery += $" and 1=2 ";
                        }
                    }

                    if (deliveryUserId.HasValue && deliveryUserId.Value > 0)
                    {
                        whereQuery += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                        whereQuery2 += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                    }

                    if (rankId.HasValue && rankId.Value > 0)
                    {
                        whereQuery += $" and t.RankId = '{rankId}' ";
                        whereQuery2 += $" and t.RankId = '{rankId}' ";
                    }

                    if (!string.IsNullOrEmpty(remark))
                    {
                        whereQuery += $" and b.Remark like '%{remark}%' ";
                        whereQuery2 += $" and b.Remark like '%{remark}%' ";
                    }

                    if (channelId.HasValue && channelId.Value > 0)
                    {
                        whereQuery += $" and t.ChannelId = '{channelId}' ";
                        whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                    }

                    //费用合同兑换商品
                    if (exchange.HasValue)
                    {
                        //费用合同兑换
                        if (exchange.Value == true)
                        {
                            whereQuery += $" and b.CostContractId > 0 ";
                        }
                        else
                        {
                            whereQuery += $" and (b.CostContractId = 0 or b.CostContractId is null )";
                        }
                    }

                    if (districtId.HasValue && districtId.Value > 0)
                    {
                        //递归片区查询
                        var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                        if (distinctIds != null && distinctIds.Count > 0)
                        {
                            string inDistinctIds = string.Join("','", distinctIds);
                            whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
                            whereQuery2 += $" and t.DistrictId in ('{inDistinctIds}') ";
                        }
                        else
                        {
                            whereQuery += $" and t.DistrictId = '{districtId}' ";
                            whereQuery2 += $" and t.DistrictId = '{districtId}' ";
                        }
                    }

                    if (payTypeId.HasValue && payTypeId.Value > 0)
                    {
                        if (payTypeId == (int)SaleReportSummaryProductPayTypeEnum.AlreadyReceiptCash)
                        {
                            whereQuery += $" and a.OweCash = 0 ";
                            whereQuery2 += $" and a.OweCash = 0 ";
                        }
                        if (payTypeId == (int)SaleReportSummaryProductPayTypeEnum.Overdraft)
                        {
                            whereQuery += $" and a.OweCash > 0 ";
                            whereQuery2 += $" and a.OweCash > 0 ";
                        }
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                        whereQuery2 += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                        whereQuery2 += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }

                    //审核状态
                    if (auditedStatus.HasValue)
                    {
                        whereQuery += $" and a.AuditedStatus={auditedStatus} ";
                        whereQuery2 += $" and a.AuditedStatus={auditedStatus} ";
                    }

                    //MYSQL
                    //string sbCount = $"select sum(allis.counter) as `Value` from ((select count(1) as counter from SaleBills a inner join SaleItems b on a.Id=b.SaleBillId left join SaleReservationBills c on a.SaleReservationBillId=c.Id  left join SaleReservationBill_Accounting_Mapping as sra on c.id = sra.SaleReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join WareHouses w on a.WareHouseId=w.Id left join Products p on b.ProductId=p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id and p.SmallUnitId > 0 left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id and p.StrokeUnitId > 0 left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id and p.BigUnitId > 0  left join ProductPrices pp on b.ProductId=pp.ProductId and b.UnitId=pp.UnitId  and b.UnitId >0  and pp.StoreId = {storeId ?? 0}  where {whereQuery} and a.ReversedStatus=0 and a.Deleted = 0) UNION ALL (select count(1) as counter from ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId left join ReturnReservationBills c on a.ReturnReservationBillId = c.Id  left join ReturnReservationBill_Accounting_Mapping as sra on c.id = sra.ReturnReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id and p.SmallUnitId > 0 left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id and p.StrokeUnitId > 0 left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id and p.BigUnitId > 0 left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and b.UnitId > 0 and pp.StoreId = {storeId ?? 0} where {whereQuery2} and a.ReversedStatus = 0 and a.Deleted = 0)) as allis";

                    //int totalCount = ProductsRepository_RO.QueryFromSql<IntQueryType>(sbCount).ToList().FirstOrDefault().Value ?? 0;



                    string sqlString = $"(select a.Id as BillId ,a.BillNumber,IFNULL(c.Id,0) as ReservationBillId ,IFNULL(c.BillNumber,'') as ReservationBillNumber ," +
                    $" 1 as BillTypeId ,'销售' as  BillTypeName ,a.TerminalId ,t.Name as TerminalName ,t.Code as TerminalCode ,a.BusinessUserId ,(select u.UserRealName from auth.Users as u where  u.StoreId = {storeId ?? 0} and  u.Id = a.BusinessUserId) AS BusinessUserName ," +
                    $"a.DeliveryUserId ,(select u.UserRealName from auth.Users as u where  u.StoreId = {storeId ?? 0} and  u.Id = a.DeliveryUserId) AS DeliveryUserName ,a.TransactionDate ,a.AuditedDate ,a.WareHouseId ,w.Name as WareHouseName ,b.ProductId ,p.Sku as ProductSKU ," +
                    $"p.Name as ProductName ,p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name as  SmallUnitName ,p.StrokeUnitId ,pa2.Name as StrokeUnitName " +
                    $",p.BigUnitId ,pa3.Name as  BigUnitName ,p.BigQuantity ,p.StrokeQuantity ,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) as SaleReturnSmallQuantity ," +
                    $"(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) as SaleReturnStrokeQuantity ," +
                    $"(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) as SaleReturnBigQuantity," +
                    $"(case when b.UnitId=p.SmallUnitId then p.SmallBarCode when b.UnitId=p.StrokeUnitId then p.StrokeBarCode when b.UnitId=p.BigUnitId then p.BigBarCode else '' end ) as BarCode ," +
                    $" {CommonHelper.GetSqlUnitConversion("p")} as UnitConversion ,b.Quantity as  Quantity ,b.UnitId ," +
                    $"(case when b.UnitId=p.SmallUnitId then pa1.Name when b.UnitId=p.StrokeUnitId then pa2.Name when b.UnitId=p.BigUnitId then pa3.Name else '' end ) as UnitName," +
                    $"b.Price ,b.Amount ,b.CostAmount ,b.Profit ,b.CostProfitRate ,0.00 as  SystemPrice ,b.Amount- (pp.RetailPrice*b.Quantity) as ChangeDifference ," +
                    $"0.00 as PresetPrice ,0.00 as RecentPurchasesPrice ,0.00 as RecentSettlementCostPrice ,b.Remark,ca.`Name` AS CategoryName,br.`Name` AS BrandName " +
                    $"from SaleBills a inner join SaleItems b on a.Id=b.SaleBillId " +
                    $"left join SaleReservationBills c on a.SaleReservationBillId=c.Id  " +
                    $"left join SaleReservationBill_Accounting_Mapping as sra on c.id = sra.SaleReservationBillId " +
                    $"left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id " +
                    $"left join WareHouses w on a.WareHouseId=w.Id " +
                    $"left join Products p on b.ProductId=p.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id and p.SmallUnitId > 0 " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id and p.StrokeUnitId > 0 " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id and p.BigUnitId > 0  " +
                    $"left join ProductPrices pp on b.ProductId=pp.ProductId and b.UnitId=pp.UnitId  and b.UnitId >0  and pp.StoreId = {storeId ?? 0}  " +
                    $"LEFT JOIN Categories ca ON p.CategoryId = ca.Id " +
                    $"LEFT JOIN Brands br ON p.BrandId = br.Id " +
                    $"where {whereQuery} and a.ReversedStatus=0 and a.Deleted = 0) " +
                    $"UNION ALL " +
                    $"(select a.Id as BillId, a.BillNumber ,IFNULL(c.Id,0) as ReservationBillId, IFNULL(c.BillNumber,'') as ReservationBillNumber,2 as BillTypeId ," +
                    $"'退货' as  BillTypeName ,a.TerminalId ,t.Name as TerminalName, t.Code as TerminalCode, a.BusinessUserId , (select u.UserRealName from auth.Users as u where  u.StoreId = {storeId ?? 0} and  u.Id = a.BusinessUserId) AS BusinessUserName ,a.DeliveryUserId ," +
                    $" (select u.UserRealName from auth.Users as u where  u.StoreId = {storeId ?? 0} and  u.Id = a.DeliveryUserId) AS DeliveryUserName ,a.TransactionDate ,a.AuditedDate ,a.WareHouseId ,w.Name as WareHouseName, b.ProductId ,p.Sku as ProductSKU, " +
                    $"p.Name as ProductName, p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name as SmallUnitName, p.StrokeUnitId ," +
                    $"pa2.Name as StrokeUnitName, p.BigUnitId ,pa3.Name as BigUnitName, p.BigQuantity ,p.StrokeQuantity ," +
                    $"(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) as SaleReturnSmallQuantity  ," +
                    $"(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) as SaleReturnStrokeQuantity  ," +
                    $"(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) as SaleReturnBigQuantity," +
                    $"(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) as BarCode ," +
                    $"{CommonHelper.GetSqlUnitConversion("p")} as UnitConversion ,b.Quantity as Quantity, b.UnitId ," +
                    $"(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end )  as UnitName ," +
                    $"b.Price ,b.Amount ,b.CostAmount ,b.Profit ,b.CostProfitRate ,0.00 as  SystemPrice ,b.Amount - (pp.RetailPrice * b.Quantity) as ChangeDifference ," +
                    $"0.00  as PresetPrice ,0.00 as RecentPurchasesPrice ,0.00 as RecentSettlementCostPrice ,b.Remark,ca.`Name` AS CategoryName,br.`Name` AS BrandName " +
                    $"from ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId " +
                    $"left join ReturnReservationBills c on a.ReturnReservationBillId = c.Id  " +
                    $"left join ReturnReservationBill_Accounting_Mapping as sra on c.id = sra.ReturnReservationBillId " +
                    $"left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id " +
                    $"left join WareHouses w on a.WareHouseId = w.Id " +
                    $"left join Products p on b.ProductId = p.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id and p.SmallUnitId > 0 " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id and p.StrokeUnitId > 0 " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id and p.BigUnitId > 0 " +
                    $"left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and b.UnitId > 0 and pp.StoreId = {storeId ?? 0} " +
                    $"left join Categories ca on p.CategoryId = ca.Id " +
                    $"left join Brands br on p.BrandId = br.Id " +
                    $"where {whereQuery2} and a.ReversedStatus = 0 and a.Deleted = 0 )";

                    //string sbQuery = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY BillId) AS RowNum, alls.* FROM({sqlString}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY BillId asc,TransactionDate desc;";

                     var lists = SaleBillsRepository_RO.QueryFromSql<SaleReportItem>(sqlString).ToList();
                    reporting = lists?.OrderByDescending(s => s.TransactionDate).ToList();

                    return reporting;
                }, force);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 销售汇总（按商品）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// /// <param name="productName">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">备注</param>
        /// <param name="payTypeId">支付方式Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="exchange">费用合同兑现商品</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <returns></returns>
        public IList<SaleReportSummaryProduct> GetSaleReportSummaryProduct(int? storeId, int? productId, string productName, int? categoryId, int? bussinessUserId, int? wareHouseId, int? terminalId, string terminalName, int? deliveryUserId, int? rankId, string remark, int? payTypeId, DateTime? startTime, DateTime? endTime, bool? exchange, int? channelId, int? districtId, bool force = false, bool? auditedStatus = null)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_PRODUCT_KEY.FillCacheKey(storeId, productId, productName, categoryId, bussinessUserId, wareHouseId, terminalId, deliveryUserId, rankId, remark, payTypeId, startTime, endTime, exchange, channelId, districtId, auditedStatus), () =>
                   {
                       productName = CommonHelper.Filter(productName);
                       terminalName = CommonHelper.Filter(terminalName);
                       remark = CommonHelper.Filter(remark);

                       var reporting = new List<SaleReportSummaryProduct>();

                       string whereQuery = $" a.StoreId= {storeId ?? 0}";
                       string whereQuery2 = $" a.StoreId= {storeId ?? 0}";

                       if (productId.HasValue && productId.Value != 0)
                       {
                           whereQuery += $" and b.ProductId = '{productId}' ";
                           whereQuery2 += $" and b.ProductId = '{productId}' ";
                       }
                       if (productName != null)
                       {
                           whereQuery += $" and p.Name like '%{productName}%' ";
                           whereQuery2 += $" and p.Name like '%{productName}%' ";
                       }

                       if (categoryId.HasValue && categoryId.Value != 0)
                       {
                           //递归商品类别查询
                           var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                           if (categoryIds != null && categoryIds.Count > 0)
                           {
                               string incategoryIds = string.Join("','", categoryIds);
                               whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                               whereQuery2 += $" and p.CategoryId in ('{incategoryIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and p.CategoryId = '{categoryId}' ";
                               whereQuery2 += $" and p.CategoryId = '{categoryId}' ";
                           }
                       }

                       if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                       {
                           whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                           whereQuery2 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                       }

                       if (wareHouseId.HasValue && wareHouseId.Value != 0)
                       {
                           whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                           whereQuery2 += $" and a.WareHouseId = '{wareHouseId}' ";
                       }

                       if (terminalId.HasValue && terminalId.Value != 0)
                       {
                           whereQuery += $" and a.TerminalId = '{terminalId}' ";
                           whereQuery2 += $" and a.TerminalId = '{terminalId}' ";
                       }

                       if (terminalName != null)
                       {
                           whereQuery += $" and t.Name like '%{terminalName}%' ";
                           whereQuery2 += $" and t.Name like '%{terminalName}%' ";
                       }

                       if (deliveryUserId.HasValue && deliveryUserId.Value != 0)
                       {
                           whereQuery += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                           whereQuery2 += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                       }

                       if (rankId.HasValue && rankId.Value != 0)
                       {
                           whereQuery += $" and t.RankId = '{rankId}' ";
                           whereQuery2 += $" and t.RankId = '{rankId}' ";
                       }

                       if (!string.IsNullOrEmpty(remark))
                       {
                           whereQuery += $" and b.Remark like '%{remark}%' ";
                           whereQuery2 += $" and b.Remark like '%{remark}%' ";
                       }

                       if (channelId.HasValue && channelId.Value != 0)
                       {
                           whereQuery += $" and t.ChannelId = '{channelId}' ";
                           whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                       }

                       if (districtId.HasValue && districtId.Value != 0)
                       {
                           //递归片区查询
                           var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                           if (distinctIds != null && distinctIds.Count > 0)
                           {
                               string inDistinctIds = string.Join("','", distinctIds);
                               whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
                               whereQuery2 += $" and t.DistrictId in ('{inDistinctIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and t.DistrictId = '{districtId}' ";
                               whereQuery2 += $" and t.DistrictId = '{districtId}' ";
                           }
                       }

                       //付款类型	  
                       //SaleReportSummaryProductPayTypeEnum
                       if (payTypeId.HasValue && payTypeId.Value != 0)
                       {
                           if (payTypeId == (int)SaleReportSummaryProductPayTypeEnum.AlreadyReceiptCash)
                           {
                               whereQuery += $" and a.OweCash = 0 ";
                               whereQuery2 += $" and a.OweCash = 0 ";
                           }
                           if (payTypeId == (int)SaleReportSummaryProductPayTypeEnum.Overdraft)
                           {
                               whereQuery += $" and a.OweCash > 0 ";
                               whereQuery2 += $" and a.OweCash > 0 ";
                           }
                       }

                       if (startTime.HasValue)
                       {
                           //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                           string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                           whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                           whereQuery2 += $" and a.CreatedOnUtc >= '{startTimedata}'";
                       }

                       if (endTime.HasValue)
                       {
                           //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                           string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                           whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                           whereQuery2 += $" and a.CreatedOnUtc <= '{endTimedata}'";
                       }

                       //费用合同兑换商品
                       if (exchange.HasValue)
                       {
                           //费用合同兑换
                           if (exchange.Value == true)
                           {
                               whereQuery += $" and b.CostContractId > 0 ";
                           }
                           else
                           {
                               whereQuery += $" and (b.CostContractId = 0 or b.CostContractId is null )";
                           }
                       }

                       //审核状态
                       if (auditedStatus.HasValue)
                       {
                           whereQuery += $" and a.AuditedStatus = {auditedStatus} ";
                           whereQuery2 += $" and a.AuditedStatus = {auditedStatus} ";
                       }
                       //MSSQL
                       //string sqlString = $"(select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join Items b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery}  and a.AuditedStatus = 1 and a.ReversedStatus = 0) UNION ALL(select b.ProductId , p.ProductCode , p.Name ProductName, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity , ''SaleQuantityConversion ,0.00 SaleAmount ,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity , ''GiftQuantityConversion ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity , ''ReturnQuantityConversion ,b.Amount ReturnAmount,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity , ''NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join Items b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0)";

                       //MYSQL
                       //string sqlString = $"(select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join Items b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery}  and a.AuditedStatus = 1 and a.ReversedStatus = 0) UNION ALL(select b.ProductId , p.ProductCode , p.Name ProductName, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity , ''SaleQuantityConversion ,0.00 SaleAmount ,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity , ''ReturnQuantityConversion ,b.Amount ReturnAmount,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity , ''NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join Items b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0)";

                       //string sqlString = $"(select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and b.IsGifts= 0 and a.AuditedStatus = 1 and a.ReversedStatus = 0) UNION ALL (select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) GiftSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) GiftStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and b.IsGifts= 1 and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0 ) UNION ALL(select b.ProductId , p.ProductCode , p.Name ProductName, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity , ''SaleQuantityConversion ,0.00 SaleAmount ,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity , ''GiftQuantityConversion ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity , ''ReturnQuantityConversion ,b.Amount ReturnAmount,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity , ''NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery2} and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0)";
                       string sqlString = $"(select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and b.IsGifts= 0 and a.ReversedStatus = 0) UNION ALL (select  b.ProductId ,p.ProductCode  ,p.Name ProductName  ,p.SmallBarCode  ,p.StrokeBarCode ,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName , p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity ,'' SaleQuantityConversion ,b.Amount SaleAmount,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) GiftSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) GiftStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) GiftBigQuantity ,'' GiftQuantityConversion ,0 ReturnSmallQuantity ,0 ReturnStrokeQuantity ,0 ReturnBigQuantity , ''ReturnQuantityConversion ,0.00 ReturnAmount ,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity ,'' NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and b.IsGifts= 1 and a.ReversedStatus = 0 and a.Deleted = 0 ) UNION ALL(select b.ProductId , p.ProductCode , p.Name ProductName, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion ,0 SaleSmallQuantity ,0 SaleStrokeQuantity ,0 SaleBigQuantity , ''SaleQuantityConversion ,0.00 SaleAmount ,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity , ''GiftQuantityConversion ,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity , ''ReturnQuantityConversion ,b.Amount ReturnAmount,0 NetSmallQuantity ,0 NetStrokeQuantity ,0 NetBigQuantity , ''NetQuantityConversion ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery2} and a.ReversedStatus = 0 and a.Deleted = 0)";

                       var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryProduct>(sqlString).ToList();

                       if (items != null && items.Count > 0)
                       {
                           items.ToList().ForEach(a =>
                           {

                               //商品 查询
                               var srsp = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                               if (srsp != null)
                               {
                                   srsp.SaleSmallQuantity = (srsp.SaleSmallQuantity ?? 0) + (a.SaleSmallQuantity ?? 0);
                                   srsp.SaleStrokeQuantity = (srsp.SaleStrokeQuantity ?? 0) + (a.SaleStrokeQuantity ?? 0);
                                   srsp.SaleBigQuantity = (srsp.SaleBigQuantity ?? 0) + (a.SaleBigQuantity ?? 0);
                                   srsp.SaleAmount = (srsp.SaleAmount ?? 0) + (a.SaleAmount ?? 0);

                                   srsp.GiftSmallQuantity = (srsp.GiftSmallQuantity ?? 0) + (a.GiftSmallQuantity ?? 0);
                                   srsp.GiftStrokeQuantity = (srsp.GiftStrokeQuantity ?? 0) + (a.GiftStrokeQuantity ?? 0);
                                   srsp.GiftBigQuantity = (srsp.GiftBigQuantity ?? 0) + (a.GiftBigQuantity ?? 0);

                                   srsp.ReturnSmallQuantity = (srsp.ReturnSmallQuantity ?? 0) + (a.ReturnSmallQuantity ?? 0);
                                   srsp.ReturnStrokeQuantity = (srsp.ReturnStrokeQuantity ?? 0) + (a.ReturnStrokeQuantity ?? 0);
                                   srsp.ReturnBigQuantity = (srsp.ReturnBigQuantity ?? 0) + (a.ReturnBigQuantity ?? 0);
                                   srsp.ReturnAmount = (srsp.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);

                                   srsp.NetSmallQuantity = (srsp.NetSmallQuantity ?? 0) + ((a.SaleSmallQuantity ?? 0) + (a.GiftSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0));
                                   srsp.NetStrokeQuantity = (srsp.NetStrokeQuantity ?? 0) + ((a.SaleStrokeQuantity ?? 0) + (a.GiftStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0));
                                   srsp.NetBigQuantity = (srsp.NetBigQuantity ?? 0) + ((a.SaleBigQuantity ?? 0) + (a.GiftBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0));
                                   srsp.NetAmount = (srsp.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));

                                   srsp.CostAmount = (srsp.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                   srsp.Profit = srsp.NetAmount - srsp.CostAmount;
                                   if (srsp.CostAmount == null || srsp.CostAmount == 0)
                                   {
                                       srsp.CostProfitRate = 100;
                                   }
                                   else
                                   {
                                       srsp.CostProfitRate = ((srsp.Profit ?? 0) / srsp.CostAmount) * 100;
                                   }


                               }
                               else
                               {
                                   srsp = new SaleReportSummaryProduct
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
                                   srsp.SaleSmallQuantity = a.SaleSmallQuantity;
                                   srsp.SaleStrokeQuantity = a.SaleStrokeQuantity;
                                   srsp.SaleBigQuantity = a.SaleBigQuantity;
                                   srsp.SaleAmount = a.SaleAmount;

                                   srsp.GiftSmallQuantity = a.GiftSmallQuantity;
                                   srsp.GiftStrokeQuantity = a.GiftStrokeQuantity;
                                   srsp.GiftBigQuantity = a.GiftBigQuantity;

                                   srsp.ReturnSmallQuantity = a.ReturnSmallQuantity;
                                   srsp.ReturnStrokeQuantity = a.ReturnStrokeQuantity;
                                   srsp.ReturnBigQuantity = a.ReturnBigQuantity;
                                   srsp.ReturnAmount = a.ReturnAmount;

                                   srsp.NetSmallQuantity = (a.SaleSmallQuantity ?? 0) + (a.GiftSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0);
                                   srsp.NetStrokeQuantity = (a.SaleStrokeQuantity ?? 0) + (a.GiftStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0);
                                   srsp.NetBigQuantity = (a.SaleBigQuantity ?? 0) + (a.GiftBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0);
                                   srsp.NetAmount = (a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0);

                                   srsp.CostAmount = a.CostAmount;
                                   srsp.Profit = srsp.NetAmount - srsp.CostAmount;
                                   if (srsp.CostAmount == null || srsp.CostAmount == 0)
                                   {
                                       srsp.CostProfitRate = 100;
                                   }
                                   else
                                   {
                                       srsp.CostProfitRate = ((srsp.Profit ?? 0) / srsp.CostAmount) * 100;
                                   }

                                   reporting.Add(srsp);
                               }
                           });
                       }

                       //将单位整理
                       if (reporting != null && reporting.Count > 0)
                       {
                           foreach (SaleReportSummaryProduct item in reporting)
                           {
                               Product product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                               Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                               //销售
                               int sumSaleQuantity = 0;
                               if (item.BigQuantity > 0)
                               {
                                   sumSaleQuantity += (item.SaleBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                               }
                               if (item.StrokeQuantity > 0)
                               {
                                   sumSaleQuantity += (item.SaleStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                               }
                               sumSaleQuantity += (item.SaleSmallQuantity ?? 0);

                               var salequantity = Pexts.StockQuantityFormat(sumSaleQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                               item.SaleBigQuantity = salequantity.Item1;
                               item.SaleStrokeQuantity = salequantity.Item2;
                               item.SaleSmallQuantity = salequantity.Item3;
                               item.SaleQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSaleQuantity);

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

                               //退货
                               int sumReturnQuantity = 0;
                               if (item.BigQuantity > 0)
                               {
                                   sumReturnQuantity += (item.ReturnBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                               }
                               if (item.StrokeQuantity > 0)
                               {
                                   sumReturnQuantity += (item.ReturnStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                               }
                               sumReturnQuantity += (item.ReturnSmallQuantity ?? 0);

                               var returnquantity = Pexts.StockQuantityFormat(sumReturnQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                               item.ReturnBigQuantity = returnquantity.Item1;
                               item.ReturnStrokeQuantity = returnquantity.Item2;
                               item.ReturnSmallQuantity = returnquantity.Item3;
                               item.ReturnQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumReturnQuantity);

                               //净销
                               int sumNetQuantity = 0;
                               if (item.BigQuantity > 0)
                               {
                                   sumNetQuantity += (item.NetBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                               }
                               if (item.StrokeQuantity > 0)
                               {
                                   sumNetQuantity += (item.NetStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                               }
                               sumNetQuantity += (item.NetSmallQuantity ?? 0);

                               var netquantity = Pexts.StockQuantityFormat(sumNetQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                               item.NetBigQuantity = netquantity.Item1;
                               item.NetStrokeQuantity = netquantity.Item2;
                               item.NetSmallQuantity = netquantity.Item3;
                               item.NetQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumNetQuantity);

                           }
                       }

                       return reporting;
                   }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportSummaryProduct>();
            }
        }

        /// <summary>
        /// 销售汇总（按客户）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="remark">整单备注</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="dic">动态列</param>
        /// <returns></returns>
        public IList<SaleReportSummaryCustomer> GetSaleReportSummaryCustomer(int? storeId, int? terminalId, string terminalName, DateTime? startTime, DateTime? endTime, int? brandId, int? productId, string productName, int? categoryId, int? districtId, int? channelId, int? rankId, string remark, int? bussinessUserId, int? wareHouseId, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_CUSTOMER_KEY.FillCacheKey(storeId, terminalId, startTime, endTime, brandId,
         productId, categoryId, districtId, channelId, rankId, remark, bussinessUserId, wareHouseId, string.Join("-", dic.Select(s => s.Key).ToArray()), auditedStatus);
                return _cacheManager.Get(key, () =>
                {
                    terminalName = CommonHelper.Filter(terminalName);
                    productName = CommonHelper.Filter(productName);
                    remark = CommonHelper.Filter(remark);

                    var reporting = new List<SaleReportSummaryCustomer>();

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

                    if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    {
                        whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value != 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (terminalId.HasValue && terminalId.Value != 0)
                    {
                        whereQuery += $" and a.TerminalId = '{terminalId}' ";
                    }
                    if (terminalName != null)
                    {
                        whereQuery += $" and t.Name like '{terminalName}' ";
                    }

                    if (brandId.HasValue && brandId.Value != 0)
                    {
                        whereQuery += $" and p.BrandId = '{brandId}' ";
                    }

                    if (rankId.HasValue && rankId.Value != 0)
                    {
                        whereQuery += $" and t.RankId = '{rankId}' ";
                    }

                    if (!string.IsNullOrEmpty(remark))
                    {
                        whereQuery += $" and b.Remark like '%{remark}%' ";
                    }

                    if (channelId.HasValue && channelId.Value != 0)
                    {
                        whereQuery += $" and t.ChannelId = '{channelId}' ";
                    }

                    if (districtId.HasValue && districtId.Value != 0)
                    {
                        //递归片区查询
                        var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
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

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }
                    //审核状态
                    if (auditedStatus.HasValue)
                    {
                        whereQuery += $" and a.AuditedStatus = {auditedStatus} ";
                    }

                    //MSSQL
                    //string sqlString = $"(select a.TerminalId ,t.Name TerminalName ,t.Code TerminalCode ,b.ProductId ,p.ProductCode ,p.Name ProductName , p.StatisticalType StatisticalTypeId, p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName, p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity, b.UnitId SaleUnitId, b.Amount SaleAmount,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join Items b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0 )UNION ALL(select a.TerminalId , t.Name TerminalName, t.Code TerminalCode, b.ProductId , p.ProductCode , p.Name ProductName, p.StatisticalType StatisticalTypeId, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId, b.Amount ReturnAmount,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join Items b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0)";

                    //MYSQL
                    //string sqlString = $"(select a.TerminalId ,t.Name TerminalName ,t.Code TerminalCode ,b.ProductId ,p.ProductCode ,p.Name ProductName , c.StatisticalType StatisticalTypeId, p.SmallBarCode, p.StrokeBarCode, p.BigBarCode, p.SmallUnitId, pa1.Name SmallUnitName, p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity, b.UnitId SaleUnitId, b.Amount SaleAmount,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join dcms.Categories c on p.CategoryId=c.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0 )UNION ALL(select a.TerminalId , t.Name TerminalName, t.Code TerminalCode, b.ProductId , p.ProductCode , p.Name ProductName, c.StatisticalType StatisticalTypeId, p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId, b.Amount ReturnAmount,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join dcms.Categories c on p.CategoryId=c.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0 )";
                    string sqlString = $"(select a.TerminalId ,t.Name TerminalName ,t.Code TerminalCode ,b.ProductId ,p.ProductCode ,p.Name ProductName , " +
                    $"c.StatisticalType StatisticalTypeId, p.SmallBarCode, p.StrokeBarCode, p.BigBarCode, p.SmallUnitId, pa1.Name SmallUnitName, p.StrokeUnitId ," +
                    $"pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity, b.UnitId SaleUnitId, " +
                    $"b.Amount SaleAmount,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit " +
                    $",b.CostProfitRate,b.IsGifts " +
                    $"from  SaleBills a " +
                    $"inner join SaleItems b on a.Id = b.SaleBillId " +
                    $"inner join Products p on b.ProductId = p.Id " +
                    $"left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id l" +
                    $"eft join dcms.Categories c on p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus = 0 and a.Deleted = 0 )" +
                    $"UNION ALL" +
                    $"(select a.TerminalId , t.Name TerminalName, t.Code TerminalCode, b.ProductId , p.ProductCode , p.Name ProductName, c.StatisticalType StatisticalTypeId," +
                    $" p.SmallBarCode , p.StrokeBarCode , p.BigBarCode , p.SmallUnitId , pa1.Name SmallUnitName, p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId , " +
                    $"pa3.Name BigUnitName, p.BigQuantity , p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId," +
                    $" b.Amount ReturnAmount,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate,b.IsGifts " +
                    $"from  ReturnBills a " +
                    $"inner join ReturnItems b on a.Id = b.ReturnBillId " +
                    $"inner join Products p on b.ProductId = p.Id " +
                    $"left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id " +
                    $"left join dcms.Categories c on p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus = 0 and a.Deleted = 0 )";

                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryCustomerQuery>(sqlString).ToList();

                    //添加动态列
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
                            var srsc = reporting.Where(s => s.TerminalId == a.TerminalId).FirstOrDefault();
                            if (srsc != null)
                            {
                                //销售数量
                                int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                //退货数量
                                int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                //赠送数量
                                int giftQuantity = 0;
                                if (a.IsGifts)
                                {
                                    giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                }

                                srsc.SaleReportStatisticalTypes.ToList().ForEach(s =>
                                {
                                    if (s.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //净销数量
                                        s.NetSmallQuantity = (s.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s.NetAmount = (s.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s.CostAmount = (s.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s.Profit = s.NetAmount - s.CostAmount;
                                        //赠送数
                                        if (a.IsGifts) 
                                        {
                                            s.GiftQuantity = (s.GiftQuantity ?? 0) + (a.SaleQuantity ?? 0);
                                        }
                                        //成本利润率
                                        if (s.CostAmount == null || s.CostAmount == 0)
                                        {
                                            s.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s.CostProfitRate = (s.Profit / s.CostAmount) * 100;
                                        }
                                    }
                                });
                                //赠送数量
                                srsc.GiftQuantity = (srsc.GiftQuantity ?? 0) + giftQuantity;
                                //销售数量 包含赠品
                                srsc.SaleSmallQuantity = (srsc.SaleSmallQuantity ?? 0) + saleQuantity - giftQuantity;
                                //退货数量
                                srsc.ReturnSmallQuantity = (srsc.ReturnSmallQuantity ?? 0) + returnQuantity;
                                //净销售量 = 销售数量 - 退货数量
                                srsc.NetSmallQuantity = srsc.SaleSmallQuantity + srsc.GiftQuantity - srsc.ReturnSmallQuantity;
                                //销售金额
                                srsc.SaleAmount = (srsc.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                //退货金额
                                srsc.ReturnAmount = (srsc.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                //销售净额 = 销售金额 - 退货金额
                                srsc.NetAmount = srsc.SaleAmount - srsc.ReturnAmount;
                                //优惠
                                //成本
                                srsc.CostAmount = (srsc.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                //利润
                                srsc.Profit = srsc.NetAmount - srsc.CostAmount;
                                //成本利润率
                                if (srsc.CostAmount == null || srsc.CostAmount == 0)
                                {
                                    srsc.CostProfitRate = 100;
                                }
                                else
                                {
                                    srsc.CostProfitRate = (srsc.Profit / srsc.CostAmount) * 100;
                                }
                            }
                            else
                            {
                                srsc = new SaleReportSummaryCustomer
                                {
                                    TerminalId = a.TerminalId,
                                    TerminalName = a.TerminalName,
                                    TerminalCode = a.TerminalCode
                                };

                                //添加动态列
                                if (dic != null && dic.Count > 0)
                                {
                                    dic.ToList().ForEach(d =>
                                    {
                                        srsc.SaleReportStatisticalTypes.Add(new SaleReportStatisticalType() { StatisticalTypeId = d.Key });
                                    });
                                }

                                //销售数量
                                int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                //退货数量
                                int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                //赠送数量
                                int giftQuantity = 0;
                                if (a.IsGifts)
                                {
                                    giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                }
                                srsc.SaleReportStatisticalTypes.ToList().ForEach(s2 =>
                                {
                                    if (s2.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //净销数量
                                        s2.NetSmallQuantity = (s2.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s2.NetAmount = (s2.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s2.CostAmount = (s2.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s2.Profit = s2.NetAmount - s2.CostAmount;
                                        //赠送数
                                        if (a.IsGifts)
                                        {
                                            s2.GiftQuantity = (s2.GiftQuantity ?? 0) + (a.SaleQuantity ?? 0);
                                        }
                                        //成本利润率
                                        if (s2.CostAmount == null || s2.CostAmount == 0)
                                        {
                                            s2.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s2.CostProfitRate = (s2.Profit / s2.CostAmount) * 100;
                                        }
                                    }
                                });

                                //主列
                                //赠送数量
                                srsc.GiftQuantity = (srsc.GiftQuantity ?? 0) + giftQuantity;
                                //销售数量 包含赠品
                                srsc.SaleSmallQuantity = (srsc.SaleSmallQuantity ?? 0) + saleQuantity - giftQuantity;
                                //退货数量
                                srsc.ReturnSmallQuantity = (srsc.ReturnSmallQuantity ?? 0) + returnQuantity;
                                //净销售量 = 销售数量 - 退货数量
                                srsc.NetSmallQuantity = (srsc.NetSmallQuantity ?? 0) + (srsc.SaleSmallQuantity + srsc.GiftQuantity - srsc.ReturnSmallQuantity);
                                //销售金额
                                srsc.SaleAmount = (srsc.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                //退货金额
                                srsc.ReturnAmount = (srsc.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                //销售净额 = 销售金额 - 退货金额
                                srsc.NetAmount = (srsc.NetAmount ?? 0) + (srsc.SaleAmount - srsc.ReturnAmount);
                                //优惠
                                //成本
                                srsc.CostAmount = (srsc.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                //利润
                                srsc.Profit = srsc.NetAmount - srsc.CostAmount;
                                //成本利润率
                                if (srsc.CostAmount == null || srsc.CostAmount == 0)
                                {
                                    srsc.CostProfitRate = 100;
                                }
                                else
                                {
                                    srsc.CostProfitRate = (srsc.Profit / srsc.CostAmount) * 100;
                                }

                                reporting.Add(srsc);
                            }
                        });
                    }
                    return reporting;
                }, force);
            }
            catch (Exception ex)
            {
                return new List<SaleReportSummaryCustomer>();
            }
        }

        /// <summary>
        /// 销售汇总（按业务员）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="dic">动态列</param>
        /// <returns></returns>
        public IList<SaleReportSummaryBusinessUser> GetSaleReportSummaryBusinessUser(int? storeId, int? bussinessUserId, DateTime? startTime, DateTime? endTime, int? brandId, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null)
        {

            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_BUSINESSUSER_KEY.FillCacheKey(storeId, bussinessUserId, startTime, endTime, brandId, string.Join("-", dic.Select(s => s.Key).ToArray()), auditedStatus);
                return _cacheManager.Get(key, () =>
                {
                    var reporting = new List<SaleReportSummaryBusinessUser>();

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";

                    if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    {
                        whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                    }

                    if (brandId.HasValue && brandId.Value != 0)
                    {
                        whereQuery += $" and p.BrandId = '{brandId}' ";
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }

                    if (auditedStatus.HasValue)
                    {
                        whereQuery += $" and a.AuditedStatus = {auditedStatus} ";
                    }

                    //MSSQL
                    //string sqlString = $"(select a.BusinessUserId ,'' BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName ,p.StatisticalType StatisticalTypeId ,p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName ,p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount ,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and  a.AuditedStatus=1 and a.ReversedStatus=0 ) UNION ALL(select a.BusinessUserId ,'' BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName, p.StatisticalType StatisticalTypeId, p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName, p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId, b.Amount ReturnAmount,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join Items b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery}  and a.AuditedStatus = 1 and a.ReversedStatus = 0)";

                    //MYSQL
                    //string sqlString = $"(select a.BusinessUserId ,'' BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName ,c.StatisticalType StatisticalTypeId ,p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName ,p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount ,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  SaleBills a inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and  a.AuditedStatus=1 and a.ReversedStatus=0 and a.Deleted = 0 ) UNION ALL(select a.BusinessUserId ,'' BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName, c.StatisticalType StatisticalTypeId, p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName, p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, p.BigQuantity ,p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId, b.Amount ReturnAmount,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate from  ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery}  and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0 )";
                    string sqlString = $"(select a.BusinessUserId , (select u.UserRealName from auth.users as u where  u.StoreId = 438 and  u.Id = a.BusinessUserId) AS BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName ,c.StatisticalType StatisticalTypeId " +
                    $",p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName " +
                    $",p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount ,0 ReturnQuantity ,0 ReturnUnitId ,0.00 ReturnAmount " +
                    $",0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate,b.IsGifts " +
                    $"from  SaleBills a " +
                    $"inner join SaleItems b on a.Id=b.SaleBillId " +
                    $"inner join Products p on b.ProductId=p.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus=0 and a.Deleted = 0 ) " +
                    $"UNION ALL" +
                    $"(select a.BusinessUserId , (select u.UserRealName from auth.users as u where  u.StoreId = 438 and  u.Id = a.BusinessUserId) AS BusinessUserName ,b.ProductId ,p.ProductCode ,p.Name ProductName, c.StatisticalType StatisticalTypeId, p.SmallBarCode " +
                    $",p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName, p.StrokeUnitId ,pa2.Name StrokeUnitName, p.BigUnitId ,pa3.Name BigUnitName, " +
                    $"p.BigQuantity ,p.StrokeQuantity ,0 SaleQuantity ,0 SaleUnitId ,0.00 SaleAmount ,b.Quantity ReturnQuantity, b.UnitId ReturnUnitId, b.Amount ReturnAmount" +
                    $",0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate,b.IsGifts" +
                    $" from  ReturnBills a " +
                    $"inner join ReturnItems b on a.Id = b.ReturnBillId " +
                    $"inner join Products p on b.ProductId = p.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus = 0 and a.Deleted = 0 )";


                    var lists = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryBusinessUserQuery>(sqlString).ToList();

                    var items = lists.ToList();

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

                            //按业务员查询
                            var srsbu = reporting.Where(s => s.BusinessUserId == a.BusinessUserId).FirstOrDefault();
                            if (srsbu != null)
                            {
                                srsbu.SaleReportStatisticalTypes.ToList().ForEach(s =>
                                {

                                    if (s.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //销售数量
                                        int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        //退货数量
                                        int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                        //赠送数量
                                        int giftQuantity = 0;
                                        if (a.IsGifts)
                                        {
                                            giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        }
                                        //净销数量
                                        s.NetSmallQuantity = (s.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s.NetAmount = (s.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s.CostAmount = (s.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s.Profit = s.NetAmount - s.CostAmount;
                                        //成本利润率
                                        if (s.CostAmount == null || s.CostAmount == 0)
                                        {
                                            s.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s.CostProfitRate = (s.Profit / s.CostAmount) * 100;
                                        }

                                        //主列
                                        //销售数量 包含赠品
                                        srsbu.SaleSmallQuantity = (srsbu.SaleSmallQuantity ?? 0) + saleQuantity;
                                        //退货数量
                                        srsbu.ReturnSmallQuantity = (srsbu.ReturnSmallQuantity ?? 0) + returnQuantity;
                                        //赠送数量
                                        srsbu.GiftQuantity = (srsbu.GiftQuantity ?? 0) + giftQuantity;
                                        //净销售量 = 销售数量 - 退货数量
                                        srsbu.NetSmallQuantity = (srsbu.NetSmallQuantity ?? 0) + (srsbu.SaleSmallQuantity - srsbu.ReturnSmallQuantity);
                                        //销售金额
                                        srsbu.SaleAmount = (srsbu.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                        //退货金额
                                        srsbu.ReturnAmount = (srsbu.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                        //销售净额 = 销售金额 - 退货金额
                                        srsbu.NetAmount = (srsbu.NetAmount ?? 0) + (srsbu.SaleAmount - srsbu.ReturnAmount);
                                        //优惠
                                        //成本
                                        srsbu.CostAmount = (srsbu.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        srsbu.Profit = srsbu.NetAmount - srsbu.CostAmount;
                                        //成本利润率
                                        if (srsbu.CostAmount == null || srsbu.CostAmount == 0)
                                        {
                                            srsbu.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            srsbu.CostProfitRate = (srsbu.Profit / srsbu.CostAmount) * 100;
                                        }

                                    }
                                });

                            }
                            else
                            {
                                srsbu = new SaleReportSummaryBusinessUser
                                {
                                    BusinessUserId = a.BusinessUserId,
                                    BusinessUserName = a.BusinessUserName
                                };

                                //添加动态列
                                if (dic != null && dic.Count > 0)
                                {
                                    dic.ToList().ForEach(d =>
                                    {
                                        srsbu.SaleReportStatisticalTypes.Add(new SaleReportStatisticalType() { StatisticalTypeId = d.Key });
                                    });
                                }
                                srsbu.SaleReportStatisticalTypes.ToList().ForEach(s2 =>
                                {
                                    if (s2.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //销售数量
                                        int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        //退货数量
                                        int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                        //赠送数量
                                        int giftQuantity = 0;
                                        if (a.IsGifts)
                                        {
                                            giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        }
                                        //净销数量
                                        s2.NetSmallQuantity = (s2.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s2.NetAmount = (s2.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s2.CostAmount = (s2.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s2.Profit = s2.NetAmount - s2.CostAmount;
                                        //成本利润率
                                        if (s2.CostAmount == null || s2.CostAmount == 0)
                                        {
                                            s2.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s2.CostProfitRate = (s2.Profit / s2.CostAmount) * 100;
                                        }

                                        //主列
                                        //销售数量 包含赠品
                                        srsbu.SaleSmallQuantity = (srsbu.SaleSmallQuantity ?? 0) + saleQuantity;
                                        //退货数量
                                        srsbu.ReturnSmallQuantity = (srsbu.ReturnSmallQuantity ?? 0) + returnQuantity;
                                        //赠送数量
                                        srsbu.GiftQuantity = (srsbu.GiftQuantity ?? 0) + giftQuantity;
                                        //净销售量 = 销售数量 - 退货数量
                                        srsbu.NetSmallQuantity = (srsbu.NetSmallQuantity ?? 0) + (srsbu.SaleSmallQuantity - srsbu.ReturnSmallQuantity);
                                        //销售金额
                                        srsbu.SaleAmount = (srsbu.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                        //退货金额
                                        srsbu.ReturnAmount = (srsbu.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                        //销售净额 = 销售金额 - 退货金额
                                        srsbu.NetAmount = (srsbu.NetAmount ?? 0) + (srsbu.SaleAmount - srsbu.ReturnAmount);
                                        //优惠
                                        //成本
                                        srsbu.CostAmount = (srsbu.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        srsbu.Profit = srsbu.NetAmount - srsbu.CostAmount;
                                        //成本利润率
                                        if (srsbu.CostAmount == null || srsbu.CostAmount == 0)
                                        {
                                            srsbu.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            srsbu.CostProfitRate = (srsbu.Profit / srsbu.CostAmount) * 100;
                                        }

                                    }
                                });
                                reporting.Add(srsbu);
                            }
                        });
                    }

                    return reporting;
                }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportSummaryBusinessUser>();
            }
        }

        /// <summary>
        /// 销售汇总（按客户/商品）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <returns></returns>
        public PagedList<SaleReportSummaryCustomerProduct> GetSaleReportSummaryCustomerProduct(int? storeId, int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId,int? channelId, int? rankId, int? bussinessUserId, int? deliveryUserId,int? terminalId, string terminalName1, string remark, DateTime? startTime, DateTime? endTime, bool force = false, int pageIndex = 0, int pageSize = 100, bool? auditedStatus = null)
        {

            try
            {
                productName = CommonHelper.Filter(productName);
                terminalName1 = CommonHelper.Filter(terminalName1);
                remark = CommonHelper.Filter(remark);

                var reporinting = new List<SaleReportSummaryCustomerProduct>();

                var pitems = GetSaleReportSummaryCustomerProductData(storeId, wareHouseId, productId, productName, categoryId, brandId, channelId, rankId, bussinessUserId, deliveryUserId, terminalId, terminalName1, remark, startTime, endTime,pageIndex:pageIndex,pageSize:pageSize,auditedStatus:auditedStatus);
                return new PagedList<SaleReportSummaryCustomerProduct>(pitems, pageIndex, pageSize, pitems.Count);

            }
            catch (Exception ex)
            {
                return new PagedList<SaleReportSummaryCustomerProduct>(null, 0, 0);
            }

        }

        public IList<SaleReportSummaryCustomerProduct> GetSaleReportSummaryCustomerProductData(int? storeId, int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? rankId, int? bussinessUserId, int? deliveryUserId, int? terminalId, string terminalName1, string remark, DateTime? startTime, DateTime? endTime, bool force = false, int pageIndex = 0, int pageSize = 0, bool? auditedStatus = null)
        {
            try
            {
                var key = DCMSDefaults.SALEBILL_CUSTOMERPRODUCT_KEY.FillCacheKey(storeId, wareHouseId, productId, productName, categoryId, brandId, channelId, rankId, bussinessUserId, deliveryUserId, terminalId, terminalName1, remark, startTime, endTime, auditedStatus);
                return _cacheManager.Get(key, () =>
                {
                    productName = CommonHelper.Filter(productName);
                    terminalName1 = CommonHelper.Filter(terminalName1);
                    remark = CommonHelper.Filter(remark);

                    var reporinting = new List<SaleReportSummaryCustomerProduct>();

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";

                    if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    {
                        whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value > 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' storeId, wareHouseId, productId, productName, categoryId, brandId, channelId, rankId, bussinessUserId, deliveryUserId, terminalId, terminalName1, remark, startTime, endTime";
                    }

                    if (productId.HasValue && productId.Value > 0)
                    {
                        whereQuery += $" and b.ProductId = '{productId}' ";
                    }

                    if (productName != null)
                    {
                        whereQuery += $" and p.Name like '%{productName}%' ";
                    }

                    if (categoryId.HasValue && categoryId.Value > 0)
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

                    if (terminalId.HasValue && terminalId.Value > 0)
                    {
                        whereQuery += $" and a.TerminalId = '{terminalId}' ";
                    }
                    if (terminalName1 != null)
                    {
                        whereQuery += $" and t.Name like '%{terminalName1}%' ";
                    }

                    if (brandId.HasValue && brandId.Value > 0)
                    {
                        whereQuery += $" and p.BrandId = '{brandId}' ";
                    }

                    if (rankId.HasValue && rankId.Value > 0)
                    {
                        whereQuery += $" and t.RankId = '{rankId}' ";
                    }

                    if (!string.IsNullOrEmpty(remark))
                    {
                        whereQuery += $" and b.Remark like '%{remark}%' ";
                    }

                    if (channelId.HasValue && channelId.Value > 0)
                    {
                        whereQuery += $" and t.ChannelId = '{channelId}' ";
                    }

                    if (deliveryUserId.HasValue && deliveryUserId.Value > 0)
                    {
                        whereQuery += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }

                    if (auditedStatus.HasValue)
                    {
                        whereQuery += $" and a.AuditedStatus = {auditedStatus}";
                    }

                    string counterSQL = @"select sum(allis.counter) as `Value` from ((SELECT 
                                        count(1) as counter
                                    FROM
                                        SaleBills a
                                            INNER JOIN
                                        SaleItems b ON a.Id = b.SaleBillId
                                            LEFT JOIN
                                        SaleReservationBills c ON a.SaleReservationBillId = c.Id
                                            LEFT JOIN
                                        dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id
                                            LEFT JOIN
                                        WareHouses w ON a.WareHouseId = w.Id
                                            LEFT JOIN
                                        Products p ON b.ProductId = p.Id
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa1 ON p.SmallUnitId = pa1.Id
                                            AND p.SmallUnitId > 0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa2 ON p.StrokeUnitId = pa2.Id
                                            AND p.StrokeUnitId > 0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa3 ON p.BigUnitId = pa3.Id AND p.BigUnitId > 0
                                    WHERE {0} AND a.ReversedStatus = '0' and a.Deleted = 0 ) UNION (SELECT 
                                       count(1) as counter
                                    FROM
                                        ReturnBills a
                                            INNER JOIN
                                        ReturnItems b ON a.Id = b.ReturnBillId
                                            LEFT JOIN
                                        ReturnReservationBills c ON a.ReturnReservationBillId = c.Id
                                            LEFT JOIN
                                        dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id
                                            LEFT JOIN
                                        WareHouses w ON a.WareHouseId = w.Id
                                            LEFT JOIN
                                        Products p ON b.ProductId = p.Id
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa1 ON p.SmallUnitId = pa1.Id
                                            AND p.SmallUnitId > 0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa2 ON p.StrokeUnitId = pa2.Id
                                            AND p.StrokeUnitId > 0
                                            LEFT JOIN
                                        SpecificationAttributeOptions pa3 ON p.BigUnitId = pa3.Id AND p.BigUnitId > 0
                                    WHERE {0} AND a.ReversedStatus = '0' and a.Deleted = 0 )) as allis";

                    //MYSQL
                    string sqlString = @"(SELECT 
                                    a.Id AS BillId,
                                    a.TerminalId,
                                    IFNULL(t.Name, '') TerminalName,
                                    IFNULL(t.Code, '') TerminalCode,
                                    b.ProductId,
                                    p.ProductCode ProductCode,
                                    IFNULL(p.Name, '') ProductName,
                                    p.SmallBarCode,
                                    p.StrokeBarCode,
                                    p.BigBarCode,
                                    p.SmallUnitId,
                                    IFNULL(pa1.Name, '') SmallUnitName,
                                    p.StrokeUnitId,
                                    IFNULL(pa2.Name, '') StrokeUnitName,
                                    p.BigUnitId,
                                    IFNULL(pa3.Name, '') BigUnitName,
                                    p.BigQuantity,
                                    p.StrokeQuantity,
                                    (CASE
                                        WHEN b.UnitId = p.SmallUnitId THEN b.Quantity
                                        ELSE 0
                                    END) SaleSmallQuantity,
                                    (CASE
                                        WHEN b.UnitId = p.StrokeUnitId THEN b.Quantity
                                        ELSE 0
                                    END) SaleStrokeQuantity,
                                    (CASE
                                        WHEN b.UnitId = p.BigUnitId THEN b.Quantity
                                        ELSE 0
                                    END) SaleBigQuantity,
                                    '' SaleQuantityConversion,
                                    b.Amount SaleAmount,
                                    0 ReturnSmallQuantity,
                                    0 ReturnStrokeQuantity,
                                    0 ReturnBigQuantity,
                                    '' ReturnQuantityConversion,
                                    0.00 ReturnAmount,
                                    0 RepaymentSmallQuantity,
                                    0 RepaymentStrokeQuantity,
                                    0 RepaymentBigQuantity,
                                    '' RepaymentQuantityConversion,
                                    0.00 RepaymentAmount,
                                    '' SumQuantityConversion,
                                    0.00 SumAmount,
                                    b.CostAmount,
                                    b.Profit,
                                    b.CostProfitRate,
                                    0.00 AS DiscountAmount,
                                    0 AS SumRowType,
                                    b.IsGifts
                                FROM
                                    SaleBills a
                                        INNER JOIN
                                    SaleItems b ON a.Id = b.SaleBillId
                                        LEFT JOIN
                                    SaleReservationBills c ON a.SaleReservationBillId = c.Id
                                        LEFT JOIN
                                    dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id
                                        LEFT JOIN
                                    WareHouses w ON a.WareHouseId = w.Id
                                        LEFT JOIN
                                    Products p ON b.ProductId = p.Id
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa1 ON p.SmallUnitId = pa1.Id
                                        AND p.SmallUnitId > 0
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa2 ON p.StrokeUnitId = pa2.Id
                                        AND p.StrokeUnitId > 0
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa3 ON p.BigUnitId = pa3.Id AND p.BigUnitId > 0
                                WHERE {0} AND a.ReversedStatus = '0' and a.Deleted = 0) UNION (SELECT 
                                    a.Id AS BillId,
                                    a.TerminalId,
                                    IFNULL(t.Name, '') TerminalName,
                                    IFNULL(t.Code, '') TerminalCode,
                                    b.ProductId,
                                    p.ProductCode ProductCode,
                                    IFNULL(p.Name, '') ProductName,
                                    p.SmallBarCode,
                                    p.StrokeBarCode,
                                    p.BigBarCode,
                                    p.SmallUnitId,
                                    IFNULL(pa1.Name, '') SmallUnitName,
                                    p.StrokeUnitId,
                                    IFNULL(pa2.Name, '') StrokeUnitName,
                                    p.BigUnitId,
                                    IFNULL(pa3.Name, '') BigUnitName,
                                    p.BigQuantity,
                                    p.StrokeQuantity,
                                    0 SaleSmallQuantity,
                                    0 SaleStrokeQuantity,
                                    0 SaleBigQuantity,
                                    '' SaleQuantityConversion,
                                    0.00 SaleAmount,
                                    (CASE
                                        WHEN b.UnitId = p.SmallUnitId THEN b.Quantity
                                        ELSE 0
                                    END) ReturnSmallQuantity,
                                    (CASE
                                        WHEN b.UnitId = p.StrokeUnitId THEN b.Quantity
                                        ELSE 0
                                    END) ReturnStrokeQuantity,
                                    (CASE
                                        WHEN b.UnitId = p.BigUnitId THEN b.Quantity
                                        ELSE 0
                                    END) ReturnBigQuantity,
                                    '' ReturnQuantityConversion,
                                    b.Amount ReturnAmount,
                                    0 RepaymentSmallQuantity,
                                    0 RepaymentStrokeQuantity,
                                    0 RepaymentBigQuantity,
                                    '' RepaymentQuantityConversion,
                                    0.00 RepaymentAmount,
                                    '' SumQuantityConversion,
                                    0.00 SumAmount,
                                    b.CostAmount,
                                    b.Profit,
                                    b.CostProfitRate,
                                    0.00 AS DiscountAmount,
                                    0 AS SumRowType,
                                    b.IsGifts
                                FROM
                                    ReturnBills a
                                        INNER JOIN
                                    ReturnItems b ON a.Id = b.ReturnBillId
                                        LEFT JOIN
                                    ReturnReservationBills c ON a.ReturnReservationBillId = c.Id
                                        LEFT JOIN
                                    dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id
                                        LEFT JOIN
                                    WareHouses w ON a.WareHouseId = w.Id
                                        LEFT JOIN
                                    Products p ON b.ProductId = p.Id
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa1 ON p.SmallUnitId = pa1.Id
                                        AND p.SmallUnitId > 0
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa2 ON p.StrokeUnitId = pa2.Id
                                        AND p.StrokeUnitId > 0
                                        LEFT JOIN
                                    SpecificationAttributeOptions pa3 ON p.BigUnitId = pa3.Id AND p.BigUnitId > 0
                                WHERE {0} AND a.ReversedStatus = '0' and a.Deleted = 0)";


                    var sbCount = string.Format(counterSQL, whereQuery);
                    int totalCount = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sbCount).ToList().FirstOrDefault().Value ?? 0;
                    if (pageSize == 0)
                    {
                        pageSize = totalCount;
                    }

                    var sbQuery = string.Format(sqlString, whereQuery);
                    //string sbQueryString = $"SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY BillId) AS RowNum, alls.* FROM({sbQuery}) as alls ) AS result  WHERE RowNum >= {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize} ORDER BY BillId desc;";
                    //var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryCustomerProduct>(sbQueryString).ToList();
                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryCustomerProduct>(sbQuery).ToList();

                    if (items != null && items.Count > 0)
                    {
                        foreach (var item in items.OrderBy(a => a.TerminalId))
                        {
                            //客户/商品
                            var s = reporinting.Where(b => b.TerminalId == item.TerminalId && b.ProductId == item.ProductId).FirstOrDefault();
                            if (s != null)
                            {
                                s.SaleSmallQuantity = (s.SaleSmallQuantity ?? 0) + (item.SaleSmallQuantity ?? 0);
                                s.SaleStrokeQuantity = (s.SaleStrokeQuantity ?? 0) + (item.SaleStrokeQuantity ?? 0);
                                s.SaleBigQuantity = (s.SaleBigQuantity ?? 0) + (item.SaleBigQuantity ?? 0);
                                s.SaleAmount = (s.SaleAmount ?? 0) + (item.SaleAmount ?? 0);
                                if (item.IsGifts) 
                                {
                                    s.GiftSmallQuantity = (s.GiftSmallQuantity ?? 0) + (item.SaleSmallQuantity ?? 0);
                                    s.GiftStrokeQuantity = (s.GiftStrokeQuantity ?? 0) + (item.SaleStrokeQuantity ?? 0);
                                    s.GiftBigQuantity = (s.GiftBigQuantity ?? 0) + (item.SaleBigQuantity ?? 0);
                                    //销售数量不包含赠送数量
                                    s.SaleSmallQuantity -= item.SaleSmallQuantity ?? 0;
                                    s.SaleStrokeQuantity -= item.SaleStrokeQuantity ?? 0;
                                    s.SaleBigQuantity -= item.SaleBigQuantity ?? 0;
                                }

                                s.ReturnSmallQuantity = (s.ReturnSmallQuantity ?? 0) + (item.ReturnSmallQuantity ?? 0);
                                s.ReturnStrokeQuantity = (s.ReturnStrokeQuantity ?? 0) + (item.ReturnStrokeQuantity ?? 0);
                                s.ReturnBigQuantity = (s.ReturnBigQuantity ?? 0) + (item.ReturnBigQuantity ?? 0);
                                s.ReturnAmount = (s.ReturnAmount ?? 0) + (item.ReturnAmount ?? 0);

                                s.RepaymentSmallQuantity = (s.RepaymentSmallQuantity ?? 0) + (item.RepaymentSmallQuantity ?? 0);
                                s.RepaymentStrokeQuantity = (s.RepaymentStrokeQuantity ?? 0) + (item.RepaymentStrokeQuantity ?? 0);
                                s.RepaymentBigQuantity = (s.RepaymentBigQuantity ?? 0) + (item.RepaymentBigQuantity ?? 0);
                                s.RepaymentAmount = (s.RepaymentAmount ?? 0) + (item.RepaymentAmount ?? 0);

                                s.SumAmount = (s.SumAmount ?? 0) + (item.SaleAmount ?? 0) + (item.ReturnAmount ?? 0) + (item.RepaymentAmount ?? 0);
                                s.CostAmount = (s.CostAmount ?? 0) + (item.CostAmount ?? 0);
                                s.Profit = s.SumAmount - s.CostAmount;
                                if (item.CostAmount == 0 || item.CostAmount == null)
                                {
                                    s.CostProfitRate = 100;
                                }
                                else
                                {
                                    s.CostProfitRate = ((item.Profit ?? 0) / item.CostAmount) * 100;
                                }

                                s.DiscountAmount = (s.DiscountAmount ?? 0) + (item.DiscountAmount ?? 0);
                            }
                            else
                            {
                                s = new SaleReportSummaryCustomerProduct
                                {
                                    SumRowType = false,
                                    TerminalId = item.TerminalId,
                                    TerminalName = item.TerminalName,
                                    TerminalCode = item.TerminalCode,
                                    ProductId = item.ProductId,
                                    ProductCode = item.ProductCode,
                                    ProductName = item.ProductName,
                                    SmallBarCode = item.SmallBarCode,
                                    StrokeBarCode = item.StrokeBarCode,
                                    BigBarCode = item.BigBarCode,
                                    SmallUnitId = item.SmallUnitId,
                                    SmallUnitName = item.SmallUnitName,
                                    StrokeUnitId = item.StrokeUnitId,
                                    StrokeUnitName = item.StrokeUnitName,
                                    BigUnitId = item.BigUnitId,
                                    BigUnitName = item.BigUnitName,
                                    BigQuantity = item.BigQuantity,
                                    StrokeQuantity = item.StrokeQuantity


                                };
                                s.SaleSmallQuantity = item.SaleSmallQuantity;
                                s.SaleStrokeQuantity = item.SaleStrokeQuantity;
                                s.SaleBigQuantity = item.SaleBigQuantity;
                                s.SaleAmount = item.SaleAmount;
                                //赠送数量
                                if (item.IsGifts)
                                {
                                    s.GiftSmallQuantity = item.SaleSmallQuantity ?? 0;
                                    s.GiftStrokeQuantity = item.SaleStrokeQuantity ?? 0;
                                    s.GiftBigQuantity = item.SaleBigQuantity ?? 0;
                                    //销售数量不包含赠送数量
                                    s.SaleSmallQuantity -= item.SaleSmallQuantity ?? 0;
                                    s.SaleStrokeQuantity -= item.SaleStrokeQuantity ?? 0;
                                    s.SaleBigQuantity -= item.SaleBigQuantity ?? 0;
                                }
                                s.ReturnSmallQuantity = item.ReturnSmallQuantity;
                                s.ReturnStrokeQuantity = item.ReturnStrokeQuantity;
                                s.ReturnBigQuantity = item.ReturnBigQuantity;
                                s.ReturnAmount = item.ReturnAmount;

                                s.RepaymentSmallQuantity = item.RepaymentSmallQuantity;
                                s.RepaymentStrokeQuantity = item.RepaymentStrokeQuantity;
                                s.RepaymentBigQuantity = item.RepaymentBigQuantity;
                                s.RepaymentAmount = item.RepaymentAmount;

                                s.SumAmount = (item.SaleAmount ?? 0) + (item.ReturnAmount ?? 0) + (item.RepaymentAmount ?? 0);
                                s.CostAmount = item.CostAmount;
                                s.Profit = s.SumAmount - s.CostAmount;
                                if (item.CostAmount == 0 || item.CostAmount == null)
                                {
                                    s.CostProfitRate = 100;
                                }
                                else
                                {
                                    s.CostProfitRate = ((item.Profit ?? 0) / item.CostAmount) * 100;
                                }

                                s.DiscountAmount = item.DiscountAmount;

                                reporinting.Add(s);

                            }
                        }
                    }

                    //将单位整理
                    if (reporinting != null && reporinting.Count > 0)
                    {
                        foreach (var item in reporinting)
                        {
                            var product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                            var dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                            //销售
                            int sumSaleQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumSaleQuantity += (item.SaleBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumSaleQuantity += (item.SaleStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumSaleQuantity += (item.SaleSmallQuantity ?? 0);
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

                            var salequantity = Pexts.StockQuantityFormat(sumSaleQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.SaleBigQuantity = salequantity.Item1;
                            item.SaleStrokeQuantity = salequantity.Item2;
                            item.SaleSmallQuantity = salequantity.Item3;
                            item.SaleQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSaleQuantity);
                            item.GiftQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumGiftQuantity);
                            //退货
                            int sumReturnQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumReturnQuantity += (item.ReturnBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumReturnQuantity += (item.ReturnStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumReturnQuantity += (item.ReturnSmallQuantity ?? 0);

                            var returnquantity = Pexts.StockQuantityFormat(sumReturnQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.ReturnBigQuantity = returnquantity.Item1;
                            item.ReturnStrokeQuantity = returnquantity.Item2;
                            item.ReturnSmallQuantity = returnquantity.Item3;
                            item.ReturnQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumReturnQuantity);

                            //还货
                            int sumNetQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumNetQuantity += (item.RepaymentBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumNetQuantity += (item.RepaymentStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumNetQuantity += (item.RepaymentSmallQuantity ?? 0);

                            var repaymentquantity = Pexts.StockQuantityFormat(sumNetQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.RepaymentBigQuantity = repaymentquantity.Item1;
                            item.RepaymentStrokeQuantity = repaymentquantity.Item2;
                            item.RepaymentSmallQuantity = repaymentquantity.Item3;
                            item.RepaymentQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumNetQuantity);

                            //总数量
                            int sumSumQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumSumQuantity += (((item.SaleBigQuantity ?? 0) - (item.ReturnBigQuantity ?? 0) + (item.RepaymentBigQuantity ?? 0))) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumSumQuantity += ((item.SaleStrokeQuantity ?? 0) - (item.ReturnStrokeQuantity ?? 0) + (item.RepaymentStrokeQuantity ?? 0)) * (item.StrokeQuantity ?? 0);
                            }
                            sumSumQuantity += ((item.SaleSmallQuantity ?? 0) - (item.ReturnSmallQuantity ?? 0) + (item.RepaymentSmallQuantity ?? 0));

                            item.SumQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSumQuantity + sumGiftQuantity);

                        }
                    }

                    //增加客户分组行
                    if (reporinting != null && reporinting.Count > 0)
                    {
                        List<int> terminalIds = reporinting.Select(a => a.TerminalId ?? 0).Distinct().ToList();
                        foreach (int t in terminalIds)
                        {
                            var terminalName = reporinting.Where(a => a.TerminalId == t).Select(a => a.TerminalName).FirstOrDefault();
                            var s = new SaleReportSummaryCustomerProduct
                            {
                                SumRowType = true,
                                TerminalId = t,
                                TerminalName = terminalName,
                                SaleSmallQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.SaleSmallQuantity ?? 0),
                                SaleStrokeQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.SaleStrokeQuantity ?? 0),
                                SaleBigQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.SaleBigQuantity ?? 0)
                            };
                            s.SaleQuantityConversion = s.SaleBigQuantity + "大" + s.SaleStrokeQuantity + "中" + s.SaleSmallQuantity + "小";
                            s.SaleAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.SaleAmount ?? 0);
                            s.GiftQuantityConversion = s.GiftBigQuantity + "大" + s.GiftStrokeQuantity + "中" + s.GiftSmallQuantity + "小";

                            s.ReturnSmallQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.ReturnSmallQuantity ?? 0);
                            s.ReturnStrokeQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.ReturnStrokeQuantity ?? 0);
                            s.ReturnBigQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.ReturnBigQuantity ?? 0);
                            s.ReturnQuantityConversion = s.ReturnBigQuantity + "大" + s.ReturnStrokeQuantity + "中" + s.ReturnSmallQuantity + "小";
                            s.ReturnAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.ReturnAmount ?? 0);

                            s.RepaymentSmallQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.RepaymentSmallQuantity ?? 0);
                            s.RepaymentStrokeQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.RepaymentStrokeQuantity ?? 0);
                            s.RepaymentBigQuantity = reporinting.Where(a => a.TerminalId == t).Sum(a => a.RepaymentBigQuantity ?? 0);
                            s.RepaymentQuantityConversion = s.RepaymentBigQuantity + "大" + s.RepaymentStrokeQuantity + "中" + s.RepaymentSmallQuantity + "小";
                            s.RepaymentAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.RepaymentAmount ?? 0);

                            s.SumQuantityConversion = ((s.SaleBigQuantity ?? 0) + (s.GiftBigQuantity) - (s.ReturnBigQuantity ?? 0) + (s.RepaymentBigQuantity ?? 0)) + "大" +
                                                      ((s.SaleStrokeQuantity ?? 0) + (s.GiftStrokeQuantity) - (s.ReturnStrokeQuantity ?? 0) + (s.RepaymentStrokeQuantity ?? 0)) + "中" +
                                                      ((s.SaleSmallQuantity ?? 0) + (s.GiftSmallQuantity) - (s.ReturnSmallQuantity ?? 0) + (s.RepaymentSmallQuantity ?? 0)) + "小";

                            s.SumAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.SaleAmount ?? 0) +
                                          reporinting.Where(a => a.TerminalId == t).Sum(a => a.ReturnAmount ?? 0) +
                                          reporinting.Where(a => a.TerminalId == t).Sum(a => a.RepaymentAmount ?? 0);
                            s.CostAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.CostAmount ?? 0);
                            s.Profit = reporinting.Where(a => a.TerminalId == t).Sum(a => a.Profit ?? 0);
                            if (s.CostAmount == null || s.CostAmount == 0)
                            {
                                s.CostProfitRate = 100;
                            }
                            else
                            {
                                s.CostProfitRate = ((s.Profit ?? 0) / s.CostAmount) * 100;
                            }

                            s.DiscountAmount = reporinting.Where(a => a.TerminalId == t).Sum(a => a.DiscountAmount ?? 0);

                            reporinting.Add(s);
                        }
                    }

                    var pitems = reporinting.OrderBy(a => a.TerminalId).OrderBy(a => a.SumRowType).ToList();
                    return pitems;
                }, force);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 销售汇总（按仓库）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="dic">动态列</param>
        /// <returns></returns>
        public IList<SaleReportSummaryWareHouse> GetSaleReportSummaryWareHouse(int? storeId, int? wareHouseId, DateTime? startTime, DateTime? endTime, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_WAREHOUSE_KEY.FillCacheKey(storeId, wareHouseId, startTime, endTime, string.Join("-", dic.Select(s => s.Key).ToArray()), auditedStatus);
                return _cacheManager.Get(key, () =>
                {

                    string whereQuery = $" a.StoreId= {storeId ?? 0}";

                    if (wareHouseId.HasValue && wareHouseId.Value != 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (startTime.HasValue)
                    {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }

                    if (auditedStatus.HasValue)
                    {
                        whereQuery += $" and a.AuditedStatus = {auditedStatus}";
                    }

                    //MSSQL
                    //string sqlString = $"(select  a.WareHouseId ,w.Name as WareHouseName ,b.ProductId ,p.ProductCode ,p.Name ProductName ,p.StatisticalType StatisticalTypeId ,p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.[Name] SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId  ,pa3.Name BigUnitName ,p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount  ,0 ReturnQuantity  ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate  from  SaleBills a   inner join Items b on a.Id=b.SaleBillId  inner join Products p on b.ProductId=p.Id  left join WareHouses w on a.WareHouseId=w.Id  left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id  left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id  left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id  where  {whereQuery} and a.AuditedStatus=1 and a.ReversedStatus=0)  UNION ALL (select a.WareHouseId ,w.Name WareHouseName ,b.ProductId,p.ProductCode ,p.Name ProductName,p.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName ,p.BigQuantity,p.StrokeQuantity ,b.Quantity SaleQuantity,b.UnitId SaleUnitId ,b.Amount SaleAmount ,0 ReturnQuantity,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount,b.CostAmount ,b.Profit,b.CostProfitRate  from  SaleBills a inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join WareHouses w on a.WareHouseId=w.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0)";

                    //MYSQL
                    //string sqlString = $"(select a.Id as BillId, a.WareHouseId ,w.Name as WareHouseName ,b.ProductId ,p.ProductCode ,p.Name ProductName, c.StatisticalType StatisticalTypeId ,p.SmallBarCode ,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId  ,pa3.Name BigUnitName ,p.BigQuantity ,p.StrokeQuantity ,b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount  ,0 ReturnQuantity  ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate  from  SaleBills a   inner join SaleItems b on a.Id=b.SaleBillId  inner join Products p on b.ProductId=p.Id  left join WareHouses w on a.WareHouseId=w.Id  left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id  left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id  left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id  where  {whereQuery} and a.AuditedStatus=1 and a.ReversedStatus=0 and a.Deleted = 0)  UNION ALL (select a.Id as BillId, a.WareHouseId ,w.Name WareHouseName ,b.ProductId,p.ProductCode ,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName ,p.BigQuantity,p.StrokeQuantity ,b.Quantity SaleQuantity,b.UnitId SaleUnitId ,b.Amount SaleAmount ,0 ReturnQuantity,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount,b.CostAmount ,b.Profit,b.CostProfitRate  from  SaleBills a inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join WareHouses w on a.WareHouseId=w.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.AuditedStatus = 1 and a.ReversedStatus = 0 and a.Deleted = 0)";
                    string sqlString = $"(select a.Id as BillId, a.WareHouseId ,w.Name as WareHouseName ,b.ProductId ,p.ProductCode ,p.Name ProductName, c.StatisticalType StatisticalTypeId ,p.SmallBarCode " +
                    $",p.StrokeBarCode ,p.BigBarCode ,p.SmallUnitId ,pa1.Name SmallUnitName ,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId  ,pa3.Name BigUnitName ,p.BigQuantity ,p.StrokeQuantity " +
                    $",b.Quantity SaleQuantity ,b.UnitId SaleUnitId ,b.Amount SaleAmount  ,0 ReturnQuantity  ,0 ReturnUnitId ,0.00 ReturnAmount ,0 NetQuantity ,0.00 NetAmount ,b.CostAmount ,b.Profit ,b.CostProfitRate,b.IsGifts  " +
                    $"from  SaleBills a   " +
                    $"inner join SaleItems b on a.Id=b.SaleBillId  " +
                    $"inner join Products p on b.ProductId=p.Id  " +
                    $"left join WareHouses w on a.WareHouseId=w.Id  " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id  " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id  " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id  where  {whereQuery} and a.ReversedStatus=0 and a.Deleted = 0)  " +
                    $"UNION ALL " +
                    $"(select a.Id as BillId, a.WareHouseId ,w.Name WareHouseName ,b.ProductId,p.ProductCode ,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode ,p.BigBarCode " +
                    $",p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId ,pa2.Name StrokeUnitName ,p.BigUnitId ,pa3.Name BigUnitName ,p.BigQuantity,p.StrokeQuantity ,0 SaleQuantity,0 SaleUnitId " +
                    $",0 SaleAmount ,b.Quantity ReturnQuantity,b.UnitId ReturnUnitId ,b.Amount ReturnAmount ,0 NetQuantity ,0.00 NetAmount,b.CostAmount ,b.Profit,b.CostProfitRate,b.IsGifts  " +
                    $"from  ReturnBills a " +
                    $"inner join ReturnItems b on a.Id=b.ReturnBillId " +
                    $"inner join Products p on b.ProductId=p.Id " +
                    $"left join WareHouses w on a.WareHouseId=w.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus = 0 and a.Deleted = 0)";

                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryWareHouseQuery>(sqlString).ToList();

                    //items = items.GroupBy(n => n.BillId).Select(n => n.First()).ToList();

                    //对查询结果进行转换
                    var srsws = new List<SaleReportSummaryWareHouse>();
                    //添加动态列

                    if (items != null && items.Count > 0)
                    {
                        items.ForEach(a =>
                        {
                            //如果商品没有配置统计类别,则统计类别默认其他
                            var disKeys = dic.Keys.ToList();
                            if (!disKeys.Contains(a.StatisticalTypeId ?? 0))
                            {
                                a.StatisticalTypeId = (int)StatisticalTypeEnum.OtherTypeId;
                            }

                            //按仓库查询
                            var srsw = srsws.Where(s => s.WareHouseId == a.WareHouseId).FirstOrDefault();
                            if (srsw != null)
                            {
                                srsw.SaleReportStatisticalTypes.ToList().ForEach(s =>
                                {

                                    if (s.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //销售数量
                                        int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        //退货数量
                                        int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                        //赠送数量
                                        int giftQuantity = 0;
                                        if (a.IsGifts)
                                        {
                                            giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        }
                                        //净销数量
                                        s.NetSmallQuantity = (s.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s.NetAmount = (s.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s.CostAmount = (s.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s.Profit = s.NetAmount - s.CostAmount;
                                        //成本利润率
                                        if (s.CostAmount == null || s.CostAmount == 0)
                                        {
                                            s.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s.CostProfitRate = (s.Profit / s.CostAmount) * 100;
                                        }

                                        //主列
                                        //销售数量 包含赠品
                                        srsw.SaleSmallQuantity = (srsw.SaleSmallQuantity ?? 0) + saleQuantity;
                                        //退货数量
                                        srsw.ReturnSmallQuantity = (srsw.ReturnSmallQuantity ?? 0) + returnQuantity;
                                        //赠送数量
                                        srsw.GiftQuantity = (srsw.GiftQuantity ?? 0) + giftQuantity;
                                        //净销售量 = 销售数量 - 退货数量
                                        srsw.NetSmallQuantity = (srsw.NetSmallQuantity ?? 0) + (srsw.SaleSmallQuantity - srsw.ReturnSmallQuantity);
                                        //销售金额
                                        srsw.SaleAmount = (srsw.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                        //退货金额
                                        srsw.ReturnAmount = (srsw.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                        //销售净额 = 销售金额 - 退货金额
                                        srsw.NetAmount = (srsw.NetAmount ?? 0) + (srsw.SaleAmount - srsw.ReturnAmount);
                                        //优惠
                                        //成本
                                        srsw.CostAmount = (srsw.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        srsw.Profit = srsw.NetAmount - srsw.CostAmount;
                                        //成本利润率
                                        if (srsw.CostAmount == null || srsw.CostAmount == 0)
                                        {
                                            srsw.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            srsw.CostProfitRate = (srsw.Profit / srsw.CostAmount) * 100;
                                        }

                                    }
                                });

                            }
                            else
                            {
                                srsw = new SaleReportSummaryWareHouse
                                {
                                    WareHouseId = a.WareHouseId,
                                    WareHouseName = a.WareHouseName
                                };

                                //添加动态列
                                if (dic != null && dic.Count > 0)
                                {
                                    dic.ToList().ForEach(d =>
                                    {
                                        srsw.SaleReportStatisticalTypes.Add(new SaleReportStatisticalType() { StatisticalTypeId = d.Key });
                                    });
                                }
                                srsw.SaleReportStatisticalTypes.ToList().ForEach(s2 =>
                                {
                                    if (s2.StatisticalTypeId == a.StatisticalTypeId)
                                    {
                                        //销售数量
                                        int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        //退货数量
                                        int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                        //赠送数量
                                        int giftQuantity = 0;
                                        if (a.IsGifts)
                                        {
                                            giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                        }
                                        //净销数量
                                        s2.NetSmallQuantity = (s2.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                        //销售净额
                                        s2.NetAmount = (s2.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                        //成本
                                        s2.CostAmount = (s2.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        s2.Profit = s2.NetAmount - s2.CostAmount;
                                        //成本利润率
                                        if (s2.CostAmount == null || s2.CostAmount == 0)
                                        {
                                            s2.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            s2.CostProfitRate = (s2.Profit / s2.CostAmount) * 100;
                                        }

                                        //主列
                                        //销售数量 包含赠品
                                        srsw.SaleSmallQuantity = (srsw.SaleSmallQuantity ?? 0) + saleQuantity;
                                        //退货数量
                                        srsw.ReturnSmallQuantity = (srsw.ReturnSmallQuantity ?? 0) + returnQuantity;
                                        //赠送数量
                                        srsw.GiftQuantity = (srsw.GiftQuantity ?? 0) + giftQuantity;
                                        //净销售量 = 销售数量 - 退货数量
                                        srsw.NetSmallQuantity = (srsw.NetSmallQuantity ?? 0) + (srsw.SaleSmallQuantity - srsw.ReturnSmallQuantity);
                                        //销售金额
                                        srsw.SaleAmount = (srsw.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                        //退货金额
                                        srsw.ReturnAmount = (srsw.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                        //销售净额 = 销售金额 - 退货金额
                                        srsw.NetAmount = (srsw.NetAmount ?? 0) + (srsw.SaleAmount - srsw.ReturnAmount);
                                        //优惠
                                        //成本
                                        srsw.CostAmount = (srsw.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                        //利润
                                        srsw.Profit = srsw.NetAmount - srsw.CostAmount;
                                        //成本利润率
                                        if (srsw.CostAmount == null || srsw.CostAmount == 0)
                                        {
                                            srsw.CostProfitRate = 100;
                                        }
                                        else
                                        {
                                            srsw.CostProfitRate = (srsw.Profit / srsw.CostAmount) * 100;
                                        }

                                    }
                                });
                                srsws.Add(srsw);
                            }
                        });
                    }

                    return srsws;
                }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportSummaryWareHouse>();
            }
        }

        /// <summary>
        /// 销售汇总（按品牌）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="brandIds">品牌Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="dic">动态列</param>
        /// <returns></returns>
        public IList<SaleReportSummaryBrand> GetSaleReportSummaryBrand(int? storeId, int[] brandIds, int? districtId, int? channelId, int? bussinessUserId, int? deliveryUserId, DateTime? startTime, DateTime? endTime, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null)
        {
            try
            {
                var reporting = new List<SaleReportSummaryBrand>();

                string whereQuery = $" a.StoreId= {storeId ?? 0}";

                if (brandIds.Length > 0 && !brandIds.Contains(0))
                {
                    whereQuery += $" and p.BrandId in ('{string.Join(",", brandIds)}') ";
                }

                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
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

                if (channelId.HasValue && channelId.Value != 0)
                {
                    whereQuery += $" and t.ChannelId = '{channelId}' ";
                }

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                {
                    var userIds = _userService.GetSubordinate(storeId, bussinessUserId ?? 0);
                    whereQuery += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                }

                if (deliveryUserId.HasValue && deliveryUserId.Value != 0)
                {
                    whereQuery += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                }

                if (startTime.HasValue)
                {
                    //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                    whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                }

                if (endTime.HasValue)
                {
                    //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                    whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                }

                if (auditedStatus.HasValue)
                {
                    whereQuery += $" and a.AuditedStatus = {auditedStatus}";
                }

                //MSSQL
                //string sqlString = $"(select p.BrandId ,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,p.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,b.Quantity SaleQuantity,b.UnitId SaleUnitId,b.Amount SaleAmount,0 ReturnQuantity,0 ReturnUnitId,0.00 ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate  from SaleBills a  inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id inner join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join Brands d on p.BrandId=d.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select p.BrandId,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,p.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleQuantity,0 SaleUnitId,0.00 SaleAmount,b.Quantity ReturnQuantity,b.UnitId ReturnUnitId,b.Amount ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join Items b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id inner join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join Brands d on p.BrandId = d.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";

                //MYSQL
                //string sqlString = $"(select p.BrandId ,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,b.Quantity SaleQuantity,b.UnitId SaleUnitId,b.Amount SaleAmount,0 ReturnQuantity,0 ReturnUnitId,0.00 ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate  from SaleBills a  inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id inner join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join Brands d on p.BrandId=d.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0' and a.Deleted = 0) UNION ALL (select p.BrandId,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleQuantity,0 SaleUnitId,0.00 SaleAmount,b.Quantity ReturnQuantity,b.UnitId ReturnUnitId,b.Amount ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id inner join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join Brands d on p.BrandId = d.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0' and a.Deleted = 0)";
                string sqlString = $"(select p.BrandId ,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId" +
                    $",pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,b.Quantity SaleQuantity,b.UnitId SaleUnitId,b.Amount SaleAmount" +
                    $",0 ReturnQuantity,0 ReturnUnitId,0.00 ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate,b.IsGifts  " +
                    $"from SaleBills a  " +
                    $"inner join SaleItems b on a.Id=b.SaleBillId " +
                    $"inner join Products p on b.ProductId=p.Id " +
                    $"inner join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id " +
                    $"left join Brands d on p.BrandId=d.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus=0 and a.Deleted = 0) " +
                    $"UNION ALL " +
                    $"(select p.BrandId,d.Name BrandName,b.ProductId,p.ProductCode,p.Name ProductName,c.StatisticalType StatisticalTypeId,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName" +
                    $",p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleQuantity,0 SaleUnitId,0.00 SaleAmount,b.Quantity ReturnQuantity,b.UnitId ReturnUnitId" +
                    $",b.Amount ReturnAmount,0 NetQuantity,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate,b.IsGifts " +
                    $"from ReturnBills a " +
                    $"inner join ReturnItems b on a.Id = b.ReturnBillId " +
                    $"inner join Products p on b.ProductId = p.Id " +
                    $"inner join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id " +
                    $"left join Brands d on p.BrandId = d.Id " +
                    $"left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id " +
                    $"left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id " +
                    $"left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id " +
                    $"LEFT JOIN Categories c ON p.CategoryId=c.Id where {whereQuery} and a.ReversedStatus = 0 and a.Deleted = 0)";

                var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryBrandQuery>(sqlString).ToList();

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

                        //品牌查询
                        var srsb = reporting.Where(s => s.BrandId == a.BrandId).FirstOrDefault();
                        if (srsb != null)
                        {
                            srsb.SaleReportStatisticalTypes.ToList().ForEach(s =>
                            {

                                if (s.StatisticalTypeId == a.StatisticalTypeId)
                                {
                                    //销售数量
                                    int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                    //退货数量
                                    int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                    //赠送数量
                                    int giftQuantity = 0;
                                    if (a.IsGifts)
                                    {
                                        giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                    }
                                    //净销数量
                                    s.NetSmallQuantity = (s.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                    //销售净额
                                    s.NetAmount = (s.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                    //成本
                                    s.CostAmount = (s.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                    //利润
                                    s.Profit = s.NetAmount - s.CostAmount;
                                    //成本利润率
                                    if (s.CostAmount == null || s.CostAmount == 0)
                                    {
                                        s.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        s.CostProfitRate = (s.Profit / s.CostAmount) * 100;
                                    }

                                    //主列
                                    //销售数量 包含赠品
                                    srsb.SaleSmallQuantity = (srsb.SaleSmallQuantity ?? 0) + saleQuantity;
                                    //退货数量
                                    srsb.ReturnSmallQuantity = (srsb.ReturnSmallQuantity ?? 0) + returnQuantity;
                                    //赠送数量
                                    srsb.GiftSmallQuantity = (srsb.GiftSmallQuantity ?? 0) + giftQuantity;
                                    //净销售量 = 销售数量 - 退货数量
                                    //srsb.NetSmallQuantity = (srsb.NetSmallQuantity ?? 0) + (srsb.SaleSmallQuantity - srsb.ReturnSmallQuantity);
                                    srsb.NetSmallQuantity = (srsb.SaleSmallQuantity - srsb.ReturnSmallQuantity);
                                    //销售金额
                                    srsb.SaleAmount = (srsb.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                    //退货金额
                                    srsb.ReturnAmount = (srsb.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                    //销售净额 = 销售金额 - 退货金额
                                    //srsb.NetAmount = (srsb.NetAmount ?? 0) + (srsb.SaleAmount - srsb.ReturnAmount);
                                    srsb.NetAmount = (srsb.SaleAmount - srsb.ReturnAmount);
                                    //优惠
                                    //成本
                                    srsb.CostAmount = (srsb.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                    //利润
                                    srsb.Profit = srsb.NetAmount - srsb.CostAmount;
                                    //成本利润率
                                    if (srsb.CostAmount == null || srsb.CostAmount == 0)
                                    {
                                        srsb.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        srsb.CostProfitRate = (srsb.Profit / srsb.CostAmount) * 100;
                                    }

                                }
                            });

                        }
                        else
                        {
                            srsb = new SaleReportSummaryBrand
                            {
                                BrandId = a.BrandId,
                                BrandName = a.BrandName,
                                SaleSmallQuantity = a.SaleQuantity,
                                SaleAmount = a.SaleAmount,
                                ReturnSmallQuantity = a.ReturnQuantity,
                                ReturnAmount = a.ReturnAmount,
                                NetSmallQuantity = a.SaleQuantity - a.ReturnQuantity,
                                NetAmount = a.SaleAmount - a.ReturnAmount,
                                CostAmount = a.CostAmount,
                                Profit = a.NetAmount - a.CostAmount,
                                CostProfitRate = a.CostProfitRate //(a.Profit / a.CostAmount) * 100
                            };

                            //添加动态列
                            if (dic != null && dic.Count > 0)
                            {
                                dic.ToList().ForEach(d =>
                                {
                                    srsb.SaleReportStatisticalTypes.Add(new SaleReportStatisticalType() { StatisticalTypeId = d.Key });
                                });
                            }
                            else
                            {
                                srsb.SaleReportStatisticalTypes.Add(new SaleReportStatisticalType() { StatisticalTypeId = a.StatisticalTypeId ?? 0 });
                            }


                            srsb.SaleReportStatisticalTypes.ToList().ForEach(s2 =>
                            {
                                if (s2.StatisticalTypeId == a.StatisticalTypeId)
                                {
                                    //销售数量
                                    int saleQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                    //退货数量
                                    int returnQuantity = (a.ReturnQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.ReturnUnitId ?? 0);
                                    //赠送数量
                                    int giftQuantity = 0;
                                    if (a.IsGifts)
                                    {
                                        giftQuantity = (a.SaleQuantity ?? 0) * CommonHelper.GetSmallConversionQuantity(a.BigUnitId ?? 0, a.StrokeUnitId ?? 0, a.SmallUnitId ?? 0, a.BigQuantity ?? 0, a.StrokeQuantity ?? 0, a.SaleUnitId ?? 0);
                                    }
                                    //净销数量
                                    s2.NetSmallQuantity = (s2.NetSmallQuantity ?? 0) + (saleQuantity - returnQuantity);
                                    //销售净额
                                    s2.NetAmount = (s2.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                    //成本
                                    s2.CostAmount = (s2.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                    //利润
                                    s2.Profit = s2.NetAmount - s2.CostAmount;
                                    //成本利润率
                                    if (s2.CostAmount == null || s2.CostAmount == 0)
                                    {
                                        s2.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        s2.CostProfitRate = (s2.Profit / s2.CostAmount) * 100;
                                    }

                                    //主列
                                    //销售数量 包含赠品
                                    //srsb.SaleSmallQuantity = (srsb.SaleSmallQuantity ?? 0) + saleQuantity;
                                    srsb.SaleSmallQuantity = saleQuantity;
                                    //退货数量
                                    srsb.ReturnSmallQuantity = (srsb.ReturnSmallQuantity ?? 0) + returnQuantity;
                                    //赠送数量
                                    srsb.GiftSmallQuantity = (srsb.GiftSmallQuantity ?? 0) + giftQuantity;
                                    //净销售量 = 销售数量 - 退货数量
                                    //srsb.NetSmallQuantity = (srsb.NetSmallQuantity ?? 0) + (srsb.SaleSmallQuantity - srsb.ReturnSmallQuantity);
                                    srsb.NetSmallQuantity = (srsb.SaleSmallQuantity - srsb.ReturnSmallQuantity);
                                    //销售金额
                                    //srsb.SaleAmount = (srsb.SaleAmount ?? 0) + (a.SaleAmount ?? 0);
                                    srsb.SaleAmount = (a.SaleAmount ?? 0);
                                    //退货金额
                                    //srsb.ReturnAmount = (srsb.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);
                                    srsb.ReturnAmount = (a.ReturnAmount ?? 0);
                                    //销售净额 = 销售金额 - 退货金额
                                    //srsb.NetAmount = (srsb.NetAmount ?? 0) + (srsb.SaleAmount - srsb.ReturnAmount);
                                    srsb.NetAmount = (srsb.SaleAmount - srsb.ReturnAmount);
                                    //优惠
                                    //成本
                                    srsb.CostAmount = (srsb.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                    //利润
                                    srsb.Profit = srsb.NetAmount - srsb.CostAmount;
                                    //成本利润率
                                    if (srsb.CostAmount == null || srsb.CostAmount == 0)
                                    {
                                        srsb.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        srsb.CostProfitRate = (srsb.Profit / srsb.CostAmount) * 100;
                                    }
                                }
                            });
                            reporting.Add(srsb);
                        }
                    });
                }
                return reporting;
            }
            catch (Exception)
            {
                return new List<SaleReportSummaryBrand>();
            }
        }

        /// <summary>
        /// 订单明细
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="saleTypeId">销售类型Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="remark">备注</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <param name="occupyStock">只展示占用库存商品</param>
        /// <returns></returns>
        public IList<SaleReportOrderItem> GetSaleReportOrderItem(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? rankId, int? bussinessUserId, int? wareHouseId, int? saleTypeId, int? channelId, DateTime? startTime, DateTime? endTime, int? districtId, string remark, bool? costContractProduct, bool? occupyStock, bool force = false)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTORDER_ITEM_KEY.FillCacheKey(storeId, productId, productName, categoryId, brandId, terminalId, terminalName, billNumber, rankId, bussinessUserId, wareHouseId, saleTypeId, channelId, startTime, endTime, districtId, remark, costContractProduct, occupyStock), () =>
                   {
                       productName = CommonHelper.Filter(productName);
                       terminalName = CommonHelper.Filter(terminalName);
                       billNumber = CommonHelper.Filter(billNumber);
                       remark = CommonHelper.Filter(remark);

                       var reporting = new List<SaleReportOrderItem>();

                       string whereQuery = $" a.StoreId= {storeId ?? 0}";
                       string whereQuery2 = $" a.StoreId= {storeId ?? 0}";
                       string whereQuery3 = $"  1=1";

                       if (productId.HasValue && productId.Value != 0)
                       {
                           whereQuery += $" and b.ProductId = '{productId}' ";
                           whereQuery2 += $" and b.ProductId = '{productId}' ";
                       }
                       if (productName != null)
                       {
                           whereQuery += $" and p.Name like '%{productName}%' ";
                           whereQuery2 += $" and p.Name like '%{productName}%' ";
                       }

                       if (categoryId.HasValue && categoryId.Value != 0)
                       {
                   //递归商品类别查询
                   var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                           if (categoryIds != null && categoryIds.Count > 0)
                           {
                               string incategoryIds = string.Join("','", categoryIds);
                               whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                               whereQuery2 += $" and p.CategoryId in ('{incategoryIds}') ";
                           }
                           else
                           {
                               whereQuery += $" and p.CategoryId = '{categoryId}' ";
                               whereQuery2 += $" and p.CategoryId = '{categoryId}' ";
                           }
                       }

                       if (brandId.HasValue && brandId.Value != 0)
                       {
                           whereQuery += $" and p.BrandId = '{brandId}' ";
                           whereQuery2 += $" and p.BrandId = '{brandId}' ";
                       }

                       if (terminalId.HasValue && terminalId.Value != 0)
                       {
                           whereQuery += $" and a.TerminalId = '{terminalId}' ";
                           whereQuery2 += $" and a.TerminalId = '{terminalId}' ";
                       }

                       if (terminalName != null)
                       {
                           whereQuery += $" and t.Name like '%{terminalName}%' ";
                           whereQuery2 += $" and t.Name like '%{terminalName}%' ";
                       }

                       if (!string.IsNullOrEmpty(billNumber))
                       {
                           whereQuery += $" and a.BillNumber like '%{billNumber}%' ";
                           whereQuery2 += $" and a.BillNumber like '%{billNumber}%' ";
                       }

                       if (rankId.HasValue && rankId.Value != 0)
                       {
                           whereQuery += $" and t.RankId = '{rankId}' ";
                           whereQuery2 += $" and t.RankId = '{rankId}' ";
                       }

                       if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                       {
                           whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                           whereQuery2 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                       }

                       if (wareHouseId.HasValue && wareHouseId.Value != 0)
                       {
                           whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                           whereQuery2 += $" and a.WareHouseId = '{wareHouseId}' ";
                       }

               //SaleReportItemSaleTypeEnum
               //销售类型
               if (saleTypeId > 0)
                       {
                   //销售商品
                   if (saleTypeId == (int)SaleReportOrderItemSaleTypeEnum.SaleProduct)
                           {
                               whereQuery2 += $" and 1=2 ";
                           }
                   //退货商品
                   else if (saleTypeId == (int)SaleReportOrderItemSaleTypeEnum.ReturnProduct)
                           {
                               whereQuery += $" and 1=2 ";
                           }
                       }

                       if (channelId.HasValue && channelId.Value != 0)
                       {
                           whereQuery += $" and t.ChannelId = '{channelId}' ";
                           whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                       }

					   if (districtId.HasValue && districtId.Value != 0)
					   {
						   //递归片区查询
						   var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
						   if (distinctIds != null && distinctIds.Count > 0)
						   {
							   string inDistinctIds = string.Join("','", distinctIds);
							   whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
							   whereQuery2 += $" and t.DistrictId in ('{inDistinctIds}') ";
						   }
						   else
						   {
							   whereQuery += $" and t.DistrictId = '{districtId}' ";
							   whereQuery2 += $" and t.DistrictId = '{districtId}' ";
						   }
					   }

					   if (!string.IsNullOrEmpty(remark))
                       {
                           whereQuery += $" and b.Remark like '%{remark}%' ";
                           whereQuery2 += $" and b.Remark like '%{remark}%' ";
                       }

                       if (channelId.HasValue && channelId.Value != 0)
                       {
                           whereQuery += $" and t.ChannelId = '{channelId}' ";
                           whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                       }

               //费用合同兑换商品
               if (costContractProduct.HasValue)
                       {
                   //费用合同兑换
                   if (costContractProduct.Value == true)
                           {
                               whereQuery += $" and b.CostContractId > 0 ";
                           }
                           else
                           {
                               whereQuery += $" and (b.CostContractId = 0 or b.CostContractId is null )";
                           }
                       }


               //只展示占用库存商品
               if (occupyStock.HasValue)
                       {
                           whereQuery3 += $" and OccupyStock = '{occupyStock}' ";
                       }

                       if (startTime.HasValue)
                       {
                   //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                   string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                           whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                       }

                       if (endTime.HasValue)
                       {
                   //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                   string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                           whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                       }

                       //MSSQL
                       //string sqlString = $"(select a.Id ReservationBillId ,a.BillNumber as ReservationBillNumber,1 BillTypeId,'销售' BillTypeName,a.TerminalId,t.Name TerminalName,t.Code TerminalCode,a.BusinessUserId,'' BusinessUserName,a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName,b.ProductId,p.Sku ProductSKU,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleReturnSmallQuantity ,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleReturnStrokeQuantity ,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleReturnBigQuantity ,(case when b.UnitId=p.SmallUnitId then p.SmallBarCode when b.UnitId=p.StrokeUnitId then p.StrokeBarCode when b.UnitId=p.BigUnitId then p.BigBarCode else '' end ) BarCode ,'' UnitConversion,b.Quantity Quantity,b.UnitId,(case when b.UnitId=p.SmallUnitId then pa1.Name when b.UnitId=p.StrokeUnitId then pa2.Name when b.UnitId=p.BigUnitId then pa3.Name else '' end ) UnitName,(case when (select count(*) from GiveQuotaRecords where ContractId>0 and ProductId=p.Id)>0 then CAST( 1 AS bit ) else CAST( 0 AS bit ) end ) CostContractProduct,(case when (select count(*) from Stocks where UsableQuantity>0 and ProductId=p.Id)>0 then CAST( 1 AS bit ) else CAST( 0 AS bit ) end ) OccupyStock,b.Price,b.Amount,b.CostAmount,b.Profit,b.CostProfitRate,0.00 SystemPrice,b.Amount- (pp.RetailPrice*b.Quantity) ChangeDifference,0.00 PresetPrice,0.00 RecentPurchasesPrice,0.00 RecentSettlementCostPrice,b.Remark from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join WareHouses w on a.WareHouseId=w.Id left join Products p on b.ProductId=p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id left join ProductPrices pp on b.ProductId=pp.ProductId and b.UnitId=pp.UnitId where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select a.Id ReservationBillId,a.BillNumber ReservationBillNumber,2 BillTypeId,'退货' BillTypeName,a.TerminalId,t.Name TerminalName,t.Code TerminalCode,a.BusinessUserId,'' BusinessUserName,a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName,b.ProductId,p.Sku ProductSKU,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleReturnSmallQuantity ,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleReturnStrokeQuantity ,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleReturnBigQuantity ,(case when b.UnitId=p.SmallUnitId then p.SmallBarCode when b.UnitId=p.StrokeUnitId then p.StrokeBarCode when b.UnitId=p.BigUnitId then p.BigBarCode else '' end ) BarCode,'' UnitConversion,b.Quantity Quantity,b.UnitId,(case when b.UnitId=p.SmallUnitId then pa1.Name when b.UnitId=p.StrokeUnitId then pa2.Name when b.UnitId=p.BigUnitId then pa3.Name else '' end ) UnitName,(case when (select count(*) from GiveQuotaRecords where ContractId>0 and ProductId=p.Id)>0 then CAST( 1 AS bit ) else CAST( 0 AS bit ) end ) CostContractProduct,(case when (select count(*) from Stocks where UsableQuantity>0 and ProductId=p.Id)>0 then CAST( 1 AS bit ) else CAST( 0 AS bit ) end ) OccupyStock,b.Price,b.Amount,b.CostAmount,b.Profit,b.CostProfitRate,0.00 SystemPrice,b.Amount- (pp.RetailPrice*b.Quantity) ChangeDifference,0.00 PresetPrice,0.00 RecentPurchasesPrice,0.00 RecentSettlementCostPrice,b.Remark from ReturnReservationBills a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join WareHouses w on a.WareHouseId=w.Id left join Products p on b.ProductId=p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id left join ProductPrices pp on b.ProductId=pp.ProductId and b.UnitId=pp.UnitId where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";


                       //MYSQL
                       string sqlString = $"select * from ((select a.Id ReservationBillId ,a.BillNumber as ReservationBillNumber,{(int)BillTypeEnum.SaleReservationBill} as BillTypeId,'销售' BillTypeName,a.TerminalId,t.Name TerminalName,t.Code TerminalCode,t.DistrictId,a.BusinessUserId,u.UserRealName  BusinessUserName,a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName,b.ProductId,p.Sku ProductSKU,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) as SaleReturnSmallQuantity ,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) as SaleReturnStrokeQuantity ,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) as SaleReturnBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) as BarCode ,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,b.Quantity Quantity, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) as UnitName,(select count(*) from GiveQuotaRecords where ContractId > 0 and ProductId = p.Id ) as CostContractProduct,(select count(*) from Stocks where UsableQuantity > 0 and ProductId = p.Id) as OccupyStock,b.Price,b.Amount,b.CostAmount,b.Profit,b.CostProfitRate,0.00 SystemPrice,(b.Amount - (pp.RetailPrice * b.Quantity)) as ChangeDifference,0.00 PresetPrice,0.00 as  RecentPurchasesPrice,0.00 RecentSettlementCostPrice,b.Remark,'' as UnitBigStrokeSmall from SaleReservationBills a inner join SaleReservationItems b on a.Id = b.SaleReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and pp.StoreId= {storeId ?? 0} LEFT JOIN auth.Users u ON a.BusinessUserId=u.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0' and a.Deleted = 0) UNION ALL(select a.Id ReservationBillId, a.BillNumber ReservationBillNumber,{(int)BillTypeEnum.ReturnReservationBill} as BillTypeId,'退货' BillTypeName,a.TerminalId,t.Name TerminalName, t.Code TerminalCode,t.DistrictId, a.BusinessUserId,u.UserRealName  BusinessUserName,a.TransactionDate,a.AuditedDate,a.WareHouseId,w.Name WareHouseName, b.ProductId,p.Sku ProductSKU, p.Name ProductName, p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName, p.StrokeUnitId,pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) SaleReturnSmallQuantity ,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) SaleReturnStrokeQuantity ,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) SaleReturnBigQuantity ,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) BarCode,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,b.Quantity Quantity, b.UnitId,(case when b.UnitId = p.SmallUnitId then pa1.Name when b.UnitId = p.StrokeUnitId then pa2.Name when b.UnitId = p.BigUnitId then pa3.Name else '' end ) UnitName,(select count(*) from GiveQuotaRecords where ContractId > 0 and ProductId = p.Id) as CostContractProduct,(select count(*) from Stocks where UsableQuantity > 0 and ProductId = p.Id) as OccupyStock,b.Price,b.Amount,b.CostAmount,b.Profit,b.CostProfitRate,0.00 SystemPrice,b.Amount - (pp.RetailPrice * b.Quantity) ChangeDifference,0.00 PresetPrice,0.00 RecentPurchasesPrice,0.00 as RecentSettlementCostPrice, b.Remark,'' as UnitBigStrokeSmall from ReturnReservationBills a inner join ReturnReservationItems b on a.Id = b.ReturnReservationBillId left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join WareHouses w on a.WareHouseId = w.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id left join ProductPrices pp on b.ProductId = pp.ProductId and b.UnitId = pp.UnitId and pp.StoreId= {storeId ?? 0} LEFT JOIN auth.Users u ON a.BusinessUserId=u.Id where {whereQuery2} and a.AuditedStatus = '1' and a.ReversedStatus = '0' and a.Deleted = 0)) as red where {whereQuery3}";

                       reporting = SaleBillsRepository_RO.QueryFromSql<SaleReportOrderItem>(sqlString).Distinct().OrderByDescending(r=>r.TransactionDate).ToList();

                       return reporting;
                   }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportOrderItem>();
            }
        }

        /// <summary>
        /// 订单汇总（按商品）
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="deliveryUserId">送货员Id</param>
        /// <param name="rankId">客户等级Id</param>
        /// <param name="costContractProduct">费用合同兑现商品</param>
        /// <returns></returns>
        public IList<SaleReportSummaryOrderProduct> GetSaleReportSummaryOrderProduct(int? storeId, int? productId, string productName, int? categoryId, int? bussinessUserId, int? wareHouseId, int? districtId, int? terminalId, string terminalName, int? channelId, string remark, DateTime? startTime, DateTime? endTime, int? deliveryUserId, int? rankId, bool? costContractProduct, bool force = false)
        {

            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_ORDERPRODUCT_KEY.FillCacheKey(storeId, productId, productName, categoryId, bussinessUserId, wareHouseId, districtId, terminalId, terminalName, channelId, remark, startTime, endTime, deliveryUserId, rankId, costContractProduct), () =>
                    {
                        productName = CommonHelper.Filter(productName);
                        terminalName = CommonHelper.Filter(terminalName);
                        remark = CommonHelper.Filter(remark);

                        var reporting = new List<SaleReportSummaryOrderProduct>();

                        string whereQuery = $" a.StoreId= {storeId ?? 0}";
                        string whereQuery2 = $" a.StoreId= {storeId ?? 0}";

                        if (productId.HasValue && productId.Value != 0)
                        {
                            whereQuery += $" and b.ProductId = '{productId}' ";
                            whereQuery2 += $" and b.ProductId = '{productId}' ";
                        }
                        if (productName != null)
                        {
                            whereQuery += $" and p.Name like '%{productName}%' ";
                            whereQuery2 += $" and p.Name like '%{productName}%' ";
                        }

                        if (categoryId.HasValue && categoryId.Value != 0)
                        {
                            //递归商品类别查询
                            var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                            if (categoryIds != null && categoryIds.Count > 0)
                            {
                                string incategoryIds = string.Join("','", categoryIds);
                                whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                                whereQuery2 += $" and p.CategoryId in ('{incategoryIds}') ";
                            }
                            else
                            {
                                whereQuery += $" and p.CategoryId = '{categoryId}' ";
                                whereQuery2 += $" and p.CategoryId = '{categoryId}' ";
                            }
                        }

                        if (terminalId.HasValue && terminalId.Value != 0)
                        {
                            whereQuery += $" and a.TerminalId = '{terminalId}' ";
                            whereQuery2 += $" and a.TerminalId = '{terminalId}' ";
                        }

                        if (terminalName != null)
                        {
                            whereQuery += $" and t.Name like '%{terminalName}%' ";
                            whereQuery2 += $" and t.Name like '%{terminalName}%' ";
                        }

                        if (rankId.HasValue && rankId.Value != 0)
                        {
                            whereQuery += $" and t.RankId = '{rankId}' ";
                            whereQuery2 += $" and t.RankId = '{rankId}' ";
                        }

                        if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                        {
                            whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                            whereQuery2 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                        }

                        if (wareHouseId.HasValue && wareHouseId.Value != 0)
                        {
                            whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                            whereQuery2 += $" and a.WareHouseId = '{wareHouseId}' ";
                        }

                        if (channelId.HasValue && channelId.Value != 0)
                        {
                            whereQuery += $" and t.ChannelId = '{channelId}' ";
                            whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                        }

                        if (districtId.HasValue && districtId.Value != 0)
                        {
                            //递归片区查询
                            var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                            if (distinctIds != null && distinctIds.Count > 0)
                            {
                                string inDistinctIds = string.Join("','", distinctIds);
                                whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
                                whereQuery2 += $" and t.DistrictId in ('{inDistinctIds}') ";
                            }
                            else
                            {
                                whereQuery += $" and t.DistrictId = '{districtId}' ";
                                whereQuery2 += $" and t.DistrictId = '{districtId}' ";
                            }
                        }

                        if (!string.IsNullOrEmpty(remark))
                        {
                            whereQuery += $" and b.Remark like '%{remark}%' ";
                            whereQuery2 += $" and b.Remark like '%{remark}%' ";
                        }

                        if (channelId.HasValue && channelId.Value != 0)
                        {
                            whereQuery += $" and t.ChannelId = '{channelId}' ";
                            whereQuery2 += $" and t.ChannelId = '{channelId}' ";
                        }

                        if (deliveryUserId.HasValue && deliveryUserId.Value != 0)
                        {
                            whereQuery += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                            whereQuery2 += $" and a.DeliveryUserId = '{deliveryUserId}' ";
                        }

                        //费用合同兑换商品
                        if (costContractProduct.HasValue)
                        {
                            //费用合同兑换
                            if (costContractProduct.Value == true)
                            {
                                whereQuery += $" and b.CostContractId > 0 ";
                            }
                            else
                            {
                                whereQuery += $" and (b.CostContractId = 0 or b.CostContractId is null )";
                            }
                        }


                        if (startTime.HasValue)
                        {
                            //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                            string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                            whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                            whereQuery2 += $" and a.CreatedOnUtc >= '{startTimedata}'";
                        }

                        if (endTime.HasValue)
                        {
                            //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                            string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                            whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                            whereQuery2 += $" and a.CreatedOnUtc <= '{endTimedata}'";
                        }

                        //MSSQL
                        //string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 'FALSE' and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,''SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) GiftSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) GiftStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 'TRUE' and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnReservationBills a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";

                        //MYSQL
                        //string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 0 and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,''SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) GiftSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) GiftStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 1 and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnReservationBills a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";
                        string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 0 and a.AuditedStatus='1' and a.ReversedStatus='0' and a.Deleted = 0) UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,''SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) GiftSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) GiftStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts= 1 and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnReservationBills a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery2} and a.AuditedStatus='1' and a.ReversedStatus='0' and a.Deleted = 0)";

                        var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryOrderProduct>(sqlString).ToList();

                        if (items != null && items.Count > 0)
                        {
                            items.ToList().ForEach(a =>
                            {

                                var srsop = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                                if (srsop != null)
                                {
                                    srsop.SaleSmallQuantity = (srsop.SaleSmallQuantity ?? 0) + (a.SaleSmallQuantity ?? 0);
                                    srsop.SaleStrokeQuantity = (srsop.SaleStrokeQuantity ?? 0) + (a.SaleStrokeQuantity ?? 0);
                                    srsop.SaleBigQuantity = (srsop.SaleBigQuantity ?? 0) + (a.SaleBigQuantity ?? 0);
                                    srsop.SaleQuantityConversion = (srsop.SaleBigQuantity ?? 0) + "大" + (srsop.SaleStrokeQuantity ?? 0) + "中" + (srsop.SaleSmallQuantity ?? 0) + "小";
                                    srsop.SaleAmount = (srsop.SaleAmount ?? 0) + (a.SaleAmount ?? 0);

                                    srsop.GiftSmallQuantity = (srsop.GiftSmallQuantity ?? 0) + (a.GiftSmallQuantity ?? 0);
                                    srsop.GiftStrokeQuantity = (srsop.GiftStrokeQuantity ?? 0) + (a.GiftStrokeQuantity ?? 0);
                                    srsop.GiftBigQuantity = (srsop.GiftBigQuantity ?? 0) + (a.GiftBigQuantity ?? 0);
                                    srsop.GiftQuantityConversion = (srsop.GiftBigQuantity ?? 0) + "大" + (srsop.GiftStrokeQuantity ?? 0) + "中" + (srsop.GiftSmallQuantity ?? 0) + "小";

                                    srsop.ReturnSmallQuantity = (srsop.ReturnSmallQuantity ?? 0) + (a.ReturnSmallQuantity ?? 0);
                                    srsop.ReturnStrokeQuantity = (srsop.ReturnStrokeQuantity ?? 0) + (a.ReturnStrokeQuantity ?? 0);
                                    srsop.ReturnBigQuantity = (srsop.ReturnBigQuantity ?? 0) + (a.ReturnBigQuantity ?? 0);
                                    srsop.ReturnQuantityConversion = (srsop.ReturnBigQuantity ?? 0) + "大" + (srsop.ReturnStrokeQuantity ?? 0) + "中" + (srsop.ReturnSmallQuantity ?? 0) + "小";
                                    srsop.ReturnAmount = (srsop.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);

                                    srsop.NetSmallQuantity = (srsop.NetSmallQuantity ?? 0) + ((a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0));
                                    srsop.NetStrokeQuantity = (srsop.NetStrokeQuantity ?? 0) + ((a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0));
                                    srsop.NetBigQuantity = (srsop.NetBigQuantity ?? 0) + ((a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0));
                                    srsop.NetQuantityConversion = (srsop.NetBigQuantity ?? 0) + "大" + (srsop.NetStrokeQuantity ?? 0) + "中" + (srsop.NetSmallQuantity ?? 0) + "小";
                                    srsop.NetAmount = (srsop.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));

                                    srsop.CostAmount = (srsop.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                    srsop.Profit = srsop.NetAmount - srsop.CostAmount;
                                    if (srsop.CostAmount == null || srsop.CostAmount == 0)
                                    {
                                        srsop.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        srsop.CostProfitRate = ((srsop.Profit ?? 0) / srsop.CostAmount) * 100;
                                    }

                                }
                                else
                                {
                                    srsop = new SaleReportSummaryOrderProduct
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

                                        SaleSmallQuantity = a.SaleSmallQuantity,
                                        SaleStrokeQuantity = a.SaleStrokeQuantity,
                                        SaleBigQuantity = a.SaleBigQuantity
                                    };
                                    srsop.SaleQuantityConversion = (srsop.SaleBigQuantity ?? 0) + "大" + (srsop.SaleStrokeQuantity ?? 0) + "中" + (srsop.SaleSmallQuantity ?? 0) + "小";
                                    srsop.SaleAmount = a.SaleAmount;

                                    srsop.GiftSmallQuantity = a.GiftSmallQuantity;
                                    srsop.GiftStrokeQuantity = a.GiftStrokeQuantity;
                                    srsop.GiftBigQuantity = a.GiftBigQuantity;
                                    srsop.GiftQuantityConversion = (srsop.GiftBigQuantity ?? 0) + "大" + (srsop.GiftStrokeQuantity ?? 0) + "中" + (srsop.GiftSmallQuantity ?? 0) + "小";

                                    srsop.ReturnSmallQuantity = a.ReturnSmallQuantity;
                                    srsop.ReturnStrokeQuantity = a.ReturnStrokeQuantity;
                                    srsop.ReturnBigQuantity = a.ReturnBigQuantity;
                                    srsop.ReturnQuantityConversion = (srsop.ReturnBigQuantity ?? 0) + "大" + (srsop.ReturnStrokeQuantity ?? 0) + "中" + (srsop.ReturnSmallQuantity ?? 0) + "小";
                                    srsop.ReturnAmount = a.ReturnAmount;

                                    srsop.NetSmallQuantity = (a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0);
                                    srsop.NetStrokeQuantity = (a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0);
                                    srsop.NetBigQuantity = (a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0);
                                    srsop.NetQuantityConversion = (srsop.NetBigQuantity ?? 0) + "大" + (srsop.NetStrokeQuantity ?? 0) + "中" + (srsop.NetSmallQuantity ?? 0) + "小";
                                    srsop.NetAmount = (a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0);

                                    srsop.CostAmount = a.CostAmount;
                                    srsop.Profit = srsop.NetAmount - srsop.CostAmount;
                                    if (srsop.CostAmount == null || srsop.CostAmount == 0)
                                    {
                                        srsop.CostProfitRate = 100;
                                    }
                                    else
                                    {
                                        srsop.CostProfitRate = ((srsop.Profit) / srsop.CostAmount) * 100;
                                    }

                                    reporting.Add(srsop);
                                }
                            });
                        }

                        return reporting;
                    }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportSummaryOrderProduct>();
            }
        }


        /// <summary>
        /// 费用合同明细表
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="accountingOptionId">费用类别Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="cashTypeId">兑现方式Id</param>
        /// <param name="remark">备注</param>
        /// <param name="statusTypeId">状态Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <returns></returns>
        public IList<SaleReportCostContractItem> GetSaleReportCostContractItem(int? storeId, int? terminalId, string terminalName, int? productId, string productName, int? bussinessUserId, int? accountingOptionId,
               string billNumber, int? categoryId, int? cashTypeId, string remark, int? statusTypeId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_COSTCONTRACTITEM_KEY.FillCacheKey(storeId, terminalId, terminalName, productId, productName, bussinessUserId, accountingOptionId,
                        billNumber, categoryId, cashTypeId, remark, statusTypeId, startTime, endTime), () =>
                        {
                            terminalName = CommonHelper.Filter(terminalName);
                            productName = CommonHelper.Filter(productName);
                            billNumber = CommonHelper.Filter(billNumber);
                            remark = CommonHelper.Filter(remark);

                            var reporting = new List<SaleReportCostContractItem>();

                            string whereQuery = $" a.StoreId= {storeId ?? 0}";

                            if (terminalId.HasValue && terminalId.Value != 0)
                            {
                                whereQuery += $" and a.TerminalId = '{terminalId}' ";
                            }
                            if (terminalName != null)
                            {
                                whereQuery += $" and t.Name like '%{terminalName}%' ";
                            }
                            if (productId.HasValue && productId.Value != 0)
                            {
                                whereQuery += $" and b.ProductId = '{productId}' ";
                            }
                            if (productName != null)
                            {
                                whereQuery += $" and p.Name like '%{productName}%' ";
                            }
                            if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                            {
                                whereQuery += $" and a.EmployeeId = '{bussinessUserId}' ";
                            }

                            if (accountingOptionId.HasValue && accountingOptionId.Value != 0)
                            {
                                whereQuery += $" and ao.Id = '{accountingOptionId}' ";
                            }

                            if (!string.IsNullOrEmpty(billNumber))
                            {
                                whereQuery += $" and a.BillNumber like '%{billNumber}%' ";
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

                            if (cashTypeId.HasValue && cashTypeId.Value != 0)
                            {
                                whereQuery += $" and b.CType = '{cashTypeId}' ";
                            }

                            if (!string.IsNullOrEmpty(remark))
                            {
                                whereQuery += $" and b.Remark like '%{remark}%' ";
                            }

                            if (statusTypeId.HasValue && statusTypeId.Value != 0)
                            {
                                whereQuery += $" and a.AbandonedStatus == '{statusTypeId}'";
                            }

                            if (startTime.HasValue)
                            {
                        //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                                whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                            }

                            if (endTime.HasValue)
                            {
                        //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                                whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                            }

                            //MSSQL
                            //string sqlString = $"select  a.Id BillId,a.BillNumber,a.CustomerId TerminalId,t.Name TerminalName,t.Code TerminalCode,a.AccountingOptionId,isnull(ao.Name,'异常支付') AccountingOptionName,a.EmployeeId BusinessUserId,'' BusinessUserName,a.CreatedOnUtc TransactionDate,a.AuditedDate,b.ProductId,isnull(p.Sku,'') ProductSKU, b.CType,(case when b.CType=0 then isnull(p.Name,'') when b.CType=1 then isnull(b.Name,'') else '' end ) ProductName,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) BarCode,'' UnitConversion,(case when b.CType = 0 then b.UnitId when b.CType = 1 then 0 end ) UnitId,(case when b.CType = 0 then isnull(sao.Name,'无') when b.CType = 1 then '元' end ) UnitName,(case when b.UnitId = p.SmallUnitId then '小' when b.UnitId = p.StrokeUnitId then '中' when b.UnitId = p.BigUnitId then '大' else '' end ) UnitBigStrokeSmall,isnull(b.Jan, 0) as Jan,isnull(b.Feb, 0) as Feb,isnull(b.Mar, 0) as Mar,isnull(b.Apr, 0) as Apr,isnull(b.May, 0) as May,isnull(b.Jun, 0) as Jun,isnull(b.Jul, 0) as Jul,isnull(b.Aug, 0) as Aug,isnull(b.Sep, 0) as Sep,isnull(b.Oct, 0) as Oct,isnull(b.Nov, 0) as Nov,isnull(b.Dec, 0) as Dec,b.Total,(case when a.AbandonedStatus = 1 then '已终止' else '正常' end ) Status,isnull(b.Remark, '') from CostContractBills a inner join CostContractItems b on a.Id = b.CostContractBillId left join dcms_crm.CRM_Terminals t on a.CustomerId = t.Id left join AccountingOptions ao on a.AccountingOptionId = ao.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions sao on b.UnitId = sao.Id where {whereQuery}";

                            //MYSQL
                            string sqlString = $"select  a.Id BillId,a.BillNumber,a.CustomerId TerminalId,t.Name TerminalName,t.Code TerminalCode,a.AccountingOptionId,IFNULL(ao.Name,'异常支付') AccountingOptionName,a.EmployeeId BusinessUserId,'' BusinessUserName,a.CreatedOnUtc TransactionDate,a.AuditedDate,b.ProductId,IFNULL(p.Sku,'') as ProductSKU, a.ContractType, b.CType,(case when b.CType=0 then IFNULL(p.Name,'') when b.CType=1 then IFNULL(b.Name,'') else '' end ) as ProductName,(case when b.UnitId = p.SmallUnitId then p.SmallBarCode when b.UnitId = p.StrokeUnitId then p.StrokeBarCode when b.UnitId = p.BigUnitId then p.BigBarCode else '' end ) as BarCode,{CommonHelper.GetSqlUnitConversion("p")} as UnitConversion,(case when b.CType = 0 then b.UnitId when b.CType = 1 then 0 end ) UnitId,(case when b.CType = 0 then IFNULL(sao.Name,'无') when b.CType = 1 then '元' end ) as UnitName,(case when b.UnitId = p.SmallUnitId then '小' when b.UnitId = p.StrokeUnitId then '中' when b.UnitId = p.BigUnitId then '大' else '' end ) as  UnitBigStrokeSmall,IFNULL(b.Jan, 0) as Jan,IFNULL(b.Feb, 0) as Feb,IFNULL(b.Mar, 0) as Mar,IFNULL(b.Apr, 0) as Apr,IFNULL(b.May, 0) as May,IFNULL(b.Jun, 0) as Jun,IFNULL(b.Jul, 0) as Jul,IFNULL(b.Aug, 0) as Aug,IFNULL(b.Sep, 0) as Sep,IFNULL(b.Oct, 0) as Oct,IFNULL(b.Nov, 0) as Nov,IFNULL(b.Dec, 0) as 'Dec',b.Total,(case when a.AbandonedStatus = 1 then '已终止' else '正常' end ) Status,IFNULL(b.Remark, '') as Remark from CostContractBills a inner join CostContractItems b on a.Id = b.CostContractBillId left join dcms_crm.CRM_Terminals t on a.CustomerId = t.Id left join AccountingOptions ao on a.AccountingOptionId = ao.Id left join Products p on b.ProductId = p.Id left join SpecificationAttributeOptions sao on b.UnitId = sao.Id where {whereQuery}";


                            reporting = SaleBillsRepository_RO.QueryFromSql<SaleReportCostContractItem>(sqlString).ToList().Select(s =>
                            {
                                s.BusinessUserName = _userService.GetUserName(storeId, s.BusinessUserId ?? 0);
                                return s;
                            }).ToList();

                            return reporting;
                        }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportCostContractItem>();
            }
        }

        /// <summary>
        /// 赠品汇总
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <returns></returns>
        public IList<GiveQuotaRecordsSummery> GetSaleReportSummaryGiveQuota(int? storeId, int? productId, int? terminalId, int? categoryId, string remark, DateTime? startTime, DateTime? endTime, int? businessUserId, bool force = false)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_GIVEQUOTA_KEY.FillCacheKey(storeId, productId, terminalId, categoryId, remark, startTime, endTime, businessUserId), () =>
                  {
                      if (startTime.HasValue)
                      {
                          startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                      }

                      if (endTime.HasValue)
                      {
                          endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                      }

                      var reporting = new List<GiveQuotaRecordsSummery>();

                      reporting = _giveQuotaService.GetAllGiveQuotaRecordsSummeries(storeId, businessUserId, productId, terminalId, categoryId, null, null, startTime, endTime).ToList();
                      return reporting;
                  }, force);
            }
            catch (Exception)
            {
                return new List<GiveQuotaRecordsSummery>();
            }
        }

        /// <summary>
        /// 热销排行榜
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="businessUserId">客户Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="topNumber">统计前</param>
        /// <returns></returns>
        public IList<SaleReportHotSale> GetSaleReportHotSale(int? storeId, int? productId, string productName, int? businessUserId, int? wareHouseId, int? terminalId, string terminalName, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber, bool force = false)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_HOTSALE_KEY.FillCacheKey(storeId, productId, productName, businessUserId, wareHouseId, terminalId, terminalName, brandId, categoryId, startTime, endTime, topNumber);
                key.CacheTime = 1;

                return _cacheManager.Get(key, () =>
                {
                    productName = CommonHelper.Filter(productName);
                    terminalName = CommonHelper.Filter(terminalName);

                    var reporting = new List<SaleReportHotSale>();

                    string whereQuery = $" 1=1 ";

                    if (storeId.HasValue && storeId.Value != 0)
                    {
                        whereQuery += $" and a.StoreId = '{storeId ?? 0}' ";
                    }

                    if (terminalId.HasValue && terminalId.Value != 0)
                    {
                        whereQuery += $" and a.TerminalId = '{terminalId}' ";
                    }
                    if (terminalName != null)
                    {
                        whereQuery += $" and t.Name like '%{terminalName}%' ";
                    }

                    if (businessUserId.HasValue && businessUserId.Value > 0)
                    {
                        //var userIds = _userService.GetSubordinate(storeId, businessUserId ?? 0);
                        whereQuery += $" and a.BusinessUserId in ({string.Join(",", businessUserId)}) ";
                    }

                    if (wareHouseId.HasValue && wareHouseId.Value != 0)
                    {
                        whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                    }

                    if (productId.HasValue && productId.Value != 0)
                    {
                        whereQuery += $" and b.ProductId = '{productId}' ";
                    }
                    if (productName != null)
                    {
                        whereQuery += $" and p.Name like '%{productName}%' ";
                    }
                    if (brandId.HasValue && brandId.Value != 0)
                    {
                        whereQuery += $" and p.BrandId = '{brandId}' ";
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

                    if (startTime.HasValue)
                    {
                        string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                        whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                    }

                    if (endTime.HasValue)
                    {
                        string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                        whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                    }

                    //MYSQL
                    string sqlString = $"(select  b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleBills a inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join ReturnItems b on a.Id = b.ReturnBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";

                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportHotSale>(sqlString).ToList();

                    if (items != null && items.Count > 0)
                    {
                        items.ToList().ForEach(a =>
                        {

                           //商品 查询
                            var srhs = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                            if (srhs != null)
                            {
                                srhs.SaleSmallQuantity = (srhs.SaleSmallQuantity ?? 0) + (a.SaleSmallQuantity ?? 0);
                                srhs.SaleStrokeQuantity = (srhs.SaleStrokeQuantity ?? 0) + (a.SaleStrokeQuantity ?? 0);
                                srhs.SaleBigQuantity = (srhs.SaleBigQuantity ?? 0) + (a.SaleBigQuantity ?? 0);
                                srhs.SaleAmount = (srhs.SaleAmount ?? 0) + (a.SaleAmount ?? 0);

                                srhs.ReturnSmallQuantity = (srhs.ReturnSmallQuantity ?? 0) + (a.ReturnSmallQuantity ?? 0);
                                srhs.ReturnStrokeQuantity = (srhs.ReturnStrokeQuantity ?? 0) + (a.ReturnStrokeQuantity ?? 0);
                                srhs.ReturnBigQuantity = (srhs.ReturnBigQuantity ?? 0) + (a.ReturnBigQuantity ?? 0);
                                srhs.ReturnAmount = (srhs.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);

                                srhs.NetSmallQuantity = (srhs.NetSmallQuantity ?? 0) + ((a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0));
                                srhs.NetStrokeQuantity = (srhs.NetStrokeQuantity ?? 0) + ((a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0));
                                srhs.NetBigQuantity = (srhs.NetBigQuantity ?? 0) + ((a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0));

                                srhs.NetAmount = (srhs.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                srhs.CostAmount = (srhs.CostAmount ?? 0) + a.CostAmount;
                                srhs.Profit = (srhs.NetAmount ?? 0) - (srhs.CostAmount ?? 0);
                                if (srhs.CostAmount == null || srhs.CostAmount == 0)
                                {
                                    srhs.CostProfitRate = 100;
                                }
                                else
                                {
                                    srhs.CostProfitRate = (srhs.Profit / srhs.CostAmount) * 100;
                                }

                            }
                            else
                            {
                                srhs = new SaleReportHotSale
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
                                srhs.SaleSmallQuantity = a.SaleSmallQuantity;
                                srhs.SaleStrokeQuantity = a.SaleStrokeQuantity;
                                srhs.SaleBigQuantity = a.SaleBigQuantity;
                                srhs.SaleAmount = a.SaleAmount;

                                srhs.ReturnSmallQuantity = a.ReturnSmallQuantity;
                                srhs.ReturnStrokeQuantity = a.ReturnStrokeQuantity;
                                srhs.ReturnBigQuantity = a.ReturnBigQuantity;
                                srhs.ReturnAmount = a.ReturnAmount;

                                srhs.NetSmallQuantity = (a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0);
                                srhs.NetStrokeQuantity = (a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0);
                                srhs.NetBigQuantity = (a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0);
                                srhs.NetAmount = (a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0);

                                srhs.CostAmount = a.CostAmount;
                                srhs.Profit = srhs.NetAmount - srhs.CostAmount;
                                if (srhs.CostAmount == null || srhs.CostAmount == 0)
                                {
                                    srhs.CostProfitRate = 100;
                                }
                                else
                                {
                                    srhs.CostProfitRate = ((srhs.Profit ?? 0) / srhs.CostAmount) * 100;
                                }

                                reporting.Add(srhs);
                            }
                        });
                    }

                    //排序
                    reporting = reporting.OrderByDescending(a => a.NetAmount).ToList();

                    //取前多少行
                    int iTopNumber = 10;
                    if (topNumber != null && topNumber > 0)
                    {
                        iTopNumber = (int)topNumber;
                    }

                    reporting = reporting.OrderByDescending(a => a.NetAmount).Take(iTopNumber).ToList();

                    //将单位整理
                    if (reporting != null && reporting.Count > 0)
                    {
                        foreach (SaleReportHotSale item in reporting)
                        {
                            Product product = new Product()
                            {
                                BigUnitId = item.BigUnitId,
                                StrokeUnitId = item.StrokeUnitId,
                                SmallUnitId = item.SmallUnitId ?? 0,
                                BigQuantity = item.BigQuantity,
                                StrokeQuantity = item.StrokeQuantity
                            };
                            Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                            //销售
                            int sumSaleQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumSaleQuantity += (item.SaleBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumSaleQuantity += (item.SaleStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumSaleQuantity += (item.SaleSmallQuantity ?? 0);

                            var salequantity = Pexts.StockQuantityFormat(sumSaleQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.SaleBigQuantity = salequantity.Item1;
                            item.SaleStrokeQuantity = salequantity.Item2;
                            item.SaleSmallQuantity = salequantity.Item3;
                            item.SaleQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSaleQuantity);

                            //退货
                            int sumReturnQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumReturnQuantity += (item.ReturnBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumReturnQuantity += (item.ReturnStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumReturnQuantity += (item.ReturnSmallQuantity ?? 0);

                            var returnquantity = Pexts.StockQuantityFormat(sumReturnQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.ReturnBigQuantity = returnquantity.Item1;
                            item.ReturnStrokeQuantity = returnquantity.Item2;
                            item.ReturnSmallQuantity = returnquantity.Item3;
                            item.ReturnQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumReturnQuantity);

                            //净销
                            int sumNetQuantity = 0;
                            if (item.BigQuantity > 0)
                            {
                                sumNetQuantity += (item.NetBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                            }
                            if (item.StrokeQuantity > 0)
                            {
                                sumNetQuantity += (item.NetStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                            }
                            sumNetQuantity += (item.NetSmallQuantity ?? 0);

                            var netquantity = Pexts.StockQuantityFormat(sumNetQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                            item.NetBigQuantity = netquantity.Item1;
                            item.NetStrokeQuantity = netquantity.Item2;
                            item.NetSmallQuantity = netquantity.Item3;
                            item.NetQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumNetQuantity);

                        }
                    }

                    return reporting;
                }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportHotSale>();
            }
        }

        /// <summary>
        /// 热定排行榜
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="businessUserId">客户Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="topNumber">统计前</param>
        /// <returns></returns>
        public IList<SaleReportHotSale> GetOrderReportHotSale(int? storeId, int? productId, int? businessUserId, int? wareHouseId, int? terminalId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber, bool force = false)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_ORDER_HOTSALE_KEY.FillCacheKey(storeId, productId, businessUserId, wareHouseId, terminalId, brandId,
                       categoryId, startTime, endTime, topNumber);
                key.CacheTime = 1;
                return _cacheManager.Get(key, () =>
                       {

                           var reporting = new List<SaleReportHotSale>();

                           string whereQuery = $" 1=1 ";

                           if (storeId.HasValue && storeId.Value != 0)
                           {
                               whereQuery += $" and a.StoreId = '{storeId ?? 0}' ";
                           }

                           if (terminalId.HasValue && terminalId.Value != 0)
                           {
                               whereQuery += $" and a.TerminalId = '{terminalId}' ";
                           }

                           if (businessUserId.HasValue && businessUserId.Value != 0)
                           {
                               var userIds = _userService.GetSubordinate(storeId, businessUserId??0);
                               whereQuery += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                           }

                           if (productId.HasValue && productId.Value != 0)
                           {
                               whereQuery += $" and b.ProductId = '{productId}' ";
                           }

                           if (brandId.HasValue && brandId.Value != 0)
                           {
                               whereQuery += $" and p.BrandId = '{brandId}' ";
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

                           if (startTime.HasValue)
                           {
                       //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                       string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                               whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                           }

                           if (endTime.HasValue)
                           {
                       //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                       string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                               whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                           }

                           //MSSQL
                           //string sqlString = $"(select  b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnReservationBills a inner join ReturnReservationItems b on a.Id = b.ReturnReservationBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";

                           //MYSQL
                           string sqlString = $"(select  b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion from SaleReservationBills a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,a.BusinessUserId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId , pa2.Name StrokeUnitName, p.BigUnitId,pa3.Name BigUnitName, p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId = p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId = p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId = p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion from ReturnReservationBills a inner join ReturnReservationItems b on a.Id = b.ReturnReservationBillId inner join Products p on b.ProductId = p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId = pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId = pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId = pa3.Id where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')";


                           var items = SaleBillsRepository_RO.QueryFromSql<SaleReportHotSale>(sqlString).ToList();

                           if (items != null && items.Count > 0)
                           {
                               items.ToList().ForEach(a =>
                               {

                           //商品 查询
                           var srhs = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                                   if (srhs != null)
                                   {
                                       srhs.SaleSmallQuantity = (srhs.SaleSmallQuantity ?? 0) + (a.SaleSmallQuantity ?? 0);
                                       srhs.SaleStrokeQuantity = (srhs.SaleStrokeQuantity ?? 0) + (a.SaleStrokeQuantity ?? 0);
                                       srhs.SaleBigQuantity = (srhs.SaleBigQuantity ?? 0) + (a.SaleBigQuantity ?? 0);
                                       srhs.SaleQuantityConversion = (srhs.SaleBigQuantity ?? 0) + "大" + (srhs.SaleStrokeQuantity ?? 0) + "中" + (srhs.SaleSmallQuantity ?? 0) + "小";
                                       srhs.SaleAmount = (srhs.SaleAmount ?? 0) + (a.SaleAmount ?? 0);

                                       srhs.ReturnSmallQuantity = (srhs.ReturnSmallQuantity ?? 0) + (a.ReturnSmallQuantity ?? 0);
                                       srhs.ReturnStrokeQuantity = (srhs.ReturnStrokeQuantity ?? 0) + (a.ReturnStrokeQuantity ?? 0);
                                       srhs.ReturnBigQuantity = (srhs.ReturnBigQuantity ?? 0) + (a.ReturnBigQuantity ?? 0);
                                       srhs.ReturnQuantityConversion = (srhs.ReturnBigQuantity ?? 0) + "大" + (srhs.ReturnStrokeQuantity ?? 0) + "中" + (srhs.ReturnSmallQuantity ?? 0) + "小";
                                       srhs.ReturnAmount = (srhs.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);

                                       srhs.NetSmallQuantity = (srhs.NetSmallQuantity ?? 0) + ((a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0));
                                       srhs.NetStrokeQuantity = (srhs.NetStrokeQuantity ?? 0) + ((a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0));
                                       srhs.NetBigQuantity = (srhs.NetBigQuantity ?? 0) + ((a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0));
                                       srhs.NetQuantityConversion = (srhs.NetBigQuantity ?? 0) + "大" + (srhs.NetStrokeQuantity ?? 0) + "中" + (srhs.NetSmallQuantity ?? 0) + "小";

                                       srhs.NetAmount = (srhs.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));
                                       srhs.CostAmount = (srhs.CostAmount ?? 0) + a.CostAmount;
                                       srhs.Profit = (srhs.NetAmount ?? 0) - (srhs.CostAmount ?? 0);
                                       if (srhs.CostAmount == null || srhs.CostAmount == 0)
                                       {
                                           srhs.CostProfitRate = 100;
                                       }
                                       else
                                       {
                                           srhs.CostProfitRate = (srhs.Profit / srhs.CostAmount) * 100;
                                       }

                                   }
                                   else
                                   {
                                       srhs = new SaleReportHotSale
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
                                           SaleSmallQuantity = a.SaleSmallQuantity,
                                           SaleStrokeQuantity = a.SaleStrokeQuantity,
                                           SaleBigQuantity = a.SaleBigQuantity
                                       };
                                       srhs.SaleQuantityConversion = (srhs.SaleBigQuantity ?? 0) + "大" + (srhs.SaleStrokeQuantity ?? 0) + "中" + (srhs.SaleSmallQuantity ?? 0) + "小";
                                       srhs.SaleAmount = a.SaleAmount;

                                       srhs.ReturnSmallQuantity = a.ReturnSmallQuantity;
                                       srhs.ReturnStrokeQuantity = a.ReturnStrokeQuantity;
                                       srhs.ReturnBigQuantity = a.ReturnBigQuantity;
                                       srhs.ReturnQuantityConversion = (srhs.ReturnBigQuantity ?? 0) + "大" + (srhs.ReturnStrokeQuantity ?? 0) + "中" + (srhs.ReturnSmallQuantity ?? 0) + "小";
                                       srhs.ReturnAmount = a.ReturnAmount;

                                       srhs.NetSmallQuantity = (a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0);
                                       srhs.NetStrokeQuantity = (a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0);
                                       srhs.NetBigQuantity = (a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0);
                                       srhs.NetQuantityConversion = (srhs.NetBigQuantity ?? 0) + "大" + (srhs.NetStrokeQuantity ?? 0) + "中" + (srhs.NetSmallQuantity ?? 0) + "小";
                                       srhs.NetAmount = (a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0);

                                       srhs.CostAmount = a.CostAmount;
                                       srhs.Profit = srhs.NetAmount - srhs.CostAmount;
                                       if (srhs.CostAmount == null || srhs.CostAmount == 0)
                                       {
                                           srhs.CostProfitRate = 100;
                                       }
                                       else
                                       {
                                           srhs.CostProfitRate = ((srhs.Profit ?? 0) / srhs.CostAmount) * 100;
                                       }

                                       reporting.Add(srhs);
                                   }
                               });
                           }

                           int iTopNumber = 10;
                           if (topNumber != null || topNumber > 0)
                           {
                               iTopNumber = (int)topNumber;
                           }

                           reporting = reporting.OrderByDescending(a => a.NetAmount).Take(iTopNumber).ToList();

                           return reporting;
                       }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportHotSale>();
            }
        }

        /// <summary>
        /// 销量走势图
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="groupyById">统计方式</param>
        /// <returns></returns>
        public IList<SaleReportSaleQuantityTrend> GetSaleReportSaleQuantityTrend(int? storeId, DateTime? startTime, DateTime? endTime, int? groupByTypeId, bool force = false)
        {

            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_SALEQUANTITYTREND_KEY.FillCacheKey(storeId, startTime, endTime, groupByTypeId);

                return _cacheManager.Get(key, () =>
                    {

                        var reporting = new List<SaleReportSaleQuantityTrend>();

                        string whereQuery = $" a.StoreId= {storeId ?? 0}";


                        if (startTime.HasValue)
                        {
                            //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                            string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                            whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                        }

                        if (endTime.HasValue)
                        {
                            //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                            string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                            whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                        }

                        string typeQuery = $" CONVERT(VARCHAR(10), a.CreatedOnUtc, 120) ";

                        if (groupByTypeId == (int)SaleReportSaleQuantityTrendGroupByTypeEnum.Day)
                        {
                            //MSSQL
                            //typeQuery = $" CONVERT(VARCHAR(10), a.CreatedOnUtc, 120) ";
                            //MYSQL
                            typeQuery = $" a.CreatedOnUtc ";
                        }
                        else if (groupByTypeId == (int)SaleReportSaleQuantityTrendGroupByTypeEnum.Week)
                        {
                            //MSSQL
                            //typeQuery = $" datepart(weekday,  a.CreatedOnUtc) ";
                            //MYSQL
                            typeQuery = $" DATE_FORMAT(a.CreatedOnUtc,'%w') ";
                        }
                        else if (groupByTypeId == (int)SaleReportSaleQuantityTrendGroupByTypeEnum.Month)
                        {
                            //MSSQL
                            //typeQuery = $" concat(Right(Year(a.CreatedOnUtc),4),'-',Right(100+Month(a.CreatedOnUtc),2)) ";
                            //MYSQL
                            typeQuery = $" concat(Right(Year(a.CreatedOnUtc),4),'-',Right(100+Month(a.CreatedOnUtc),2)) ";
                        }

                        //MSSQL/MYSQL
                        string sqlString = $"select SUM(alls.SaleAmount) as SaleAmount,SUM(alls.ReturnAmount) as ReturnAmount,alls.GroupDate,0 as NetAmount,'' as ShowDate from ( (select a.ReceivableAmount SaleAmount,0.00 ReturnAmount,{typeQuery} as GroupDate from SaleBills a where {whereQuery} and a.AuditedStatus=1 and a.ReversedStatus=0 ) UNION ALL (select  0.00 SaleAmount,a.ReceivableAmount ReturnAmount,{typeQuery} as GroupDate from ReturnBills a where {whereQuery} and a.AuditedStatus=1 and a.ReversedStatus=0) ) as  alls group by alls.GroupDate order by alls.GroupDate asc";

                        reporting = SaleBillsRepository_RO.QueryFromSql<SaleReportSaleQuantityTrend>(sqlString).ToList().Select(s =>
                        {
                            s.NetAmount = s.SaleAmount - s.ReturnAmount;
                            s.ShowDate = s.GroupDate;
                            return s;
                        }).ToList();

                        return reporting;
                    }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportSaleQuantityTrend>();
            }
        }

        /// <summary>
        /// 销售商品成本利润
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="wareHouseId">仓库Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <returns></returns>
        public IList<SaleReportProductCostProfit> GetSaleReportProductCostProfit(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? terminalId, string terminalName, int? bussinessUserId, int? wareHouseId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_PRODUCTCOSTPROFIT_KEY.FillCacheKey(storeId, productId, productName, categoryId, brandId, channelId,
                   terminalId, terminalName, bussinessUserId, wareHouseId, startTime, endTime), () =>
                  {
                      productName = CommonHelper.Filter(productName);
                      terminalName = CommonHelper.Filter(terminalName);

                      var reporting = new List<SaleReportProductCostProfit>();

                      string whereQuery = $" a.StoreId= {storeId ?? 0}";

                      if (terminalId.HasValue && terminalId.Value != 0)
                      {
                          whereQuery += $" and a.TerminalId = '{terminalId}' ";
                      }
                      if (terminalName != null)
                      {
                          whereQuery += $" and t.Name like '%{terminalName}%' ";
                      }
                      if (productId.HasValue && productId.Value != 0)
                      {
                          whereQuery += $" and b.ProductId = '{productId}' ";
                      }
                      if (productName != null)
                      {
                          whereQuery += $" and p.Name like '%{productName}%' ";
                      }
                      if (brandId.HasValue && brandId.Value != 0)
                      {
                          whereQuery += $" and p.BrandId = '{brandId}' ";
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

                      if (channelId.HasValue && channelId.Value != 0)
                      {
                          whereQuery += $" and t.ChannelId = '{channelId}' ";
                      }

                      if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                      {
                          whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                      }

                      if (wareHouseId.HasValue && wareHouseId.Value != 0)
                      {
                          whereQuery += $" and a.WareHouseId = '{wareHouseId}' ";
                      }

                      if (startTime.HasValue)
                      {
                  //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                  string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                          whereQuery += $" and a.CreatedOnUtc >= '{startTimedata}'";
                      }

                      if (endTime.HasValue)
                      {
                  //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                  string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                          whereQuery += $" and a.CreatedOnUtc <= '{endTimedata}'";
                      }

                      //MSSQL
                      //string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleBills a inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts ='0' and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join Items b on a.Id=b.ReturnBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";

                      //MYSQL
                      //string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleBills a inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts ='0' and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0 GiftSmallQuantity ,0 GiftStrokeQuantity ,0 GiftBigQuantity ,'' GiftQuantityConversion ,0.00 SaleAmount,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join Items b on a.Id=b.ReturnBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";

                      string sqlString = $"(select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) SaleSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) SaleStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) SaleBigQuantity,'' SaleQuantityConversion,b.Amount SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,0 ReturnSmallQuantity,0 ReturnStrokeQuantity,0 ReturnBigQuantity,'' ReturnQuantityConversion,0.00 ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from SaleBills a inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and b.IsGifts ='0' and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL (select b.ProductId,p.ProductCode,p.Name ProductName,p.SmallBarCode,p.StrokeBarCode,p.BigBarCode,p.SmallUnitId,pa1.Name SmallUnitName,p.StrokeUnitId,pa2.Name StrokeUnitName,p.BigUnitId,pa3.Name BigUnitName,p.BigQuantity,p.StrokeQuantity,{CommonHelper.GetSqlUnitConversion("p")} UnitConversion,0 SaleSmallQuantity,0 SaleStrokeQuantity,0 SaleBigQuantity,'' SaleQuantityConversion,0.00 SaleAmount,0 GiftSmallQuantity,0 GiftStrokeQuantity,0 GiftBigQuantity,'' GiftQuantityConversion,(case when b.UnitId=p.SmallUnitId then b.Quantity else 0 end) ReturnSmallQuantity,(case when b.UnitId=p.StrokeUnitId then b.Quantity else 0 end) ReturnStrokeQuantity,(case when b.UnitId=p.BigUnitId then b.Quantity else 0 end) ReturnBigQuantity,'' ReturnQuantityConversion,b.Amount ReturnAmount,0 NetSmallQuantity,0 NetStrokeQuantity,0 NetBigQuantity,'' NetQuantityConversion,0.00 NetAmount,b.CostAmount,b.Profit,b.CostProfitRate from ReturnBills a inner join ReturnItems b on a.Id=b.ReturnBillId inner join Products p on b.ProductId=p.Id left join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id left join SpecificationAttributeOptions pa1 on p.SmallUnitId=pa1.Id left join SpecificationAttributeOptions pa2 on p.StrokeUnitId=pa2.Id left join SpecificationAttributeOptions pa3 on p.BigUnitId=pa3.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0')";

                      var items = SaleBillsRepository_RO.QueryFromSql<SaleReportSummaryProduct>(sqlString).ToList();
                      if (items != null && items.Count > 0)
                      {
                          items.ToList().ForEach(a =>
                          {
                      //商品 查询
                      var srpcp = reporting.Where(s => s.ProductId == a.ProductId).FirstOrDefault();
                              if (srpcp != null)
                              {
                                  srpcp.SaleSmallQuantity = (srpcp.SaleSmallQuantity ?? 0) + (a.SaleSmallQuantity ?? 0);
                                  srpcp.SaleStrokeQuantity = (srpcp.SaleStrokeQuantity ?? 0) + (a.SaleStrokeQuantity ?? 0);
                                  srpcp.SaleBigQuantity = (srpcp.SaleBigQuantity ?? 0) + (a.SaleBigQuantity ?? 0);
                                  srpcp.SaleAmount = (srpcp.SaleAmount ?? 0) + (a.SaleAmount ?? 0);

                                  srpcp.ReturnSmallQuantity = (srpcp.ReturnSmallQuantity ?? 0) + (a.ReturnSmallQuantity ?? 0);
                                  srpcp.ReturnStrokeQuantity = (srpcp.ReturnStrokeQuantity ?? 0) + (a.ReturnStrokeQuantity ?? 0);
                                  srpcp.ReturnBigQuantity = (srpcp.ReturnBigQuantity ?? 0) + (a.ReturnBigQuantity ?? 0);
                                  srpcp.ReturnAmount = (srpcp.ReturnAmount ?? 0) + (a.ReturnAmount ?? 0);

                                  srpcp.NetSmallQuantity = (srpcp.NetSmallQuantity ?? 0) + ((a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0));
                                  srpcp.NetStrokeQuantity = (srpcp.NetStrokeQuantity ?? 0) + ((a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0));
                                  srpcp.NetBigQuantity = (srpcp.NetBigQuantity ?? 0) + ((a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0));
                                  srpcp.NetAmount = (srpcp.NetAmount ?? 0) + ((a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0));

                                  srpcp.CostAmount = (srpcp.CostAmount ?? 0) + (a.CostAmount ?? 0);
                                  srpcp.Profit = srpcp.NetAmount - srpcp.CostAmount;

                                  if (srpcp.SaleAmount == null || srpcp.SaleAmount == 0)
                                  {
                                      srpcp.SaleProfitRate = 100;
                                  }
                                  else
                                  {
                                      srpcp.SaleProfitRate = ((srpcp.Profit ?? 0) / srpcp.SaleAmount) * 100;
                                  }
                                  if (srpcp.CostAmount == null || srpcp.CostAmount == 0)
                                  {
                                      srpcp.CostProfitRate = 100;
                                  }
                                  else
                                  {
                                      srpcp.CostProfitRate = ((srpcp.Profit ?? 0) / srpcp.CostAmount) * 100;
                                  }

                              }
                              else
                              {
                                  srpcp = new SaleReportProductCostProfit
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
                                  srpcp.SaleSmallQuantity = a.SaleSmallQuantity;
                                  srpcp.SaleStrokeQuantity = a.SaleStrokeQuantity;
                                  srpcp.SaleBigQuantity = a.SaleBigQuantity;
                                  srpcp.SaleAmount = a.SaleAmount;

                                  srpcp.ReturnSmallQuantity = a.ReturnSmallQuantity;
                                  srpcp.ReturnStrokeQuantity = a.ReturnStrokeQuantity;
                                  srpcp.ReturnBigQuantity = a.ReturnBigQuantity;
                                  srpcp.ReturnAmount = a.ReturnAmount;

                                  srpcp.NetSmallQuantity = (a.SaleSmallQuantity ?? 0) - (a.ReturnSmallQuantity ?? 0);
                                  srpcp.NetStrokeQuantity = (a.SaleStrokeQuantity ?? 0) - (a.ReturnStrokeQuantity ?? 0);
                                  srpcp.NetBigQuantity = (a.SaleBigQuantity ?? 0) - (a.ReturnBigQuantity ?? 0);
                                  srpcp.NetAmount = (a.SaleAmount ?? 0) - (a.ReturnAmount ?? 0);

                                  srpcp.CostAmount = a.CostAmount;
                                  srpcp.Profit = srpcp.NetAmount - srpcp.CostAmount;

                                  if (srpcp.SaleAmount == null || srpcp.SaleAmount == 0)
                                  {
                                      srpcp.SaleProfitRate = 100;
                                  }
                                  else
                                  {
                                      srpcp.SaleProfitRate = ((srpcp.Profit ?? 0) / srpcp.SaleAmount) * 100;
                                  }

                                  if (srpcp.CostAmount == null || srpcp.CostAmount == 0)
                                  {
                                      srpcp.CostProfitRate = 100;
                                  }
                                  else
                                  {
                                      srpcp.CostProfitRate = ((srpcp.Profit ?? 0) / srpcp.CostAmount) * 100;
                                  }

                                  reporting.Add(srpcp);
                              }
                          });
                      }
              //将单位整理
              if (reporting != null && reporting.Count > 0)
                      {
                          foreach (SaleReportProductCostProfit item in reporting)
                          {
                              Product product = new Product() { BigUnitId = item.BigUnitId, StrokeUnitId = item.StrokeUnitId, SmallUnitId = item.SmallUnitId ?? 0, BigQuantity = item.BigQuantity, StrokeQuantity = item.StrokeQuantity };
                              Dictionary<string, int> dic = Pexts.GetProductUnits(item.BigUnitId ?? 0, item.BigUnitName, item.StrokeUnitId ?? 0, item.StrokeUnitName, item.SmallUnitId ?? 0, item.SmallUnitName);

                      //销售
                      int sumSaleQuantity = 0;
                              if (item.BigQuantity > 0)
                              {
                                  sumSaleQuantity += (item.SaleBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                              }
                              if (item.StrokeQuantity > 0)
                              {
                                  sumSaleQuantity += (item.SaleStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                              }
                              sumSaleQuantity += (item.SaleSmallQuantity ?? 0);

                              var salequantity = Pexts.StockQuantityFormat(sumSaleQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                              item.SaleBigQuantity = salequantity.Item1;
                              item.SaleStrokeQuantity = salequantity.Item2;
                              item.SaleSmallQuantity = salequantity.Item3;
                              item.SaleQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumSaleQuantity);

                      //退货
                      int sumReturnQuantity = 0;
                              if (item.BigQuantity > 0)
                              {
                                  sumReturnQuantity += (item.ReturnBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                              }
                              if (item.StrokeQuantity > 0)
                              {
                                  sumReturnQuantity += (item.ReturnStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                              }
                              sumReturnQuantity += (item.ReturnSmallQuantity ?? 0);

                              var returnquantity = Pexts.StockQuantityFormat(sumReturnQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                              item.ReturnBigQuantity = returnquantity.Item1;
                              item.ReturnStrokeQuantity = returnquantity.Item2;
                              item.ReturnSmallQuantity = returnquantity.Item3;
                              item.ReturnQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumReturnQuantity);

                      //净销
                      int sumNetQuantity = 0;
                              if (item.BigQuantity > 0)
                              {
                                  sumNetQuantity += (item.NetBigQuantity ?? 0) * (item.BigQuantity ?? 0);
                              }
                              if (item.StrokeQuantity > 0)
                              {
                                  sumNetQuantity += (item.NetStrokeQuantity ?? 0) * (item.StrokeQuantity ?? 0);
                              }
                              sumNetQuantity += (item.NetSmallQuantity ?? 0);

                              var netquantity = Pexts.StockQuantityFormat(sumNetQuantity, item.StrokeQuantity ?? 0, item.BigQuantity ?? 0);
                              item.NetBigQuantity = netquantity.Item1;
                              item.NetStrokeQuantity = netquantity.Item2;
                              item.NetSmallQuantity = netquantity.Item3;
                              item.NetQuantityConversion = product.GetConversionFormat(dic, item.SmallUnitId ?? 0, sumNetQuantity);

                          }
                      }

                      return reporting;
                  }, force);
            }
            catch (Exception)
            {
                return new List<SaleReportProductCostProfit>();
            }
        }

        /// <summary>
        /// 销售额分析API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public SaleAnalysis GetSaleAnalysis(int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GetSaleAnalysis_KEY.FillCacheKey(storeId, businessUserId, brandId, productId, categoryId);
                _cacheManager.Remove(key);
                key.CacheTime = 1;
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GetSaleAnalysis_KEY.FillCacheKey(storeId, businessUserId, brandId, productId, categoryId), () =>
                  {

                      var reporting = new SaleAnalysis
                      {
                          BusinessUserId = businessUserId ?? 0,
                          BusinessUserName = _userService.GetUserName(storeId, businessUserId ?? 0),

                          BrandId = brandId ?? 0,
                          BrandName = _brandService.GetBrandName(storeId, brandId ?? 0),

                          ProductId = productId ?? 0,
                          ProductName = _productService.GetProductName(storeId, productId ?? 0),

                          CategoryId = categoryId ?? 0,
                          CategoryName = _categoryService.GetCategoryName(storeId, categoryId ?? 0),

                  // 今日
                  Today = GetSaleAnalysis(1, storeId, businessUserId, brandId, productId, categoryId),
                  // 今日上周同期
                  LastWeekSame = GetSaleAnalysis(2, storeId, businessUserId, brandId, productId, categoryId),
                  // 昨天
                  Yesterday = GetSaleAnalysis(3, storeId, businessUserId, brandId, productId, categoryId),
                  // 前天
                  BeforeYesterday = GetSaleAnalysis(4, storeId, businessUserId, brandId, productId, categoryId),
                  // 上周
                  LastWeek = GetSaleAnalysis(5, storeId, businessUserId, brandId, productId, categoryId),
                  // 本周
                  ThisWeek = GetSaleAnalysis(6, storeId, businessUserId, brandId, productId, categoryId),
                  //  上月
                  LastMonth = GetSaleAnalysis(7, storeId, businessUserId, brandId, productId, categoryId),
                  // 本月
                  ThisMonth = GetSaleAnalysis(8, storeId, businessUserId, brandId, productId, categoryId),
                  // 本年
                  ThisYear = GetSaleAnalysis(10, storeId, businessUserId, brandId, productId, categoryId)
                      };

                      return reporting;
                  }, force);
            }
            catch (Exception)
            {

                return new SaleAnalysis();
            }
        }

        /// <summary>
        /// 获取订单销售分析
        /// </summary>
        /// <param name="type">1:今日,2:今日上周同期,3:昨天,4:前天,5:上周,6:本周,7:上月,8:本月,9:本季,10:本年</param>
        /// <returns></returns>
        private Sale GetSaleAnalysis(int type, int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false)
        {
            var sale = new Sale();
            try
            {
                #region MSSQL
                //销售
                //string sqlString1 = $"select isnull(sum(isnull(b.Amount,0.00)),0.00) from SaleBills as a inner join Items b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId=b1.Id inner join Categories c1 on p.CategoryId=c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus='1' and a.ReversedStatus = '0' ";
                //销退
                //string sqlString2 = $"select isnull(sum(isnull(b.Amount,0.00)),0.00) from ReturnBills as a inner join Items b on a.Id=b.ReturnBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId = b1.Id inner join Categories c1 on p.CategoryId = c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus = '1' and a.ReversedStatus = '0' ";
                #endregion

                #region MYSQL
                //销售
                string sqlString1 = $"select IFNULL(sum(IFNULL(b.Amount,0.00)),0.00) as 'Value' from SaleBills as a inner join SaleItems b on a.Id=b.SaleBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId=b1.Id inner join Categories c1 on p.CategoryId=c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus='1' and a.ReversedStatus = '0' ";
                //销退
                string sqlString2 = $"select IFNULL(sum(IFNULL(b.Amount,0.00)),0.00) as 'Value' from ReturnBills as a inner join ReturnItems b on a.Id=b.ReturnBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId = b1.Id inner join Categories c1 on p.CategoryId = c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus = '1' and a.ReversedStatus = '0' ";
                #endregion

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    var userIds = _userService.GetSubordinate(storeId??0, businessUserId ?? 0);
                    sqlString1 += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                    sqlString2 += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                }

                if (brandId.HasValue && brandId.Value != 0)
                {
                    sqlString1 += $" and p.BrandId = {brandId} ";
                    sqlString2 += $" and p.BrandId = {brandId} ";
                }

                if (productId.HasValue && productId.Value != 0)
                {
                    sqlString1 += $" and b.ProductId = {productId} ";
                    sqlString2 += $" and b.ProductId = {productId} ";
                }

                if (categoryId.HasValue && categoryId.Value != 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        string incategoryIds = string.Join("','", categoryIds);
                        sqlString1 += $" and p.CategoryId in ('{incategoryIds}') ";
                        sqlString2 += $" and p.CategoryId in ('{incategoryIds}') ";
                    }
                    else
                    {
                        sqlString1 += $" and p.CategoryId = {categoryId} ";
                        sqlString2 += $" and p.CategoryId = {categoryId} ";
                    }
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and to_days(a.CreatedOnUtc) = to_days(now()) ";
                            sqlString2 += $" and to_days(a.CreatedOnUtc) = to_days(now()) ";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";
                            //sqlString2 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                            sqlString2 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                            sqlString2 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                        }
                        break;
                }

                sale.SaleAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(sqlString1).FirstOrDefault().Value;
                sale.SaleReturnAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(sqlString2).FirstOrDefault().Value;
                sale.NetAmount = sale.SaleAmount - sale.SaleReturnAmount;

                return sale;
            }
            catch (Exception)
            {
                return sale;
            }
        }

        /// <summary>
        /// 客户拜访分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public CustomerVistAnalysis GetCustomerVistAnalysis(int? storeId, int? businessUserId, bool force = false)
        {
            try
            {
                int totalTerminals = TerminalsRepository_RO.Table.Where(s => s.StoreId == storeId).Count();

                var reporting = new CustomerVistAnalysis
                {
                    BusinessUserId = businessUserId ?? 0,
                    BusinessUserName = _userService.GetUserName(storeId, businessUserId ?? 0),
                    TotalCustomer = totalTerminals,
                    TotalVist = VisitStoreRepository_RO.Table.Where(s => s.StoreId == storeId && s.BusinessUserId == businessUserId).Count(),
                    // 今日
                    Today = GetVistAnalysis(1, storeId, businessUserId, totalTerminals),
                    // 今日上周同期
                    LastWeekSame = GetVistAnalysis(2, storeId, businessUserId, totalTerminals),
                    // 昨天
                    Yesterday = GetVistAnalysis(3, storeId, businessUserId, totalTerminals),
                    // 前天
                    BeforeYesterday = GetVistAnalysis(4, storeId, businessUserId, totalTerminals),
                    // 上周
                    LastWeek = GetVistAnalysis(5, storeId, businessUserId, totalTerminals),
                    // 本周
                    ThisWeek = GetVistAnalysis(6, storeId, businessUserId, totalTerminals),
                    //  上月
                    LastMonth = GetVistAnalysis(7, storeId, businessUserId, totalTerminals),
                    // 本月
                    ThisMonth = GetVistAnalysis(8, storeId, businessUserId, totalTerminals),
                    // 本年
                    ThisYear = GetVistAnalysis(10, storeId, businessUserId, totalTerminals),
                };
                return reporting;
            }
            catch (Exception)
            {
                return new CustomerVistAnalysis();
            }
        }

        /// <summary>
        /// 获取拜访分析
        /// </summary>
        /// <param name="type">1:今日,2:今日上周同期,3:昨天,4:前天,5:上周,6:本周,7:上月,8:本月,9:本季,10:本年</param>
        /// <returns></returns>
        private Vist GetVistAnalysis(int type, int? storeId, int? businessUserId, int totalTerminals, bool force = false)
        {
            var vist = new Vist();

            try
            {
                string sqlString = $"select count(*) as 'Value' from VisitStore as a where a.StoreId={storeId ?? 0} ";

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    sqlString += $" and a.BusinessUserId = {businessUserId} ";
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(day, a.SigninDateTime, GETDATE())= 0 ";
                            //MYSQL
                            //sqlString += $" and DiffDays(current_date,a.signinDateTime)= 0 ";
                            sqlString += $" and to_days(now()) = to_days(a.signinDateTime)";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(day,a.SigninDateTime,DATEADD(day,-7,GETDATE()))=0 ";
                            //MYSQL
                            //sqlString += $" and DiffDays(date_add(current_date, interval -7 day),a.signinDateTime)= 0 ";
                            sqlString += $" and to_days(now()) - to_days(a.signinDateTime) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(day, a.SigninDateTime, GETDATE())= 1 ";
                            //MYSQL
                            //sqlString += $" and DiffDays(current_date,a.signinDateTime)= 1 ";
                            sqlString += $" and to_days(now()) - to_days(a.signinDateTime) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(day, a.SigninDateTime, GETDATE())= 2 ";
                            //MYSQL
                            //sqlString += $" and DiffDays(current_date,a.signinDateTime)= 2 ";
                            sqlString += $" and to_days(now()) - to_days(a.signinDateTime) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(week, a.SigninDateTime, GETDATE())= 1 ";
                            //MYSQL
                            sqlString += $" and YEARWEEK(date_format(a.signinDateTime,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(week, a.SigninDateTime, GETDATE())= 0 ";
                            //MYSQL
                            sqlString += $" and YEARWEEK(date_format(a.signinDateTime,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(month, a.SigninDateTime, GETDATE())= 1 ";
                            //MYSQL
                            sqlString += $" and date_format(a.signinDateTime,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(month, a.SigninDateTime, GETDATE())= 0 ";
                            //MYSQL
                            sqlString += $" and date_format(a.signinDateTime,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(quarter, a.SigninDateTime, GETDATE())= 0 ";
                            //MYSQL
                            sqlString += $" and QUARTER(a.signinDateTime)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MSSQL
                            //sqlString += $" and DATEDIFF(year, a.SigninDateTime, GETDATE())= 0 ";
                            //MYSQL
                            sqlString += $" and YEAR(a.signinDateTime)=YEAR(NOW()) ";
                        }
                        break;
                }

                vist.VistCount = VisitStoreRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList().FirstOrDefault().Value;
                if (totalTerminals > 0)
                {
                    vist.Percentage = vist.VistCount / (double)totalTerminals * 100;
                }

                return vist;
            }
            catch (Exception)
            {
                return vist;
            }
        }

        /// <summary>
        /// 新增加客户分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public NewCustomerAnalysis GetNewUserAnalysis(int? storeId, int? businessUserId, bool force = false)
        {
            try
            {
                int totalTerminals = TerminalsRepository_RO.Table.Where(s => s.StoreId == storeId).Count();
                var reporting = new NewCustomerAnalysis
                {
                    BusinessUserId = businessUserId ?? 0,
                    BusinessUserName = _userService.GetUserName(storeId, businessUserId ?? 0),
                    TotalCustomer = totalTerminals,
                    // 今日
                    Today = GetNewAddAnalysis(1, storeId, businessUserId),
                    // 今日上周同期
                    LastWeekSame = GetNewAddAnalysis(2, storeId, businessUserId),
                    // 昨天
                    Yesterday = GetNewAddAnalysis(3, storeId, businessUserId),
                    // 前天
                    BeforeYesterday = GetNewAddAnalysis(4, storeId, businessUserId),
                    // 上周
                    LastWeek = GetNewAddAnalysis(5, storeId, businessUserId),
                    // 本周
                    ThisWeek = GetNewAddAnalysis(6, storeId, businessUserId),
                    //  上月
                    LastMonth = GetNewAddAnalysis(7, storeId, businessUserId),
                    // 本月
                    ThisMonth = GetNewAddAnalysis(8, storeId, businessUserId),
                    // 本年
                    ThisYear = GetNewAddAnalysis(10, storeId, businessUserId)
                };
                return reporting;
            }
            catch (Exception)
            {
                return new NewCustomerAnalysis();
            }
        }

        /// <summary>
        /// 获取新增分析
        /// </summary>
        /// <param name="type">1:今日,2:今日上周同期,3:昨天,4:前天,5:上周,6:本周,7:上月,8:本月,9:本季,10:本年</param>
        /// <returns></returns>
        private Signin GetNewAddAnalysis(int type, int? storeId, int? businessUserId, bool force = false)
        {
            var signin = new Signin();

            try
            {
                //MSSQL
                //string sqlString1 = $"select count(*) from dcms_crm.CRM_Terminals as a where a.StoreId={storeId ?? 0} ";
                //string sqlString2 = $"select (STUFF(( SELECT  ',' + CONVERT(VARCHAR(10),a.Id)  from  Terminals as a where a.StoreId = {storeId ?? 0} ";
                //MYSQL
                string sqlString1 = $"select count(*) as 'Value' from dcms_crm.CRM_Terminals as a where a.StoreId={storeId ?? 0} ";
                string sqlString2 = $"select IFNULL(group_concat(a.Id separator ','),'0') as 'Value'  from  dcms_crm.CRM_Terminals as a where a.StoreId = {storeId ?? 0} ";


                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    sqlString1 += $" and a.CreatedUserId = {businessUserId} ";
                    sqlString2 += $" and a.CreatedUserId = {businessUserId} ";
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) = to_days(a.CreatedOnUtc) ";
                            sqlString2 += $" and to_days(now()) = to_days(a.CreatedOnUtc) ";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";
                            //sqlString2 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                            sqlString2 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                            sqlString2 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                        }
                        break;
                }

                //MSSQL
                //sqlString2 += $" FOR XML PATH('') ), 1, 1, ''))";

                string ids = TerminalsRepository_RO.QueryFromSql<StringQueryType>(sqlString2).ToList().FirstOrDefault()?.Value;

                signin.Count = TerminalsRepository_RO.QueryFromSql<IntQueryType>(sqlString1).ToList().FirstOrDefault().Value;
                signin.TerminalIds = !string.IsNullOrEmpty(ids) ? ids.Split(',').Select(s => int.Parse(s)).ToList() : new List<int>();

                return signin;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return signin;
            }
        }


        /// <summary>
        /// 获取经销商品牌销量汇总API
        /// </summary>
        /// <param name="store"></param>
        /// <param name="brandIds"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<BrandRanking> GetBrandRanking(int? storeId, int[] brandIds, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false, bool? auditedStatus = null)
        {
            try
            {
                var reporting = new List<BrandRanking>();
                var dic = new Dictionary<int, string>();

                reporting = GetSaleReportSummaryBrand(storeId, brandIds, null, null, businessUserId, null, startTime, endTime, dic, auditedStatus: auditedStatus).Select(s =>
                {
                    return new BrandRanking
                    {
                        BrandId = s.BrandId ?? 0,
                        BrandName = s.BrandName,
                        NetAmount = s.NetAmount,
                        Profit = s.Profit,
                        SaleAmount = s.SaleAmount,
                        SaleReturnAmount = s.ReturnAmount
                    };
                }).ToList();

                var totalNetAmount = reporting.Select(s => s.NetAmount ?? 0).Sum();

                reporting.ForEach(s =>
                {
                    s.Percentage = totalNetAmount > 0 ? (double)(s.NetAmount ?? 0 / totalNetAmount * 100) : 0;
                });
                return reporting.OrderByDescending(s => s.NetAmount).ToList();
            }
            catch (Exception)
            {
                return new List<BrandRanking>();
            }
        }

        /// <summary>
        /// 获取经销商业务员排行榜API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<BusinessRanking> GetBusinessRanking(int? storeId, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            var reporting = new List<BusinessRanking>();
            try
            {
                var users = _userService.GetAllUsers(storeId);
                users.ForEach(u =>
                {
                    var sale = SaleBillsRepository_RO.Table.Where(s => s.StoreId == storeId && s.BusinessUserId == u.Id);
                    var retu = ReturnBillsRepository_RO.Table.Where(s => s.StoreId == storeId && s.BusinessUserId == u.Id);
                    if (startTime.HasValue)
                    {
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                        sale = sale.Where(s => s.CreatedOnUtc >= startTime);
                        retu = retu.Where(s => s.CreatedOnUtc >= startTime);
                    }

                    if (endTime.HasValue)
                    {
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                        sale = sale.Where(s => s.CreatedOnUtc <= endTime);
                        retu = retu.Where(s => s.CreatedOnUtc <= endTime);
                    }

                    reporting.Add(new BusinessRanking
                    {
                        BusinessUserId = u.Id,
                        BusinessUserName = u.UserRealName,
                        NetAmount = sale.ToList()?.Select(s => s.ReceivableAmount).Sum() - retu.ToList()?.Select(s => s.ReceivableAmount).Sum(),
                        Profit = sale.ToList()?.Select(s => s.SumProfit).Sum() - retu.ToList()?.Select(s => s.SumProfit).Sum(),
                        SaleAmount = sale.ToList()?.Select(s => s.ReceivableAmount).Sum(),
                        SaleReturnAmount = retu.ToList()?.Select(s => s.ReceivableAmount).Sum()
                    });
                });

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    reporting = reporting.Where(s => s.BusinessUserId == businessUserId).ToList();
                }

                return reporting.OrderByDescending(s => s.NetAmount).ToList();
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 获取客户排行榜API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="terminalId"></param>
        /// <param name="districtId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<CustomerRanking> GetCustomerRanking(int? storeId, int? terminalId, int? districtId, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false)
        {

            var reporting = new List<CustomerRanking>();

            try
            {
                string whereQuery = $" a.StoreId= {storeId ?? 0}";
                string whereQuery2 = $" a.StoreId= {storeId ?? 0}";

                if (terminalId.HasValue && terminalId.Value != 0)
                {
                    whereQuery += $" and a.TerminalId = '{terminalId}' ";
                    whereQuery2 += $" and a.TerminalId = '{terminalId}' ";
                }

                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                    if (distinctIds != null && distinctIds.Count > 0)
                    {
                        string inDistinctIds = string.Join("','", distinctIds);
                        whereQuery += $" and t.DistrictId in ('{inDistinctIds}') ";
                        whereQuery2 += $" and a.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        whereQuery += $" and t.DistrictId = '{districtId}' ";
                        whereQuery2 += $" and a.DistrictId = '{districtId}' ";
                    }
                }

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    var userIds = _userService.GetSubordinate(storeId, businessUserId??0);
                    whereQuery += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                    whereQuery2 += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                }
                if (startTime.HasValue)
                {
                    //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    //string startTimedata = startTime.Value.ToString("yyyy-MM-dd 00:00:00");
                    whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                    whereQuery2 += $" and a.SigninDateTime >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                }

                if (endTime.HasValue)
                {
                    //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    //string endTimedata = endTime.Value.ToString("yyyy-MM-dd 23:59:59");
                    whereQuery += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                    whereQuery2 += $" and a.SigninDateTime <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                }

                //MSSQL
                //string sqlString = $"select alls.TerminalId,alls.TerminalName,alls.VisitSum,sum(alls.SaleAmount) as SaleAmount,sum(alls.SaleReturnAmount) as SaleReturnAmount from ((select t.Id TerminalId  ,t.Name TerminalName, 0 VisitSum ,a.SumAmount as SaleAmount,'0' SaleReturnAmount  from  SaleBills a inner join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id  where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL(select t.Id TerminalId, t.Name TerminalName, 0 VisitSum  ,'0' SaleAmount,a.SumAmount as SaleReturnAmount  from ReturnBills a inner join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id  where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')) as alls group by alls.TerminalId,alls.TerminalName,alls.VisitSum";

                //MYSQL
                string sqlString = $"select alls.TerminalId,alls.TerminalName,alls.VisitSum,sum(alls.SaleAmount) as SaleAmount,sum(alls.SaleReturnAmount) as SaleReturnAmount,sum(alls.SaleAmount) - sum(alls.SaleReturnAmount) as NetAmount from ((select t.Id TerminalId ,t.Name TerminalName, 0 VisitSum ,a.ReceivableAmount as SaleAmount,'0' SaleReturnAmount  from  SaleBills a inner join dcms_crm.CRM_Terminals t on a.TerminalId=t.Id  where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0') UNION ALL(select t.Id TerminalId, t.Name TerminalName, 0 VisitSum  ,'0' SaleAmount,a.ReceivableAmount as SaleReturnAmount  from ReturnBills a inner join dcms_crm.CRM_Terminals t on a.TerminalId = t.Id  where {whereQuery} and a.AuditedStatus = '1' and a.ReversedStatus = '0')) as alls group by alls.TerminalId,alls.TerminalName,alls.VisitSum";


                var items = SaleBillsRepository_RO.QueryFromSql<CustomerRanking>(sqlString).ToList();
                if (items != null)
                {
                    items.ForEach(s =>
                    {
                        string sqlString2 = $"select count(*) as 'Value' from VisitStore as a where {whereQuery2}";
                        if (!terminalId.HasValue || terminalId.Value == 0)
                        {
                            sqlString2 = $"select count(*) as 'Value' from VisitStore as a where a.TerminalId ={s.TerminalId} and {whereQuery2}";
                        }

                        s.VisitSum = VisitStoreRepository_RO.QueryFromSql<IntQueryType>(sqlString2).ToList().FirstOrDefault().Value;
                        s.NetAmount = s.SaleAmount - s.SaleReturnAmount;
                    });
                }

                return items.OrderByDescending(s => s.NetAmount).ToList();
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 获取经销商滞销排行榜API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<UnSaleRanking> GetUnSaleRanking(int? storeId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            var reporting = new List<UnSaleRanking>();

            try
            {
                int[] brandIds = new int[] { brandId ?? 0 };

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
                        string inDistinctIds = string.Join("','", categoryIds);
                        query = query.Where(a => categoryIds.Contains(a.CategoryId));
                    }
                    else
                    {
                        query = query.Where(p => p.CategoryId == categoryId);
                    }
                }

                //去重
                //query = from p in query group p by p.Id into pGroup orderby pGroup.Key select pGroup.FirstOrDefault();

                var allProducts = query.ToList();
                var productIds = allProducts.Select(sd => sd.Id).ToArray();
                var uctos = _productService.UnitConversions(storeId, allProducts, productIds);

                var products = allProducts.Select(p =>
                {
                    var utc = uctos.Where(s => s.Product.Id == p.Id).FirstOrDefault();
                    var sqt = p.Stocks.Select(s => s.UsableQuantity ?? 0).Sum();
                    return new UnSaleRanking()
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        BrandId = p.BrandId,
                        CategoryId = p.CategoryId,

                        TerminalId = 0,
                        BusinessUserId = 0,

                        TotalSumSaleQuantity = 0,
                        TotalSumSaleQuantityConversion = "",
                        TotalSumSaleAmount = 0,

                        TotalSumReturnQuantity = 0,
                        TotalSumReturnQuantityConversion = "",
                        TotalSumReturnAmount = 0,


                        TotalSumNetQuantity = 0,
                        TotalSumNetQuantityConversion = "",
                        TotalSumNetAmount = 0,
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

                if (businessUserId.HasValue && businessUserId != 0)
                {
                    sbs = sbs.Where(p => p.BusinessUserId == businessUserId);
                }
                //销售项目
                var saleItems = new List<SaleItem>();
                sbs.ToList().ForEach(sb =>
                {
                    saleItems.AddRange(sb.Items);
                });
                var allspds = saleItems.Where(s => productIds.Contains(s.ProductId));

                //（3）取未销售商品(差集)
                var unSalepds = from p in allspds
                                group p by p.ProductId into pGroup
                                orderby pGroup.Key
                                select pGroup.FirstOrDefault().ProductId;
                var unSales = productIds.Except(unSalepds);
                products = products.Where(s => unSales.Contains(s.ProductId ?? 0)).ToList();

                //（4）取未销售的销售情况
                products.ForEach(p =>
                {
                    p.TerminalId = 0;
                    p.BusinessUserId = businessUserId ?? 0;

            //销售量必为空
            p.TotalSumSaleQuantity = 0;
                    p.TotalSumSaleQuantityConversion = "";
                    p.TotalSumSaleAmount = 0;

                    p.TotalSumReturnQuantity = 0;
                    p.TotalSumReturnQuantityConversion = "";
                    p.TotalSumReturnAmount = 0;

                    p.TotalSumNetQuantity = 0;
                    p.TotalSumNetQuantityConversion = "";
                    p.TotalSumNetAmount = 0;
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

                if (businessUserId.HasValue && businessUserId != 0)
                {
                    rbs = rbs.Where(p => p.BusinessUserId == businessUserId);
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

                    p.TotalSumReturnQuantity = returnQuantity;
                    p.TotalSumReturnQuantityConversion = cup.StockQuantityFormat(utc.Option, returnQuantity);
                    p.TotalSumReturnAmount = returnAmount;

                    p.TotalSumNetQuantity = 0;
                    p.TotalSumNetQuantityConversion = "";
                    p.TotalSumNetAmount = 0;
                });


                //（3）计算净额
                products.ForEach(p =>
                {
            //cup 不可能为空
            var cup = allProducts.Select(s => s).Where(s => s.Id == p.ProductId).First();
                    var utc = uctos.Where(s => s.Product.Id == cup.Id).FirstOrDefault();
                    var curps = allspds.Select(s => s).Where(s => s.ProductId == p.ProductId).ToList();

                    var netQuantity = p.TotalSumSaleQuantity - p.TotalSumReturnQuantity;

                    p.TotalSumNetQuantity = netQuantity;
                    p.TotalSumNetQuantityConversion = cup.StockQuantityFormat(utc.Option, netQuantity);
                    p.TotalSumNetAmount = p.TotalSumSaleAmount - p.TotalSumReturnAmount;
                });

                //排序
                reporting = products.OrderByDescending(s => s.TotalSumNetAmount).ToList();

                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }

        }

        /// <summary>
        /// 订单额分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public OrderQuantityAnalysisQuery GetOrderQuantityAnalysis(int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false)
        {
            try
            {
                var key = DCMSDefaults.SALEREPORTSERVICE_GETORDER_QUANTITY_ANALYSIS_KEY.FillCacheKey(storeId, businessUserId, brandId, productId, categoryId);
                key.CacheTime = 1;
                return _cacheManager.Get(key, () =>
                   {
                       var reporting = new OrderQuantityAnalysisQuery
                       {
                   // 今日
                   Today = GetOrderSale(1, storeId, businessUserId, brandId, productId, categoryId),
                   // 今日上周同期
                   LastWeekSame = GetOrderSale(2, storeId, businessUserId, brandId, productId, categoryId),
                   // 昨天
                   Yesterday = GetOrderSale(3, storeId, businessUserId, brandId, productId, categoryId),
                   // 前天
                   BeforeYesterday = GetOrderSale(4, storeId, businessUserId, brandId, productId, categoryId),
                   // 上周
                   LastWeek = GetOrderSale(5, storeId, businessUserId, brandId, productId, categoryId),
                   // 本周
                   ThisWeek = GetOrderSale(6, storeId, businessUserId, brandId, productId, categoryId),
                   //  上月
                   LastMonth = GetOrderSale(7, storeId, businessUserId, brandId, productId, categoryId),
                   // 本月
                   ThisMonth = GetOrderSale(8, storeId, businessUserId, brandId, productId, categoryId),
                   // 本季
                   ThisQuarter = GetOrderSale(9, storeId, businessUserId, brandId, productId, categoryId),
                   // 本年
                   ThisYear = GetOrderSale(10, storeId, businessUserId, brandId, productId, categoryId)
                       };

                       return reporting;
                   }, force);
            }
            catch (Exception)
            {
                return new OrderQuantityAnalysisQuery();
            }
        }

        /// <summary>
        /// 获取订单销售分析
        /// </summary>
        /// <param name="type">1:今日,2:今日上周同期,3:昨天,4:前天,5:上周,6:本周,7:上月,8:本月,9:本季,10:本年</param>
        /// <returns></returns>
        private Sale GetOrderSale(int type, int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false)
        {
            var sale = new Sale();

            try
            {
                #region MSSQL
                //销定
                //string sqlString1 = $"select isnull(sum(isnull(b.Amount,0.00)),0.00) from SaleReservationBills as a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId=b1.Id inner join Categories c1 on p.CategoryId=c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus='1' and a.ReversedStatus = '0' ";
                //退定
                //string sqlString2 = $"select isnull(sum(isnull(b.Amount,0.00)),0.00) from ReturnReservationBills as a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId = b1.Id inner join Categories c1 on p.CategoryId = c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus = '1' and a.ReversedStatus = '0' ";
                #endregion

                #region MYSQL
                //销定
                string sqlString1 = $"select IFNULL(sum(IFNULL(b.Amount,0.00)),0.00) as 'Value' from SaleReservationBills as a inner join SaleReservationItems b on a.Id=b.SaleReservationBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId=b1.Id inner join Categories c1 on p.CategoryId=c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus='1' and a.ReversedStatus = '0' ";
                //退定
                string sqlString2 = $"select IFNULL(sum(IFNULL(b.Amount,0.00)),0.00) as 'Value' from ReturnReservationBills as a inner join ReturnReservationItems b on a.Id=b.ReturnReservationBillId inner join Products p on b.ProductId=p.Id inner join Brands b1 on p.BrandId = b1.Id inner join Categories c1 on p.CategoryId = c1.Id where a.StoreId={storeId ?? 0} and a.AuditedStatus = '1' and a.ReversedStatus = '0' ";
                #endregion

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    var userIds = _userService.GetSubordinate(storeId, businessUserId??0);
                    sqlString1 += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                    sqlString2 += $" and a.BusinessUserId in ({string.Join(",", userIds)}) ";
                }

                if (brandId.HasValue && brandId.Value != 0)
                {
                    sqlString1 += $" and p.BrandId = {brandId} ";
                    sqlString2 += $" and p.BrandId = {brandId} ";
                }

                if (productId.HasValue && productId.Value != 0)
                {
                    sqlString1 += $" and b.ProductId = {productId} ";
                    sqlString2 += $" and b.ProductId = {productId} ";
                }

                if (categoryId.HasValue && categoryId.Value != 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        string incategoryIds = string.Join("','", categoryIds);
                        sqlString1 += $" and c1.Id in ('{incategoryIds}') ";
                        sqlString2 += $" and c1.Id in ('{incategoryIds}') ";
                    }
                    else
                    {
                        sqlString1 += $" and p.CategoryId = {categoryId} ";
                        sqlString2 += $" and p.CategoryId = {categoryId} ";
                    }
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL 
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(a.CreatedOnUtc) = to_days(now()) ";
                            sqlString2 += $" and to_days(a.CreatedOnUtc) = to_days(now()) ";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";
                            //sqlString2 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                            sqlString2 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                            sqlString2 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                        }
                        break;
                }

                sale.SaleAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(sqlString1).FirstOrDefault().Value;
                sale.SaleReturnAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(sqlString2).FirstOrDefault().Value;
                sale.NetAmount = sale.SaleAmount - sale.SaleReturnAmount;

                return sale;
            }
            catch (Exception )
            {
                return sale;
            }
        }

        /// <summary>
        /// 获取经销商热销排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<HotSaleRanking> GetHotSaleRanking(int? storeId, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {
            try
            {
                var reporting = new List<HotSaleRanking>();

                var hotSales = GetSaleReportHotSale(storeId, null, null, businessUserId, null, terminalId, null, brandId, categoryId, startTime, endTime, 100).ToList();
                hotSales.ForEach(h =>
                {

                    var sQ = ((h.SaleBigQuantity ?? 0) * (h.BigQuantity ?? 0)) + ((h.SaleStrokeQuantity ?? 0) * (h.StrokeQuantity ?? 0)) + h.SaleSmallQuantity ?? 0;
                    var rQ = ((h.ReturnBigQuantity ?? 0) * (h.BigQuantity ?? 0)) + ((h.ReturnStrokeQuantity ?? 0) * (h.StrokeQuantity ?? 0)) + h.ReturnSmallQuantity ?? 0;

                    reporting.Add(new HotSaleRanking
                    {
                        /// 商品
                        ProductId = h.ProductId,
                        /// 商品
                        ProductName = h.ProductName,
                        /// 客户Id
                        TerminalId = terminalId,
                        /// 业务员
                        BusinessUserId = h.BusinessUserId,
                        /// 品牌
                        BrandId = brandId,
                        /// 商品类别
                        CategoryId = categoryId,
                        /// 销售数量
                        TotalSumSaleQuantity = sQ,
                        /// 销售数量转换
                        TotalSumSaleQuantityConversion = $"{h.SaleBigQuantity}大{h.SaleStrokeQuantity}中{h.SaleSmallQuantity}小",
                        /// 销售金额
                        TotalSumSaleAmount = h.SaleAmount,

                        /// 退货数量
                        TotalSumReturnQuantity = rQ,
                        /// 退货数量转换
                        TotalSumReturnQuantityConversion = $"{h.ReturnBigQuantity}大{h.ReturnStrokeQuantity}中{h.ReturnSmallQuantity}小",
                        /// 退货金额
                        TotalSumReturnAmount = h.ReturnAmount,

                        /// 净销售量 = 销售数量 - 退货数量
                        TotalSumNetQuantity = sQ - rQ,
                        /// 净销售量转换
                        TotalSumNetQuantityConversion = h.NetQuantityConversion,
                        /// 销售净额 = 销售金额 - 退货金额
                        TotalSumNetAmount = h.NetAmount,
                        /// 退货率 = 退货数量  /  销售数量
                        ReturnRate = sQ > 0 ? ((double)rQ / (double)sQ * 100) : 0,
                    });
                });

                return reporting.OrderByDescending(s => s.TotalSumNetAmount).ToList();
            }
            catch (Exception)
            {
                return new List<HotSaleRanking>();
            }
        }

        /// <summary>
        /// 获取经销商热定(销订)排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IList<HotSaleRanking> GetHotOrderRanking(int? storeId, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false)
        {

            var reporting = new List<HotSaleRanking>();

            try
            {
                var hotSales = GetOrderReportHotSale(storeId, null, businessUserId, null, terminalId, brandId, categoryId, startTime, endTime, 100).ToList();
                hotSales.ForEach(h =>
                {

                    var sQ = ((h.SaleBigQuantity ?? 0) * (h.BigQuantity ?? 0)) + ((h.SaleStrokeQuantity ?? 0) * (h.StrokeQuantity ?? 0)) + h.SaleSmallQuantity ?? 0;
                    var rQ = ((h.ReturnBigQuantity ?? 0) * (h.BigQuantity ?? 0)) + ((h.ReturnStrokeQuantity ?? 0) * (h.StrokeQuantity ?? 0)) + h.ReturnSmallQuantity ?? 0;

                    reporting.Add(new HotSaleRanking
                    {
                /// 商品
                ProductId = h.ProductId,
                /// 商品
                ProductName = h.ProductName,
                /// 客户Id
                TerminalId = terminalId,
                /// 业务员
                BusinessUserId = h.BusinessUserId,
                /// 品牌
                BrandId = brandId,
                /// 商品类别
                CategoryId = categoryId,
                /// 销售数量
                TotalSumSaleQuantity = sQ,
                /// 销售数量转换
                TotalSumSaleQuantityConversion = $"{h.SaleBigQuantity}大{h.SaleStrokeQuantity}中{h.SaleSmallQuantity}小",
                /// 销售金额
                TotalSumSaleAmount = h.SaleAmount,

                /// 退货数量
                TotalSumReturnQuantity = rQ,
                /// 退货数量转换
                TotalSumReturnQuantityConversion = $"{h.ReturnBigQuantity}大{h.ReturnStrokeQuantity}中{h.ReturnSmallQuantity}小",
                /// 退货金额
                TotalSumReturnAmount = h.ReturnAmount,

                /// 净销售量 = 销售数量 - 退货数量
                TotalSumNetQuantity = sQ - rQ,
                /// 净销售量转换
                TotalSumNetQuantityConversion = h.NetQuantityConversion,
                /// 销售净额 = 销售金额 - 退货金额
                TotalSumNetAmount = h.NetAmount,
                /// 退货率 = 退货数量  /  销售数量
                ReturnRate = sQ > 0 ? (rQ / sQ * 100) : 0,
                    });
                });

                return reporting.OrderByDescending(s => s.TotalSumNetAmount).ToList();
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 新增加订单分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public NewOrderAnalysis GetNewOrderAnalysis(int? storeId, int? businessUserId, bool force = false)
        {
            try
            {
                int totalOrders = SaleReservationBillsRepository_RO.Table.Where(s => s.StoreId == storeId).Count();
                var reporting = new NewOrderAnalysis
                {
                    BusinessUserId = businessUserId ?? 0,
                    BusinessUserName = _userService.GetUserName(storeId, businessUserId ?? 0),
                    // 今日
                    Today = GetNewAddOrderAnalysis(1, storeId, businessUserId),
                    // 今日上周同期
                    LastWeekSame = GetNewAddOrderAnalysis(2, storeId, businessUserId),
                    // 昨天
                    Yesterday = GetNewAddOrderAnalysis(3, storeId, businessUserId),
                    // 前天
                    BeforeYesterday = GetNewAddOrderAnalysis(4, storeId, businessUserId),
                    // 上周
                    LastWeek = GetNewAddOrderAnalysis(5, storeId, businessUserId),
                    // 本周
                    ThisWeek = GetNewAddOrderAnalysis(6, storeId, businessUserId),
                    //  上月
                    LastMonth = GetNewAddOrderAnalysis(7, storeId, businessUserId),
                    // 本月
                    ThisMonth = GetNewAddOrderAnalysis(8, storeId, businessUserId),
                    // 本年
                    ThisYear = GetNewAddOrderAnalysis(10, storeId, businessUserId)
                };
                return reporting;
            }
            catch (Exception)
            {
                return new NewOrderAnalysis();
            }
        }

        //
        private Order GetNewAddOrderAnalysis(int type, int? storeId, int? businessUserId)
        {
            var signin = new Order();

            try
            {
                //MSSQL
                //string sqlString1 = $"select count(*) from dcms_crm.CRM_Terminals as a where a.StoreId={storeId ?? 0} ";
                //string sqlString2 = $"select (STUFF(( SELECT  ',' + CONVERT(VARCHAR(10),a.Id)  from  Terminals as a where a.StoreId = {storeId ?? 0} ";
                //MYSQL
                string sqlString1 = $"select count(*) as 'Value' from SaleReservationBills as a where a.StoreId={storeId ?? 0} and a.AuditedStatus = true and a.ReversedStatus = false ";
                string sqlString2 = $"select group_concat(a.Id separator ',') as 'Value'  from  SaleReservationBills as a where a.StoreId = {storeId ?? 0} and a.AuditedStatus = true and a.ReversedStatus = false  ";

                //if (makeuserId.HasValue && makeuserId > 0)
                //{
                //    var userIds = _userService.GetSubordinate(store, makeuserId ?? 0)?.Where(s => s > 0).ToList();
                //    if (userIds.Count > 0)
                //        query = query.Where(x => userIds.Contains(x.MakeUserId));
                //}

                if (businessUserId.HasValue && businessUserId.Value != 0)
                {
                    sqlString1 += $" and a.BusinessUserId in ({businessUserId}) ";
                    sqlString2 += $" and a.BusinessUserId in ({businessUserId}) ";

                    //var userIds = _userService.GetSubordinate(storeId, businessUserId ?? 0);
                    //if (userIds.Count > 0)
                    //{
                    //    sqlString1 += $" and a.BusinessUserId in ({businessUserId}) ";
                    //    sqlString2 += $" and a.BusinessUserId in ({businessUserId}) ";
                    //}
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) = to_days(a.CreatedOnUtc) ";
                            sqlString2 += $" and to_days(now()) = to_days(a.CreatedOnUtc) ";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            //MSSQL
                            //sqlString1 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";
                            //sqlString2 += $" and DATEDIFF(day,a.CreatedOnUtc,DATEADD(day,-7,GETDATE()))=0 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            //sqlString2 += $" and DiffDays(date_add(current_date, interval -7 day),a.CreatedOnUtc)= 0 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 1 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";
                            //sqlString2 += $" and DATEDIFF(day, a.CreatedOnUtc, GETDATE())= 2 ";

                            //MYQL
                            //sqlString1 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            //sqlString2 += $" and DiffDays(current_date,a.CreatedOnUtc)= 2 ";
                            sqlString1 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                            sqlString2 += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(week, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                            sqlString2 += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 1 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(month, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                            sqlString2 += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(quarter, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                            sqlString2 += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MSQL
                            //sqlString1 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";
                            //sqlString2 += $" and DATEDIFF(year, a.CreatedOnUtc, GETDATE())= 0 ";

                            //MYQL
                            sqlString1 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                            sqlString2 += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                        }
                        break;
                }

                //MSSQL
                //sqlString2 += $" FOR XML PATH('') ), 1, 1, ''))";

                string ids = SaleReservationBillsRepository_RO.QueryFromSql<StringQueryType>(sqlString2).ToList().FirstOrDefault()?.Value;

                signin.OrderCount = SaleReservationBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString1).ToList().FirstOrDefault().Value;
                signin.BillIds = !string.IsNullOrEmpty(ids) ? ids.Split(',').Select(s => int.Parse(s)).ToList() : new List<int>();

                return signin;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return signin;
            }
        }

        /// <summary>
        /// 业务员综合分析
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public BusinessAnalysis GetBusinessAnalysis(int type, int? storeId, DateTime? start = null, DateTime? end = null, int userId = 0)
        {
            try
            {
                var s1 = $" and to_days(now()) = to_days(v.SigninDateTime) ";
                var s2 = $" and to_days(now()) = to_days(sb.CreatedOnUtc) ";

                string userWhere = "";
                if (userId > 0 && !_userService.IsAdmin(storeId,userId))
                {
                    userWhere = $" and u.Id = {userId} ";
                }

                if (type == 0 && start.HasValue && end.HasValue)
                {
                    s1 = $" and (v.SigninDateTime >= '" + start?.ToString("yyyy-MM-dd 00:00:00") + "' and v.SigninDateTime <= '" + end?.ToString("yyyy-MM-dd 23:59:59") + "') ";
                    s2 = $" and (sb.CreatedOnUtc >= '" + start?.ToString("yyyy-MM-dd 00:00:00") + "' and sb.CreatedOnUtc <= '" + end?.ToString("yyyy-MM-dd 23:59:59") + "') ";
                }
                else
                {
                    switch (type)
                    {
                        //今日
                        case 1:
                            {
                                s1 = $" and to_days(now()) = to_days(v.SigninDateTime) ";
                                s2 = $" and to_days(now()) = to_days(sb.CreatedOnUtc) ";
                            }
                            break;
                        //昨天
                        case 3:
                            {
                                s1 = $" and to_days(now()) - to_days(v.SigninDateTime) = 1 ";
                                s2 = $" and to_days(now()) - to_days(sb.CreatedOnUtc) = 1 ";
                            }
                            break;
                        //前天
                        case 4:
                            {
                                s1 = $" and to_days(now()) - to_days(v.SigninDateTime) = 2 ";
                                s2 = $" and to_days(now()) - to_days(sb.CreatedOnUtc) = 2 ";
                            }
                            break;
                        //上周
                        case 5:
                            {
                                s1 = $" and YEARWEEK(date_format(v.SigninDateTime,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                                s2 = $" and YEARWEEK(date_format(sb.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                            }
                            break;
                        //本周
                        case 6:
                            {
                                s1 = $" and YEARWEEK(date_format(v.SigninDateTime,'%Y-%m-%d')) = YEARWEEK(now()) ";
                                s2 = $" and YEARWEEK(date_format(sb.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                            }
                            break;
                        //上月
                        case 7:
                            {
                                s1 = $" and date_format(v.SigninDateTime,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                                s2 = $" and date_format(sb.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                            }
                            break;
                        //本月
                        case 8:
                            {
                                s1 = $" and date_format(v.SigninDateTime,'%Y-%m')=date_format(now(),'%Y-%m') ";
                                s2 = $" and date_format(sb.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                            }
                            break;
                        //本年
                        case 9:
                            {
                                s1 = $" and YEAR(v.SigninDateTime)=YEAR(NOW()) ";
                                s2 = $" and YEAR(sb.CreatedOnUtc)=YEAR(NOW()) ";
                            }
                            break;
                    }
                }

                string sqlString = $"SELECT u.UserRealName as UserName,(select count(1) from dcms.VisitStore as v where v.StoreId = {storeId} and v.BusinessUserId = u.id {s1} ) as VistCount,(select count(1) from dcms.SaleBills as sb where sb.StoreId = {storeId} and sb.BusinessUserId = u.id {s2} ) as SaleCount,(select count(1) from dcms.SaleReservationBills as sb where sb.StoreId = {storeId} and sb.BusinessUserId = u.id {s2} ) as OrderCount,(SELECT  COUNT(1) FROM dcms_crm.CRM_Terminals AS sb WHERE sb.StoreId = {storeId} AND sb.CreatedUserId = u.id {s2} ) AS NewAddCount FROM auth.User_UserRole_Mapping AS urm LEFT JOIN auth.Users AS u ON u.id = urm.UserId LEFT JOIN auth.UserRoles AS ur ON urm.UserRoleId = ur.id WHERE u.StoreId = {storeId} AND ur.SystemName = 'Salesmans' {userWhere} GROUP BY u.UserRealName, VistCount,SaleCount,OrderCount,NewAddCount";

                var analysis = SaleReservationBillsRepository_RO.QueryFromSql<BusinessAnalysisQuery>(sqlString).ToList();

                var result = new BusinessAnalysis();

                if (analysis != null && analysis.Any())
                {
                    result = new BusinessAnalysis()
                    {
                        UserNames = analysis.OrderBy(s => s.UserName).Select(s => s.UserName).ToList(),
                        VistCounts = analysis.OrderBy(s => s.UserName).Select(s => s.VistCount).ToList(),
                        SaleCounts = analysis.OrderBy(s => s.UserName).Select(s => s.SaleCount).ToList(),
                        NewAddCounts = analysis.OrderBy(s => s.UserName).Select(s => s.NewAddCount).ToList(),
                        OrderCounts = analysis.OrderBy(s => s.UserName).Select(s => s.OrderCount).ToList(),
                    };
                }

                return result;
            }
            catch (Exception)
            {
                return new BusinessAnalysis();
            }
        }

        public IList<SaleReportBusinessDaily> GetSaleReportSummaryBusinessDaily(int storeId, DateTime? start = null, DateTime? end = null, int userId = 0)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_BUSINESSDAILY_KEY.FillCacheKey(storeId, start, end, userId), () =>
                {
                    var strSql = @"SELECT a.CreatedOnUtc,a.BusinessUserId,a.SumAmount,a.SumCostAmount,a.PreferentialAmount,a.SumAmount-a.OweCash-a.PreferentialAmount ProceedsAmount,a.OweCash BalanceAmount,
                                    (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0)
                                     FROM SaleItems b INNER JOIN Products p on b.productId = p.Id WHERE b.SaleBillId=a.Id) as SaleQuantity,
                                     (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0)
                                     FROM SaleItems b INNER JOIN Products p on b.productId = p.Id WHERE b.SaleBillId=a.Id AND b.IsGifts=TRUE) as GiftQuantity,
                                      (SELECT IFNULL(SUM(b.CostAmount),0) FROM SaleItems b WHERE b.SaleBillId=a.Id AND b.IsGifts=TRUE) as GiftCostAmount,0 as ReturnQuantity,0 as ReturnAmount,a.Id,1 TypeId
                                    FROM SaleBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {3}
                                    UNION
                                    SELECT a.CreatedOnUtc,a.BusinessUserId,0 SumAmount,0 SumCostAmount,a.PreferentialAmount,-(a.SumAmount-a.OweCash-a.PreferentialAmount) ProceedsAmount,a.OweCash BalanceAmount,
                                    0 as SaleQuantity,0 as GiftQuantity,0 as GiftCostAmount,
                                    (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0) 
                                     FROM ReturnItems b INNER JOIN Products p on b.productId = p.Id WHERE b.ReturnBillId=a.Id) as ReturnQuantity,
                                    a.SumAmount as ReturnAmount,a.Id,2 TypeId
                                    FROM ReturnBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {3}
                                    UNION
                                    SELECT a.CreatedOnUtc,a.Payeer BusinessUserId,0 as SumAmount,0 as SumCostAmount,a.DiscountAmount PreferentialAmount,
                                    a.AdvanceAmount-a.OweCash-a.DiscountAmount as ProceedsAmount,a.OweCash BalanceAmount, 0 as SaleQuantity,0 as GiftQuantity,0 as GiftCostAmount,
                                    0 as ReturnQuantity,0 as ReturnAmount,a.Id,3 TypeId
                                    FROM AdvanceReceiptBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {4}";
                    var strWhere = "";
                    var strWhere1 = "";
                    if (userId>0) 
                    {
                        strWhere += $" AND a.BusinessUserId={userId}";
                        strWhere1 += $" AND a.Payeer={userId}";
                    }
                    strSql = string.Format(strSql, storeId, start, end, strWhere, strWhere1);
                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportBusinessDaily>(strSql).ToList();
                    return items;
                }, false);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IList<SaleReportBusinessDaily> GetSaleReportSummaryBusinessYearly(int storeId, int year, int userId = 0)
        {
            try
            {
                var start = Convert.ToDateTime($"{year}-01-01");
                var end = Convert.ToDateTime($"{year}-12-31 23:59:59");
                return _cacheManager.Get(DCMSDefaults.SALEREPORTSERVICE_GETSALE_REPORT_BUSINESSDAILY_KEY.FillCacheKey(storeId, start, end, userId), () =>
                {
                    var strSql = @"SELECT a.CreatedOnUtc,a.BusinessUserId,a.SumAmount,a.SumCostAmount,a.PreferentialAmount,a.SumAmount-a.OweCash-a.PreferentialAmount ProceedsAmount,a.OweCash BalanceAmount,
                                    (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0)
                                     FROM SaleItems b INNER JOIN Products p on b.productId = p.Id WHERE b.SaleBillId=a.Id) as SaleQuantity,
                                     (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0)
                                     FROM SaleItems b INNER JOIN Products p on b.productId = p.Id WHERE b.SaleBillId=a.Id AND b.IsGifts=TRUE) as GiftQuantity,
                                      (SELECT IFNULL(SUM(b.CostAmount),0) FROM SaleItems b WHERE b.SaleBillId=a.Id AND b.IsGifts=TRUE) as GiftCostAmount,0 as ReturnQuantity,0 as ReturnAmount,a.Id,1 TypeId
                                    FROM SaleBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {3}
                                    UNION
                                    SELECT a.CreatedOnUtc,a.BusinessUserId,0 SumAmount,0 SumCostAmount,a.PreferentialAmount,-(a.SumAmount-a.OweCash-a.PreferentialAmount) ProceedsAmount,a.OweCash BalanceAmount,
                                    0 as SaleQuantity,0 as GiftQuantity,0 as GiftCostAmount,
                                    (SELECT IFNULL(SUM(CASE WHEN b.UnitId=p.BigUnitId THEN b.Quantity*p.BigQuantity WHEN b.UnitId=p.StrokeUnitId THEN b.Quantity*p.StockQuantity ELSE b.Quantity END),0) 
                                     FROM ReturnItems b INNER JOIN Products p on b.productId = p.Id WHERE b.ReturnBillId=a.Id) as ReturnQuantity,
                                    a.SumAmount as ReturnAmount,a.Id,2 TypeId
                                    FROM ReturnBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {3}
                                    UNION
                                    SELECT a.CreatedOnUtc,a.Payeer BusinessUserId,0 as SumAmount,0 as SumCostAmount,a.DiscountAmount PreferentialAmount,
                                    a.AdvanceAmount-a.OweCash-a.DiscountAmount as ProceedsAmount,a.OweCash BalanceAmount, 0 as SaleQuantity,0 as GiftQuantity,0 as GiftCostAmount,
                                    0 as ReturnQuantity,0 as ReturnAmount,a.Id,3 TypeId
                                    FROM AdvanceReceiptBills a
                                    WHERE a.ReversedStatus=FALSE AND a.AuditedStatus=TRUE AND a.StoreId={0} AND a.CreatedOnUtc>='{1}' AND a.CreatedOnUtc<='{2}' {4}";
                    var strWhere = "";
                    var strWhere1 = "";
                    if (userId > 0)
                    {
                        strWhere += $" AND a.BusinessUserId={userId}";
                        strWhere1 += $" AND a.Payeer={userId}";
                    }
                    strSql = string.Format(strSql, storeId, start, end, strWhere, strWhere1);
                    var items = SaleBillsRepository_RO.QueryFromSql<SaleReportBusinessDaily>(strSql).ToList();
                    return items;
                }, false);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
