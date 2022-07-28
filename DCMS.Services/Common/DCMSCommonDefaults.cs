namespace DCMS.Services.Common
{
    /// <summary>
    /// Represents default values related to common services
    /// </summary>
    public static partial class DCMSCommonDefaults
    {
        /// <summary>
        /// Gets a request path to the keep alive URL
        /// </summary>
        public static string KeepAlivePath => "keepalive/index";

        #region Address attributes

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static string AddressAttributesAllCacheKey => "DCMS.addressattribute.all";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address attribute ID
        /// </remarks>
        public static string AddressAttributesByIdCacheKey => "DCMS.addressattribute.id-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address attribute ID
        /// </remarks>
        public static string AddressAttributeValuesAllCacheKey => "DCMS.addressattributevalue.all-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address attribute value ID
        /// </remarks>
        public static string AddressAttributeValuesByIdCacheKey => "DCMS.addressattributevalue.id-{0}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string AddressAttributesPrefixCacheKey => "DCMS.addressattribute.";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string AddressAttributeValuesPrefixCacheKey => "DCMS.addressattributevalue.";

        /// <summary>
        /// Gets a name of the custom address attribute control
        /// </summary>
        /// <remarks>
        /// {0} : address attribute id
        /// </remarks>
        public static string AddressAttributeControlName => "address_attribute_{0}";

        #endregion

        #region Addresses

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address ID
        /// </remarks>
        public static string AddressesByIdCacheKey => "DCMS.address.id-{0}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string AddressesPrefixCacheKey => "DCMS.address.";

        #endregion

        #region Generic attributes

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : key group
        /// </remarks>
        public static string GenericAttributeCacheKey => "DCMS.genericattribute.{0}-{1}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string GenericAttributePrefixCacheKey => "DCMS.genericattribute.";

        #endregion

        #region Maintenance

        /// <summary>
        /// Gets a path to the database backup files
        /// </summary>
        public static string DbBackupsPath => "db_backups\\";

        /// <summary>
        /// Gets a database backup file extension
        /// </summary>
        public static string DbBackupFileExtension => "bak";

        #endregion

        #region Favicon and app icons

        /// <summary>
        /// Gets a name of the file with code for the head element
        /// </summary>
        public static string HeadCodeFileName => "html_code.html";

        /// <summary>
        /// Gets a path to the favicon and app icons
        /// </summary>
        public static string FaviconAndAppIconsPath => "icons\\icons_{0}";

        /// <summary>
        /// Gets a name of the old favicon icon for current store
        /// </summary>
        public static string OldFaviconIconName => "favicon-{0}.ico";

        #endregion

        #region jsdcms official site

        /// <summary>
        /// Gets a path to request the jsdcms official site for copyright warning
        /// </summary>
        /// <remarks>
        /// {0} : store URL
        /// {1} : whether the store based is on the localhost
        /// </remarks>
        public static string DCMSCopyrightWarningPath => "SiteWarnings.aspx?local={0}&url={1}";

        /// <summary>
        /// Gets a path to request the jsdcms official site for available categories of marketplace extensions
        /// </summary>
        public static string DCMSExtensionsCategoriesPath => "ExtensionsXml.aspx?getCategories=1";

        /// <summary>
        /// Gets a path to request the jsdcms official site for available versions of marketplace extensions
        /// </summary>
        public static string DCMSExtensionsVersionsPath => "ExtensionsXml.aspx?getVersions=1";

        /// <summary>
        /// Gets a path to request the jsdcms official site for marketplace extensions
        /// </summary>
        /// <remarks>
        /// {0} : extension category identifier
        /// {1} : extension version identifier
        /// {2} : extension price identifier
        /// {3} : search term
        /// {4} : page index
        /// {5} : page size
        /// </remarks>
        public static string DCMSExtensionsPath => "ExtensionsXml.aspx?category={0}&version={1}&price={2}&searchTerm={3}&pageIndex={4}&pageSize={5}";

        #endregion
    }
}