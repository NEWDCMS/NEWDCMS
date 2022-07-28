namespace DCMS.Services.Installation
{
    public static partial class DCMSInstallationDefaults
    {
        /// <summary>
        /// 安装请求路径
        /// </summary>
        public static string InstallPath => "install";
        /// <summary>
        /// 必要数据快速安装脚本
        /// </summary>
        public static string RequiredDataPath => "~/App_Data/Install/Fast/create_required_data.sql";
        /// <summary>
        /// 示例数据快速安装脚本
        /// </summary>
        public static string SampleDataPath => "~/App_Data/Install/Fast/create_sample_data.sql";
    }
}
