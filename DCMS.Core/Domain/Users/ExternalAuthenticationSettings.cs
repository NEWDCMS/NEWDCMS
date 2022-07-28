using DCMS.Core.Configuration;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// External authentication settings
    /// </summary>
    public class ExternalAuthenticationSettings : ISettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalAuthenticationSettings()
        {
            ActiveAuthenticationMethodSystemNames = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether email validation is required.
        /// </summary>
        public bool RequireEmailValidation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is allowed to remove external authentication associations
        /// </summary>
        public bool AllowUsersToRemoveAssociations { get; set; }

        /// <summary>
        /// Gets or sets system names of active payment methods
        /// </summary>
        public List<string> ActiveAuthenticationMethodSystemNames { get; set; }
    }
}