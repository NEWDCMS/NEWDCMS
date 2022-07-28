
namespace DCMS.Data
{
    /// <summary>
    /// 用于表示安装初始数据
    /// </summary>
    public static partial class DCMSDataDefaults
    {
        public static string SqlServerIndexesFilePath => "~/App_Data/Install/SqlServer.Indexes.sql";
        public static string SqlServerStoredProceduresFilePath => "~/App_Data/Install/SqlServer.StoredProcedures.sql";
    }
}