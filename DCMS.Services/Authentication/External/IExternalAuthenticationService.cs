using Microsoft.AspNetCore.Mvc;
using DCMS.Core.Domain.Users;

namespace DCMS.Services.Authentication.External
{
    /// <summary>
    /// External authentication service
    /// </summary>
    public partial interface IExternalAuthenticationService
    {
        /// <summary>
        /// Authenticate user by passed parameters
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <returns>Result of an authentication</returns>
        IActionResult Authenticate(ExternalAuthenticationParameters parameters, string returnUrl = null);

        /// <summary>
        /// Associate external account with user
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="parameters">External authentication parameters</param>
        void AssociateExternalAccountWithUser(User user, ExternalAuthenticationParameters parameters);

        /// <summary>
        /// Get the particular user with specified parameters
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        /// <returns>User</returns>
        User GetUserByExternalAuthenticationParameters(ExternalAuthenticationParameters parameters);

        /// <summary>
        /// Remove the association
        /// </summary>
        /// <param name="parameters">External authentication parameters</param>
        void RemoveAssociation(ExternalAuthenticationParameters parameters);

        /// <summary>
        /// Delete the external authentication record
        /// </summary>
        /// <param name="externalAuthenticationRecord">External authentication record</param>
        void DeleteExternalAuthenticationRecord(ExternalAuthenticationRecord externalAuthenticationRecord);
    }
}