using DCMS.Core;
using DCMS.Core.Caching;

namespace DCMS.Core
{
	/// <summary>
	/// 表示与缓存相关的默认值
	/// </summary>
	public static partial class DCMSCachingDefaults
	{
		/// <summary>
		/// 获取默认缓存时间（分钟）
		/// </summary>
		public static int CacheTime => 60;
		public static string DCMSEntityCacheKey => "DCMS.{0}.ID-{1}";

		/// <summary>
		/// 获取用于将保护密钥列表存储到Redis的密钥（与启用的persistDataProtectionKeysRedis选项一起使用）
		/// </summary>
		public static string RedisDataProtectionKey => "DCMS.DATAPROTECTIONKEYS";

		/// <summary>
		/// URL+storeId+userId+MD5(Data)
		/// </summary>
		public static string RedisDataReSubmitKey => "DCMS.REDISDATARESUBMITKEYS-{0}-{1}-{2}-{3}";
		public static string AuditingSubmitKey => "DCMS.REDISDATARESUBMITKEYS-AUDITING-{0}-{1}-{2}-{3}";
		public static string RedisDataAdjustmentStockKey => "DCMS.REDISDATAADJUSTMENTSTOCKKEY-{0}-{1}-{2}";

	}


	/// <summary>
	/// 缓存Key
	/// </summary>
	public static partial class DCMSDefaults
	{
		#region 默认配置



		public static string SETTINGS_PK => "SETTINGS_PK.{0}";
		public static CacheKey SETTINGSALLASDICTIONARYCACHEKEY => new($"{SETTINGS_PK}-1");
		public static CacheKey SETTINGSALLCACHEKEY => new($"{SETTINGS_PK}-2");



		#endregion

		#region  财务


		public static string ACCOUNTTYPES_PK => "ACCOUNTTYPES_PK.{0}";
		public static CacheKey ACCOUNTTYPES_GETALLACCOUNTINGTYPES_KEY => new($"{ACCOUNTTYPES_PK}-1");


		public static string ACCOUNTINGOPTION_PK => "ACCOUNTINGOPTION_PK.{0}";
		public static CacheKey ACCOUNTOPTIONS_ALL_BY_STORE_KEY => new($"{ACCOUNTINGOPTION_PK}-1");
		public static CacheKey ACCOUNTINGOPTION_BY_IDS_KEY => new($"{ACCOUNTINGOPTION_PK}-2");
		public static CacheKey ACCOUNTINGOPTION_NAME_BY_ID_KEY => new($"{ACCOUNTINGOPTION_PK}-3");
		public static CacheKey ACCOUNTINGOPTION_PASEACCOUNTINGOPTIONTREE_KEY => new($"{ACCOUNTINGOPTION_PK}-4");
		public static CacheKey ACCOUNTINGOPTION_ALL_BY_STOREID_KEY => new($"{ACCOUNTINGOPTION_PK}-5");
		public static CacheKey ACCOUNTINGOPTION_PASEOPTIONSTREE_KEY => new($"{ACCOUNTINGOPTION_PK}-6");
		public static CacheKey ACCOUNTINGOPTION_PASEOPTIONSTREE_TYPE_KEY => new($"{ACCOUNTINGOPTION_PK}-7");
		public static CacheKey ACCOUNTINGOPTION_PASEACCOUNTINGTREE_KEY => new($"{ACCOUNTINGOPTION_PK}-8");
		public static CacheKey GETALLACCOUNTS_KEY => new($"{ACCOUNTINGOPTION_PK}-9");
		public static CacheKey ACCOUNTINGOPTION_NAME_BY_CODETYPEID_KEY => new($"{ACCOUNTINGOPTION_PK}-10");

		#endregion

		#region  报表

		/// <summary>
		/// 匹配
		/// </summary>
		public static string GETSTAFFREPORT_PERCENTAGE_PK => "GETSTAFFREPORT_PERCENTAGE_PK.{0}";
		public static CacheKey GETSTAFFREPORT_PERCENTAGE_SUMMARY_KEY => new($"{GETSTAFFREPORT_PERCENTAGE_PK}-1");
		public static CacheKey GETSTAFFREPORT_PERCENTAGE_ITEM_KEY => new($"{GETSTAFFREPORT_PERCENTAGE_PK}-2");
		public static CacheKey GETSTAFFREPORT_BUSINESSUSER_ACHIEVEMENT_KEY => new($"{GETSTAFFREPORT_PERCENTAGE_PK}-3");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string FUND_GETFUND_PK => "FUND_GETFUND_PK.{0}";
		public static CacheKey FUND_GETFUND_REPORTCUSTOMER_ACCOUNT_KEY => new($"{FUND_GETFUND_PK}-1");
		public static CacheKey FUND_GETFUND_CUSTOMERRECEIPTCASH_KEY => new($"{FUND_GETFUND_PK}-2");
		public static CacheKey FUND_GETFUND_MANUFACTURERACCOUNT_KEY => new($"{FUND_GETFUND_PK}-3");
		public static CacheKey FUND_GETFUND_MANUFACTURERPAYCASH_KEY => new($"{FUND_GETFUND_PK}-4");
		public static CacheKey FUND_GETFUND_ADVANCERECEIPTOVERAGE_KEY => new($"{FUND_GETFUND_PK}-5");
		public static CacheKey FUND_GETFUND_REPORTADVANCEPAYMENTOVERAGE_KEY => new($"{FUND_GETFUND_PK}-6");


		#endregion

		#region 访问控制

		/// <summary>
		/// 匹配
		/// </summary>
		public static string ACLRECORD_PK => "ACLRECORD_PK.{0}";
		public static CacheKey ACLRECORD_BY_ENTITYID_NAME_KEY => new($"{ACLRECORD_PK}-1");



		/// <summary>
		/// 匹配 
		/// </summary>
		public static string PERMISSIONS_PK => "PERMISSIONS_PK.{0}";
		public static CacheKey PERMISSIONS_BY_MODULE_KEY => new($"{PERMISSIONS_PK}-1");
		public static CacheKey PERMISSIONS_BY_ALL_MODULE_KEY => new($"{PERMISSIONS_PK}-2");
		public static CacheKey DADAPERMISSIONS_BY_ROLE_KEY => new($"{PERMISSIONS_PK}-3");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_PERMISSIONID_KEY => new($"{PERMISSIONS_PK}-4");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_SIGN_PERMISSIONID_KEY => new($"{PERMISSIONS_PK}-5");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_USERROLEID_KEY => new($"{PERMISSIONS_PK}-6");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_SIGN_USERROLEID_KEY => new($"{PERMISSIONS_PK}-7");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_PATTERN_BYUSER_KEY => new($"{PERMISSIONS_PK}-8");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_PATTERN_CURROLES_KEY => new($"{PERMISSIONS_PK}-9");
		public static CacheKey GET_GETUSERAPPMODULERECORDS_BYID_KEY => new($"{PERMISSIONS_PK}-10");
		public static CacheKey GET_PERMISSIONRECORDROLESBY_PATTERN_BYUSERROLE_KEY => new($"{PERMISSIONS_PK}-11");
		public static CacheKey GET_GETUSERAPPMODULERECORDS_BYUSERROLEID_KEY => new($"{PERMISSIONS_PK}-12");
		public static CacheKey GET_GETUSERAPPMODULERECORDS_ISINUSERROLE_KEY => new($"{PERMISSIONS_PK}-13");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string PARTNER_PK => "PARTNER_PK.{0}";
		public static CacheKey PARTNER_BY_ID_KEY => new($"{PARTNER_PK}-1");
		public static CacheKey PARTNER_BY_GUID_KEY => new($"{PARTNER_PK}-2");


		#endregion

		#region 档案

		/// <summary>
		/// 匹配
		/// </summary>
		public static string BINDBRANCH_PK => "BINDBRANCH_PK.{0}";
		public static CacheKey BINDBRANCH_ALL_STORE_KEY => new($"{BINDBRANCH_PK}-1");
		public static CacheKey BINDBRANCH_GETALLBRANCHS_STORE_KEY => new($"{BINDBRANCH_PK}-2");
		public static CacheKey BINDBRANCH_GETBRANCHS_STORE_KEY => new($"{BINDBRANCH_PK}-3");
		public static CacheKey BINDBRANCH_GETBRANCHZTREE_STORE_KEY => new($"{BINDBRANCH_PK}-4");

