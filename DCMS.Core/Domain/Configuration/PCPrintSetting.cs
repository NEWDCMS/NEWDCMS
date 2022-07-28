using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Configuration
{
    //PC端打印
    public class PCPrintSetting : ISettings
    {

        /// <summary>
        /// 商铺名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 订货电话
        /// </summary>
        public string PlaceOrderTelphone { get; set; }

        /// <summary>
        /// 打印方式
        /// </summary>
        public int PrintMethod { get; set; }

        /// <summary>
        /// 打印纸张类型
        /// </summary>
        public int PaperType { get; set; }

        /// <summary>
        /// 纸张宽
        /// </summary>
        public double PaperWidth { get; set; }

        /// <summary>
        /// 纸张高
        /// </summary>
        public double PaperHeight { get; set; }

        /// <summary>
        /// 打印边距类型
        /// </summary>
        public int BorderType { get; set; }

        /// <summary>
        /// 上边距
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// 下边距
        /// </summary>
        public int MarginBottom { get; set; }

        /// <summary>
        /// 左边距
        /// </summary>
        public int MarginLeft { get; set; }

        /// <summary>
        /// 右边距
        /// </summary>
        public int MarginRight { get; set; }

        /// <summary>
        /// 是否打印页码
        /// </summary>
        public bool IsPrintPageNumber { get; set; }

        /// <summary>
        /// 每页打印表头
        /// </summary>
        public bool PrintHeader { get; set; }

        /// <summary>
        /// 每页打印表尾
        /// </summary>
        public bool PrintFooter { get; set; }

        /// <summary>
        /// 每页固定行数
        /// </summary>
        public bool IsFixedRowNumber { get; set; }

        /// <summary>
        /// 每页固定行数
        /// </summary>
        public int FixedRowNumber { get; set; }

        /// <summary>
        /// 每页打印小计
        /// </summary>
        public bool PrintSubtotal { get; set; }

        /// <summary>
        /// 云打印端口   
        /// </summary>
        public int PrintPort { get; set; }

        #region 2021-09-09 mu 添加
        /// <summary>
        /// 是否每页打印页眉页脚
        /// </summary>
        public bool PrintInAllPages { get; set; }
        /// <summary>
        /// 每页最多打印行数
        /// </summary>
        public int PageRowsCount { get; set; }
        /// <summary>
        /// 页头高度
        /// </summary>
        public decimal HeaderHeight { get; set; }
        /// <summary>
        /// 页脚高度
        /// </summary>
        public decimal FooterHeight { get; set; }
        #endregion
    }
}
