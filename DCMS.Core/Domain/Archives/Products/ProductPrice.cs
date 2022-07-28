using Newtonsoft.Json;
namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 商品价目实体
    /// </summary>
    public class ProductPrice : BaseEntity
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; } = 0;
        /// <summary>
        /// 单位Id
        /// </summary>
        public int UnitId { get; set; } = 0;
        /// <summary>
        /// 批发价
        /// </summary>
        public decimal? TradePrice { get; set; } = 0;
        /// <summary>
        /// 零售价
        /// </summary>
        public decimal? RetailPrice { get; set; } = 0;
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal? FloorPrice { get; set; } = 0;
        /// <summary>
        /// 进价
        /// </summary>
        public decimal? PurchasePrice { get; set; } = 0;
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; } = 0;
        /// <summary>
        /// 特价1
        /// </summary>
        public decimal? SALE1 { get; set; } = 0;
        /// <summary>
        /// 特价2
        /// </summary>
        public decimal? SALE2 { get; set; } = 0;
        /// <summary>
        /// 特价3
        /// </summary>
        public decimal? SALE3 { get; set; } = 0;


        /// <summary>
        /// 商品
        /// </summary>
        [JsonIgnore]
        public virtual Product Product { get; set; }
    }
}
