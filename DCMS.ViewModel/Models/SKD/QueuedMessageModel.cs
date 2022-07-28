//
using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Tasks
{

    /// <summary>
    /// 消息队列
    /// </summary>
    public partial class QueuedMessageListModel : BaseEntityModel
    {
        public QueuedMessageListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<QueuedMessageModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<QueuedMessageModel> Lists { get; set; }


        public SelectList Stores { get; set; }

        [HintDisplayName("消息类型", "消息类型")]
        public int MTypeId { get; set; } = 0;
        public string MTypeName { get; set; }
        [HintDisplayName("消息类型", "消息类型")]
        public SelectList MTypes { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? SentStatus { get; set; }

        [HintDisplayName("按创建日期排序", "按创建日期排序")]
        public bool? OrderByCreatedOnUtc { get; set; }

        [HintDisplayName("发送次数大于", "发送次数大于")]
        public int? MaxSendTries { get; set; }

        [DisplayName("开始时间")]

        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime EndTime { get; set; }

    }

    public partial class QueuedMessageModel : BaseEntityModel
    {

        [HintDisplayName("经销商", "经销商")]
        public string StoreName { get; set; }
        public SelectList Stores { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        [HintDisplayName("优先级", "优先级")]
        public int Priority { get; set; }

        /// <summary>
        /// 消息：
        /// 0（审批），1（收款），2（交账），3（推送）
        /// 通知：
        /// 4 (审核完成），5 调度完成，6 盘点完成，7 转单/签收完成，8 库存预警，9 签到异常，10 客户流失预警，11 开单异常，12 交账完成/撤销
        /// </summary>
        public MTypeEnum MType
        {
            get { return (MTypeEnum)MTypeId; }
            set { MTypeId = (int)value; }
        }
        [HintDisplayName("消息类型", "消息类型")]
        public int MTypeId { get; set; }
        public string MTypeName { get; set; }
        [HintDisplayName("消息类型", "消息类型")]
        public SelectList MTypes { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [HintDisplayName("标题", "标题")]
        public string Title { get; set; }

        /// <summary>
        /// 消息正文
        /// </summary>
        [HintDisplayName("消息正文", "消息正文")]
        public string Content { get; set; }

        /// <summary>
        /// 显示图标
        /// </summary>
        [HintDisplayName("显示图标", "显示图标")]
        public string Icon { get; set; }

        /// <summary>
        /// 单据创建时间
        /// </summary>
        [HintDisplayName("单据创建时间", "单据创建时间")]
        public DateTime Date { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public BillTypeEnum BillType
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }
        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; }
        public SelectList BillTypes { get; set; }

        /// <summary>
        /// 单据号
        /// </summary>
        [HintDisplayName("单据号", "单据号")]
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据Id
        /// </summary>
        [HintDisplayName("单据Id", "单据Id")]
        public int? BillId { get; set; }

        /// <summary>
        /// 消息创建时间
        /// </summary>
        [HintDisplayName("消息创建时间", "消息创建时间")]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 消息发送时间
        /// </summary>
        [HintDisplayName("消息发送时间", "消息发送时间")]
        public DateTime? SentOnUtc { get; set; }

        /// <summary>
        /// 尝试次数
        /// </summary>
        [HintDisplayName("尝试次数", "尝试次数")]
        public int SentTries { get; set; }

        /// <summary>
        /// 发送用户序列“|”分割
        /// </summary>
        [HintDisplayName("发送用户序列“|”分割", "发送用户序列“|”分割")]
        public string ToUser { get; set; }

        /// <summary>
        /// 客户序列“|”分割
        /// </summary>
        [HintDisplayName("客户序列“|”分割", "客户序列“|”分割")]
        public string TerminalNames { get; set; }

        /// <summary>
        /// 商品序列“|”分割
        /// </summary>
        [HintDisplayName("商品序列“|”分割", "商品序列“|”分割")]
        public string ProductNames { get; set; }

        /// <summary>
        /// 单据序列“|”分割
        /// </summary>
        [HintDisplayName("单据序列“|”分割", "单据序列“|”分割")]
        public string BillNumbers { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        [HintDisplayName("业务员", "业务员")]
        public string BusinessUser { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        /// <summary>
        /// 开单距离
        /// </summary>
        [HintDisplayName("开单距离", "开单距离")]
        public double? Distance { get; set; }

        /// <summary>
        /// 超期月份
        /// </summary>
        [HintDisplayName("超期月份", "超期月份")]
        public int? Month { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [HintDisplayName("金额", "金额")]
        public decimal? Amount { get; set; }

        public bool IsSend { get; set; }
    }


}