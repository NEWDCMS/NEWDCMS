using DCMS.Core.Infrastructure;

namespace DCMS.Core
{

    public interface IAutofacHelper
    {
        /// <summary>
        /// 一个接口多个实现并定义多个Name的情况下获取的实例，配置需定义名称如
        /// builder.RegisterType<RoleService>().Named<IRoleService>("roleSc");
        /// //通过Name单独注册AlipayService和WxpayService
        /// builder.RegisterType<RoleService>().Named<IRoleService>(typeof(AlipayService).Name);
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="name">配置时定义名称</param>
        /// <returns></returns>
        T GetByName<T>(string name);
    }

    public class AutofacHelper : IAutofacHelper
    {
        public T GetByName<T>(string name)
        {
            //return AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(name);
            //return EngineContext.Current.ContainerManager.GetByName<T>(name);
            return EngineContext.Current.GetByName<T>(name);
        }
    }

}
