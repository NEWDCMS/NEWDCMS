using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.News
{
    public partial class NewsCategoryListModel : BaseEntityModel
    {
        public NewsCategoryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }


        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<NewsCategoryModel> NewsCategoryItems { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        [HintDisplayName("类别名称", "类别名称")]
        public string Name { get; set; }

    }



    /// <summary>
    /// 消息类别模型
    /// </summary>
    //[Validator(typeof(NewsCategoryValidator))]
    public class NewsCategoryModel : BaseEntityModel
    {
        [HintDisplayName("类别名称", "类别名称")]

        [JsonProperty("name")]
        public string Name { get; set; }

        [HintDisplayName("序号", "序号")]

        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        [HintDisplayName("父级Id", "父级Id")]

        [JsonProperty("parentId")]
        public int? ParentId { get; set; }
        [HintDisplayName("父级名称", "父级名称")]

        [JsonProperty("parentName")]
        public string ParnetName { get; set; }
        public SelectList Parents { get; set; }

        [HintDisplayName("是否显示", "是否显示")]

        [JsonProperty("published")]
        public bool Published { get; set; }

        [HintDisplayName("是否删除", "是否删除")]

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [HintDisplayName("是否首页显示", "是否首页显示")]
        [JsonProperty("showOnHomePage")]
        public bool ShowOnHomePage { get; set; }
        [HintDisplayName("", "")]
        public bool SubjectToAcl { get; set; }

        [HintDisplayName("是否显示经销商", "是否限制经销商")]
        [JsonProperty("limitedToStores")]
        public bool LimitedToStores { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        [JsonProperty("createDate")]
        public DateTime CreateDate { get; set; }
    }
}