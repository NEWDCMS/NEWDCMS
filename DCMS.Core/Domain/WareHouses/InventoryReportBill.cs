using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于门店库存上报
    /// </summary>
    public class InventoryReportBill : BaseBill<InventoryReportItem>
    {

        public InventoryReportBill()
        {
            BillType = BillTypeEnum.InventoryReportBill;
        }


        //private ICollection<InventoryReportItem> _inventoryReportItems;

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }
        ///// <summary>
        ///// 红冲人
        ///// </summary>
        //public int? ReversedUserId { get; set; }
        ///// <summary>
        ///// 红冲状态
        ///// </summary>
        //[Column(TypeName = "BIT(1)")]
        //public bool ReversedStatus { get; set; }
        ///// <summary>
        ///// 红冲时间
        ///// </summary>
        //public DateTime? ReversedDate { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public int Operation { get; set; }

        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        //public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        //public string Remark { get; set; }


        //public virtual ICollection<InventoryReportItem> InventoryReportItems
        //{
        //    get { return _inventoryReportItems ?? (_inventoryReportItems = new List<InventoryReportItem>()); }
        //    protected set { _inventoryReportItems = value; }
        //}
    }

    /// <summary>
    /// 上报关联商品
    /// </summary>
    public class InventoryReportItem : BaseEntity
    {

        private ICollection<InventoryReportStoreQuantity> _inventoryReportStoreQuantities;



        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 采购大单位
        /// </summary>
        public int BigUnitId { get; set; }

        /// <summary>
        /// 采购大单位数量
        /// </summary>
        public int BigQuantity { get; set; }


        /// <summary>
        /// 采购小单位
        /// </summary>
        public int SmallUnitId { get; set; }

        /// <summary>
        /// 采购小单位数量
        /// </summary>
        public int SmallQuantity { get; set; }

        //Stroke
        /// <summary>
        /// 采购中单位
        /// </summary>
        public int StrokeUnitId { get; set; }

        /// <summary>
        /// 采购中单位数量
        /// </summary>
        public int StrokeQuantity { get; set; }

        #region 导航

        public int InventoryReportBillId { get; set; }
        public virtual InventoryReportBill InventoryReportBill { get; set; }

        public virtual ICollection<InventoryReportStoreQuantity> InventoryReportStoreQuantities
        {
            get { return _inventoryReportStoreQuantities ?? (_inventoryReportStoreQuantities = new List<InventoryReportStoreQuantity>()); }
            protected set { _inventoryReportStoreQuantities = value; }
        }

        #endregion

    }


    /// <summary>
    /// 商品关联库存量
    /// </summary>
    public class InventoryReportStoreQuantity : BaseEntity
    {


        /// <summary>
        /// 大单位库存量
        /// </summary>
        public int BigStoreQuantity { get; set; }

        /// <summary>
        /// 小单位库存量
        /// </summary>
        public int SmallStoreQuantity { get; set; }

        /// <summary>
        /// 中单位库存量
        /// </summary>
        public int StrokeStoreQuantity { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDete { get; set; }


        #region 导航

        public int InventoryReportItemId { get; set; }
        public virtual InventoryReportItem InventoryReportItem { get; set; }

        #endregion

    }


    /// <summary>
    /// 上报汇总表
    /// </summary>
    public class InventoryReportSummary : BaseEntity
    {


        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }
        //public string TerminalName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }
        //public string ProductName { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        //public string UnitConversion { get; set; }

        /// <summary>
        /// 大单位
        /// </summary>
        //public int BigUnitId { get; set; }
        //public string BigUnitName { get; set; }

        /// <summary>
        /// 小单位
        /// </summary>
        //public int SmallUnitId { get; set; }
        //public string SmallUnitName { get; set; }

        /// <summary>
        /// 期初时间
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// 期末时间
        /// </summary>
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// 期初库存量
        /// </summary>
        public int BeginStoreQuantity { get; set; }
        /// <summary>
        /// 期末库存量
        /// </summary>
        public int EndStoreQuantity { get; set; }


        /// <summary>
        /// 采购量
        /// </summary>
        public int PurchaseQuantity { get; set; }


        /// <summary>
        /// 销售量
        /// </summary>
        public int SaleQuantity { get; set; }

    }


    /*
     * InventoryReportBill，ReportItem，ReportStoreQuantity 用于记录，InventoryReportSummary 用于汇总
     *  每次上报时更新或者插入上报汇总表（InventoryReportSummary）
        汇总时需要用到上报汇总表（ 这里的结构应在 ViewModel 绑定  ）：

        //客户
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        //商品
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        //单位换算
        public string UnitConversion { get; set; }

        //期初时间
        public DateTime  BeginDate  { get; set; }
        //期末时间
        public DateTime  EndDate  { get; set; }


        //期初库存量
        public int  BeginStoreQuantity  { get; set; }
        //期末库存量
        public int  EndStoreQuantity { get; set; }


        //采购量
        public int  PurchaseQuantity { get; set; }


        //销售量
        public int  SaleSum { get; set; }


        1. 第一次上报商品 时，期末时间未空，有期初，采购量为上报采购量， 期初库存量 = 采购量。
        2. 第二次/之后任何一笔上报认为之前一笔的期末，本期采购量为 = 期初采购量+本次采购量，期初库存不变，期末库存 =  本次上报库存， 期末时间为=本次上报时间
           销售量=  期初库存+ 本期采购（期初采购量+本次采购量）- 期末库存（本次上报库存）
     */

}
