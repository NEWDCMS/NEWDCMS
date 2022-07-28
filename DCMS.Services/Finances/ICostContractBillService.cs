using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Sales;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface ICostContractBillService
    {
        bool Exists(int billId);
        void DeleteCostContractBill(CostContractBill costContractBill);
        void DeleteCostContractItem(CostContractItem costContractItem);
        IList<CostContractBill> GetAllCostContractBills();
        IPagedList<CostContractBill> GetAllCostContractBills(int? store, int? makeuserId, int? customerId, string customerName, int? employeeId, string billNumber = "", string remark = "", DateTime? start = null, DateTime? end = null, bool? deleted = null, int? accountingOptionId=0, int? contractType=0, int? cType=0, bool? auditedStatus = null, bool? showReverse = null, int pageIndex = 0, int pageSize = int.MaxValue);

        IPagedList<CostContractBill> GetAllCostContractBills(int? store, int? userId, int? customerId, int? accountOptionId, int? accountCodeTypeId, int year, int month, int? contractType = 0, bool? auditedStatus = null, bool? showReverse = null, int pageIndex = 0, int pageSize = int.MaxValue);

        CostContractBill GetCostContractBillById(int? store, int costContractBillId);
        CostContractBill GetCostContractBillById(int? store, int costContractBillId, bool isInclude = false);
        CostContractItem GetCostContractItemById(int? store, int costContractItemId);
        IPagedList<CostContractItem> GetCostContractItemsByCostContractBillId(int costContractBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        void InsertCostContractBill(CostContractBill costContractBill);
        void InsertCostContractItem(CostContractItem costContractItem);
        void UpdateCostContractBill(CostContractBill costContractBill);
        void UpdateCostContractItem(CostContractItem costContractItem);

        void UpdateCostContractBillActive(int? store, int? billId, int? user);

        IPagedList<CostContractItem> GetAllCostContractItems(int? storeId, int customerId, int pageIndex, int pageSize);

        IList<CostContractItem> GetAllCostContractItems(int? storeId, int customerId);

        IList<CostContractBill> GetAvailableCostContracts(int storeId, int customerId);
        IList<CostContractBill> GetAvailableCostContracts(int storeId, int customerId, int businessUserId);

        IList<CostContractBill> GetCostContractBillsByIds(int[] ids);
        IList<CostContractItem> GetCostContractItemsByIds(int[] ids);

        bool CheckGift(int storeId, int terminalId, List<SaleItem> items, out string errMsg);
        bool CheckGift(int storeId, int terminalId, List<SaleReservationItem> items, out string errMsg);

        /// <summary>
        /// 费用合同修改、赠送记录修改
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bill"></param>
        void CostContractRecordUpdate(int storeId, int type, SaleBill bill);
        /// <summary>
        /// 费用合同修改、赠送记录修改
        /// </summary>
        /// <param name="type"></param>
        /// <param name="saleReservationBill"></param>
        void CostContractRecordUpdate(int type, SaleReservationBill saleReservationBill);


        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, List<AccountingOption> accountings, CostContractBillUpdate data, List<CostContractItem> items, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, CostContractBill costContractBill);
        BaseResult Rejected(int storeId, int userId, CostContractBill costContractBill);
        BaseResult Abandoned(int storeId, int userId, CostContractBill costContractBill);

        /// <summary>
        /// 计算客户终端费用合同费用兑付分摊余额
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="customerId"></param>
        /// <param name="bill"></param>
        /// <returns></returns>
        IList<CostContractItem> CalcCostContractBalances(int? storeId, int customerId, CostContractBill bill);

        /// <summary>
        /// 验证费用合同
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="terminalId"></param>
        /// <param name="accountOptionId"></param>
        /// <param name="items"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        bool CheckContract(int storeId, int year, int terminalId, int accountOptionId, List<CostContractItem> items, out string errMsg);
    }
}