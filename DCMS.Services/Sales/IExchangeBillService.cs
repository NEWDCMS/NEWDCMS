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
    public interface IExchangeBillService
    {
        BaseResult Auditing(int storeId, int userId, ExchangeBill bill);
        BaseResult AuditingNoTran(int storeId, int userId, ExchangeBill bill);
        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ExchangeBill bill, List<AccountingOption> accountings, ExchangeBillUpdate data, List<ExchangeItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);
        void ChangedBill(int billId, int userId);
        void DeleteExchangeBill(ExchangeBill bill);
        void DeleteExchangeItem(ExchangeItem exchangeItem);
        int ExchangeItemQtySum(int storeId, int productId, int exchangeBillId);
        bool Exists(int billId);
        int GetBillId(int? store, string billNumber);
        ExchangeBill GetExchangeBillById(int? store, int exchangeBillId, bool isInclude = false);
        ExchangeBill GetExchangeBillByNumber(int? store, string billNumber);
        IList<ExchangeBill> GetExchangeBillByStoreIdTerminalId(int storeId, int terminalId);
        IPagedList<ExchangeBill> GetExchangeBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue, bool platform = false);
        IList<ExchangeBill> GetExchangeBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null);
        IList<IGrouping<DateTime, ExchangeBill>> GetExchangeBillsAnalysisByCreate(int? storeId, int? user, DateTime date);
        IList<ExchangeBill> GetExchangeBillsByIds(int[] sIds, bool isInclude = false);
        IList<ExchangeBill> GetExchangeBillsByStoreId(int? storeId);
        IList<ExchangeBill> GetExchangeBillsNullWareHouseByStoreId(int storeId);
        IList<ExchangeBill> GetExchangeBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<ExchangeBill> GetExchangeBillsToPicking(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, string remark = "", int pickingFilter = 0, int? wholeScrapStatus = 0, int? scrapStatus = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<ExchangeBill> GetExchangeBillToChangeList(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<ExchangeBill> GetExchangeBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null, string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null);
        IList<ExchangeItem> GetExchangeItemByExchangeBillId(int exchangeBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        ExchangeItem GetExchangeItemById(int exchangeItemId);
        List<ExchangeItem> GetExchangeItemList(int exchangeBillId);
        int GetSumQuantityByExchangeBillId(int storeId, ISpecificationAttributeService _specificationAttributeService, IProductService _productService, int exchangeBillId);
        void InsertExchangeBill(ExchangeBill bill);
        void InsertExchangeItem(ExchangeItem exchangeItem);
        BaseResult Reverse(int userId, ExchangeBill bill);
        BaseResult Delete(int userId, ExchangeBill bill);
        void SetExchangeBillAmount(int exchangeBillId);
        void UpdateExchangeBill(ExchangeBill bill);
        void UpdateExchangeBillActive(int? store, int? billId, int? user);
        void UpdateExchangeItem(ExchangeItem exchangeItem);

        BaseResult ExchangeSignIn(int storeId, int userId, ExchangeBill bill, DispatchItem data, List<RetainPhoto> photos, string signature);
    }
}