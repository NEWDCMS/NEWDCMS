using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Users;
using System.Collections.Generic;

namespace DCMS.Web.Factories
{
    public interface ICommonModelFactory
    {
        CommonStatisticsModel PrepareCommonStatisticsModel();
        SystemInfoModel PrepareSystemInfoModel(SystemInfoModel model);
        IList<SystemWarningModel> PrepareSystemWarningModels();
        UrlRecordListModel PrepareUrlRecordListModel(UrlRecordSearchModel searchModel);
        UrlRecordSearchModel PrepareUrlRecordSearchModel(UrlRecordSearchModel searchModel);
        FaviconAndAppIconsModel PrepareFaviconAndAppIconsModel();
        UserModel LoginStates();
        MenuModel LeftSidebar();
    }
}