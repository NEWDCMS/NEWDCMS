using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.News
{
    public partial class NewsListModel : BaseModel
    {
        public NewsListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public int StoreId { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<NewsModel> NewsItems { get; set; }

        #region 查询条件
        /// <summary>
        /// 标题
        /// </summary>
        [HintDisplayName("标题", "标题")]
        public string Title { get; set; }

        [HintDisplayName("类别", "类别")]
        public int NewsCategoryId { get; set; }
        [HintDisplayName("类别名称", "类别名称")]
        public string NewsCategoryName { get; set; }
        public SelectList NewsCategories { get; set; }
        #endregion

    }


    /// <summary>
    /// 消息模型
    /// </summary>
    //[Validator(typeof(NewsValidator))]
    public class NewsModel : BaseEntityModel
    {
        [HintDisplayName("类别", "类别")]
        public int NewsCategoryId { get; set; }
        [HintDisplayName("类别名称", "类别名称")]
        public string NewsCategoryName { get; set; }
        public SelectList NewsCategories { get; set; }

        [HintDisplayName("标题", "标题")]
        public string Title { get; set; }

        [HintDisplayName("简称", "简称")]
        public string Short { get; set; }

        [HintDisplayName("全称", "全称")]
        public string Full { get; set; }

        [HintDisplayName("内容", "内容")]

        public string Content { get; set; }

        [HintDisplayName("图片上传", "图片上传")]

        public string PicturePath { get; set; }
        public string PictureId { get; set; }

        [HintDisplayName("开始时间", "开始时间")]
        public DateTime? StartDateUtc { get; set; }

        [HintDisplayName("结束时间", "结束时间")]
        public DateTime? EndDateUtc { get; set; }

        [HintDisplayName("关键字", "关键字")]
        public string MetaKeywords { get; set; }

        [HintDisplayName("描述", "描述")]
        public string MetaDescription { get; set; }

        [HintDisplayName("子标题", "子标题")]
        public string MetaTitle { get; set; }

        [HintDisplayName("是否显示", "是否显示")]
        public bool Published { get; set; }

        [HintDisplayName("是否显示经销商", "是否限制经销商")]
        public bool LimitedToStores { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
    }

    public class SendResultModel : BaseEntityModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}