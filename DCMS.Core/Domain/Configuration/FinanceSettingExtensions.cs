
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core.Domain.Configuration
{
    public static class FinanceSettingExtensions
    {

        #region 销售
        /// <summary>
        /// 获取销售单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap SaleBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.SaleBillAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取销售订单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap SaleReservationBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.SaleReservationBillAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取退货单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap ReturnBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReturnBillAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取退货订单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap ReturnReservationBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReturnReservationBillAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region 采购
        /// <summary>
        /// 获取采购单(付款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap PurchaseBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                var account = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PurchaseBillAccountingOptionConfiguration);
                //account.Options.ToList().ForEach(p =>
                //{
                //    //预付款
                //    if (p.AccountCodeTypeId == (int)AccountingCodeEnum.AdvancePayment)
                //    {
                //        p.Balance = 0; //这里赋值余额
                //    }
                //});
                return account;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取采购退货单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap PurchaseReturnBillAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PurchaseReturnBillAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region 仓储
        /// <summary>
        /// 盘点盈亏单(盘点盈亏账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap InventoryProfitLossAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.InventoryProfitLossAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 报损单(报损账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap ScrapProductAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ScrapProductAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region 财务
        /// <summary>
        /// 获取收款单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap ReceiptAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReceiptAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 付款单(付款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap PaymentAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.PaymentAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取预收款单(收款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap AdvanceReceiptAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.AdvanceReceiptAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取预付款单(付款账户) 配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap AdvancePaymentAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.AdvancePaymentAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 费用支出（支出账户）  配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap CostExpenditureAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.CostExpenditureAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 财务收入（收款账户）  配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap FinancialIncomeAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.FinancialIncomeAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 收款对账（会计科目配置）
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap FinanceReceiveAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                var result = new FinanceAccountingMap();
                var accountlist = new List<AccountingOption>();

                //销售
                var sailBill = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.SaleBillAccountingOptionConfiguration).Options.ToList();
                //退货
                var returnBill = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReturnBillAccountingOptionConfiguration).Options.ToList();
                //收款
                var receiptBill = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.ReceiptAccountingOptionConfiguration).Options.ToList();
                //预收款
                var advanceBill = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.AdvanceReceiptAccountingOptionConfiguration).Options.ToList();
                //费用支出
                var costBill = JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.CostExpenditureAccountingOptionConfiguration).Options.ToList();

                //取并集
                accountlist = sailBill.Union(returnBill).Union(receiptBill).Union(advanceBill).Union(costBill).ToList();
                //去重
                accountlist = accountlist.Distinct().ToList();

                result.Options = accountlist;

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取 财务收入（费用合同）  配置
        /// </summary>
        /// <param name="financeSetting"></param>
        /// <returns></returns>
        public static FinanceAccountingMap CostContractAccountingOptionConfiguration(this FinanceSetting financeSetting)
        {
            if (financeSetting == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<FinanceAccountingMap>(financeSetting.CostContractAccountingOptionConfiguration);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion


    }
}
