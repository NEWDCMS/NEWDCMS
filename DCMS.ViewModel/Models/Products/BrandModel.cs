using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{

    public partial class BrandListModel : BaseModel
    {
        public BrandListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Name { get; set; }

        public string Key { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<BrandModel> Items { get; set; }
    }


    //[Validator(typeof(BrandValidator))]
    public partial class BrandModel : BaseEntityModel
    {


        [HintDisplayName("品牌名称", "品牌名称")]
        public string Name { get; set; }


        [HintDisplayName("状态", "状态")]
        public bool Status { get; set; }


        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; } = 0;


        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
    }
}