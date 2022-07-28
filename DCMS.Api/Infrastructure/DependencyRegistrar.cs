using Autofac;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Web.Framework.Factories;
//using DCMS.Web.Factories;


namespace DCMS.Api.Infrastructure
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
        }

        public int Order => 2;
    }
}
