using DCMS.Core;
using DCMS.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace DCMS.Web.Framework.Mvc.Filters
{

    ///// <summary>
    ///// WebAPI防篡改签名验证抽象基类过滤特性
    ///// </summary>
    //public abstract class AbsBaseAuthenticationAttribute : TypeFilterAttribute
    //{
    //    /// <summary>
    //    /// 在调用操作方法之前发生
    //    /// </summary>
    //    /// <param name="actionContext">The action context</param>
    //    public override void OnActionExecuting(ActionExecutingContext actionContext)
    //    {
    //        try
    //        {
    //            //获取Asp.Net对应的Request
    //            //var request = ((HttpContextWrapper)actionContext.Request.Properties["MS_HttpContext"]).Request;
    //            //此签名要求Partner及Sign均通过QueryString传递  
    //            var request = actionContext.HttpContext.Request;

    //            //开发调试从Header取的Partner
    //            var partner = request.Headers["Partner"].ToString();
    //            var partnerKey = request.Headers["PartnerKey"].ToString();

    //            NameValueCollection getCollection = HttpUtility.ParseQueryString(request.QueryString.Value);
    //            if (getCollection != null && getCollection.Count > 0)
    //            {
    //                //从客户端取Partner
    //                if (!request.Headers.ContainsKey("Partner") || string.IsNullOrEmpty(partner))
    //                {
    //                    partner = getCollection[SecuritySignHelper.Partner];
    //                }

    //                if (!request.Headers.ContainsKey("PartnerKey") || string.IsNullOrEmpty(partnerKey))
    //                {
    //                    partnerKey = getCollection["PartnerKey"];
    //                }

    //                //获取客户端保存签名
    //                string sign = getCollection[SecuritySignHelper.Sign];

    //                //Debug 下使用
    //                //if (!string.IsNullOrEmpty(partnerKey))
    //                //{
    //                //    //sign = getCollection.GetSecuritySign(partner, partnerKey, null);
    //                //    if (string.Equals(partnerKey, this.GetPartnerKey(partner), StringComparison.OrdinalIgnoreCase))
    //                //    {
    //                //        //验证通过,执行基类方法
    //                //        base.OnActionExecuting(actionContext);
    //                //        return;
    //                //    }
    //                //}

    //                if (!string.IsNullOrWhiteSpace(partner)//必须包含partner
    //                   && !string.IsNullOrWhiteSpace(sign)//必须包含sign
    //                   && Regex.IsMatch(sign, "^[0-9A-Za-z]{32}$"))//sign必须为32位Md5摘要
    //                {
    //                    //获取partner对应的key
    //                    //这里暂时只做了合作key校验，不做访问权限校验，如有需要，此处可进行调整，建议RBAC
    //                    partnerKey = this.GetPartnerKey(partner);
    //                    if (!string.IsNullOrWhiteSpace(partnerKey))
    //                    {
    //                        //移除Key保证和客户端一致
    //                        getCollection.Remove("partner");
    //                        getCollection.Remove("sign");

    //                        NameValueCollection postCollection = null;
    //                        switch (request.Method)
    //                        {
    //                            case "GET":
    //                            case "DELETE": break;//只是为了同时显示restful四种方式才有这部分无意义代码
    //                            case "POST":
    //                            case "PUT":
    //                                {
    //                                    var model = actionContext.ActionArguments.Values.OfType<BaseModel>().FirstOrDefault();
    //                                    if (model != null)
    //                                    {
    //                                        postCollection = SecuritySignHelper.ToNameValueCollection(model);
    //                                    }
    //                                }
    //                                break;
    //                            default:
    //                                throw new NotImplementedException();
    //                        }
    //                        //根据请求数据获取MD5签名
    //                        //"86055e3da86e55059f45eee601b35ed9"
    //                        //"f2f03a14210adc6f34e75345dc2342f8"
    //                        //"658c9c27ba8910f8283ad64eae91fb73"
    //                        string vSign = getCollection.GetSecuritySign(partner, partnerKey, postCollection);
    //                        if (string.Equals(sign, vSign, StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            //验证通过,执行基类方法
    //                            base.OnActionExecuting(actionContext);
    //                            return;
    //                        }
    //                    }
    //                }
    //            }

    //            actionContext.Result = new ContentResult
    //            {
    //                Content = "Authentication failed Unauthorized!",
    //                ContentType = "application/json",
    //                StatusCode = (int)HttpStatusCode.Unauthorized
    //            };
    //        }
    //        catch (Exception ex)
    //        {
    //            actionContext.Result = new ContentResult
    //            {
    //                Content = ex.Message,
    //                ContentType = "application/json",
    //                StatusCode = (int)HttpStatusCode.Forbidden
    //            };
    //        }
    //    }

    //    /// <summary>
    //    /// 获取合作号对应的合作Key,如果未能获取，则返回空字符串或者null
    //    /// </summary>
    //    /// <param name="partner"></param>
    //    /// <returns></returns>
    //    protected abstract string GetPartnerKey(string partner);
    //}

    /// <summary>
    /// WebAPI防篡改签名验证抽象基类过滤特性
    /// </summary>
    public class AuthBaseFilterAttribute : TypeFilterAttribute
    {

        private readonly bool _ignoreFilter;

        public AuthBaseFilterAttribute(bool ignore = false) : base(typeof(AuthenticationFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        /// <summary>
        /// 是否忽略筛选器操作的执行
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;


        /// <summary>
        /// 表示确认访问管理面板的筛选器
        /// </summary>
        private class AuthenticationFilter : IActionFilter
        {

            private readonly bool _ignoreFilter;
            private readonly IPartnerService _partnerService;
            //private static readonly ConcurrentDictionary<string, string> _dictionary = new ConcurrentDictionary<string, string>();
            //private static object sign = new object();

            public AuthenticationFilter(bool ignoreFilter, IPartnerService partnerService)
            {
                _ignoreFilter = ignoreFilter;
                _partnerService = partnerService;
            }

            /// <summary>
            /// 在筛选器管道的早期调用以确认请求已授权
            /// </summary>
            /// <param name="filterContext"></param>
            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException(nameof(filterContext));
                }

                //检查是否已为此操作重写此筛选器
                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<AuthBaseFilterAttribute>().FirstOrDefault();

                //忽略筛选器（即使用户没有访问管理区域，该操作也可用）
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                //检查访问权限
                if (filterContext.Filters.Any(filter => filter is AuthenticationFilter))
                {
                    try
                    {

                        //此签名要求Partner及Sign均通过QueryString传递  
                        var request = filterContext.HttpContext.Request;

                        //开发调试从Header取的Partner
                        var partner = request.Headers["Partner"].ToString();
                        var partnerKey = request.Headers["PartnerKey"].ToString();

                        string msq = "";
                        var getCollection = HttpUtility.ParseQueryString(request.QueryString.Value);


                        //debug(平台测试使用)
                        if (!string.IsNullOrEmpty(partnerKey) || request.QueryString.Value.Contains("PartnerKey"))
                        {
                            return;
                        }

                        if (getCollection != null && getCollection.Count > 0)
                        {
                            //从客户端取Partner
                            if (!request.Headers.ContainsKey("Partner") || string.IsNullOrEmpty(partner))
                            {
                                partner = getCollection[SecuritySignHelper.Partner];
                            }

                            if (!request.Headers.ContainsKey("PartnerKey") || string.IsNullOrEmpty(partnerKey))
                            {
                                partnerKey = getCollection["PartnerKey"];
                            }

                            //debug(平台测试使用)
                            if (!string.IsNullOrEmpty(partnerKey))
                            {
                                return;
                            }


                            //获取客户端保存签名
                            string sign = getCollection[SecuritySignHelper.Sign];

                            if (!string.IsNullOrWhiteSpace(partner)//必须包含partner
                               && !string.IsNullOrWhiteSpace(sign)//必须包含sign
                               && Regex.IsMatch(sign, "^[0-9A-Za-z]{32}$"))//sign必须为32位Md5摘要
                            {
                                //获取partner对应的key
                                //这里暂时只做了合作key校验，不做访问权限校验，如有需要，此处可进行调整，建议RBAC
                                var _partner = _partnerService.GetPartnerByUserName(partner);
                                if (_partner != null)
                                {
                                    partnerKey = _partner.AccessKey;
                                }
                                else
                                {
                                    partnerKey = "";
                                }

                                if (!string.IsNullOrWhiteSpace(partnerKey))
                                {

                                    //移除Key保证和客户端一致
                                    //getCollection.Remove("partner");
                                    //getCollection.Remove("sign");

                                    //var headersString = string.Join(Environment.NewLine, request.Headers.AllKeys.SelectMany(request.Headers.GetValues, (k, v) => k + ": " + v));
                                    //string headersString = string.Join("&", request.Headers.Select(s => s.Key + "=" + s.Value.ToString()));

                                    var tempGetCollection = new NameValueCollection();
                                    var postCollection = new NameValueCollection();

                                    lock (sign)
                                    {
                                        foreach (string key in getCollection)
                                        {
                                            if (!key.ToLower().Equals("partner", StringComparison.OrdinalIgnoreCase) && !key.ToLower().Equals("sign", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (!tempGetCollection.AllKeys.Contains(key))
                                                {
                                                    tempGetCollection.Add(key.ToLower(), getCollection[key].ToLower());
                                                }
                                            }
                                        }

                                        switch (request.Method)
                                        {
                                            case "GET":
                                            case "DELETE":
                                                break;
                                            case "POST":
                                            case "PUT":
                                                {
                                                    //var model = filterContext.ActionArguments.Values.OfType<BaseModel>().FirstOrDefault();
                                                    //if (model != null)
                                                    //{
                                                    //    postCollection = SecuritySignHelper.ToNameValueCollection(model);
                                                    //}
                                                    return;
                                                }
                                            default:
                                                throw new NotImplementedException();
                                        }
                                        //"53698f03b74cb1772a6968ab33bdbcc3"
                                        //"462c7300b2e3ed635d0b0179f63c6391"
                                        string vSign = tempGetCollection.GetSecuritySign(partner, partnerKey, postCollection);
                                        if (string.Equals(sign, vSign, StringComparison.OrdinalIgnoreCase))
                                        {
                                            //验证通过,执行基类方法
                                            return;
                                        }
                                        else
                                        {
                                            msq += $"sign != vSign;  partner = {partner}  vSign = {vSign}  tempGetCollection={string.Join("&", tempGetCollection.AllKeys.SelectMany(tempGetCollection.GetValues, (k, v) => k + "=" + v))}";
                                        }
                                    }

                                }
                                else
                                {
                                    msq += "partnerKey 为空;";
                                }
                            }
                            else
                            {
                                msq += "partner/sign 为空;";
                            }
                        }
                        else
                        {
                            msq += "getCollection 为空;";
                        }

                        filterContext.Result = new ContentResult
                        {
                            Content = $"Authentication failed Unauthorized!  Error:{msq}",
                            ContentType = "application/json",
                            StatusCode = (int)HttpStatusCode.Unauthorized
                        };

                    }
                    catch (Exception ex)
                    {
                        filterContext.Result = new ContentResult
                        {
                            Content = ex.Message,
                            ContentType = "application/json",
                            StatusCode = (int)HttpStatusCode.Forbidden
                        };
                    }
                }

            }


            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

        }
    }
}