		#endregion

		#region 模块

		/// <summary>
		/// 匹配    
		/// </summary>
		public static string MODULES_PK => "MODULES_PK.{0}";
		public static CacheKey MODULES_ALLOWED_ISPALTFORM_KEY => new($"{MODULES_PK}-1");
		public static CacheKey MODULES_GETMODULEPERMISSIONRECORDS_ISPALTFORM_KEY => new($"{MODULES_PK}-2");
		public static CacheKey MODULES_ALLOWED_KEY => new($"{MODULES_PK}-3");
		public static CacheKey MODULES_ALLS_KEY => new($"{MODULES_PK}-4");
		public static CacheKey MODULES_GETMODULES_BYSTORE_KEY => new($"{MODULES_PK}-5");


		#endregion

		#region  活动

		/// <summary>
		/// 匹配
		/// </summary>
		public static string CAMPAIGN_PK => "CAMPAIGN_PK.{0}";
		public static CacheKey CAMPAIGN_BY_ID_KEY => new($"{CAMPAIGN_PK}-1");
		public static CacheKey CAMPAIGN_CHANNEL_BY_AMPAIGNID_KEY => new($"{CAMPAIGN_PK}-2");
		public static CacheKey CAMPAIGN_CHANNEL_ALLBY_CAMPAIGNID_KEY => new($"{CAMPAIGN_PK}-3");
		public static CacheKey CAMPAIGN_PRODUCT_ALLBY_CAMPAIGNID_KEY => new($"{CAMPAIGN_PK}-4");
		public static CacheKey CAMPAIGN_GIVEPRODUCT_ALLBY_CAMPAIGNID_KEY => new($"{CAMPAIGN_PK}-5");
		public static CacheKey CAMPAIGN_GIVEPRODUCTS_KEY => new($"{CAMPAIGN_PK}-6");
		public static CacheKey CAMPAIGN_BUYPRODUCTS_KEY => new($"{CAMPAIGN_PK}-7");
		public static CacheKey CAMPAIGN_GETAVAILABLECAMPAIGNS => new($"{CAMPAIGN_PK}-8");

		#endregion

		#region  方案

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PLAN_PK => "PLAN_PK.{0}";
		public static CacheKey PLAN_ALL_KEY => new($"{PLAN_PK}-1");
		public static CacheKey PLAN_BY_ID_KEY => new($"{PLAN_PK}-2");
		public static CacheKey BINDPERCENTAGEPLANLIST_STORE_KEY => new($"{PLAN_PK}-3");
		public static CacheKey BINDBUSINESSPERCENTAGEPLANLIST_STORE_KEY => new($"{PLAN_PK}-4");
		public static CacheKey BINDDELIVERPERCENTAGEPLANLIST_STORE_KEY => new($"{PLAN_PK}-5");

		#endregion

		#region  用户

		/// <summary>
		/// 匹配
		/// </summary>
		public static string USER_PK => "USER_PK.{0}";
		public static CacheKey USERROLES_ALL_KEY => new($"{USER_PK}-1");
		public static CacheKey USERS_ALL_BY_STORE_KEY => new($"{USER_PK}-2");
		public static CacheKey USERS_ALL_ADMIN_BY_STOREIDS_KEY => new($"{USER_PK}-3");
		public static CacheKey USERS_ALL_ADMIN_KEY => new($"{USER_PK}-4");
		public static CacheKey USERROLES_ALL_BY_STORE_KEY => new($"{USER_PK}-5");
		public static CacheKey USERROLES_BY_SYSTEMNAME_KEY => new($"{USER_PK}-6");
		public static CacheKey USERROLES_BY_SYSTEMNAME_GETUSERROLESBYUSER_KEY => new($"{USER_PK}-7");
		public static CacheKey USERROLES_BY_GETALLADMIN_USERMOBILENUMBERS_BY_STORE_KEY => new($"{USER_PK}-8");
		public static CacheKey USERROLES_ISINUSERROLE_USER_USERROLESYSTEMNAME_ONLYACTIVEUSERROLES => new($"{USER_PK}-9");
		public static CacheKey USERS_USERDISTRICTSL_BY_ID_KEY => new($"{USER_PK}-10");
		public static CacheKey USERROLES_ALL_PAGE_KEY => new($"{USER_PK}-11");
		public static CacheKey USERAUTHORIZECODES_BY_USERID_KEY => new($"{USER_PK}-12");
		public static CacheKey USER_NAME_BY_ID_KEY => new($"{USER_PK}-13");
		public static CacheKey USER_APPID_BY_ID_KEY => new($"{USER_PK}-14");
		public static CacheKey USER_GETUSER_BY_NAME_KEY =>new($"{USER_PK}-15");
		public static CacheKey USER_GETUSER_BY_GUID_KEY => new($"{USER_PK}-16");
		public static CacheKey USER_GETUSER_BY_EMAILL_KEY => new($"{USER_PK}-17");
		public static CacheKey USER_GETUSER_BY_SYSTEMNAME_KEY => new($"{USER_PK}-18");
		public static CacheKey USER_GETUSER_BY_MOBILENAMBER_KEY => new($"{USER_PK}-19");
		public static CacheKey USER_GETUSER_BY_IDS_KEY => new($"{USER_PK}-20");
		public static CacheKey USER_GETSUBORDINATE_BY_IDS_KEY => new($"{USER_PK}-21");
		public static CacheKey USER_GETUSER_BY_SYSTEM_ROLE_NAME_KEY => new($"{USER_PK}-22");
		public static CacheKey USER_BINDUSER_BY_SYSTEM_ROLE_NAME_KEY => new($"{USER_PK}-23");
		public static CacheKey USER_GETSTOREMANAGERSBYSTOREID_KEY => new($"{USER_PK}-24");
		public static CacheKey USERROLES_ALLBY_SYSTEMNAME_GETUSERROLESBYUSER_KEY => new($"{USER_PK}-25");



		#endregion

