using DCMS.Core;
using DCMS.Core.Domain.Security;
using DCMS.Services.Security;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DCMS.Web.Framework.Security.Captcha
{
    /// <summary>
    /// 请求验证码服务
    /// </summary>
    public partial class CaptchaHttpClient
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly HttpClient _httpClient;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CaptchaHttpClient(CaptchaSettings captchaSettings,
            HttpClient client,
            IWebHelper webHelper)
        {
            //configure client
            client.BaseAddress = new Uri(DCMSSecurityDefaults.RecaptchaApiUrl);
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"jsdcms-{DCMSVersion.CurrentVersion}");

            _captchaSettings = captchaSettings;
            _httpClient = client;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate reCAPTCHA
        /// </summary>
        /// <param name="responseValue">Response value</param>
        /// <returns>The asynchronous task whose result contains response from the reCAPTCHA service</returns>
        public virtual async Task<CaptchaResponse> ValidateCaptchaAsync(string responseValue)
        {
            //prepare URL to request
            var url = string.Format(DCMSSecurityDefaults.RecaptchaValidationPath,
                _captchaSettings.ReCaptchaPrivateKey,
                responseValue,
                _webHelper.GetCurrentIpAddress());

            //get response
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<CaptchaResponse>(response);

        }

        #endregion
    }
}