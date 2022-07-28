using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{
    /// <summary>
    /// 仓库表
    /// </summary>
    public class WareHouse : BaseEntity
    {

        private ICollection<StockEarlyWarning> _stockEarlyWarnings;

        public WareHouse() { Status = true; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 仓库类型（枚举 1 仓库 2 车辆）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 允许负库存:不勾选时，则审核/红冲影响实际库存类单据时，不允许出现负库存。（包括：销售单，退货单，采购单，采购退货单，调拨单，盘点盈亏单，报损单，组合单，拆分单）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowNegativeInventory { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public int CreatedUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool IsSystem { get; set; }


        /// <summary>
        /// 允许负库存预售:不勾选时，审核预售类单据，预售数量不能超过库存数量（包括：销售订单）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowNegativeInventoryPreSale { get; set; }

        /// <summary>
        /// 库存用户访问权限配置
        /// </summary>
        public string WareHouseAccessSettings { get; set; }

        /// <summary>
        ///  (导航)库存预警
        /// </summary>
        public virtual ICollection<StockEarlyWarning> StockEarlyWarnings
        {
            get { return _stockEarlyWarnings ?? (_stockEarlyWarnings = new List<StockEarlyWarning>()); }
            protected set { _stockEarlyWarnings = value; }
        }


        /// <summary>
        /// 访问权限
        /// </summary>
        [NotMapped]
        public List<WareHouseAccess> WareHouseAccess { get; set; } = new List<WareHouseAccess>();
    }

    /// <summary>
    ///库存用户查询权限
    /// </summary>
    public class WareHouseAccess
    {
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool StockQuery { get; set; }
        public List<WareHouseAccessBillType> BillTypes { get; set; } = new List<WareHouseAccessBillType>();
    }

    /// <summary>
    /// 库存用户单据访问权限
    /// </summary>
    public class WareHouseAccessBillType
    {
        public int BillTypeId { get; set; }
        public string BillTypeName { get; set; }
        public bool Selected { get; set; }
    }

}
