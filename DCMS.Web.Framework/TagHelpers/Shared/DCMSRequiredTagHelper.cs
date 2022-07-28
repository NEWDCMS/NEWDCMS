using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace DCMS.Web.Framework.TagHelpers.Shared
{
    /// <summary>
    /// dcms-required tag helper
    /// </summary>
    [HtmlTargetElement("dcms-required", TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSRequiredTagHelper : TagHelper
    {
        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            //clear the output
            output.SuppressOutput();

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "required");
            output.Content.SetContent("*");
        }
    }
}