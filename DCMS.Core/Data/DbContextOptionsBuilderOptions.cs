using Microsoft.EntityFrameworkCore;
using System;

namespace DCMS.Core.Data
{
    /// <summary>
    /// DOBOptions 用于构建数据库上下文配置 
    /// </summary>
    public class DOBOptions
    {

        public DbContextOptionsBuilder Builder { get; }
        public string DbName { get; }
        public Type DbContextType { get; }

        /// <summary>
        /// 配置DbContextOptionsBuilder, dbName指定数据库名称, 为null时表示所有数据库,默认为null
        /// </summary>
        /// <param name="build"></param>
        /// <param name="dbName"></param>
        /// <param name="dbContextType">代表数据库上下文</param>
        public DOBOptions(DbContextOptionsBuilder build, string dbName = null, Type dbContextType = null)
        {
            Builder = build;
            DbName = dbName;
            DbContextType = dbContextType;
        }
    }
}
