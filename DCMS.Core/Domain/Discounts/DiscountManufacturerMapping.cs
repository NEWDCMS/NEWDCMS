using DCMS.Core.Domain.Products;

namespace DCMS.Core.Domain.Discounts
{

    public partial class DiscountManufacturerMapping : BaseEntity
    {
        public int DiscountId { get; set; }


        public int ManufacturerId { get; set; }

        public virtual Discount Discount { get; set; }

        public virtual Manufacturer Manufacturer { get; set; }
    }
}