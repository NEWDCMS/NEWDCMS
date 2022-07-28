using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Xml.Serialization;
using DCMS.Core;


namespace DCMS.ViewModel.Models.Configuration
{


    public class PrintTemplateListModel : BaseEntityModel
    {

        public PrintTemplateListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<PrintTemplateModel>();
        }
        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PrintTemplateModel> Lists { get; set; }
    }


    // [Validator(typeof(PrintTemplateValidator))]
    public class PrintTemplateModel : BaseEntityModel
    {

        [HintDisplayName("模板类型", "模板类型")]
        public int? TemplateType { get; set; } = 0;
        public string TemplateTypeName { get; set; }
        [XmlIgnore]
        public SelectList TemplateTypes { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int? BillType { get; set; } = 0;
        public string BillTypeName { get; set; }
        [XmlIgnore]
        public SelectList BillTypes { get; set; }

        [HintDisplayName("标题", "标题")]
        public string Title { get; set; }

        [HintDisplayName("内容", "内容")]
        public string Content { get; set; }

        [HintDisplayName("纸张类型", "纸张类型")]
        public int PaperType { get; set; }
        public SelectList PaperTypes { get; set; }
        public PaperTypeEnum EPaperTypes
        {
            get => (PaperTypeEnum)PaperType;
            set => PaperType = (int)value;
        }

        [HintDisplayName("纸张宽度", "纸张宽度")]
        public double PaperWidth { get; set; }

        [HintDisplayName("纸张高度", "纸张高度")]
        public double PaperHeight { get; set; }

    }
}
