//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Tasks
{

    public partial class QueuedEmailListModel : BaseEntityModel
    {
        public QueuedEmailListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<QueuedEmailModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<QueuedEmailModel> Lists { get; set; }


        public SelectList Stores { get; set; }

        [HintDisplayName("发送人", "发送人")]
        public string From { get; set; }

        [HintDisplayName("收件人", "收件人")]
        public string To { get; set; }

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


    //[Validator(typeof(QueuedEmailValidator))]
    public partial class QueuedEmailModel : BaseEntityModel
    {


        [HintDisplayName("经销商", "经销商")]
        public string StoreName { get; set; }
        public SelectList Stores { get; set; }

        [HintDisplayName("优先级别", "优先级别")]
        public int Priority { get; set; }

        [HintDisplayName("发送人", "发送人")]

        public string From { get; set; }

        [HintDisplayName("发送人", "发送人")]

        public string FromName { get; set; }

        [HintDisplayName("收件人", "收件人")]

        public string To { get; set; }

        [HintDisplayName("收件人", "收件人")]

        public string ToName { get; set; }

        [HintDisplayName("抄送", "抄送")]

        public string CC { get; set; }

        [HintDisplayName("暗抄送", "暗抄送")]

        public string Bcc { get; set; }

        [HintDisplayName("标题", "标题")]

        public string Subject { get; set; }

        [HintDisplayName("正文", "正文")]

        public string Body { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("发送次数", "发送次数")]
        public int SentTries { get; set; }

        [HintDisplayName("发送时间", "发送时间")]
        [DisplayFormat(DataFormatString = "{0}", NullDisplayText = "Not sent yet")]
        public DateTime? SentOnUtc { get; set; }

        [HintDisplayName("账户Id", "账户Id")]

        public int EmailAccountId { get; set; }
        [HintDisplayName("账户名称", "账户名称")]

        public string EmailAccountName { get; set; }
        public SelectList EmailAccounts { get; set; }

        public bool IsSend { get; set; }
    }









}