using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface ICostAdjustmentBillService
    {
        bool Exists(int billId);
        void DeleteCostAdjustmentBill(CostAdjustmentBill costAdjustmentBill);
        void DeleteCostAdjustmentItem(CostAdjustmentItem costAdjustmentItem);
        IList<CostAdjustmentBill> GetAllCostAdjustmentBills();
        IPagedList<CostAdjustmentBill> GetAllCostAdjustmentBills(int? store, int? makeuserId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int pageIndex = 0, int pageSize = int.MaxValue);

        CostAdjustmentBill GetCostAdjustmentBillById(int? store, int costAdjustmentBillId);
        CostAdjustmentBill GetCostAdjustmentBillById(int? store, int costAdjustmentBillId, bool isInclude = false);
        CostAdjustmentBill GetCostAdjustmentBillByNumber(int? store, string billNumber);
        CostAdjustmentItem GetCostAdjustmentItemById(int? store, int costAdjustmentItemId);
        IPagedList<CostAdjustmentItem> GetCostAdjustmentItemsByCostAdjustmentBillId(int costAdjustmentBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        void InsertCostAdjustmentBill(CostAdjustmentBill costAdjustmentBill);
        void InsertCostAdjustmentItem(CostAdjustmentItem costAdjustmentItem);
        void UpdateCostAdjustmentBill(CostAdjustmentBill costAdjustmentBill);
        void UpdateCostAdjustmentItem(CostAdjustmentItem costAdjustmentItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CostAdjustmentBill costAdjustmentBill, CostAdjustmentBillUpdate data, List<CostAdjustmentItem> items, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, CostAdjustmentBill costAdjustmentBill);
        BaseResult Reverse(int userId, CostAdjustmentBill costAdjustmentBill);

    }
}