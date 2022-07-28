using DCMS.Core.Domain.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Products
{


    public partial class ProductTierPricePlanListModel : BaseModel
    {
        public ProductTierPricePlanListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        [HintDisplayName("关键字", "搜索关键字")]
        public string Key { get; set; }

        public string Name { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductTierPricePlanModel> Items { get; set; }
    }

    //[Validator(typeof(ProductTierPricePlanValidator))]
    public partial class ProductTierPricePlanModel : BaseEntityModel
    {

        public string StoreName { get; set; }

        [HintDisplayName("方案名", "方案名")]
        public string Name { get; set; }
    }


    public partial class ProductTierPriceModel : BaseEntityModel
    {



        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;


        [HintDisplayName("方案", "自定义方案")]
        public int PricesPlanId { get; set; }


        [HintDisplayName("表示价格类型", "表示价格类型")]
        public int PriceTypeId { get; set; } = 0;
        //public string PriceType { get; set; }
        public string PriceTypeName { get; set; }
        public PriceType PriceType
        {
            get { return (PriceType)PriceTypeId; }
            set { PriceTypeId = (byte)value; }
        }

        [HintDisplayName("小单位价格", "小单位价格")]
        public decimal? SmallUnitPrice { get; set; } = 0;
        public int SmallUnitId { get; set; } = 0;

        [HintDisplayName("中单位价格", "中单位价格")]
        public decimal? StrokeUnitPrice { get; set; } = 0;
        public int StrokeUnitId { get; set; } = 0;

        [HintDisplayName("大单位价格", "大单位价格")]
        public decimal? BigUnitPrice { get; set; } = 0;
        public int BigUnitId { get; set; } = 0;

    }
}