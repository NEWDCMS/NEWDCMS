using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IScrapProductBillService
    {
        bool Exists(int billId);
        void DeleteScrapProductBill(ScrapProductBill scrapProductBill);
        void DeleteScrapProductItem(ScrapProductItem scrapProductItem);
        IList<ScrapProductBill> GetAllScrapProductBills();
        IPagedList<ScrapProductBill> GetAllScrapProductBills(int? store, int? makeuserId, int? chargePerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);

        ScrapProductBill GetScrapProductBillById(int? store, int scrapProductBillId);
        ScrapProductBill GetScrapProductBillById(int? store, int scrapProductBillId, bool isInclude = false);
        ScrapProductBill GetScrapProductBillByNumber(int? store, string billNumber);
        ScrapProductItem GetScrapProductItemById(int? store, int scrapProductItemId);
        IPagedList<ScrapProductItem> GetScrapProductItemsByScrapProductBillId(int scrapProductBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<ScrapProductItem> GetScrapProductItemList(int scrapProductBillId);

        void InsertScrapProductBill(ScrapProductBill scrapProductBill);
        void InsertScrapProductItem(ScrapProductItem scrapProductItem);
        void UpdateScrapProductBill(ScrapProductBill scrapProductBill);
        void UpdateScrapProductItem(ScrapProductItem scrapProductItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ScrapProductBill scrapProductBill, ScrapProductBillUpdate data, List<ScrapProductItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, ScrapProductBill scrapProductBill);
        BaseResult Reverse(int userId, ScrapProductBill scrapProductBill);

    }
}