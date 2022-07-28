using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Sales
{
    public interface ISaleBillService
    {
        bool Exists(int billId);
        void DeleteSaleBill(SaleBill bill);
        IPagedList<SaleBill> GetSaleBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null, bool? receipted = null, bool? deleted = null, bool? handleStatus = null, int? sign = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null);
        IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int? businessUserId = null, DateTime? startTime = null, DateTime? endTime = null);
        IList<SaleBill> GetSaleBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, DateTime? beginDate = null);

        IList<SaleBill> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);
        IList<SaleBill> GetCostProfitRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);

        IList<SaleBill> GetSaleBillByStoreIdTerminalId(int storeId, int terminalId);

        void InsertSaleBill(SaleBill bill);
        SaleBill GetSaleBillById(int? store, int saleBillId);
        /// <summary>
        /// 根据销售单id获取销售单
        /// </summary>
        /// <param name="saleBillId"></param>
        /// <returns></returns>
        SaleBill GetSaleBillById(int? store, int saleBillId, bool isInclude=false);
        IList<SaleBill> GetSaleBillsByIds(int[] sIds);
        /// <summary>
        /// 根据销售订单ids 获取销售单
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        IList<SaleBill> GetSaleBillsBySaleReservationIds(int? store, int[] sIds);

        SaleBill GetSaleBillBySaleReservationBillId(int? store, int saleReservationBillId);

        /// <summary>
        /// 送货签收时可能会新增销售单单据
        /// </summary>
        /// <param name="saleReservationBillId"></param>
        /// <returns></returns>
        IList<SaleBill> GetSaleBillBySaleReservationsBillId(int saleReservationBillId);
        int GetBillId(int? store, string billNumber);
        SaleBill GetSaleBillByNumber(int? store, string billNumber);
        void UpdateSaleBill(SaleBill bill);

        void SetSaleBillAmount(int saleBillId);

        void DeleteSaleItem(SaleItem saleItem);
        List<SaleItem> GetSaleItemList(int saleBillId);
        IList<SaleItem> GetSaleItemBySaleBillId(int saleBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        void UpdateSaleItem(SaleItem saleItem);
        void UpdateSaleItem(int storeId, int billId, int productId, decimal costPrice = 0);
        SaleItem GetSaleItemById(int saleItemId);
        void InsertSaleItem(SaleItem saleItem);
        void InsertSaleItems(List<SaleItem> saleItems);
        int SaleItemQtySum(int storeId, int productId, int saleBillId);

        IPagedList<SaleBillAccounting> GetSaleBillAccountingsBySaleId(int storeId, int userId, int saleBillId, int pageIndex, int pageSize);
        IList<SaleBillAccounting> GetSaleBillAccountingsBySaleBillId(int? store, int saleBillId);
        SaleBillAccounting GetSaleBillAccountingById(int saleBillAccountingId);
        void InsertSaleBillBillAccounting(SaleBillAccounting saleBillAccounting);
        void UpdateSaleBillAccounting(SaleBillAccounting saleBillAccounting);
        void DeleteSaleBillAccounting(SaleBillAccounting saleBillAccounting);

        void UpdateSaleBillReceipted(string billNumber);
        void UpdateSaleBillOweCash(string billNumber, decimal oweCash);
        IList<SaleBillAccounting> GetAllSaleBillAccountingsByBillIds(int? store, int[] billIds);

        SaleBill GetLastSaleBill(int? storeId, int? terminalId, int? businessUserId);

        void UpdateSaleBillActive(int? store, int? billId, int? user);

        IList<SaleBill> GetSaleBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null);
        IList<BaseItem> GetSaleBillsByBusinessUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] businessUserIds = null, DateTime? startTime = null, DateTime? endTime = null);
        IList<BaseItem> GetSaleBillsByDeliveryUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] deliveryUserIds = null, DateTime? startTime = null, DateTime? endTime = null);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, SaleBill bill, List<SaleBillAccounting> accountingOptions, List<AccountingOption> accountings, SaleBillUpdate data, List<SaleItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, List<RetainPhoto> photos = null,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, SaleBill bill);
        BaseResult Reverse(int userId, SaleBill bill);
        BaseResult Delete(int userId, SaleBill bill);

        /// <summary>
        /// 验证销售订单=》装车调度=》送货签收转单后是否红冲
        /// </summary>
        /// <param name="BillId"></param>
        /// <returns></returns>
        public bool CheckReversed(int? BillId);

        void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus);
        void UpdateHandInStatus(int? store, int billId, bool handInStatus);
    }
}
