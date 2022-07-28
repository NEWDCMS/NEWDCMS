using DCMS.Core.Domain.Users;

namespace DCMS.Services.Authentication.External
{
    /// <summary>
    /// User auto registered by external authentication method event
    /// </summary>
    public class UserAutoRegisteredByExternalMethodEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="parameters">Parameters</param>
        public UserAutoRegisteredByExternalMethodEvent(User user, ExternalAuthenticationParameters parameters)
        {
            User = user;
            AuthenticationParameters = parameters;
        }

        /// <summary>
        /// Gets or sets user
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets or sets external authentication parameters
        /// </summary>
        public ExternalAuthenticationParameters AuthenticationParameters { get; }
    }
}
