using DCMS.Core;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport.Help;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.Services.WareHouses;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DCMS.Services.ExportImport
{
    /// <summary>
    /// 导出管理器
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ITerminalService _terminalService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IAccountingService _accountingService;
        private readonly IStatisticalTypeService _statisticalTypeService;
        private readonly IBrandService _brandService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly ILineTierService _lineTierService;
        private readonly IRankService _rankService;


        #endregion

        #region Ctor

        public ExportManager(
            ICategoryService categoryService,
            IUserService userService,
            IGenericAttributeService genericAttributeService,
            IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
            IWareHouseService wareHouseService,
            ITerminalService terminalService,
            ISettingService settingService,
            IProductService productService,
            IAccountingService accountingService,
            IStatisticalTypeService statisticalTypeService,
            IBrandService brandService,
            IDistrictService districtService,
            IChannelService channelService,
            ILineTierService lineTierService,
            IRankService rankService,
            IWorkContext workContext)
        {
            _categoryService = categoryService;
            _userService = userService;
            _genericAttributeService = genericAttributeService;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
            _wareHouseService = wareHouseService;
            _terminalService = terminalService;
            _productService = productService;
            _settingService = settingService;
            _accountingService = accountingService;
            _statisticalTypeService = statisticalTypeService;
            _brandService = brandService;
            _districtService = districtService;
            _channelService = channelService;
            _lineTierService = lineTierService;
            _rankService = rankService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 商品类别
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="parentCategoryId"></param>
        protected virtual void WriteCategories(XmlWriter xmlWriter, int storeId, int userId, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(storeId, parentCategoryId, true);
            if (categories == null || !categories.Any())
            {
                return;
            }

            foreach (var category in categories)
            {
                xmlWriter.WriteStartElement("Category");
                xmlWriter.WriteString("Id", category.Id);
                xmlWriter.WriteString("Name", category.Name);
                //TODO...  在这里追加项目

                xmlWriter.WriteStartElement("Products");
                var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, userId, storeId, 0, int.MaxValue, showHidden: true);
                foreach (var productCategory in productCategories)
                {
                    var product = productCategory.Product;
                    if (product == null || product.Deleted)
                    {
                        continue;
                    }

                    xmlWriter.WriteStartElement("ProductCategory");
                    xmlWriter.WriteString("ProductCategoryId", productCategory.Id);
                    xmlWriter.WriteString("ProductId", productCategory.ProductId);
                    xmlWriter.WriteString("ProductName", product.Name);
                    xmlWriter.WriteString("DisplayOrder", productCategory.DisplayOrder);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("SubCategories");
                WriteCategories(xmlWriter, storeId, userId, category.Id);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
        }


        protected virtual string GetCategories(int storeId, int userId, DCMS.Core.Domain.Products.Product product)
        {
            string categoryNames = null;
            foreach (var pc in _categoryService.GetProductCategoriesByProductId(product.Id, userId, storeId, true))
            {
                categoryNames += pc.Category.Id.ToString();
                categoryNames += ";";
            }

            return categoryNames;
        }



        private PropertyManager<ExportProductAttribute> GetProductAttributeManager()
        {
            var attributeProperties = new[]
            {
                new PropertyByName<ExportProductAttribute>("AttributeId", p => p.AttributeId),
                new PropertyByName<ExportProductAttribute>("AttributeName", p => p.AttributeName),
            };

            return new PropertyManager<ExportProductAttribute>(attributeProperties);
        }

        private PropertyManager<ExportSpecificationAttribute> GetSpecificationAttributeManager(int storeId)
        {
            var attributeProperties = new[]
            {
                new PropertyByName<ExportSpecificationAttribute>("AttributeType", p => p.AttributeTypeId)
                {
                    DropDownElements = SpecificationAttributeType.Option.ToSelectList(useLocalization: false)
                },
                new PropertyByName<ExportSpecificationAttribute>("SpecificationAttribute", p => p.SpecificationAttributeId)
                {
                    DropDownElements = _specificationAttributeService.GetSpecificationAttributesBtStore(storeId).Select(sa => sa as BaseEntity).ToSelectList(p => (p as SpecificationAttribute)?.Name ?? string.Empty)
                },
                new PropertyByName<ExportSpecificationAttribute>("CustomValue", p => p.CustomValue),
                new PropertyByName<ExportSpecificationAttribute>("SpecificationAttributeOptionId", p => p.SpecificationAttributeOptionId),
                new PropertyByName<ExportSpecificationAttribute>("AllowFiltering", p => p.AllowFiltering),
                new PropertyByName<ExportSpecificationAttribute>("ShowOnProductPage", p => p.ShowOnProductPage),
                new PropertyByName<ExportSpecificationAttribute>("DisplayOrder", p => p.DisplayOrder)
            };

            return new PropertyManager<ExportSpecificationAttribute>(attributeProperties);
        }

        private byte[] ExportProductsToXlsxWithAttributes(int storeId, PropertyByName<DCMS.Core.Domain.Products.Product>[] properties, IEnumerable<DCMS.Core.Domain.Products.Product> itemsToExport)
        {
            var productAttributeManager = GetProductAttributeManager();
            var specificationAttributeManager = GetSpecificationAttributeManager(storeId);

            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handles to the worksheets
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(DCMS.Core.Domain.Products.Product).Name);
                    var fpWorksheet = xlPackage.Workbook.Worksheets.Add("DataForProductsFilters");
                    fpWorksheet.Hidden = eWorkSheetHidden.VeryHidden;
                    var fbaWorksheet = xlPackage.Workbook.Worksheets.Add("DataForProductAttributesFilters");
                    fbaWorksheet.Hidden = eWorkSheetHidden.VeryHidden;
                    var fsaWorksheet = xlPackage.Workbook.Worksheets.Add("DataForSpecificationAttributesFilters");
                    fsaWorksheet.Hidden = eWorkSheetHidden.VeryHidden;

                    //create Headers and format them 
                    var manager = new PropertyManager<DCMS.Core.Domain.Products.Product>(properties);
                    manager.WriteCaption(worksheet);

                    var row = 2;
                    foreach (var item in itemsToExport)
                    {
                        manager.CurrentObject = item;
                        manager.WriteToXlsx(worksheet, row++, fWorksheet: fpWorksheet);

                        //row = ExportProductAttributes(item, productAttributeManager, worksheet, row, fbaWorksheet);

                        //row = ExportSpecificationAttributes(item, specificationAttributeManager, worksheet, row, fsaWorksheet);
                    }

                    xlPackage.Save();
                }

                return stream.ToArray();
            }
        }


        #endregion

        #region Methods

        #region 销售

        #region 销售单据

        public byte[] ExportSaleReservationBillToXlsx(IList<SaleReservationBill> saleReservationBills, int store)
        {
            //默认付款账户动态列
            //var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.SaleReservationBill);

            var properties = new List<PropertyByName<SaleReservationBill>>()
           {
                new PropertyByName<SaleReservationBill>("编号", p => p.BillNumber),
                new PropertyByName<SaleReservationBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<SaleReservationBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<SaleReservationBill>("客户", p =>  _terminalService.GetTerminalById(0, p.TerminalId)?.Name),
                 new PropertyByName<SaleReservationBill>("客户编码", p => _terminalService.GetTerminalById(0, p.TerminalId)?.Code),
                new PropertyByName<SaleReservationBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<SaleReservationBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<SaleReservationBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<SaleReservationBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<SaleReservationBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<SaleReservationBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<SaleReservationBill>("备注", p => p.Remark),
                     new PropertyByName<SaleReservationBill>("打印数", p => p.PrintNum),
            };

            //if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            //{
            //    defaultAcc.Item4.ToList().ForEach(o =>
            //    {
            //        properties.Add(new PropertyByName<SaleReservationBill>(o.Value, p => p.SaleReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.SaleReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
            //    });
            //}

            return new PropertyManager<SaleReservationBill>(properties).ExportToXlsx(saleReservationBills);
        }

        public byte[] ExportReturnReservationBillToXlsx(IList<ReturnReservationBill> returnReservationBills, int store)
        {
            //默认付款账户动态列
            //var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.ReturnReservationBill);

            var properties = new List<PropertyByName<ReturnReservationBill>>()
           {
                new PropertyByName<ReturnReservationBill>("编号", p => p.BillNumber),
                new PropertyByName<ReturnReservationBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<ReturnReservationBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<ReturnReservationBill>("客户", p =>  _terminalService.GetTerminalById(0, p.TerminalId)?.Name),
                 new PropertyByName<ReturnReservationBill>("客户编码", p => _terminalService.GetTerminalById(0, p.TerminalId)?.Code),
                new PropertyByName<ReturnReservationBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<ReturnReservationBill>("总金额", p => p.SumAmount),
                new PropertyByName<ReturnReservationBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<ReturnReservationBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<ReturnReservationBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<ReturnReservationBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<ReturnReservationBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<ReturnReservationBill>("备注", p => p.Remark),
                     new PropertyByName<ReturnReservationBill>("打印数", p => p.PrintNum),
            };

            //if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            //{
            //    defaultAcc.Item4.ToList().ForEach(o =>
            //    {
            //        properties.Add(new PropertyByName<ReturnReservationBill>(o.Value, p => p.ReturnReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.ReturnReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
            //    });
            //}

            return new PropertyManager<ReturnReservationBill>(properties).ExportToXlsx(returnReservationBills);
        }

        public byte[] ExportSaleBillToXlsx(IList<SaleBill> saleBills, int store)
        {
            //默认付款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.SaleBill);

            var properties = new List<PropertyByName<SaleBill>>()
           {
                new PropertyByName<SaleBill>("编号", p => p.BillNumber),
                new PropertyByName<SaleBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<SaleBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<SaleBill>("客户", p =>  _terminalService.GetTerminalById(0, p.TerminalId)?.Name),
                 new PropertyByName<SaleBill>("客户编码", p => _terminalService.GetTerminalById(0, p.TerminalId)?.Code),
                new PropertyByName<SaleBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<SaleBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<SaleBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<SaleBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<SaleBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<SaleBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<SaleBill>("备注", p => p.Remark),
                     new PropertyByName<SaleBill>("打印数", p => p.PrintNum),
            };


            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<SaleBill>(o.Value, p => p.SaleBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.SaleBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<SaleBill>(properties).ExportToXlsx(saleBills);
        }

        public byte[] ExportReturnBillToXlsx(IList<ReturnBill> returnBills, int store)
        {
            //默认付款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.ReturnBill);

            var properties = new List<PropertyByName<ReturnBill>>()
           {
                new PropertyByName<ReturnBill>("编号", p => p.BillNumber),
                new PropertyByName<ReturnBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<ReturnBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<ReturnBill>("客户", p =>  _terminalService.GetTerminalById(0, p.TerminalId)?.Name),
                 new PropertyByName<ReturnBill>("客户编码", p => _terminalService.GetTerminalById(0, p.TerminalId)?.Code),
                new PropertyByName<ReturnBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<ReturnBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<ReturnBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<ReturnBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<ReturnBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<ReturnBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<ReturnBill>("备注", p => p.Remark),
                     new PropertyByName<ReturnBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<ReturnBill>(o.Value, p => p.ReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.ReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<ReturnBill>(properties).ExportToXlsx(returnBills);
        }

        #endregion

        #region 销售报表
        public byte[] ExportSaleReportItemToXlsx(IList<SaleReportItem> saleReportItems)
        {
            var properties = new List<PropertyByName<SaleReportItem>>()
           {
                new PropertyByName<SaleReportItem>("单据编号", p => p.BillNumber),
                new PropertyByName<SaleReportItem>("单据类型", p => p.BillTypeName),
                new PropertyByName<SaleReportItem>("客户", p =>  p.TerminalName),
                new PropertyByName<SaleReportItem>("客户编码", p =>  p.TerminalCode),
                new PropertyByName<SaleReportItem>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<SaleReportItem>("送货员", p =>  p.DeliveryUserName),
                new PropertyByName<SaleReportItem>("交易时间", p => p.TransactionDate.ToString()),
                 new PropertyByName<SaleReportItem>("审核时间", p => p.AuditedDate.ToString()),
                  new PropertyByName<SaleReportItem>("仓库", p => p.WareHouseName),
                   new PropertyByName<SaleReportItem>("商品编号", p =>p.ProductSKU),
                    new PropertyByName<SaleReportItem>("商品名称", p => p.ProductName),
                     new PropertyByName<SaleReportItem>("条形码", p => p.BarCode),
                     new PropertyByName<SaleReportItem>("单位换算", p => p.UnitConversion),
                     new PropertyByName<SaleReportItem>("数量", p => p.Quantity??0),
                     new PropertyByName<SaleReportItem>("单位", p => p.UnitName),
                     new PropertyByName<SaleReportItem>("单价", p => p.Price??0),
                     new PropertyByName<SaleReportItem>("金额", p => p.Amount??0),
                     new PropertyByName<SaleReportItem>("成本金额", p => p.CostAmount??0),
                     new PropertyByName<SaleReportItem>("利润", p => p.Profit??0),
                     new PropertyByName<SaleReportItem>("备注", p => p.Remark),
            };

            return new PropertyManager<SaleReportItem>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryProductToXlsx(IList<SaleReportSummaryProduct> saleReportItems)
        {
            var properties = new List<PropertyByName<SaleReportSummaryProduct>>()
           {
                new PropertyByName<SaleReportSummaryProduct>("商品编号", p => p.ProductCode),
                new PropertyByName<SaleReportSummaryProduct>("商品名称", p => p.ProductName),
                new PropertyByName<SaleReportSummaryProduct>("条形码", p =>  p.SmallBarCode),
                new PropertyByName<SaleReportSummaryProduct>("销售数量", p =>  p.SaleQuantityConversion),
                new PropertyByName<SaleReportSummaryProduct>("销售金额", p =>  p.SaleAmount),
                new PropertyByName<SaleReportSummaryProduct>("赠送数量", p =>  p.GiftQuantityConversion),
                new PropertyByName<SaleReportSummaryProduct>("退货数量", p => p.ReturnQuantityConversion),
                 new PropertyByName<SaleReportSummaryProduct>("退货金额", p => p.ReturnAmount),
                  new PropertyByName<SaleReportSummaryProduct>("净销售量", p => p.NetQuantityConversion),
                   new PropertyByName<SaleReportSummaryProduct>("净销售额", p =>p.NetAmount),
                    new PropertyByName<SaleReportSummaryProduct>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportSummaryProduct>("利润", p => p.Profit),
                     new PropertyByName<SaleReportSummaryProduct>("成本利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),
            };

            return new PropertyManager<SaleReportSummaryProduct>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryCustomerToXlsx(IList<SaleReportSummaryCustomer> saleReportItems, int store)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store);

            var properties = new List<PropertyByName<SaleReportSummaryCustomer>>()
           {
                new PropertyByName<SaleReportSummaryCustomer>("客户", p => p.TerminalId),
                new PropertyByName<SaleReportSummaryCustomer>("客户编码", p => p.TerminalCode),
            };

            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                statisticalTypes.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<SaleReportSummaryCustomer>(o.Name + "净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetSmallQuantity ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryCustomer>(o.Name + "销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryCustomer>(o.Name + "成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryCustomer>(o.Name + "利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).Profit ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryCustomer>(o.Name + "成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostProfitRate ?? 0));
                });
            }

            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("其他净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetSmallQuantity ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("其他销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("其他成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("其他利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).Profit ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("其他成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostProfitRate ?? 0));


            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("销售数量", p => p.SaleSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("退货数量", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("赠送数量", p => p.GiftQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("销售金额", p => p.SaleAmount));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("退货金额", p => p.ReturnAmount));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("销售净额", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("优惠", p => p.DiscountAmount));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("成本", p => p.CostAmount));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("利润", p => p.Profit));
            properties.Add(new PropertyByName<SaleReportSummaryCustomer>("成本利润率", p => p.CostProfitRate));

            return new PropertyManager<SaleReportSummaryCustomer>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryBusinessUserToXlsx(IList<SaleReportSummaryBusinessUser> saleReportItems, int store)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store);

            var properties = new List<PropertyByName<SaleReportSummaryBusinessUser>>()
           {
                new PropertyByName<SaleReportSummaryBusinessUser>("业务员", p => p.BusinessUserName),
            };

            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                statisticalTypes.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>(o.Name + "净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetSmallQuantity ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>(o.Name + "销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>(o.Name + "成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>(o.Name + "利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).Profit ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>(o.Name + "成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostProfitRate ?? 0));
                });
            }

            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("其他净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetSmallQuantity ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("其他销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("其他成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("其他利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).Profit ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("其他成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostProfitRate ?? 0));


            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("销售数量", p => p.SaleSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("退货数量", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("赠送数量", p => p.GiftQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("销售金额", p => p.SaleAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("退货金额", p => p.ReturnAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("销售净额", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("优惠", p => p.DiscountAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("成本", p => p.CostAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("利润", p => p.Profit));
            properties.Add(new PropertyByName<SaleReportSummaryBusinessUser>("成本利润率", p => p.CostProfitRate));

            return new PropertyManager<SaleReportSummaryBusinessUser>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryCustomerProductToXlsx(IList<SaleReportSummaryCustomerProduct> saleReportItems)
        {
            var properties = new List<PropertyByName<SaleReportSummaryCustomerProduct>>()
           {
                new PropertyByName<SaleReportSummaryCustomerProduct>("客户名称", p => p.TerminalName),
                new PropertyByName<SaleReportSummaryCustomerProduct>("商品名称", p => p.ProductName),
                new PropertyByName<SaleReportSummaryCustomerProduct>("销售数量", p =>  p.SaleQuantityConversion),
                new PropertyByName<SaleReportSummaryCustomerProduct>("销售金额", p =>  p.SaleAmount),
                new PropertyByName<SaleReportSummaryCustomerProduct>("退货数量", p => p.ReturnQuantityConversion),
                new PropertyByName<SaleReportSummaryCustomerProduct>("退货金额", p => p.ReturnAmount),
                 new PropertyByName<SaleReportSummaryCustomerProduct>("还货数量", p => p.RepaymentQuantityConversion),
                 new PropertyByName<SaleReportSummaryCustomerProduct>("还货金额", p => p.RepaymentAmount),
                  new PropertyByName<SaleReportSummaryCustomerProduct>("总数量", p => p.SumQuantityConversion),
                   new PropertyByName<SaleReportSummaryCustomerProduct>("总金额", p =>p.SumAmount),
                    new PropertyByName<SaleReportSummaryCustomerProduct>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportSummaryCustomerProduct>("利润", p => p.Profit),
                     new PropertyByName<SaleReportSummaryCustomerProduct>("利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),
            };

            return new PropertyManager<SaleReportSummaryCustomerProduct>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryWareHouseToXlsx(IList<SaleReportSummaryWareHouse> saleReportItems, int store)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store);

            var properties = new List<PropertyByName<SaleReportSummaryWareHouse>>()
           {
                new PropertyByName<SaleReportSummaryWareHouse>("仓库", p => p.WareHouseName),
            };

            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                statisticalTypes.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<SaleReportSummaryWareHouse>(o.Name + "净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetSmallQuantity ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryWareHouse>(o.Name + "销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryWareHouse>(o.Name + "成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryWareHouse>(o.Name + "利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).Profit ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryWareHouse>(o.Name + "成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostProfitRate ?? 0));
                });
            }

            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("其他净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetSmallQuantity ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("其他销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("其他成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("其他利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).Profit ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("其他成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostProfitRate ?? 0));


            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("销售数量", p => p.SaleSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("退货数量", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("销售金额", p => p.SaleAmount));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("退货金额", p => p.ReturnAmount));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("销售净额", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("优惠", p => p.DiscountAmount));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("成本", p => p.CostAmount));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("利润", p => p.Profit));
            properties.Add(new PropertyByName<SaleReportSummaryWareHouse>("成本利润率", p => p.CostProfitRate));

            return new PropertyManager<SaleReportSummaryWareHouse>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportSummaryBrandToXlsx(IList<SaleReportSummaryBrand> saleReportItems, int store)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store);

            var properties = new List<PropertyByName<SaleReportSummaryBrand>>()
           {
                new PropertyByName<SaleReportSummaryBrand>("品牌", p => p.BrandName),
            };

            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                statisticalTypes.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<SaleReportSummaryBrand>(o.Name + "净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetSmallQuantity ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBrand>(o.Name + "销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).NetAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBrand>(o.Name + "成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostAmount ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBrand>(o.Name + "利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).Profit ?? 0));
                    properties.Add(new PropertyByName<SaleReportSummaryBrand>(o.Name + "成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).CostProfitRate ?? 0));
                });
            }

            properties.Add(new PropertyByName<SaleReportSummaryBrand>("其他净销售量", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetSmallQuantity ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("其他销售净额", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).NetAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("其他成本", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostAmount ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("其他利润", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).Profit ?? 0));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("其他成本利润率", p => p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.SaleReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).CostProfitRate ?? 0));


            properties.Add(new PropertyByName<SaleReportSummaryBrand>("销售数量", p => p.SaleSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("退货数量", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("销售金额", p => p.SaleAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("退货金额", p => p.ReturnAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("销售净额", p => p.ReturnSmallQuantity));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("优惠", p => p.DiscountAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("成本", p => p.CostAmount));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("利润", p => p.Profit));
            properties.Add(new PropertyByName<SaleReportSummaryBrand>("成本利润率", p => p.CostProfitRate));

            return new PropertyManager<SaleReportSummaryBrand>(properties).ExportToXlsx(saleReportItems);
        }
        public byte[] ExportSaleReportOrderItemToXlsx(IList<SaleReportOrderItem> saleReportOrderItems)
        {
            var properties = new List<PropertyByName<SaleReportOrderItem>>()
           {
                new PropertyByName<SaleReportOrderItem>("单据编号", p => p.ReservationBillNumber),
                new PropertyByName<SaleReportOrderItem>("单据类型", p => p.BillTypeName),
                new PropertyByName<SaleReportOrderItem>("客户", p =>  p.TerminalName),
                new PropertyByName<SaleReportOrderItem>("客户编码", p =>  p.TerminalCode),
                new PropertyByName<SaleReportOrderItem>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<SaleReportOrderItem>("交易时间", p =>  p.TransactionDate),
                new PropertyByName<SaleReportOrderItem>("审核时间", p => p.AuditedDate),
                 new PropertyByName<SaleReportOrderItem>("仓库", p => p.WareHouseName),
                  new PropertyByName<SaleReportOrderItem>("商品编号", p => p.ProductSKU),
                   new PropertyByName<SaleReportOrderItem>("商品名称", p =>p.ProductName),
                    new PropertyByName<SaleReportOrderItem>("条形码", p => p.BarCode),
                     new PropertyByName<SaleReportOrderItem>("单位换算", p => p.UnitConversion),
                     new PropertyByName<SaleReportOrderItem>("数量", p => p.Quantity),
                     new PropertyByName<SaleReportOrderItem>("单位", p => p.UnitName),
                     new PropertyByName<SaleReportOrderItem>("金额", p => p.Amount),
                     new PropertyByName<SaleReportOrderItem>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportOrderItem>("利润", p => p.Profit),
                     new PropertyByName<SaleReportOrderItem>("成本利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),
                     new PropertyByName<SaleReportOrderItem>("备注", p => p.Remark),

            };

            return new PropertyManager<SaleReportOrderItem>(properties).ExportToXlsx(saleReportOrderItems);
        }
        public byte[] ExportSaleReportSummaryOrderProductToXlsx(IList<SaleReportSummaryOrderProduct> saleReportOrderItems)
        {
            var properties = new List<PropertyByName<SaleReportSummaryOrderProduct>>()
           {
                new PropertyByName<SaleReportSummaryOrderProduct>("商品编号", p => p.ProductCode),
                new PropertyByName<SaleReportSummaryOrderProduct>("商品名称", p => p.ProductName),
                new PropertyByName<SaleReportSummaryOrderProduct>("条形码", p =>  p.SmallBarCode),
                new PropertyByName<SaleReportSummaryOrderProduct>("订单数量", p =>  p.SaleQuantityConversion),
                new PropertyByName<SaleReportSummaryOrderProduct>("订单金额", p =>  p.SaleAmount),
                new PropertyByName<SaleReportSummaryOrderProduct>("赠送数量", p =>  p.GiftQuantityConversion),
                new PropertyByName<SaleReportSummaryOrderProduct>("退货数量", p => p.ReturnQuantityConversion),
                 new PropertyByName<SaleReportSummaryOrderProduct>("退货金额", p => p.ReturnAmount),
                  new PropertyByName<SaleReportSummaryOrderProduct>("净销售量", p => p.NetQuantityConversion),
                   new PropertyByName<SaleReportSummaryOrderProduct>("销售净额", p =>p.NetAmount),
                     new PropertyByName<SaleReportSummaryOrderProduct>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportSummaryOrderProduct>("利润", p => p.Profit),
                     new PropertyByName<SaleReportSummaryOrderProduct>("成本利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),

            };

            return new PropertyManager<SaleReportSummaryOrderProduct>(properties).ExportToXlsx(saleReportOrderItems);
        }
        public byte[] ExportSaleReportCostContractItemToXlsx(IList<SaleReportCostContractItem> saleReportCostContractItems)
        {
            var properties = new List<PropertyByName<SaleReportCostContractItem>>()
           {
                new PropertyByName<SaleReportCostContractItem>("单据编号", p => p.BillNumber),
                new PropertyByName<SaleReportCostContractItem>("客户", p => p.TerminalName),
                new PropertyByName<SaleReportCostContractItem>("客户编码", p =>  p.TerminalCode),
                new PropertyByName<SaleReportCostContractItem>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<SaleReportCostContractItem>("交易时间", p =>  p.TransactionDate),
                new PropertyByName<SaleReportCostContractItem>("审核时间", p =>  p.AuditedDate),
                new PropertyByName<SaleReportCostContractItem>("商品/现金", p => p.ProductName),
                 new PropertyByName<SaleReportCostContractItem>("条形码", p => p.BarCode),
                  new PropertyByName<SaleReportCostContractItem>("单位换算", p => p.UnitConversion),
                   new PropertyByName<SaleReportCostContractItem>("单位", p =>p.UnitName),
                     new PropertyByName<SaleReportCostContractItem>("一月", p => p.Jan),
                     new PropertyByName<SaleReportCostContractItem>("二月", p => p.Feb),
                     new PropertyByName<SaleReportCostContractItem>("三月", p => p.Mar),
                     new PropertyByName<SaleReportCostContractItem>("四月", p => p.Apr),
                     new PropertyByName<SaleReportCostContractItem>("五月", p => p.May),
                     new PropertyByName<SaleReportCostContractItem>("六月", p => p.Jun),
                     new PropertyByName<SaleReportCostContractItem>("七月", p => p.Jul),
                     new PropertyByName<SaleReportCostContractItem>("八月", p => p.Aug),
                     new PropertyByName<SaleReportCostContractItem>("九月", p => p.Sep),
                     new PropertyByName<SaleReportCostContractItem>("十月", p => p.Oct),
                     new PropertyByName<SaleReportCostContractItem>("十一月", p => p.Nov),
                     new PropertyByName<SaleReportCostContractItem>("十二月", p => p.Dec),
                     new PropertyByName<SaleReportCostContractItem>("总计", p => p.Total),
                     new PropertyByName<SaleReportCostContractItem>("状态", p => p.Status),
                     new PropertyByName<SaleReportCostContractItem>("备注", p => p.Remark),

            };

            return new PropertyManager<SaleReportCostContractItem>(properties).ExportToXlsx(saleReportCostContractItems);
        }
        public byte[] ExportSaleReportSummaryGiveQuotaToXlsx(IList<GiveQuotaRecordsSummery> saleReportOrderItems)
        {
            var properties = new List<PropertyByName<GiveQuotaRecordsSummery>>()
           {
                new PropertyByName<GiveQuotaRecordsSummery>("客户名称", p => p.TerminalName),
                new PropertyByName<GiveQuotaRecordsSummery>("客户编码", p => p.TerminalCode),
                new PropertyByName<GiveQuotaRecordsSummery>("商品名称", p =>  p.ProductName),
                new PropertyByName<GiveQuotaRecordsSummery>("条形码", p =>  p.BarCode),
                new PropertyByName<GiveQuotaRecordsSummery>("单位换算", p =>  p.UnitConversion),
                new PropertyByName<GiveQuotaRecordsSummery>("普通赠品（数量）", p => p.GeneralQuantity),
                 new PropertyByName<GiveQuotaRecordsSummery>("普通赠品（成本）", p => Math.Round(p.GeneralCostAmount, 2)),
                  new PropertyByName<GiveQuotaRecordsSummery>("订货赠品（数量）", p => p.OrderQuantity),
                   new PropertyByName<GiveQuotaRecordsSummery>("订货赠品（成本）", p =>Math.Round(p.OrderCostAmount, 2)),
                     new PropertyByName<GiveQuotaRecordsSummery>("促销赠品（数量）", p => p.PromotionalQuantity),
                     new PropertyByName<GiveQuotaRecordsSummery>("促销赠品（成本）", p => Math.Round(p.PromotionalCostAmount, 2)),
                     new PropertyByName<GiveQuotaRecordsSummery>("费用合同（数量）", p => p.ContractQuantity),
                     new PropertyByName<GiveQuotaRecordsSummery>("费用合同（成本）", p => Math.Round(p.ContractCostAmount, 2)),

            };

            return new PropertyManager<GiveQuotaRecordsSummery>(properties).ExportToXlsx(saleReportOrderItems);
        }
        public byte[] ExportSaleReportHotSaleToXlsx(IList<SaleReportHotSale> saleReportOrderItems)
        {
            var properties = new List<PropertyByName<SaleReportHotSale>>()
           {
                new PropertyByName<SaleReportHotSale>("商品编号", p => p.ProductCode),
                new PropertyByName<SaleReportHotSale>("商品名称", p => p.ProductName),
                new PropertyByName<SaleReportHotSale>("销售数量", p =>  p.SmallBarCode),
                new PropertyByName<SaleReportHotSale>("销售金额", p =>  p.SaleQuantityConversion),
                new PropertyByName<SaleReportHotSale>("退货数量", p =>  p.ReturnQuantityConversion),
                new PropertyByName<SaleReportHotSale>("退货金额", p =>  p.ReturnAmount),
                new PropertyByName<SaleReportHotSale>("净销售量", p => p.ReturnQuantityConversion),
                 new PropertyByName<SaleReportHotSale>("销售净额", p => p.ReturnAmount),
                     new PropertyByName<SaleReportHotSale>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportHotSale>("利润", p => p.Profit),
                     new PropertyByName<SaleReportHotSale>("成本利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),

            };

            return new PropertyManager<SaleReportHotSale>(properties).ExportToXlsx(saleReportOrderItems);
        }
        public byte[] ExportSaleReportProductCostProfitToXlsx(IList<SaleReportProductCostProfit> saleReportProductCostProfits)
        {
            var properties = new List<PropertyByName<SaleReportProductCostProfit>>()
           {
                new PropertyByName<SaleReportProductCostProfit>("商品编号", p => p.ProductCode),
                new PropertyByName<SaleReportProductCostProfit>("商品名称", p => p.ProductName),
                new PropertyByName<SaleReportProductCostProfit>("销售数量", p =>  p.SmallBarCode),
                new PropertyByName<SaleReportProductCostProfit>("销售金额", p =>  p.SaleQuantityConversion),
                new PropertyByName<SaleReportProductCostProfit>("退货数量", p =>  p.ReturnQuantityConversion),
                new PropertyByName<SaleReportProductCostProfit>("退货金额", p =>  p.ReturnAmount),
                new PropertyByName<SaleReportProductCostProfit>("净销售量", p => p.ReturnQuantityConversion),
                 new PropertyByName<SaleReportProductCostProfit>("销售净额", p => p.ReturnAmount),
                     new PropertyByName<SaleReportProductCostProfit>("成本金额", p => p.CostAmount),
                     new PropertyByName<SaleReportProductCostProfit>("利润", p => p.Profit),
                     new PropertyByName<SaleReportProductCostProfit>("销售利润率", p => p.SaleProfitRate),
                     new PropertyByName<SaleReportProductCostProfit>("成本利润率", p => Math.Round((p.CostProfitRate ?? 0) * 100, 2) + "%"),

            };

            return new PropertyManager<SaleReportProductCostProfit>(properties).ExportToXlsx(saleReportProductCostProfits);
        }
        #endregion

        #endregion

        #region 采购

        #region 采购单据
        public byte[] ExportPurchaseBillToXlsx(IList<PurchaseBill> purchaseBills, int store)
        {
            //默认付款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.PurchaseBill);

            var properties = new List<PropertyByName<PurchaseBill>>()
           {
                new PropertyByName<PurchaseBill>("编号", p => p.BillNumber),
                new PropertyByName<PurchaseBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<PurchaseBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<PurchaseBill>("供应商", p => _manufacturerService.GetManufacturerById(store, p.ManufacturerId)?.Name),
                new PropertyByName<PurchaseBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<PurchaseBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<PurchaseBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<PurchaseBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<PurchaseBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<PurchaseBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<PurchaseBill>("备注", p => p.Remark),
                     new PropertyByName<PurchaseBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<PurchaseBill>(o.Value, p => p.PurchaseBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.PurchaseBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<PurchaseBill>(properties).ExportToXlsx(purchaseBills);
        }

        public byte[] ExportPurchaseReturnBillToXlsx(IList<PurchaseReturnBill> purchaseRetuenBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.PurchaseReturnBill);

            var properties = new List<PropertyByName<PurchaseReturnBill>>()
           {
                new PropertyByName<PurchaseReturnBill>("编号", p => p.BillNumber),
                new PropertyByName<PurchaseReturnBill>("交易时间", p => (p.TransactionDate==null)? "" : ((DateTime)p.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<PurchaseReturnBill>("业务员", p =>  _userService.GetUserName(store, p.BusinessUserId)),
                new PropertyByName<PurchaseReturnBill>("供应商", p => _manufacturerService.GetManufacturerById(store, p.ManufacturerId)?.Name),
                new PropertyByName<PurchaseReturnBill>("仓库", p => _wareHouseService.GetWareHouseName(store, p.WareHouseId)),
                new PropertyByName<PurchaseReturnBill>("应收金额", p => p.ReceivableAmount),
                new PropertyByName<PurchaseReturnBill>("优惠金额", p => p.PreferentialAmount),
                 new PropertyByName<PurchaseReturnBill>("欠款金额", p => p.OweCash),
                  new PropertyByName<PurchaseReturnBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<PurchaseReturnBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<PurchaseReturnBill>("备注", p => p.Remark),
                     new PropertyByName<PurchaseReturnBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<PurchaseReturnBill>(o.Value, p => p.PurchaseReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.PurchaseReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<PurchaseReturnBill>(properties).ExportToXlsx(purchaseRetuenBills);
        }
        #endregion

        #region 采购报表
        public byte[] ExportPurchaseReportItemToXlsx(IList<PurchaseReportItem> purchaseReportItems)
        {
            var properties = new List<PropertyByName<PurchaseReportItem>>()
           {
                new PropertyByName<PurchaseReportItem>("单据编号", p => p.BillNumber),
                new PropertyByName<PurchaseReportItem>("单据类型", p => p.BillTypeName),
                new PropertyByName<PurchaseReportItem>("供应商", p =>  p.ManufacturerName),
                new PropertyByName<PurchaseReportItem>("交易时间", p => p.TransactionDate.ToString()),
                 new PropertyByName<PurchaseReportItem>("审核时间", p => p.AuditedDate.ToString()),
                  new PropertyByName<PurchaseReportItem>("仓库", p => p.WareHouseName),
                   new PropertyByName<PurchaseReportItem>("商品编号", p =>p.ProductSKU),
                    new PropertyByName<PurchaseReportItem>("商品名称", p => p.ProductName),
                     new PropertyByName<PurchaseReportItem>("条形码", p => p.BarCode),
                     new PropertyByName<PurchaseReportItem>("单位换算", p => p.UnitConversion),
                     new PropertyByName<PurchaseReportItem>("数量", p => p.QuantityConversion),
                     new PropertyByName<PurchaseReportItem>("单价", p => p.Price),
                     new PropertyByName<PurchaseReportItem>("金额", p => p.Amount),
                     new PropertyByName<PurchaseReportItem>("备注", p => p.Remark),
            };

            return new PropertyManager<PurchaseReportItem>(properties).ExportToXlsx(purchaseReportItems);
        }
        public byte[] ExportPurchaseReportSummaryProductToXlsx(IList<PurchaseReportSummaryProduct> purchaseReportItems)
        {
            var properties = new List<PropertyByName<PurchaseReportSummaryProduct>>()
           {
                new PropertyByName<PurchaseReportSummaryProduct>("商品编号", p => p.ProductCode),
                new PropertyByName<PurchaseReportSummaryProduct>("商品名称", p => p.ProductName),
                new PropertyByName<PurchaseReportSummaryProduct>("条形码", p =>  p.SmallBarCode),
                new PropertyByName<PurchaseReportSummaryProduct>("采购数量", p => p.PurchaseQuantityConversion),
                 new PropertyByName<PurchaseReportSummaryProduct>("采购金额", p => p.PurchaseAmount),
                  new PropertyByName<PurchaseReportSummaryProduct>("赠送数量", p => p.GiftQuantityConversion),
                   new PropertyByName<PurchaseReportSummaryProduct>("退购数量", p =>p.PurchaseReturnQuantityConversion),
                    new PropertyByName<PurchaseReportSummaryProduct>("退购金额", p => p.PurchaseReturnAmount),
                     new PropertyByName<PurchaseReportSummaryProduct>("数量小计", p => p.SumQuantityConversion),
                     new PropertyByName<PurchaseReportSummaryProduct>("金额小计", p => p.SumAmount),
            };

            return new PropertyManager<PurchaseReportSummaryProduct>(properties).ExportToXlsx(purchaseReportItems);
        }
        public byte[] ExportPurchaseReportSummaryManufacturerToXlsx(IList<PurchaseReportSummaryManufacturer> purchaseReportItems, int store)
        {
            //商品统计类别动态列
            var statisticalTypes = _statisticalTypeService.GetAllStatisticalTypess(store);

            var properties = new List<PropertyByName<PurchaseReportSummaryManufacturer>>()
           {
                new PropertyByName<PurchaseReportSummaryManufacturer>("供应商", p => p.ManufacturerName),
                new PropertyByName<PurchaseReportSummaryManufacturer>("采购数量", p => p.SumPurchaseSmallUnitConversion),
                new PropertyByName<PurchaseReportSummaryManufacturer>("采购金额", p =>  p.SumOrderAmount),
            };

            if (statisticalTypes != null && statisticalTypes.Count > 0)
            {
                statisticalTypes.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<PurchaseReportSummaryManufacturer>(o.Name + "数量", p => p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? "" : p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).SumPurchaseSmallUnitConversion));
                    properties.Add(new PropertyByName<PurchaseReportSummaryManufacturer>(o.Name + "金额", p => p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id) == null ? 0 : p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == o.Id).OrderAmount ?? 0));
                });
            }

            properties.Add(new PropertyByName<PurchaseReportSummaryManufacturer>("其他数量", p => p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? "" : p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).SumPurchaseSmallUnitConversion));
            properties.Add(new PropertyByName<PurchaseReportSummaryManufacturer>("其他金额", p => p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId) == null ? 0 : p.PurchaseReportStatisticalTypes.FirstOrDefault(sao => sao.StatisticalTypeId == (int)StatisticalTypeEnum.OtherTypeId).OrderAmount ?? 0));
            return new PropertyManager<PurchaseReportSummaryManufacturer>(properties).ExportToXlsx(purchaseReportItems);
        }
        #endregion

        #endregion

        #region 仓库

        #region 仓库单据
        public byte[] ExportAllocationBillToXlsx(IList<AllocationBill> allocationBills)
        {
            var properties = new List<PropertyByName<AllocationBill>>()
           {
                new PropertyByName<AllocationBill>("调拨单编号", p => p.BillNumber),
                new PropertyByName<AllocationBill>("调拨时间", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<AllocationBill>("出货仓库", p => _wareHouseService.GetWareHouseName(0, p.ShipmentWareHouseId)),
                new PropertyByName<AllocationBill>("入货仓库", p => _wareHouseService.GetWareHouseName(0, p.IncomeWareHouseId)),
                  new PropertyByName<AllocationBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<AllocationBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<AllocationBill>("备注", p => p.Remark),
                     new PropertyByName<AllocationBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<AllocationBill>(properties).ExportToXlsx(allocationBills);
        }
        public byte[] ExportInventoryProfitLossBillToXlsx(IList<InventoryProfitLossBill> inventoryProfitLossBills)
        {
            var properties = new List<PropertyByName<InventoryProfitLossBill>>()
           {
                new PropertyByName<InventoryProfitLossBill>("单据编号", p => p.BillNumber),
                new PropertyByName<InventoryProfitLossBill>("盘点时间", p => (p.InventoryDate==null)? "" : p.InventoryDate.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<InventoryProfitLossBill>("操作员", p => _userService.GetUserName(p.StoreId, p.ChargePerson)),
                new PropertyByName<InventoryProfitLossBill>("仓库", p => _wareHouseService.GetWareHouseName(0, p.WareHouseId)),
                  new PropertyByName<InventoryProfitLossBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<InventoryProfitLossBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<InventoryProfitLossBill>("备注", p => p.Remark),
                     new PropertyByName<InventoryProfitLossBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<InventoryProfitLossBill>(properties).ExportToXlsx(inventoryProfitLossBills);
        }
        public byte[] ExportCostAdjustmentBillToXlsx(IList<CostAdjustmentBill> costAdjustmentBills)
        {
            var properties = new List<PropertyByName<CostAdjustmentBill>>()
           {
                new PropertyByName<CostAdjustmentBill>("单据编号", p => p.BillNumber),
                new PropertyByName<CostAdjustmentBill>("调价时间", p => (p.AdjustmentDate==null)? "" : p.AdjustmentDate.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<CostAdjustmentBill>("操作员", p => _userService.GetUserName(p.StoreId, p.MakeUserId)),
                  new PropertyByName<CostAdjustmentBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<CostAdjustmentBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<CostAdjustmentBill>("备注", p => p.Remark),
                     new PropertyByName<CostAdjustmentBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<CostAdjustmentBill>(properties).ExportToXlsx(costAdjustmentBills);
        }
        public byte[] ExportScrapProductBillToXlsx(IList<ScrapProductBill> scrapProductBills)
        {
            var properties = new List<PropertyByName<ScrapProductBill>>()
           {
                new PropertyByName<ScrapProductBill>("单据编号", p => p.BillNumber),
                new PropertyByName<ScrapProductBill>("制单时间", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<ScrapProductBill>("操作员", p => _userService.GetUserName(p.StoreId, p.ChargePerson)),
                new PropertyByName<ScrapProductBill>("仓库", p => _wareHouseService.GetWareHouseName(0, p.WareHouseId)),
                  new PropertyByName<ScrapProductBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<ScrapProductBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<ScrapProductBill>("备注", p => p.Remark),
                     new PropertyByName<ScrapProductBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<ScrapProductBill>(properties).ExportToXlsx(scrapProductBills);
        }
        public byte[] ExportInventoryAllTaskBillToXlsx(IList<InventoryAllTaskBill> inventoryAllTaskBills)
        {
            var properties = new List<PropertyByName<InventoryAllTaskBill>>()
           {
                new PropertyByName<InventoryAllTaskBill>("单据编号", p => p.BillNumber),
                new PropertyByName<InventoryAllTaskBill>("盘点时间", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<InventoryAllTaskBill>("操作员", p => _userService.GetUserName(p.StoreId, p.InventoryPerson)),
                new PropertyByName<InventoryAllTaskBill>("仓库", p => _wareHouseService.GetWareHouseName(0, p.WareHouseId)),
                  new PropertyByName<InventoryAllTaskBill>("状态", p => (p.InventoryStatus == 1) ? "进行中" : (p.InventoryStatus==2)? "已结束" : "已取消"),
                   new PropertyByName<InventoryAllTaskBill>("完成时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                     new PropertyByName<InventoryAllTaskBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<InventoryAllTaskBill>(properties).ExportToXlsx(inventoryAllTaskBills);
        }
        public byte[] ExportInventoryPartTaskBillToXlsx(IList<InventoryPartTaskBill> inventoryPartTaskBills)
        {
            var properties = new List<PropertyByName<InventoryPartTaskBill>>()
           {
                new PropertyByName<InventoryPartTaskBill>("单据编号", p => p.BillNumber),
                new PropertyByName<InventoryPartTaskBill>("盘点时间", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<InventoryPartTaskBill>("操作员", p => _userService.GetUserName(p.StoreId, p.InventoryPerson)),
                new PropertyByName<InventoryPartTaskBill>("仓库", p => _wareHouseService.GetWareHouseName(0, p.WareHouseId)),
                  new PropertyByName<InventoryPartTaskBill>("状态", p =>  (p.InventoryStatus == 1) ? "进行中" : (p.InventoryStatus==2)? "已结束" : "已取消"),
                   new PropertyByName<InventoryPartTaskBill>("完成时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                     new PropertyByName<InventoryPartTaskBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<InventoryPartTaskBill>(properties).ExportToXlsx(inventoryPartTaskBills);
        }
        public byte[] ExportCombinationProductBillToXlsx(IList<CombinationProductBill> combinationProductBills)
        {
            var properties = new List<PropertyByName<CombinationProductBill>>()
           {
                new PropertyByName<CombinationProductBill>("单据编号", p => p.BillNumber),
                new PropertyByName<CombinationProductBill>("业务员", p =>  _userService.GetUserName(p.StoreId, p.MakeUserId)),
                new PropertyByName<CombinationProductBill>("组合商品", p =>_productService.GetProductName(0, p.ProductId)),
                new PropertyByName<CombinationProductBill>("数量", p =>p.Quantity),
                  new PropertyByName<CombinationProductBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<CombinationProductBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<CombinationProductBill>("备注", p =>p.Remark),
                     new PropertyByName<CombinationProductBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<CombinationProductBill>(properties).ExportToXlsx(combinationProductBills);
        }
        public byte[] ExportSplitProductBillToXlsx(IList<SplitProductBill> splitProductBills)
        {
            var properties = new List<PropertyByName<SplitProductBill>>()
           {
                new PropertyByName<SplitProductBill>("单据编号", p => p.BillNumber),
                new PropertyByName<SplitProductBill>("业务员", p =>  _userService.GetUserName(p.StoreId, p.MakeUserId)),
                new PropertyByName<SplitProductBill>("组合商品", p =>_productService.GetProductName(0, p.ProductId)),
                new PropertyByName<SplitProductBill>("数量", p => p.Quantity),
                  new PropertyByName<SplitProductBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<SplitProductBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<SplitProductBill>("备注", p =>p.Remark),
                     new PropertyByName<SplitProductBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<SplitProductBill>(properties).ExportToXlsx(splitProductBills);
        }
        #endregion

        #region 库存报表

        public byte[] ExportStockListToXlsx(IList<StockReportProduct> stockReportProducts)
        {
            var properties = new List<PropertyByName<StockReportProduct>>()
           {
                new PropertyByName<StockReportProduct>("商品类别", p => p.CategoryName),
                new PropertyByName<StockReportProduct>("商品编号", p => p.ProductCode),
                new PropertyByName<StockReportProduct>("商品名称", p =>  p.ProductName),
                new PropertyByName<StockReportProduct>("条形码", p =>  p.SmallBarCode),
                new PropertyByName<StockReportProduct>("现货库存", p =>  p.CurrentQuantityConversion),
                new PropertyByName<StockReportProduct>("可用库存", p =>  p.UsableQuantityConversion),
                new PropertyByName<StockReportProduct>("预占库存", p => p.OrderQuantityConversion),
                 new PropertyByName<StockReportProduct>("单位换算", p => p.UnitConversion),
                     new PropertyByName<StockReportProduct>("成本单价", p => p.CostPrice),
                     new PropertyByName<StockReportProduct>("成本金额", p => p.CostAmount),
                     new PropertyByName<StockReportProduct>("批发单价", p => p.TradePrice),
                     new PropertyByName<StockReportProduct>("批发金额", p => p.TradeAmount),

            };

            return new PropertyManager<StockReportProduct>(properties).ExportToXlsx(stockReportProducts);
        }


        public byte[] ExportGenerateStockListToXlsx(IList<StockReportProduct> stockReportProducts)
        {
            var properties = new List<PropertyByName<StockReportProduct>>()
           {
                new PropertyByName<StockReportProduct>("仓库ID", p => p.WareHouseId),
                new PropertyByName<StockReportProduct>("商品ID", p => p.ProductId),
                new PropertyByName<StockReportProduct>("库位号", p =>  ""),
                new PropertyByName<StockReportProduct>("现货库存", p =>  p.CurrentQuantity),
                new PropertyByName<StockReportProduct>("可用库存", p =>  p.UsableQuantity),
                new PropertyByName<StockReportProduct>("预占库存", p => p.OrderQuantity)
            };
            return new PropertyManager<StockReportProduct>(properties).ExportToXlsx(stockReportProducts);
        }
        public byte[] ExportChangeSummaryToXlsx(IList<StockChangeSummary> stockChangeSummaries)
        {
            var properties = new List<PropertyByName<StockChangeSummary>>()
           {
                new PropertyByName<StockChangeSummary>("商品编号", p => p.ProductSKU),
                new PropertyByName<StockChangeSummary>("商品名称", p =>  p.ProductName),
                new PropertyByName<StockChangeSummary>("条形码", p =>  p.BarCode),
                new PropertyByName<StockChangeSummary>("单位换算", p => p.UnitConversion),
                new PropertyByName<StockChangeSummary>("单价", p => p.Price),
                new PropertyByName<StockChangeSummary>("期初数量", p =>  p.InitialQuantity+ "" + p.UnitName),
                new PropertyByName<StockChangeSummary>("期初金额", p =>  Math.Round(p.InitialAmount, 2)),
                new PropertyByName<StockChangeSummary>("期末数量", p => p.EndQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("期末金额", p => Math.Round(p.EndAmount, 2)),
                     new PropertyByName<StockChangeSummary>("本期采购", p => p.CurrentPurchaseQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("本期退购", p => p.CurrentReturnQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("本期调入", p => p.CurrentAllocationInQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("本期调出", p => p.CurrentAllocationOutQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("本期销售", p => p.CurrentSaleQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("本期退售", p => p.CurrentSaleReturnQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("组合", p => p.CurrentCombinationQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("拆分", p => p.CurrentSplitReturnQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("报损", p => p.CurrentWasteQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("盘盈", p => p.CurrentVolumeQuantity + "" + p.UnitName),
                     new PropertyByName<StockChangeSummary>("盘亏", p => p.CurrentLossesQuantity + "" + p.UnitName),

            };

            return new PropertyManager<StockChangeSummary>(properties).ExportToXlsx(stockChangeSummaries);
        }

        public byte[] ExportCostPriceSummeryToXlsx(IList<CostPriceSummery> CostPriceSummery)
        {
            var properties = new List<PropertyByName<CostPriceSummery>>()
           {
                new PropertyByName<CostPriceSummery>("商品编号", p => p.ProductCode),
                new PropertyByName<CostPriceSummery>("商品名称", p =>  p.ProductName),
                new PropertyByName<CostPriceSummery>("单位", p =>  p.UnitName),
                new PropertyByName<CostPriceSummery>("期初数量", p => p.InitQuantity),
                new PropertyByName<CostPriceSummery>("期初成本单价", p => p.InitPrice),
                new PropertyByName<CostPriceSummery>("期初成本金额", p =>  Math.Round(p.InitAmount, 2)),
                new PropertyByName<CostPriceSummery>("收入数量", p => p.InQuantity),
                new PropertyByName<CostPriceSummery>("收入成本单价", p => p.InPrice),
                new PropertyByName<CostPriceSummery>("收入成本金额", p =>  Math.Round(p.InAmount, 2)),
                new PropertyByName<CostPriceSummery>("支出数量", p => p.OutQuantity),
                new PropertyByName<CostPriceSummery>("支出成本单价", p => p.OutPrice),
                new PropertyByName<CostPriceSummery>("支出成本金额", p =>  Math.Round(p.OutAmount, 2)),
                new PropertyByName<CostPriceSummery>("期末结存数量", p => p.EndQuantity),
                new PropertyByName<CostPriceSummery>("期末结存成本单价", p => p.EndPrice),
                new PropertyByName<CostPriceSummery>("期末结存成本金额", p =>  Math.Round(p.EndAmount, 2)),

            };

            return new PropertyManager<CostPriceSummery>(properties).ExportToXlsx(CostPriceSummery);
        }
        public byte[] ExportCostPriceChangeRecordsToXlsx(IList<CostPriceChangeRecords> CostPriceChangeRecords)
        {
            var properties = new List<PropertyByName<CostPriceChangeRecords>>()
           {
                new PropertyByName<CostPriceChangeRecords>("商品编号", p => p.ProductCode),
                new PropertyByName<CostPriceChangeRecords>("商品名称", p =>  p.ProductName),
                new PropertyByName<CostPriceChangeRecords>("单位", p =>  p.UnitName),
                new PropertyByName<CostPriceChangeRecords>("收入数量", p => p.InQuantity),
                new PropertyByName<CostPriceChangeRecords>("收入成本单价", p => p.InPrice),
                new PropertyByName<CostPriceChangeRecords>("收入成本金额", p =>  Math.Round(p.InAmount, 2)),
                new PropertyByName<CostPriceChangeRecords>("支出数量", p => p.OutQuantity),
                new PropertyByName<CostPriceChangeRecords>("支出成本单价", p => p.OutPrice),
                new PropertyByName<CostPriceChangeRecords>("支出成本金额", p =>  Math.Round(p.OutAmount, 2)),
                new PropertyByName<CostPriceChangeRecords>("期末结存数量", p => p.EndQuantity),
                new PropertyByName<CostPriceChangeRecords>("期末结存成本单价", p => p.EndPrice),
                new PropertyByName<CostPriceChangeRecords>("期末结存成本金额", p =>  Math.Round(p.EndAmount, 2)),

            };

            return new PropertyManager<CostPriceChangeRecords>(properties).ExportToXlsx(CostPriceChangeRecords);
        }
        public byte[] ExportChangeByOrderToXlsx(IList<StockChangeSummaryOrder> stockChangeSummaryOrders)
        {
            var properties = new List<PropertyByName<StockChangeSummaryOrder>>()
           {
                new PropertyByName<StockChangeSummaryOrder>("单据编号", p => p.BillCode),
                new PropertyByName<StockChangeSummaryOrder>("单据类型", p =>  p.BillTypeName),
                new PropertyByName<StockChangeSummaryOrder>("客户/供应商", p =>  p.CustomerSupplier),
                new PropertyByName<StockChangeSummaryOrder>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<StockChangeSummaryOrder>("交易时间", p => (p.CreatedOnUtc == null) ? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<StockChangeSummaryOrder>("商品编号", p =>  p.ProductSKU),
                new PropertyByName<StockChangeSummaryOrder>("商品名称", p =>  p.ProductName),
                new PropertyByName<StockChangeSummaryOrder>("条形码", p => p.BarCode),
                     new PropertyByName<StockChangeSummaryOrder>("单位换算", p => p.UnitConversion),
                     new PropertyByName<StockChangeSummaryOrder>("增加数量", p => (p.UsableQuantityChange > 0) ? p.UsableQuantityChangeConversion : ""),
                     new PropertyByName<StockChangeSummaryOrder>("减少数量", p => (p.UsableQuantityChange < 0) ? p.UsableQuantityChangeConversion : ""),
                     new PropertyByName<StockChangeSummaryOrder>("库存数量", p => (p.UsableQuantityAfter > 0) ? p.UsableQuantityAfterConversion : ""),
            };

            return new PropertyManager<StockChangeSummaryOrder>(properties).ExportToXlsx(stockChangeSummaryOrders);
        }
        public byte[] ExportStockReportListToXlsx(IList<InventoryReportList> inventoryReportLists)
        {
            var properties = new List<PropertyByName<InventoryReportList>>()
           {
                new PropertyByName<InventoryReportList>("客户", p => p.TerminalName),
                new PropertyByName<InventoryReportList>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<InventoryReportList>("商品编号", p =>  p.ProductCode),
                new PropertyByName<InventoryReportList>("采购商品", p => p.ProductName),
                new PropertyByName<InventoryReportList>("条形码", p => p.SmallBarCode),
                     new PropertyByName<InventoryReportList>("单位换算", p => p.UnitConversion),
                     new PropertyByName<InventoryReportList>("期初时间", p => p.BeginDate),
                     new PropertyByName<InventoryReportList>("期初库存", p => p.BeginStoreQuantityConversion),
                     new PropertyByName<InventoryReportList>("采购量", p => p.PurchaseQuantityConversion),
                      new PropertyByName<InventoryReportList>("期末库存", p => p.EndStoreQuantityConversion),
                       new PropertyByName<InventoryReportList>("期末时间", p => p.EndDate),
                        new PropertyByName<InventoryReportList>("销售量", p => p.SaleQuantityConversion),
            };

            return new PropertyManager<InventoryReportList>(properties).ExportToXlsx(inventoryReportLists);
        }
        public byte[] ExportStockReportAllToXlsx(IList<InventoryReportList> inventoryReportLists)
        {
            var properties = new List<PropertyByName<InventoryReportList>>()
           {
                new PropertyByName<InventoryReportList>("商品编号", p =>  p.ProductCode),
                new PropertyByName<InventoryReportList>("采购商品", p => p.ProductName),
                new PropertyByName<InventoryReportList>("条形码", p => p.SmallBarCode),
                     new PropertyByName<InventoryReportList>("单位换算", p => p.UnitConversion),
                     new PropertyByName<InventoryReportList>("期初时间", p => p.BeginDate),
                     new PropertyByName<InventoryReportList>("期初库存", p => p.BeginStoreQuantityConversion),
                     new PropertyByName<InventoryReportList>("采购量", p => p.PurchaseQuantityConversion),
                      new PropertyByName<InventoryReportList>("期末库存", p => p.EndStoreQuantityConversion),
                       new PropertyByName<InventoryReportList>("期末时间", p => p.EndDate),
                        new PropertyByName<InventoryReportList>("销售量", p => p.SaleQuantityConversion),
            };

            return new PropertyManager<InventoryReportList>(properties).ExportToXlsx(inventoryReportLists);
        }
        public byte[] ExportAllocationDetailsToXlsx(IList<AllocationDetailsList> allocationDetails)
        {
            var properties = new List<PropertyByName<AllocationDetailsList>>()
           {
                new PropertyByName<AllocationDetailsList>("单据编号", p =>  p.BillNumber),
                new PropertyByName<AllocationDetailsList>("出货仓库", p => p.ShipmentWareHouseName),
                new PropertyByName<AllocationDetailsList>("入货仓库", p => p.IncomeWareHouseName),
                     new PropertyByName<AllocationDetailsList>("调拨日期", p => p.CreatedOnUtc),
                     new PropertyByName<AllocationDetailsList>("审核时间", p => p.AuditedDate),
                     new PropertyByName<AllocationDetailsList>("商品名称", p => p.ProductName),
                     new PropertyByName<AllocationDetailsList>("单位换算", p => p.UnitConversion),
                      new PropertyByName<AllocationDetailsList>("数量", p => p.QuantityConversion),
                       new PropertyByName<AllocationDetailsList>("单位", p => p.UnitName),
            };

            return new PropertyManager<AllocationDetailsList>(properties).ExportToXlsx(allocationDetails);
        }
        public byte[] ExportAllocationDetailsByProductToXlsx(IList<AllocationDetailsList> allocationDetails)
        {
            var properties = new List<PropertyByName<AllocationDetailsList>>()
           {
                new PropertyByName<AllocationDetailsList>("商品名称", p =>  p.ProductName),
                new PropertyByName<AllocationDetailsList>("商品条码", p => p.BarCode),
                new PropertyByName<AllocationDetailsList>("单位换算", p => p.UnitConversion),
                     new PropertyByName<AllocationDetailsList>("出货仓库", p => p.ShipmentWareHouseName),
                     new PropertyByName<AllocationDetailsList>("入货仓库", p => p.IncomeWareHouseName),
                     new PropertyByName<AllocationDetailsList>("数量", p => p.Quantity),
            };

            return new PropertyManager<AllocationDetailsList>(properties).ExportToXlsx(allocationDetails);
        }
        #endregion

        #region 库存提醒
        public byte[] ExportStockUnsalableToXlsx(IList<StockUnsalable> stockUnsalables)
        {
            var properties = new List<PropertyByName<StockUnsalable>>()
           {
                new PropertyByName<StockUnsalable>("商品编号", p =>  p.ProductCode),
                new PropertyByName<StockUnsalable>("商品名称", p => p.ProductName),
                new PropertyByName<StockUnsalable>("库存数", p => p.StockQuantityConversion),
                     new PropertyByName<StockUnsalable>("单位换算", p => p.UnitConversion),
                     new PropertyByName<StockUnsalable>("销售数量", p => p.SaleQuantityConversion),
                     new PropertyByName<StockUnsalable>("退货数量", p => p.ReturnQuantityConversion),
                     new PropertyByName<StockUnsalable>("净销售量", p => p.NetQuantityConversion),
                     new PropertyByName<StockUnsalable>("销售金额", p => p.SaleAmount),
                     new PropertyByName<StockUnsalable>("退货金额", p => p.ReturnAmount),
                     new PropertyByName<StockUnsalable>("销售净额", p => p.NetAmount),
            };

            return new PropertyManager<StockUnsalable>(properties).ExportToXlsx(stockUnsalables);
        }
        public byte[] ExportEarlyWarningToXlsx(IList<EarlyWarning> earlyWarnings)
        {
            var properties = new List<PropertyByName<EarlyWarning>>()
           {
                new PropertyByName<EarlyWarning>("商品编号", p =>  p.ProductCode),
                new PropertyByName<EarlyWarning>("商品名称", p => p.ProductName),
                new PropertyByName<EarlyWarning>("条形码", p => p.SmallBarCode),
                     new PropertyByName<EarlyWarning>("单位换算", p => p.UnitConversion),
                     new PropertyByName<EarlyWarning>("库存数", p => p.StockQuantityConversion),
                     new PropertyByName<EarlyWarning>("缺货数", p => p.LessQuantityConversion),
                     new PropertyByName<EarlyWarning>("积压数", p => p.MoreQuantityConversion),
            };

            return new PropertyManager<EarlyWarning>(properties).ExportToXlsx(earlyWarnings);
        }
        public byte[] ExportExpirationWarningToXlsx(IList<ExpirationWarning> expirationWarnings)
        {
            var properties = new List<PropertyByName<ExpirationWarning>>()
           {
                new PropertyByName<ExpirationWarning>("商品编号", p =>  p.ProductCode),
                new PropertyByName<ExpirationWarning>("商品名称", p => p.ProductName),
                new PropertyByName<ExpirationWarning>("条形码", p => p.SmallBarCode),
                     new PropertyByName<ExpirationWarning>("单位换算", p => p.UnitConversion),
                     new PropertyByName<ExpirationWarning>("保质期", p => p.ExpirationDays),
                     new PropertyByName<ExpirationWarning>("1/3保质期", p => p.OneThirdQuantityUnitConversion),
                     new PropertyByName<ExpirationWarning>("2/3保质期", p => p.TwoThirdQuantityUnitConversion),
                     new PropertyByName<ExpirationWarning>("预警", p => p.WarningQuantityUnitConversion),
                     new PropertyByName<ExpirationWarning>("过期", p => p.ExpiredQuantityUnitConversion),
            };

            return new PropertyManager<ExpirationWarning>(properties).ExportToXlsx(expirationWarnings);
        }
        #endregion

        #endregion

        #region 财务

        #region 财务单据
        public byte[] ExportReceiptCashBillToXlsx(IList<CashReceiptBill> cashReceiptBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.CashReceiptBill);

            var properties = new List<PropertyByName<CashReceiptBill>>()
           {
                new PropertyByName<CashReceiptBill>("编号", p => p.BillNumber),
                new PropertyByName<CashReceiptBill>("收款日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<CashReceiptBill>("收款人", p =>  _userService.GetUserName(store, p.Payeer)),
                new PropertyByName<CashReceiptBill>("客户", p =>  _terminalService.GetTerminalById(0, p.CustomerId)?.Name),
                 new PropertyByName<CashReceiptBill>("客户编码", p => _terminalService.GetTerminalById(0, p.CustomerId)?.Code),
                new PropertyByName<CashReceiptBill>("收款前欠款", p => p.Items.Sum(sb => sb.ArrearsAmount)),
                //new PropertyByName<CashReceiptBill>("本次优惠金额", p => p.TotalDiscountAmount),
                // new PropertyByName<CashReceiptBill>("剩余欠款金额", p => p.TotalAmountOwedAfterReceipt),
                  new PropertyByName<CashReceiptBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<CashReceiptBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<CashReceiptBill>("备注", p => p.Remark),
                     new PropertyByName<CashReceiptBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<CashReceiptBill>(o.Value, p => p.CashReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.CashReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<CashReceiptBill>(properties).ExportToXlsx(cashReceiptBills);
        }
        public byte[] ExportPaymentReceiptBillToXlsx(IList<PaymentReceiptBill> paymentReceiptBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.PaymentReceiptBill);

            var properties = new List<PropertyByName<PaymentReceiptBill>>()
           {
                new PropertyByName<PaymentReceiptBill>("编号", p => p.BillNumber),
                new PropertyByName<PaymentReceiptBill>("付款日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<PaymentReceiptBill>("付款人", p =>  _userService.GetUserName(store, p.Draweer)),
                new PropertyByName<PaymentReceiptBill>("供应商", p =>  _manufacturerService.GetManufacturerName(store, p.ManufacturerId)),
                new PropertyByName<PaymentReceiptBill>("付款前欠款", p => p.Items.Sum(sb => sb.ArrearsAmount)),
                new PropertyByName<PaymentReceiptBill>("本次优惠金额", p => p.DiscountAmount),
                 new PropertyByName<PaymentReceiptBill>("剩余欠款金额", p => p.AmountOwedAfterReceipt),
                  new PropertyByName<PaymentReceiptBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<PaymentReceiptBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<PaymentReceiptBill>("备注", p => p.Remark),
                     new PropertyByName<PaymentReceiptBill>("打印数", p => p.PrintNum),
            };


            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<PaymentReceiptBill>(o.Value, p => p.PaymentReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.PaymentReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<PaymentReceiptBill>(properties).ExportToXlsx(paymentReceiptBills);
        }

        public byte[] ExportAdvanceReceiptBillToXlsx(IList<AdvanceReceiptBill> advanceReceiptBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.AdvanceReceiptBill);


            var properties = new List<PropertyByName<AdvanceReceiptBill>>()
           {
                new PropertyByName<AdvanceReceiptBill>("编号", p => p.BillNumber),
                new PropertyByName<AdvanceReceiptBill>("收款日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<AdvanceReceiptBill>("收款人", p =>  _userService.GetUserName(store, p.Payeer)),
                new PropertyByName<AdvanceReceiptBill>("客户", p =>  _terminalService.GetTerminalById(0, p.CustomerId).Name),
                new PropertyByName<AdvanceReceiptBill>("预收款账户", p => _accountingService.GetAccountingOptionName(store, p.AccountingOptionId ?? 0)),
                new PropertyByName<AdvanceReceiptBill>("预收金额", p => p.AdvanceAmount),
                 new PropertyByName<AdvanceReceiptBill>("优惠", p => p.DiscountAmount),
                  new PropertyByName<AdvanceReceiptBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<AdvanceReceiptBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<AdvanceReceiptBill>("备注", p => p.Remark),
                     new PropertyByName<AdvanceReceiptBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<AdvanceReceiptBill>(o.Value, p => p.Items.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.Items.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<AdvanceReceiptBill>(properties).ExportToXlsx(advanceReceiptBills);
        }

        public byte[] ExportAdvancePaymentBillToXlsx(IList<AdvancePaymentBill> advancePaymentBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.AdvancePaymentBill);

            var properties = new List<PropertyByName<AdvancePaymentBill>>()
           {
                new PropertyByName<AdvancePaymentBill>("编号", p => p.BillNumber),
                new PropertyByName<AdvancePaymentBill>("付款日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<AdvancePaymentBill>("付款人", p =>  _userService.GetUserName(store, p.Draweer)),
                new PropertyByName<AdvancePaymentBill>("供应商", p =>  _manufacturerService.GetManufacturerName(store, p.ManufacturerId)),
                new PropertyByName<AdvancePaymentBill>("预付款账户", p => _accountingService.GetAccountingOptionName(store,p.AccountingOptionId ?? 0)),
                new PropertyByName<AdvancePaymentBill>("预付金额", p => p.AdvanceAmount),
                  new PropertyByName<AdvancePaymentBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<AdvancePaymentBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<AdvancePaymentBill>("备注", p => p.Remark),
                     new PropertyByName<AdvancePaymentBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<AdvancePaymentBill>(o.Value, p => p.Items.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.Items.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<AdvancePaymentBill>(properties).ExportToXlsx(advancePaymentBills);
        }

        public byte[] ExportCostExpenditureBillToXlsx(IList<CostExpenditureBill> costExpenditureBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.CostExpenditureBill);

            var properties = new List<PropertyByName<CostExpenditureBill>>()
           {
                new PropertyByName<CostExpenditureBill>("编号", p => p.BillNumber),
                new PropertyByName<CostExpenditureBill>("操作日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                 new PropertyByName<CostExpenditureBill>("付款日期", p => (p.PayDate==null)? "" : ((DateTime)p.PayDate).ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<CostExpenditureBill>("员工", p =>  _userService.GetUserName(store, p.EmployeeId)),
                //new PropertyByName<CostExpenditureBill>("客户", p =>  _terminalService.GetTerminalById(p.CustomerId).Name),
                 //new PropertyByName<CostExpenditureBill>("客户编码", p =>  _manufacturerService.GetManufacturerName(p.ManufacturerId)),
                //new PropertyByName<CostExpenditureBill>("费用类别", p => ),
                  new PropertyByName<CostExpenditureBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<CostExpenditureBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<CostExpenditureBill>("备注", p => p.Remark),
                     new PropertyByName<CostExpenditureBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<CostExpenditureBill>(o.Value, p => p.CostExpenditureBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.CostExpenditureBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<CostExpenditureBill>(properties).ExportToXlsx(costExpenditureBills);
        }
        public byte[] ExportCostContractBillToXlsx(IList<CostContractBill> costContractBills)
        {
            var properties = new List<PropertyByName<CostContractBill>>()
           {
                new PropertyByName<CostContractBill>("编号", p => p.BillNumber),
                new PropertyByName<CostContractBill>("合同类型", p =>  p.ContractType==0?"按月兑付":p.ContractType==1?"按单位量总计兑付":"从主管赠品扣减"),
                 new PropertyByName<CostContractBill>("费用类型", p =>  _accountingService.GetAccountingOptionName(0, p.AccountingOptionId)),
                new PropertyByName<CostContractBill>("业务员", p =>  _userService.GetUserName(p.StoreId, p.EmployeeId)),
                new PropertyByName<CostContractBill>("客户", p =>  _terminalService.GetTerminalById(0, p.CustomerId).Name),
                 new PropertyByName<CostContractBill>("客户编码", p =>  _terminalService.GetTerminalById(0, p.CustomerId).Code),
                  new PropertyByName<CostContractBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<CostContractBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<CostContractBill>("备注", p => p.Remark),
                     new PropertyByName<CostContractBill>("打印数", p => p.PrintNum),
            };

            return new PropertyManager<CostContractBill>(properties).ExportToXlsx(costContractBills);
        }
        public byte[] ExportFinancialIncomeBillToXlsx(IList<FinancialIncomeBill> financialIncomeBills, int store)
        {
            var defaultAcc = _accountingService.GetDefaultAccounting(store, BillTypeEnum.FinancialIncomeBill);

            var properties = new List<PropertyByName<FinancialIncomeBill>>()
           {
                new PropertyByName<FinancialIncomeBill>("编号", p => p.BillNumber),
                new PropertyByName<FinancialIncomeBill>("交易日期", p => (p.CreatedOnUtc==null)? "" : p.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<FinancialIncomeBill>("业务员", p =>  _userService.GetUserName(store, p.SalesmanId)),
                  new PropertyByName<FinancialIncomeBill>("状态", p => (p.AuditedStatus == false) ? "未审核" : "已审核"),
                   new PropertyByName<FinancialIncomeBill>("审核时间", p =>(p.AuditedDate == null) ? "" : ((DateTime)p.AuditedDate).ToString("yyyy/MM/dd HH:mm:ss")),
                    new PropertyByName<FinancialIncomeBill>("备注", p => p.Remark),
                     new PropertyByName<FinancialIncomeBill>("打印数", p => p.PrintNum),
            };

            if (defaultAcc.Item4 != null && defaultAcc.Item4.Count > 0)
            {
                defaultAcc.Item4.ToList().ForEach(o =>
                {
                    properties.Add(new PropertyByName<FinancialIncomeBill>(o.Value, p => p.FinancialIncomeBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault() == null ? 0 : p.FinancialIncomeBillAccountings.Where(a => a?.AccountingOption?.ParentId == o.Key).FirstOrDefault().CollectionAmount));
                });
            }

            return new PropertyManager<FinancialIncomeBill>(properties).ExportToXlsx(financialIncomeBills);
        }
        #endregion

        #region 财务凭证
        public byte[] ExportRecordingVoucherToXlsx(IList<RecordingVoucher> recordingVouchers)
        {
            var properties = new List<PropertyByName<RecordingVoucher>>()
           {
                new PropertyByName<RecordingVoucher>("日期", p => p.RecordTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new PropertyByName<RecordingVoucher>("凭证字号", p => p.RecordName),
                 new PropertyByName<RecordingVoucher>("单据编号", p => p.BillNumber),
                new PropertyByName<RecordingVoucher>("摘要", p =>  p.Items.FirstOrDefault(c=>c.RecordingVoucherId==p.Id).Summary),
                new PropertyByName<RecordingVoucher>("科目", p =>  p.Items.FirstOrDefault(c=>c.RecordingVoucherId==p.Id).AccountingOptionId+""+ _accountingService.GetAccountingOptionById(p.Items.FirstOrDefault(c=>c.RecordingVoucherId==p.Id).AccountingOptionId).Name),
                 new PropertyByName<RecordingVoucher>("借方金额", p => Math.Round(p.Items.FirstOrDefault(c=>c.RecordingVoucherId==p.Id).DebitAmount ?? 0, 2)),
                  new PropertyByName<RecordingVoucher>("贷方金额", p => Math.Round(p.Items.FirstOrDefault(c=>c.RecordingVoucherId==p.Id).CreditAmount ?? 0, 2)),
                   new PropertyByName<RecordingVoucher>("状态", p =>(p.AuditedStatus == false) ? "未审核" : "已审核"),
                    new PropertyByName<RecordingVoucher>("生成方式", p => CommonHelper.GetEnumDescription((GenerateMode)Enum.Parse(typeof(GenerateMode), p.GenerateMode.ToString()))),
                     new PropertyByName<RecordingVoucher>("制单人", p => _userService.GetUserName(p.StoreId, p.MakeUserId)),
                     new PropertyByName<RecordingVoucher>("审核人", p => _userService.GetUserName(p.StoreId, p.AuditedUserId??0)),
            };

            return new PropertyManager<RecordingVoucher>(properties).ExportToXlsx(recordingVouchers);
        }
        #endregion

        #region 财务报表
        public byte[] ExportTrialBalanceToXlsx(IList<TrialBalanceExport> trialBalances)
        {
            var properties = new List<PropertyByName<TrialBalanceExport>>()
           {
                new PropertyByName<TrialBalanceExport>("科目名称", p => p.AccountingOptionName),
                new PropertyByName<TrialBalanceExport>("科目代码", p => p.AccountingOptionCode),
                new PropertyByName<TrialBalanceExport>("期初余额(借方)", p => p.InitialBalanceDebit),
                new PropertyByName<TrialBalanceExport>("期初余额(贷方)", p => p.InitialBalanceCredit),
                new PropertyByName<TrialBalanceExport>("本期发生额(借方)", p => p.PeriodBalanceDebit),
                new PropertyByName<TrialBalanceExport>("本期发生额(贷方)", p => p.PeriodBalanceCredit),
                new PropertyByName<TrialBalanceExport>("期末余额(借方)", p => p.EndBalanceDebit),
                new PropertyByName<TrialBalanceExport>("期末余额(贷方)", p => p.EndBalanceCredit),
            };

            return new PropertyManager<TrialBalanceExport>(properties).ExportToXlsx(trialBalances);
        }
        public byte[] ExportBalanceSheetToXlsx(IList<BalanceSheetExport> balancesheets)
        {
            var properties = new List<PropertyByName<BalanceSheetExport>>()
           {
                new PropertyByName<BalanceSheetExport>("科目类型", p => p.AccountingTypeName),
                new PropertyByName<BalanceSheetExport>("科目名称", p => p.AccountingOptionName),
                new PropertyByName<BalanceSheetExport>("科目代码", p => p.AccountingOptionCode),
                new PropertyByName<BalanceSheetExport>("期末余额", p => p.EndBalance),
                new PropertyByName<BalanceSheetExport>("年初余额", p => p.InitialBalance),

            };

            return new PropertyManager<BalanceSheetExport>(properties).ExportToXlsx(balancesheets);
        }
        //利润表
        public byte[] ExportProfitSheetToXlsx(IList<ProfitSheetExport> profitsheet)
        {
            var properties = new List<PropertyByName<ProfitSheetExport>>()
           {
                new PropertyByName<ProfitSheetExport>("科目类型", p => p.AccountingTypeName),
                new PropertyByName<ProfitSheetExport>("科目名称", p => p.AccountingOptionName),
                new PropertyByName<ProfitSheetExport>("科目代码", p => p.AccountingOptionCode),
                new PropertyByName<ProfitSheetExport>("本年累计金额", p => p.AccumulatedAmountOfYear),
                new PropertyByName<ProfitSheetExport>("本期金额", p => p.CurrentAmount),

            };

            return new PropertyManager<ProfitSheetExport>(properties).ExportToXlsx(profitsheet);
        }
        #endregion

        #endregion

        #region 档案
        /// <summary>
        /// 导出商品到 XLSX
        /// </summary>
        /// <param name="products">Products</param>
        public virtual byte[] ExportProductsToXlsx(IEnumerable<DCMS.Core.Domain.Products.Product> products)
        {
            var properties = new[]
            {
                new PropertyByName<DCMS.Core.Domain.Products.Product>("商品名称", p => p.Name),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("助记码", p => p.MnemonicCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("商品类别", p => _categoryService.GetCategoryById(0, p.CategoryId)?.Name),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("品牌", p => _brandService.GetBrandById(0, p.BrandId)?.Name),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("小单位", p => p.SmallUnitId),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("中单位", p => p.StrokeUnitId),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("大单位", p => p.BigUnitId),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("商品编码", p => p.ProductCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("规格", p => p.Specification),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("原产地", p => p.CountryOrigin),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("主供应商", p => p.Supplier),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("其他条码", p => p.OtherBarCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("其他条码1", p => p.OtherBarCode1),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("其他条码2", p => p.OtherBarCode2),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("统计类别", p => p.StatisticalType),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("保质期", p => p.ExpirationDays),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("临期预警（天）", p => p.AdventDays),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("是否分口味核算库存", p => p.IsFlavor),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("小单位条码", p => p.SmallBarCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("中单位量", p => p.StrokeQuantity),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("中单位条码", p => p.StrokeBarCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("大单位量", p => p.BigQuantity),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("大单位条码", p => p.BigBarCode),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("SKU", p => p.Sku),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("提供商编号", p => p.ManufacturerPartNumber),

                new PropertyByName<DCMS.Core.Domain.Products.Product>("库存量", p => p.StockQuantity),
                new PropertyByName<DCMS.Core.Domain.Products.Product>("最小库存", p => p.MinStockQuantity),
            };

            var productList = products.ToList();

            var productAdvancedMode = true;
            try
            {
                productAdvancedMode = _genericAttributeService.GetAttribute<bool>(_workContext.CurrentUser, "product-advanced-mode");
            }
            catch (ArgumentNullException)
            {
            }

            return new PropertyManager<DCMS.Core.Domain.Products.Product>(properties).ExportToXlsx(productList);
        }

        /// <summary>
        /// 导出用户到 XLSX
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public virtual byte[] ExportUsersToXlsx(IList<User> users)
        {
            //property manager 
            var manager = new PropertyManager<User>(new[]
            {
                new PropertyByName<User>("UserId", p => p.Id),
                new PropertyByName<User>("UserGuid", p => p.UserGuid),
                new PropertyByName<User>("Email", p => p.Email),
                new PropertyByName<User>("Username", p => p.Username),
                new PropertyByName<User>("Password", p => _userService.GetCurrentPassword(p.Id)?.Password),
                new PropertyByName<User>("PasswordFormatId", p => _userService.GetCurrentPassword(p.Id)?.PasswordFormatId ?? 0),
                new PropertyByName<User>("PasswordSalt", p => _userService.GetCurrentPassword(p.Id)?.PasswordSalt),
            });

            return manager.ExportToXlsx(users);
        }

        /// <summary>
        /// 导出用户到 XML
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public virtual string ExportUsersToXml(IList<User> users)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Users");
            xmlWriter.WriteAttributeString("Version", DCMSVersion.CurrentVersion);

            foreach (var user in users)
            {
                xmlWriter.WriteStartElement("User");
                xmlWriter.WriteElementString("UserId", null, user.Id.ToString());
                xmlWriter.WriteElementString("UserGuid", null, user.UserGuid.ToString());
                xmlWriter.WriteElementString("Email", null, user.Email);
                xmlWriter.WriteElementString("Username", null, user.Username);

                var userPassword = _userService.GetCurrentPassword(user.Id);
                xmlWriter.WriteElementString("Password", null, userPassword?.Password);
                xmlWriter.WriteElementString("PasswordFormatId", null, (userPassword?.PasswordFormatId ?? 0).ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, userPassword?.PasswordSalt);


                var selectedUserAttributesString = _genericAttributeService.GetAttribute<string>(user, DCMSDefaults.CustomUserAttributes);

                if (!string.IsNullOrEmpty(selectedUserAttributesString))
                {
                    var selectedUserAttributes = new StringReader(selectedUserAttributesString);
                    var selectedUserAttributesXmlReader = XmlReader.Create(selectedUserAttributes);
                    xmlWriter.WriteNode(selectedUserAttributesXmlReader, false);
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// 商品类别
        /// </summary>
        /// <param name="categories"></param>
        /// <returns></returns>
        public byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories)
        {
            var manager = new PropertyManager<Category>(new[]
            {
                new PropertyByName<Category>("名称", p => p.Name),
                new PropertyByName<Category>("父类代码", p => p.ParentId),
                new PropertyByName<Category>("继承路径", p => p.PathCode),
                new PropertyByName<Category>("统计类别", p => p.StatisticalType),
                new PropertyByName<Category>("品牌", p => p.BrandName),
                new PropertyByName<Category>("状态", p => p.Status),
                new PropertyByName<Category>("序号", p=> p.OrderNo),
            });

            return manager.ExportToXlsx(categories);
        }

        /// <summary>
        /// 品牌
        /// </summary>
        /// <param name="brands"></param>
        /// <returns></returns>
        public byte[] ExportBrandsToXlsx(IEnumerable<Brand> brands)
        {
            var manager = new PropertyManager<Brand>(new[]
           {
                new PropertyByName<Brand>("名称", p => p.Name),
                new PropertyByName<Brand>("状态", p => p.Status==true ? 1:0),
                new PropertyByName<Brand>("排序", p=> p.DisplayOrder),
            });

            return manager.ExportToXlsx(brands);
        }

        /// <summary>
        /// 片区
        /// </summary>
        /// <param name="districts"></param>
        /// <returns></returns>
        public byte[] ExportDistrictsToXlsx(IEnumerable<District> districts)
        {
            var manager = new PropertyManager<District>(new[]
          {
                new PropertyByName<District>("名称", p => p.Name),
                new PropertyByName<District>("父级代码", p => p.ParentId),
                new PropertyByName<District>("序号", p=> p.OrderNo),
                new PropertyByName<District>("描述", p=> p.Describe),
            });

            return manager.ExportToXlsx(districts);
        }

        /// <summary>
        /// 渠道
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public byte[] ExportChannelsToXlsx(IEnumerable<Channel> channels)
        {
            var manager = new PropertyManager<Channel>(new[]
          {
                new PropertyByName<Channel>("名称", p => p.Name),
                new PropertyByName<Channel>("序号", p=> p.OrderNo),
                new PropertyByName<Channel>("描述", p=> p.Describe),
                 new PropertyByName<Channel>("属性", p=> p.Attribute),
            });

            return manager.ExportToXlsx(channels);
        }

        /// <summary>
        /// 终端等级
        /// </summary>
        /// <param name="ranks"></param>
        /// <returns></returns>
        public byte[] ExportRanksToXlsx(IEnumerable<Rank> ranks)
        {
            var manager = new PropertyManager<Rank>(new[]
          {
                new PropertyByName<Rank>("名称", p => p.Name),
                new PropertyByName<Rank>("描述", p=> p.Describe),
            });

            return manager.ExportToXlsx(ranks);
        }

        /// <summary>
        /// 线路
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public byte[] ExportLineTiersToXlsx(IEnumerable<LineTier> lines)
        {
            var manager = new PropertyManager<LineTier>(new[]
          {
                new PropertyByName<LineTier>("名称", p => p.Name),
                new PropertyByName<LineTier>("是否启用", p=> p.Enabled==true ? 1:0),
            });

            return manager.ExportToXlsx(lines);
        }

        /// <summary>
        /// 供应商
        /// </summary>
        /// <param name="manufacturers"></param>
        /// <returns></returns>
        public byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers)
        {
            var manager = new PropertyManager<Manufacturer>(new[]
           {
                new PropertyByName<Manufacturer>("名称", p => p.Name),
                 new PropertyByName<Manufacturer>("描述", p=> p.Description),
                new PropertyByName<Manufacturer>("助记码", p=> p.MnemonicName),
                 new PropertyByName<Manufacturer>("联系人", p=> p.ContactName),
                  new PropertyByName<Manufacturer>("联系电话", p=> p.ContactPhone),
                   new PropertyByName<Manufacturer>("地址", p=> p.Address),
                    new PropertyByName<Manufacturer>("状态", p=> p.Status==true ? 1:0),
                     new PropertyByName<Manufacturer>("价格范围", p=> p.PriceRanges),
                      new PropertyByName<Manufacturer>("排序", p=> p.DisplayOrder),
            });

            return manager.ExportToXlsx(manufacturers);
        }

        /// <summary>
        /// 仓库
        /// </summary>
        /// <param name="wareHouses"></param>
        /// <returns></returns>
        public byte[] ExportWareHousesToXlsx(IEnumerable<WareHouse> wareHouses)
        {
            var manager = new PropertyManager<WareHouse>(new[]
           {
                new PropertyByName<WareHouse>("仓库编号", p => p.Code),
                 new PropertyByName<WareHouse>("名称", p=> p.Name),
                new PropertyByName<WareHouse>("仓库类型", p=> p.Type),
                    new PropertyByName<WareHouse>("状态", p=> p.Status==true ? 1:0),
            });

            return manager.ExportToXlsx(wareHouses);
        }

        /// <summary>
        /// 终端档案
        /// </summary>
        /// <param name="terminals"></param>
        /// <returns></returns>
        public byte[] ExportTerminalsToXlsx(IEnumerable<Terminal> terminals)
        {
            var manager = new PropertyManager<Terminal>(new[]
           {
                new PropertyByName<Terminal>("名称", p => p.Name),
                new PropertyByName<Terminal>("助记码", p=> p.MnemonicName),
                 new PropertyByName<Terminal>("老板姓名", p=> p.BossName),
                  new PropertyByName<Terminal>("联系电话", p=> p.BossCall),
                  new PropertyByName<Terminal>("状态", p=> p.Status==true ? 1:0),
                  new PropertyByName<Terminal>("最大欠款额度", p=> p.MaxAmountOwed),
                  new PropertyByName<Terminal>("终端编码", p=> p.Code),
                   new PropertyByName<Terminal>("地址", p=> p.Address),
                     new PropertyByName<Terminal>("备注", p=> p.Remark),
                      new PropertyByName<Terminal>("片区", p=> _districtService.GetDistrictById(0, p.DistrictId)?.Name),
                      new PropertyByName<Terminal>("渠道", p=> _channelService.GetChannelById(0, p.ChannelId)?.Name),
                      new PropertyByName<Terminal>("线路", p=> _lineTierService.GetLineTierById(0, p.LineId)?.Name),
                      new PropertyByName<Terminal>("终端等级", p=> _rankService.GetRankById(0, p.RankId)?.Name),
                      new PropertyByName<Terminal>("付款方式", p=> p.PaymentMethod),
                      new PropertyByName<Terminal>("经度", p=> p.Location_Lng),
                      new PropertyByName<Terminal>("纬度", p=> p.Location_Lat),
                      new PropertyByName<Terminal>("营业编号", p=> p.BusinessNo),
                      new PropertyByName<Terminal>("许可证号", p=> p.FoodBusinessLicenseNo),
                      new PropertyByName<Terminal>("企业注册号", p=> p.EnterpriseRegNo),
            });

            return manager.ExportToXlsx(terminals);
        }

        /// <summary>
        /// 初始库存
        /// </summary>
        /// <param name="stocks"></param>
        /// <returns></returns>
        public byte[] ExportStocksToXlsx(IEnumerable<Stock> stocks)
        {
            var manager = new PropertyManager<Stock>(new[]
          {
                new PropertyByName<Stock>("仓库ID", p => _wareHouseService.GetWareHouseById(0, p.WareHouseId)?.Name),
                new PropertyByName<Stock>("商品ID", p=> _productService.GetProductById(0, p.ProductId)?.Name),
                 new PropertyByName<Stock>("库位号", p=> p.PositionCode),
                  new PropertyByName<Stock>("可用库存量", p=> p.UsableQuantity),
                  new PropertyByName<Stock>("现货库存量", p=> p.CurrentQuantity),
                  new PropertyByName<Stock>("预占库存量", p=> p.LockQuantity),
            });

            return manager.ExportToXlsx(stocks);
        }

        /// <summary>
        /// 科目余额
        /// </summary>
        /// <param name="trialBalances"></param>
        /// <returns></returns>
        public byte[] ExportTrialBalancesToXlsx(IEnumerable<TrialBalance> trialBalances)
        {
            var manager = new PropertyManager<TrialBalance>(new[]
          {
                new PropertyByName<TrialBalance>("科目类别", p => _accountingService.GetAccountingTypeById(p.AccountingTypeId)?.Name),
                new PropertyByName<TrialBalance>("科目名称", p=> _accountingService.GetAccountingOptionById(p.AccountingOptionId)?.Name),
                 new PropertyByName<TrialBalance>("结转期数", p=> p.PeriodDate),
                  new PropertyByName<TrialBalance>("期初余额(借方)", p=> p.InitialBalanceDebit),
                  new PropertyByName<TrialBalance>("期初余额(贷方)", p=> p.InitialBalanceCredit),
                  new PropertyByName<TrialBalance>("本期发生额(借方)", p=> p.PeriodBalanceDebit),
                  new PropertyByName<TrialBalance>("本期发生额(贷方)", p=> p.PeriodBalanceCredit),
                  new PropertyByName<TrialBalance>("期末余额(借方)", p=> p.EndBalanceDebit),
                  new PropertyByName<TrialBalance>("期末余额(贷方)", p=> p.EndBalanceCredit),
            });

            return manager.ExportToXlsx(trialBalances);
        }

        /// <summary>
        /// 应收款期初
        /// </summary>
        /// <param name="trialBalances"></param>
        /// <returns></returns>
        public byte[] ExportReceivablesToXlsx(IEnumerable<Receivable> Receivables)
        {
            var manager = new PropertyManager<Receivable>(new[]
          {
                new PropertyByName<Receivable>("终端名称", p => _terminalService.GetTerminalName(0,p.TerminalId)),
                new PropertyByName<Receivable>("初始欠款", p=> p.OweCash),
                new PropertyByName<Receivable>("业务员", p => _userService.GetUserById(0,p.Id)?.UserRealName),
                 new PropertyByName<Receivable>("欠款时间", p=> p.BalanceDate),
                  new PropertyByName<Receivable>("备注", p=> p.Remark),
            });

            return manager.ExportToXlsx(Receivables);
        }
        #endregion

        #region 报表

        #region 资金报表
        public byte[] ExportFundReportCustomerAccountToXlsx(IList<CustomerAccountDealings> customerAccountDealings)
        {
            var properties = new List<PropertyByName<CustomerAccountDealings>>()
           {
                new PropertyByName<CustomerAccountDealings>("单据编号", p =>  p.BillNumber),
                new PropertyByName<CustomerAccountDealings>("单据类型", p => p.BillTypeName),
                new PropertyByName<CustomerAccountDealings>("客户", p => p.TerminalName),
                     new PropertyByName<CustomerAccountDealings>("客户编码", p => p.TerminalCode),
                     new PropertyByName<CustomerAccountDealings>("交易时间", p => p.TransactionDate.Value. ToString("yyyy/MM/dd HH:mm:ss")),
                        new PropertyByName<CustomerAccountDealings>("单据金额", p => p.BillAmount),
                        new PropertyByName<CustomerAccountDealings>("优惠金额", p => p.PreferentialAmount),
                        new PropertyByName<CustomerAccountDealings>("收款金额", p => p.CashReceiptAmount),
                        new PropertyByName<CustomerAccountDealings>("应收款减", p => p.ReceivableAmountSubtract),
                        new PropertyByName<CustomerAccountDealings>("应收款增", p => p.ReceivableAmountAdd),
                        new PropertyByName<CustomerAccountDealings>("应收款余额", p => p.ReceivableAmountOverage),
                        new PropertyByName<CustomerAccountDealings>("预收款减", p => p.AdvancePaymentAmountSubtract),
                        new PropertyByName<CustomerAccountDealings>("预收款增", p => p.AdvancePaymentAmountAdd),
                        new PropertyByName<CustomerAccountDealings>("预收款余额", p => p.AdvancePaymentAmountOverage),
                        new PropertyByName<CustomerAccountDealings>("订货款余额", p => p.SubscribeCashAmountOverage),
                        new PropertyByName<CustomerAccountDealings>("往来账余额", p => p.AccountAmountOverage),
                        new PropertyByName<CustomerAccountDealings>("备注", p => p.Remark),

            };

            return new PropertyManager<CustomerAccountDealings>(properties).ExportToXlsx(customerAccountDealings);
        }
        public byte[] ExportFundReportCustomerReceiptCashToXlsx(IList<FundReportCustomerReceiptCash> fundReportCustomerReceiptCashes)
        {
            var properties = new List<PropertyByName<FundReportCustomerReceiptCash>>()
           {
                new PropertyByName<FundReportCustomerReceiptCash>("客户", p => p.TerminalName),
                     new PropertyByName<FundReportCustomerReceiptCash>("客户编码", p => p.TerminalCode),
                     new PropertyByName<FundReportCustomerReceiptCash>("累计欠款", p => p.OweCase),
                        new PropertyByName<FundReportCustomerReceiptCash>("销售金额", p => p.SaleAmount),
                        new PropertyByName<FundReportCustomerReceiptCash>("退货金额", p => p.ReturnAmount),
                        new PropertyByName<FundReportCustomerReceiptCash>("净销售额", p => p.NetAmount),
                        new PropertyByName<FundReportCustomerReceiptCash>("首次欠款时间", p => p.FirstOweCaseDate.Value.ToString("yyyy/MM/dd HH:mm:ss")),
                        new PropertyByName<FundReportCustomerReceiptCash>("末次欠款时间", p => p.LastOweCaseDate.Value.ToString("yyyy/MM/dd HH:mm:ss")),

            };

            return new PropertyManager<FundReportCustomerReceiptCash>(properties).ExportToXlsx(fundReportCustomerReceiptCashes);
        }
        public byte[] ExportFundReportManufacturerAccountToXlsx(IList<FundReportManufacturerAccount> fundReportManufacturerAccounts)
        {
            var properties = new List<PropertyByName<FundReportManufacturerAccount>>()
           {
                new PropertyByName<FundReportManufacturerAccount>("单据编号", p =>  p.BillNumber),
                new PropertyByName<FundReportManufacturerAccount>("单据类型", p => p.BillTypeName),
                new PropertyByName<FundReportManufacturerAccount>("供应商", p => p.ManufacturerName),
                     new PropertyByName<FundReportManufacturerAccount>("发生日期", p => p.TransactionDate.Value. ToString("yyyy/MM/dd HH:mm:ss")),
                        new PropertyByName<FundReportManufacturerAccount>("单据金额", p => p.BillAmount),
                        new PropertyByName<FundReportManufacturerAccount>("优惠金额", p => p.PreferentialAmount),
                        new PropertyByName<FundReportManufacturerAccount>("付款金额", p => p.PayCashAmount),
                        new PropertyByName<FundReportManufacturerAccount>("应付款减", p => p.PayAmountSubtract),
                        new PropertyByName<FundReportManufacturerAccount>("应付款增", p => p.PayAmountAdd),
                        new PropertyByName<FundReportManufacturerAccount>("应付款余额", p => p.PayAmountOverage),
                        new PropertyByName<FundReportManufacturerAccount>("预付款减", p => p.AdvancePayAmountSubtract),
                        new PropertyByName<FundReportManufacturerAccount>("预付款增", p => p.AdvancePayAmountAdd),
                        new PropertyByName<FundReportManufacturerAccount>("预付款余额", p => p.AdvancePayAmountOverage),
                        new PropertyByName<FundReportManufacturerAccount>("往来账余额", p => p.AccountAmountOverage),
                        new PropertyByName<FundReportManufacturerAccount>("备注", p => p.Remark),

            };

            return new PropertyManager<FundReportManufacturerAccount>(properties).ExportToXlsx(fundReportManufacturerAccounts);
        }
        public byte[] ExportFundReportManufacturerPayCashToXlsx(IList<FundReportManufacturerPayCash> fundReportManufacturerPayCashes)
        {
            var properties = new List<PropertyByName<FundReportManufacturerPayCash>>()
           {
                new PropertyByName<FundReportManufacturerPayCash>("供应商", p => p.ManufacturerName),
                     new PropertyByName<FundReportManufacturerPayCash>("累计欠款", p => p.OweCase),
                        new PropertyByName<FundReportManufacturerPayCash>("首次欠款时间", p => p.FirstOweCaseDate.Value.ToString("yyyy/MM/dd HH:mm:ss")),
                        new PropertyByName<FundReportManufacturerPayCash>("末次欠款时间", p => p.LastOweCaseDate.Value.ToString("yyyy/MM/dd HH:mm:ss")),

            };

            return new PropertyManager<FundReportManufacturerPayCash>(properties).ExportToXlsx(fundReportManufacturerPayCashes);
        }
        public byte[] ExportFundReportAdvanceReceiptOverageToXlsx(IList<FundReportAdvanceReceiptOverage> fundReportAdvanceReceiptOverages, int store)
        {
            var dic = _accountingService.GetAccountingOptionsByParentId(store, new int[] { 23 }, true).Select(a => a).ToList();

            var properties = new List<PropertyByName<FundReportAdvanceReceiptOverage>>()
           {
                new PropertyByName<FundReportAdvanceReceiptOverage>("客户", p => p.TerminalName),
                     new PropertyByName<FundReportAdvanceReceiptOverage>("客户编码", p => p.TerminalCode),
                        new PropertyByName<FundReportAdvanceReceiptOverage>("预收款余额", p => p.AdvanceReceiptOverageAmount),
                        new PropertyByName<FundReportAdvanceReceiptOverage>("应收款余额", p => p.ReceivableOverageAmount),
                        new PropertyByName<FundReportAdvanceReceiptOverage>("余额", p => p.OverageAmount),

            };

            if (dic != null && dic.Count > 0)
            {
                dic.ForEach(o =>
                {
                    properties.Add(new PropertyByName<FundReportAdvanceReceiptOverage>(o.Name, p => Math.Round(p.AccountingOptions.FirstOrDefault(sao => sao.AccountingOptionId == o.Id) == null ? 0 : p.AccountingOptions.FirstOrDefault(sao => sao.AccountingOptionId == o.Id).CollectionAmount)));
                });
            }

            return new PropertyManager<FundReportAdvanceReceiptOverage>(properties).ExportToXlsx(fundReportAdvanceReceiptOverages);
        }
        public byte[] ExportFundReportAdvancePaymentOverageToXlsx(IList<FundReportAdvancePaymentOverage> fundReportAdvancePaymentOverages)
        {
            var properties = new List<PropertyByName<FundReportAdvancePaymentOverage>>()
           {
                new PropertyByName<FundReportAdvancePaymentOverage>("供应商", p => p.ManufacturerName),
                     new PropertyByName<FundReportAdvancePaymentOverage>("预付款账户（预付款）", p => p.AdvancePaymentAmount),
                        new PropertyByName<FundReportAdvancePaymentOverage>("余额", p => p.OverageAmount),

            };

            return new PropertyManager<FundReportAdvancePaymentOverage>(properties).ExportToXlsx(fundReportAdvancePaymentOverages);
        }
        #endregion

        #region 员工报表
        public byte[] ExportStaffReportBusinessUserAchievementToXlsx(IList<StaffReportBusinessUserAchievement> staffReportBusinessUserAchievements)
        {
            var properties = new List<PropertyByName<StaffReportBusinessUserAchievement>>()
           {
                new PropertyByName<StaffReportBusinessUserAchievement>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<StaffReportBusinessUserAchievement>("销售金额", p => p.SaleAmount),
                new PropertyByName<StaffReportBusinessUserAchievement>("退货金额", p => p.ReturnAmount),
                new PropertyByName<StaffReportBusinessUserAchievement>("销售净额", p => p.NetAmount),
            };

            return new PropertyManager<StaffReportBusinessUserAchievement>(properties).ExportToXlsx(staffReportBusinessUserAchievements);
        }
        public byte[] ExportStaffReportPercentageSummaryToXlsx(IList<StaffReportPercentageSummary> staffReportPercentageSummaries)
        {
            var properties = new List<PropertyByName<StaffReportPercentageSummary>>()
           {
                new PropertyByName<StaffReportPercentageSummary>("员工名称", p =>  p.StaffUserName),
                new PropertyByName<StaffReportPercentageSummary>("业务提成", p => p.BusinessPercentage),
                new PropertyByName<StaffReportPercentageSummary>("送货提成", p => p.DeliveryPercentage),
                     new PropertyByName<StaffReportPercentageSummary>("提成合计", p => p.PercentageTotal),
            };

            return new PropertyManager<StaffReportPercentageSummary>(properties).ExportToXlsx(staffReportPercentageSummaries);
        }
        public byte[] ExportBusinessUserVisitingRecordToXlsx(IList<VisitStore> visitStores)
        {
            var properties = new List<PropertyByName<VisitStore>>()
           {
                new PropertyByName<VisitStore>("业务员", p =>  p.BusinessUserName),
                new PropertyByName<VisitStore>("客户", p => p.TerminalName),
                new PropertyByName<VisitStore>("客户编码", p => p.CodeNumber),
                     new PropertyByName<VisitStore>("状态", p => p.VisitTypeName),
                     new PropertyByName<VisitStore>("签到时间", p => p.SigninDateTime.ToString("yyyy/MM/dd HH:mm:ss")),
                        new PropertyByName<VisitStore>("在店时间", p => p.OnStoreStopSeconds),
                        //new PropertyByName<VisitStore>("线路", p => p.LineName),
                        //new PropertyByName<VisitStore>("未拜访天数", p => p.NoVisitedDays),
                        //new PropertyByName<VisitStore>("销订金额", p => Math.Round(p.SaleOrderAmount ?? 0, 2)),
                        //new PropertyByName<VisitStore>("退订金额", p => Math.Round(p.ReturnOrderAmount ?? 0, 2)),
                        //new PropertyByName<VisitStore>("销售金额", p => Math.Round(p.SaleAmount ?? 0, 2)),
                        //new PropertyByName<VisitStore>("退货金额", p => Math.Round(p.ReturnAmount ?? 0, 2)),
                        new PropertyByName<VisitStore>("是否下单", p => ((p.SaleBillId != null && p.SaleBillId != 0)||(p.SaleReservationBillId != null && p.SaleReservationBillId != 0)||(p.ReturnBillId != null && p.ReturnBillId != 0)||(p.ReturnReservationBillId != null && p.ReturnReservationBillId !=0))?"是":"否"),
                        new PropertyByName<VisitStore>("备注", p => p.Remark),

            };

            return new PropertyManager<VisitStore>(properties).ExportToXlsx(visitStores);
        }
        #endregion

        #region 市场报表
        //客户活跃度
        public byte[] ExportMarketReportTerminalActiveToXlsx(IList<MarketReportTerminalActive> marketReportTerminalActives)
        {
            var properties = new List<PropertyByName<MarketReportTerminalActive>>()
           {
                new PropertyByName<MarketReportTerminalActive>("客户", p => p.TerminalName),
                new PropertyByName<MarketReportTerminalActive>("客户编码", p => p.TerminalCode),
                new PropertyByName<MarketReportTerminalActive>("无拜访天数", p => p.NoVisitDays),
                new PropertyByName<MarketReportTerminalActive>("无销售天数", p => p.NoSaleDays),
                new PropertyByName<MarketReportTerminalActive>("片区", p => p.DistrictName),

            };

            return new PropertyManager<MarketReportTerminalActive>(properties).ExportToXlsx(marketReportTerminalActives);
        }

        public byte[] ExportMarketReportTerminalValueAnalysisToXlsx(IList<MarketReportTerminalValueAnalysis> marketReportTerminalValueAnalysiss)
        {
            var properties = new List<PropertyByName<MarketReportTerminalValueAnalysis>>()
           {
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户", p => p.TerminalName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户编码", p => p.TerminalCode),
                new PropertyByName<MarketReportTerminalValueAnalysis>("类型", p => p.TerminalTypeName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("未采购天数", p => p.NoPurchaseDays),
                new PropertyByName<MarketReportTerminalValueAnalysis>("采购次数", p => p.PurchaseNumber),
                new PropertyByName<MarketReportTerminalValueAnalysis>("采购额度", p => p.PurchaseAmount),
                new PropertyByName<MarketReportTerminalValueAnalysis>("所属片区", p => p.DistrictName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("未拜访天数", p => p.NoVisitDays),
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户价值排名", p => p.RFMScore),
            };

            return new PropertyManager<MarketReportTerminalValueAnalysis>(properties).ExportToXlsx(marketReportTerminalValueAnalysiss);
        }

        //客户流失预警
        public byte[] ExportMarketReportTerminalLossWarningToXlsx(IList<MarketReportTerminalValueAnalysis> marketReportTerminalLossWarnings)
        {
            var properties = new List<PropertyByName<MarketReportTerminalValueAnalysis>>()
           {
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户", p => p.TerminalName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户编码", p => p.TerminalCode),
                new PropertyByName<MarketReportTerminalValueAnalysis>("类型", p => p.TerminalTypeName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("未采购天数", p => p.NoPurchaseDays),
                new PropertyByName<MarketReportTerminalValueAnalysis>("采购次数", p => p.PurchaseNumber),
                new PropertyByName<MarketReportTerminalValueAnalysis>("采购额度", p => p.PurchaseAmount),
                new PropertyByName<MarketReportTerminalValueAnalysis>("所属片区", p => p.DistrictName),
                new PropertyByName<MarketReportTerminalValueAnalysis>("未拜访天数", p => p.NoVisitDays),
                new PropertyByName<MarketReportTerminalValueAnalysis>("客户价值排名", p => p.RFMScore),
            };

            return new PropertyManager<MarketReportTerminalValueAnalysis>(properties).ExportToXlsx(marketReportTerminalLossWarnings);
        }
        //铺市率报表
        public byte[] ExportMarketReportShopRateToXlsx(IList<MarketReportShopRate> marketReportShopRates)
        {
            var properties = new List<PropertyByName<MarketReportShopRate>>()
           {
                new PropertyByName<MarketReportShopRate>("购买商品", p => p.ProductName),
                new PropertyByName<MarketReportShopRate>("条形码", p => p.SmallBarCode),
                new PropertyByName<MarketReportShopRate>("单位换算", p => p.UnitConversion),
                new PropertyByName<MarketReportShopRate>("销售金额", p => p.SaleAmount),
                new PropertyByName<MarketReportShopRate>("退货金额", p => p.ReturnAmount),
                new PropertyByName<MarketReportShopRate>("门店数", p => p.DoorQuantity),
                new PropertyByName<MarketReportShopRate>("期内铺市率（期内）", p => p.InsideQuantity),
                new PropertyByName<MarketReportShopRate>("期内铺市率（减少）", p => p.DecreaseQuantity),
                new PropertyByName<MarketReportShopRate>("全局铺市率（期初）", p => p.BeginQuantity),
                new PropertyByName<MarketReportShopRate>("全局铺市率（增加）", p => p.AddQuantity),
                new PropertyByName<MarketReportShopRate>("全局铺市率（期末）", p => p.EndQuantity),
            };

            return new PropertyManager<MarketReportShopRate>(properties).ExportToXlsx(marketReportShopRates);
        }
        #endregion

        #endregion

        #region 提成

        public byte[] ExportStaffSaleQueryToXlsx(IList<StaffSaleQuery> staffSales, int store)
        {
            var properties = new List<PropertyByName<StaffSaleQuery>>()
            {
                new PropertyByName<StaffSaleQuery>("商品全称", p => p.Name),
                new PropertyByName<StaffSaleQuery>("规格", p => p.BigQuantity),
                new PropertyByName<StaffSaleQuery>("件数", p => p.Quantity),
                new PropertyByName<StaffSaleQuery>("单位", p => p.UnitName),
                new PropertyByName<StaffSaleQuery>("标准", p => (0)),
                new PropertyByName<StaffSaleQuery>("金额", p => (0)), 
                new PropertyByName<StaffSaleQuery>("交易时间", p => p.CreatedOnUtc.ToString("yyyy-MM-dd"))
            };
            return new PropertyManager<StaffSaleQuery>(properties).ExportToXlsx(staffSales);
        }

        public byte[] ExportVisitSummeryQueryToXlsx(IList<VisitSummeryQuery> summeryQueries, int store)
        {
            var properties = new List<PropertyByName<VisitSummeryQuery>>()
            {
                new PropertyByName<VisitSummeryQuery>("业务员", p => p.UserName),
                new PropertyByName<VisitSummeryQuery>("拜访量", p => p.VistCount),
                new PropertyByName<VisitSummeryQuery>("新增终端量", p => p.NewAddCount),
                new PropertyByName<VisitSummeryQuery>("门头照片", p => p.DoorheadPhotoCount),
                new PropertyByName<VisitSummeryQuery>("陈列照片", p => p.DisplayPhotoCount)
            };
            return new PropertyManager<VisitSummeryQuery>(properties).ExportToXlsx(summeryQueries);
        }

        #endregion
        #endregion

        #region
        /// <summary>
        /// 导出商品到 XLSX
        /// </summary>
        /// <param name="products">Products</param>
        public virtual byte[] ExportBusinessUserVisitOfYear(IEnumerable<BusinessUserVisitOfYear> bussinessVisit, int year, int month)
        {
            var days = DateTime.DaysInMonth(year, month);
            PropertyByName<BusinessUserVisitOfYear>[] properties = new PropertyByName<BusinessUserVisitOfYear>[days + 2];
            properties[0] = new PropertyByName<BusinessUserVisitOfYear>("业务员", p => p.UserName);
            properties[properties.Length - 1] = new PropertyByName<BusinessUserVisitOfYear>("合计", p => p.Total);
            properties[1] = new PropertyByName<BusinessUserVisitOfYear>(1 + "日", p => p.Days1);
            properties[2] = new PropertyByName<BusinessUserVisitOfYear>(2 + "日", p => p.Days2);
            properties[3] = new PropertyByName<BusinessUserVisitOfYear>(3 + "日", p => p.Days3);
            properties[4] = new PropertyByName<BusinessUserVisitOfYear>(4 + "日", p => p.Days4);
            properties[5] = new PropertyByName<BusinessUserVisitOfYear>(5 + "日", p => p.Days5);
            properties[6] = new PropertyByName<BusinessUserVisitOfYear>(6 + "日", p => p.Days6);
            properties[7] = new PropertyByName<BusinessUserVisitOfYear>(7 + "日", p => p.Days7);
            properties[8] = new PropertyByName<BusinessUserVisitOfYear>(8 + "日", p => p.Days8);
            properties[9] = new PropertyByName<BusinessUserVisitOfYear>(9 + "日", p => p.Days9);
            properties[10] = new PropertyByName<BusinessUserVisitOfYear>(10 + "日", p => p.Days10);
            properties[11] = new PropertyByName<BusinessUserVisitOfYear>(11 + "日", p => p.Days11);
            properties[12] = new PropertyByName<BusinessUserVisitOfYear>(12 + "日", p => p.Days12);
            properties[13] = new PropertyByName<BusinessUserVisitOfYear>(13 + "日", p => p.Days13);
            properties[14] = new PropertyByName<BusinessUserVisitOfYear>(14 + "日", p => p.Days14);
            properties[15] = new PropertyByName<BusinessUserVisitOfYear>(15 + "日", p => p.Days15);
            properties[16] = new PropertyByName<BusinessUserVisitOfYear>(16 + "日", p => p.Days16);
            properties[17] = new PropertyByName<BusinessUserVisitOfYear>(17 + "日", p => p.Days17);
            properties[18] = new PropertyByName<BusinessUserVisitOfYear>(18 + "日", p => p.Days18);
            properties[19] = new PropertyByName<BusinessUserVisitOfYear>(19 + "日", p => p.Days19);
            properties[20] = new PropertyByName<BusinessUserVisitOfYear>(20 + "日", p => p.Days20);
            properties[21] = new PropertyByName<BusinessUserVisitOfYear>(21 + "日", p => p.Days21);
            properties[22] = new PropertyByName<BusinessUserVisitOfYear>(22 + "日", p => p.Days22);
            properties[23] = new PropertyByName<BusinessUserVisitOfYear>(23 + "日", p => p.Days23);
            properties[24] = new PropertyByName<BusinessUserVisitOfYear>(24 + "日", p => p.Days24);
            properties[25] = new PropertyByName<BusinessUserVisitOfYear>(25 + "日", p => p.Days25);
            properties[26] = new PropertyByName<BusinessUserVisitOfYear>(26 + "日", p => p.Days26);
            properties[27] = new PropertyByName<BusinessUserVisitOfYear>(27 + "日", p => p.Days27);
            properties[28] = new PropertyByName<BusinessUserVisitOfYear>(28 + "日", p => p.Days28);
            if (days > 28) properties[29] = new PropertyByName<BusinessUserVisitOfYear>(29 + "日", p => p.Days29);
            if (days > 29) properties[30] = new PropertyByName<BusinessUserVisitOfYear>(30 + "日", p => p.Days30);
            if (days > 30) properties[31] = new PropertyByName<BusinessUserVisitOfYear>(31 + "日", p => p.Days31);
            var bussinessVisitList = bussinessVisit.ToList();

            var bussinessVisitMode = true;
            try
            {
                bussinessVisitMode = _genericAttributeService.GetAttribute<bool>(_workContext.CurrentUser, "product-advanced-mode");
            }
            catch (ArgumentNullException)
            {
            }
            return new PropertyManager<BusinessUserVisitOfYear>(properties).ExportToXlsx(bussinessVisitList);
        }
        #endregion
    }
}