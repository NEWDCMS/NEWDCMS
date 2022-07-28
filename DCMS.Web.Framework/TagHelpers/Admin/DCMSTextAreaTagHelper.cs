using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DCMS.Web.Framework.TagHelpers.Admin
{
    /// <summary>
    /// dcms-textarea tag helper
    /// </summary>
    [HtmlTargetElement("dcms-textarea", Attributes = ForAttributeName)]
    public class DCMSTextAreaTagHelper : TextAreaTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string RequiredAttributeName = "asp-required";
        private const string DisabledAttributeName = "asp-disabled";

        /// <summary>
        /// Indicates whether the input is disabled
        /// </summary>
        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        /// <summary>
        /// Indicates whether the field is required
        /// </summary>
        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="generator">HTML generator</param>
        public DCMSTextAreaTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //tag details
            output.TagName = "textarea";
            output.TagMode = TagMode.StartTagAndEndTag;

            //merge classes
            var classValue = output.Attributes.ContainsName("class")
                ? $"{output.Attributes["class"].Value} form-control"
                : "form-control";
            output.Attributes.SetAttribute("class", classValue);

            //add disabled attribute
            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            //additional parameters
            var rowsNumber = output.Attributes.ContainsName("rows") ? output.Attributes["rows"].Value : 4;
            output.Attributes.SetAttribute("rows", rowsNumber);
            var colsNumber = output.Attributes.ContainsName("cols") ? output.Attributes["cols"].Value : 20;
            output.Attributes.SetAttribute("cols", colsNumber);

            //required asterisk
            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            base.Process(context, output);
        }
    }


    /// <summary>
    /// dcms-input tag helper
    /// </summary>
    [HtmlTargetElement("dcms-input", Attributes = ForAttributeName)]
    public class DCMSInputTagHelper : InputTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string ReadonlyAttributeName = "asp-readonly";
        private const string DisabledAttributeName = "asp-disabled";

        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }


        [HtmlAttributeName(ReadonlyAttributeName)]
        public string IsReadonly { set; get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="generator">HTML generator</param>
        public DCMSInputTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //tag details
            output.TagName = "input";
            output.TagMode = TagMode.StartTagAndEndTag;

            //merge classes
            var classValue = output.Attributes.ContainsName("class")
                ? $"{output.Attributes["class"].Value} form-control"
                : "form-control";
            output.Attributes.SetAttribute("class", classValue);


            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            bool.TryParse(IsReadonly, out bool onlyread);
            if (onlyread)
            {
                var d = new TagHelperAttribute("Readonly", "readonly");
                output.Attributes.Add(d);
            }

            base.Process(context, output);
        }
    }

}