namespace DCMS.Core.Domain.Configuration
{
    /// <summary>
    /// 表示打印模板
    /// </summary>
    public partial class PrintTemplate : BaseEntity
    {
        /// <summary>
        /// 模板类型
        /// </summary>
        public int TemplateType { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }


        public int PaperType { get; set; }

        public PaperTypeEnum EPaperTypes
        {
            get => (PaperTypeEnum)PaperType;
            set => PaperType = (int)value;
        }


        /// <summary>
        /// 纸张宽度
        /// </summary>
        public double PaperWidth { get; set; }

        /// <summary>
        /// 纸张高度
        /// </summary>
        public double PaperHeight { get; set; }

    }
}
