using DCMS.Core;
using DCMS.Web.Framework.Models;
using System;

namespace DCMS.ViewModel.Models.WareHouses
{
    /// <summary>
    /// 用于表示库存
    /// </summary>
    public class StockModel : BaseEntityModel
    {


        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 库位号(A100_01_01)
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// 可用库存量
        /// </summary>
        public int? UsableQuantity { get; set; } = 0;

        /// <summary>
        /// 现货库存量
        /// </summary>
        public int? CurrentQuantity { get; set; } = 0;

        /// <summary>
        /// 预占库存量
        /// </summary>
        public int? OrderQuantity { get; set; } = 0;

        /// <summary>
        /// 锁定库存量
        /// </summary>
        public int? LockQuantity { get; set; } = 0;

        /// <summary>
        /// 创建人
        /// </summary>
        public int? CreaterId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public int? UpdaterId { get; set; } = 0;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeStamp { get; set; }

    }

    /// <summary>
    /// 用于表示出入库记录
    /// </summary>
    public class StockInOutRecordModel : BaseEntityModel
    {

        /// <summary>
        /// 单据编号
        /// </summary>
        public int BillId { get; set; }


        /// <summary>
        /// 出入单据编号
        /// </summary>
        public string BillCode { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int? Quantity { get; set; } = 0;
        public DirectionEnum DirectionEnum
        {
            get { return (DirectionEnum)Direction; }
            set { Direction = (int)value; }
        }

        /// <summary>
        /// 出入方向(0:默认,1:入,2:出)
        /// </summary>
        public int Direction { get; set; } = 0;

        /// <summary>
        /// 调出仓库ID
        /// </summary>
        public int? OutWareHouseId { get; set; } = 0;

        /// <summary>
        /// 调入仓库ID
        /// </summary>
        public int? InWareHouseId { get; set; } = 0;

        /// <summary>
        /// 出入时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }

    /// <summary>
    /// 用于表示库存流水(历史)记录
    /// </summary>
    public class StockFlowModel : BaseEntityModel
    {

        /// <summary>
        /// 库存ID
        /// </summary>
        public int StockId { get; set; } = 0;



        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 可用库存量修改前的值
        /// </summary>
        public int? UsableQuantityBefor { get; set; } = 0;
        /// <summary>
        /// 可用库存量修改后的值
        /// </summary>
        public int? UsableQuantityAfter { get; set; } = 0;
        /// <summary>
        /// 可用库存量改变值 （增加为正，减少为负）
        /// </summary>
        public int? UsableQuantityChange { get; set; } = 0;

        /// <summary>
        /// 现货库存量修改前的值
        /// </summary>
        public int? CurrentQuantityBefor { get; set; } = 0;
        /// <summary>
        /// 现货库存量修改后的值
        /// </summary>
        public int? CurrentQuantityAfter { get; set; } = 0;
        /// <summary>
        /// 现货库存量改变值 （增加为正，减少为负）
        /// </summary>
        public int? CurrentQuantityChange { get; set; } = 0;

        /// <summary>
        /// 预占库存量修改前的值
        /// </summary>
        public int? OrderQuantityBefor { get; set; } = 0;
        /// <summary>
        /// 预占库存量修改后的值
        /// </summary>
        public int? OrderQuantityAfter { get; set; } = 0;
        /// <summary>
        /// 预占库存量改变值 （增加为正，减少为负）
        /// </summary>
        public int? OrderQuantityChange { get; set; } = 0;

        /// <summary>
        /// 锁定库存量修改前的值
        /// </summary>
        public int? LockQuantityBefor { get; set; } = 0;
        /// <summary>
        /// 锁定库存量修改后的值
        /// </summary>
        public int? LockQuantityAfter { get; set; } = 0;
        /// <summary>
        /// 锁定库存量改变值 （增加为正，减少为负）
        /// </summary>
        public int? LockQuantityChange { get; set; } = 0;

        /// <summary>
        /// 创建人
        /// </summary>
        public int? CreaterId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeStamp { get; set; }

    }
}
