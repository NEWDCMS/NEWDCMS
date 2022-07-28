using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Configuration
{
    /// <summary>
    /// 用于表示APP打印设置
    /// </summary>
    public class APPPrintSetting : ISettings
    {
        /// <summary>
        /// 打印小包价格
        /// </summary>
        public bool AllowPrintPackPrice { get; set; }
        /// <summary>
        /// 打印模式(枚举 1紧缩模式 ，2宽松模式)
        /// </summary>
        public int PrintMode { get; set; }
        /// <summary>
        /// 单据打印份数
        /// </summary>
        public int PrintingNumber { get; set; }

        /// <summary>
        /// 收款对账页面，允许自动打印销售和退货详情
        /// </summary>
        public bool AllowAutoPrintSalesAndReturn { get; set; }
        /// <summary>
        /// 收款对账页面，允许自动打印订货款详情
        /// </summary>
        public bool AllowAutoPrintOrderAndReturn { get; set; }
        /// <summary>
        /// 收款对账页面，允许自动打印预收款详情
        /// </summary>
        public bool AllowAutoPrintAdvanceReceipt { get; set; }
        /// <summary>
        /// 收款对账页面，允许自动打印欠款详情
        /// </summary>
        public bool AllowAutoPrintArrears { get; set; }

        /// <summary>
        /// 允许打印一票通
        /// </summary>
        public bool AllowPrintOnePass { get; set; }
        /// <summary>
        /// 允许打印商品汇总
        /// </summary>
        public bool AllowPrintProductSummary { get; set; }
        /// <summary>
        /// 是否打印手机号
        /// </summary>
        public bool AllowPringMobile { get; set; }
        /// <summary>
        /// 打印时间和打印次数
        /// </summary>
        public bool AllowPrintingTimeAndNumber { get; set; }
        /// <summary>
        /// 打印客户余额
        /// </summary>
        public bool AllowPrintCustomerBalance { get; set; }

        /// <summary>
        /// 页头文字
        /// </summary>
        public string PageHeaderText { get; set; }
        /// <summary>
        /// 页尾文字1
        /// </summary>
        public string PageFooterText1 { get; set; }
        /// <summary>
        /// 页尾文字2
        /// </summary>
        public string PageFooterText2 { get; set; }

        /// <summary>
        /// 页头图片
        /// </summary>
        public string PageHeaderImage { get; set; }
        /// <summary>
        /// 页尾图片
        /// </summary>
        public string PageFooterImage { get; set; }
    }
}
