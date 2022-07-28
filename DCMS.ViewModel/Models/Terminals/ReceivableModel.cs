using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Terminals
{

    public partial class ReceivableListModel : BaseModel
    {

        public ReceivableListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<ReceivableModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ReceivableModel> Lists { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

    }

    public class ReceivableModel : BaseEntityModel
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        /// <summary>
        /// 客户
        /// </summary>
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        /// <summary>
        /// 老板名称
        /// </summary>
        [HintDisplayName("老板名称", "老板名称")]
        public string BossName { get; set; }

        /// <summary>
        /// 老板电话
        /// </summary>
        [HintDisplayName("老板电话", "老板电话")]
        public string BossCall { get; set; }

        /// <summary>
        /// 初始欠款
        /// </summary>
        [HintDisplayName("初始欠款", "初始欠款")]
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 业务员ID
        /// </summary>
        [HintDisplayName("业务员Id", "业务员Id")]
        public int OperationUserId { get; set; } = 0;
        /// <summary>
        /// 业务员姓名
        /// </summary>
        [HintDisplayName("业务员姓名", "业务员姓名")]
        public string OperationUserName { get; set; }
        public SelectList OperationUsers { get; set; }

        /// <summary>
        /// 欠款时间
        /// </summary>
        [HintDisplayName("欠款时间", "欠款时间")]
        [UIHint("DateTime")]
        public DateTime BalanceDate { get; set; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        [HintDisplayName("初始化状态", "初始化状态")]
        public bool Inited { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }
    }

    /// <summary>
    /// 应收款汇总
    /// </summary>
    public class ReceivableSummeriesModel : BaseEntityModel
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        [HintDisplayName("客户Id", "客户Id")]
        public int TerminalId { get; set; } = 0;
        /// <summary>
        /// 客户
        /// </summary>
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        /// <summary>
        /// 老板名称
        /// </summary>
        [HintDisplayName("老板名称", "老板名称")]
        public string BossName { get; set; }

        /// <summary>
        /// 老板电话
        /// </summary>
        [HintDisplayName("老板电话", "老板电话")]
        public string BossCall { get; set; }

        public string BillId { get; set; }
        public string BillType { get; set; }
        public string BillNumber { get; set; }

        /// <summary>
        /// 欠款
        /// </summary>
        [HintDisplayName("初始欠款", "初始欠款")]
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 业务员ID
        /// </summary>
        [HintDisplayName("业务员Id", "业务员Id")]
        public int OperationUserId { get; set; } = 0;
        /// <summary>
        /// 业务员姓名
        /// </summary>
        [HintDisplayName("业务员姓名", "业务员姓名")]
        public string OperationUserName { get; set; }
        public SelectList OperationUsers { get; set; }

        /// <summary>
        /// 欠款时间
        /// </summary>
        [HintDisplayName("欠款时间", "欠款时间")]
        [UIHint("DateTime")]
        public DateTime BalanceDate { get; set; }


    }
}