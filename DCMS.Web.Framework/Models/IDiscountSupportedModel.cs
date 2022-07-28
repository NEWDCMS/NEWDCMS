using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DCMS.Web.Framework.Models
{
    /// <summary>
    /// Represents a discount supported model
    /// </summary>
    public partial interface IDiscountSupportedModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets identifiers of the selected discounts
        /// </summary>
        IList<int> SelectedDiscountIds { get; set; }

        /// <summary>
        /// Gets or sets items for the all available discounts
        /// </summary>
        IList<SelectListItem> AvailableDiscounts { get; set; }

        #endregion
    }
}