using Autofac;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Web.Factories;
using DCMS.Web.Framework.Factories;

namespace DCMS.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// 在这里注册 Factories
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        /// <param name="config"></param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, DCMSConfig config, bool apiPlatform = false)
        {
            builder.RegisterType<AclSupportedModelFactory>().As<IAclSupportedModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<StoreMappingSupportedModelFactory>().As<IStoreMappingSupportedModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<HomeModelFactory>().As<IHomeModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TopicModelFactory>().As<ITopicModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CommonModelFactory>().As<ICommonModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UserModelFactory>().As<IUserModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SettingModelFactory>().As<ISettingModelFactory>().InstancePerLifetimeScope();
        }

        public int Order => 2;
    }
}
