using DCMS.Core.Infrastructure;
using DCMS.Core.Redis;
using Newtonsoft.Json;
using System;
using System.Text;

namespace DCMS.Core.Data
{
    /// <summary>
    /// 数据配置提供管理器(用于数据库配置/组合（经销商）快速初始化安装)
    /// </summary>
    public partial class DataSettingsManager
    {
        //IRedisConnectionWrapper
        //var redisConnectionWrapper = EngineContext.Current.Resolve<IRedisConnectionWrapper>();

        /// <summary>
        /// 是否已安装数据库
        /// </summary>
        public static bool RedisIsInstalled
        {
            get
            {
                var redisConnectionWrapper = EngineContext.Current.Resolve<IRedisConnectionWrapper>();
                if (string.IsNullOrEmpty(redisConnectionWrapper.GetConnectionString()))
                {
                    return redisConnectionWrapper.Connected();
                }
                else
                {
                    return false;
                }
            }
        }

        #region 数据库相关

        //DataSettingsManager.SaveSettings(new DataSettings
        //{
        //    DataProvider = DataProviderType.SqlServer,
        //    DataConnectionString = "Data Source=192.168.50.179; Database=DCMS; User ID=sa; Password=dcms.1;"
        //}, fileProvider);

        /*
        /// <summary>
        /// 载入数据库配置
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="reloadSettings"></param>
        /// <param name="fileProvider"></param>
        /// <returns></returns>
        public static DataSettings LoadSettings(string filePath = null, bool reloadSettings = false, IDCMSFileProvider fileProvider = null)
        {
            if (!reloadSettings && Singleton<DataSettings>.Instance != null)
                return Singleton<DataSettings>.Instance;

            fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
            filePath = filePath ?? fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);


            //配置是否存在
            if (!fileProvider.FileExists(filePath))
            {
                filePath = fileProvider.MapPath(DCMSDataSettingsDefaults.ObsoleteFilePath);
                if (!fileProvider.FileExists(filePath))
                    return new DataSettings();

                //旧文件（）
                var dataSettings = new DataSettings();
                using (var reader = new StringReader(fileProvider.ReadAllText(filePath, Encoding.UTF8)))
                {
                    string settingsLine;
                    while ((settingsLine = reader.ReadLine()) != null)
                    {
                        var separatorIndex = settingsLine.IndexOf(':');
                        if (separatorIndex == -1)
                            continue;

                        var key = settingsLine.Substring(0, separatorIndex).Trim();
                        var value = settingsLine.Substring(separatorIndex + 1).Trim();

                        switch (key)
                        {
                            case "DataProvider":
                                dataSettings.DataProvider = Enum.TryParse(value, true, out DataProviderType providerType) ? providerType : DataProviderType.Unknown;
                                continue;
                            case "DataConnectionString":
                                dataSettings.DataConnectionString = value;
                                continue;
                            default:
                                dataSettings.RawDataSettings.Add(key, value);
                                continue;
                        }
                    }
                }

                //重写保存
                SaveSettings(dataSettings, fileProvider);

                //删除一次旧的配置
                fileProvider.DeleteFile(filePath);

                Singleton<DataSettings>.Instance = dataSettings;
                return Singleton<DataSettings>.Instance;
            }

            var text = fileProvider.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrEmpty(text))
                return new DataSettings();

            //从JSON文件获取数据设置
            Singleton<DataSettings>.Instance = JsonConvert.DeserializeObject<DataSettings>(text);

            return Singleton<DataSettings>.Instance;
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        /// <param name="settings">Data settings</param>
        /// <param name="fileProvider">File provider</param>
        public static void SaveSettings(DataSettings settings, IDCMSFileProvider fileProvider = null)
        {
            Singleton<DataSettings>.Instance = settings ?? throw new ArgumentNullException(nameof(settings));

            fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
            //数据库保存地址
            var filePath = fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);

            //创建文件（如果不存在）
            fileProvider.CreateFile(filePath);

            //将数据设置保存到文件
            var text = JsonConvert.SerializeObject(Singleton<DataSettings>.Instance, Formatting.Indented);
            fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        }
        */

        ///// <summary>
        ///// 载入数据库配置
        ///// </summary>
        ///// <param name="filePath"></param>
        ///// <param name="reloadSettings"></param>
        ///// <param name="fileProvider"></param>
        ///// <returns></returns>
        //public static DataSettings LoadSettings(string filePath = null, bool reloadSettings = false, IDCMSFileProvider fileProvider = null)
        //{
        //    if (!reloadSettings && Singleton<DataSettings>.Instance != null)
        //        return Singleton<DataSettings>.Instance;

        //    fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
        //    filePath = filePath ?? fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);

