using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Core.Domain.Stores
{
    public static class StoreExtensions
    {

        /// <summary>
        /// 分段解析域
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>Comma-separated hosts</returns>
        public static string[] ParseHostValues(this Store store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            var parsedValues = new List<string>();
            if (!string.IsNullOrEmpty(store.Hosts))
            {
                string[] hosts = store.Hosts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string host in hosts)
                {
                    var tmp = host.Trim();
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        parsedValues.Add(tmp);
                    }
                }
            }
            return parsedValues.ToArray();
        }

        /// <summary>
        /// 是否包含指定域
        /// </summary>
        /// <param name="store">Store</param>
        /// <param name="host">Host</param>
        /// <returns>true - contains, false - no</returns>
        public static bool ContainsHostValue(this Store store, string host)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            var contains = store.ParseHostValues()
                                .FirstOrDefault(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase)) != null;
            return contains;
        }
    }
}
