using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Configuration
{

    public class TraditionSetting : ISettings
    {
        public string EndPointType { get; set; }
        public string ModeOfCooperation { get; set; }
        public string EndPointStates { get; set; }

    }

    public class RestaurantSetting : ISettings
    {
        public string EndPointType { get; set; }
        public string Characteristics { get; set; }
        public string ModeOfCooperation { get; set; }
        public string EndPointStates { get; set; }
        public string PerConsumptions { get; set; }
    }

    public class SalesProductSetting : ISettings
    {
        public string Brand { get; set; }
        public string PackingForm { get; set; }
        public string ChannelAttributes { get; set; }
        public string Specification { get; set; }
    }

    public class VersionUpdateSetting : ISettings
    {
        public bool Enable { get; set; }
        public string Version { get; set; }
        public string DownLoadUrl { get; set; }
        //public string FtpUserName { get; set; }
        //public string FtpPassword { get; set; }
        public string UpgradeDescription { get; set; }
    }

    public class NewsSetting : ISettings
    {
        public string NewsPath { get; set; }
    }



}
