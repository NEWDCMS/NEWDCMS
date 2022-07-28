using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Localization
{

    public class LocalizationSettings : ISettings
    {

        public int DefaultAdminLanguageId { get; set; }

        public bool UseImagesForLanguageSelection { get; set; }


        public bool SeoFriendlyUrlsForLanguagesEnabled { get; set; }


        public bool AutomaticallyDetectLanguage { get; set; }


        public bool LoadAllLocaleRecordsOnStartup { get; set; }


        public bool LoadAllLocalizedPropertiesOnStartup { get; set; }

        public bool LoadAllUrlRecordsOnStartup { get; set; }


        public bool IgnoreRtlPropertyForAdminArea { get; set; }
    }
}