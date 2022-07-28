using System;

namespace DCMS.Core.Domain.Census
{

    /// <summary>
    /// 销售商品
    /// </summary>
    public class SalesProduct : BaseEntity
    {
        /// <summary>
        /// 品牌(终端所销售的啤酒品牌)青岛啤酒、雪花啤酒、Budweiser百威、其他
        /// </summary>
        public string Brand { get; set; }
        /// <summary>
        ///  产品名称(扫码+手输)
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        ///  年销量（填写整数 年/箱）4大类品牌（青岛啤酒、雪花啤酒、Budweiser百威、其他）分为四档的容量，高档及以上，中档及中档高，中档低、其他
        /// </summary>
        public string AnnualSales { get; set; }

        /// <summary>
        ///  包装形式(瓶、听、桶)(所售啤酒产品的最小包装形式)
        /// </summary>
        public string PackingForm { get; set; }

        /// <summary>
        /// 产品供货商
        /// </summary>
        public string ProductProvider { get; set; }

        /// <summary>
        /// 渠道属性((一批、二批、其他)（销售啤酒产品给此终端的渠道商在其代理的啤酒产品渠道中的层级，包括一批、二批和其他（一批、二批之外））
        /// </summary>
        public string ChannelAttributes { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; }

        public DateTime UpdateDate { get; set; }


        #region  导航属性

        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }


        public int TraditionId { get; set; }
        public virtual Tradition Tradition { get; set; }

        #endregion

    }


}
