using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace DCMS.Core.Configuration
{
    /// <summary>
    /// 表示启动DCMS配置参数
    /// </summary>
    public partial class DCMSConfig
    {
        public string ManageStoreCode { get; set; }

        public bool DisplayFullErrorStack { get; set; }

        public bool RedisEnabled { get; set; }

        public string RedisConnectionString { get; set; }

        public int? RedisDatabaseId { get; set; }

        public bool UseRedisToStoreDataProtectionKeys { get; set; }

        public bool UseRedisForCaching { get; set; }

        public bool UseRedisToStorePluginsInfo { get; set; }


        public string UserAgentStringsPath { get; set; }

        public string CrawlerOnlyUserAgentStringsPath { get; set; }

        public bool UseUnsafeLoadAssembly { get; set; }

        public bool UseSessionStateTempDataProvider { get; set; }

        public string RabbitMQConnectionString { get; set; }

        public bool CustomModelBindEnabled { get; set; }

        /// <summary>
        /// 忽略Redis 超时异常
        /// </summary>
        public bool IgnoreRedisTimeoutException { get; set; } = true;


    }


    public static class DCMSConfigExt
    {
        public static string RabbitNode(this DCMSConfig config, string key)
        {
            var dicts = CommonHelper.GetFormData(config.RabbitMQConnectionString, ';');
            return dicts.ContainsKey(key) ? dicts[key] : "";
        }

        public static string RabbitMQConnectionString(this DCMSConfig config)
        {
            return config.RabbitMQConnectionString;
        }
    }

    public class AppSettings
    {
        public IConfiguration Configuration { get; }

        public AppSettings(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string RedisConnectionString => Configuration.GetConnectionString("Redis");
    }
}