using DCMS.Core.Domain.Products;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DCMS.Core.Domain.WareHouses
{

    #region 实体类

    /// <summary>
    /// 用于表示库存
    /// </summary>
    public class Stock : BaseEntity
    {

        private ICollection<StockFlow> _stockFlows;


        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 库位号(A001_01_01)
        /// </summary>
        public string PositionCode { get; set; } = "";

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
        [ConcurrencyCheck]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime TimeStamp { get; set; }
        public bool IsExpiredGood { get; set; } = false;
        public int Version { get; set; }


        [JsonIgnore]
        public virtual Product Product { get; set; }
        [JsonIgnore]
        public virtual WareHouse WareHouse { get; set; }


        public virtual ICollection<StockFlow> StockFlows
        {
            get { return _stockFlows ?? (_stockFlows = new List<StockFlow>()); }
            protected set { _stockFlows = value; }
        }

    }

    /// <summary>
    /// 用于表示出入库业务记录
    /// </summary>
    public class StockInOutRecord : BaseEntity
    {
        private ICollection<StockInOutRecordStockFlow> _stockInOutRecordStockFlows;

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

        /// <summary>
        /// 出入方向(0:默认,1:入,2:出)
        /// </summary>
        public int Direction { get; set; }
        public DirectionEnum DirectionEnum
        {
            get { return (DirectionEnum)Direction; }
            set { Direction = (int)value; }
        }

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


        /// <summary>
        /// (导航) 出入库流水
        /// </summary>
        public virtual ICollection<StockInOutRecordStockFlow> StockInOutRecordStockFlows
        {
            get { return _stockInOutRecordStockFlows ?? (_stockInOutRecordStockFlows = new List<StockInOutRecordStockFlow>()); }
            protected set { _stockInOutRecordStockFlows = value; }
        }
    }



    public class StockInOutRecordQuery 
    {
        /// <summary>
        /// 获取或设置实体标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; } = 0;

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

        /// <summary>
        /// 出入方向(0:默认,1:入,2:出)
        /// </summary>
        public int Direction { get; set; }

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
    /// 用于表示库存流水(更改历史)记录
    /// </summary>
    public class StockFlow : BaseEntity
    {

        /// <summary>
        /// 库存ID
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品出入库单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 转化结算小单位
        /// </summary>
        public int SmallUnitId { get; set; }

        /// <summary>
        /// 转化结算大单位
        /// </summary>
        public int BigUnitId { get; set; }


        /// <summary>
        /// 出入量
        /// </summary>
        public int Quantity { get; set; }

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
        /// 变更类型
        /// </summary>
        public int? ChangeType { get; set; } = 0;

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
        //[Timestamp]
        [ConcurrencyCheck]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? TimeStamp { get; set; }
        public bool IsExpiredGood { get; set; } = false;

        public virtual Stock Stock { get; set; }

    }


    public class StockFlowQuery
    {
        /// <summary>
        /// 获取或设置实体标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; } = 0;
        /// <summary>
        /// 库存ID
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品出入库单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 转化结算小单位
        /// </summary>
        public int SmallUnitId { get; set; }

        /// <summary>
        /// 转化结算大单位
        /// </summary>
        public int BigUnitId { get; set; }


        /// <summary>
        /// 出入量
        /// </summary>
        public int Quantity { get; set; }

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
        /// 变更类型
        /// </summary>
        public int? ChangeType { get; set; } = 0;

        /// <summary>
        /// 创建人
        /// </summary>
        public int? CreaterId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        public DateTime? TimeStamp { get; set; }
        public bool IsExpiredGood { get; set; } = false;

        public int Version { get; set; } = 0;

    }


    /// <summary>
    /// 用于表示出入库记录和流水映射
    /// </summary>
    public class StockInOutRecordStockFlow : BaseEntity
    {
        public int StockInOutRecordId { get; set; }
        public int StockFlowId { get; set; }

        public virtual StockInOutRecord StockInOutRecord { get; set; }
        public virtual StockFlow StockFlow { get; set; }
    }

    /// <summary>
    /// 用于表示商品库存信息
    /// </summary>
    public class ProductStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public int UnitId { get; set; }
        public int SmallUnitId { get; set; }
        public int BigUnitId { get; set; }
        public bool IsExpiredGood { get; set; } = false;

        //单据项目
        public BaseItem BillItem { get; set; }

    }


    /// <summary>
    /// 用于商品出入库明细记录
    /// </summary>
    public class StockInOutDetails : BaseEntity
    {
        /// <summary>
        /// 库存Id
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// 出入库业务记录Id
        /// </summary>
        public int StockInOutRecordId { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int WareHouseId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 出入数量
        /// </summary>
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 剩余数量
        /// </summary>
        public int? RemainderQuantity { get; set; } = 0;

        /// <summary>
        /// 出入方向(0:默认,1:入,2:出)
        /// </summary>
        public int Direction { get; set; }

        /// <summary>
        /// 商品批次
        /// </summary>
        public string ProductionBatch { get; set; }

        /// <summary>
        /// ERP同步库存商品关联Id
        /// </summary>
        public int DealerStockId { get; set; }

        /// <summary>
        /// 出入库时间
        /// </summary>
        public DateTime InOutDate { get; set; }

        /// <summary>
        /// 生成日期
        /// </summary>
        public DateTime DateOfManufacture { get; set; }

        /// <summary>
        /// 是否换货商品
        /// </summary>
        public bool IsExpiredGood { get; set; } = false;
    }

    #endregion

    #region 查询分析用

    public class StockQuery
    {

        public int Id { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 库位号(A001_01_01)
        /// </summary>
        public string PositionCode { get; set; } = "";

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

        public int Version { get; set; }
}

    public class StockQtySummery : IEquatable<StockQtySummery>
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public int SmallUnitId { get; set; }
        public int BigUnitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        public string SmallUnitName { get; set; }
        public string BigUnitName { get; set; }

        /// <summary>
        /// 出入库方向
        /// </summary>
        public int Direction { get; set; }



        /// <summary>
        /// 单据ID（“，”号分割）
        /// </summary>
        public string Bills { get; set; }
        /// <summary>
        /// 单据编号（“，”号分割）
        /// </summary>
        public string BillNumbers { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        [NotMapped]
        public List<int> BillLists { get; set; } = new List<int>();
        [NotMapped]
        public List<string> BillNumberLists { get; set; } = new List<string>();
        [NotMapped]
        public IList<StockQty> Products { get; set; } = new List<StockQty>();


        public List<string> GetBillNumberLists()
        {
            var spliter = BillNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return spliter.ToList();
        }

        public List<int> GetBillLists()
        {
            var spliter = Bills.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return spliter.Select(s => int.Parse(s)).ToList();
        }


        #region override


        //public bool Equals(StockQtySummery obj)
        //{
        //    if (obj == this)
        //        return true;

        //    if (!(obj is StockQtySummery))
        //        return false;

        //    var stockqty = (StockQtySummery)obj;
        //    return stockqty.ProductId == this.UnitId && stockqty.UnitId == this.UnitId && stockqty.Direction == this.Direction;
        //}


        public override int GetHashCode()
        {
            return ProductId.GetHashCode() ^ Direction.GetHashCode();
        }
        public bool Equals(StockQtySummery other)
        {
            return ProductId == other.ProductId && Direction == other.Direction;
        }

        #endregion
    }

    public class StockQty
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int UnitId { get; set; }
        public int SmallUnitId { get; set; }
        public int BigUnitId { get; set; }

        public string UnitName { get; set; }

        public string SmallUnitName { get; set; }
        public string BigUnitName { get; set; }

        public int Direction { get; set; }


        public int StrokeQuantity { get; set; }
        public int BigQuantity { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int BillId { get; set; }
        public int BillType { get; set; }
        public string BillCode { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    #endregion
}
