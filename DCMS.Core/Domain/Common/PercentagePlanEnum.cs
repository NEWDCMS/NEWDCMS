using System.ComponentModel;


namespace DCMS.Core.Domain.Common
{

    /// <summary>
    /// 提成方案类型
    /// </summary>
    public enum PercentagePlanType
    {
        [Description("全部")]
        AllPlan = 0,
        [Description("业务员提成方案")]
        BusinessExtractPlan = 1,
        [Description("送货员提成方案")]
        DeliveryExtractPlan = 2
    }


    /// <summary>
    /// 员工提成计算方式
    /// </summary>
    public enum PercentageCalCulateMethod
    {
        [Description("销售额百分比")]
        PercentageOfSales = 1,

        [Description("销售额变化百分比")]
        PercentageChangeInSales = 2,

        [Description("销售额分段变化百分比")]
        SalesSegmentChangePercentage = 3,

        [Description("销售数量每件固定额")]
        SalesVolumeFixedAmount = 4,

        [Description("按销售数量变化每件提成金额")]
        ChangesInSalesVolume = 5,

        [Description("按销售数量分段变化每件提成金额")]
        AccordingToSalesVolume = 6,

        [Description("利润额百分比")]
        ProfitPercentage = 7,

        [Description("利润额变化百分比")]
        ProfitChangePercentage = 8,

        [Description("利润额分段变化百分比")]
        PercentageChangeInProfit = 9
    }

    /// <summary>
    /// 数量核算方式
    /// </summary>
    public enum QuantityCalCulateMethod
    {

        [Description("使用基本单位核算数量")]
        UsingBasicUnitsCalculateQuantity = 0,

        [Description("使用大包单位核算数量")]
        UselargePackageUnitsCalculateQuantity = 1
    }


    /// <summary>
    /// 成本计算方式
    /// </summary>
    public enum CostingCalCulateMethod
    {

        [Description("当时加权平均价")]
        WeightedAveragePriceTime = 1,

        [Description("现在加权平均价")]
        WeightedAveragePriceNow = 2,

        [Description("现在预设进价")]
        PresetPurchasePriceNow = 3

    }
}
