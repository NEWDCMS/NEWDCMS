using DCMS.Core.Domain.Stores;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DCMS.Core.Domain.News
{

    public class NewsItem : BaseEntity, /*ISlugSupported,*/ IStoreMappingSupported
    {

        public int NewsCategoryId { get; set; }

        public string Title { get; set; }

        public string Short { get; set; }

        public string Full { get; set; }

        public string Content { get; set; }
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; }

        public DateTime? StartDateUtc { get; set; }

        public DateTime? EndDateUtc { get; set; }
        [Column(TypeName = "BIT(1)")]
        public bool AllowComments { get; set; }

        public int CommentCount { get; set; }
        [Column(TypeName = "BIT(1)")]
        public bool LimitedToStores { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string PictureId { get; set; }


        public DateTime CreatedOnUtc { get; set; }


        //public virtual ICollection<NewsComment> NewsComments
        //{
        //    get { return _newsComments ?? (_newsComments = new List<NewsComment>()); }
        //    protected set { _newsComments = value; }
        //}
        [JsonIgnore]
        public virtual NewsCategory NewsCategory { get; set; }

        //public virtual ICollection<NewsPicture> NewsPictures
        //{
        //    get { return _newsPictures ?? (_newsPictures = new List<NewsPicture>()); }
        //    protected set { _newsPictures = value; }
        //}

        //public virtual Language Language { get; set; }
    }
}