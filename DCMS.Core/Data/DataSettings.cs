using System;

namespace DCMS.Core.Data
{
    //    /// <summary>
    //    /// 数据库配置类（v1.0）
    //    /// </summary>
    //    public partial class DataSettings
    //    {

    //        public DataSettings()
    //        {
    //            RawDataSettings = new Dictionary<string, string>();
    //        }


    //        [JsonConverter(typeof(StringEnumConverter))]
    //        public DataProviderType DataProvider { get; set; }

    //        public string DataConnectionString { get; set; }

    //        public IDictionary<string, string> RawDataSettings { get; }

    //        [JsonIgnore]
    //        public bool IsValid => DataProvider != DataProviderType.Unknown && !string.IsNullOrEmpty(DataConnectionString);

    //    }


    public class ProcessStatus
    {
        public bool Result { get; set; }
        public string Errors { get; set; }
    }

    public static class InstallExtensions
    {
        public static void RollBack(this ProcessStatus processStatus, Action<bool> invoke)
        {
            if (!processStatus.Result)
            {
                invoke(processStatus.Result);
            }
        }
    }


    /// <summary>
    ///  初始安装配置类
    /// </summary>

    public partial class InstallSettings
    {

        //输入配置

    }

}