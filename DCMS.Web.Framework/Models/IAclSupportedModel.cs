using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DCMS.Web.Framework.Models
{
    /// <summary>
    /// Represents a model which supports access control list (ACL)
    /// </summary>
    public partial interface IAclSupportedModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets identifiers of the selected user roles
        /// </summary>
        IList<int> SelectedUserRoleIds { get; set; }

        /// <summary>
        /// Gets or sets items for the all available user roles
        /// </summary>
        IList<SelectListItem> AvailableUserRoles { get; set; }

        #endregion
    }
}