using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.CRM;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.ExportImport.Help;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.Services.WareHouses;
using Microsoft.AspNetCore.StaticFiles;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMS.Services.ExportImport
{
    /// <summary>
    /// 导入管理器
    /// </summary>
    public partial class ImportManager : BaseService, IImportManager
    {
        #region Constants

        //区分对象
        private const string IMAGE_HASH_ALGORITHM = "SHA1";
        private const string UPLOADS_TEMP_PATH = "~/App_Data/TempUploads";

        #endregion

        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IBrandService _brandService;
        private readonly IDistrictService _districtService;
        private readonly ILineTierService _lineTierService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IAccountingService _accountingService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        #endregion

        #region Ctor

        public ImportManager(
            IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            ICategoryService categoryService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IBrandService brandService,
            IDistrictService districtService,
            ILineTierService lineTierService,
            IChannelService channelService,
            IRankService rankService,
            IWareHouseService wareHouseService,
            IAccountingService accountingService, ITerminalService terminalService, IUserService userService) : base(getter, cacheManager, eventPublisher)
        {
            _categoryService = categoryService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _brandService = brandService;
            _districtService = districtService;
            _lineTierService = lineTierService;
            _channelService = channelService;
            _rankService = rankService;
            _wareHouseService = wareHouseService;
            _accountingService = accountingService;
            _terminalService = terminalService;
            _userService = userService;
        }

        #endregion

        #region Utilities

        private static ExportedAttributeType GetTypeOfExportedAttribute(ExcelWorksheet worksheet, PropertyManager<ExportProductAttribute> productAttributeManager, PropertyManager<ExportSpecificationAttribute> specificationAttributeManager, int iRow)
        {
            productAttributeManager.ReadFromXlsx(worksheet, iRow, ExportProductAttribute.ProducAttributeCellOffset);

            if (productAttributeManager.IsCaption)
            {
                return ExportedAttributeType.ProductAttribute;
            }

            specificationAttributeManager.ReadFromXlsx(worksheet, iRow, ExportProductAttribute.ProducAttributeCellOffset);

            if (specificationAttributeManager.IsCaption)
            {
                return ExportedAttributeType.SpecificationAttribute;
            }

            return ExportedAttributeType.NotSpecified;
        }

        private static void SetOutLineForSpecificationAttributeRow(object cellValue, ExcelWorksheet worksheet, int endRow)
        {
            var attributeType = (cellValue ?? string.Empty).ToString();

            if (attributeType.Equals("AttributeType", StringComparison.InvariantCultureIgnoreCase))
            {
                worksheet.Row(endRow).OutlineLevel = 1;
            }
            else
            {
                worksheet.Row(endRow).OutlineLevel = 1;
            }
        }

        private static void CopyDataToNewFile(ImportProductMetadata metadata, ExcelWorksheet worksheet, string filePath, int startRow, int endRow, int endCell)
        {
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handles to the worksheets
                    var outWorksheet = xlPackage.Workbook.Worksheets.Add(typeof(Product).Name);
                    metadata.Manager.WriteCaption(outWorksheet);
                    var outRow = 2;
                    for (var row = startRow; row <= endRow; row++)
                    {
                        outWorksheet.Row(outRow).OutlineLevel = worksheet.Row(row).OutlineLevel;
                        for (var cell = 1; cell <= endCell; cell++)
                        {
                            outWorksheet.Cells[outRow, cell].Value = worksheet.Cells[row, cell].Value;
                        }

                        outRow += 1;
                    }

                    xlPackage.Save();
                }
            }
        }

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            for (var i = 0; i < properties.Length; i++)
            {
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i + 1; //excel indexes start from 1
                }
            }

            return 0;
        }

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }




        #endregion

        #region Methods


        public static IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(ExcelWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Cells[1, poz];

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                    {
                        break;
                    }

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }

        /// <summary>
        /// 从 Xlsx 导入商品
        /// </summary>
        /// <param name="stream"></param>
        public virtual int ImportProductsFromXlsx(Stream stream, int store, int categoryId)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 31, "商品名称"))
                {
                    //var properties = GetPropertiesByExcelCells<Product>(worksheet);

                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                                                                //List<Product> products = new List<Product>();
                    List<ProductSpecificationAttribute> productSpecificationAttributes = new List<ProductSpecificationAttribute>(); //商品规格属性


                    for (int row = 2; row <= rowCount; row++)
                    {
                        var product = new Product
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    product.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    product.MnemonicCode = CommonHelper.GenerateStrchar(8) + "_" + worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 3:
                                    product.CategoryId = _categoryService.GetCategoryId(store, worksheet.Cells[row, col]?.Value.ToString());
                                    break;
                                case 4:
                                    product.BrandId = _brandService.GetBrandId(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 5:
                                    product.SmallUnitId = _specificationAttributeService.GetSpecificationAttributeOptionId(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 6:
                                    product.StrokeUnitId = _specificationAttributeService.GetSpecificationAttributeOptionId(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 7:
                                    product.BigUnitId = _specificationAttributeService.GetSpecificationAttributeOptionId(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 8:
                                    product.ProductCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 9:
                                    product.Specification = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 11:
                                    product.CountryOrigin = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 12:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int s);
                                    product.Supplier = s;
                                    break;
                                case 13:
                                    product.OtherBarCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 14:
                                    product.OtherBarCode1 = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 15:
                                    product.OtherBarCode2 = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 16:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int st);
                                    product.StatisticalType = st;
                                    break;
                                case 17:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int ex);
                                    product.ExpirationDays = ex;
                                    break;
                                case 18:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int ad);
                                    product.AdventDays = ad;
                                    break;
                                case 19:
                                    product.IsFlavor = worksheet.Cells[row, col].Value?.ToString() == "1" ? true : false;
                                    break;
                                case 20:
                                    product.SmallBarCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 21:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int b);
                                    product.StrokeQuantity = b;
                                    break;
                                case 22:
                                    product.StrokeBarCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 23:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int bg);
                                    product.BigQuantity = bg;
                                    break;
                                case 24:
                                    product.BigBarCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 25:
                                    product.Sku = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 26:
                                    product.ManufacturerPartNumber = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 30:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int soq);
                                    product.StockQuantity = soq;
                                    break;
                                case 31:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int mq);
                                    product.MinStockQuantity = mq;
                                    break;
                            }
                        }

                        product.UpdatedOnUtc = DateTime.Now;
                        product.CreatedOnUtc = DateTime.Now;
                        product.Deleted = false;
                        product.Published = true;
                        product.DisplayOrder = 0;
                        product.Status = true; //状态
                        product.IsAdjustPrice = true;
                        product.IsManufactureDete = true;


                        ////其他信息
                        product.HasSold = false; //是否开单
                        product.ManageInventoryMethodId = 0;
                        product.DisablePlaceButton = false;
                        product.AllowBackInStockSubscriptions = false;
                        product.BackorderModeId = 0;
                        product.NotifyAdminForQuantityBelow = 0;
                        product.LowStockActivityId = 0;
                        product.DisplayStockQuantity = true;
                        product.DisplayStockAvailability = true;
                        product.RequireOtherProducts = false;
                        product.ERPProductId = 0;

                        //products.Add(product);
                        var uow = ProductsRepository.UnitOfWork;
                        ProductsRepository.Insert(product);
                        uow.SaveChanges();

                        _eventPublisher.EntityInserted(product);

                        if (product.SmallUnitId > 0)
                        {
                            var smallPSpec = new ProductSpecificationAttribute()
                            {
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.SmallUnitId,
                                CustomValue = "小",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(smallPSpec);
                        }

                        //中单位
                        if (product.StrokeUnitId > 0)
                        {
                            var strokePSpec = new ProductSpecificationAttribute()
                            {
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.StrokeUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(store).FirstOrDefault().Id,
                                CustomValue = "中",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(strokePSpec);
                        }
                        //大单位
                        if (product.BigUnitId > 0)
                        {
                            var bigSpec = new ProductSpecificationAttribute()
                            {
                                ProductId = product.Id,
                                SpecificationAttributeOptionId = product.BigUnitId ?? _specificationAttributeService.GetSpecificationAttributeOptionsByStore(store).FirstOrDefault().Id,
                                CustomValue = "大",
                                AllowFiltering = true,
                                ShowOnProductPage = true,
                                DisplayOrder = 0
                            };
                            productSpecificationAttributes.Add(bigSpec);
                        }
                    }

                    //更新商品规格属性映射
                    if (productSpecificationAttributes != null && productSpecificationAttributes.Count > 0)
                    {
                        var uow1 = ProductsRepository.UnitOfWork;
                        ProductsSpecificationAttributeMappingRepository.Insert(productSpecificationAttributes);
                        uow1.SaveChanges();
                    }

                    result = 1;
                }
                else
                {
                    result = -1;
                }

            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入客户
        /// </summary>
        /// <param name="stream"></param>
        public virtual int ImportTerminalFromXlsx(Stream stream, int store, int userId, int distrctid)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 21, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Terminal> terminals = new List<Terminal>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var terminal = new Terminal
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    terminal.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    terminal.MnemonicName = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 3:
                                    terminal.BossName = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 4:
                                    terminal.BossCall = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 5:
                                    terminal.Status = true;
                                    break;
                                case 6:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    terminal.MaxAmountOwed = a;
                                    break;
                                case 7:
                                    terminal.Code = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 8:
                                    terminal.Address = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 9:
                                    terminal.Remark = (worksheet.Cells[row, col].Value == null) ? "NULL" : worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 10:
                                    terminal.DistrictId = _districtService.GetDistrictByName(store, worksheet.Cells[row, col].Value.ToString()) == 0 ? distrctid : _districtService.GetDistrictByName(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 11:
                                    terminal.ChannelId = _channelService.GetChannelByName(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 12:
                                    terminal.LineId = _lineTierService.GetLineTierByName(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 13:
                                    terminal.RankId = _rankService.GetRankByName(store, worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 14:
                                    terminal.PaymentMethod = worksheet.Cells[row, col].Value?.ToString().Trim() == "1" ? 1 : 2;
                                    break;
                                case 15:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int lng);
                                    terminal.Location_Lng = lng;
                                    break;
                                case 16:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int lat);
                                    terminal.Location_Lat = lat;
                                    break;
                                case 17:
                                    terminal.BusinessNo = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 18:
                                    terminal.FoodBusinessLicenseNo = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 19:
                                    terminal.EnterpriseRegNo = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                //case 20:
                                //    terminal.StoreCode = worksheet.Cells[row, col].Value?.ToString();
                                //    break;
                                //case 21:
                                //    terminal.StoreName = worksheet.Cells[row, col].Value?.ToString();
                                //    break;
                            }
                        }

                        terminal.CreatedUserId = userId;
                        terminal.CreatedOnUtc = DateTime.Now;
                        terminal.Deleted = false;
                        terminal.Related = false;
                        terminals.Add(terminal);
                    }

                    //更新终端信息
                    var uow = TerminalsRepository.UnitOfWork;
                    TerminalsRepository.Insert(terminals);
                    uow.SaveChanges();

                    terminals.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }

            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入品牌
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportBrandsFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 3, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Brand> brands = new List<Brand>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var brand = new Brand
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    brand.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    brand.Status = worksheet.Cells[row, col].Value?.ToString() == "1" ? true : false;
                                    break;
                                case 3:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    brand.DisplayOrder = a;
                                    break;
                            }
                        }

                        brand.CreatedOnUtc = DateTime.Now;
                        brands.Add(brand);
                    }

                    var uow = BrandsRepository.UnitOfWork;
                    BrandsRepository.Insert(brands);
                    uow.SaveChanges();

                    brands.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入商品类别
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportCategoriesFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 7, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Category> categories = new List<Category>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var category = new Category
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    category.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int p);
                                    category.ParentId = p == 0 ? _categoryService.GetCategoriesMinId(store) : p;
                                    break;
                                case 3:
                                    category.PathCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 4:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    category.StatisticalType = a;
                                    break;
                                case 5:
                                    category.BrandId = _brandService.GetBrandId(store, worksheet.Cells[row, col].Value?.ToString());
                                    category.BrandName = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 6:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int s);
                                    category.Status = s;
                                    break;
                                case 7:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int o);
                                    category.OrderNo = o;
                                    break;
                            }
                        }

                        category.Deleted = false;
                        category.Published = true;
                        category.PercentageId = 0;

                        //更新终端信息
                        categories.Add(category);
                    }

                    var uow = CategoriesRepository.UnitOfWork;
                    CategoriesRepository.Insert(categories);
                    uow.SaveChanges();

                    categories.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }

            }
            return result;
        }

        /// <summary>
        /// 从XLSX导入片区
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportDistrictsFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 4, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<District> districts = new List<District>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var district = new District
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    district.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int p);
                                    district.ParentId = p;
                                    break;
                                case 3:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int d);
                                    district.OrderNo = d;
                                    break;
                                case 4:
                                    district.Describe = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                            }

                            district.Deleted = false;


                        }

                        //更新终端信息
                        districts.Add(district);
                    }

                    var uow = DistrictsRepository.UnitOfWork;
                    DistrictsRepository.Insert(districts);
                    uow.SaveChanges();

                    districts.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入渠道信息
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportChannelsFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 4, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Channel> channels = new List<Channel>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var channel = new Channel
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    channel.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int d);
                                    channel.OrderNo = d;
                                    break;
                                case 3:
                                    channel.Describe = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 4:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    channel.Attribute = a;
                                    break;
                            }
                        }

                        channel.Deleted = false;
                        channel.CreateDate = DateTime.Now;


                        //更新终端信息
                        channels.Add(channel);
                    }

                    var uow = ChannelsRepository.UnitOfWork;
                    ChannelsRepository.Insert(channels);
                    uow.SaveChanges();

                    channels.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }
        /// <summary>
        /// 从XLSX导入终端等级
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportRanksFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 2, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Rank> ranks = new List<Rank>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var rank = new Rank
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    rank.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    rank.Describe = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                            }
                        }

                        rank.Deleted = false;
                        rank.CreateDate = DateTime.Now;

                        ranks.Add(rank);
                    }

                    var uow = RanksRepository.UnitOfWork;
                    RanksRepository.Insert(ranks);
                    uow.SaveChanges();

                    ranks.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入线路
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportLinesFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 2, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<LineTier> lineTiers = new List<LineTier>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var lineTier = new LineTier
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    lineTier.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    lineTier.Enabled = worksheet.Cells[row, col].Value?.ToString() == "1" ? true : false;
                                    break;
                            }
                        }


                        lineTiers.Add(lineTier);
                    }
                    var uow = LineTiersRepository.UnitOfWork;
                    LineTiersRepository.Insert(lineTiers);
                    uow.SaveChanges();

                    lineTiers.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入供应商
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportManufacturersFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 9, "名称"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Manufacturer> manufacturers = new List<Manufacturer>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var manufacturer = new Manufacturer
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    manufacturer.Name = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    manufacturer.Description = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 3:
                                    manufacturer.MnemonicName = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 4:
                                    manufacturer.ContactName = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 5:
                                    manufacturer.ContactPhone = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 6:
                                    manufacturer.Address = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 7:
                                    manufacturer.Status = worksheet.Cells[row, col].Value?.ToString() == "0" ? false : true;
                                    break;
                                case 8:
                                    manufacturer.PriceRanges = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 9:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    manufacturer.DisplayOrder = a;
                                    break;
                            }
                        }

                        manufacturer.Deleted = false;
                        manufacturer.CreatedOnUtc = DateTime.Now;
                        manufacturer.UpdatedOnUtc = DateTime.Now;

                        manufacturers.Add(manufacturer);
                    }

                    var uow = ManufacturerRepository.UnitOfWork;
                    ManufacturerRepository.Insert(manufacturers);
                    uow.SaveChanges();

                    manufacturers.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }
        /// <summary>
        /// 从XLSX导入仓库信息
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportWareHousesFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 4, "仓库编号"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<WareHouse> wareHouses = new List<WareHouse>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var wareHouse = new WareHouse
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    wareHouse.Code = worksheet.Cells[row, col].Value.ToString();
                                    break;
                                case 2:
                                    wareHouse.Name = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 3:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int a);
                                    wareHouse.Type = a;
                                    break;
                                case 4:
                                    wareHouse.Status = worksheet.Cells[row, col].Value?.ToString() == "1" ? true : false;
                                    break;
                            }
                        }


                        wareHouse.CreatedUserId = store;
                        wareHouse.CreatedOnUtc = DateTime.Now;
                        wareHouse.Deleted = false;


                        wareHouses.Add(wareHouse);
                    }

                    var uow = WareHousesRepository.UnitOfWork;
                    WareHousesRepository.Insert(wareHouses);
                    uow.SaveChanges();

                    wareHouses.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }
        /// <summary>
        /// 从XLSX导入库存信息
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportStocksFromXlsx(Stream stream, int store, int userId)
        {
            int result = 0;


            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 6, "仓库ID"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<Stock> stocks = new List<Stock>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var stock = new Stock
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    int.TryParse(worksheet.Cells[row, col].Value.ToString(), out int wareHouseId);
                                    stock.WareHouseId = wareHouseId;
                                    break;
                                case 2:
                                    int.TryParse(worksheet.Cells[row, col].Value.ToString(), out int productId);
                                    stock.ProductId = productId;
                                    break;
                                case 3:
                                    stock.PositionCode = worksheet.Cells[row, col].Value?.ToString();
                                    break;
                                case 4:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int uq);
                                    stock.UsableQuantity = uq;
                                    break;
                                case 5:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int cq);
                                    stock.CurrentQuantity = cq;
                                    break;
                                case 6:
                                    int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int lq);
                                    stock.LockQuantity = lq;
                                    break;
                            }
                        }

                        stock.CreaterId = userId;
                        stock.CreateTime = DateTime.Now;
                        stock.UpdaterId = userId;
                        stock.UpdateTime = DateTime.Now;
                        stock.TimeStamp = DateTime.Now;

                        stocks.Add(stock);
                    }

                    var uow = StocksRepository.UnitOfWork;
                    StocksRepository.Insert(stocks);
                    uow.SaveChanges();

                    stocks.ForEach(s => { _eventPublisher.EntityInserted(s); });
                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 从XLSX导入会计科目余额
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportTrialBalancesFromXlsx(Stream stream, int store)
        {
            int result = 0;
            using (var xlPackage = new ExcelPackage(stream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new DCMSException("No worksheet found");
                }

                if (CheckExcelTemplate(worksheet, 9, "科目类别"))
                {
                    int rowCount = worksheet.Dimension.Rows; //行数
                    int ColCount = worksheet.Dimension.Columns; //列数
                    List<TrialBalance> trialBalances = new List<TrialBalance>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var trialBalance = new TrialBalance
                        {
                            StoreId = store
                        };

                        for (int col = 1; col <= ColCount; col++)
                        {
                            switch (col)
                            {
                                case 1:
                                    trialBalance.AccountingTypeId = _accountingService.GetAccountingTypeByName(worksheet.Cells[row, col].Value.ToString())?.Id ?? 0;
                                    break;
                                case 2:
                                    trialBalance.AccountingOptionId = _accountingService.GetAccountingOptionByName(store, worksheet.Cells[row, col].Value?.ToString())?.Id ?? 0;
                                    break;
                                case 3:
                                    trialBalance.PeriodDate = worksheet.Cells[row, col].Value?.ToString() == "0" ? DateTime.Now : DateTime.Parse(worksheet.Cells[row, col].Value?.ToString());
                                    break;
                                case 4:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal idt);
                                    trialBalance.InitialBalanceDebit = idt;
                                    break;
                                case 5:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal ict);
                                    trialBalance.InitialBalanceCredit = ict;
                                    break;
                                case 6:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal pdt);
                                    trialBalance.PeriodBalanceDebit = pdt;
                                    break;
                                case 7:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal pct);
                                    trialBalance.PeriodBalanceCredit = pct;
                                    break;
                                case 8:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal edt);
                                    trialBalance.EndBalanceDebit = edt;
                                    break;
                                case 9:
                                    decimal.TryParse(worksheet.Cells[row, col].Value?.ToString(), out decimal ect);
                                    trialBalance.EndBalanceCredit = ect;
                                    break;
                            }
                        }

                        trialBalance.CreatedOnUtc = DateTime.Now;

                        trialBalances.Add(trialBalance);
                    }
                    var uow = TrialBalancesRepository.UnitOfWork;
                    TrialBalancesRepository.Insert(trialBalances);
                    uow.SaveChanges();

                    result = 1;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }

        /// <summary>
        /// 从XLSX导入应收款期初余额
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="store"></param>
        public virtual int ImportReceivablesFromXlsx(Stream stream, int store, int userId)
        {
            try
            {
                int result = 0;
                using (var xlPackage = new ExcelPackage(stream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new DCMSException("No worksheet found");
                    }

                    if (CheckExcelTemplate(worksheet, 5, "终端名称"))
                    {
                        int rowCount = worksheet.Dimension.Rows; //行数
                        int ColCount = worksheet.Dimension.Columns; //列数
                        List<Receivable> trialReceivable = new List<Receivable>();
                        List<SaleBill> saleBills = new List<SaleBill>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var Receivables = new Receivable
                            {
                                StoreId = store
                            };

                            for (int col = 1; col <= ColCount; col++)
                            {
                                switch (col)
                                {
                                    case 1:
                                        Receivables.TerminalId = _terminalService.GetTerminalIds(store, worksheet.Cells[row, col].Value.ToString(), true).FirstOrDefault();
                                        break;
                                    case 2:
                                        int.TryParse(worksheet.Cells[row, col].Value?.ToString(), out int uq);
                                        Receivables.OweCash = uq;
                                        break;
                                    case 3:
                                        var u = _userService.GetUserByUsername(store, worksheet.Cells[row, col].Value.ToString(), true);
                                        Receivables.OperationUserId = u != null ? u.Id : 0;
                                        break;
                                    case 4:
                                        var flag = double.TryParse(worksheet.Cells[row, col].Value?.ToString(),out double dateNum);
                                        Receivables.BalanceDate = worksheet.Cells[row, col].Value?.ToString() == "0" ? DateTime.Now : (flag ? HSSFDateUtil.GetJavaDate(dateNum) : DateTime.Parse(worksheet.Cells[row, col].Value?.ToString()));
                                        break;
                                    case 5:
                                        Receivables.Remark = worksheet.Cells[row, col].Value?.ToString();
                                        break;
                                }
                                //Receivables.OperationUserId = userId;
                                //Receivables.CreatedUserId = userId;
                                //Receivables.Deleted = false;
                                //Receivables.Status = false;
                                //Receivables.CreatedOnUtc = DateTime.Now;
                            }
                            //Receivables.OperationUserId = userId;
                            Receivables.CreatedUserId = userId;
                            Receivables.Deleted = false;
                            Receivables.Status = false;
                            Receivables.Inited = true;
                            Receivables.CreatedOnUtc = DateTime.Now;

                            if (ReceivablesRepository.Table.Where(r => r.TerminalId == Receivables.TerminalId && r.OweCash == Receivables.OweCash && r.OperationUserId == Receivables.OperationUserId && r.BalanceDate == Receivables.BalanceDate && r.Deleted == false).Count() > 0)
                            {
                                continue;
                            }
                            trialReceivable.Add(Receivables);

                            var sale = new SaleBill
                            {
                                StoreId = store,
                                TerminalId = Receivables.TerminalId,
                                BusinessUserId = Receivables.OperationUserId
                            };
                            sale.BillNumber = sale.GenerateNumber();
                            sale.OweCash = Receivables.OweCash;
                            sale.Remark = "应收款销售单/应收款期初备注";
                            sale.MakeUserId = userId;
                            sale.CreatedOnUtc = DateTime.Now;

                            sale.DeliveryUserId = 0;
                            sale.DepartmentId = 0;
                            sale.DistrictId = 0;
                            sale.WareHouseId = 0;
                            sale.PayTypeId = 0;
                            sale.TransactionDate = DateTime.Now;
                            sale.IsMinUnitSale = false;
                            sale.DefaultAmountId = "";
                            sale.PaymentMethodType = 0;
                            sale.SumAmount = 0;
                            sale.ReceivableAmount = 0;
                            sale.PreferentialAmount = 0;
                            sale.SumCostAmount = 0;
                            sale.SumProfit = 0;
                            sale.SumCostProfitRate = 0;
                            sale.SumCostPrice = 0;
                            sale.PrintNum = 0;
                            sale.ReceiptStatus = 0;
                            sale.Operation = 0;
                            sale.VoucherId = 0;
                            sale.TaxAmount = 0;
                            sale.AuditedUserId = 0;
                            sale.AuditedStatus = true;
                            sale.AuditedDate = DateTime.Now;
                            sale.ReversedStatus = false;
                            sale.ReversedUserId = 0;

                            saleBills.Add(sale);
                        }

                        var uow = ReceivablesRepository.UnitOfWork;
                        ReceivablesRepository.Insert(trialReceivable);
                        SaleBillsRepository.Insert(saleBills);
                        uow.SaveChanges();

                        result = 1;
                    }
                    else
                    {
                        result = -1;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return -1;
            }

        }
        #endregion

        private bool CheckExcelTemplate(ExcelWorksheet excelWorksheet, int colNum, string firstColName)
        {
            if (excelWorksheet.Dimension.Columns != colNum)
            {
                return false;
            }

            if (((object[,])excelWorksheet.Cells.Value)[0, 0].ToString() != firstColName)
            {
                return false;
            }

            return true;
        }

        public virtual int ImportTerminals(List<Terminal> terminals, string storeCode, string storeName)
        {
            try
            {
                int result = 0;
                if (!string.IsNullOrWhiteSpace(storeCode) && !string.IsNullOrWhiteSpace(storeName))
                {

                    var uow = TerminalsRepository.UnitOfWork;
                    TerminalsRepository.Insert(terminals);
                    //terminal.Code = storeCode + terminal.Id;
                    //TerminalsRepository.Update(terminals);

                    uow.SaveChanges();

                    //foreach (var terminal in terminals) 
                    //{
                    //    //if (!string.IsNullOrWhiteSpace(terminal.Code)) 
                    //    //{
                    //        //terminal.Id = TerminalsRepository.Table.Where(t => t.Code == terminal.Code && t.Deleted == false).Select(t => t.Id).FirstOrDefault();
                    //        //判断终端是否存在
                    //        if (terminal.Id > 0)
                    //        {
                    //            #region 暂时关闭关系
                    //            /*
                    //            //查询是否有手动加入的关系，终端关系id
                    //            var relationid1 = CRM_RELATIONRepository.Table.Where(tr => tr.TerminalId == terminal.Id && tr.StoreId == terminal.StoreId).Select(tr => tr.Id).FirstOrDefault();
                    //            if (!(relationid1 > 0))
                    //            {
                    //                //查询经销商，终端关系id
                    //                var relation = CRM_RELATIONRepository.Table.Where(tr => tr.PARTNER1 == terminal.Code && tr.PARTNER2 == storeCode).Select(r => new CRM_RELATION() { 
                    //                    Id = r.Id,
                    //                    TerminalId = r.TerminalId,
                    //                StoreId = r.StoreId,
                    //                PARTNER1 = r.PARTNER1,
                    //                PARTNER2 = r.PARTNER2,
                    //                CreatedOnUtc = r.CreatedOnUtc,
                    //                RELTYP = r.RELTYP,
                    //                ZUPDMODE = r.ZUPDMODE,
                    //                ZDATE = DateTime.Now
                    //            }).FirstOrDefault();
                    //                //如果经销商，终端关系存在更新 两个对应的id
                    //                if (relation != null && relation.Id > 0)
                    //                {
                    //                    relation.TerminalId = terminal.Id;
                    //                    relation.StoreId = terminal.StoreId;
                    //                    var uow = CRM_RELATIONRepository.UnitOfWork;
                    //                    CRM_RELATIONRepository.Update(relation);
                    //                    uow.SaveChanges();

                    //                    //通知
                    //                    _eventPublisher.EntityUpdated(relation);
                    //                }
                    //                //如果经销商，终端关系不存在插入关系
                    //                else
                    //                {
                    //                    relation = new CRM_RELATION();
                    //                    relation.TerminalId = terminal.Id;
                    //                    relation.StoreId = terminal.StoreId;
                    //                    relation.PARTNER1 = terminal.Code;
                    //                    relation.PARTNER2 = storeCode;
                    //                    var uow = CRM_RELATIONRepository.UnitOfWork;
                    //                    CRM_RELATIONRepository.Insert(relation);
                    //                    uow.SaveChanges();

                    //                    //通知
                    //                    _eventPublisher.EntityInserted(relation);
                    //                }
                    //            }
                    //            */
                    //            #endregion
                    //        }
                    //        else
                    //        {
                    //            var uow = TerminalsRepository.UnitOfWork;

                    //            TerminalsRepository.Insert(terminal);
                    //            terminal.Code = storeCode + terminal.Id;
                    //            TerminalsRepository.Update(terminal);

                    //            uow.SaveChanges();

                    //            //通知
                    //            _eventPublisher.EntityInserted(terminal);
                    //        }
                    //    //}
                    //}
                }
                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}