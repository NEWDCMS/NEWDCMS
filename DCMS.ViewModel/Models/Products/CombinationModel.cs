using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Products
{

    public partial class CombinationListModel : BaseModel
    {
        public CombinationListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CombinationModel> Items { get; set; }
    }



    public partial class ProductCombinationListModel : BaseModel
    {

    }


    public partial class CombinationModel : BaseEntityModel
    {
        public CombinationModel()
        {
            SubProducts = new List<ProductCombinationModel>();
        }



        [HintDisplayName("是否启用", "是否启用")]
        public bool Enabled { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; } = 0;


        [HintDisplayName("商品名称", "商品名称")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("主商品成本", "主商品成本")]
        public decimal ProductCost { get; set; }

        public IList<ProductCombinationModel> SubProducts { get; set; }

        public string JSONData { get; set; }

    }


    //[Validator(typeof(ProductCombinationValidator))]
    public class ProductCombinationModel : BaseEntityModel
    {



        public int CombinationId { get; set; } = 0;


        [HintDisplayName("商品名称", "商品名称")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("数量", "数量")]
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// 单位成本
        /// </summary>
        public decimal CostPrice { get; set; } = 0;

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal CostAmount { get; set; } = 0;

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; } = 0;

        /// <summary>
        /// 库存数量显示
        /// </summary>
        public string StockQuantityConversion { get; set; }


        [HintDisplayName("单位", "单位(规格属性项)")]
        public int UnitId { get; set; } = 0;
        public SelectList Units { get; set; }
        public string UnitName { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; } = 0;

    }
}