using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Configuration
{
    public class APPPrintSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [HintDisplayName("打印小包价格", "打印小包价格")]
        public bool AllowPrintPackPrice { get; set; }
        [HintDisplayName("打印模式", "打印模式")]
        public int PrintMode { get; set; } = 0;
        [XmlIgnore]
        public SelectList PrintModes { get; set; }
        [HintDisplayName("单据打印份数", "单据打印份数")]
        public int PrintingNumber { get; set; } = 0;

        [HintDisplayName("收款对账页面，允许自动打印销售和退货详情", "收款对账页面，允许自动打印销售和退货详情")]
        public bool AllowAutoPrintSalesAndReturn { get; set; }
        [HintDisplayName("收款对账页面，允许自动打印订货款详情", "收款对账页面，允许自动打印订货款详情")]
        public bool AllowAutoPrintOrderAndReturn { get; set; }
        [HintDisplayName("收款对账页面，允许自动打印预收款详情", "收款对账页面，允许自动打印预收款详情")]
        public bool AllowAutoPrintAdvanceReceipt { get; set; }
        [HintDisplayName("收款对账页面，允许自动打印欠款详情", "收款对账页面，允许自动打印欠款详情")]
        public bool AllowAutoPrintArrears { get; set; }

        [HintDisplayName("允许打印一票通", "允许打印一票通")]
        public bool AllowPrintOnePass { get; set; }
        [HintDisplayName("允许打印商品汇总", "允许打印商品汇总")]
        public bool AllowPrintProductSummary { get; set; }
        [HintDisplayName("是否打印手机号", "是否打印手机号")]
        public bool AllowPringMobile { get; set; }
        [HintDisplayName("打印时间和打印次数", "打印时间和打印次数")]
        public bool AllowPrintingTimeAndNumber { get; set; }
        [HintDisplayName("打印客户余额", "打印客户余额(往来余额=预收款余额-累计欠款)")]
        public bool AllowPrintCustomerBalance { get; set; }

        [HintDisplayName("页头文字", "页头文字")]
        public string PageHeaderText { get; set; }
        [HintDisplayName("页尾文字1", "页尾文字1")]
        public string PageFooterText1 { get; set; }
        [HintDisplayName("页尾文字2", "页尾文字2")]
        public string PageFooterText2 { get; set; }

        [HintDisplayName("页头图片", "页头图片")]
        public string PageHeaderImage { get; set; }
        [HintDisplayName("页尾图片", "页尾图片")]
        public string PageFooterImage { get; set; }

    }
}