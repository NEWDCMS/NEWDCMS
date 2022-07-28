using System.Collections.Specialized;

namespace DCMS.Core
{
    public class DCMSValueCollection : NameValueCollection
    {
        public DCMSValueCollection()
            : base() // case-insensitive keys
        {
            Get = new NameValueCollection();
            Post = new NameValueCollection();
        }
        /// <summary>
        /// 通过QueryString方式传递的键值集合,如果内部包含parnter或者sign，相关字段在组织原始字符串时将会被移除
        /// </summary>
        public new NameValueCollection Get { get; set; }
        /// <summary>
        /// 通过Form方式传递的键值集合，如果包含parnter或者sign
        /// </summary>
        public NameValueCollection Post { get; set; }
    }

}

