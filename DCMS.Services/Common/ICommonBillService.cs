using DCMS.Core;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Terminals;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Common
{
    public interface ICommonBillService
    {
        #region 

        decimal GetTerminalBalance(int storeId, int terminalId, int accountCodeTypeId);
        decimal GetManufacturerBalance(int storeId, int manufacturerId, int accountCodeTypeId);

        //IList<BillCashReceiptSummary> GetBillPaymentReceiptSummary(int storeId, int? payeer,
        //    int? manufacturerId,
        //    int? billTypeId,
        //    string billNumber = "",
        //    string remark = "",
        //    DateTime? startTime = null,
        //    DateTime? endTime = null,
        //    int pageIndex = 0,
        //    int pageSize = 20);

        #endregion

        #region 开单员工所开单据欠款

        /// <summary>
        /// 开单员工所开单据欠款
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        decimal GetUserUsedAmount(int storeId, int userId);

        Tuple<decimal, decimal> GetUserAvailableOweCash(int storeId, int userId);

        #endregion


        TerminalBalance CalcTerminalBalance(int storeId, int terminalId);
        List<CashReceiptItemView> GetCashReceiptItemView(int storeId, int terminalId);

        ManufacturerBalance CalcManufacturerBalance(int storeId, int manufacturerId);
        List<CashReceiptItemView> GetPaymentReceiptItemView(int storeId, int manufacturerid);

        decimal GetBillDiscountAmountOnce(int storeId, int billId);
        decimal GetBillReceivableAmountOnce(int storeId, int billId);
        decimal GetBillCollectionAmount(int storeId, int billId, BillTypeEnum billTypeEnum);


        decimal GetPayBillDiscountAmountOnce(int storeId, int billId);
        decimal GetPayBillReceivableAmountOnce(int storeId, int billId);
        decimal GetPayBillCollectionAmount(int storeId, int billId, BillTypeEnum billTypeEnum);

        bool RollBackBill<T, T1>(T bill) where T : BaseBill<T1> where T1 : BaseEntity;

    }
}