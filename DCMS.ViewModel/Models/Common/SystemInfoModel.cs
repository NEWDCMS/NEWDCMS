using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Common
{
    public partial class SystemInfoModel : BaseModel
    {
        public SystemInfoModel()
        {
            Headers = new List<HeaderModel>();
            LoadedAssemblies = new List<LoadedAssembly>();
        }

        [HintDisplayName("Admin.System.SystemInfo.ASPNETInfo", "")]
        public string AspNetInfo { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.IsFullTrust", "")]
        public string IsFullTrust { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.NopVersion", "")]
        public string NopVersion { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.OperatingSystem", "")]
        public string OperatingSystem { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.ServerLocalTime", "")]
        public DateTime ServerLocalTime { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.ServerTimeZone", "")]
        public string ServerTimeZone { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.UTCTime", "")]
        public DateTime UtcTime { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.CurrentUserTime", "")]
        public DateTime CurrentUserTime { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.CurrentStaticCacheManager", "")]
        public string CurrentStaticCacheManager { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.HTTPHOST", "")]
        public string HttpHost { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.Headers", "")]
        public IList<HeaderModel> Headers { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.LoadedAssemblies", "")]
        public IList<LoadedAssembly> LoadedAssemblies { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.RedisEnabled", "")]
        public bool RedisEnabled { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.UseRedisToStoreDataProtectionKeys", "")]
        public bool UseRedisToStoreDataProtectionKeys { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.UseRedisForCaching", "")]
        public bool UseRedisForCaching { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.UseRedisToStorePluginsInfo", "")]
        public bool UseRedisToStorePluginsInfo { get; set; }

        [HintDisplayName("Admin.System.SystemInfo.AzureBlobStorageEnabled", "")]
        public bool AzureBlobStorageEnabled { get; set; }

        public partial class HeaderModel : BaseModel
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public partial class LoadedAssembly : BaseModel
        {
            public string FullName { get; set; }
            public string Location { get; set; }
            public bool IsDebug { get; set; }
            public DateTime? BuildDate { get; set; }
        }
    }
}