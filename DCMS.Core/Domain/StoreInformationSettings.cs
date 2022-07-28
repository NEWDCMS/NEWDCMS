using DCMS.Core.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain
{

    public class SMSParams
    {
        public int StoreId { get; set; }
        public int RType { get; set; }
        public string Mobile { get; set; }
        public int Id { get; set; }
    }

    /// <summary>
    /// 用于经销商管理客户端基础信息配置
    /// </summary>
    public class StoreInformationSettings : ISettings
    {

        //[Column(TypeName = "BIT(1)")]
        public bool HidePoweredByDCMS { get; set; }


        //[Column(TypeName = "BIT(1)")]
        public bool StoreClosed { get; set; }


        public int LogoPictureId { get; set; }


        public string DefaultStoreTheme { get; set; }


        //[Column(TypeName = "BIT(1)")]
        public bool AllowUserToSelectTheme { get; set; }


        //[Column(TypeName = "BIT(1)")]
        public bool DisplayMiniProfilerInPublicStore { get; set; }


        [Column(TypeName = "BIT(1)")]
        public bool DisplayMiniProfilerForAdminOnly { get; set; }


        //[Column(TypeName = "BIT(1)")]
        public bool DisplayEuCookieLawWarning { get; set; }




        ///// <summary>
        ///// 关闭站点
        ///// </summary>
        //public bool StoreClosed { get; set; }

        /// <summary>
        /// 允许管理员浏览（关闭站点）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool StoreClosedAllowForAdmins { get; set; }

        /// <summary>
        /// 存储默认桌面主题
        /// </summary>
        public string DefaultStoreThemeForDesktops { get; set; }

        /// <summary>
        /// 读取默销售区域
        /// </summary>
        public int DefaultCurrentRegion { get; set; }

        ///// <summary>
        ///// 允许自定义选择主题
        ///// </summary>
        //public bool AllowUserToSelectTheme { get; set; }

        /// <summary>
        /// 是否支持移动版驱动
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool MobileDevicesSupported { get; set; }

        /// <summary>
        /// 存储移动版桌面主题
        /// </summary>
        public string DefaultStoreThemeForMobileDevices { get; set; }

        /// <summary>
        /// 支持移动设备
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool EmulateMobileDevice { get; set; }

        ///// <summary>
        ///// 获取或设置一个值，指示 miniprofiler 是否对站点进行性能调试
        ///// </summary>
        //public bool DisplayMiniProfilerInPublicStore { get; set; }

    }
}
