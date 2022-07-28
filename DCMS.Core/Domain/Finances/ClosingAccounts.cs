using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示期末结账
    /// </summary>
    public class ClosingAccounts : BaseEntity
    {

        /// <summary>
        /// 结转日期(例：2019-04)
        /// </summary>
        public DateTime ClosingAccountDate { get; set; }

        /// <summary>
        /// (锁账状态)
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool LockStatus { get; set; }

        /// <summary>
        /// 结账人
        /// </summary>
        public int? CheckUserId { get; set; } = 0;

        /// <summary>
        /// 是否已结
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool CheckStatus { get; set; }

        /// <summary>
        /// 结账时间
        /// </summary>
        public DateTime? CheckDate { get; set; }

    }


    /// <summary>
    /// 用于表示成本汇总表
    /// </summary>
    public class CostPriceSummery : BaseEntity
    {

        /// <summary>
        /// 结转日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }

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
        public string UnitName { get; set; }


        #region 期初

        /// <summary>
        /// 期初结存商品数量
        /// </summary>
        public int InitQuantity { get; set; } = 0;

        /// <summary>
        /// 期初结存商品成本价格
        /// </summary>
        public decimal InitPrice { get; set; } = 0;

        /// <summary>
        /// 期初结存商品金额
        /// </summary>
        public decimal InitAmount { get; set; } = 0;

        #endregion

        #region 收入

        /// <summary>
        ///  本期收入商品数量
        /// </summary>
        public int InQuantity { get; set; } = 0;

        /// <summary>
        /// 本期收入商品成本价格
        /// </summary>
        public decimal InPrice { get; set; } = 0;

        /// <summary>
        /// 本期收入商品成本金额
        /// </summary>
        public decimal InAmount { get; set; } = 0;

        #endregion

        #region 支出

        /// <summary>
        ///  本期支出商品数量
        /// </summary>
        public int OutQuantity { get; set; } = 0;

        /// <summary>
        /// 本期支出商品成本价格
        /// </summary>
        public decimal OutPrice { get; set; } = 0;

        /// <summary>
        /// 本期支出商品成本金额
        /// </summary>
        public decimal OutAmount { get; set; } = 0;


        /// <summary>
        /// 本期非销售发出(支出)商品数量
        /// </summary>
        public int OutNoSaleQuantity { get; set; } = 0;

        /// <summary>
        /// 本期非销售发出(支出)商品金额
        /// </summary>
        public decimal OutNoSaleAmount { get; set; } = 0;

        #endregion

        #region 结存

        /// <summary>
        /// 期末结存数量
        /// </summary>
        public int EndQuantity { get; set; } = 0;

        /// <summary>
        /// 期末结存成本价格
        /// </summary>
        public decimal EndPrice { get; set; } = 0;

        /// <summary>
        ///期末结存成本金额
        /// </summary>
        public decimal EndAmount { get; set; } = 0;

        #endregion

        /// <summary>
        /// 商品明细记录
        /// </summary>
        public IList<CostPriceChangeRecords> Records { get; set; } = new List<CostPriceChangeRecords>();

    }


    /// <summary>
    /// 用于表示成本变化明细记录
    /// </summary>
    public class CostPriceChangeRecords : BaseEntity
    {

        /// <summary>
        /// 汇总Id
        /// </summary>
        public int CostPriceSummeryId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 开单日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }
        public string BillTypeName { get; set; }

        public int BillId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }


        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }

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
        public string UnitName { get; set; }



        #region 收入

        /// <summary>
        /// 数量
        /// </summary>
        public int InQuantity { get; set; } = 0;

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal InPrice { get; set; } = 0;

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal InAmount { get; set; } = 0;

        #endregion

        #region 支出

        /// <summary>
        /// 数量
        /// </summary>
        public int OutQuantity { get; set; } = 0;

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal OutPrice { get; set; } = 0;

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal OutAmount { get; set; } = 0;

        #endregion

        #region 结存

        /// <summary>
        /// 数量
        /// </summary>
        public int EndQuantity { get; set; } = 0;

        /// <summary>
        /// 成本单价
        /// </summary>
        public decimal EndPrice { get; set; } = 0;

        /// <summary>
        /// 金额
        /// </summary>
        public decimal EndAmount { get; set; } = 0;

        #endregion
    }

}
