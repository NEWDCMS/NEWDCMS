using DCMS.Core;
using DCMS.Core.Domain.Sales;

namespace DCMS.Services.Sales
{
    public interface IChangeReservationBillService
    {

        BaseResult BillCreateOrUpdate(int storeId, int userId, ChangeReservationBillUpdate data, bool isAdmin = false);

    }
}
