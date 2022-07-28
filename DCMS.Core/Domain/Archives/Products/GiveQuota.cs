using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 表示赠品额度
    /// </summary>

    public class GiveQuota : BaseEntity
    {
        [JsonIgnore]
        private ICollection<GiveQuotaOption> _giveQuotaOptions;

        /// <summary>
        /// 主管
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        [JsonIgnore]
        public virtual ICollection<GiveQuotaOption> GiveQuotaOptions
        {
            get { return _giveQuotaOptions ?? (_giveQuotaOptions = new List<GiveQuotaOption>()); }
            protected set { _giveQuotaOptions = value; }
        }

    }

    /// <summary>
    /// 表示赠品额度项目
    /// </summary>
    public class GiveQuotaOption : BaseEntity
    {
        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; } = 0;


        /// <summary>
        /// 各月量
        /// </summary>
        [Tag(1)]
        public decimal? Jan { get; set; }
        [Tag(2)]
        public decimal? Feb { get; set; }
        [Tag(3)]
        public decimal? Mar { get; set; }
        [Tag(4)]
        public decimal? Apr { get; set; }
        [Tag(5)]
        public decimal? May { get; set; }
        [Tag(6)]
        public decimal? Jun { get; set; }
        [Tag(7)]
        public decimal? Jul { get; set; }
        [Tag(8)]
        public decimal? Aug { get; set; }
        [Tag(9)]
        public decimal? Sep { get; set; }
        [Tag(10)]
        public decimal? Oct { get; set; }
        [Tag(11)]
        public decimal? Nov { get; set; }
        [Tag(12)]
        public decimal? Dec { get; set; }

        /// <summary>
        /// 各月余额 注意这些都是最小单位数量
        /// </summary>
        public decimal? Jan_Balance { get; set; } = 0;
        public decimal? Feb_Balance { get; set; } = 0;
        public decimal? Mar_Balance { get; set; } = 0;
        public decimal? Apr_Balance { get; set; } = 0;
        public decimal? May_Balance { get; set; } = 0;
        public decimal? Jun_Balance { get; set; } = 0;
        public decimal? Jul_Balance { get; set; } = 0;
        public decimal? Aug_Balance { get; set; } = 0;
        public decimal? Sep_Balance { get; set; } = 0;
        public decimal? Oct_Balance { get; set; } = 0;
        public decimal? Nov_Balance { get; set; } = 0;
        public decimal? Dec_Balance { get; set; } = 0;


        /// <summary>
        /// 总计
        /// </summary>
        public decimal? Total { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }



        /// <summary>
        /// 赠品额度
        /// </summary>
        public int GiveQuotaId { get; set; }

        public virtual GiveQuota GiveQuota { get; set; }
    }

    /// <summary>
    /// 用于表示赠送记录（可用于汇总）
    /// </summary>
    public class GiveQuotaRecords : BaseEntity
    {

        /// <summary>
        /// 单据来源（销售/订）
        /// </summary>
        public int? BillId { get; set; } = 0;

        /// <summary>
        /// 单据来源（促销）
        /// </summary>
        public int? CampaignId { get; set; } = 0;

        /// <summary>
        /// 单据来源（合同）
        /// </summary>
        public int? ContractId { get; set; } = 0;

        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }


        /// <summary>
        /// 成本计算方式
        /// </summary>
        public int? CostingCalCulateMethodId { get; set; } = 0;


        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int? UnitId { get; set; } = 0;
        public string UnitName { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 赠品类型
        /// </summary>
        public int GiveTypeId { get; set; }
        public GiveTypeEnum GiveType
        {
            get { return (GiveTypeEnum)GiveTypeId; }
            set { GiveTypeId = (int)value; }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// 成本
        /// </summary>
        public decimal CostAmount { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 兑付年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 兑付月份
        /// </summary>
        public int Monthly { get; set; }
    }

    /// <summary>
    /// 用于表示赠品汇总
    /// </summary>
    public class GiveQuotaRecordsSummery
    {
        public long Id { get; set; }
        public string Records { get; set; }
        public int StoreId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }


        /// <summary>
        /// 终端客户
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 普通赠品
        /// </summary>
        public int GeneralQuantity { get; set; } = 0;
        public Tuple<int, int, int, decimal> GeneralQuantityTuple { get; set; }
        public string GeneralQuantityFormat { get; set; }
        public decimal GeneralCostAmount { get; set; } = 0;

        /// <summary>
        /// 订货赠品
        /// </summary>
        public int OrderQuantity { get; set; } = 0;
        public Tuple<int, int, int, decimal> OrderQuantityTuple { get; set; }
        public string OrderQuantityFormat { get; set; }
        public decimal OrderCostAmount { get; set; } = 0;

        /// <summary>
        /// 促销赠品
        /// </summary>
        public int PromotionalQuantity { get; set; } = 0;
        public Tuple<int, int, int, decimal> PromotionalQuantityTuple { get; set; }
        public string PromotionalQuantityFormat { get; set; }
        public decimal PromotionalCostAmount { get; set; } = 0;

        /// <summary>
        /// 费用合同
        /// </summary>
        public int ContractQuantity { get; set; } = 0;
        public Tuple<int, int, int, decimal> ContractQuantityTuple { get; set; }
        public string ContractQuantityFormat { get; set; }
        public decimal ContractCostAmount { get; set; } = 0;
        //普通赠品分类
        public IList<OrdinaryGiftSummery> OrdinaryGiftSummerys { get; set; }
    }

    public class OrdinaryGiftSummery
    {
        public int RemarkConfigId { get; set; }

        public string Quantity { get; set; } = "";

        public decimal CostAmount { get; set; } =0;

        public Tuple<int, int, int, decimal> QuantityTuple { get; set; }
    }

    /// <summary>
    /// 用于表示赠品汇总查询结构体
    /// </summary>
    public class GiveSummery
    {
        /// <summary>
        /// 汇总类型 1：普通赠品，3：促销赠品，4：费用合同
        /// </summary>
        public int Gtype { get; set; }

        /// <summary>
        /// 终端客户
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 大单位转换量
        /// </summary>
        public int BigQuantity { get; set; }

        /// <summary>
        /// 小/中/大单位
        /// </summary>
        public int SmallUnitId { get; set; }
        public int StrokeUnitId { get; set; }
        public int BigUnitId { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        /// <summary>
        /// 赠送量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 成本价格
        /// </summary>
        public decimal CostAmount { get; set; }


        /// <summary>
        /// 小/中/大单位名称
        /// </summary>
        public string SUnitName { get; set; }
        public string MUnitName { get; set; }
        public string BUnitName { get; set; }
        public int? RemarkConfigId { get; set; }
    }


    public class GiveQuotaUpdate : BaseEntity
    {
        //移除 = 0;
        public int? UserId { get; set; } = 0;
        public int? Year { get; set; } = 0;
        public string Remark { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<GiveQuotaOption> Items { get; set; }
    }

}
