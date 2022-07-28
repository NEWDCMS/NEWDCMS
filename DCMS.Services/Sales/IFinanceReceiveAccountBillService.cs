using DCMS.Core;
using DCMS.Core.Domain.Sales;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Sales
{
    public interface IFinanceReceiveAccountBillService
    {
        void DeleteFinanceReceiveAccountBill(FinanceReceiveAccountBill bill);
        void DeleteFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting);
        IPagedList<FinanceReceiveAccountView> GetFinanceReceiveAccounts(int storeId, DateTime? start, DateTime? end, int? businessUserId, int? payeer, int? accountingOptionId, string billNumber = "", int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<FinanceReceiveAccountBill> GetSubmittedBills(int storeId, DateTime? start, DateTime? end, int? businessUserId, int? billTypeId, string billNumber = "", int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<RankProduct> GetRankProducts(int storeId, bool gift, int? userId, int billType, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<RankProduct> GetRankProducts(int storeId, bool gift, int? userId, int billType, DateTime? start, DateTime? end, int[] billIds);
        IList<FinanceReceiveAccountBillAccounting>  GetFinanceReceiveAccountBillAccountings(int storeId, int billId);
        FinanceReceiveAccountBill GetFinanceReceiveAccountBillById(int? store, int financeReceiveAccountBillId, bool isInclude = false);
        void InsertFinanceReceiveAccountBill(FinanceReceiveAccountBill bill);
        void InsertFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting);
        void InsertFinanceReceiveAccountBillAccountings(List<FinanceReceiveAccountBillAccounting> financeReceiveAccountBillAccountings);
        BaseResult SubmitAccountStatement(int storeId, int userId, FinanceReceiveAccountBill bill);
        BaseResult BatchSubmitAccountStatements(int storeId, int userId, List<FinanceReceiveAccountBill> bills);
        void UpdateFinanceReceiveAccountBill(FinanceReceiveAccountBill bill);
        void UpdateFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting);
    }
}