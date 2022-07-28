using System;
using System.Collections.Generic;
using System.Reflection;

namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 实现此接口的类向DCMS引擎中的各种服务提供有关类型的信息 3.1
    /// </summary>
    public interface ITypeFinder
    {
        /// <summary>
        /// 查找类的类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="onlyConcreteClasses">指示是否只查找具体的类</param>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

        /// <summary>
        /// 查找类的类型
        /// </summary>
        /// <param name="assignTypeFrom">指定类型自</param>
        /// <param name="onlyConcreteClasses">指示是否只查找具体的类</param>
        /// <returns></returns>
        /// <returns></returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

        /// <summary>
        /// 查找类的类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assemblies">程序集</param>
        /// <param name="onlyConcreteClasses">指示是否只查找具体的类</param>
        /// <returns>Result</returns>
        IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

        /// <summary>
        /// 查找类的类型
        /// </summary>
        /// <param name="assignTypeFrom">指定类型自</param>
        /// <param name="assemblies">程序集</param>
        /// <param name="onlyConcreteClasses">指示是否只查找具体的类</param>
        /// <returns>Result</returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

        /// <summary>
        /// 获取与当前实现相关的程序集
        /// </summary>
        /// <returns></returns>
        IList<Assembly> GetAssemblies();
    }
}
