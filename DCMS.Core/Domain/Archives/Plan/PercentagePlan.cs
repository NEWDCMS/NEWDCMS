using DCMS.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Plan
{

    /// <summary>
    /// 表示提成方案
    /// </summary>
    public class PercentagePlan : BaseEntity
    {

        private ICollection<Percentage> _percentages;


        /// <summary>
        /// 方案名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 方案类型
        /// </summary>
        public int PlanTypeId { get; set; }
        /// <summary>
        /// 方案类型
        /// </summary>
        public PercentagePlanType PlanType
        {
            get { return (PercentagePlanType)PlanTypeId; }
            set { PlanTypeId = (int)value; }
        }

        /// <summary>
        /// 是否按回款提成
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsByReturn { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        public virtual ICollection<Percentage> Percentages
        {
            get { return _percentages ?? (_percentages = new List<Percentage>()); }
            protected set { _percentages = value; }
        }

    }


    /// <summary>
    /// 表示员工提成配置
    /// </summary>
    public class Percentage : BaseEntity
    {

        private ICollection<PercentageRangeOption> _percentageRangeOptions;


        /// <summary>
        /// 方案ID
        /// </summary>
        public int PercentagePlanId { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CatagoryId { get; set; } = 0;

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; } = 0;

        /// <summary>
        /// 品项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 员工提成计算方式
        /// </summary>
        public int CalCulateMethodId { get; set; }
        public PercentageCalCulateMethod CalCulateMethod
        {
            get { return (PercentageCalCulateMethod)CalCulateMethodId; }
            set { CalCulateMethodId = (int)value; }
        }


        /// <summary>
        /// 数量核算方式
        /// </summary>
        public int QuantityCalCulateMethodId { get; set; }
        public QuantityCalCulateMethod QuantityCalCulateMethod
        {
            get { return (QuantityCalCulateMethod)QuantityCalCulateMethodId; }
            set { QuantityCalCulateMethodId = (int)value; }
        }


        /// <summary>
        /// 成本计算方式
        /// </summary>
        public int CostingCalCulateMethodId { get; set; }
        public CostingCalCulateMethod CostingCalCulateMethod
        {
            get { return (CostingCalCulateMethod)CostingCalCulateMethodId; }
            set { CostingCalCulateMethodId = (int)value; }
        }


        /// <summary>
        /// 是否退货参与提成
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsReturnCalCulated { get; set; }

        /// <summary>
        /// 是否赠品参与提成
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGiftCalCulated { get; set; }


        /// <summary>
        /// 销售提成比例
        /// </summary>
        public decimal? SalesPercent { get; set; }

        /// <summary>
        /// 退货提成比例
        /// </summary>
        public decimal? ReturnPercent { get; set; }

        /// <summary>
        /// 销售提成金额
        /// </summary>
        public decimal? SalesAmount { get; set; }
        /// <summary>
        /// 退货提成金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }




        public virtual ICollection<PercentageRangeOption> PercentageRangeOptions
        {
            get { return _percentageRangeOptions ?? (_percentageRangeOptions = new List<PercentageRangeOption>()); }
            protected set { _percentageRangeOptions = value; }
        }

        /// <summary>
        /// 方案
        /// </summary>
        public virtual PercentagePlan PercentagePlan { get; set; }

    }


    /// <summary>
    /// 按利润区间范围计算提成
    /// </summary>
    public class PercentageRangeOption : BaseEntity
    {
        /// <summary>
        /// 净销售额区间范围
        /// </summary>
        public decimal? NetSalesRange { get; set; }

        /// <summary>
        /// 销售提成(%)
        /// </summary>
        public decimal? SalesPercent { get; set; }

        //退货提成(%)
        public decimal? ReturnPercent { get; set; }

        /// <summary>
        /// 员工提成配置Id
        /// </summary>
        public int PercentageId { get; set; }

        /// <summary>
        /// 员工提成
        /// </summary>
        public virtual Percentage Percentage { get; set; }

    }


    /// <summary>
    /// 保存序列
    /// </summary>
    public class PercentageSerializeUpdate : BaseEntity
    {
        public Percentage Percentage { get; set; }
    }


}
