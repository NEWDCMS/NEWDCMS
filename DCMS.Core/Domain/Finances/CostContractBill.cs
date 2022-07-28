using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示费用合同单据
    /// </summary>
    public class CostContractBill : BaseBill<CostContractItem>
    {
        public CostContractBill()
        {
            BillType = BillTypeEnum.CostContractBill;
        }

        /// <summary>
        /// 主管
        /// </summary>
        public int? LeaderId { get; set; } = 0;

        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 员工
        /// </summary>
        public int EmployeeId { get; set; }


        /// <summary>
        /// 费用类别
        /// </summary>
        public int AccountingOptionId { get; set; }
        public int AccountCodeTypeId { get; set; }

        /// <summary>
        /// 合同类型(0:按月兑付,1:按单位量总计兑付,2:从主管赠品扣减)
        /// </summary>
        public int? ContractType { get; set; } = 0;

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; } = 0;

        /// <summary>
        /// 月份
        /// </summary>
        public int? Month { get; set; } = 0;

        /// <summary>
        /// 销售备注
        /// </summary>
        public string SaleRemark { get; set; }

        /// <summary>
        /// 驳回人
        /// </summary>
        public int? RejectUserId { get; set; } = 0;
        /// <summary>
        /// 状态(驳回)
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool RejectedStatus { get; set; }
        /// <summary>
        /// 驳回时间
        /// </summary>
        public DateTime? RejectedDate { get; set; }

        /// <summary>
        /// 终止合同
        /// </summary>
        public int? AbandonedUserId { get; set; } = 0;
        /// <summary>
        /// 终止状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AbandonedStatus { get; set; }
        /// <summary>
        ///终止时间
        /// </summary>
        public DateTime? AbandonedDate { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; } = 0;

        /// <summary>
        /// 是否按月兑现
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool CashonMonthlyBasis { get; set; }

        ///// <summary>
        ///// 开始时间
        ///// </summary>
        //public DateTime? StartTime { get; set; }
        ///// <summary>
        ///// 结束时间
        ///// </summary>
        //public DateTime? EndTime { get; set; }
        /// <summary>
        /// TPM协议编码（PZC00000000000）PZC+11位 数字   TLX合同 11位
        /// </summary>
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }
    }

    public class MonthAllocateQuota : BaseEntity
    {
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
    }

    /// <summary>
    /// 用于表示费用合同单据项目
    /// </summary>
    public class CostContractItem : MonthAllocateQuota
    {

        /// <summary>
        /// 费用合同单Id
        /// </summary>
        public int CostContractBillId { get; set; }

        /// <summary>
        /// 0:商品，1：现金
        /// </summary>
        public int? CType { get; set; } = 0;
        public string Name { get; set; }
        public string UnitName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; } = 0;

        //-------------
        public int? BigUnitId { get; set; } = 0;

        public int? SmallUnitId { get; set; } = 0;

        public int? BigUnitQuantity { get; set; } = 0;

        public int? SmallUnitQuantity { get; set; } = 0;


        /// <summary>
        /// 按月兑付时记录各月余额。  注意：如果是商品 则 这些都是最小单位数量
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
        ///  ContractType= 1:按单位量总计兑付时才记录
        /// </summary>
        public decimal? Total_Balance { get; set; } = 0;

        /// <summary>
        /// 总计 当使用ContractType= 2:从主管赠品扣减时，Total 只能小于等于主管月份的额度
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        public virtual CostContractBill CostContractBill { get; set; }


        /// <summary>
        /// 主管赠品 （年份+主管）
        /// </summary>
        public int GiveQuotaId { get; set; } = 0;
        /// <summary>
        /// 主管赠品项 （年份+主管+商品+月份）
        /// </summary>
        public int GiveQuotaOptionId { get; set; } = 0;
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CostContractBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; } = 0;

        public int LeaderId { get; set; } = 0;

        /// <summary>
        /// 员工
        /// </summary>
        public int EmployeeId { get; set; } = 0;

        /// <summary>
        /// 合同类型(0:按月兑付,1:按年兑付,2:从主管赠品扣减)
        /// </summary>
        public int ContractType { get; set; }

        /// <summary>
        /// 费用类别
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;
        public int AccountCodeTypeId { get; set; } = 0;
        
        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; } = 0;

        public int Month { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 销售备注
        /// </summary>
        public string SaleRemark { get; set; }
        /// <summary>
        /// TPM协议编码
        /// </summary> 
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<CostContractItem> Items { get; set; }


    }

}
