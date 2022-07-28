using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DCMS.ViewModel.Models.Products
{


    public partial class CategoryModel : BaseEntityModel, IParentList
    {



        [HintDisplayName("分类名称", "分类名称")]
        public string Name { get; set; }

        /// <summary>
        /// 所属父级
        /// </summary>
        [HintDisplayName("所属父级", "所属父级")]
        public int ParentId { get; set; } = 0;
        [HintDisplayName("所属父级", "所属父级")]
        public string ParentName { get; set; }
        public SelectList ParentList { get; set; }


        [HintDisplayName("继承路径", "继承路径")]
        public string PathCode { get; set; }

        [HintDisplayName("统计类别", "统计类别")]
        public int StatisticalType { get; set; } = 0;
        public SelectList StatisticalTypes { get; set; }



        [HintDisplayName("状态", "状态")]
        public int Status { get; set; }


        [HintDisplayName("排序", "排序")]
        public int OrderNo { get; set; } = 0;

        [HintDisplayName("品牌", "品牌")]
        public int BrandId { get; set; } = 0;
        [HintDisplayName("品牌", "品牌")]
        public string BrandName { get; set; }


        [HintDisplayName("状态", "停用商品类别后，该类别和该类别下的商品将无法被选择")]
        public bool Published { get; set; }

        [HintDisplayName("提成方案", "提成方案")]
        public int? PercentageId { get; set; } = 0;

    }



}