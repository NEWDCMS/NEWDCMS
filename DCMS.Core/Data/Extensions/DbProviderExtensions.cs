namespace DCMS.Core.Data
{
    public static class DbProviderExtensions
    {
        /// <summary>
        /// 获取UnitOfWork对象, dbName为空时,使用第一个数据库名称
        /// 如：     var dbProvider = _serviceProvider.GetRequiredService<![CDATA[TDbContext]]>;
        /// var uow = dbProvider.GetUnitOfWork<![CDATA[TDbContext]]>("TestDb")
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="provider"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static IUnitOfWork GetUnitOfWork<TDbContext>(this IDbProvider provider, string dbName = null)
        {
            if (provider == null)
            {
                return null;
            }

            return provider.GetUnitOfWork(typeof(TDbContext), dbName);
        }
    }
}
