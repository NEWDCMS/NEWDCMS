using DCMS.Core.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace DCMS.Core.Configuration
{
    public class DCMSOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseType DefaultDatabaseType { get; set; } = DatabaseType.MySql;
        public IDictionary<string, DbConnectionOptions> DbConnections { get; set; }
        //public IDictionary<string, OAuth2Options> OAuth2s { get; set; }
        //public RedisOptions Redis { get; set; }
        public JwtOptions JWT { get; set; }
    }
}
