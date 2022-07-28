using DCMS.Core.Domain.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Security
{

    /// <summary>
    /// 表示权限
    /// </summary>
    public class PermissionRecord : BaseEntity
    {
        private ICollection<PermissionRecordRoles> _permissionRecordRoles;


        /// <summary>
        /// 权限名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 权限编码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// 所属模块Id
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 手机端是否支持次功能
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ShowMobile { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        #region  导航属性

        /// <summary>
        /// 模块
        /// </summary>
        [JsonIgnore]
        public virtual Module Module { get; set; }


        /// <summary>
        /// 权限角色集合
        /// </summary>
        public virtual ICollection<PermissionRecordRoles> PermissionRecordRoles
        {
            get { return _permissionRecordRoles ?? (_permissionRecordRoles = new List<PermissionRecordRoles>()); }
            protected set { _permissionRecordRoles = value; }
        }

        #endregion

    }

    /// <summary>
    /// 表示权限角色映射
    /// </summary>
    public class PermissionRecordRoles : BaseEntity
    {

        public int PermissionRecord_Id { get; set; }
        public int UserRole_Id { get; set; }
        /// <summary>
        /// 平台（0 表示PC 端，1：表示APP）
        /// </summary>
        public int Platform { get; set; }

        [JsonIgnore]
        public virtual UserRole UserRole { get; set; }
        [JsonIgnore]
        public virtual PermissionRecord PermissionRecord { get; set; }

    }

    /// <summary>
    /// 表示数据和频道权限
    /// </summary>
    public class DataChannelPermission : BaseEntity
    {


        public int UserRoleId { get; set; }


        #region  价格

        /// <summary>
        /// 是否允许查看进价
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ViewPurchasePrice { get; set; }

        /// <summary>
        /// WEB端开单价格允许低于最低售价
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool PlaceOrderPricePermitsLowerThanMinPriceOnWeb { get; set; }

        /// <summary>
        /// APP销售类单据允许优惠
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool APPSaleBillsAllowPreferences { get; set; }

        /// <summary>
        /// APP预收款单允许优惠
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool APPAdvanceReceiptFormAllowsPreference { get; set; }

        /// <summary>
        /// 最大优惠额度
        /// </summary>
        public decimal? MaximumDiscountAmount { get; set; }

        /// <summary>
        /// APP销售类单据允许欠款
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool APPSaleBillsAllowArrears { get; set; }

        /// <summary>
        /// App开单选赠品权限
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AppOpenChoiceGift { get; set; }

        #endregion

        #region  打印

        /// <summary>
        /// 销售单/销售订单/退货单/退货订单不审核也可以打印 
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool PrintingIsNotAudited { get; set; }


        #endregion

        #region  单据

        /// <summary>
        /// 允许查看自己单据报表
        /// </summary>
        public int AllowViewReportId { get; set; }
        public AllowViewReport AllowViewReport
        {
            get { return (AllowViewReport)AllowViewReportId; }
            set { AllowViewReportId = (int)value; }
        }

        #endregion

        #region  客户档案

        /// <summary>
        /// APP允许修改客户档案
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool APPAllowModificationUserInfo { get; set; }

        #endregion

        #region  通知

        /// <summary>
        /// 审核完成通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Auditcompleted { get; set; }
        /// <summary>
        /// 调度完成通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableSchedulingCompleted { get; set; }
        /// <summary>
        /// 盘点完成通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableInventoryCompleted { get; set; }
        /// <summary>
        /// 转单/签收完成通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableTransfeCompleted { get; set; }
        /// <summary>
        /// 库存预警通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableStockEarlyWarning { get; set; }
        /// <summary>
        /// 客户流失预警通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableUserLossWarning { get; set; }
        /// <summary>
        /// 开单异常通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableBillingException { get; set; }
        /// <summary>
        /// 交账完成/撤销通知
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableCompletionOrCancellationOfAccounts { get; set; }


        #endregion

        #region  待办

        /// <summary>
        /// 审批访问控制
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableApprovalAcl { get; set; }

        /// <summary>
        /// 收款访问控制
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableReceivablesAcl { get; set; }

        /// <summary>
        /// 交账访问控制
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableAccountAcl { get; set; }

        /// <summary>
        /// 资料审核访问控制
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnableDataAuditAcl { get; set; }


        #endregion
    }


}
