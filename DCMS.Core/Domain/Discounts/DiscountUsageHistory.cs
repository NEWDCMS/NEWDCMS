using System;

namespace DCMS.Core.Domain.Discounts
{

    public partial class DiscountUsageHistory : BaseEntity
    {
        public int DiscountId { get; set; }
        public int BillId { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public virtual Discount Discount { get; set; }
    }
}
