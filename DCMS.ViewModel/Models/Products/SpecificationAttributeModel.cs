using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Products
{

    public partial class SpecificationAttributeListModel : BaseModel
    {
        public SpecificationAttributeListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SpecificationAttributeModel> SpecificationAttributes { get; set; }

        public SelectList Stores { get; set; }

        [DisplayName("关键字")]

        public string SearchKey { get; set; }
    }


    // [Validator(typeof(SpecificationAttributeValidator))]
    public partial class SpecificationAttributeModel : BaseEntityModel
    {
        /// <summary>
        /// 经销商
        /// </summary>
        [HintDisplayName("经销商", "经销商")]
        //移除 = 0;
        public SelectList StoreList { get; set; }

        [HintDisplayName("", "")]
        public string StoreName { get; set; }


        [HintDisplayName("规格属性名", "规格属性名")]

        public string Name { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; } = 0;
    }

}