namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// ÓÃ»§µÇÂ¼Event
    /// </summary>
    public class UserLoggedinEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="user">User</param>
        public UserLoggedinEvent(User user)
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