using DCMS.Core.Domain.Common;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Plan
{

    public partial class PercentagePlanListModel : BaseModel
    {
        public PercentagePlanListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public string Name { get; set; }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PercentagePlanModel> Items { get; set; }
    }


    //[Validator(typeof(PercentagePlanValidator))]
    public partial class PercentagePlanModel : BaseEntityModel
    {



        [HintDisplayName("方案名称", "方案名称")]
        public string Name { get; set; }

        [HintDisplayName("方案类型", "方案类型")]
        public int PlanTypeId { get; set; } = 0;
        public PercentagePlanType PlanType { get; set; }
        public string PlanTypeName { get; set; }


        [HintDisplayName("是否按回款提成", "是否按回款提成")]
        public bool IsByReturn { get; set; }


        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
    }


    /// <summary>
    /// 表示员工提成配置
    /// </summary>
    public class PercentageModel : BaseEntityModel
    {

        public PercentageModel()
        {
            Rangs = new List<PercentageRangeOptionModel>();
        }



        [HintDisplayName("方案", "方案")]
        public int PercentagePlanId { get; set; } = 0;


        [HintDisplayName("商品类别", "商品类别")]
        public int? CatagoryId { get; set; } = 0;
        public string CatagoryName { get; set; }

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("品项名称", "品项名称")]
        public string Name { get; set; }

        [HintDisplayName("员工提成计算方式", "员工提成计算方式")]
        public int CalCulateMethodId { get; set; } = 0;
        public SelectList CalCulateMethods { get; set; }
        public PercentageCalCulateMethod CalCulateMethod { get; set; }


        [HintDisplayName("数量核算方式", "数量核算方式")]
        public int QuantityCalCulateMethodId { get; set; } = 0;
        public SelectList QuantityCalCulateMethods { get; set; }
        public QuantityCalCulateMethod QuantityCalCulateMethod { get; set; }


        [HintDisplayName("成本计算方式", "成本计算方式")]
        public int CostingCalCulateMethodId { get; set; } = 0;
        public SelectList CostingCalCulateMethods { get; set; }
        public CostingCalCulateMethod CostingCalCulateMethod { get; set; }


        [HintDisplayName("是否退货参与提成", "是否退货参与提成")]
        public bool IsReturnCalCulated { get; set; }

        [HintDisplayName("是否赠品参与提成", "是否赠品参与提成")]
        public bool IsGiftCalCulated { get; set; }


        [HintDisplayName("销售提成比例", "销售提成比例")]
        public decimal? SalesPercent { get; set; } = 0;

        [HintDisplayName("退货提成比例", "退货提成比例")]
        public decimal? ReturnPercent { get; set; } = 0;



        [HintDisplayName("销售提成金额", "销售提成金额")]
        public decimal? SalesAmount { get; set; } = 0;

        [HintDisplayName("退货提成金额", "退货提成金额")]
        public decimal? ReturnAmount { get; set; } = 0;



        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }
        public List<PercentageRangeOptionModel> Rangs { get; set; }
    }


    /// <summary>
    /// 按利润区间范围计算提成
    /// </summary>
    public class PercentageRangeOptionModel : BaseEntityModel
    {

        [HintDisplayName("净销售额区间范围", "净销售额区间范围")]
        public decimal? NetSalesRange { get; set; } = 0;


        [HintDisplayName("销售提成(%)", " 销售提成(%)")]
        public decimal? SalesPercent { get; set; } = 0;

        [HintDisplayName("退货提成(%)", "退货提成(%)")]
        public decimal? ReturnPercent { get; set; } = 0;

    }


    /// <summary>
    /// 保存序列
    /// </summary>
    public class PercentageSerializeModel : BaseEntityModel
    {
        public PercentageModel Percentage { get; set; }
    }
}