using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Infrastructure;
using DCMS.Data.Extensions;

namespace DCMS.Data
{
    /// <summary>
    /// MSSQL数据提供器
    /// </summary>
    public partial class SqlServerDataProvider: IDataProvider
    {
        #region Methods

        /// <summary>
        /// 初始数据库
        /// </summary>
        public virtual void InitializeDatabase()
        {
            var context = EngineContext.Current.Resolve<IDbContext>();

            //check some of table names to ensure that we have jsdcms 2.00+ installed
            var tableNamesToValidate = new List<string> { "Customer", "Discount", "Order", "Product", "ShoppingCartItem" };
            var existingTableNames = context
                .QueryFromSql<StringQueryType>("SELECT table_name AS Value FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'")
                .Select(stringValue => stringValue.Value).ToList();
            var createTables = !existingTableNames.Intersect(tableNamesToValidate, StringComparer.InvariantCultureIgnoreCase).Any();
            if (!createTables)
                return;

            var fileProvider = EngineContext.Current.Resolve<IDCMSFileProvider>();

            //创建表
            //EngineContext.Current.Resolve<IRelationalDatabaseCreator>().CreateTables();
            //(context as DbContext).Database.EnsureCreated();
            //context.ExecuteSqlScript(context.GenerateCreateScript());

            //创建索引
            //context.ExecuteSqlScriptFromFile(fileProvider.MapPath(DCMSDataDefaults.SqlServerIndexesFilePath));

            //创建存储过程
            //context.ExecuteSqlScriptFromFile(fileProvider.MapPath(DCMSDataDefaults.SqlServerStoredProceduresFilePath));
        }

 
        public virtual DbParameter GetParameter()
        {
            return new SqlParameter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        public virtual bool BackupSupported => true;

        /// <summary>
        /// Gets a maximum length of the data for HASHBYTES functions, returns 0 if HASHBYTES function is not supported
        /// </summary>
        public virtual int SupportedLengthOfBinaryHash => 8000; //for SQL Server 2008 and above HASHBYTES function has a limit of 8000 characters.

        #endregion
    }
}