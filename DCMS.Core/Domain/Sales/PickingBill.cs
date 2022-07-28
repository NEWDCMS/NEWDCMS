using System;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 拣货单
    /// </summary>
    public class PickingBill : BaseBill<PickingItem>
    {
        public PickingBill()
        {
            BillType = BillTypeEnum.PickingBill;
        }

        //private ICollection<PickingItem> _pickingItems;


        /// <summary>
        /// 业务员Ids 多个
        /// </summary>
        public int BusinessUserIds { get; set; }

        /// <summary>
        /// 创建人员
        /// </summary>
        public int? AddUserId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? AddDate { get; set; }

        /// <summary>
        /// 修改人员
        /// </summary>
        public string EditUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? EditDate { get; set; }

        ///// <summary>
        ///// (导航)明细
        ///// </summary>
        //public virtual ICollection<PickingItem> PickingItems
        //{
        //    get { return _pickingItems ?? (_pickingItems = new List<PickingItem>()); }
        //    protected set { _pickingItems = value; }
        //}


    }


    /// <summary>
    /// 拣货单明细
    /// </summary>
    public class PickingItem : BaseEntity
    {


        /// <summary>
        /// 拣货单Id
        /// </summary>
        public int? PickingId { get; set; } = 0;

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserName { get; set; } = 0;

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; } = 0;

        /// <summary>
        /// 客户Name
        /// </summary>
        public int? TerminalName { get; set; } = 0;

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal? OrderAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人员
        /// </summary>
        public int? AddUserId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? AddDate { get; set; }

        /// <summary>
        /// 修改人员
        /// </summary>
        public string EditUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? EditDate { get; set; }

        #region 导航

        public virtual PickingBill PickingBill { get; set; }


        #endregion

    }

}
