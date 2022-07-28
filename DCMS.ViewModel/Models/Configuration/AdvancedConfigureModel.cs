using DCMS.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;


namespace DCMS.ViewModel.Models.Configuration
{

    public class AdvancedConfigureModel
    {
        public TraditionSettingModel TraditionSettingModel { get; set; }

        public RestaurantSettingModel RestaurantSettingModel { get; set; }

        public SalesProductSettingModel SalesProductSettingModel { get; set; }

        public VersionUpdateSettingModel VersionUpdateSettingModel { get; set; }

        public NewsSettingModel NewSettingModel { get; set; }

        public EmailSettingsModel EmailSettingsModel { get; set; }

    }



    /// <summary>
    /// 传统
    /// </summary>
    public class TraditionSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }


        [Display(Name = "终端类型")]
        /// <summary>
        /// 终端类型(现代小零售(X)/名烟名酒店(M)/传统小零售(T)/批零店（P）)
        /// </summary>
        public string EndPointType { get; set; }

        [Display(Name = "合作方式")]
        /// <summary>
        /// 合作方式(专营保量、专营不保量、混营保量、混营不保量)是协议店时必填，非协议店不填
        /// </summary>
        public string ModeOfCooperation { get; set; }

        [Display(Name = "终端状态")]
        /// <summary>
        // 终端状态(专销、强势混销、弱势混销、空白)
        //专销：单店我司份额=100%;强势混销：单店我司份额第一且>=40%；弱势混销：单店我司份额<40% or 第二名及以后；空白：单店我司份额=0
        /// </summary>
        public string EndPointStates { get; set; }

    }

    /// <summary>
    /// 餐饮
    /// </summary>
    public class RestaurantSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "终端类型")]
        /// <summary>
        /// 终端类型(S超高档餐饮、A高档餐饮、B中档餐饮、C大众餐饮、D低档餐饮)
        /// </summary>
        public string EndPointType { get; set; }


        [Display(Name = "经营特色")]
        /// <summary>
        /// 经营特色(炒菜/火锅/烧烤/小吃（面馆）快餐/西餐（含日韩料理/自助餐/其它)
        /// </summary>
        public string Characteristics { get; set; }


        [Display(Name = "合作方式")]
        /// <summary>
        /// 合作方式(专营保量、专营不保量、混营保量、混营不保量)
        /// </summary>
        public string ModeOfCooperation { get; set; }


        [Display(Name = "终端状态")]
        /// <summary>
        /// 终端状态(专销、强势混销、弱势混销、空白)
        /// </summary>
        public string EndPointStates { get; set; }


        [Display(Name = "")]
        /// <summary>
        /// 
        /// </summary>
        public string PerConsumptions { get; set; }


    }

    /// <summary>
    /// 销售
    /// </summary>
    public class SalesProductSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "品牌")]
        /// <summary>
        /// 品牌(终端所销售的啤酒品牌)青岛啤酒、雪花啤酒、Budweiser百威、其他
        /// </summary>
        public string Brand { get; set; }

        [Display(Name = "包装形式")]
        /// <summary>
        ///  包装形式(瓶、听、桶)(所售啤酒产品的最小包装形式)
        /// </summary>
        public string PackingForm { get; set; }

        [Display(Name = "渠道属性")]
        /// <summary>
        /// 渠道属性((一批、二批、其他)（销售啤酒产品给此终端的渠道商在其代理的啤酒产品渠道中的层级，包括一批、二批和其他（一批、二批之外））
        /// </summary>
        public string ChannelAttributes { get; set; }

        [Display(Name = "规格")]
        /// <summary>
        /// 规格"200ML", "280ML", "375ML", "12*330ML", "24*330ML", "9*500ML", "12*500ML", "750ML", "1000ML", "2000ML", "4000ML", "5000ML"
        /// </summary>
        public string Specification { get; set; }
    }

    /// <summary>
    /// 版本更新
    /// </summary>
    public class VersionUpdateSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "是否启用版本更新")]
        public bool Enable { get; set; }
        [Display(Name = "最新版本号")]
        public string Version { get; set; }
        [Display(Name = "下载地址")]
        public string DownLoadUrl { get; set; }

        //[Display(Name = "账号")]
        //public string FtpUserName { get; set; }

        //[Display(Name = "密码")]
        //public string FtpPassword { get; set; }


        [Display(Name = "升级描述")]
        public string UpgradeDescription { get; set; }

    }

    /// <summary>
    /// 新闻配置
    /// </summary>
    public class NewsSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "新闻路径")]
        public string NewsPath { get; set; }
    }

    public class EmailSettingsModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "发送地址")]
        public string Form { get; set; }
        [Display(Name = "端口")]
        public int Port { get; set; }
        [Display(Name = "Smtp")]
        public string Smtp { get; set; }
        [Display(Name = "账号")]
        public string Account { get; set; }
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Display(Name = "是否启用SSL")]
        public bool SSL { get; set; }

        public string To { get; set; }
        public string ReplyTo { get; set; }
    }
}