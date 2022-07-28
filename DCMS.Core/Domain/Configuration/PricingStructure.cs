using System;
namespace DCMS.Core.Domain.Configuration
{

    /// <summary>
    /// 表示价格体系配置
    /// </summary>
    public partial class PricingStructure : BaseEntity
    {


        /// <summary>
        /// 价格体系类别
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public int? CustomerId { get; set; } = 0;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public int? ChannelId { get; set; } = 0;

        /// <summary>
        /// 片区
        /// </summary>
        public string DistrictIds { get; set; }

        /// <summary>
        /// 片区
        /// </summary>
        public string DistrictName { get; set; }


        /// <summary>
        /// 等级
        /// </summary>

        public int? EndPointLevel { get; set; } = 0;

        /// <summary>
        /// 首选价格
        /// </summary>
        public string PreferredPrice { get; set; }

        /// <summary>
        /// 次选价格
        /// </summary>
        public string SecondaryPrice { get; set; }

        /// <summary>
        /// 末选价格
        /// </summary>
        public string FinalPrice { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 权重
        /// </summary>
        public int Order { get; set; }
    }
}
