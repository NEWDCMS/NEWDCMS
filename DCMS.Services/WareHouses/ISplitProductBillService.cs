using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface ISplitProductBillService
    {
        bool Exists(int billId);
        void DeleteSplitProductBill(SplitProductBill splitProductBill);
        void DeleteSplitProductItem(SplitProductItem splitProductItem);
        IList<SplitProductBill> GetAllSplitProductBills();
        IPagedList<SplitProductBill> GetAllSplitProductBills(int? store, int? makeuserId, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);

        SplitProductBill GetSplitProductBillById(int? store, int splitProductBillId);
        SplitProductBill GetSplitProductBillById(int? store, int splitProductBillId, bool isInclude = false);
        SplitProductBill GetSplitProductBillByNumber(int? store, string billNumber);
        SplitProductItem GetSplitProductItemById(int? store, int splitProductItemId);
        IPagedList<SplitProductItem> GetSplitProductItemsBySplitProductBillId(int splitProductBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<SplitProductItem> GetSplitProductItemList(int splitProductBillId);

        void InsertSplitProductBill(SplitProductBill splitProductBill);
        void InsertSplitProductItem(SplitProductItem splitProductItem);
        void UpdateSplitProductBill(SplitProductBill splitProductBill);
        void UpdateSplitProductItem(SplitProductItem splitProductItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, SplitProductBill splitProductBill, SplitProductBillUpdate data, List<SplitProductItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, SplitProductBill splitProductBill);
        BaseResult Reverse(int userId, SplitProductBill splitProductBill);

    }
}