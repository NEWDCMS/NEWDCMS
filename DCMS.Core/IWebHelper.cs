using Microsoft.AspNetCore.Http;

namespace DCMS.Core
{
    public interface IWebHelper
    {
        string CurrentRequestProtocol { get; }
        bool IsPostBeingDone { get; set; }
        bool IsRequestBeingRedirected { get; }

        string GetCurrentIpAddress();
        string GetRawUrl(HttpRequest request);
        string GetStoreHost(bool useSsl);
        string GetStoreLocation(bool? useSsl = null);
        string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false);
        string GetUrlReferrer();
        bool IsAjaxRequest(HttpRequest request);
        bool IsCurrentConnectionSecured();
        bool IsLocalRequest(HttpRequest req);
        bool IsStaticResource();
        string ModifyQueryString(string url, string key, params string[] values);
        T QueryString<T>(string name);
        string RemoveQueryString(string url, string key, string value = null);
        void RestartAppDomain(bool makeRedirect = false);
    }
}