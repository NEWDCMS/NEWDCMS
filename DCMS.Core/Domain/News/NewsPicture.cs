using DCMS.Core.Domain.Media;

namespace DCMS.Core.Domain.News
{
    /// <summary>
    /// 表示新闻图片映射
    /// </summary>
    public class NewsPicture : BaseEntity
    {

        /// <summary>
        /// 新闻标识
        /// </summary>
        public virtual int NewsItemId { get; set; }

        /// <summary>
        /// 图片标识
        /// </summary>
        public virtual int PictureId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// 获取图片
        /// </summary>
        public virtual Picture Picture { get; set; }

        /// <summary>
        /// 获取新闻
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }
    }

}
