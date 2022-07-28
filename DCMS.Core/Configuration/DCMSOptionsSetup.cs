using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Core.Configuration
{
    /// <summary>
    /// 配置选项创建者
    /// </summary>
    public class DCMSOptionsSetup : IConfigureOptions<DCMSOptions>
    {
        private readonly IConfiguration _configuration;

        public DCMSOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 配置options各属性信息
        /// </summary>
        /// <param name="options"></param>
        public void Configure(DCMSOptions options)
        {
            SetDbConnectionsOptions(options);
            SetJwtOptions(options);
            //SetOAuth2Options(options);
        }

        /// <summary>
        /// JWT认证配置项
        /// </summary>
        /// <param name="options"></param>
        private void SetJwtOptions(DCMSOptions options)
        {
            var section = _configuration.GetSection("DCMS:JWT");
            JwtOptions jwt = section.Get<JwtOptions>();
            options.JWT = jwt;
            if (jwt != null)
            {
                if (jwt.Secret == null)
                {
                    jwt.Secret = _configuration["DCMS:JWT:Secret"];
                }
            }
        }

        //private void SetOAuth2Options(DCMSOptions options)
        //{
        //    var oauth2s = new Dictionary<string, OAuth2Options>();
        //    options.OAuth2s = oauth2s;
        //    var section = _configuration.GetSection("DCMS:OAuth2");
        //    IDictionary<string, OAuth2Options> dict = section.Get<Dictionary<string, OAuth2Options>>();
        //    if (dict != null)
        //    {
        //        foreach (KeyValuePair<string, OAuth2Options> item in dict)
        //        {
        //            oauth2s.Add(item.Key, item.Value);
        //        }
        //    }
        //}

        /// <summary>
        /// 数据库连接配置项
        /// </summary>
        /// <param name="options"></param>
        private void SetDbConnectionsOptions(DCMSOptions options)
        {
            var dbConnectionMap = new Dictionary<string, DbConnectionOptions>();
            options.DbConnections = dbConnectionMap;
            IConfiguration section = _configuration.GetSection("DCMS:DbConnections");
            Dictionary<string, DbConnectionOptions> dict = section.Get<Dictionary<string, DbConnectionOptions>>();
            if (dict == null || dict.Count == 0)
            {
                string connectionString = _configuration["ConnectionStrings:DefaultDbContext"];
                if (connectionString == null)
                {
                    return;
                }
                dbConnectionMap.Add("DefaultDb", new DbConnectionOptions
                {
                    ConnectionString = connectionString,
                    DatabaseType = options.DefaultDatabaseType
                });

                return;
            }

            var ambiguous = dict.Keys.GroupBy(d => d).FirstOrDefault(d => d.Count() > 1);
            if (ambiguous != null)
            {
                throw new DCMSException($"数据上下文配置中存在多个配置节点拥有同一个数据库连接名称，存在二义性：{ambiguous.First()}");
            }
            foreach (var db in dict)
            {
                dbConnectionMap.Add(db.Key, db.Value);
            }
        }
    }
}
