using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Finances
{
    /// <summary>
    /// 用于表示记账凭证
    /// </summary>
    public class RecordingVoucher : BaseBill<VoucherItem>
    {

        public RecordingVoucher()
        {
            BillType = BillTypeEnum.RecordingVoucher;
        }
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; } = 0;

        /// <summary>
        /// 生成方式(手工生成:0,系统生成:1,应收系统:2,应付系统:3,固定资产:4,工资系统:5)
        /// </summary>
        public int GenerateMode { get; set; }
        public VouchSourceEnum Generate
        {
            get => (VouchSourceEnum)GenerateMode;
            set => GenerateMode = (int)value;
        }


        /// <summary>
        /// 凭证字(记) 格式：记-xxx
        /// </summary>
        public string RecordName { get; set; }

        /// <summary>
        /// 凭证号
        /// </summary>
        public int RecordNumber { get; set; }

        /// <summary>
        /// 记账日期
        /// </summary>
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 记帐标志(0—未记帐 1—记帐凭证)
        /// </summary>
        public int BookFlag { get; set; }
    }


    /// <summary>
    /// 用于表示凭证项目
    /// </summary>
    public class VoucherItem : BaseEntity
    {
        /// <summary>
        /// 记账凭证
        /// </summary>
        public int RecordingVoucherId { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 会计科目
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;

        /// <summary>
        /// 会计科目名称
        /// </summary>
        public string AccountingOptionName { get; set; }

        public string AccountingCode { get; set; }

        /// <summary>
        /// 借方金额
        /// </summary>
        public decimal? DebitAmount { get; set; } = 0;

        /// <summary>
        /// 贷方金额
        /// </summary>
        public decimal? CreditAmount { get; set; } = 0;

        /// <summary>
        /// 记账日期
        /// </summary>
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 参考记账方向：0，借；1，贷；(记账方向只有2个，要么借，要么贷)
        /// </summary>
        public int Direction { get; set; } = -1;

        public virtual RecordingVoucher RecordingVoucher { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class RecordingVoucherUpdate : BaseEntity
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
        public List<VoucherItem> Items { get; set; }

    }

}
