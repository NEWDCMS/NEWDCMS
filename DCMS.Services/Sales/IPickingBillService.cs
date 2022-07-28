using DCMS.Core.Domain.Sales;

namespace DCMS.Services.Sales
{
    public interface IPickingBillService
    {

        PickingBill GetPickingById(int pickingId);
        void InsertPicking(PickingBill picking);
        void UpdatePicking(PickingBill picking);
        void DeletePicking(PickingBill picking);

        PickingItem GetPickingDetailById(int pickingDetailId);
        void InsertPickingDetail(PickingItem pickingDetail);
        void UpdatePickingDetail(PickingItem pickingDetail);
        void DeletePickingDetail(PickingItem pickingDetail);




    }
}
