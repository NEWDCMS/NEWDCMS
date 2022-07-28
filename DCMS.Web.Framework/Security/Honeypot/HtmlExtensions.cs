using DCMS.Core.Domain.Security;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;

namespace DCMS.Web.Framework.Security.Honeypot
{
    /// <summary>
    /// HTML 扩展
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// 产生蜜罐输入（防伪造）
        /// </summary>
        /// <param name="helper">HTML helper</param>
        /// <returns>Result</returns>
        public static IHtmlContent GenerateHoneypotInput(this IHtmlHelper helper)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("<div style=\"display:none;\">");
            sb.Append(Environment.NewLine);

            var securitySettings = EngineContext.Current.Resolve<SecuritySettings>();
            sb.AppendFormat("<input id=\"{0}\" name=\"{0}\" type=\"text\">", securitySettings.HoneypotInputName);

            sb.Append(Environment.NewLine);
            sb.Append("</div>");

            return new HtmlString(sb.ToString());
        }
    }
}