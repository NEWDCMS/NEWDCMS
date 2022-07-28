using DCMS.Web.Framework.Models;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.ViewModel.Models.WareHouses
{
    /// <summary>
    /// 仓库表
    /// </summary>
    public class WareHouseModel : BaseEntityModel
    {
        public WareHouseModel() { Status = true; }

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
        public bool AllowNegativeInventory { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Status { get; set; }

        public bool IsSystem { get; set; }


        /// <summary>
        /// 允许负库存预售:不勾选时，审核预售类单据，预售数量不能超过库存数量（包括：销售订单）
        /// </summary>
        public bool AllowNegativeInventoryPreSale { get; set; } = true;

        /// <summary>
        /// 访问权限
        /// </summary>
        public List<WareHouseAccessModel> WareHouseAccess { get; set; } = new List<WareHouseAccessModel>();
    }

    /// <summary>
    ///库存用户查询权限
    /// </summary>
    public class WareHouseAccessModel
    {
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool StockQuery { get; set; }
        public List<WareHouseAccessBillTypeModel> BillTypes { get; set; } = new List<WareHouseAccessBillTypeModel>();
    }

    /// <summary>
    /// 库存用户单据访问权限
    /// </summary>
    public class WareHouseAccessBillTypeModel
    {
        public int BillTypeId { get; set; }
        public string BillTypeName { get; set; }
        public bool Selected { get; set; }
    }
}
