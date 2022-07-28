using Autofac;
using DCMS.Core.Configuration;

namespace DCMS.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// 依赖注册接口（v2实现）
    /// </summary>
    public interface IDependencyRegistrar
    {
        /// <summary>
        /// 服务注册接口
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        void Register(ContainerBuilder builder, ITypeFinder typeFinder, DCMSConfig config, bool apiPlatform = false);

        //void Register(ContainerBuilder builder, List<Type> listType);

        /// <summary>
        /// 获取此依赖项注册器实现的顺序
        /// </summary>
        int Order { get; }
    }
}
