using DCMS.Core.Domain.Terminals;
using System.Collections.Generic;
using System.IO;

namespace DCMS.Services.ExportImport
{
    public interface IImportManager
    {
        int ImportProductsFromXlsx(Stream stream, int store, int categoryId); //商品档案
        int ImportTerminalFromXlsx(Stream stream, int store, int userId, int distrctid); //终端档案
        int ImportBrandsFromXlsx(Stream stream, int store); //品牌信息
        int ImportCategoriesFromXlsx(Stream stream, int store); //商品类别
        int ImportDistrictsFromXlsx(Stream stream, int store); //片区信息
        int ImportChannelsFromXlsx(Stream stream, int store); //渠道信息
        int ImportRanksFromXlsx(Stream stream, int store); //终端等级
        int ImportLinesFromXlsx(Stream stream, int store); //线路信息
        int ImportManufacturersFromXlsx(Stream stream, int store); //供应商信息
        int ImportWareHousesFromXlsx(Stream stream, int store); //仓库信息
        int ImportStocksFromXlsx(Stream stream, int store, int userId); //初始库存
        int ImportTrialBalancesFromXlsx(Stream stream, int store); //科目余额
        int ImportReceivablesFromXlsx(Stream stream, int store, int userId); //应收款期初余额
        int ImportTerminals(List<Terminal> terminals, string storeCode,string storeName);
    }
}