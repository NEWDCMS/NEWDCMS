using DCMS.Core.Domain.Users;


namespace DCMS.Core
{

    /// <summary>
    /// 用于表示工作上下文,获取用户在整个生命周期中的全局初始和动态数据
    /// </summary>
    public interface IWorkContext
    {

        User CurrentUser { get; set; }
        User OriginalUserIfImpersonated { get; }
        bool IsAdmin { get; set; }

        //    /// <summary>
        //    /// 用户权限码
        //    /// </summary>
        //    List<string> AuthorizeCodes { get; }
    }
}
