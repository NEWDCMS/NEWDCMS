using DCMS.Core;
using DCMS.Core.Domain.CSMS;
using DCMS.Core.Domain.Sales;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Sales
{
    public interface IDispatchBillService
    {
        bool Exists(int billId);
        DispatchBill GetDispatchBillById(int? store, int dispatchBillId);
        void InsertDispatchBill(DispatchBill dispatchBill);
        void UpdateDispatchBill(DispatchBill dispatchBill);
        void DeleteDispatchBill(DispatchBill dispatchBill);

        DispatchItem GetDispatchItemsById(int? store, int dispatchItemId);
        void InsertDispatchItem(DispatchItem dispatchItem);
        void UpdateDispatchItem(DispatchItem dispatchItem);
        void DeleteDispatchItem(DispatchItem dispatchItem);

        int GetCarId(int billTypeId, int billId);

        /// <summary>
        /// 获取调度单调度的 销售订单、退货订单
        /// </summary>
        /// <param name="dispatchBillId">调度单Id</param>
        /// <param name="billTypeId">单据类型</param>
        /// <param name="billId">单据Id</param>
        /// <returns></returns>
        DispatchItem GetDispatchItemByDispatchBillIdBillTypeBillId(int dispatchBillId, int billTypeId, int billId);

        IList<DispatchItem> GetDispatchItemByDispatchBillId(int dispatchBillId, int? userId, int? storeId, int pageIndex, int pageSize);

        //装车调度查询
        IPagedList<DispatchBill> GetDispatchBillList(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null,
                   string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// 验证销售订单是否签收
        /// </summary>
        /// <param name="saleReservationBill"></param>
        /// <returns></returns>
        bool CheckSign(SaleReservationBill saleReservationBill);
        /// <summary>
        /// 验证销售订单对应调拨单是否红冲
        /// </summary>
        /// <param name="saleReservationBill"></param>
        /// <returns></returns>
        public bool CheckReversed(int? BillId);

        bool CheckSign(ExchangeBill srb);
        BaseResult CreateBill(int storeId, int userId, DispatchBill dispatchBill, int deliveryId, int carId, List<DispatchItem> selectDatas, int autoAllocationBill, int? operation, out string allocationBillNumbers, out int dispatchBillId);

        BaseResult UpdateBill(int storeId, int userId, DispatchBill dispatchBill, int? id, int deliveryId, int carId);

        BaseResult Reverse(int userId, DispatchBill dispatchBill);

        //送货签收
        void InsertDeliverySign(DeliverySign deliverySign);
        DeliverySign GetDeliverySignById(int? store, int deliverySignId);
        void UpdateDeliverySign(DeliverySign deliverySign);
        void DeleteDeliverySign(DeliverySign deliverySign);

        void InsertRetainPhoto(RetainPhoto retainPhoto);
        RetainPhoto GetRetainPhotoById(int? store, int retainPhotoId);
        void UpdateRetainPhoto(RetainPhoto retainPhoto);
        void DeleteRetainPhoto(RetainPhoto retainPhoto);

        BaseResult BillPrint(int storeId, int userId, DispatchBill dispatchBill, List<string> printTypeList);


        /// <summary>
        /// 拒签
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="deliverySignUpdate"></param>
        /// <param name="dispatchBill"></param>
        /// <param name="dispatchItem"></param>
        /// <returns></returns>
        BaseResult RefusedConfirm(int storeId, int userId, DeliverySignUpdate deliverySignUpdate, DispatchBill dispatchBill, DispatchItem dispatchItem);

        /// <summary>
        /// 签收
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        BaseResult SignConfirm(int storeId, int userId, DeliverySign data, TerminalSignReport terminalSignReport, List<OrderDetail> orderDetails);

        /// <summary>
        /// 获取已经签收单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<DeliverySign> GetSignedBills(int? storeId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? terminalId = null, int pageIndex = 0, int pageSize = int.MaxValue);

        bool SendMessage(string mobile, string content);
    }
}
