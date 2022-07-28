using DCMS.Core.Configuration;

namespace DCMS.Services.Helpers
{
    public class DateTimeSettings : ISettings
    {
        public string DefaultStoreTimeZoneId { get; set; }
        public bool AllowUsersToSetTimeZone { get; set; }
    }
}