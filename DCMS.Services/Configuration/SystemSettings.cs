using DCMS.Core.Configuration;
using System;

namespace DCMS.Services.Configuration
{

    public class EmailSettings : ISettings
    {
        public string Form { get; set; }
        public string To { get; set; }
        public string ReplyTo { get; set; }
        public int Port { get; set; }
        public string Smtp { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }
    }

    public class BaseSetting : ISettings
    {
        public int DefaultPlan { get; set; }
    }


    public class PromotionSetting : ISettings
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Enable { get; set; }
        public int RewardDay { get; set; }
        public string Description { get; set; }
    }


    public class SendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
