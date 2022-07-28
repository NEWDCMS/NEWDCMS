using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{


    public partial class RecordingVoucherListModel : BaseModel
    {
        public RecordingVoucherListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<RecordingVoucherModel> Items { get; set; }


        [HintDisplayName("凭证字(记)", "凭证字(记)")]
        public string RecordName { get; set; }


        [HintDisplayName("生成方式", "生成方式")]
        public int? GenerateModeId { get; set; } = 0;
        public GenerateMode GenerateMode { get; set; }


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }


        [HintDisplayName("摘要", "摘要")]
        public string Summary { get; set; }


        [HintDisplayName("会计科目", "会计科目")]
        public int? AccountingOptionId { get; set; } = 0;
        [HintDisplayName("会计科目", "会计科目")]
        public string AccountingOptionName { get; set; }


        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        public DateTime? StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime? EndTime { get; set; }


        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; } = 0;
        public SelectList BillTypes { get; set; }

        public BillTypeEnum BillTypeEnum { get; set; }
    }

    /// <summary>
    /// 用于表示记账凭证
    /// </summary>
    public class RecordingVoucherModel : BaseEntityModel
    {
        public RecordingVoucherModel()
        {
            Vouchers = new List<VoucherItemModel>();
        }


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public int BillId { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int BillTypeId { get; set; } = 0;

        /// <summary>
        /// 生成方式(系统生成:0,手工生成:1)
        [HintDisplayName("生成方式", "生成方式")]
        public int GenerateMode { get; set; } = 0;
        public string GenerateModeName { get; set; }

        [HintDisplayName("凭证字(记)", "凭证字(记)")]
        public string RecordName { get; set; }

        [HintDisplayName("凭证号", "凭证号")]
        public int RecordNumber { get; set; } = 0;


        [HintDisplayName("记账日期", "记账日期")]
        public DateTime RecordTime { get; set; }


        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }


        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }


        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }

        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }

        public string BillLink { get; set; }

        public List<VoucherItemModel> Vouchers { get; set; }
    }


    /// <summary>
    /// 用于表示凭证项目
    /// </summary>
    public class VoucherItemModel : BaseEntityModel
    {

        [HintDisplayName("记账凭证", "记账凭证")]
        public int RecordingVoucherId { get; set; } = 0;

        [HintDisplayName("摘要", "摘要")]
        public string Summary { get; set; }

        [HintDisplayName("会计科目", "会计科目")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }


        [HintDisplayName("借方金额", "借方金额")]
        public decimal? DebitAmount { get; set; } = 0;


        [HintDisplayName("贷方金额", "贷方金额")]
        public decimal? CreditAmount { get; set; } = 0;

        [HintDisplayName("记账方向", "记账方向")]
        public int Direction { get; set; }
    }


    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class RecordingVoucherUpdateModel : BaseEntityModel
    {
        public int BillTypeId { get; set; } = 0;

        public string BillNumber { get; set; }

        public int GenerateMode { get; set; } = 0;

        /// <summary>
        /// 凭证字(记)
        /// </summary>
        public string RecordName { get; set; }

        /// <summary>
        /// 凭证号
        /// </summary>
        public int RecordNumber { get; set; } = 0;


        /// <summary>
        /// 记账日期
        /// </summary>
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<VoucherItemModel> Items { get; set; }

    }



}
