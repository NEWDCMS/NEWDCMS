using DCMS.Core.Domain.Census;
using System.Collections.Generic;

namespace DCMS.Services.Census
{
    public interface ISalesProductService
    {
        IList<SalesProduct> GetSalesProductsByRestaurantId(int restaurantId = 0);
        IList<SalesProduct> GetSalesProductsByTraditionId(int traditionId = 0);
        int Insert(SalesProduct saleReservationsProduct);
    }
}