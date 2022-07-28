using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Data
{

    /// <summary>
    /// 实现数据库提供者接口 v2.0
    /// </summary>
    public class DbProvider : IDbProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, IUnitOfWork> _works = new Dictionary<string, IUnitOfWork>();
        //private static ConcurrentDictionary<Type, Func<IServiceProvider, DbContextOptions, DbContext>> _expressionFactoryDict = new ConcurrentDictionary<Type, Func<IServiceProvider, DbContextOptions, DbContext>>();

        public DbProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUnitOfWork GetUnitOfWork(Type dbContextType, string dbName = null)
        {

            // 若替换之前的UnitOfWork则有可能造成数据库连接释放不及时
            // 若使用锁，可以确保UnitOrWork在当前Scope生命周期的唯一性，但会影响性能
            var key = string.Format("{0}${1}$", dbName, dbContextType.FullName);
            if (_works.ContainsKey(key))
            {
                return _works[key];
            }
            else
            {
                foreach (var k in _works.Keys)
                {
                    if (k.StartsWith(key))
                    {
                        return _works[k];
                    }
                }

                //DbContext dbContext;
                key += DateTime.Now.Ticks.ToString();

                //1. 获取DbConnections配置节点
                //var dbConnections1 = _serviceProvider.GetRequiredService<IOptions<AlasFxOptions>>().Value.DbConnections;
                var dbConnections = _serviceProvider.DCMSOptions().DbConnections;
                if (dbConnections == null || dbConnections.Count <= 0)
                {
                    throw new DCMSException("无法获取数据库配置");
                }
                //2. 获取指定数据库配置选项(数据库字符串 和 数据库类型)
                var dbConnectionOptions = dbName == null ? dbConnections.First().Value : dbConnections[dbName];


                //3. 获取DbContextOptionsBuilder 集合 这里面通过 dbName 和  DbContext 类型区分不同库的 OptionsBuilder
                //在注册服务时可以通过 AddDbBuilderOptions 添加 dbName 和 DbContext 创建如 ：new DOBOptions(dob) 
                //dob = new DbContextOptionsBuilder<AUTHObjectContext>(), "AUTH_RO", typeof(AUTHObjectContext))
                //一个 builderOption 就是一个 DOBOptions
                //当然此时，在这里 builderOption.Builder 还没有指定连接字符串 IsConfigured = false

                // var ss = _serviceProvider.GetServices<DOBOptions>().ToList();

                var builderOption = _serviceProvider.GetServices<DOBOptions>()
                      ?.Where(d => (d.DbName == null || d.DbName == dbName) && (d.DbContextType == null || d.DbContextType == dbContextType))
                      ?.OrderByDescending(d => d.DbName)
                      ?.OrderByDescending(d => d.DbContextType).FirstOrDefault();
                if (builderOption == null)
                {
                    throw new DCMSException("无法获取匹配的DbContextOptionsBuilder");
                }

                //4. 通过配置的数据库连接类型来取实现了 IDbContextOptionsBuilderUser 的 BuilderUser （代表一个数据库使用者）
                var dbUser = _serviceProvider.GetServices<IDbContextOptionsBuilderUser>()?.FirstOrDefault(u => u.Type == dbConnectionOptions.DatabaseType);
                if (dbUser == null)
                {
                    throw new DCMSException($"无法解析类型为“{dbConnectionOptions.DatabaseType}”的 {typeof(IDbContextOptionsBuilderUser).FullName} 实例");
                }

                //5. 通过调用数据库使用者的Use 方法，传入builderOption.Builder , ConnectionString 来构建一个 DbContextOptionsBuilder
                //这里返回的就是已经动态指定好连接字符串的DbContextOptionsBuilder   IsConfigured = true
                var dbContextOptions = dbUser.Use(builderOption.Builder, dbConnectionOptions.ConnectionString).Options;

                #region
                //if (_expressionFactoryDict.TryGetValue(dbContextType, out Func<IServiceProvider, DbContextOptions, DbContext> factory))
                //{
                //    dbContext = factory(_serviceProvider, dbContextOptions);
                //}
                //else
                //{
                //    // 使用Expression创建DbContext
                //    var constructorMethod = dbContextType.GetConstructors()
                //        .Where(c => c.IsPublic && !c.IsAbstract && !c.IsStatic)
                //        .OrderByDescending(c => c.GetParameters().Length)
                //        .FirstOrDefault();
                //    if (constructorMethod == null)
                //    {
                //        throw new AlasFxException("无法获取有效的上下文构造器");
                //    }

                //    var dbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>);
                //    var dbContextOptionsType = typeof(DbContextOptions);
                //    var dbContextOptionsGenericType = typeof(DbContextOptions<>);
                //    var serviceProviderType = typeof(IServiceProvider);
                //    var getServiceMethod = serviceProviderType.GetMethod("GetService");
                //    var lambdaParameterExpressions = new ParameterExpression[2];
                //    lambdaParameterExpressions[0] = (Expression.Parameter(serviceProviderType, "serviceProvider"));
                //    lambdaParameterExpressions[1] = (Expression.Parameter(dbContextOptionsType, "dbContextOptions"));
                //    var paramTypes = constructorMethod.GetParameters();
                //    var argumentExpressions = new Expression[paramTypes.Length];
                //    for (int i = 0; i < paramTypes.Length; i++)
                //    {
                //        var pType = paramTypes[i];
                //        if (pType.ParameterType == dbContextOptionsType ||
                //            (pType.ParameterType.IsGenericType && pType.ParameterType.GetGenericTypeDefinition() == dbContextOptionsGenericType))
                //        {
                //            argumentExpressions[i] = Expression.Convert(lambdaParameterExpressions[1], pType.ParameterType);
                //        }
                //        else if (pType.ParameterType == serviceProviderType)
                //        {
                //            argumentExpressions[i] = lambdaParameterExpressions[0];
                //        }
                //        else
                //        {
                //            argumentExpressions[i] = Expression.Call(lambdaParameterExpressions[0], getServiceMethod);
                //        }
                //    }

                //    factory = Expression
                //        .Lambda<Func<IServiceProvider, DbContextOptions, DbContext>>(
                //            Expression.Convert(Expression.New(constructorMethod, argumentExpressions), typeof(DbContext)), lambdaParameterExpressions.AsEnumerable())
                //        .Compile();
                //    _expressionFactoryDict.TryAdd(dbContextType, factory);

                //    dbContext = factory(_serviceProvider, dbContextOptions);
                //}
                #endregion

                //6. 返回工作单元
                var dopts = (DbContextOptions<DbContextBase>)Convert.ChangeType(dbContextOptions, typeof(DbContextOptions<DbContextBase>));
                var dbContext = new DbContextBase(dopts);
                var unitOfWorkFactory = _serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
                var unitOfWork = unitOfWorkFactory.GetUnitOfWork(_serviceProvider, dbContext);

                _works.Add(key, unitOfWork);

                return unitOfWork;
            }
        }

        public void Dispose()
        {
            if (_works != null && _works.Count > 0)
            {
                foreach (var unitOfWork in _works.Values)
                {
                    unitOfWork.Dispose();
                }

                _works.Clear();
            }
        }
    }
}
