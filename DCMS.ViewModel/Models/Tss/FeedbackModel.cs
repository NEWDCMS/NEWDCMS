using DCMS.Web.Framework.Models;
using System;

namespace DCMS.ViewModel.Models.Tss
{
    public partial class FeedbackModel: BaseEntityModel
    {
        public int FeedbackTyoe { get; set; }
        public string IssueDescribe { get; set; }
        public string Contacts { get; set; }
        public string Screenshot1 { get; set; }
        public string Screenshot2 { get; set; }
        public string Screenshot3 { get; set; }
        public string Screenshot4 { get; set; }
        public string Screenshot5 { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
    }

    /// <summary>
    /// 市场反馈
    /// </summary>
    public class MarketFeedbackModel : BaseEntityModel
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
