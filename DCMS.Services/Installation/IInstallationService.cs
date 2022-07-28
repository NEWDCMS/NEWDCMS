using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Stores;

namespace DCMS.Services.Installation
{

    public interface IInstallationService
    {
        ProcessStatus InstallAccounting(int storeId); //会计科目
        ProcessStatus InstallCategories(int storeId); //商品类别
        ProcessStatus InstallManufacturers(int storeId); //供应商
        ProcessStatus InstallBrands(int storeId); //品牌
        ProcessStatus InstallCompanySettings(Store store, string path); //公司配置
        ProcessStatus InstallAppPrintSettings(Store store, string path); //APP打印配置
        ProcessStatus InstallPCPrintSettings(Store store, string path); //PC打印配置
        ProcessStatus InstallProductSettings(Store store, string path); //商品配置
        ProcessStatus InstallFinanceSettings(Store store, string path); //财务配置
        ProcessStatus InstallPercentage(int storeId);
        ProcessStatus InstallRemarkConfigs(int storeId, string path); //备注配置
        ProcessStatus InstallStandardPermissions(int storeId); //标准权限
        ProcessStatus InstallStores(string storeName, out int storeId); //经销商
        ProcessStatus InstallUsers(int storeId, string defaultUserName, string defaultUserEmail, string defaultUserPassword, string defaultMobileNumber); //用户
        ProcessStatus InstallWarehouses(int storeId); //仓库
        ProcessStatus InstallPrintTemplate(int storeId);
        ProcessStatus InstallProductUnit(int storeId); //商品单位
        ProcessStatus InstallDistrictTemplate(int storeId);
        void RollBackInstall(int storeId); //回滚
        void ConfirmCompletion(int storeId);

    }
}