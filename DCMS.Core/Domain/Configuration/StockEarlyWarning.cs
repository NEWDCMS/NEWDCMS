using DCMS.Core.Domain.WareHouses;
using System;
namespace DCMS.Core.Domain.Configuration
{

    /// <summary>
    /// 表示库存预警配置
    /// </summary>
    public partial class StockEarlyWarning : BaseEntity
    {



        /// <summary>
        /// 仓库Id
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
        /// 单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 辅助单位
        /// </summary>
        public string AuxiliaryUnit { get; set; }

        /// <summary>
        /// 缺货预警数
        /// </summary>
        public int ShortageWarningQuantity { get; set; }

        /// <summary>
        /// 积压预警数
        /// </summary>
        public int BacklogWarningQuantity { get; set; }


        public DateTime CreatedOnUtc { get; set; }




        public virtual WareHouse WareHouse { get; set; }

    }
}
