using DCMS.Core;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace DCMS.Web.Framework
{
    /// <summary>
    /// 用于远程保存请求
    /// </summary>
    public partial class RemotePost
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly NameValueCollection _inputValues;

        public string Url { get; set; }
        public string Method { get; set; }
        public string FormName { get; set; }
        public string AcceptCharset { get; set; }
        public bool NewInputForEachValue { get; set; }

        public NameValueCollection Params
        {
            get
            {
                return _inputValues;
            }
        }


        public RemotePost()
            : this(EngineContext.Current.Resolve<IHttpContextAccessor>(), EngineContext.Current.Resolve<IWebHelper>())
        {
        }


        public RemotePost(IHttpContextAccessor httpContextAccessor, IWebHelper webHelper)
        {
            _inputValues = new NameValueCollection();
            Url = "http://www.jsdcms.com";
            Method = "post";
            FormName = "formName";

            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
        }


        public void Add(string name, string value)
        {
            _inputValues.Add(name, value);
        }

        public void Post()
        {
            //text
            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append($"</head><body onload=\"document.{FormName}.submit()\">");
            if (!string.IsNullOrEmpty(AcceptCharset))
            {
                //AcceptCharset specified
                sb.Append(
                    $"<form name=\"{FormName}\" method=\"{Method}\" action=\"{Url}\" accept-charset=\"{AcceptCharset}\">");
            }
            else
            {
                //no AcceptCharset specified
                sb.Append($"<form name=\"{FormName}\" method=\"{Method}\" action=\"{Url}\" >");
            }
            if (NewInputForEachValue)
            {
                foreach (string key in _inputValues.Keys)
                {
                    var values = _inputValues.GetValues(key);
                    if (values != null)
                    {
                        foreach (var value in values)
                        {
                            sb.Append(
                                $"<input name=\"{WebUtility.HtmlEncode(key)}\" type=\"hidden\" value=\"{WebUtility.HtmlEncode(value)}\">");
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < _inputValues.Keys.Count; i++)
                {
                    sb.Append(
                        $"<input name=\"{WebUtility.HtmlEncode(_inputValues.Keys[i])}\" type=\"hidden\" value=\"{WebUtility.HtmlEncode(_inputValues[_inputValues.Keys[i]])}\">");
                }
            }
            sb.Append("</form>");
            sb.Append("</body></html>");


            //post
            var httpContext = _httpContextAccessor.HttpContext;
            var response = httpContext.Response;
            response.Clear();
            var data = Encoding.UTF8.GetBytes(sb.ToString());
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength = data.Length;

            response.Body.Write(data, 0, data.Length);

            //store a value indicating whether POST has been done
            _webHelper.IsPostBeingDone = true;
        }
    }
}