
namespace DCMS.Core.Data
{
    /// <summary>
    /// 数据库配置存放地址默认值
    /// </summary>
    public static partial class DCMSDataSettingsDefaults
    {
        /// <summary>
        /// 数据库配置文件路径 （TXT v1.0 时的使用）~/App_Data/Settings.txt
        /// </summary>
        public static string ObsoleteFilePath => "~/App_Data/Settings.txt";

        /// <summary>
        /// 数据库配置文件路径 ~/App_Data/dataSettings.json
        /// </summary>
        public static string FilePath => "~/App_Data/dataSettings.json";

        /// <summary>
        /// 安装配置文件路径 ~/App_Data/Install/SetupSettings.json
        /// </summary>
        public static string SetupConfigFilePath => "~/App_Data/Install/SetupSettings.json";
    }
}