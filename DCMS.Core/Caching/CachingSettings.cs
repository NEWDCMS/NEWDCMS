using DCMS.Core.Configuration;

namespace DCMS.Core.Caching
{
    /// <summary>
    /// 用于表示缓存配置
    /// </summary>
    public class CachingSettings : ISettings
    {
        /// <summary>
        /// 获取或设置默认缓存时间（分钟）
        /// </summary>
        public int DefaultCacheTime { get; set; } = 60;

        /// <summary>
        /// 获取或设置以分钟为单位的短期缓存时间
        /// </summary>
        public int ShortTermCacheTime { get; set; }

        /// <summary>
        /// 获取或设置捆绑文件的缓存时间（分钟）
        /// </summary>
        public int BundledFilesCacheTime { get; set; }
    }
}
