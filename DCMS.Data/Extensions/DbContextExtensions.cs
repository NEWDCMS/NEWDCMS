
using DCMS.Core;
using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DCMS.Data.Extensions
{
    /// <summary>
    /// 数据库上下文扩展方法 v2.0
    /// </summary>
    public static class DbContextExtensions
    {


        /// <summary>
        /// 数据库名称
        /// </summary>
        private static string databaseName;
        /// <summary>
        /// 
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> tableNames = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, IEnumerable<(string, int?)>> columnsMaxLength = new ConcurrentDictionary<string, IEnumerable<(string, int?)>>();
        private static readonly ConcurrentDictionary<string, IEnumerable<(string, decimal?)>> decimalColumnsMaxValue = new ConcurrentDictionary<string, IEnumerable<(string, decimal?)>>();

        /// <summary>
        /// 使用委托加载实体的副本
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <param name="entity">Entity</param>
        /// <param name="getValuesFunction">Function to get the values of the tracked entity</param>
        /// <returns>Copy of the passed entity</returns>
        private static TEntity LoadEntityCopy<TEntity>(DbContext context, TEntity entity, Func<EntityEntry<TEntity>, PropertyValues> getValuesFunction)
            where TEntity : BaseEntity
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //try to get the EF database context
            if (!(context is DbContext dbContext))
            {
                throw new InvalidOperationException("Context does not support operation");
            }

            //try to get the entity tracking object
            var entityEntry = dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(entry => entry.Entity == entity);
            if (entityEntry == null)
            {
                return null;
            }

            //get a copy of the entity
            var entityCopy = getValuesFunction(entityEntry)?.ToObject() as TEntity;

            return entityCopy;
        }


        private static IList<string> GetCommandsFromScript(string sql)
        {
            var commands = new List<string>();

            sql = Regex.Replace(sql, @"\\\r?\n", string.Empty);
            var batches = Regex.Split(sql, @"^\s*(GO[ \t]+[0-9]+|GO)(?:\s+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            for (var i = 0; i < batches.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(batches[i]) || batches[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var count = 1;
                if (i != batches.Length - 1 && batches[i + 1].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(batches[i + 1], "([0-9]+)");
                    if (match.Success)
                    {
                        count = int.Parse(match.Value);
                    }
                }

                var builder = new StringBuilder();
                for (var j = 0; j < count; j++)
                {
                    builder.Append(batches[i]);
                    if (i == batches.Length - 1)
                    {
                        builder.AppendLine();
                    }
                }

                commands.Add(builder.ToString());
            }

            return commands;
        }


        public static TEntity LoadOriginalCopy<TEntity>(this DbContext context, TEntity entity) where TEntity : BaseEntity
        {
            return LoadEntityCopy(context, entity, entityEntry => entityEntry.OriginalValues);
        }


        public static TEntity LoadDatabaseCopy<TEntity>(this DbContext context, TEntity entity) where TEntity : BaseEntity
        {
            return LoadEntityCopy(context, entity, entityEntry => entityEntry.GetDatabaseValues());
        }



        public static string GetTableName<TEntity>(this DbContext context) where TEntity : BaseEntity
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //try to get the EF database context
            if (!(context is DbContext dbContext))
            {
                throw new InvalidOperationException("Context does not support operation");
            }

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!tableNames.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindRuntimeEntityType(typeof(TEntity));

                //get the name of the table to which the entity type is mapped
                //.Relational().TableName
                tableNames.TryAdd(entityTypeFullName, entityType.Name);
            }

            tableNames.TryGetValue(entityTypeFullName, out var tableName);

            return tableName;
        }


        public static IEnumerable<(string Name, int? MaxLength)> GetColumnsMaxLength<TEntity>(this DbContext context) where TEntity : BaseEntity
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //try to get the EF database context
            if (!(context is DbContext dbContext))
            {
                throw new InvalidOperationException("Context does not support operation");
            }

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!columnsMaxLength.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

                //get property name - max length pairs
                columnsMaxLength.TryAdd(entityTypeFullName,
                    entityType.GetProperties().Select(property => (property.Name, property.GetMaxLength())));
            }

            columnsMaxLength.TryGetValue(entityTypeFullName, out var result);

            return result;
        }


        public static IEnumerable<(string Name, decimal? MaxValue)> GetDecimalColumnsMaxValue<TEntity>(this DbContext context)
            where TEntity : BaseEntity
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //try to get the EF database context
            if (!(context is DbContext dbContext))
            {
                throw new InvalidOperationException("Context does not support operation");
            }

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!decimalColumnsMaxValue.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

                //get entity decimal properties
                var properties = entityType.GetProperties().Where(property => property.ClrType == typeof(decimal));

                //return property name - max decimal value pairs
                decimalColumnsMaxValue.TryAdd(entityTypeFullName, properties.Select(property =>
                {
                    var mapping = new RelationalTypeMappingInfo(property);
                    if (!mapping.Precision.HasValue || !mapping.Scale.HasValue)
                    {
                        return (property.Name, null);
                    }

                    return (property.Name, new decimal?((decimal)Math.Pow(10, mapping.Precision.Value - mapping.Scale.Value)));
                }));
            }

            decimalColumnsMaxValue.TryGetValue(entityTypeFullName, out var result);

            return result;
        }

        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string DbName(this DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //try to get the EF database context
            if (!(context is DbContext dbContext))
            {
                throw new InvalidOperationException("Context does not support operation");
            }

            if (!string.IsNullOrEmpty(databaseName))
            {
                return databaseName;
            }

            //get database connection
            var dbConnection = dbContext.Database.GetDbConnection();

            //return the database name
            databaseName = dbConnection.Database;

            return databaseName;
        }


        /// <summary>
        /// 更新上下文中指定实体的状态
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        public static void Update<TEntity>(this DbContext context, params TEntity[] entities)
            where TEntity : class, IEntity
        {
            //Check.NotNull(context, nameof(context));
            //Check.NotNull(entities, nameof(entities));
            context.Set<TEntity>().BulkUpdate(entities);
            //context.Set<TEntity>().UpdateRange(entities);
        }

        /// <summary>
        /// 转换为DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DbContext AsDbContext(this DbContext context)
        {
            return context as DbContext;
        }

      

        /// <summary>
        /// 执行指定的Sql语句
        /// </summary>
        public static int ExecuteSqlCommand(this DbContext dbContext, string sql, params object[] parameters)
        {
            if (!(dbContext is DbContext context))
            {
                throw new DCMSException($"参数dbContext类型为“{dbContext.GetType()}”，不能转换为 DbContext");
            }
            // 3.0 中 ExecuteSqlRawAsync
            return context.Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <summary>
        /// 异步执行指定的Sql语句
        /// </summary>
        public static Task<int> ExecuteSqlCommandAsync(this DbContext dbContext, string sql, params object[] parameters)
        {
            if (!(dbContext is DbContext context))
            {
                throw new DCMSException($"参数dbContext类型为“{dbContext.GetType()}”，不能转换为 DbContext");
            }
            // 3.0 中 ExecuteSqlRawAsync
            return context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

    }

}