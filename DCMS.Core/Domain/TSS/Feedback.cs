using System;

namespace DCMS.Core.Domain.TSS
{
    /// <summary>
    /// 表示 TSS 服务支持（意见反馈）
    /// </summary>
    public class Feedback : BaseEntity
    {
        /// <summary>
        /// 类别
        /// </summary>
        public int FeedbackTyoe { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string IssueDescribe { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// 图片一
        /// </summary>
        public string Screenshot1 { get; set; }
        /// <summary>
        /// 图片二
        /// </summary>
        public string Screenshot2 { get; set; }
        /// <summary>
        /// 图片三
        /// </summary>
        public string Screenshot3 { get; set; }
        /// <summary>
        /// 图片四
        /// </summary>
        public string Screenshot4 { get; set; }
        /// <summary>
        /// 图片五
        /// </summary>
        public string Screenshot5 { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
    }


    /// <summary>
    /// 市场反馈
    /// </summary>
    public class MarketFeedback : BaseEntity
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int MType { get; set; }
        public string CompetitiveDescribe { get; set; }
        public string ConditionDescribe { get; set; }

        /// <summary>
        /// 图片一
        /// </summary>
        public string Screenshot1 { get; set; }
        /// <summary>
        /// 图片二
        /// </summary>
        public string Screenshot2 { get; set; }
        /// <summary>
        /// 图片三
        /// </summary>
        public string Screenshot3 { get; set; }
        /// <summary>
        /// 图片四
        /// </summary>
        public string Screenshot4 { get; set; }
        /// <summary>
        /// 图片五
        /// </summary>
        public string Screenshot5 { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
    }
}
