using DCMS.Core.Data;


namespace DCMS.Core.Configuration
{
    /// <summary>
    /// 数据库配置选项(数据库字符串 and 数据库类型)
    /// </summary>
    public class DbConnectionOptions
    {
        /// <summary>
        /// 数据库字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; set; }
    }
}