        //    if (!fileProvider.FileExists(filePath))
        //    {
        //        return new DataSettings();
        //    }

        //    var text = fileProvider.ReadAllText(filePath, Encoding.UTF8);
        //    if (string.IsNullOrEmpty(text))
        //        return new DataSettings();

        //    //从json文件获取数据设置
        //    Singleton<DataSettings>.Instance = JsonConvert.DeserializeObject<DataSettings>(text);

        //    return Singleton<DataSettings>.Instance;
        //}



        //public static DCMSOptions LoadSettings(string filePath = null, bool reloadSettings = false, IDCMSFileProvider fileProvider = null)
        //{
        //    if (!reloadSettings && Singleton<DCMSOptions>.Instance != null)
        //        return Singleton<DCMSOptions>.Instance;

        //    fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
        //    filePath = filePath ?? fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);

        //    if (!fileProvider.FileExists(filePath))
        //    {
        //        return new DCMSOptions();
        //    }

        //    var text = fileProvider.ReadAllText(filePath, Encoding.UTF8);
        //    if (string.IsNullOrEmpty(text))
        //        return new DCMSOptions();

        //    //从json文件获取数据设置
        //    Singleton<DCMSOptions>.Instance = JsonConvert.DeserializeObject<DCMSOptions>(text);

        //    return Singleton<DCMSOptions>.Instance;
        //}
        //public static void SaveSettings(DCMSOptions settings, IDCMSFileProvider fileProvider = null)
        //{
        //    Singleton<DCMSOptions>.Instance = settings ?? throw new ArgumentNullException(nameof(settings));

        //    fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
        //    //数据库保存地址
        //    var filePath = fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);

        //    fileProvider.CreateFile(filePath);

        //    var text = JsonConvert.SerializeObject(Singleton<DCMSOptions>.Instance, Formatting.Indented);
        //    fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        //}




        ///// <summary>
        ///// 保存配置到文件
        ///// </summary>
        ///// <param name="settings"></param>
        ///// <param name="fileProvider"></param>
        //public static void SaveSettings(DataSettings settings, IDCMSFileProvider fileProvider = null)
        //{
        //    Singleton<DataSettings>.Instance = settings ?? throw new ArgumentNullException(nameof(settings));

        //    fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
        //    //数据库保存地址
        //    var filePath = fileProvider.MapPath(DCMSDataSettingsDefaults.FilePath);

        //    fileProvider.CreateFile(filePath);

        //    var text = JsonConvert.SerializeObject(Singleton<DataSettings>.Instance, Formatting.Indented);
        //    fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        //}


        //public static void ResetCache()
        //{
        //    _databaseIsInstalled = null;
        //}


        /// <summary>
        /// 是否已安装数据库
        /// </summary>
        public static bool DatabaseIsInstalled => true;
        #endregion

        #region  （经销商）租户相关

        /// <summary>
        /// 载入安装配置
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="reloadSettings"></param>
        /// <param name="fileProvider"></param>
        /// <returns></returns>
        public static InstallSettings LoadInstallSettings(string filePath = null, bool reloadSettings = false, IDCMSFileProvider fileProvider = null)
        {
            if (!reloadSettings && Singleton<InstallSettings>.Instance != null)
            {
                return Singleton<InstallSettings>.Instance;
            }

            //安装配置文件路径
            fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
            filePath = filePath ?? fileProvider.MapPath(DCMSDataSettingsDefaults.SetupConfigFilePath);

            //配置是否存在
            if (!fileProvider.FileExists(filePath))
            {
                return new InstallSettings();
            }

            var text = fileProvider.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrEmpty(text))
            {
                return new InstallSettings();
            }

            //从JSON文件获取数据设置
            Singleton<InstallSettings>.Instance = JsonConvert.DeserializeObject<InstallSettings>(text);

            return Singleton<InstallSettings>.Instance;
        }


        /// <summary>
        /// 保存安装配置
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="fileProvider"></param>
        public static void SaveInstallSettings(InstallSettings settings, IDCMSFileProvider fileProvider = null)
        {
            Singleton<InstallSettings>.Instance = settings ?? throw new ArgumentNullException(nameof(settings));

            fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;

            //安装配置文件路径
            var filePath = fileProvider.MapPath(DCMSDataSettingsDefaults.SetupConfigFilePath);

            //创建文件（如果不存在）
            fileProvider.CreateFile(filePath);

            //将数据设置保存到文件
            var text = JsonConvert.SerializeObject(Singleton<InstallSettings>.Instance, Formatting.Indented);
            fileProvider.WriteAllText(filePath, text, Encoding.UTF8);

        }
        #endregion

    }
}