		#region 商品
		/// <summary>
		/// 匹配
		/// </summary>
		public static string PERCENTAGE_PK => "PERCENTAGE_PK.{0}";
		public static CacheKey PERCENTAGE_ALL_KEY => new($"{PERCENTAGE_PK}-1");
		public static CacheKey PERCENTAGE_BY_ID_KEY => new($"{PERCENTAGE_PK}-2");
		public static CacheKey PERCENTAGES_BY_IDS_KEY => new($"{PERCENTAGE_PK}-3");
		public static CacheKey PERCENTAGE_OPTION_ALL_KEY => new($"{PERCENTAGE_PK}-4");
		public static CacheKey PERCENTAGE_OPTION_BY_ID_KEY => new($"{PERCENTAGE_PK}-5");
		public static CacheKey PERCENTAGERANGEOPTIONS_BY_IDS_KEY => new($"{PERCENTAGE_PK}-6");
		public static CacheKey GETPERCENTAGE_BY_BYPRODUCT_KEY => new($"{PERCENTAGE_PK}-7");
		public static CacheKey GETPERCENTAGE_BY_BYCATAGORY_KEY => new($"{PERCENTAGE_PK}-8");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRODUCTS_PK => "PRODUCTS_PK.{0}";
		public static CacheKey PRODUCTS_BY_ID_KEY => new($"{PRODUCTS_PK}-1");
		public static CacheKey PRODUCTS_BY_CATAGORYID_KEY => new($"{PRODUCTS_PK}-2");
		public static CacheKey PRODUCTSPRICES_BY_ID_KEY => new($"{PRODUCTS_PK}-3");
		public static CacheKey PRODUCTPRICES_BY_PRODUCTIDS_KEY => new($"{PRODUCTS_PK}-4");
		public static CacheKey PRODUCTPRICES_GETALLPRODUCTPRICES_BY_STORE_KEY => new($"{PRODUCTS_PK}-5");
		public static CacheKey PRODUCTSPRICES_UNITS_BY_ID_KEY => new($"{PRODUCTS_PK}-6");
		public static CacheKey PRODUCTSPRICES_BY_PRODUCTID_KEY => new($"{PRODUCTS_PK}-7");
		public static CacheKey PRODUCTS_IDS_BY_NAME_KEY => new($"{PRODUCTS_PK}-8");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string BRAND_PK => "BRAND_PK.{0}";
		public static CacheKey BRAND_ALL_KEY => new($"{BRAND_PK}-1");
		public static CacheKey BRAND_BY_ID_KEY => new($"{BRAND_PK}-2");
		public static CacheKey BINDBRAND_ALL_STORE_KEY => new($"{BRAND_PK}-3");
		public static CacheKey BRANDS_BY_STORE_IDS_KEY => new($"{BRAND_PK}-4");
		public static CacheKey BRANDS_NOTRACK_BY_STORE_IDS_KEY => new($"{BRAND_PK}-5");
		public static CacheKey BRAND_BY_IDS_KEY => new($"{BRAND_PK}-6");
		public static CacheKey BRAND_NAME_BY_ID_KEY => new($"{BRAND_PK}-7");
		public static CacheKey BRAND_GETALLBRANDS_BY_KEY => new($"{BRAND_PK}-8");
		public static CacheKey BRAND_GETALLBRANDS_GETBRANDLIST_BY_KEY => new($"{BRAND_PK}-9");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string CATEGORIES_PK => "CATEGORIES_PK.{0}";
		public static CacheKey CATEGORIES_BY_ID_KEY => new($"{CATEGORIES_PK}-1");
		public static CacheKey CATEGORIES_BY_IDS_KEY => new($"{CATEGORIES_PK}-2");
		public static CacheKey CATEGORIES_NOTRACT_BY_IDS_KEY => new($"{CATEGORIES_PK}-3");
		public static CacheKey BINDCATEGORIES_ALL_STORE_KEY => new($"{CATEGORIES_PK}-4");
		public static CacheKey CATEGORIES_BY_PARENT_CATEGORY_ID_KEY => new($"{CATEGORIES_PK}-5");
		public static CacheKey PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY => new($"{CATEGORIES_PK}-6");
		public static CacheKey PRODUCTCATEGORIES_ALLBYPRODUCTID_KEY => new($"{CATEGORIES_PK}-7");
		public static CacheKey CATEGORY_NAME_BY_ID_KEY => new($"{CATEGORIES_PK}-8");
		public static CacheKey GETALLCATEGORIES_KEY => new($"{CATEGORIES_PK}-9");



		/// <summary>
		/// 匹配
		/// </summary>
		public static string GIVEQUOTA_PK => "GIVEQUOTA_PK.{0}";
		public static CacheKey GIVEQUOTARECORDS_ALL_KEY => new($"{GIVEQUOTA_PK}-1");
		public static CacheKey GIVEQUOTARECORDS_BY_ID_KEY => new($"{GIVEQUOTA_PK}-2");
		public static CacheKey GIVEQUOTA_ALL_KEY => new($"{GIVEQUOTA_PK}-3");
		public static CacheKey GIVEQUOTA_BY_ID_KEY => new($"{GIVEQUOTA_PK}-4");
		public static CacheKey GIVEQUOTA_BY_STORE_USER_YEAR_KEY => new($"{GIVEQUOTA_PK}-5");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string MANUFACTURER_PK => "MANUFACTURER_PK.{0}";
		public static CacheKey MANUFACTURER_ALL_STORE_KEY => new($"{MANUFACTURER_PK}-1");
		public static CacheKey MANUFACTURER_BY_ID_KEY => new($"{MANUFACTURER_PK}-2");
		public static CacheKey BINDMANUFACTURER_ALL_STORE_KEY => new($"{MANUFACTURER_PK}-3");
		public static CacheKey MANUFACTURER_BY_IDS_KEY => new($"{MANUFACTURER_PK}-4");
		public static CacheKey MANUFACTURER_NAME_BY_ID_KEY => new($"{MANUFACTURER_PK}-5");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRODUCTATTRIBUTES_PK => "PRODUCTATTRIBUTES_PK.{0}";
		public static CacheKey PRODUCTATTRIBUTES_ALL_KEY => new($"{PRODUCTATTRIBUTES_PK}-1");
		public static CacheKey PRODUCTATTRIBUTES_BY_ID_KEY => new($"{PRODUCTATTRIBUTES_PK}-2");
		public static CacheKey PRODUCTVARIANTATTRIBUTES_ALL_KEY => new($"{PRODUCTATTRIBUTES_PK}-3");
		public static CacheKey PRODUCTVARIANTATTRIBUTES_BY_ID_KEY => new($"{PRODUCTATTRIBUTES_PK}-4");
		public static CacheKey PRODUCTVARIANTATTRIBUTEVALUES_ALL_KEY => new($"{PRODUCTATTRIBUTES_PK}-5");
		public static CacheKey PRODUCTVARIANTATTRIBUTEVALUES_BY_ID_KEY => new($"{PRODUCTATTRIBUTES_PK}-6");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRICEPLAN_PK => "PRICEPLAN_PK.{0}";
		public static CacheKey PRICEPLAN_ALL_KEY => new($"{PRICEPLAN_PK}-1");
		public static CacheKey PRICEPLAN_BY_ID_KEY => new($"{PRICEPLAN_PK}-2");
		public static CacheKey TIERPRICE_BY_PRODUCTIDS_KEY => new($"{PRICEPLAN_PK}-3");
		public static CacheKey PRODUCTSPECIFICATIONATTRIBUTE_ALLBYPRODUCTID_KEY => new($"{PRICEPLAN_PK}-4");
		public static CacheKey ALLPRODUCTSPECIFICATIONATTRIBUTE_ALLBYPRODUCTID_KEY => new($"{PRICEPLAN_PK}-5");
		public static CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY => new($"{PRICEPLAN_PK}-6");
		public static CacheKey GETSPECIFICATIONATTRIBUTEOPTIONSBYSTORE_BY_STORE_KEY => new($"{PRICEPLAN_PK}-7");
		public static CacheKey SPECIFICATIONATTRIBUTEOPTION_NAME_BY_ID_KEY => new($"{PRICEPLAN_PK}-8");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string STATISTICALTYPE_PK => "STATISTICALTYPE_PK.{0}";
		public static CacheKey STATISTICALTYPE_ALL_KEY => new($"{STATISTICALTYPE_PK}-1");
		public static CacheKey STATISTICALTYPE_BY_ID_KEY => new($"{STATISTICALTYPE_PK}-2");


		#endregion

		#region 租户

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STORES_PK => "STORES_PK.{0}";
		public static CacheKey STORES_ALL_KEY => new($"{STORES_PK}-1");
		public static CacheKey STORES_BY_ID_KEY => new($"{STORES_PK}-2");
		public static CacheKey BINDSTORE_ALLLIST => new($"{STORES_PK}-3");
		public static CacheKey STORE_NAME_BY_ID_KEY => new($"{STORES_PK}-4");
		public static CacheKey STOREMAPPING_BY_ENTITYID_NAME_KEY => new($"{STORES_PK}-5");

		#endregion

		#region 渠道片区


		/// <summary>
		/// 匹配
		/// </summary>
		public static string CHANNEL_PK => "CHANNEL_PK.{0}";
		public static CacheKey CHANNEL_ALL_STORE_KEY => new($"{CHANNEL_PK}-1");
		public static CacheKey BINDCHANNEL_ALL_STORE_KEY => new($"{CHANNEL_PK}-2");
		public static CacheKey CHANNEL_BY_ID_KEY => new($"{CHANNEL_PK}-3");
		public static CacheKey CHANNEL_NAME_BY_ID_KEY => new($"{CHANNEL_PK}-4");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string DISTRICT_PK => "DISTRICT_PK.{0}";
		public static CacheKey DISTRICT_ALL_STORE_KEY => new($"{DISTRICT_PK}-1");
		public static CacheKey BINDDISTRICT_ALL_STORE_KEY => new($"{DISTRICT_PK}-2");
		public static CacheKey DISTRICT_BY_ID_KEY => new($"{DISTRICT_PK}-3");
		public static CacheKey DISTRICT_BY_IDS_KEY => new($"{DISTRICT_PK}-4");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string RANK_PK => "RANK_PK.{0}";
		public static CacheKey RANK_ALL_STORE_KEY => new($"{RANK_PK}-1");
		public static CacheKey BINDRANK_ALL_STORE_KEY => new($"{RANK_PK}-2");
		public static CacheKey RANK_BY_ID_KEY => new($"{RANK_PK}-3");
		public static CacheKey RANK_BY_IDS_KEY => new($"{RANK_PK}-4");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string RECEIVABLEDETAIL_PK => "RECEIVABLEDETAIL_PK.{0}";
		public static CacheKey RECEIVABLEDETAIL_ALL_STORE_KEY => new($"{RECEIVABLEDETAIL_PK}-1");
		public static CacheKey RECEIVABLEDETAIL_BY_ID_KEY => new($"{RECEIVABLEDETAIL_PK}-2");



