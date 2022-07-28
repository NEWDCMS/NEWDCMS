using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class ChangeReservationBillUpdate : BaseEntity
    {
        public ChangeReservationBillUpdate()
        {
            //查询
            SaleReservationBillAccountings = new List<SaleReservationBillAccounting>();
            ReturnReservationBillAccountings = new List<ReturnReservationBillAccounting>();

            //弹出框
            SaleBillAccountings = new List<AccountingOption>();
            ReturnBillAccountings = new List<AccountingOption>();

        }

        //收款账户
        public IList<SaleReservationBillAccounting> SaleReservationBillAccountings { get; set; }
        public IList<ReturnReservationBillAccounting> ReturnReservationBillAccountings { get; set; }

        #region 转单弹出框 下拉会计科目数据源
        //收款账户（这里需要转成销售单、退货单所以去销售单、退货单科目）
        //销售单科目
        public IList<AccountingOption> SaleBillAccountings { get; set; }
        //退货单科目
        public IList<AccountingOption> ReturnBillAccountings { get; set; }

        /// <summary>
        /// 收款方式
        /// </summary>
        public int AccountingId { get; set; } = 0;

        #endregion

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;
        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        public int BillType { get; set; } = 0;
        public string Ids { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

    }

}
