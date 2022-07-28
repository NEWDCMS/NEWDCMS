using DCMS.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Net;
namespace DCMS.Web.Framework.TagHelpers.Admin
{
    /// <summary>
    /// dcms-label tag helper
    /// </summary>
    [HtmlTargetElement("dcms-label", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSLabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisplayHintAttributeName = "asp-display-hint";

        private readonly IWorkContext _workContext;

        /// <summary>
        /// HtmlGenerator
        /// </summary>
        protected IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Indicates whether the hint should be displayed
        /// </summary>
        [HtmlAttributeName(DisplayHintAttributeName)]
        public bool DisplayHint { get; set; } = true;

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="generator">HTML generator</param>
        /// <param name="workContext">Work context</param>
        /// <param name="localizationService">Localization service</param>
        public DCMSLabelTagHelper(IHtmlGenerator generator, IWorkContext workContext)
        {
            Generator = generator;
            _workContext = workContext;
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

            //generate label
            //var tagBuilder = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, new { @class = "control-label" });
            //if (tagBuilder != null)
            //{
            //    //create a label wrapper
            //    output.TagName = "div";
            //    output.TagMode = TagMode.StartTagAndEndTag;
            //    //merge classes
            //    var classValue = output.Attributes.ContainsName("class")
            //                        ? $"{output.Attributes["class"].Value} label-wrapper"
            //                        : "label-wrapper";
            //    output.Attributes.SetAttribute("class", classValue);

            //    //add label
            //    output.Content.SetHtmlContent(tagBuilder);

            //    //  [HintDisplayName("进货平均价计算历史次数", "进货平均价计算历史次数")]
            //    //add hint
            //    if (For.Metadata.AdditionalValues.TryGetValue("HintDisplayNameAttribute", out object value))
            //    {
            //        var hintContent = $"<div title='{WebUtility.HtmlEncode(value.ToString())}' data-toggle='tooltip' class='ico-help'><i class='fa fa-question-circle'></i></div>";
            //        output.Content.AppendHtml(hintContent);
            //    }
            //}

            var tagBuilder = new TagBuilder("label")
            {
                Attributes =
                {
                    new KeyValuePair<string, string>("id", For.Name),
                    new KeyValuePair<string, string>("data-name", For.Name),
                }
            };


            if (tagBuilder != null)
            {
                //create a label wrapper
                output.TagName = "div";
                output.TagMode = TagMode.StartTagAndEndTag;
                //merge classes
                var classValue = output.Attributes.ContainsName("class")
                                    ? $"{output.Attributes["class"].Value} label-wrapper"
                                    : "label-wrapper";
                output.Attributes.SetAttribute("class", classValue);

                var hits = For.Metadata.DisplayName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string name = "";
                string hit = "";

                if (hits.Length > 0)
                {
                    name = hits[0];
                }

                if (hits.Length > 1)
                {
                    hit = hits[1];
                }

                tagBuilder.Attributes.Add("title", WebUtility.HtmlEncode(hit));
                tagBuilder.Attributes.Add("data-toggle", "tooltip");
                tagBuilder.Attributes.Add("class", "ico-help");

                tagBuilder.InnerHtml.AppendHtml(name);

                //add label
                output.Content.SetHtmlContent(tagBuilder);
                //
                //if (For.Metadata.AdditionalValues.TryGetValue("HintDisplayName", out object value))
                //{
                //    var hintContent = $"<i class=\"fa fa-exclamation-circle text-warning fa-lg pr10\"></i>";
                //    output.Content.AppendHtml(hintContent);
                //}

                var hintContent = $"<i class=\"fa fa-exclamation-circle text-warning fa-lg pr10 ml5\"></i>";
                output.Content.AppendHtml(hintContent);
            }

        }
    }
}