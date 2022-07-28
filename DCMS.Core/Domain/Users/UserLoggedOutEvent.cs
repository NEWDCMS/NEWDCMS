namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// ÓÃ»§µÇ³öEvent
    /// </summary>
    public class UserLoggedOutEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="user">User</param>
        public UserLoggedOutEvent(User user)
        {
            User = user;
        }

        /// <summary>
        /// Get or set the user
        /// </summary>
        public User User { get; }
    }
}