using DCMS.Core.Configuration;
using System.Collections.Generic;
using System.ComponentModel;


namespace DCMS.Core.Domain.Common
{

    public enum MenuLayoutPosition : int
    {
        [Description("头部")]
        Top = 0,

        [Description("右侧")]
        Right = 1,

        [Description("左侧")]
        Left = 2,

        [Description("底部")]
        Bottom = 3,
    }


    public class CommonSettings : ISettings
    {

        public bool DisplayJavaScriptDisabledWarning { get; set; }
        public bool Log404Errors { get; set; }

        public bool RenderXuaCompatible { get; set; }

        public string XuaCompatibleValue { get; set; }

        public List<string> IgnoreLogWordlist { get; set; }

        public bool UseResponseCompression { get; set; }

        public string StaticFilesCacheControl { get; set; }

        public string FaviconAndAppIconsHeadCode { get; set; }

        public bool EnableHtmlMinification { get; set; }

        public bool EnableJsBundling { get; set; }

        public bool EnableCssBundling { get; set; }

        public int? ScheduleTaskRunTimeout { get; set; } = 0;

        public bool UseStoredProceduresIfSupported { get; set; }

    }
}