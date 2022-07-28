using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Infrastructure;
using DCMS.Data;
using DCMS.Data.Extensions;

namespace DCMS.Services.Common
{
    /// <summary>
    ///  数据库维修服务
    /// </summary>
    public partial class MaintenanceService : IMaintenanceService
    {
        #region Fields

        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly IDCMSFileProvider _fileProvider;

        #endregion

        #region Ctor

        public MaintenanceService(IDataProvider dataProvider,
            IDbContext dbContext,
            IDCMSFileProvider fileProvider)
        {
            _dataProvider = dataProvider;
            _dbContext = dbContext;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 获取备份的目录路径
        /// </summary>
        /// <param name="ensureFolderCreated"></param>
        /// <returns></returns>
        protected virtual string GetBackupDirectoryPath(bool ensureFolderCreated = true)
        {
            var path = _fileProvider.GetAbsolutePath(DCMSCommonDefaults.DbBackupsPath);
            if (ensureFolderCreated)
                _fileProvider.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 检查是否支持备份
        /// </summary>
        protected virtual void CheckBackupSupported()
        {
            if (!_dataProvider.BackupSupported)
                throw new DataException("This database does not support backup");
        }

        #endregion

        #region Methods

        /// <summary>
        /// 获取当前表标识值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual int? GetTableIdent<T>() where T : BaseEntity
        {
            var tableName = _dbContext.GetTableName<T>();
            var result = _dbContext
                .QueryFromSql<DecimalQueryType>($"SELECT IDENT_CURRENT('[{tableName}]') as Value")
                .Select(decimalValue => decimalValue.Value).FirstOrDefault();
            return result.HasValue ? Convert.ToInt32(result) : 1;
        }

        /// <summary>
        /// 设置表标识
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ident"></param>
        public virtual void SetTableIdent<T>(int ident) where T : BaseEntity
        {
            var currentIdent = GetTableIdent<T>();
            if (!currentIdent.HasValue || ident <= currentIdent.Value) 
                return;

            var tableName = _dbContext.GetTableName<T>();
            //_dbContext.ExecuteSqlCommand($"DBCC CHECKIDENT([{tableName}], RESEED, {ident})");
        }

        /// <summary>
        /// 获取备份文件
        /// </summary>
        /// <returns></returns>
        public virtual IList<string> GetAllBackupFiles()
        {
            var path = GetBackupDirectoryPath();

            if (!_fileProvider.DirectoryExists(path))
            {
                throw new DCMSException("Backup directory not exists");
            }

            return _fileProvider.GetFiles(path, $"*.{DCMSCommonDefaults.DbBackupFileExtension}")
                .OrderByDescending(p => _fileProvider.GetLastWriteTime(p)).ToList();
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public virtual void BackupDatabase()
        {
            CheckBackupSupported();
            var fileName = _fileProvider.Combine(GetBackupDirectoryPath(), $"database_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{CommonHelper.GenerateRandomDigitCode(10)}.{DCMSCommonDefaults.DbBackupFileExtension}");

            var commandText = $"BACKUP DATABASE [{_dbContext.DbName()}] TO DISK = '{fileName}' WITH FORMAT";

            //_dbContext.ExecuteSqlCommand(commandText, true);
        }

        /// <summary>
        /// 从备份还原数据库
        /// </summary>
        /// <param name="backupFileName"></param>
        public virtual void RestoreDatabase(string backupFileName)
        {
            CheckBackupSupported();

            var conn = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings(fileProvider: _fileProvider).DataConnectionString)
            {
                InitialCatalog = "master"
            };

            //this method (backups) works only with SQL Server database
            using (var sqlConnectiononn = new SqlConnection(conn.ToString()))
            {
                var commandText = string.Format(
                    "DECLARE @ErrorMessage NVARCHAR(4000)\n" +
                    "ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE\n" +
                    "BEGIN TRY\n" +
                        "RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH REPLACE\n" +
                    "END TRY\n" +
                    "BEGIN CATCH\n" +
                        "SET @ErrorMessage = ERROR_MESSAGE()\n" +
                    "END CATCH\n" +
                    "ALTER DATABASE [{0}] SET MULTI_USER WITH ROLLBACK IMMEDIATE\n" +
                    "IF (@ErrorMessage is not NULL)\n" +
                    "BEGIN\n" +
                        "RAISERROR (@ErrorMessage, 16, 1)\n" +
                    "END",
                    _dbContext.DbName(),
                    backupFileName);

                DbCommand dbCommand = new SqlCommand(commandText, sqlConnectiononn);
                if (sqlConnectiononn.State != ConnectionState.Open)
                    sqlConnectiononn.Open();
                dbCommand.ExecuteNonQuery();
            }

            //clear all pools
            SqlConnection.ClearAllPools();
        }

        /// <summary>
        /// 返回备份文件的路径
        /// </summary>
        /// <param name="backupFileName"></param>
        /// <returns></returns>
        public virtual string GetBackupPath(string backupFileName)
        {
            return _fileProvider.Combine(GetBackupDirectoryPath(), backupFileName);
        }

        /// <summary>
        /// 重新创建数据库表索引
        /// </summary>
        public virtual void ReIndexTables()
        {
            var commandText = $@"
                DECLARE @TableName sysname 
                DECLARE cur_reindex CURSOR FOR
                SELECT table_name
                FROM [{_dbContext.DbName()}].information_schema.tables
                WHERE table_type = 'base table'
                OPEN cur_reindex
                FETCH NEXT FROM cur_reindex INTO @TableName
                WHILE @@FETCH_STATUS = 0
                    BEGIN
		                exec('ALTER INDEX ALL ON [' + @TableName + '] REBUILD')
                        FETCH NEXT FROM cur_reindex INTO @TableName
                    END
                CLOSE cur_reindex
                DEALLOCATE cur_reindex";

           // _dbContext.ExecuteSqlCommand(commandText, true);
        }

        #endregion
    }
}