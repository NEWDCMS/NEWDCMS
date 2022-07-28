using DCMS.Core;
using DCMS.Core.Data;

namespace DCMS.Data
{
    /// <summary>
    /// EF 数据提供管理器
    /// </summary>
    public partial class EfDataProviderManager : IDataProviderManager
    {

        /// <summary>
        /// 获取数据提供
        /// </summary>
        public IDataProvider DataProvider
        {
            get
            {
                var providerName = DataSettingsManager.LoadSettings().AUTH_RW?.DataProvider;
                switch (providerName)
                {
                    case DataProviderType.SqlServer:
                        return new SqlServerDataProvider();

                    case DataProviderType.MySql:
                        return new SqlServerDataProvider();

                    default:
                        throw new DCMSException($"Not supported data provider name: '{providerName}'");
                }
            }
        }

        public IDBProvider DBProvider
        {
            get
            {
                //var providerName = DataSettingsManager.LoadSettings().AUTH_RW?.DataProvider;
                throw new System.NotImplementedException();
            }
        }
    }
}