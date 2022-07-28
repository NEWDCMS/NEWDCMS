using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.News
{
    public partial class NewsCategory : BaseEntity, IAclSupported, IStoreMappingSupported
    {
        private ICollection<NewsItem> _newsItems;

        public NewsCategory()
        {
            _newsItems = new List<NewsItem>();
        }


        public int? NewsItemId { get; set; } = 0;
        public int? NewsCategoryId { get; set; } = 0;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public int? ParentId { get; set; } = 0;

        /// <summary>
        /// 是否显示
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 是否显示在首页
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ShowOnHomePage { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// 是否限制经销商
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 父级类别
        /// </summary>
        public virtual NewsCategory NewsCategories { get; set; }

        /// <summary>
        /// 子类别集合
        /// </summary>
        public virtual ICollection<NewsCategory> ChildCategories { get; set; }


        public virtual ICollection<NewsItem> NewsItems
        {
            get { return _newsItems ?? (_newsItems = new List<NewsItem>()); }
            protected set { _newsItems = value; }
        }

    }

}
