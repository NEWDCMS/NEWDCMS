using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 应用程序域类型查找器，用于从匹配可选程序集加载程序集
    /// </summary>
    public class AppDomainTypeFinder : ITypeFinder
    {
        #region Fields

        private bool _ignoreReflectionErrors = true;
        protected IDCMSFileProvider _fileProvider;

        #endregion

        #region Ctor

        public AppDomainTypeFinder(IDCMSFileProvider fileProvider = null)
        {
            _fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Iterates all assemblies in the AppDomain and if it's name matches the configured patterns add it to our list.
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies().OrderBy(s => s.FullName);
            foreach (var assembly in ass)
            {
                if (assembly.FullName.Contains("Views"))
                {
                    System.Diagnostics.Debug.WriteLine(assembly.FullName);
                }

                if (!Matches(assembly.FullName))
                {
                    continue;
                }

                if (addedAssemblyNames.Contains(assembly.FullName))
                {
                    continue;
                }

                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }

        /// <summary>
        /// Adds specifically configured assemblies.
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (var assemblyName in AssemblyNames)
            {
                var assembly = Assembly.Load(assemblyName);
                if (addedAssemblyNames.Contains(assembly.FullName))
                {
                    continue;
                }

                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }

        /// <summary>
        /// Check if a dll is one of the shipped dlls that we know don't need to be investigated.
        /// </summary>
        /// <param name="assemblyFullName">
        /// The name of the assembly to check.
        /// </param>
        /// <returns>
        /// True if the assembly should be loaded into DCMS.
        /// </returns>
        public virtual bool Matches(string assemblyFullName)
        {
            return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
                   && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
        }

        /// <summary>
        /// Check if a dll is one of the shipped dlls that we know don't need to be investigated.
        /// </summary>
        /// <param name="assemblyFullName">
        /// The assembly name to match.
        /// </param>
        /// <param name="pattern">
        /// The regular expression pattern to match against the assembly name.
        /// </param>
        /// <returns>
        /// True if the pattern matches the assembly name.
        /// </returns>
        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Makes sure matching assemblies in the supplied folder are loaded in the app domain.
        /// </summary>
        /// <param name="directoryPath">
        /// The physical path to a directory containing dlls to load in the app domain.
        /// </param>
        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            var loadedAssemblyNames = new List<string>();

            foreach (var a in GetAssemblies())
            {

                loadedAssemblyNames.Add(a.FullName);
            }

            if (!_fileProvider.DirectoryExists(directoryPath))
            {
                return;
            }

            foreach (var dllPath in _fileProvider.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllPath);

                    if (an.FullName.Contains("Views"))
                    {
                        System.Diagnostics.Debug.WriteLine(dllPath);
                    }


                    if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
                    {
                        App.Load(an);
                    }

                }
                catch (BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Does type implement generic?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="openGeneric"></param>
        /// <returns></returns>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find classes of type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
        /// <returns>Result</returns>
        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        /// <summary>
        /// Find classes of type
        /// </summary>
        /// <param name="assignTypeFrom">Assign type from</param>
        /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
        /// <returns>Result</returns>
        /// <returns></returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        /// <summary>
        /// Find classes of type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="assemblies">Assemblies</param>
        /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
        /// <returns>Result</returns>
        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }

        /// <summary>
        /// Find classes of type
        /// </summary>
        /// <param name="assignTypeFrom">Assign type from</param>
        /// <param name="assemblies">Assemblies</param>
        /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
        /// <returns>Result</returns>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch
                    {
                        //Entity Framework 6 doesn't allow getting types (throws an exception)
                        if (!_ignoreReflectionErrors)
                        {
                            throw;
                        }
                    }

                    if (types == null)
                    {
                        continue;
                    }

                    foreach (var t in types)
                    {
                        if (!assignTypeFrom.IsAssignableFrom(t) && (!assignTypeFrom.IsGenericTypeDefinition || !DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                        {
                            continue;
                        }

                        if (t.IsInterface)
                        {
                            continue;
                        }

                        if (onlyConcreteClasses)
                        {
                            if (t.IsClass && !t.IsAbstract)
                            {
                                result.Add(t);
                            }
                        }
                        else
                        {
                            result.Add(t);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                {
                    msg += e.Message + Environment.NewLine;
                }

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }

            return result;
        }

        /// <summary>
        /// Gets the assemblies related to the current implementation.
        /// </summary>
        /// <returns>A list of assemblies</returns>
        public virtual IList<Assembly> GetAssemblies()
        {
            var addedAssemblyNames = new List<string>();
            var assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies)
            {
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
            }

            AddConfiguredAssemblies(addedAssemblyNames, assemblies);

            return assemblies;
        }

        #endregion

        #region Properties

        /// <summary>The app domain to look for types in.</summary>
        public virtual AppDomain App => AppDomain.CurrentDomain;

        /// <summary>Gets or sets whether DCMS should iterate assemblies in the app domain when loading DCMS types. Loading patterns are applied when loading these assemblies.</summary>
        public bool LoadAppDomainAssemblies { get; set; } = true;

        /// <summary>Gets or sets assemblies loaded a startup in addition to those loaded in the AppDomain.</summary>
        public IList<string> AssemblyNames { get; set; } = new List<string>();

        /// <summary>Gets the pattern for dlls that we know don't need to be investigated.</summary>
        public string AssemblySkipLoadingPattern { get; set; } = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

        /// <summary>Gets or sets the pattern for dll that will be investigated. For ease of use this defaults to match all but to increase performance you might want to configure a pattern that includes assemblies and your own.</summary>
        /// <remarks>If you change this so that DCMS assemblies aren't investigated (e.g. by not including something like "^DCMS|..." you may break core functionality.</remarks>
        public string AssemblyRestrictToLoadingPattern { get; set; } = ".*";

        #endregion
    }



    // public class AppDomainTypeFinder : ITypeFinder
    // {

    //     private bool _ignoreReflectionErrors = true;
    //     protected IDCMSFileProvider _fileProvider;


    //     public AppDomainTypeFinder(IDCMSFileProvider fileProvider = null)
    //     {
    //         _fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
    //     }



    //     /// <summary>
    //     /// 迭代AppDomain中的所有程序集，如果它的名称与配置的模式匹配，则将其添加到我们的列表中
    //     /// </summary>
    //     /// <param name="addedAssemblyNames"></param>
    //     /// <param name="assemblies"></param>
    //     private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
    //     {
    //         var ass = AppDomain.CurrentDomain.GetAssemblies().OrderBy(s => s.FullName);
    //         foreach (var assembly in ass)
    //         {
    //             if (assembly.FullName.Contains("Views"))
    //             {
    //                 System.Diagnostics.Debug.WriteLine(assembly.FullName);
    //             }

    //             if (!Matches(assembly.FullName))
    //                 continue;

    //             if (addedAssemblyNames.Contains(assembly.FullName))
    //                 continue;

    //             assemblies.Add(assembly);
    //             addedAssemblyNames.Add(assembly.FullName);
    //         }
    //     }

    //     /// <summary>
    //     /// 添加特定配置的程序集
    //     /// </summary>
    //     /// <param name="addedAssemblyNames"></param>
    //     /// <param name="assemblies"></param>
    //     protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
    //     {
    //         foreach (var assemblyName in AssemblyNames)
    //         {
    //             var assembly = Assembly.Load(assemblyName);
    //             if (addedAssemblyNames.Contains(assembly.FullName))
    //                 continue;

    //             assemblies.Add(assembly);
    //             addedAssemblyNames.Add(assembly.FullName);
    //         }
    //     }

    //     /// <summary>
    //     /// 检查DLL是否是不需要
    //     /// </summary>
    //     /// <param name="assemblyFullName">
    //     /// 要检查的程序集的名称
    //     /// </param>
    //     /// <returns>
    //     /// 如果程序集应加载到DCMS中，则为True
    //     /// </returns>
    //     public virtual bool Matches(string assemblyFullName)
    //     {
    //         return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
    //                && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
    //     }

    //     /// <summary>
    //     /// 检查DLL是否是不需要
    //     /// </summary>
    //     /// <param name="assemblyFullName"></param>
    //     /// <param name="pattern"></param>
    //     /// <returns></returns>
    //     protected virtual bool Matches(string assemblyFullName, string pattern)
    //     {
    //         return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    //     }

    //     /// <summary>
    //     /// 确保已在应用程序域中加载所提供文件夹中的匹配程序集
    //     /// </summary>
    //     /// <param name="directoryPath">
    //     /// 包含要在应用程序域中加载的dll的目录的物理路径.
    //     /// </param>
    //     protected virtual void LoadMatchingAssemblies(string directoryPath)
    //     {
    //         var loadedAssemblyNames = new List<string>();

    //         foreach (var a in GetAssemblies())
    //         {
    //             /*
    //             "DCMS.Web, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //             "netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51"
    //             "DCMS.Core, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //             "DCMS.Web.Framework, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"

    //             "DCMS.Web.Views, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //             "DCMS.Data, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //             "DCMS.Services, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //             "Anonymously Hosted DynamicMethods Assembly, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
    //              */
    //             loadedAssemblyNames.Add(a.FullName);
    //         }

    //         if (!_fileProvider.DirectoryExists(directoryPath))
    //         {
    //             return;
    //         }

    //         foreach (var dllPath in _fileProvider.GetFiles(directoryPath, "*.dll"))
    //         {
    //             try
    //             {
    //                 var ass = AppDomain.CurrentDomain.GetAssemblies();
    //                 var an = AssemblyName.GetAssemblyName(dllPath);

    //                 System.Diagnostics.Debug.WriteLine(dllPath);
    //                 //"E:\\Git\\DCMS.Studio.Core\\DCMS.Core\\DCMS.Web\\bin\\Debug\\netcoreapp3.1\\DCMS.Web.Views.dll"
    //                 //"DCMS.Web.Views, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //                 //"DCMS.Web.Framework, Version=3.0.0.1, Culture=neutral, PublicKeyToken=null"
    //                 //"E:\\Git\\DCMS.Studio.Core\\DCMS.Core\\DCMS.Web\\bin\\Debug\\netcoreapp3.1\\DCMS.Web.Views.dll"
    //                 //DCMS.Web.Views.dll
    //                 if (an.FullName.Contains("Framework"))
    //                 {
    //                     System.Diagnostics.Debug.WriteLine(an.FullName);
    //                 }

    //                 //E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Web\bin\Debug\netcoreapp3.1

    //                 if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
    //                 {
    //                     App.Load(an);
    //                 }
    //             }
    //             catch (BadImageFormatException ex)
    //             {
    //                 Trace.TraceError(ex.ToString());
    //             }
    //         }
    //     }
    //     /*
    //         at System.Reflection.RuntimeAssembly.nLoad(AssemblyName fileName, String codeBase, RuntimeAssembly assemblyContext, StackCrawlMark& stackMark, Boolean throwOnFileNotFound, AssemblyLoadContext assemblyLoadContext)
    //at System.Reflection.RuntimeAssembly.InternalLoadAssemblyName(AssemblyName assemblyRef, StackCrawlMark& stackMark, AssemblyLoadContext assemblyLoadContext)
    //at System.Reflection.Assembly.Load(AssemblyName assemblyRef, StackCrawlMark& stackMark, AssemblyLoadContext assemblyLoadContext)
    //at System.Reflection.Assembly.Load(AssemblyName assemblyRef)
    //at System.AppDomain.Load(AssemblyName assemblyRef)
    //at DCMS.Core.Infrastructure.AppDomainTypeFinder.LoadMatchingAssemblies(String directoryPath) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Core\Infrastructure\AppDomainTypeFinder.cs:line 138
    //at DCMS.Core.Infrastructure.WebAppTypeFinder.GetAssemblies() in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Core\Infrastructure\WebAppTypeFinder.cs:line 57
    //at DCMS.Core.Infrastructure.AppDomainTypeFinder.FindClassesOfType(Type assignTypeFrom, Boolean onlyConcreteClasses) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Core\Infrastructure\AppDomainTypeFinder.cs:line 190
    //at DCMS.Core.Infrastructure.AppDomainTypeFinder.FindClassesOfType[T](Boolean onlyConcreteClasses) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Core\Infrastructure\AppDomainTypeFinder.cs:line 184
    //at DCMS.Core.Infrastructure.DCMSEngine.ConfigureServices(IServiceCollection services, IConfiguration configuration, DCMSConfig nopConfig) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Core\Infrastructure\DCMSEngine.cs:line 262
    //at DCMS.Web.Framework.Infrastructure.Extensions.ServiceCollectionExtensions.ConfigureApplicationServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostingEnvironment, Boolean apiPlatform, Boolean isManagePlatform) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Web.Framework\Infrastructure\Extensions\ServiceCollectionExtensions.cs:line 116
    //at DCMS.Web.Startup.ConfigureServices(IServiceCollection services) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Web\Startup.cs:line 81
    //at System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions)
    //at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
    //at Microsoft.AspNetCore.Hosting.ConfigureServicesBuilder.InvokeCore(Object instance, IServiceCollection services)
    //at Microsoft.AspNetCore.Hosting.ConfigureServicesBuilder.<>c__DisplayClass9_0.<Invoke>g__Startup|0(IServiceCollection serviceCollection)
    //at Microsoft.AspNetCore.Hosting.ConfigureServicesBuilder.Invoke(Object instance, IServiceCollection services)
    //at Microsoft.AspNetCore.Hosting.ConfigureServicesBuilder.<>c__DisplayClass8_0.<Build>b__0(IServiceCollection services)
    //at Microsoft.AspNetCore.Hosting.GenericWebHostBuilder.UseStartup(Type startupType, HostBuilderContext context, IServiceCollection services)
    //at Microsoft.AspNetCore.Hosting.GenericWebHostBuilder.<>c__DisplayClass12_0.<UseStartup>b__0(HostBuilderContext context, IServiceCollection services)
    //at Microsoft.Extensions.Hosting.HostBuilder.CreateServiceProvider()
    //at Microsoft.Extensions.Hosting.HostBuilder.Build()
    //at DCMS.Web.Program.Main(String[] args) in E:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Web\Program.cs:line 27
    //          */

    //     /// <summary>
    //     /// 类型是否实现泛型
    //     /// </summary>
    //     /// <param name="type"></param>
    //     /// <param name="openGeneric"></param>
    //     /// <returns></returns>
    //     protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
    //     {
    //         try
    //         {
    //             var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
    //             foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
    //             {
    //                 if (!implementedInterface.IsGenericType)
    //                     continue;

    //                 if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
    //                     return true;
    //             }

    //             return false;
    //         }
    //         catch
    //         {
    //             return false;
    //         }
    //     }



    //     #region Methods


    //     public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
    //     {
    //         return FindClassesOfType(typeof(T), onlyConcreteClasses);
    //     }


    //     public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
    //     {
    //         return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
    //     }


    //     public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
    //     {
    //         return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
    //     }

    //     public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
    //     {
    //         var result = new List<Type>();
    //         try
    //         {
    //             foreach (var a in assemblies)
    //             {
    //                 Type[] types = null;
    //                 try
    //                 {
    //                     types = a.GetTypes();
    //                 }
    //                 catch
    //                 {
    //                     //Entity Framework 6 doesn't allow getting types (throws an exception)
    //                     if (!_ignoreReflectionErrors)
    //                     {
    //                         throw;
    //                     }
    //                 }

    //                 if (types == null)
    //                     continue;

    //                 foreach (var t in types)
    //                 {
    //                     if (!assignTypeFrom.IsAssignableFrom(t) && (!assignTypeFrom.IsGenericTypeDefinition || !DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
    //                         continue;

    //                     if (t.IsInterface)
    //                         continue;

    //                     if (onlyConcreteClasses)
    //                     {
    //                         if (t.IsClass && !t.IsAbstract)
    //                         {
    //                             result.Add(t);
    //                         }
    //                     }
    //                     else
    //                     {
    //                         result.Add(t);
    //                     }
    //                 }
    //             }
    //         }
    //         catch (ReflectionTypeLoadException ex)
    //         {
    //             var msg = string.Empty;
    //             foreach (var e in ex.LoaderExceptions)
    //                 msg += e.Message + Environment.NewLine;

    //             var fail = new Exception(msg, ex);
    //             Debug.WriteLine(fail.Message, fail);

    //             throw fail;
    //         }

    //         return result;
    //     }


    //     public virtual IList<Assembly> GetAssemblies()
    //     {
    //         var addedAssemblyNames = new List<string>();
    //         var assemblies = new List<Assembly>();

    //         if (LoadAppDomainAssemblies)
    //             AddAssembliesInAppDomain(addedAssemblyNames, assemblies);

    //         AddConfiguredAssemblies(addedAssemblyNames, assemblies);

    //         return assemblies;
    //     }

    //     #endregion

    //     #region Properties

    //     /// <summary>要在其中查找类型的应用程序域.</summary>
    //     public virtual AppDomain App => AppDomain.CurrentDomain;

    //     /// <summary>获取或设置加载DCMS类型时DCMS是否应在应用程序域中迭代程序集。加载这些程序集时应用加载模式.</summary>
    //     public bool LoadAppDomainAssemblies { get; set; } = true;

    //     /// <summary>获取或设置除了在AppDomain中加载的程序集之外加载的启动程序集.</summary>
    //     public IList<string> AssemblyNames { get; set; } = new List<string>();

    //     /// <summary>
    //     /// 匹配获取不需要检查的dll
    //     /// </summary>
    //     public string AssemblySkipLoadingPattern { get; set; } = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

    //     /// <summary>Gets or sets the pattern for dll that will be investigated. For ease of use this defaults to match all but to increase performance you might want to configure a pattern that includes assemblies and your own.</summary>
    //     /// <remarks>If you change this so that DCMS assemblies aren't investigated (e.g. by not including something like "^DCMS|..." you may break core functionality.</remarks>
    //     public string AssemblyRestrictToLoadingPattern { get; set; } = ".*";

    //     #endregion
    // }
}