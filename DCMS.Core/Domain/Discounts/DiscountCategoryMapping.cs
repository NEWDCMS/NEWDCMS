using DCMS.Core.Domain.Products;

namespace DCMS.Core.Domain.Discounts
{

    public partial class DiscountCategoryMapping : BaseEntity
    {
        public int DiscountId { get; set; }
        public int CategoryId { get; set; }
        public virtual Discount Discount { get; set; }

        public virtual Category Category { get; set; }
    }
}