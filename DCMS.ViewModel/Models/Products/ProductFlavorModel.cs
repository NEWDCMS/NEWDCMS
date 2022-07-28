using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{

    public partial class ProductFlavorListModel : BaseModel
    {
        public ProductFlavorListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string TargetForm { get; set; }
        public List<string> TargetDoms { get; set; }
        public int RowIndex { get; set; }
        public string Target { get; set; }
        public int ParentId { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductFlavorModel> Items { get; set; }
    }


    //[Validator(typeof(ProductFlavorValidator))]
    public partial class ProductFlavorModel : BaseEntityModel
    {
        [HintDisplayName("父商品", "父商品")]
        public int ParentId { get; set; } = 0;

        [HintDisplayName("关联商品(可选)", "关联商品(可选)")]
        public int ProductId { get; set; } = 0;

        [HintDisplayName("商品名称", "商品名称")]
        public string ProductName { get; set; }

        [HintDisplayName("口味", "口味")]
        public string Name { get; set; }

        [HintDisplayName("小单位条码", "小单位条码")]
        public string SmallUnitBarCode { get; set; }

        [HintDisplayName("中单位条码", "中单位条码")]
        public string StrokeUnitBarCode { get; set; }

        [HintDisplayName("大单位条码", "大单位条码")]
        public string BigUnitBarCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; }

        /// <summary>
        /// 添加方式 0：输入，1：选择
        /// 输入：productId = 当前表Id
        /// 选择：productId = 用户选择商品Id
        /// </summary>
        public int AddType { get; set; }

    }
}