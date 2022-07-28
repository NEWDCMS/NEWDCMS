using System.Collections.Generic;

namespace DCMS.Web.Framework.Events
{
    /// <summary>
    /// 商品搜索事件
    /// </summary>
    public class ProductSearchEvent
    {
        /// <summary>
        /// 搜索词
        /// </summary>
        public string SearchTerm { get; set; }
        /// <summary>
        /// 搜索描述
        /// </summary>
        public bool SearchInDescriptions { get; set; }
        /// <summary>
        /// 类别标识
        /// </summary>
        public IList<int> CategoryIds { get; set; }
        /// <summary>
        /// 生产商标识
        /// </summary>
        public int ManufacturerId { get; set; }
        /// <summary>
        /// 供应商标识
        /// </summary>
        public int VendorId { get; set; }
    }
}
