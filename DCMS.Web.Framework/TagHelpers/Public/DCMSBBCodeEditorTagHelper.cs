using DCMS.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace DCMS.Web.Framework.TagHelpers.Public
{
    /// <summary>
    /// dcms-bb-code-editor tag helper
    /// </summary>
    [HtmlTargetElement("dcms-bb-code-editor", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSBBCodeEditorTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        private readonly IWebHelper _webHelper;

        /// <summary>
        /// An expression to be evaluated against the current model
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="webHelper">Web helper</param>
        public DCMSBBCodeEditorTagHelper(IWebHelper webHelper)
        {
            _webHelper = webHelper;
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

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", "bb-code-editor-wrapper");

            var storeLocation = _webHelper.GetStoreLocation();
            var bbEditorWebRoot = $"{storeLocation}js/";

            var script1 = new TagBuilder("script");
            script1.Attributes.Add("src", $"{storeLocation}js/bbeditor/ed.js");

            var script2 = new TagBuilder("script");
            script2.Attributes.Add("language", "javascript");
            script2.InnerHtml.AppendHtml($"edToolbar('{For.Name}','{bbEditorWebRoot}');");

            output.Content.AppendHtml(script1);
            output.Content.AppendHtml(script2);
        }
    }
}