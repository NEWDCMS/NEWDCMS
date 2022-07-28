namespace DCMS.Core
{
    public static partial class DCMSDefaults
    {
        #region SEO友好
        public static int ForumTopicLength => 100;
        public static int SearchEngineNameLength => 200;
        public static string SitemapDateFormat => @"yyyy-MM-dd";
        public static int SitemapMaxUrlNumber => 50000;
        public static string UrlRecordActiveByIdNameLanguageCacheKey => "DCMS.urlrecord.active.id-name-{0}-{1}";
        public static string UrlRecordAllCacheKey => "DCMS.urlrecord.all";
        public static string UrlRecordBySlugCacheKey => "DCMS.urlrecord.active.slug-{0}";
        public static string UrlRecordPrefixCacheKey => "DCMS.urlrecord.";

        #endregion

        #region 系统角色

        /// <summary>
        /// 超级管理员
        /// </summary>
        public static string Administrators => "Administrators";

        /// <summary>
        /// 营销总经理
        /// </summary>
        public static string MarketManagers => "MarketManagers";

        /// <summary>
        /// 大区总经理
        /// </summary>
        public static string RegionManagers => "RegionManagers";

        /// <summary>
        /// 财务经理
        /// </summary>
        public static string FinancialManagers => "FinancialManagers";

        /// <summary>
        /// 业务部经理
        /// </summary>
        public static string BusinessManagers => "BusinessManagers";

        /// <summary>
        /// 业务员
        /// </summary>
        public static string Salesmans => "Salesmans";

        /// <summary>
        /// 员工
        /// </summary>
        public static string Employees => "Employees";

        /// <summary>
        /// 配送员
        /// </summary>
        public static string Delivers => "Delivers";

        /// <summary>
        /// 仓储员
        /// </summary>
        public static string Storekeeper => "Storekeepers";

        /// <summary>
        /// 财务岗位
        /// </summary>
        public static string Financials => "Financials";

        /// <summary>
        /// 经销商管理员
        /// </summary>
        public static string Distributors => "Distributors";

        /// <summary>
        /// 注册用户
        /// </summary>
        public static string RegisteredRoleName => "Registered";

        #endregion

        #region 系统用户

        /// <summary>
        /// 表示搜索引擎
        /// </summary>
        public static string SearchEngineUserName => "SearchEngineUserName";

        /// <summary>
        /// 表示后台任务
        /// </summary>
        public static string BackgroundTaskUserName => "BackgroundTaskUserName";

        #endregion

        #region 用户特性

        public static string UserNameAttribute => "UserName";
        public static string RealNameAttribute => "UserRealName";
        public static string GenderAttribute => "Gender";

        public static string DateOfBirthAttribute => "DateOfBirth";

        public static string CompanyAttribute => "Company";

        public static string StreetAddressAttribute => "StreetAddress";


        public static string StreetAddress2Attribute => "StreetAddress2";


        public static string ZipPostalCodeAttribute => "ZipPostalCode";


        public static string CityAttribute => "City";

        public static string CountyAttribute => "County";


        public static string CountryIdAttribute => "CountryId";


        public static string StateProvinceIdAttribute => "StateProvinceId";

        public static string PhoneAttribute => "MobileNumber";


        public static string FaxAttribute => "Fax";

        public static string VatNumberAttribute => "VatNumber";


        public static string VatNumberStatusIdAttribute => "VatNumberStatusId";

        public static string TimeZoneIdAttribute => "TimeZoneId";


        public static string CustomUserAttributes => "CustomUserAttributes";


        public static string DiscountCouponCodeAttribute => "DiscountCouponCode";


        public static string GiftCardCouponCodesAttribute => "GiftCardCouponCodes";


        public static string AvatarPictureIdAttribute => "AvatarPictureId";


        public static string ForumPostCountAttribute => "ForumPostCount";

        public static string SignatureAttribute => "Signature";


        public static string PasswordRecoveryTokenAttribute => "PasswordRecoveryToken";

        public static string PasswordRecoveryTokenDateGeneratedAttribute => "PasswordRecoveryTokenDateGenerated";


        public static string AccountActivationTokenAttribute => "AccountActivationToken";

        public static string EmailRevalidationTokenAttribute => "EmailRevalidationToken";


        public static string LastVisitedPageAttribute => "LastVisitedPage";


        public static string ImpersonatedUserIdAttribute => "ImpersonatedUserId";


        public static string AdminAreaStoreScopeConfigurationAttribute => "AdminAreaStoreScopeConfiguration";

        public static string CurrencyIdAttribute => "CurrencyId";


        public static string LanguageIdAttribute => "LanguageId";


        public static string LanguageAutomaticallyDetectedAttribute => "LanguageAutomaticallyDetected";


        public static string SelectedPaymentMethodAttribute => "SelectedPaymentMethod";

        public static string SelectedShippingOptionAttribute => "SelectedShippingOption";


        public static string SelectedPickupPointAttribute => "SelectedPickupPoint";


        public static string CheckoutAttributes => "CheckoutAttributes";

        public static string OfferedShippingOptionsAttribute => "OfferedShippingOptions";

        public static string LastContinueShoppingPageAttribute => "LastContinueShoppingPage";


        public static string NotifiedAboutNewPrivateMessagesAttribute => "NotifiedAboutNewPrivateMessages";


        public static string WorkingThemeNameAttribute => "WorkingThemeName";


        public static string TaxDisplayTypeIdAttribute => "TaxDisplayTypeId";


        public static string UseRewardPointsDuringCheckoutAttribute => "UseRewardPointsDuringCheckout";


        public static string EuCookieLawAcceptedAttribute => "EuCookieLaw.Accepted";




        ///// <summary>
        ///// 表单字段
        ///// </summary>
        //public static string UserRealName { get { return "UserRealName"; } }
        //public static string PetName { get { return "PetName"; } }
        //public static string Gender { get { return "Gender"; } }
        //public static string DateOfBirth { get { return "DateOfBirth"; } }
        //public static string Company { get { return "Company"; } }
        //public static string StreetAddress { get { return "StreetAddress"; } }
        //public static string StreetAddress2 { get { return "StreetAddress2"; } }
        //public static string ZipPostalCode { get { return "ZipPostalCode"; } }
        //public static string City { get { return "City"; } }
        //public static string CountryId { get { return "CountryId"; } }
        //public static string StateProvinceId { get { return "StateProvinceId"; } }
        //public static string MobileNumber { get { return "MobileNumber"; } }
        //public static string Mobile { get { return "Mobile"; } }
        //public static string Fax { get { return "Fax"; } }
        //public static string VatNumber { get { return "VatNumber"; } }
        //public static string VatNumberStatusId { get { return "VatNumberStatusId"; } }
        //public static string TimeZoneId { get { return "TimeZoneId"; } }

        public static string WorkingDesktopThemeNameAttribute { get { return "WorkingDesktopThemeName"; } }
        public static string DontUseMobileVersionAttribute { get { return "DontUseMobileVersion"; } }
        //用户中心常规菜单项
        public static string UseDefinedMenuCodesAttribute { get { return "UseDefinedMenuCodes"; } }
        //默认常规菜单项是否收起
        public static string UseDefinedMenuRetractFlagAttribute { get { return "UseDefinedMenuRetractFlag"; } }

        #endregion


    }
}
