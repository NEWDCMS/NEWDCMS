namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// User registered event
    /// </summary>
    public class UserRegisteredEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customer">customer</param>
        public UserRegisteredEvent(User user)
        {
            User = user;
        }

        /// <summary>
        /// Customer
        /// </summary>
        public User User
        {
            get;
        }
    }
}