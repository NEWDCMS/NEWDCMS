using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Configuration
{
    public class PushSetting : ISettings
    {
        #region 高级配置
        //public int StoreId { get; set; }
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SaltKey { get; set; }
        #endregion
    }
}
