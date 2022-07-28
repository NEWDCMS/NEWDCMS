using DCMS.Core.Domain.Users;

namespace DCMS.Services.Authentication
{
    /// <summary>
    /// Authentication service interface
    /// </summary>
    //public partial interface IAuthenticationService 
    //{
    //    /// <summary>
    //    /// Sign in
    //    /// </summary>
    //    /// <param name="user">User</param>
    //    /// <param name="isPersistent">Whether the authentication session is persisted across multiple requests</param>
    //    void SignIn(User user, bool isPersistent);

    //    /// <summary>
    //    /// Sign out
    //    /// </summary>
    //    void SignOut();

    //    /// <summary>
    //    /// Get authenticated user
    //    /// </summary>
    //    /// <returns>User</returns>
    //    User GetAuthenticatedUser();
    //}


    /// <summary>
    /// 身份认证服务接口
    /// </summary>
    public partial interface IAuthenticationService
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="user">创建持久化cookies</param>
        /// <param name="createPersistentCookie"></param>
        void SignIn(User user, bool isPersistent);
        //void SignIn(User user, bool isPersistent);

        /// <summary>
        /// 注销
        /// </summary>
        void SignOut();

        /// <summary>
        /// 获取身份用户
        /// </summary>
        /// <returns></returns>
        User GetAuthenticatedUser();
    }

}