namespace DCMS.Services.Stores
{
    using DCMS.Core.Caching;


    /// <summary>
    /// 表示与经销商站点服务相关的缓存默认值
    /// </summary>
    public static partial class DCMSStoreDefaults
    {
        public static CacheKey StoreMappingByEntityIdNameCacheKey => new CacheKey("DCMS.STOREMAPPING.ENTITYID-NAME-{0}-{1}", StoreMappingPrefixCacheKey);
        public static string StoreMappingPrefixCacheKey => "DCMS.STOREMAPPING.";


        public static string StoresAllCacheKey => "DCMS.STORES.ALL";
        public static CacheKey StoresByIdCacheKey => new CacheKey("DCMS.STORES.ID-{0}", StoresPrefixCacheKey);
        public static string StoresPrefixCacheKey => "DCMS.STORES.";

    }
}