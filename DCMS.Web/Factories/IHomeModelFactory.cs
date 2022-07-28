using DCMS.ViewModel.Models.Home;

namespace DCMS.Web.Factories
{
    public partial interface IHomeModelFactory
    {
        DashboardModel PrepareDashboardModel(DashboardModel model);
    }
}