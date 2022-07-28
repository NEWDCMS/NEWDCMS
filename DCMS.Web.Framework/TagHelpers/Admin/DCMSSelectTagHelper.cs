using DCMS.Web.Framework.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Framework.TagHelpers.Admin
{
    /// <summary>
    /// dcms-select tag helper
    /// </summary>
    [HtmlTargetElement("dcms-select", TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSSelectTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string NameAttributeName = "asp-for-name";
        private const string ItemsAttributeName = "asp-items";
        private const string MultipleAttributeName = "asp-multiple";
        private const string RequiredAttributeName = "asp-required";
        private const string DefaultAttributeName = "asp-default";
        private const string DisabledAttributeName = "asp-disabled";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }


        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }
        //

        [HtmlAttributeName(ItemsAttributeName)]
        public SelectList Items { set; get; }

        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        [HtmlAttributeName(DefaultAttributeName)]
        public string Default { set; get; }


        [HtmlAttributeName(MultipleAttributeName)]
        public string IsMultiple { set; get; }


        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public DCMSSelectTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
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

            //required asterisk
            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }


            //add disabled attribute
            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //get htmlAttributes object
            var htmlAttributes = new Dictionary<string, object>();
            var attributes = context.AllAttributes;

            if (disabled)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            //htmlAttributes.Add(attribute.Name, attribute.Value);

            foreach (var attribute in attributes)
            {
                if (!attribute.Name.Equals(ForAttributeName) &&
                !attribute.Name.Equals(NameAttributeName) &&
                !attribute.Name.Equals(ItemsAttributeName) &&
                !attribute.Name.Equals(DisabledAttributeName) &&
                !attribute.Name.Equals(RequiredAttributeName))
                {
                    htmlAttributes.Add(attribute.Name, attribute.Value);
                }
            }

            //generate editor
            var tagName = For != null ? For.Name : Name;
            bool.TryParse(IsMultiple, out bool multiple);
            if (!string.IsNullOrEmpty(tagName))
            {

                var items = Items.Select(s => s).ToList();

                if (!string.IsNullOrEmpty(Default))
                {
                    items.Add(new SelectListItem { Text = $"-{Default}-", Value = "0", Selected = true });
                }

                IHtmlContent selectList;

                if (multiple)
                {
                    selectList = _htmlHelper.Editor(tagName, "MultiSelect", new { htmlAttributes, SelectList = items });
                }
                else
                {
                    if (htmlAttributes.ContainsKey("class"))
                    {
                        htmlAttributes["class"] += " form-control";
                    }
                    else
                    {
                        htmlAttributes.Add("class", "form-control");
                    }

                    selectList = _htmlHelper.DropDownList(tagName, items, htmlAttributes);
                }
                output.Content.SetHtmlContent(selectList.RenderHtmlContent());
            }
        }
    }



    [HtmlTargetElement("dcms-select-bool", TagStructure = TagStructure.WithoutEndTag)]
    public class DCMSBoolSelectTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string NameAttributeName = "asp-for-name";
        private const string ItemsAttributeName = "asp-items";
        private const string RequiredAttributeName = "asp-required";
        private const string DefaultAttributeName = "asp-default";
        private const string DisabledAttributeName = "asp-disabled";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        [HtmlAttributeName(DefaultAttributeName)]
        public string Default { set; get; }

        [HtmlAttributeName(ItemsAttributeName)]
        public string[] Items { set; get; }

        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public DCMSBoolSelectTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
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

            output.SuppressOutput();


            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            //add disabled attribute
            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //get htmlAttributes object
            var htmlAttributes = new Dictionary<string, object>();
            var attributes = context.AllAttributes;

            if (disabled)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            foreach (var attribute in attributes)
            {
                if (!attribute.Name.Equals(ForAttributeName) &&
                !attribute.Name.Equals(NameAttributeName) &&
                !attribute.Name.Equals(ItemsAttributeName) &&
                !attribute.Name.Equals(DisabledAttributeName) &&
                !attribute.Name.Equals(RequiredAttributeName))
                {
                    htmlAttributes.Add(attribute.Name, attribute.Value);
                }
            }

            //generate editor
            var tagName = For != null ? For.Name : Name;
            if (!string.IsNullOrEmpty(tagName))
            {

                var items = new List<SelectListItem>();

                if (!string.IsNullOrEmpty(Default))
                {
                    items.Add(new SelectListItem { Text = $"-{Default}-", Value = "", Selected = true });
                }

                //int[] values = new int[] { 0, 1 };
                //foreach (var v in values)
                //{
                //    if (Items.Length > 1)
                //        items.Add(new SelectListItem { Text = (v == 0 ? Items[0] : Items[1]), Value = v.ToString() });
                //    else
                //        items.Add(new SelectListItem { Text = (v == 0 ? "False" : "True"), Value = v.ToString() });
                //}
                int[] values = new int[] { 1, 0 };
                foreach (var v in values)
                {
                    if (Items.Length > 1)
                    {
                        items.Add(new SelectListItem { Text = (v == 1 ? Items[0] : Items[1]), Value = (v == 1 ? "True" : "False") });
                    }
                    else
                    {
                        items.Add(new SelectListItem { Text = (v == 0 ? "False" : "True"), Value = v.ToString() });
                    }
                }

                IHtmlContent selectList;

                if (htmlAttributes.ContainsKey("class"))
                {
                    htmlAttributes["class"] += " form-control";
                }
                else
                {
                    htmlAttributes.Add("class", "form-control");
                }

                selectList = _htmlHelper.DropDownList(tagName, items, htmlAttributes);

                output.Content.SetHtmlContent(selectList.RenderHtmlContent());
            }
        }
    }
}

