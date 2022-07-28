using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DCMS.Web.Framework.TagHelpers.Admin
{
    /// <summary>
    /// dcms-panel tag helper
    /// </summary>
    [HtmlTargetElement("dcms-panels", Attributes = ID_ATTRIBUTE_NAME)]
    public class DCMSPanelsTagHelper : TagHelper
    {
        private const string ID_ATTRIBUTE_NAME = "id";

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
    }
}