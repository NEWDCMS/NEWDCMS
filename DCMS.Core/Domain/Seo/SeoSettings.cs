using DCMS.Core.Configuration;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Seo
{
    /// <summary>
    /// SEO settings
    /// </summary>
    public class SeoSettings : ISettings
    {
        /// <summary>
        /// Page title separator
        /// </summary>
        public string PageTitleSeparator { get; set; }

        /// <summary>
        /// Page title SEO adjustment
        /// </summary>
        public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }

        /// <summary>
        /// Default title
        /// </summary>
        public string DefaultTitle { get; set; }

        /// <summary>
        /// Default META keywords
        /// </summary>
        public string DefaultMetaKeywords { get; set; }

        /// <summary>
        /// Default META description
        /// </summary>
        public string DefaultMetaDescription { get; set; }

        /// <summary>
        /// A value indicating whether product META descriptions will be generated automatically (if not entered)
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool GenerateProductMetaDescription { get; set; }

        /// <summary>
        /// A value indicating whether we should convert non-western chars to western ones
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ConvertNonWesternChars { get; set; }

        /// <summary>
        /// A value indicating whether unicode chars are allowed
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool AllowUnicodeCharsInUrls { get; set; }

        /// <summary>
        /// A value indicating whether canonical URL tags should be used
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool CanonicalUrlsEnabled { get; set; }

        /// <summary>
        /// A value indicating whether to use canonical URLs with query string parameters
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool QueryStringInCanonicalUrlsEnabled { get; set; }

        /// <summary>
        /// WWW requires (with or without WWW)
        /// </summary>
        public WwwRequirement WwwRequirement { get; set; }

        /// <summary>
        /// A value indicating whether Twitter META tags should be generated
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool TwitterMetaTags { get; set; }

        /// <summary>
        /// A value indicating whether Open Graph META tags should be generated
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool OpenGraphMetaTags { get; set; }

        /// <summary>
        /// Slugs (sename) reserved for some other needs
        /// </summary>
        public List<string> ReservedUrlRecordSlugs { get; set; }

        /// <summary>
        /// Custom tags in the <![CDATA[<head></head>]]> section
        /// </summary>
        public string CustomHeadTags { get; set; }
    }
}