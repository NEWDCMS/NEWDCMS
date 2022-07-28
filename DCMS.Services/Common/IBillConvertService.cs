using System;
using DCMS.Core;
namespace DCMS.Services.Common
{
    public interface IBillConvertService
    {
        bool AdjustBillCost(int storeId, int billTypeEnum, int billId, int productId, decimal costPrice = 0);
        bool CheckStore(int StoreId);
        bool CheckWareHouse(int wareHouseId);
        //string GenerateBillUrl(int billType, string billNumber);
        string GenerateBillUrl(int billType, int billId);
        void UpdateCombinationProductItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateCostAdjustmentItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateInventoryProfitLossItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdatePurchaseItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdatePurchaseReturnItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateReturnItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateSaleItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateScrapProductItem(int storeId, int billId, int productId, decimal costPrice = 0);
        void UpdateSplitProductItem(int storeId, int billId, int productId, decimal costPrice = 0);

        Tuple<string, decimal> JudgmentLending(decimal balance, decimal balance_credit, decimal balance_debit);
        Tuple<string, decimal> JudgmentLending(decimal balance_credit, decimal balance_debit);
        Tuple<string, decimal> JudgmentLending(Tuple<decimal, decimal, decimal> tuple);
        bool UpdataBillAccountStatement(int storeId, int billId, BillTypeEnum billTypeEnum);
        bool Cy(int change, int storeId, int billId, int productId);
    }
}