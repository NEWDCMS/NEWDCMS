using DCMS.Core;
using DCMS.Core.Domain.Security;
using DCMS.Core.Infrastructure;
//using DCMS.Services.Localization;
using DCMS.Services.Security;
using DCMS.Web.Framework.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DCMS.Web.Framework.Security.Captcha
{
    /// <summary>
    /// HTML extensions
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Generate reCAPTCHA Control
        /// </summary>
        /// <param name="helper">HTML helper</param>
        /// <returns>Result</returns>
        public static IHtmlContent GenerateCaptcha(this IHtmlHelper helper)
        {
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();
            //prepare theme
            var theme = (captchaSettings.ReCaptchaTheme ?? string.Empty).ToLower();
            switch (theme)
            {
                case "blackglass":
                case "dark":
                    theme = "dark";
                    break;

                case "clean":
                case "red":
                case "white":
                case "light":
                default:
                    theme = "light";
                    break;
            }

            //prepare identifier
            var id = $"captcha_{CommonHelper.GenerateRandomInteger()}";

            //prepare public key
            var publicKey = captchaSettings.ReCaptchaPublicKey ?? string.Empty;

            //generate reCAPTCHA Control
            var scriptCallbackTag = new TagBuilder("script") { TagRenderMode = TagRenderMode.Normal };
            scriptCallbackTag.InnerHtml
                .AppendHtml($"var onloadCallback{id} = function() {{grecaptcha.render('{id}', {{'sitekey' : '{publicKey}', 'theme' : '{theme}' }});}};");

            var captchaTag = new TagBuilder("div") { TagRenderMode = TagRenderMode.Normal };
            captchaTag.Attributes.Add("id", id);

            var url = string.Format($"{DCMSSecurityDefaults.RecaptchaApiUrl}{DCMSSecurityDefaults.RecaptchaScriptPath}", id, "&hl=zh");
            var scriptLoadApiTag = new TagBuilder("script") { TagRenderMode = TagRenderMode.Normal };
            scriptLoadApiTag.Attributes.Add("src", url);
            scriptLoadApiTag.Attributes.Add("async", null);
            scriptLoadApiTag.Attributes.Add("defer", null);

            return new HtmlString(scriptCallbackTag.RenderHtmlContent() + captchaTag.RenderHtmlContent() + scriptLoadApiTag.RenderHtmlContent());
        }
    }
}