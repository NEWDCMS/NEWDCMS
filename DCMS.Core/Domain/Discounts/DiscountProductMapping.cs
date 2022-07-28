using DCMS.Core.Domain.Products;

namespace DCMS.Core.Domain.Discounts
{

    public partial class DiscountProductMapping : BaseEntity
    {

        public int DiscountId { get; set; }

        public int ProductId { get; set; }

        public virtual Discount Discount { get; set; }

        public virtual Product Product { get; set; }
    }
}