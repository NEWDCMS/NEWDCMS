using DCMS.Web.Framework;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Configuration
{
    public class PCPrintSettingModel
    {
        //电脑打印信息设置

        [HintDisplayName("商铺名称", "商铺名称")]
        public string StoreName { get; set; }

        [HintDisplayName("公司地址", "公司地址")]
        public string Address { get; set; }

        [HintDisplayName("订货电话", "订货电话")]
        public string PlaceOrderTelphone { get; set; }

        [HintDisplayName("打印方式", "打印方式")]
        public int PrintMethod { get; set; } = 0;
        [XmlIgnore]
        public SelectList PrintMethods { get; set; }


        //电脑打印纸张设置

        [HintDisplayName("打印纸张类型", "打印纸张类型")]
        public int PaperType { get; set; } = 0;
        [XmlIgnore]
        public SelectList PaperTypes { get; set; }


        [HintDisplayName("纸张宽", "纸张宽")]
        public double PaperWidth { get; set; } = 0;

        [HintDisplayName("纸张高", "纸张高")]
        public double PaperHeight { get; set; } = 0;


        [HintDisplayName("打印边距类型", "打印边距类型")]
        public int BorderType { get; set; } = 0;
        [XmlIgnore]
        public SelectList BorderTypes { get; set; }


        [HintDisplayName("上边距", "上边距")]
        public int MarginTop { get; set; } = 0;

        [HintDisplayName("下边距", "下边距")]
        public int MarginBottom { get; set; } = 0;

        [HintDisplayName("左边距", "左边距")]
        public int MarginLeft { get; set; } = 0;

        [HintDisplayName("右边距", "右边距")]
        public int MarginRight { get; set; } = 0;

        [HintDisplayName("是否打印页码", "是否打印页码")]
        public bool IsPrintPageNumber { get; set; }

        [HintDisplayName("每页打印表头", "每页打印表头")]
        public bool PrintHeader { get; set; }

        [HintDisplayName("每页打印表尾", "每页打印表尾")]
        public bool PrintFooter { get; set; }


        [HintDisplayName("每页固定行数", "每页固定行数")]
        public bool IsFixedRowNumber { get; set; }
        public int FixedRowNumber { get; set; } = 0;

        [HintDisplayName("每页打印小计", "每页打印小计")]
        public bool PrintSubtotal { get; set; }


        [HintDisplayName("云打印端口", "云打印端口")]
        public int PrintPort { get; set; } = 0;

        #region 每页打印头尾配置 2021-09-10 mu 添加

        [HintDisplayName("每页打印表头尾", "每页打印表头尾")]
        public bool PrintInAllPages { get; set; }

        [HintDisplayName("每页最多打印行数", "每页最多打印行数")]
        public int PageRowsCount { get; set; }

        [HintDisplayName("页头高度", "页头高度")]
        public decimal HeaderHeight { get; set; } = 15;

        [HintDisplayName("页脚高度", "页脚高度")]
        public decimal FooterHeight { get; set; } = 15;
        #endregion
    }
}