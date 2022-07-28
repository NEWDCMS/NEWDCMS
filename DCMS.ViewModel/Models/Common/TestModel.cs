using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Tests
{
    public partial class DemoModel
    {
        public int PriceTypeId { get; set; } = 0;
        public PriceType PriceType { get; set; }

        public int StatusId { get; set; } = 0;
        public Status Status { get; set; }


        public IEnumerable<SelectListItem> PriceTypes { get; set; }



        public int[] SelectedIds { get; set; }
        public IEnumerable<SelectListItem> Selecteds { get; set; }
    }


    public enum PriceType : int
    {
        [Description("进价(成本价)")]
        ProductCost = 0,

        [Description("批发价格")]
        WholesalePrice = 1,

        [Description("零售价格")]
        RetailPrice = 2,

        [Description("最低售价")]
        LowestPrice = 3,

        [Description("自定义方案")]
        CustomPlan = 88
    }


    public enum Status : int
    {
        [Description("是")]
        Yes = 0,

        [Description("否")]
        No = 1,
    }
}