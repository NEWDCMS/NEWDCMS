using DCMS.ViewModel.Models.Configuration;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{
    public partial class CostContractBillListModel : BaseModel
    {
        public CostContractBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CostContractBillModel> Items { get; set; }

        public List<string> DynamicColumns { get; set; }

        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


        [HintDisplayName("员工", "员工")]
        public int? EmployeeId { get; set; } = 0;
        public string EmployeeName { get; set; }
        public SelectList Employees { get; set; }


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        public DateTime? StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime? EndTime { get; set; }
    }


    /// <summary>
    /// 用于表示费用合同单据
    /// </summary>
    public class CostContractBillModel : BaseEntityModel
    {

        public CostContractBillModel()
        {
            Items = new List<CostContractItemModel>();
        }

        public int BillTypeEnumId { get; set; }

        public int RowIndex { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }
        public string CustomerPointCode { get; set; }

        [HintDisplayName("员工", "员工")]
        public int? EmployeeId { get; set; } = 0;
        public string EmployeeName { get; set; }
        public SelectList Employees { get; set; }

        [HintDisplayName("主管", "主管")]
        public int? LeaderId { get; set; } = 0;
        public string LeaderName { get; set; }
        public SelectList Leaders { get; set; }

        [HintDisplayName("年份", "年份")]
        public int Year { get; set; } = 0;

        [HintDisplayName("月份", "月份")]
        public int Month { get; set; } = 0;

        /// <summary>
        /// 合同类型(0:按月兑付,1:按单位量总计兑付,2:从主管赠品扣减)
        /// </summary>
        public int ContractType { get; set; }


        [HintDisplayName("费用类别", "费用类别")]
        public int AccountingOptionId { get; set; } = 0;
        public int AccountCodeTypeId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public List<AccountingOptionModel> AccountingOptionSelects { get; set; } = new List<AccountingOptionModel>();


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }


        [HintDisplayName("销售备注", "销售备注")]
        public string SaleRemark { get; set; }

        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }


        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }

        public string AuditedStatusName { get; set; }
        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }


        [HintDisplayName("驳回人", "驳回人")]
        public int? RejectUserId { get; set; } = 0;


        [HintDisplayName("状态(驳回)", "状态(驳回)")]
        public bool RejectedStatus { get; set; }


        [HintDisplayName("驳回时间", "驳回时间")]
        public DateTime? RejectedDate { get; set; }


        [HintDisplayName("终止合同", "终止合同")]
        public int? AbandonedUserId { get; set; } = 0;


        [HintDisplayName("终止状态", "终止状态")]
        public bool AbandonedStatus { get; set; }

        [HintDisplayName("终止时间", "终止时间")]
        public DateTime? AbandonedDate { get; set; }


        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;


        [HintDisplayName("是否按月兑现", "是否按月兑现")]
        public bool CashonMonthlyBasis { get; set; }


        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;
        ///// <summary>
        ///// 开始时间
        ///// </summary>
        //public DateTime StartTime { get; set; }
        ///// <summary>
        ///// 结束时间
        ///// </summary>
        //public DateTime EndTime { get; set; }
        /// <summary>
        /// TPM协议编码（PZC00000000000）
        /// </summary>
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public IList<CostContractItemModel> Items { get; set; }

    }


    /// <summary>
    /// 用于表示费用合同单据项目
    /// </summary>
    public class CostContractItemModel : ProductBaseModel
    {

        [HintDisplayName("费用合同单", "费用合同单Id")]
        public int CostContractBillId { get; set; } = 0;


        /// <summary>
        /// 合同类型
        /// </summary>
        public string ContractTypeName { get; set; }
        public int ContractType { get; set; } = 0;

        /// <summary>
        /// 0:商品，1：现金
        /// </summary>
        public int CType { get; set; } = 0;
        public string Name { get; set; }

        public int? BigUnitId { get; set; } = 0;

        public int? SmallUnitId { get; set; } = 0;

        public int? BigUnitQuantity { get; set; } = 0;

        public int? SmallUnitQuantity { get; set; } = 0;

        public string SmallUnitName { get; set; }


        public decimal? Jan { get; set; } = 0;
        public decimal? Feb { get; set; } = 0;
        public decimal? Mar { get; set; } = 0;
        public decimal? Apr { get; set; } = 0;
        public decimal? May { get; set; } = 0;
        public decimal? Jun { get; set; } = 0;
        public decimal? Jul { get; set; } = 0;
        public decimal? Aug { get; set; } = 0;
        public decimal? Sep { get; set; } = 0;
        public decimal? Oct { get; set; } = 0;
        public decimal? Nov { get; set; } = 0;
        public decimal? Dec { get; set; } = 0;


        /// <summary>
        /// 按月兑付时记录各月余额
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
        /// 当使用ContractType= 2:从主管赠品扣减时，Total 只能小于等于主管月份的额度
        /// </summary>
        [HintDisplayName("总计", "总计")]
        public decimal? Total { get; set; } = 0;

        public int TotalQuantity { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }



        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string MonthName { get; set; }

        /// <summary>
        /// 剩余数量或金额
        /// </summary>
        public decimal AvailableQuantityOrAmount { get; set; }


        /// <summary>
        /// 主管赠品 （年份+主管）
        /// </summary>
        public int GiveQuotaId { get; set; }
        /// <summary>
        /// 主管赠品项 （年份+主管+商品+月份）
        /// </summary>
        public int GiveQuotaOptionId { get; set; }

        public int SaleProductTypeId { get; set; } = 0;
        public string SaleProductTypeName { get; set; }

        /// <summary>
        /// 赠品类型 关联 GiveTypeEnum 枚举
        /// </summary>
        public int GiveTypeId { get; set; } = 0;

        public decimal Price { get; set; } = 0;
        public decimal CostPrice { get; set; } = 0;


    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CostContractUpdateModel : BaseEntityModel
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
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }
        ///// <summary>
        ///// 开始时间
        ///// </summary>
        //public DateTime? StartTime { get; set; }
        ///// <summary>
        ///// 结束时间
        ///// </summary>
        //public DateTime? EndTime { get; set; }
        /// <summary>
        /// TPM协议编码
        /// </summary>
        public string ProtocolNum { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<CostContractItemModel> Items { get; set; }


    }


    /// <summary>
    /// 合同选择绑定
    /// </summary>
    public class CostContractBindingModel : BaseEntityModel
    {
        /// <summary>
        /// 合同编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }

        public decimal? Amount { get; set; } = 0;

        /// <summary>
        /// 当月余额
        /// </summary>
        public decimal? Balance { get; set; } = 0;

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }
        public string MonthName { get; set; }


        /// <summary>
        /// 费用类别
        /// </summary>
        public int AccountingOptionId { get; set; }
        public int AccountCodeTypeId { get; set; }
        public string AccountingOptionName { get; set; }

    }
}