		/// <summary>
		/// 匹配
		/// </summary>
		public static string RECEIVABLE_PK => "RECEIVABLE_PK.{0}";
		public static CacheKey RECEIVABLE_ALL_STORE_KEY => new($"{RECEIVABLE_PK}-1");
		public static CacheKey RECEIVABLE_BY_ID_KEY => new($"{RECEIVABLE_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string TERMINAL_PK => "RECEIVABLE_PK.{0}";
		public static CacheKey TERMINAL_NAME_BY_ID_KEY => new($"{TERMINAL_PK}-1");
		public static CacheKey TERMINAL_CODE_BY_ID_KEY => new($"{TERMINAL_PK}-2");
		public static CacheKey TERMINAL_IDS_BY_NAME_KEY => new($"{TERMINAL_PK}-3");
		public static CacheKey TERMINAL_ALL_STORE_KEY => new($"{TERMINAL_PK}-4");
		public static CacheKey TERMINAL_BY_ID_KEY => new($"{TERMINAL_PK}-5");
		public static CacheKey TERMINAL_BY_IDS_KEY => new($"{TERMINAL_PK}-6");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string LINETIER_PK => "LINETIER_PK.{0}";
		public static CacheKey LINETIER_ALL_STORE_KEY => new($"{LINETIER_PK}-1");
		public static CacheKey BINDLINETIER_ALL_STORE_KEY => new($"{LINETIER_PK}-2");
		public static CacheKey LINETIER_BY_ID_KEY => new($"{LINETIER_PK}-3");
		public static CacheKey USERLINETIERASSIGN_BY_ID_KEY => new($"{LINETIER_PK}-3");
		public static CacheKey LINETIEROPTION_BY_ID_KEY => new($"{LINETIER_PK}-4");



		/// <summary>
		/// 匹配
		/// </summary>
		public static string TRACKING_PK => "TRACKING_PK.{0}";
		public static CacheKey TRACKING_ALL_STORE_KEY => new($"{TRACKING_PK}-1");
		public static CacheKey TRACKING_BY_ID_KEY => new($"{TRACKING_PK}-2");
		public static CacheKey VISITSTORE_BY_ID_KEY => new($"{TRACKING_PK}-3");



		/// <summary>
		/// 匹配
		/// </summary>
		public static string ADVANCEPAYMENTBILL_PK => "ADVANCEPAYMENTBILL_PK.{0}";
		public static CacheKey ADVANCEPAYMENTBILL_BY_ID_KEY => new($"{ADVANCEPAYMENTBILL_PK}-1");
		public static CacheKey ADVANCEPAYMENTBILL_ACCOUNTING_BY_ID_KEY => new($"{ADVANCEPAYMENTBILL_PK}-2");
		public static CacheKey ADVANCEPAYMENTBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{ADVANCEPAYMENTBILL_PK}-3");
		public static CacheKey ADVANCEPAYMENTBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{ADVANCEPAYMENTBILL_PK}-4");

		#endregion

		#region 财务类

		/// <summary>
		/// 匹配
		/// </summary>
		public static string ADVANCEPRCEIPTBILL_PK => "ADVANCEPRCEIPTBILL_PK.{0}";
		public static CacheKey ADVANCEPRCEIPTBILL_BY_ID_KEY => new($"{ADVANCEPRCEIPTBILL_PK}-1");
		public static CacheKey ADVANCEPRCEIPTBILL_BY_NUMBER_KEY => new($"{ADVANCEPRCEIPTBILL_PK}-2");
		public static CacheKey ADVANCEPRCEIPTBILL_ACCOUNTING_BY_ID_KEY => new($"{ADVANCEPRCEIPTBILL_PK}-3");
		public static CacheKey ADVANCEPRCEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{ADVANCEPRCEIPTBILL_PK}-4");
		public static CacheKey ADVANCEPRCEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{ADVANCEPRCEIPTBILL_PK}-5");



		/// <summary>
		/// 匹配
		/// </summary>
		public static string CASHRECEIPTBILL_PK => "CASHRECEIPTBILL_PK.{0}";
		public static CacheKey CASHRECEIPTBILL_BY_ID_KEY => new($"{CASHRECEIPTBILL_PK}-1");
		public static CacheKey CASHRECEIPTBILL_BY_NUMBER_KEY => new($"{CASHRECEIPTBILL_PK}-2");
		public static CacheKey CASHRECEIPTBILLITEM_BY_ID_KEY => new($"{CASHRECEIPTBILL_PK}-3");
		public static CacheKey CASHRECEIPTBILLITEM_ALL_KEY => new($"{CASHRECEIPTBILL_PK}-4");
		public static CacheKey CASHRECEIPTBILL_ACCOUNTING_BY_ID_KEY => new($"{CASHRECEIPTBILL_PK}-5");
		public static CacheKey CASHRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{CASHRECEIPTBILL_PK}-6");
		public static CacheKey CASHRECEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{CASHRECEIPTBILL_PK}-7");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string COSTCONTRACTBILL_PK => "COSTCONTRACTBILL_PK.{0}";
		public static CacheKey COSTCONTRACTBILL_BY_ID_KEY => new($"{COSTCONTRACTBILL_PK}-1");
		public static CacheKey COSTCONTRACTBILLITEM_BY_ID_KEY => new($"{COSTCONTRACTBILL_PK}-2");
		public static CacheKey COSTCONTRACTBILLITEM_ALL_KEY => new($"{COSTCONTRACTBILL_PK}-3");
		public static CacheKey COSTCONTRACTBILLITEMS_ALL_KEY => new($"{COSTCONTRACTBILL_PK}-4");
		public static CacheKey CAMPAIGN_GETTCOSTCONTRACTS => new($"{COSTCONTRACTBILL_PK}-5");
		public static CacheKey CAMPAIGN_GETTCOSTCONTRACTS_STOREID_CUSTOMERID_BUSINESSUSERID => new($"{COSTCONTRACTBILL_PK}-6");
		public static CacheKey CAMPAIGN_GETTCOSTCONTRACTS_STOREID_CUSTOMERID => new($"{COSTCONTRACTBILL_PK}-7");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string COSTEXPENDITUREBILL_PK => "COSTEXPENDITUREBILL_PK.{0}";
		public static CacheKey COSTEXPENDITUREBILL_BY_ID_KEY => new($"{COSTEXPENDITUREBILL_PK}-1");
		public static CacheKey COSTEXPENDITUREBILL_BY_NUMBER_KEY => new($"{COSTEXPENDITUREBILL_PK}-2");
		public static CacheKey COSTEXPENDITUREBILLITEM_BY_ID_KEY => new($"{COSTEXPENDITUREBILL_PK}-3");
		public static CacheKey COSTEXPENDITUREBILLITEM_ALL_KEY => new($"{COSTEXPENDITUREBILL_PK}-4");
		public static CacheKey COSTEXPENDITUREBILL_ACCOUNTING_BY_ID_KEY => new($"{COSTEXPENDITUREBILL_PK}-5");
		public static CacheKey COSTEXPENDITUREBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{COSTEXPENDITUREBILL_PK}-6");
		public static CacheKey COSTEXPENDITUREBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{COSTEXPENDITUREBILL_PK}-7");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string FINANCIALINCOMEBILL_PK => "FINANCIALINCOMEBILL_PK.{0}";
		public static CacheKey FINANCIALINCOMEBILL_BY_ID_KEY => new($"{FINANCIALINCOMEBILL_PK}-1");
		public static CacheKey FINANCIALINCOMEBILLITEM_BY_ID_KEY => new($"{FINANCIALINCOMEBILL_PK}-2");
		public static CacheKey FINANCIALINCOMEBILLITEM_ALL_KEY => new($"{FINANCIALINCOMEBILL_PK}-3");
		public static CacheKey FINANCIALINCOMEBILL_ACCOUNTING_BY_ID_KEY => new($"{FINANCIALINCOMEBILL_PK}-4");
		public static CacheKey FINANCIALINCOMEBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{FINANCIALINCOMEBILL_PK}-5");
		public static CacheKey FINANCIALINCOMEBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{FINANCIALINCOMEBILL_PK}-6");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string PAYMENTRECEIPTBILL_PK => "PAYMENTRECEIPTBILL_PK.{0}";
		public static CacheKey PAYMENTRECEIPTBILL_BY_NUMBER_KEY => new($"{PAYMENTRECEIPTBILL_PK}-1");
		public static CacheKey PAYMENTRECEIPTBILL_BY_ID_KEY => new($"{PAYMENTRECEIPTBILL_PK}-2");
		public static CacheKey PAYMENTRECEIPTBILLITEM_BY_ID_KEY => new($"{PAYMENTRECEIPTBILL_PK}-3");
		public static CacheKey PAYMENTRECEIPTBILLITEM_ALL_KEY => new($"{PAYMENTRECEIPTBILL_PK}-4");
		public static CacheKey PAYMENTRECEIPTBILL_ACCOUNTING_BY_ID_KEY => new($"{PAYMENTRECEIPTBILL_PK}-5");
		public static CacheKey PAYMENTRECEIPTBILL_ACCOUNTINGL_BY_BILLID_KEY => new($"{PAYMENTRECEIPTBILL_PK}-6");
		public static CacheKey PAYMENTRECEIPTBILL_ACCOUNTING_ALLBY_BILLID_KEY => new($"{PAYMENTRECEIPTBILL_PK}-7");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string RECORDINGVOUCHER_PK => "RECORDINGVOUCHER_PK.{0}";
		public static CacheKey RECORDINGVOUCHER_BY_ID_KEY => new($"{RECORDINGVOUCHER_PK}-1");
		public static CacheKey RECORDINGVOUCHERITEM_BY_ID_KEY => new($"{RECORDINGVOUCHER_PK}-2");
		public static CacheKey RECORDINGVOUCHERITEM_ALL_KEY => new($"{RECORDINGVOUCHER_PK}-3");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string GENERICATTRIBUTE_PK => "GENERICATTRIBUTE_PK.{0}";
		public static CacheKey GENERICATTRIBUTE_KEY => new($"{GENERICATTRIBUTE_PK}-1");


		#endregion

		#region 

		/// <summary>
		/// 匹配
		/// </summary>
		public static string NEWSCATEGORIES_PK => "NEWSCATEGORIES_PK.{0}";
		public static CacheKey NEWSCATEGORIES_BY_ID_KEY => new($"{NEWSCATEGORIES_PK}-1");
		public static CacheKey NEWSCATEGORIES_BY_PARENT_CATEGORY_ID_KEY => new($"{NEWSCATEGORIES_PK}-2");
		public static CacheKey NEWSNEWSCATEGORIES_ALLBYCATEGORYID_KEY => new($"{NEWSCATEGORIES_PK}-3");
		public static CacheKey NEWSNEWSCATEGORIES_ALLBYPRODUCTID_KEY => new($"{NEWSCATEGORIES_PK}-4");
		public static CacheKey NEWCATEGORY_NAME_BY_ID_KEY => new($"{NEWSCATEGORIES_PK}-5");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PURCHASEBILL_PK => "PURCHASEBILL_PK.{0}";

		public static CacheKey PURCHASEBILL_BY_ID_KEY => new($"{PURCHASEBILL_PK}-1");
		public static CacheKey PURCHASEBILL_BY_NUMBER_KEY => new($"{PURCHASEBILL_PK}-2");
		public static CacheKey PURCHASEBILL_BY_STOREID_KEY => new($"{PURCHASEBILL_PK}-3");
		public static CacheKey PURCHASEBILL_BY_BILLNUMBERID_KEY => new($"{PURCHASEBILL_PK}-4");

		public static CacheKey PURCHASEBILL_ITEM_ALLBY_PURCHASEID_KEY => new($"{PURCHASEBILL_PK}-5");
		public static CacheKey PURCHASEBILL_ACCOUNTING_BY_ID_KEY => new($"{PURCHASEBILL_PK}-6");

		public static CacheKey PURCHASEBILL_ACCOUNTINGL_BY_PURCHASEID_KEY => new($"{PURCHASEBILL_PK}-7");

		public static CacheKey PURCHASEBILL_ACCOUNTINGL_BY_BILLIDS_KEY => new($"{PURCHASEBILL_PK}-8");
		
		public static CacheKey PURCHASEBILL_ACCOUNTING_ALLBY_PURCHASEID_KEY => new($"{PURCHASEBILL_PK}-9");


		public static string PURCHASE_PK => "PURCHASE_PK.{0}";
		public static CacheKey PURCHASE_BY_ID_KEY => new($"{PURCHASE_PK}-1");
		public static CacheKey PURCHASE_GETPURCHASE_REPORTITEM_KEY => new($"{PURCHASE_PK}-2");
		public static CacheKey PURCHASE_GETPURCHASE_REPORTSUMMARY_PRODUCT_KEY => new($"{PURCHASE_PK}-3");
		public static CacheKey PURCHASE_GETPURCHASE_REPORTSUMMARY_MANUFACTURER_KEY => new($"{PURCHASE_PK}-4");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string PURCHASERETURNBILL_PK => "PURCHASERETURNBILL_PK.{0}";
		public static CacheKey PURCHASERETURNBILL_BY_ID_KEY => new($"{PURCHASERETURNBILL_PK}-1");
		public static CacheKey PURCHASERETURNBILL_BY_NUMBER_KEY => new($"{PURCHASERETURNBILL_PK}-2");
		public static CacheKey PURCHASERETURNBILL_BY_STOREID_KEY => new($"{PURCHASERETURNBILL_PK}-3");
		public static CacheKey PURCHASERETURNBILL_BY_BILLNUMBERID_KEY => new($"{PURCHASERETURNBILL_PK}-4");
		public static CacheKey PURCHASERETURNBILL_ITEM_ALLBY_SALEID_KEY => new($"{PURCHASERETURNBILL_PK}-5");
		public static CacheKey PURCHASERETURNBILL_ACCOUNTING_BY_ID_KEY => new($"{PURCHASERETURNBILL_PK}-6");
		public static CacheKey PURCHASERETURNBILL_ACCOUNTINGL_BY_SALEID_KEY => new($"{PURCHASERETURNBILL_PK}-7");
		public static CacheKey PURCHASERETURNBILL_ACCOUNTING_ALLBY_SALEID_KEY => new($"{PURCHASERETURNBILL_PK}-8");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string DISPATCHBILL_PK => "DISPATCHBILL_PK.{0}";
		public static CacheKey DISPATCHBILL_BY_ID_KEY => new($"{DISPATCHBILL_PK}-1");
		public static CacheKey DISPATCHBILL_ITEMS_BY_ID_KEY => new($"{DISPATCHBILL_PK}-2");
		public static CacheKey DISPATCHBILL_BY_STOREID_KEY => new($"{DISPATCHBILL_PK}-3");
		public static CacheKey DISPATCHBILL_ITEM_ALLBY_SALEID_KEY => new($"{DISPATCHBILL_PK}-4");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string FINANCERECEIVEACCOUNTBILL_PK => "FINANCERECEIVEACCOUNTBILL_PK.{0}";
		public static CacheKey FINANCERECEIVEACCOUNTBILL_BY_ID_KEY => new($"{FINANCERECEIVEACCOUNTBILL_PK}-1");
		public static CacheKey FINANCERECEIVEACCOUNTBILL_BY_NUMBER_KEY => new($"{FINANCERECEIVEACCOUNTBILL_PK}-2");
		public static CacheKey FINANCERECEIVEACCOUNTBILL_BY_STOREID_KEY => new($"{FINANCERECEIVEACCOUNTBILL_PK}-3");
		public static CacheKey FINANCERECEIVEACCOUNTBILL_ACCOUNTINGL_BY_FINANCERECEIVEACCOUNTBILLID_KEY => new($"{FINANCERECEIVEACCOUNTBILL_PK}-4");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PICKING_PK => "PICKING_PK.{0}";
		public static CacheKey PICKING_BY_STOREID_KEY => new($"{PICKING_PK}-1");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string RETURNBILL_PK => "RETURNBILL_PK.{0}";
		public static CacheKey RETURNBILL_BY_ID_KEY => new($"{RETURNBILL_PK}-1");
		public static CacheKey RETURNBILL_BY_RESERVATIONID_KEY => new($"{RETURNBILL_PK}-2");
		public static CacheKey RETURNBILL_BY_NUMBER_KEY => new($"{RETURNBILL_PK}-3");
		public static CacheKey RETURNBILL_BY_STOREID_KEY => new($"{RETURNBILL_PK}-4");
		public static CacheKey RETURNBILL_ITEM_ALLBY_RETURNID_KEY => new($"{RETURNBILL_PK}-5");
		public static CacheKey GETRETURNBILLSBYBUSINESSUSERS => new($"{RETURNBILL_PK}-6");
		public static CacheKey GERETURNBILLSBYDELIVERYUSERS => new($"{RETURNBILL_PK}-7");
		public static CacheKey RETURNBILL_ACCOUNTING_BY_ID_KEY => new($"{RETURNBILL_PK}-8");
		public static CacheKey RETURNBILL_ACCOUNTINGL_BY_RETURNID_KEY => new($"{RETURNBILL_PK}-9");
		public static CacheKey RETURNBILL_ACCOUNTING_ALLBY_RETURNID_KEY => new($"{RETURNBILL_PK}-10");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string RETURNRESERVATIONBILL_PK => "RETURNRESERVATIONBILL_PK.{0}";
		public static CacheKey RETURNRESERVATIONBILL_BY_ID_KEY => new($"{RETURNRESERVATIONBILL_PK}-1");
		public static CacheKey RETURNRESERVATIONBILL_BY_NUMBER_KEY => new($"{RETURNRESERVATIONBILL_PK}-2");
		public static CacheKey RETURNRESERVATIONBILL_BY_STOREID_KEY => new($"{RETURNRESERVATIONBILL_PK}-3");
		public static CacheKey RETURNRESERVATIONBILLNULLWAREHOUSE_BY_STOREID_KEY => new($"{RETURNRESERVATIONBILL_PK}-4");
		public static CacheKey RETURNRESERVATIONBILL_ITEM_ALLBY_RETURNRESERVATIONID_KEY => new($"{RETURNRESERVATIONBILL_PK}-5");
		public static CacheKey RETURNRESERVATIONACCOUNTING_BY_ID_KEY => new($"{RETURNRESERVATIONBILL_PK}-6");
		public static CacheKey RETURNRESERVATIONACCOUNTINGL_BY_RETURNRESERVATIONID_KEY => new($"{RETURNRESERVATIONBILL_PK}-7");
		public static CacheKey RETURNRESERVATIONACCOUNTING_ALLBY_RETURNRESERVATIONID_KEY => new($"{RETURNRESERVATIONBILL_PK}-8");
		#endregion

		#region
		/// <summary>
		/// 匹配
		/// </summary>
		public static string SALEBILL_PK => "SALEBILL_PK.{0}";
		public static CacheKey SALEBILL_BY_ID_KEY => new($"{SALEBILL_PK}-1");
		public static CacheKey SALEBILL_BY_RESERVATIONID_KEY => new($"{SALEBILL_PK}-2");
		public static CacheKey SALEBILL_BY_NUMBER_KEY => new($"{SALEBILL_PK}-3");
		public static CacheKey SALEBILL_BY_STOREID_KEY => new($"{SALEBILL_PK}-4");
		public static CacheKey SALEBILL_BY_STOREID_KEY_1 => new($"{SALEBILL_PK}-5");
		public static CacheKey SALEBILL_BY_STOREID_KEY_2 => new($"{SALEBILL_PK}-6");
		public static CacheKey SALEBILL_BY_BILLNUMBERID_KEY => new($"{SALEBILL_PK}-7");
		public static CacheKey SALEBILL_ITEM_ALLBY_SALEID_KEY => new($"{SALEBILL_PK}-8");
		public static CacheKey GETSALEBILLSBYBUSINESSUSERS => new($"{SALEBILL_PK}-9");
		public static CacheKey GETSALEBILLSBYDELIVERYUSERS => new($"{SALEBILL_PK}-10");
		public static CacheKey SALEBILL_ACCOUNTING_BY_ID_KEY => new($"{SALEBILL_PK}-11");
		public static CacheKey SALEBILL_ACCOUNTINGL_BY_SALEID_KEY => new($"{SALEBILL_PK}-12");
		public static CacheKey SALEBILL_ACCOUNTING_ALLBY_SALEID_KEY => new($"{SALEBILL_PK}-13");
		public static CacheKey SALEBILL_GETSALE_REPORTITEM_KEY => new($"{SALEBILL_PK}-14");
		public static CacheKey SALEBILL_CUSTOMERPRODUCT_KEY => new($"{SALEBILL_PK}-15");


		public static string DELIVERYSIGN_PK => "DELIVERYSIGN_PK.{0}";
		public static CacheKey DELIVERYSIGN_BY_ID_KEY => new($"{DELIVERYSIGN_PK}-1");

		public static string RETAINPHOTO_PK => "RETAINPHOTO_PK.{0}";
		public static CacheKey RETAINPHOTO_BY_ID_KEY => new($"{RETAINPHOTO_PK}-1");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string SALE_PK => "SALE_PK.{0}";
		public static CacheKey SALE_BY_ID_KEY => new($"{SALE_PK}-1");
		public static CacheKey SALEREPORTSERVICE_GETORDER_QUANTITY_ANALYSIS_KEY => new($"{SALE_PK}-2");
		public static CacheKey SALEREPORTSERVICE_GetSaleAnalysis_KEY => new($"{SALE_PK}-3");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_WAREHOUSE_KEY => new($"{SALE_PK}-4");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_GIVEQUOTA_KEY => new($"{SALE_PK}-5");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_PRODUCT_KEY => new($"{SALE_PK}-6");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTITEM_KEY => new($"{SALE_PK}-7");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_CUSTOMER_KEY => new($"{SALE_PK}-8");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_BUSINESSUSER_KEY => new($"{SALE_PK}-9");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_CUSTOMERPRODUCT_KEY => new($"{SALE_PK}-10");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_BRAND_KEY => new($"{SALE_PK}-11");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTORDER_ITEM_KEY => new($"{SALE_PK}-12");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORTSUMMARY_ORDERPRODUCT_KEY => new($"{SALE_PK}-13");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_COSTCONTRACTITEM_KEY => new($"{SALE_PK}-14");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_HOTSALE_KEY => new($"{SALE_PK}-15");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_ORDER_HOTSALE_KEY => new($"{SALE_PK}-16");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_SALEQUANTITYTREND_KEY => new($"{SALE_PK}-17");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_PRODUCTCOSTPROFIT_KEY => new($"{SALE_PK}-18");
		public static CacheKey SALEREPORTSERVICE_GETSALE_REPORT_BUSINESSDAILY_KEY => new($"{SALE_PK}-19");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string SALERESERVATIONBILL_PK => "SALERESERVATIONBILL_PK.{0}";
		public static CacheKey SALERESERVATIONBILL_BY_ID_KEY => new($"{SALERESERVATIONBILL_PK}-1");
		public static CacheKey SALERESERVATIONBILL_BY_NUMBER_KEY => new($"{SALERESERVATIONBILL_PK}-2");
		public static CacheKey SALERESERVATIONBILL_BY_STOREID_KEY => new($"{SALERESERVATIONBILL_PK}-3");
		public static CacheKey SALERESERVATIONBILLNULLWAREHOUSE_BY_STOREID_KEY => new($"{SALERESERVATIONBILL_PK}-4");
		public static CacheKey SALERESERVATIONBILL_ITEM_ALLBY_SALEID_KEY => new($"{SALERESERVATIONBILL_PK}-5");
		public static CacheKey SALERESERVATIONBILL_ACCOUNTING_BY_ID_KEY => new($"{SALERESERVATIONBILL_PK}-6");
		public static CacheKey SALERESERVATIONBILL_ACCOUNTINGL_BY_SALEID_KEY => new($"{SALERESERVATIONBILL_PK}-7");
		public static CacheKey SALERESERVATIONBILL_ACCOUNTING_ALLBY_SALEID_KEY => new($"{SALERESERVATIONBILL_PK}-8");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string EXCHANGEBILL_PK => "EXCHANGEBILL_PK.{0}";
		public static CacheKey EXCHANGEBILL_BY_NUMBER_KEY => new($"{EXCHANGEBILL_PK}-1");
		public static CacheKey EXCHANGEBILL_BY_STOREID_KEY => new($"{EXCHANGEBILL_PK}-2");
		public static CacheKey EXCHANGEBILLNULLWAREHOUSE_BY_STOREID_KEY => new($"{EXCHANGEBILL_PK}-3");
		public static CacheKey EXCHANGEBILL_ITEM_ALLBY_SALEID_KEY => new($"{EXCHANGEBILL_PK}-4");

		#endregion

		#region

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRICESTRUCTURE_PK => "PRICESTRUCTURE_PK.{0}";
		public static CacheKey PRICESTRUCTURE_ALL_KEY => new($"{PRICESTRUCTURE_PK}-1");
		public static CacheKey PRICESTRUCTURE_BY_ID_KEY => new($"{PRICESTRUCTURE_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRINTTEMPLATE_PK => "PRINTTEMPLATE_PK.{0}";
		public static CacheKey PRINTTEMPLATE_ALL_KEY => new($"{PRINTTEMPLATE_PK}-1");
		public static CacheKey PRINTTEMPLATE_BY_ID_KEY => new($"{PRINTTEMPLATE_PK}-2");
		/// <summary>
		/// 匹配
		/// </summary>
		public static string REMARKCONFIG_PK => "REMARKCONFIG_PK.{0}";
		public static CacheKey REMARKCONFIG_ALL_KEY => new($"{REMARKCONFIG_PK}-1");
		public static CacheKey REMARKCONFIG_BY_ID_KEY => new($"{REMARKCONFIG_PK}-2");
		public static CacheKey BINDREMARKCONFIG_ALL_STORE_KEY => new($"{REMARKCONFIG_PK}-3");


		#endregion

		#region

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCKEARLYWARING_PK => "STOCKEARLYWARING_PK.{0}";
		public static CacheKey STOCKEARLYWARING_ALL_KEY => new($"{STOCKEARLYWARING_PK}-1");
		public static CacheKey STOCKEARLYWARING_BY_ID_KEY => new($"{STOCKEARLYWARING_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string ALLOCATIONBILL_PK => "ALLOCATIONBILL_PK.{0}";
		public static CacheKey ALLOCATIONBILL_BY_ID_KEY => new($"{ALLOCATIONBILL_PK}-1");
		public static CacheKey ALLOCATIONBILL_BY_NUMBER_KEY => new($"{ALLOCATIONBILL_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string ALLOCATIONBILLITEM_PK => "ALLOCATIONBILL_PK.{0}";
		public static CacheKey ALLOCATIONBILLITEM_BY_ID_KEY => new($"{ALLOCATIONBILLITEM_PK}-1");
		public static CacheKey ALLOCATIONBILLITEM_ALL_KEY => new($"{ALLOCATIONBILLITEM_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>

		public static string COMBINATIONPRODUCTBILL_PK => "COMBINATIONPRODUCTBILL_PK.{0}";
		public static CacheKey COMBINATIONPRODUCTBILL_BY_ID_KEY => new($"{COMBINATIONPRODUCTBILL_PK}-1");
		public static CacheKey COMBINATIONPRODUCTBILL_BY_NUMBER_KEY => new($"{COMBINATIONPRODUCTBILL_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string COMBINATIONPRODUCTBILLITEM_PK => "COMBINATIONPRODUCTBILLITEM_PK.{0}";
		public static CacheKey COMBINATIONPRODUCTBILLITEM_BY_ID_KEY => new($"{COMBINATIONPRODUCTBILLITEM_PK}-1");
		public static CacheKey COMBINATIONPRODUCTBILLITEM_ALL_KEY => new($"{COMBINATIONPRODUCTBILLITEM_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string CONSTADJUSTMENTBILL_PK => "CONSTADJUSTMENTBILL_PK.{0}";
		public static CacheKey CONSTADJUSTMENTBILL_BY_ID_KEY => new($"{CONSTADJUSTMENTBILL_PK}-1");
		public static CacheKey CONSTADJUSTMENTBILL_BY_NUMBER_KEY => new($"{CONSTADJUSTMENTBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string CONSTADJUSTMENTBILLITEM_PK => "CONSTADJUSTMENTBILLITEM_PK.{0}";
		public static CacheKey CONSTADJUSTMENTBILLITEM_BY_ID_KEY => new($"{CONSTADJUSTMENTBILLITEM_PK}-1");
		public static CacheKey CONSTADJUSTMENTBILLITEM_ALL_KEY => new($"{CONSTADJUSTMENTBILLITEM_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string SCRAPPRODUCTBILL_PK => "SCRAPPRODUCTBILL_PK.{0}";
		public static CacheKey SCRAPPRODUCTBILL_BY_ID_KEY => new($"{SCRAPPRODUCTBILL_PK}-1");
		public static CacheKey SCRAPPRODUCTBILL_BY_NUMBER_KEY => new($"{SCRAPPRODUCTBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string SCRAPPRODUCTBILLITEM_PK => "SCRAPPRODUCTBILLITEM_PK.{0}";
		public static CacheKey SCRAPPRODUCTBILLITEM_BY_ID_KEY => new($"{SCRAPPRODUCTBILLITEM_PK}-1");
		public static CacheKey SCRAPPRODUCTBILLITEM_ALL_KEY => new($"{SCRAPPRODUCTBILLITEM_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string SPLITPRODUCTBILL_PK => "SPLITPRODUCTBILL_PK.{0}";
		public static CacheKey SPLITPRODUCTBILL_BY_ID_KEY => new($"{SPLITPRODUCTBILL_PK}-1");
		public static CacheKey SPLITPRODUCTBILL_BY_NUMBER_KEY => new($"{SPLITPRODUCTBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string SPLITPRODUCTBILLITEM_PK => "SPLITPRODUCTBILLITEM_PK.{0}";
		public static CacheKey SPLITPRODUCTBILLITEM_BY_ID_KEY => new($"{SPLITPRODUCTBILLITEM_PK}-1");
		public static CacheKey SPLITPRODUCTBILLITEM_ALL_KEY => new($"{SPLITPRODUCTBILLITEM_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCKFLOW_PK => "STOCKFLOW_PK.{0}";
		public static CacheKey STOCKFLOW_BY_ID_KEY => new($"{STOCKFLOW_PK}-1");
		public static CacheKey STOCKFLOW_ALL_KEY => new($"{STOCKFLOW_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCKINOUTRECORDS_PK => "STOCKINOUTRECORDS_PK.{0}";
		public static CacheKey STOCKINOUTRECORDS_BY_ID_KEY => new($"{STOCKINOUTRECORDS_PK}-1");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCK_PK => "STOCK_PK.{0}";
		public static CacheKey STOCK_BY_ID_KEY => new($"{STOCK_PK}-1");
		public static CacheKey STOCK_REPORT_ALLLIST_KEY => new($"{STOCK_PK}-2");
		public static CacheKey STOCKS_BY_PRODUCTIDS_KEY => new($"{STOCK_PK}-3");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCKINOUTRECORDSSTOCKFLOW_PK => "STOCKINOUTRECORDSSTOCKFLOW_PK.{0}";
		public static CacheKey STOCKINOUTRECORDSSTOCKFLOW_BY_ID_KEY => new($"{STOCKINOUTRECORDSSTOCKFLOW_PK}-1");
		public static CacheKey STOCKINOUTRECORDSSTOCKFLOW_BY_RECORED_ID_KEY => new($"{STOCKINOUTRECORDSSTOCKFLOW_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>

		public static string WAREHOUSE_PK => "WAREHOUSE_PK.{0}";
		public static CacheKey WAREHOUSE_ALL_STORE_KEY => new($"{WAREHOUSE_PK}-1");
		public static CacheKey BINDWAREHOUSE_ALL_STORE_KEY => new($"{WAREHOUSE_PK}-2");
		public static CacheKey BINDWAREHOUSE_ALL_STORE_KEY_TYPE => new($"{WAREHOUSE_PK}-3");
		public static CacheKey WAREHOUSE_CARALL_STORE_KEY => new($"{WAREHOUSE_PK}-4");
		public static CacheKey WAREHOUSE_BY_ID_KEY => new($"{WAREHOUSE_PK}-5");
		public static CacheKey WAREHOUSE_BY_IDS_KEY => new($"{WAREHOUSE_PK}-6");
		public static CacheKey WAREHOUSE_NAME_BY_ID_KEY => new($"{WAREHOUSE_PK}-7");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYALLTASKBILL_PK => "INVENTORYALLTASKBILL_PK.{0}";
		public static CacheKey INVENTORYALLTASKBILL_BY_ID_KEY => new($"{INVENTORYALLTASKBILL_PK}-1");
		public static CacheKey INVENTORYALLTASKBILL_BY_NUMBER_KEY => new($"{INVENTORYALLTASKBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYALLTASKBILLITEM_PK => "INVENTORYALLTASKBILLITEM_PK.{0}";
		public static CacheKey INVENTORYALLTASKBILLITEM_BY_ID_KEY => new($"{INVENTORYALLTASKBILLITEM_PK}-1");
		public static CacheKey INVENTORYALLTASKBILLITEM_ALL_KEY => new($"{INVENTORYALLTASKBILLITEM_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYPARTTASKBILL_PK => "INVENTORYPARTTASKBILL_PK.{0}";
		public static CacheKey INVENTORYPARTTASKBILL_BY_ID_KEY => new($"{INVENTORYPARTTASKBILL_PK}-1");
		public static CacheKey INVENTORYPARTTASKBILL_BY_NUMBER_KEY => new($"{INVENTORYPARTTASKBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYPARTTASKBILLITEM_PK => "INVENTORYPARTTASKBILLITEM_PK.{0}";
		public static CacheKey INVENTORYPARTTASKBILLITEM_BY_ID_KEY => new($"{INVENTORYPARTTASKBILLITEM_PK}-1");
		public static CacheKey INVENTORYPARTTASKBILLITEM_ALL_KEY => new($"{INVENTORYPARTTASKBILLITEM_PK}-2");
		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORPROFITLOSSBILL_PK => "INVENTORPROFITLOSSBILL_PK.{0}";
		public static CacheKey INVENTORPROFITLOSSBILL_BY_ID_KEY => new($"{INVENTORPROFITLOSSBILL_PK}-1");
		public static CacheKey INVENTORPROFITLOSSBILL_BY_NUMBER_KEY => new($"{INVENTORPROFITLOSSBILL_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORPROFITLOSSBILLITEM_PK => "INVENTORPROFITLOSSBILLITEM_PK.{0}";
		public static CacheKey INVENTORPROFITLOSSBILLITEM_BY_ID_KEY => new($"{INVENTORPROFITLOSSBILLITEM_PK}-1");
		public static CacheKey INVENTORPROFITLOSSBILLITEM_ALL_KEY => new($"{INVENTORPROFITLOSSBILLITEM_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>

		public static string INVENTORYREPORTBILL_PK => "INVENTORYREPORTBILL_PK.{0}";
		public static CacheKey INVENTORYREPORTBILL_BY_ID_KEY => new($"{INVENTORYREPORTBILL_PK}-1");
		public static CacheKey INVENTORYREPORTBILL_BY_NUMBER_KEY => new($"{INVENTORYREPORTBILL_PK}-2");


		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYREPORTBILLITEM_PK => "INVENTORYREPORTBILLITEM_PK.{0}";
		public static CacheKey INVENTORYREPORTBILLITEM_BY_ID_KEY => new($"{INVENTORYREPORTBILLITEM_PK}-1");
		public static CacheKey INVENTORYREPORTBILLITEM_ALL_KEY => new($"{INVENTORYREPORTBILLITEM_PK}-2");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string INVENTORYREPORTSTOREQUANTITY_PK => "INVENTORYREPORTSTOREQUANTITY_PK.{0}";
		public static CacheKey INVENTORYREPORTSTOREQUANTITY_BY_ID_KEY => new($"{INVENTORYREPORTSTOREQUANTITY_PK}-1");
		public static CacheKey INVENTORYREPORTSTOREQUANTITY_ALL_KEY => new($"{INVENTORYREPORTSTOREQUANTITY_PK}-2");
		/// <summary>
		/// 匹配
		/// </summary>

		public static string INVENTORYREPORTSUMMARY_PK => "INVENTORYREPORTSUMMARY_PK.{0}";
		public static CacheKey INVENTORYREPORTSUMMARY_BY_ID_KEY => new($"{INVENTORYREPORTSUMMARY_PK}-1");
		public static CacheKey INVENTORYREPORTSUMMARY_ALL_KEY => new($"{INVENTORYREPORTSUMMARY_PK}-2");


		#endregion

		#region
		/// <summary>
		/// 匹配
		/// </summary>
		public static string ACTIVITYTYPE_PK => "ACTIVITYTYPE_PK.{0}";
		public static CacheKey ACTIVITYTYPE_ALL_KEY => new($"{ACTIVITYTYPE_PK}-1");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string PRODUCTFLAVOR_PK => "PRODUCTFLAVOR_PK.{0}";
		public static CacheKey PRODUCTFLAVOR_ALLBYPRODUCTID_KEY => new($"{PRODUCTFLAVOR_PK}-1");

		/// <summary>
		/// 匹配
		/// </summary>
		public static string STOCKREPORTSERVICE_PK => "STOCKREPORTSERVICE_PK.{0}";
		public static CacheKey STOCKREPORTSERVICE_GETINVENTORYREPORTLIST_KEY => new($"{STOCKREPORTSERVICE_PK}-1");
		public static CacheKey STOCKREPORTSERVICE_GETINVENTORYREPORTLISTAPI_KEY => new($"{STOCKREPORTSERVICE_PK}-2");
		public static CacheKey STOCKREPORTSERVICE_GETINVENTORYREPORTSUMMARYLIST_KEY => new($"{STOCKREPORTSERVICE_PK}-3");
		public static CacheKey STOCKREPORTSERVICE_GETALLOCATIONDETAILS_KEY => new($"{STOCKREPORTSERVICE_PK}-4");
		public static CacheKey STOCKREPORTSERVICE_GETALLOCATIONDETAILSBYPRODUCTS_KEY => new($"{STOCKREPORTSERVICE_PK}-5");
		public static CacheKey MAINPAGEREPORTSERVICE_GETEARLYWARNING_KEY => new($"{STOCKREPORTSERVICE_PK}-6");
		public static CacheKey MAINPAGEREPORTSERVICE_GETEXPIRATIONWARNING_KEY => new($"{STOCKREPORTSERVICE_PK}-7");
		public static CacheKey STOCKREPORTSERVICE_GETSTOCKREPORTPRODUCT_KEY => new($"{STOCKREPORTSERVICE_PK}-8");
		public static CacheKey MAINPAGEREPORTSERVICE_GETMONTHSALEREPORT_KEY => new($"{STOCKREPORTSERVICE_PK}-9");
		public static CacheKey MAINPAGEREPORTSERVICE_GETDASHBOARDREPORT_KEY => new($"{STOCKREPORTSERVICE_PK}-10");
		#endregion
	}
}