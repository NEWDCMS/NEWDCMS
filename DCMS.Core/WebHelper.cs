using DCMS.Core.Configuration;
using DCMS.Core.Data;
using DCMS.Core.Http;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DCMS.Core
{

    public partial class WebHelper : IWebHelper
    {

        private readonly HostingConfig _hostingConfig;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDCMSFileProvider _fileProvider;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public WebHelper(HostingConfig hostingConfig,
            IActionContextAccessor actionContextAccessor,
            IHostApplicationLifetime applicationLifetime,
            IHttpContextAccessor httpContextAccessor,
            IDCMSFileProvider fileProvider,
            IUrlHelperFactory urlHelperFactory)
        {
            _hostingConfig = hostingConfig;
            _actionContextAccessor = actionContextAccessor;
            _applicationLifetime = applicationLifetime;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _urlHelperFactory = urlHelperFactory;
        }


        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                return false;
            }

            try
            {
                if (_httpContextAccessor.HttpContext.Request == null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        protected virtual bool IsIpAddressSet(IPAddress address)
        {
            return address != null && address.ToString() != IPAddress.IPv6Loopback.ToString();
        }

        protected virtual bool TryWriteWebConfig()
        {
            try
            {
                _fileProvider.SetLastWriteTimeUtc(_fileProvider.MapPath(DCMSInfrastructureDefaults.WebConfigPath), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
            {
                return string.Empty;
            }

            //URL referrer is null in some case (for example, in IE 8)
            return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }


        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
            {
                return string.Empty;
            }

            var result = string.Empty;
            try
            {
                //first try to get IP address from the forwarded header
                if (_httpContextAccessor.HttpContext.Request.Headers != null)
                {
                    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                    //connecting to a web server through an HTTP proxy or load balancer
                    var forwardedHttpHeaderKey = DCMSHttpDefaults.XForwardedForHeader;
                    if (!string.IsNullOrEmpty(_hostingConfig.ForwardedHttpHeader))
                    {
                        //but in some cases server use other HTTP header
                        //in these cases an administrator can specify a custom Forwarded HTTP header (e.g. CF-Connecting-IP, X-FORWARDED-PROTO, etc)
                        forwardedHttpHeaderKey = _hostingConfig.ForwardedHttpHeader;
                    }

                    var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers[forwardedHttpHeaderKey];
                    if (!StringValues.IsNullOrEmpty(forwardedHeader))
                    {
                        result = forwardedHeader.FirstOrDefault();
                    }
                }

                //if this header not exists try get connection remote IP address
                if (string.IsNullOrEmpty(result) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
                {
                    result = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }

            //some of the validation
            if (result != null && result.Equals(IPAddress.IPv6Loopback.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                result = IPAddress.Loopback.ToString();
            }

            //"TryParse" doesn't support IPv4 with port number
            if (IPAddress.TryParse(result ?? string.Empty, out var ip))
            {
                //IP address is valid 
                result = ip.ToString();
            }
            else if (!string.IsNullOrEmpty(result))
            {
                //remove port
                result = result.Split(':').FirstOrDefault();
            }

            return result;
        }


        public virtual string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false)
        {
            if (!IsRequestAvailable())
            {
                return string.Empty;
            }

            //get store location
            var storeLocation = GetStoreLocation(useSsl ?? IsCurrentConnectionSecured());

            //add local path to the URL
            var pageUrl = $"{storeLocation.TrimEnd('/')}{_httpContextAccessor.HttpContext.Request.Path}";

            //add query string to the URL
            if (includeQueryString)
            {
                pageUrl = $"{pageUrl}{_httpContextAccessor.HttpContext.Request.QueryString}";
            }

            //whether to convert the URL to lower case
            if (lowercaseUrl)
            {
                pageUrl = pageUrl.ToLowerInvariant();
            }

            return pageUrl;
        }


        public virtual bool IsCurrentConnectionSecured()
        {
            if (!IsRequestAvailable())
            {
                return false;
            }

            //check whether hosting uses a load balancer
            //use HTTP_CLUSTER_HTTPS?
            if (_hostingConfig.UseHttpClusterHttps)
            {
                return _httpContextAccessor.HttpContext.Request.Headers[DCMSHttpDefaults.HttpClusterHttpsHeader].ToString().Equals("on", StringComparison.OrdinalIgnoreCase);
            }

            //use HTTP_X_FORWARDED_PROTO?
            if (_hostingConfig.UseHttpXForwardedProto)
            {
                return _httpContextAccessor.HttpContext.Request.Headers[DCMSHttpDefaults.HttpXForwardedProtoHeader].ToString().Equals("https", StringComparison.OrdinalIgnoreCase);
            }

            return _httpContextAccessor.HttpContext.Request.IsHttps;
        }


        public virtual string GetStoreHost(bool useSsl)
        {
            if (!IsRequestAvailable())
            {
                return string.Empty;
            }

            //try to get host from the request HOST header
            var hostHeader = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Host];
            if (StringValues.IsNullOrEmpty(hostHeader))
            {
                return string.Empty;
            }

            //add scheme to the URL
            var storeHost = $"{(useSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)}{Uri.SchemeDelimiter}{hostHeader.FirstOrDefault()}";

            //ensure that host is ended with slash
            storeHost = $"{storeHost.TrimEnd('/')}/";

            return storeHost;
        }


        public virtual string GetStoreLocation(bool? useSsl = null)
        {
            var storeLocation = string.Empty;

            //get store host
            var storeHost = GetStoreHost(useSsl ?? IsCurrentConnectionSecured());
            if (!string.IsNullOrEmpty(storeHost))
            {
                //add application path base if exists
                storeLocation = IsRequestAvailable() ? $"{storeHost.TrimEnd('/')}{_httpContextAccessor.HttpContext.Request.PathBase}" : storeHost;
            }

            //if host is empty (it is possible only when HttpContext is not available), use URL of a store entity configured in admin area
            if (string.IsNullOrEmpty(storeHost) && DataSettingsManager.DatabaseIsInstalled)
            {
                //do not inject IWorkContext via constructor because it'll cause circular references
                storeLocation = EngineContext.Current.Resolve<IStoreContext>().CurrentStore?.Url
                    ?? throw new Exception("Current store cannot be loaded");
            }

            //ensure that URL is ended with slash
            storeLocation = $"{storeLocation.TrimEnd('/')}/";

            return storeLocation;
        }


        public virtual bool IsStaticResource()
        {
            if (!IsRequestAvailable())
            {
                return false;
            }

            string path = _httpContextAccessor.HttpContext.Request.Path;

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            return contentTypeProvider.TryGetContentType(path, out var _);
        }


        public virtual string ModifyQueryString(string url, string key, params string[] values)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(key))
            {
                return url;
            }

            //prepare URI object
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var isLocalUrl = urlHelper.IsLocalUrl(url);
            var uri = new Uri(isLocalUrl ? $"{GetStoreLocation().TrimEnd('/')}{url}" : url, UriKind.Absolute);

            //get current query parameters
            var queryParameters = QueryHelpers.ParseQuery(uri.Query);

            //and add passed one
            queryParameters[key] = string.Join(",", values);

            //add only first value
            //two the same query parameters? theoretically it's not possible.
            //but MVC has some ugly implementation for checkboxes and we can have two values
            //find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
            //we do this validation just to ensure that the first one is not overridden
            var queryBuilder = new QueryBuilder(queryParameters
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value.FirstOrDefault()?.ToString() ?? string.Empty));

            //create new URL with passed query parameters
            url = $"{(isLocalUrl ? uri.LocalPath : uri.GetLeftPart(UriPartial.Path))}{queryBuilder.ToQueryString()}{uri.Fragment}";

            return url;
        }


        public virtual string RemoveQueryString(string url, string key, string value = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(key))
            {
                return url;
            }

            //prepare URI object
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var isLocalUrl = urlHelper.IsLocalUrl(url);
            var uri = new Uri(isLocalUrl ? $"{GetStoreLocation().TrimEnd('/')}{url}" : url, UriKind.Absolute);

            //get current query parameters
            var queryParameters = QueryHelpers.ParseQuery(uri.Query)
                .SelectMany(parameter => parameter.Value, (parameter, queryValue) => new KeyValuePair<string, string>(parameter.Key, queryValue))
                .ToList();

            if (!string.IsNullOrEmpty(value))
            {
                //remove a specific query parameter value if it's passed
                queryParameters.RemoveAll(parameter => parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)
                    && parameter.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                //or remove query parameter by the key
                queryParameters.RemoveAll(parameter => parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            }

            var queryBuilder = new QueryBuilder(queryParameters);

            //create new URL without passed query parameters
            url = $"{(isLocalUrl ? uri.LocalPath : uri.GetLeftPart(UriPartial.Path))}{queryBuilder.ToQueryString()}{uri.Fragment}";

            return url;
        }


        public virtual T QueryString<T>(string name)
        {
            if (!IsRequestAvailable())
            {
                return default(T);
            }

            if (StringValues.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Query[name]))
            {
                return default(T);
            }

            return CommonHelper.To<T>(_httpContextAccessor.HttpContext.Request.Query[name].ToString());
        }


        public virtual void RestartAppDomain(bool makeRedirect = false)
        {
            var success = TryWriteWebConfig();
            if (!success)
            {
                throw new DCMSException("jsdcms needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                    "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                    "- run the application in a full trust environment, or" + Environment.NewLine +
                    "- give the application write access to the 'web.config' file.");
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _applicationLifetime.StopApplication();
            }
        }


        public virtual bool IsRequestBeingRedirected
        {
            get
            {
                var response = _httpContextAccessor.HttpContext.Response;
                //ASP.NET 4 style - return response.IsRequestBeingRedirected;
                int[] redirectionStatusCodes = { StatusCodes.Status301MovedPermanently, StatusCodes.Status302Found };
                return redirectionStatusCodes.Contains(response.StatusCode);
            }
        }


        public virtual bool IsPostBeingDone
        {
            get
            {
                if (_httpContextAccessor.HttpContext.Items[DCMSHttpDefaults.IsPostBeingDoneRequestItem] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(_httpContextAccessor.HttpContext.Items[DCMSHttpDefaults.IsPostBeingDoneRequestItem]);
            }

            set => _httpContextAccessor.HttpContext.Items[DCMSHttpDefaults.IsPostBeingDoneRequestItem] = value;
        }


        public virtual string CurrentRequestProtocol => IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;


        public virtual bool IsLocalRequest(HttpRequest req)
        {
            //source: https://stackoverflow.com/a/41242493/7860424
            var connection = req.HttpContext.Connection;
            if (IsIpAddressSet(connection.RemoteIpAddress))
            {
                //We have a remote address set up
                return IsIpAddressSet(connection.LocalIpAddress)
                    //Is local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    //else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }


        public virtual string GetRawUrl(HttpRequest request)
        {
            //first try to get the raw target from request feature
            //note: value has not been UrlDecoded
            var rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            //or compose raw URL manually
            if (string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = $"{request.PathBase}{request.Path}{request.QueryString}";
            }

            return rawUrl;
        }


        public virtual bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Headers == null)
            {
                return false;
            }

            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

    }
}