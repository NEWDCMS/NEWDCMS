using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace DCMS.Web.Framework.TagHelpers.Public
{
    /// <summary>
    /// label tag helper
    /// </summary>
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class LabelTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.LabelTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string PostfixAttributeName = "asp-postfix";

        /// <summary>
        /// Indicates whether the input is disabled
        /// </summary>
        [HtmlAttributeName(PostfixAttributeName)]
        public string Postfix { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="generator">HTML generator</param>
        public LabelTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        /// <returns>Task</returns>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //HintDisplayName
            //output.Content.Append(Postfix);
            //return base.ProcessAsync(context, output);

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }


            var tagBuilder = new TagBuilder("label")
            {
                Attributes =
                {
                    new KeyValuePair<string, string>("id", For.Name+"_label"),
                    new KeyValuePair<string, string>("data-name", For.Name),
                }
            };

            //generate label
            //var tagBuilder = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, new { @class = "control-label" });
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

                var hits = For.Metadata.DisplayName?.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string name = "";
                string hit = "";

                if (hits?.Length > 0)
                {
                    name = hits[0];
                }

                if (hits?.Length > 1)
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

            return base.ProcessAsync(context, output);
        }
    }




}