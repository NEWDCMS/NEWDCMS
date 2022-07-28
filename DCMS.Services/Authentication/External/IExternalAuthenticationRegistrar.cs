using Microsoft.AspNetCore.Authentication;

namespace DCMS.Services.Authentication.External
{
    /// <summary>
    /// 用于注册（配置）外部身份验证服务（插件）的接口
    /// </summary>
    public interface IExternalAuthenticationRegistrar
    {
        void Configure(AuthenticationBuilder builder);
    }
}
