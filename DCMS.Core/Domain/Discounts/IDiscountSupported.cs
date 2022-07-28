using System.Collections.Generic;

namespace DCMS.Core.Domain.Discounts
{

    public partial interface IDiscountSupported
    {
        IList<Discount> AppliedDiscounts { get; }
    }
}