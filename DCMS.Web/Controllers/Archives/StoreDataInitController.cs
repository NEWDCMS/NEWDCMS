using DCMS.Core;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
using DCMS.Services.Visit;
using DCMS.Services.WareHouses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace DCMS.Web.Controllers.Archives
{
    /// <summary>
    /// 用于期初数据导入
    /// </summary>
    public class StoreDataInitController : BasePublicController
    {
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IDistrictService _districtService;
        private readonly IChannelService _channelService;
        private readonly IRankService _rankService;
        private readonly ILineTierService _lineTierService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ITerminalService _terminalService;
        private readonly IStockService _stockService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IReceivableService _receivableService;


        public StoreDataInitController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            INotificationService notificationService,
            IExportManager exportManager,
            IImportManager importManager,
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IDistrictService districtService,
            IChannelService channelService,
            IRankService rankService,
            ILineTierService lineTierService,
            IManufacturerService manufacturerService,
            IWareHouseService wareHouseService,
            ITerminalService terminalService,
            IStockService stockService,
            ITrialBalanceService trialBalanceService, IReceivableService receivableService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _exportManager = exportManager;
            _importManager = importManager;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _districtService = districtService;
            _channelService = channelService;
            _rankService = rankService;
            _lineTierService = lineTierService;
            _manufacturerService = manufacturerService;
            _wareHouseService = wareHouseService;
            _terminalService = terminalService;
            _stockService = stockService;
            _trialBalanceService = trialBalanceService;
            _receivableService = receivableService;
        }

        public IActionResult Init()
        {
            return View();
        }

        /// <summary>
        /// 模板下载
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public IActionResult UploadDownTemplate(int order)
        {
            byte[] ms;
            switch (order)
            {
                case 1: //品牌信息
                    var brands = _brandService.GetAllBrands(2).Take(1);
                    ms = _exportManager.ExportBrandsToXlsx(brands);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "品牌信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "品牌信息.xlsx");
                    }
                case 2: //商品类别
                    var cattegories = _categoryService.GetAllCategories(2).Take(1);
                    ms = _exportManager.ExportCategoriesToXlsx(cattegories);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "商品类别.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "商品类别.xlsx");
                    }
                case 3: //片区信息
                    var districts = _districtService.GetAll(2).Take(1);
                    ms = _exportManager.ExportDistrictsToXlsx(districts);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "片区信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "片区信息.xlsx");
                    }
                case 4: //渠道信息
                    var channels = _channelService.GetAll(2).Take(1);
                    ms = _exportManager.ExportChannelsToXlsx(channels);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "渠道信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "渠道信息.xlsx");
                    }
                case 5: //终端等级
                    var ranks = _rankService.GetAll(2).Take(1);
                    ms = _exportManager.ExportRanksToXlsx(ranks);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "终端等级.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "终端等级.xlsx");
                    }
                case 6: //线路
                    var lines = _lineTierService.GetAll(2).Take(1);
                    ms = _exportManager.ExportLineTiersToXlsx(lines);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "线路信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "线路信息.xlsx");
                    }
                case 7: //供应商
                    var manufacturers = _manufacturerService.GetAllManufacturers().Take(1);
                    ms = _exportManager.ExportManufacturersToXlsx(manufacturers);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "供应商信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "供应商信息.xlsx");
                    }
                case 8: //仓库信息
                    var wareHouses = _wareHouseService.GetWareHouseList(2).Take(1);
                    ms = _exportManager.ExportWareHousesToXlsx(wareHouses);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "仓库信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "仓库信息.xlsx");
                    }
                case 9: //终端档案
                    var terminals = _terminalService.GetAllTerminal(2).Take(1);
                    ms = _exportManager.ExportTerminalsToXlsx(terminals);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "终端档案.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "终端档案.xlsx");
                    }
                case 10: //商品档案
                    var products = _productService.GetAllProducts(2).Take(1);
                    ms = _exportManager.ExportProductsToXlsx(products);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "商品档案.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "商品档案.xlsx");
                    }
                case 11: //商品库存初始
                    var stocks = _stockService.GetAllStocks(2).Take(1);
                    ms = _exportManager.ExportStocksToXlsx(stocks);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "库存信息.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "库存信息.xlsx");
                    }
                case 12: //期末余额初始
                    var trialBalances = _trialBalanceService.GetAll(2).Take(1);
                    ms = _exportManager.ExportTrialBalancesToXlsx(trialBalances);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "科目余额.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "科目余额.xlsx");
                    }
                case 13: //应收款期初余额
                    var Receivables = _receivableService.GetAll(2).Take(1);
                    ms = _exportManager.ExportReceivablesToXlsx(Receivables);
                    if (ms != null)
                    {
                        return File(ms, "application/vnd.ms-excel", "应收款期初余额.xlsx");
                    }
                    else
                    {
                        return File(new MemoryStream(), "application/vnd.ms-excel", "应收款期初余额.xlsx");
                    }
            }

            return RedirectToAction("Init");
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ImportInitData(IFormCollection form, int order)
        {
            try
            {
                int result = 0;
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    var fileName = file.FileName.Trim('"');



                    //获取程序根目录 
                    //string sFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
                    string filepath = AppContext.BaseDirectory + "\\App_Data\\TempUploads";
                    if (!Directory.Exists(filepath))
                    {
                        Directory.CreateDirectory(filepath);
                    }

                    fileName = filepath + $@"\{fileName}";//指定文件上传的路径
                    //FileInfo fileinfo = new FileInfo(fileName);
                    using (FileStream fs = System.IO.File.Create(fileName))//创建文件流
                    {
                        file.CopyTo(fs);//将上载文件的内容复制到目标流

                        if (IsAllowedExtension(fs, file))
                        {
                            switch (order)
                            {
                                case 1: //品牌信息
                                    result = _importManager.ImportBrandsFromXlsx(fs, curStore.Id);
                                    break;
                                case 2: //商品类别
                                    result = _importManager.ImportCategoriesFromXlsx(fs, curStore.Id);
                                    break;
                                case 3: //片区信息
                                    result = _importManager.ImportDistrictsFromXlsx(fs, curStore.Id);
                                    break;
                                case 4: //渠道信息
                                    result = _importManager.ImportChannelsFromXlsx(fs, curStore.Id);
                                    break;
                                case 5: //终端等级
                                    result = _importManager.ImportRanksFromXlsx(fs, curStore.Id);
                                    break;
                                case 6: //线路
                                    result = _importManager.ImportLinesFromXlsx(fs, curStore.Id);
                                    break;
                                case 7: //供应商
                                    result = _importManager.ImportManufacturersFromXlsx(fs, curStore.Id);
                                    break;
                                case 8: //仓库信息
                                    result = _importManager.ImportWareHousesFromXlsx(fs, curStore.Id);
                                    break;
                                case 9: //终端档案
                                    result = _importManager.ImportTerminalFromXlsx(fs, curStore.Id, curUser.Id, 0);
                                    break;
                                case 10: //商品档案
                                    result = _importManager.ImportProductsFromXlsx(fs, curStore.Id, 0);
                                    break;
                                case 11: //商品库存初始
                                    result = _importManager.ImportStocksFromXlsx(fs, curStore.Id, curUser.Id);
                                    break;
                                case 12: //期末余额初始
                                    result = _importManager.ImportTrialBalancesFromXlsx(fs, curStore.Id);
                                    break;
                                case 13: //应收款期初余额
                                    result = _importManager.ImportReceivablesFromXlsx(fs, curStore.Id, curUser.Id);
                                    break;
                            }
                        }
                        else
                        {
                            return Warning("您上传文件类型有误，请重新选择!");
                        }

                        //fs.Flush();//清除此流的缓冲区并导致将任何缓冲数据写入
                    }

                    System.IO.File.Delete(fileName);

                }

                if (result < 0)
                {
                    return Warning("您上传的模板有误，请重新选择!");
                }

                return Successful("文件上传成功!");
            }
            catch (Exception ex)
            {
                return Warning(ex.Message + ";" + ex.StackTrace);
            }

        }

        //真正判断文件类型的关键函数
        public static bool IsAllowedExtension(FileStream fs, IFormFile file)
        {
            BinaryReader r = new BinaryReader(fs);
            r.BaseStream.Seek(0, SeekOrigin.Begin);
            string fileclass = "";
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                fileclass = buffer.ToString();
                buffer = r.ReadByte();
                fileclass += buffer.ToString();
            }
            catch
            {
            }

            //r.Close();

            if (file.ContentType.ToLower() == "application/vnd.ms-excel" || file.ContentType.ToLower() == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                if (fileclass == "208207" || fileclass == "8075")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}