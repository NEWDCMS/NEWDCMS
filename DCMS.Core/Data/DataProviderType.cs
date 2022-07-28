using System.Runtime.Serialization;

namespace DCMS.Core.Data
{
    /// <summary>
    /// 数据库提供枚举
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// 未知
        /// </summary>
        [EnumMember(Value = "")]
        Unknown = 0,
        /// <summary>
        /// SqlServer数据库类型
        /// </summary>
        [EnumMember(Value = "mssql")]
        SqlServer = 1,
        /// <summary>
        /// MySql数据库类型
        /// </summary>
        [EnumMember(Value = "mysql")]
        MySql = 2,
        /// <summary>
        /// Sqlite数据库类型
        /// </summary>
        [EnumMember(Value = "sqlite")]
        Sqlite = 3
    }
}