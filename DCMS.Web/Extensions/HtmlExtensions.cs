using DCMS.ViewModel.Models.Common;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.UI.Paging;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;
using System.Text.Encodings.Web;

namespace DCMS.Web.Extensions
{
    public static class HtmlExtensions
    {

        public static IHtmlContent Pager<TModel>(this IHtmlHelper<TModel> html, PagerModel model)
        {
            if (model.TotalRecords == 0)
            {
                return new HtmlString("");
            }

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(model.CurrentPageText, model.PageIndex + 1, model.TotalPages, model.TotalRecords));
                links.Append("</li>");
            }
            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.page = 1;

                        links.Append("<li class=\"first-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = "<<" });
                            links.Append(link.ToHtmlString());
                        }
                        else
                        {
                            var link = html.ActionLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = "<<" });
                            links.Append(link.ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.page = (model.PageIndex);

                        links.Append("<li class=\"previous-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = "<" });
                            links.Append(link.ToHtmlString());
                        }
                        else
                        {
                            var link = html.ActionLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = "<" });
                            links.Append(link.ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowIndividualPages)
                {
                    //individual pages
                    var firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page\"><span>{0}</span></li>", (i + 1));
                        }
                        else
                        {
                            model.RouteValues.page = (i + 1);

                            links.Append("<li class=\"individual-page\">");
                            if (model.UseRouteLinks)
                            {
                                var link = html.RouteLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = string.Format("{0}", (i + 1)) });
                                links.Append(link.ToHtmlString());
                            }
                            else
                            {
                                var link = html.ActionLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = string.Format("{0}", (i + 1)) });
                                links.Append(link.ToHtmlString());
                            }
                            links.Append("</li>");
                        }
                    }
                }
                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.page = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = ">" });
                            links.Append(link.ToHtmlString());
                        }
                        else
                        {
                            var link = html.ActionLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = ">" });
                            links.Append(link.ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.page = model.TotalPages;

                        links.Append("<li class=\"last-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = ">>" });
                            links.Append(link.ToHtmlString());
                        }
                        else
                        {
                            var link = html.ActionLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = ">>" });
                            links.Append(link.ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
            }
            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "<ul>" + result + "</ul>";
            }
            return new HtmlString(result);
        }
        public static Pager Pager(this IHtmlHelper helper, IPageableModel pagination)
        {
            return new Pager(pagination, helper.ViewContext);
        }


        public static IHtmlContent PayStatus(this IHtmlHelper helper, int value)
        {
            string result;
            TagBuilder span = new TagBuilder("span");
            foreach (var item in Enum.GetValues(typeof(DCMS.Core.PayStatus)))
            {
                if ((int)item == value)
                {
                    var des = DCMS.Core.CommonHelper.GetEnumDescription<DCMS.Core.PayStatus>(value);
                    var names = des.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (names.Length > 0)
                    {
                        span.InnerHtml.Append(names[0]);
                    }

                    if (names.Length > 1)
                    {
                        span.MergeAttribute("class", $"label label-{names[1]}");
                    }

                    break;
                }
            }

            using (var sw = new System.IO.StringWriter())
            {
                span.WriteTo(sw, HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }


        public static IHtmlContent ReceiptStatus(this IHtmlHelper helper, int value)
        {
            string result;
            TagBuilder span = new TagBuilder("span");
            foreach (var item in Enum.GetValues(typeof(DCMS.Core.ReceiptStatus)))
            {
                if ((int)item == value)
                {
                    var des = DCMS.Core.CommonHelper.GetEnumDescription<DCMS.Core.ReceiptStatus>(value);
                    var names = des.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (names.Length > 0)
                    {
                        span.InnerHtml.Append(names[0]);
                    }

                    if (names.Length > 1)
                    {
                        span.MergeAttribute("class", $"label label-{names[1]}");
                    }

                    break;
                }
            }

            using (var sw = new System.IO.StringWriter())
            {
                span.WriteTo(sw, HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        /// <param name="ReversedStatus"></param>
        /// <param name="AuditedStatus"></param>
        /// <returns></returns>
        public static IHtmlContent AuditedStatus(this IHtmlHelper helper, bool ReversedStatus, bool AuditedStatus)
        {
            //int value;
            //int value1= ReversedStatus ? 2 : 0;
            //if (value1 == 2)
            //{
            //    value = 2;
            //}
            //else
            //{
            //    value = AuditedStatus ? 1 : 0;
            //}
            //string result;
            //TagBuilder span = new TagBuilder("span");
            //foreach (var item in Enum.GetValues(typeof(DCMS.Core.BillStates)))
            //{
            //    if ((int)item == value)
            //    {
            //        var des = DCMS.Core.CommonHelper.GetEnumDescription<DCMS.Core.BillStates>(value);
            //        var names = des.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            //        if (names.Length > 0)
            //            span.InnerHtml.Append(names[0]);
            //        if (names.Length > 1)
            //            span.MergeAttribute("class", $"label label-{names[1]}");
            //        break;
            //    }
            //}
            string result;
            TagBuilder span = new TagBuilder("span");
            if (ReversedStatus)
            {
                span.MergeAttribute("class", "label label-danger");
                span.InnerHtml.SetContent("已红冲");
            }
            else
            {
                if (AuditedStatus)
                {
                    span.MergeAttribute("class", "label label-success");
                    span.InnerHtml.SetContent("已审核");
                }
                else
                {
                    span.MergeAttribute("class", "label label-danger");
                    span.InnerHtml.SetContent("未审核");
                }
            }
            using (var sw = new System.IO.StringWriter())
            {
                span.WriteTo(sw, HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }

        public static IHtmlContent Operation(this IHtmlHelper helper, int value)
        {
            string result;
            TagBuilder span = new TagBuilder("span");
            span.MergeAttribute("class", "label label-info");
            if (value == 0)
            {
                span.InnerHtml.SetContent("线上");
            }
            else
            {
                span.InnerHtml.SetContent("线下");
            }
            using (var sw = new System.IO.StringWriter())
            {
                span.WriteTo(sw, HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }

    }
}