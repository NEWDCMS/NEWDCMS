using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface ICombinationProductBillService
    {
        bool Exists(int billId);
        void DeleteCombinationProductBill(CombinationProductBill combinationProductBill);
        void DeleteCombinationProductItem(CombinationProductItem combinationProductItem);
        IList<CombinationProductBill> GetAllCombinationProductBills();
        IPagedList<CombinationProductBill> GetAllCombinationProductBills(int? store, int? makeuserId, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);

        CombinationProductBill GetCombinationProductBillById(int? store, int combinationProductBillId);
        CombinationProductBill GetCombinationProductBillById(int? store, int combinationProductBillId, bool isInclude = false);
        CombinationProductBill GetCombinationProductBillByNumber(int? store, string billNumber);
        CombinationProductItem GetCombinationProductItemById(int? store, int combinationProductItemId);
        IPagedList<CombinationProductItem> GetCombinationProductItemsByCombinationProductBillId(int combinationProductBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<CombinationProductItem> GetCombinationProductItemList(int combinationProductBillId);

        void InsertCombinationProductBill(CombinationProductBill combinationProductBill);
        void InsertCombinationProductItem(CombinationProductItem combinationProductItem);
        void UpdateCombinationProductBill(CombinationProductBill combinationProductBill);
        void UpdateCombinationProductItem(CombinationProductItem combinationProductItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CombinationProductBill combinationProductBill, CombinationProductBillUpdate data, List<CombinationProductItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, CombinationProductBill combinationProductBill);
        BaseResult Reverse(int userId, CombinationProductBill combinationProductBill);

    }
}