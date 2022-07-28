//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{

    public partial class ProductAttributeListModel : BaseModel
    {
        public ProductAttributeListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductAttributeModel> ProductAttributs { get; set; }

        public SelectList Stores { get; set; }

        [HintDisplayName("关键字", "关键字")]
        public string Name { get; set; }
    }


    //[Validator(typeof(ProductAttributeValidator))]
    public partial class ProductAttributeModel : BaseEntityModel
    {

        /// <summary>
        /// 经销商
        /// </summary>
        [HintDisplayName("经销商", "经销商")]
        //移除
        public SelectList StoreList { get; set; }

        [HintDisplayName("", "")]
        public string StoreName { get; set; }

        [HintDisplayName("属性名", "属性名")]

        public string Name { get; set; }

        [HintDisplayName("属性描述", "属性描述")]

        public string Description { get; set; }
    }

}