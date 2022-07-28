namespace DCMS.Core.UrlFirewall
{
    public interface IUrlFirewallValidator
    {
        UrlFirewallOptions Options { get; set; }

        /// <summary>
        /// ValidateUrl ip
        /// </summary>
        /// <param name="url">Request ip</param>
        /// <param name="method">Request method</param>
        /// <returns>True:Allow this request.False:This request is not allowed</returns>
        bool ValidateUrl(string url, string method);

        bool ValidateIp(string ip, string method);
    }
}