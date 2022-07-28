using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Sales
{
    public interface ISaleReservationBillService
    {
        bool Exists(int billId);
        void DeleteSaleReservationBill(SaleReservationBill saleReservationBill);
        IPagedList<SaleReservationBill> GetSaleReservationBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue, bool platform = false);
        IList<SaleReservationBill> GetSaleReservationBillsByStoreId(int? storeId);

        IList<SaleReservationBill> GetSaleReservationBillByStoreIdTerminalId(int storeId, int terminalId);

        IList<SaleReservationBill> GetSaleReservationBillsNullWareHouseByStoreId(int storeId);

        IList<SaleReservationBill> GetHotSaleReservationRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);

        void InsertSaleReservationBill(SaleReservationBill saleReservationBill);
        SaleReservationBill GetSaleReservationBillById(int? store, int saleReservationBillId, bool isInclude = false);
        IList<SaleReservationBill> GetSaleReservationBillsByIds(int[] sIds, bool isInclude = false);
        int GetBillId(int? store, string billNumber);
        SaleReservationBill GetSaleReservationBillByNumber(int? store, string billNumber);
        void UpdateSaleReservationBill(SaleReservationBill saleReservationBill);
        void ChangedBill(int billId, int userId);
        IList<SaleReservationBill> GetSaleReservationBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null);
        void SetSaleReservationBillAmount(int saleReservationBillId);

        IList<IGrouping<DateTime, SaleReservationBill>> GetSaleReservationBillsAnalysisByCreate(int? storeId, int? user, DateTime date);

        void DeleteSaleReservationItem(SaleReservationItem saleReservationItem);
        List<SaleReservationItem> GetSaleReservationItemList(int saleReservationBillId);
        IList<SaleReservationItem> GetSaleReservationItemBySaleReservationBillId(int saleReservationBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        void UpdateSaleReservationItem(SaleReservationItem saleReservationItem);
        SaleReservationItem GetSaleReservationItemById(int saleReservationItemId);
        void InsertSaleReservationItem(SaleReservationItem saleReservationItem);
        int SaleReservationItemQtySum(int storeId, int productId, int saleReservationBillId);
        IList<SaleReservationBillAccounting> GetSaleReservationBillAccountingsBySaleReservationBillId(int? store, int saleReservationBillId);
        SaleReservationBillAccounting GetSaleReservationBillAccountingById(int saleReservationBillAccountingId);
        void InsertSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting);
        void UpdateSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting);
        void DeleteSaleReservationBillAccounting(SaleReservationBillAccounting saleReservationBillAccounting);

        //根据销售订单ID获取商品总数量
        int GetSumQuantityBySaleReservationBillId(int storeId, ISpecificationAttributeService specificationAttributeService, IProductService productService, int saleReservationBillId);

        //装车调度查询
        IList<SaleReservationBill> GetSaleReservationBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null,
                   string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null);

        //销售订单转销售单查询
        IPagedList<SaleReservationBill> GetSaleReservationBillToChangeList(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);

        //车辆对货单查询
        IList<SaleReservationBill> GetSaleReservationBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);

        //仓库分拣单查询
        IList<SaleReservationBill> GetSaleReservationBillsToPicking(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, string remark = "", int pickingFilter = 0, int? wholeScrapStatus = 0, int? scrapStatus = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        IList<SaleReservationBillAccounting> GetAllSaleReservationBillAccountingsByBillIds(int? store, int[] billIds, bool platform = false);
        void UpdateSaleReservationBillActive(int? store, int? billId, int? user);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, SaleReservationBill saleReservationBill, List<SaleReservationBillAccounting> accountingOptions, List<AccountingOption> accountings, SaleReservationBillUpdate data, List<SaleReservationItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, SaleReservationBill saleReservationBill);
        BaseResult Reverse(int userId, SaleReservationBill saleReservationBill);
        BaseResult Delete(int userId, SaleReservationBill saleReservationBill);
    }
}
