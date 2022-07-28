using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Common
{
    public partial class CommonStatisticsModel : BaseModel
    {
        public int NumberOfOrders { get; set; }

        public int NumberOfCustomers { get; set; }

        public int NumberOfPendingReturnRequests { get; set; }

        public int NumberOfLowStockProducts { get; set; }
    }
}