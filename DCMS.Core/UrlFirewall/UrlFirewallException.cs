using System;

namespace DCMS.Core.UrlFirewall
{
    public class UrlFirewallException : Exception
    {
        public UrlFirewallException(string message, Exception innerExcetion) : base(message, innerExcetion)
        {

        }

        public UrlFirewallException(string message) : base(message)
        {

        }
    }
}