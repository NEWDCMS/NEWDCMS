using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace DCMS.Web.Framework.TagHelpers.Shared
{
    /// <summary>
    /// dcms-antiforgery-token tag helper
    /// </summary>
    [HtmlTargetElement("dcms-antiforgery-token", TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSAntiForgeryTokenTagHelper : TagHelper
    {
        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// HtmlGenerator
        /// </summary>
        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="generator">HTML generator</param>
        public DCMSAntiForgeryTokenTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

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

            //generate antiforgery
            var antiforgeryTag = Generator.GenerateAntiforgery(ViewContext);
            if (antiforgeryTag != null)
            {
                output.Content.SetHtmlContent(antiforgeryTag);
            }
        }
    }
}