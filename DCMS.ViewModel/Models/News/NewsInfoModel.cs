using DCMS.Web.Framework.Models;
using System;

namespace DCMS.ViewModel.Models.News
{
    public partial class NewsInfoModel : BaseModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 新闻图片
        /// </summary>
        public string PicturePath { get; set; }
        /// <summary>
        /// 新闻标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 新闻内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 导航链接
        /// </summary>
        public string Navigation { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string Short { get; set; }

        /// <summary>
        /// 全称
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public int NewsCategoryId { get; set; } = 0;
        public string NewsCategoryName { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string MetaTitle { get; set; }
    }
}
